using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;

public class KinectSkeleton : MonoBehaviour
{
//Use in the future
	public Transform Head;
	public Transform Neck;
	public Transform Torso;
	public Transform Waist;
	public Transform LeftCollar;
	public Transform LeftShoulder;
	public Transform LeftElbow;
	public Transform LeftWrist;
	public Transform LeftFingertip;
	public Transform RightCollar;
	public Transform RightShoulder;
	public Transform RightElbow;
	public Transform RightWrist;
	public Transform RightFingertip;
	public Transform LeftHand;
	public Transform RightHand;
	public GameObject leftHandPrefab;
	public GameObject rightHandPrefab;
	bool leftHandTrack = false;
	bool rightHandTrack = false;
	GameObject rightHandInstance;
	GameObject leftHandInstance;
	public bool mirror = false;
	public bool UpdateJointPositions = false;
	public bool UpdateRootPosition = false;
	public bool UpdateOrientation = true;
	public bool RotateToPsiPose = false;
	public float RotationDamping = 30.0f;
	public float Damping = 10.0f;
	public Vector3 Scale = new Vector3 (0.001f, 0.001f, 0.001f);
	public Vector3 PositionBias = Vector3.zero;
	private Transform[] transforms;
	private Quaternion[] initialRotations;
	private Vector3 rootPosition;
	Vector3 leftHandViewport;
	Vector3 rightHandViewport;
	float F = 0.0019047619f;
	Image<Gray,byte> handDepth = new Image<Gray, byte> (640, 480);
	Image<Bgr,byte> handColor = new Image<Bgr, byte> (640, 480);

	ZigJointId mirrorJoint (ZigJointId joint)
	{
		switch (joint) {
		case ZigJointId.LeftCollar:
			return ZigJointId.RightCollar;
		case ZigJointId.LeftShoulder:
			return ZigJointId.RightShoulder;
		case ZigJointId.LeftElbow:
			return ZigJointId.RightElbow;
		case ZigJointId.LeftWrist:
			return ZigJointId.RightWrist;
		case ZigJointId.LeftHand:
			return ZigJointId.RightHand;
		case ZigJointId.LeftFingertip:
			return ZigJointId.RightFingertip;
		case ZigJointId.RightCollar:
			return ZigJointId.LeftCollar;
		case ZigJointId.RightShoulder:
			return ZigJointId.LeftShoulder;
		case ZigJointId.RightElbow:
			return ZigJointId.LeftElbow;
		case ZigJointId.RightWrist:
			return ZigJointId.LeftWrist;
		case ZigJointId.RightHand:
			return ZigJointId.LeftHand;
		case ZigJointId.RightFingertip:
			return ZigJointId.LeftFingertip;
		default:
			return joint;
		}
	}

	public void Awake ()
	{
		int jointCount = Enum.GetNames (typeof(ZigJointId)).Length;

		transforms = new Transform[jointCount];
		initialRotations = new Quaternion[jointCount];

		transforms [(int)ZigJointId.Head] = Head;
		transforms [(int)ZigJointId.Neck] = Neck;
		transforms [(int)ZigJointId.Torso] = Torso;
		transforms [(int)ZigJointId.Waist] = Waist;
		transforms [(int)ZigJointId.LeftCollar] = LeftCollar;
		transforms [(int)ZigJointId.LeftShoulder] = LeftShoulder;
		transforms [(int)ZigJointId.LeftElbow] = LeftElbow;
		transforms [(int)ZigJointId.LeftWrist] = LeftWrist;
		transforms [(int)ZigJointId.LeftFingertip] = LeftFingertip;
		transforms [(int)ZigJointId.RightCollar] = RightCollar;
		transforms [(int)ZigJointId.RightShoulder] = RightShoulder;
		transforms [(int)ZigJointId.RightElbow] = RightElbow;
		transforms [(int)ZigJointId.RightWrist] = RightWrist;
		transforms [(int)ZigJointId.RightFingertip] = RightFingertip;
		
		//Set the transforms
		transforms [(int)ZigJointId.LeftHand] = GameObject.Find ("LeftHand").transform;
		transforms [(int)ZigJointId.RightHand] = GameObject.Find ("RightHand").transform;



		// save all initial rotations
		// NOTE: Assumes skeleton model is in "T" pose since all rotations are relative to that pose
		foreach (ZigJointId j in Enum.GetValues(typeof(ZigJointId))) {
			if (transforms [(int)j]) {
				// we will store the relative rotation of each joint from the gameobject rotation
				// we need this since we will be setting the joint's rotation (not localRotation) but we 
				// still want the rotations to be relative to our game object
				initialRotations [(int)j] = Quaternion.Inverse (transform.rotation) * transforms [(int)j].rotation;
			}
		}
	}

	void Start ()
	{
		// start out in calibration pose
		if (RotateToPsiPose) {
			RotateToCalibrationPose ();
		}
	}

	void UpdateRoot (Vector3 skelRoot)
	{
		// +Z is backwards in OpenNI coordinates, so reverse it
		rootPosition = Vector3.Scale (new Vector3 (skelRoot.x, skelRoot.y, skelRoot.z), doMirror (Scale)) + PositionBias;
		if (UpdateRootPosition) {
			transform.localPosition = (transform.rotation * rootPosition);
		}
	}

	void UpdateRotation (ZigJointId joint, Quaternion orientation)
	{
		joint = mirror ? mirrorJoint (joint) : joint;
		// make sure something is hooked up to this joint
		if (!transforms [(int)joint]) {
			return;
		}

		if (UpdateOrientation) {
			Quaternion newRotation = transform.rotation * orientation * initialRotations [(int)joint];
			if (mirror) {
				newRotation.y = -newRotation.y;
				newRotation.z = -newRotation.z;
			}
			transforms [(int)joint].rotation = Quaternion.Slerp (transforms [(int)joint].rotation, newRotation, Time.deltaTime * RotationDamping);
		}
	}

	Vector3 doMirror (Vector3 vec)
	{
		return new Vector3 (mirror ? -vec.x : vec.x, vec.y, vec.z);
	}

	void UpdatePosition (ZigJointId joint, Vector3 position)
	{
		joint = mirror ? mirrorJoint (joint) : joint;
		// make sure something is hooked up to this joint
		if (!transforms [(int)joint]) {
			return;
		}

		GameObject screenReference = GameObject.Find ("ScreenReference");
		Camera userCamera = GameObject.Find ("UserCamera").camera;
		CreatePlane createPlane = GameObject.Find ("Screen").GetComponent<CreatePlane> ();
		
		//In zigFu, the left hand represents the right hand and the right hand represents the left hand
		if (UpdateJointPositions) {
			if (joint == ZigJointId.RightHand) {
				//Get the object of the left hand
				GameObject leftHand = GameObject.Find ("LeftHand");
				//Set the position of leftHand in the space
				getSpacePosition (leftHand, position);
				
				
				if (leftHandTrack == false) {
					leftHandInstance = (GameObject)Instantiate (leftHandPrefab, userCamera.transform.position, userCamera.transform.rotation);
					leftHandTrack = true;
				}
				
				if (leftHandTrack == true) {
					//Set the position of the left hand projection in the viewport
					leftHandViewport = getViewportPosition (screenReference, leftHand.transform.position, createPlane.getStartPoint (), createPlane.getScreenWidth (), createPlane.getScreenHeight ());
					setProjectionPosition (leftHandViewport, leftHandInstance, userCamera);
				}

			}
			if (joint == ZigJointId.LeftHand) {
				//Get the object of the right hand
				GameObject rightHand = GameObject.Find ("RightHand");
				//Set the position of rightHand in the space
				getSpacePosition (rightHand, position);
				
				
			
				if (rightHandTrack == false) {
					rightHandInstance = (GameObject)Instantiate (rightHandPrefab, userCamera.transform.position, userCamera.transform.rotation);
					rightHandTrack = true;
				}
				
				if (rightHandTrack == true) {
					//Set the position of the right hand projection in the viewport
					rightHandViewport = getViewportPosition (screenReference, rightHand.transform.position, createPlane.getStartPoint (), createPlane.getScreenWidth (), createPlane.getScreenHeight ());
					setProjectionPosition (rightHandViewport, rightHandInstance, userCamera);
				}
				
			}
		}
	}
	
	void getSpacePosition (GameObject representation, Vector3 position)
	{
		//Get the hand position in the image
		Vector3 imagePosition = ZigInput.ConvertWorldToImageSpace (position);
		OpenNI.Point3D image = new OpenNI.Point3D ((640 - imagePosition.x), imagePosition.y, -imagePosition.z);
		
		//Detect Hand
		ScreenDetection screenDetection = GameObject.Find ("Screen").GetComponent<ScreenDetection> ();
		Image<Gray,byte> depthImage = screenDetection.getKinectDepth ();
		Image<Bgr,byte> colorImage = screenDetection.getKinectColor ();
		short[] originalDepth = screenDetection.getOriginalDepth ();
		byte[,,] depthData = depthImage.Data;
		byte[,,] handDepthData = handDepth.Data;
		byte[,,] colorData = colorImage.Data;
		byte[,,] handColorData = handColor.Data;
	
		int edge = 50;
		int centerX = (int)(640 - imagePosition.x);
		int centerY = (int)(480 - imagePosition.y);
		for (int i=(int)(centerX-edge); i<(int)(centerX+edge); i++) {
			for (int j=(int)(centerY-edge); j<(int)(centerY+edge); j++) {
			
				short centerDepth = originalDepth [centerY * 640 + centerX];
				short anotherDepth = originalDepth [j * 640 + i];
				
				if (System.Math.Abs (anotherDepth - centerDepth) < 250) {
					handDepthData [j, i, 0] = depthData [j, i, 0];
					handColorData [j, i, 0] = colorData [j, i, 0];
					handColorData [j, i, 1] = colorData [j, i, 1];
					handColorData [j, i, 2] = colorData [j, i, 2];
				}	
			}
		}
		handDepth.Draw (new CircleF (new PointF (640 - imagePosition.x, 480 - imagePosition.y), 1), new Gray (255), 1);
		string win7 = "handCenterDepth";
		Emgu.CV.CvInvoke.cvNamedWindow (win7);
		CvInvoke.cvShowImage (win7, handDepth);
		
		string win8 = "mix";
		Emgu.CV.CvInvoke.cvNamedWindow (win8);
		Image<Bgr,byte> mixHand = new Image<Bgr, byte> (640, 480);
		CvInvoke.cvAddWeighted (handColor, 0.5, handDepth.Convert<Bgr,byte> (), 0.5, 0, mixHand);
		CvInvoke.cvShowImage (win8, mixHand);
		//
		
		//Get the hand position in the space
	
		OpenNI.Point3D real = new OpenNI.Point3D ((image.X - 320) * image.Z * F, (image.Y - 240) * image.Z * F, image.Z);	
		Vector3 destination = new Vector3 (real.X, real.Y, real.Z);

		representation.transform.position = Vector3.Lerp (representation.transform.position, destination, Time.deltaTime * Damping);
	}
	
	Vector3 getViewportPosition (GameObject screenReference, Vector3 position, Vector3 startPoint, double screenWidth, double screenHeight)
	{
		Vector3 leftHandScreen = screenReference.transform.InverseTransformPoint (position);
				
		double leftXCoor = (leftHandScreen - startPoint).z / screenWidth;
		double leftYCoor = (-leftHandScreen + startPoint).x / screenHeight;
				
		Vector3 viewportPosition = new Vector3 ((float)(leftXCoor), (float)(1 - leftYCoor), (float)0);
		return viewportPosition;
	}
	
	void setProjectionPosition (Vector3 viewportPosition, GameObject instance, Camera userCamera)
	{
		Vector3 screenPos = new Vector3 (viewportPosition.x * userCamera.pixelWidth, viewportPosition.y * userCamera.pixelHeight, 0);
		instance.transform.position = userCamera.ScreenToViewportPoint (screenPos);
	}

	public void RotateToCalibrationPose ()
	{
		foreach (ZigJointId j in Enum.GetValues(typeof(ZigJointId))) {
			if (null != transforms [(int)j]) {
				transforms [(int)j].rotation = transform.rotation * initialRotations [(int)j];
			}
		}

		// calibration pose is skeleton base pose ("T") with both elbows bent in 90 degrees
		if (null != RightElbow) {
			RightElbow.rotation = transform.rotation * Quaternion.Euler (0, -90, 90) * initialRotations [(int)ZigJointId.RightElbow];
		}
		if (null != LeftElbow) {
			LeftElbow.rotation = transform.rotation * Quaternion.Euler (0, 90, -90) * initialRotations [(int)ZigJointId.LeftElbow];
		}
	}

	public void SetRootPositionBias ()
	{
		this.PositionBias = -rootPosition;
	}

	public void SetRootPositionBias (Vector3 bias)
	{
		this.PositionBias = bias;
	}

	void Zig_UpdateUser (ZigTrackedUser user)
	{
		UpdateRoot (user.Position);
		handDepth.SetZero ();
		handColor.SetZero ();
		if (user.SkeletonTracked) {
			foreach (ZigInputJoint joint in user.Skeleton) {
				if (joint.GoodPosition)
					UpdatePosition (joint.Id, joint.Position);
				if (joint.GoodRotation)
					UpdateRotation (joint.Id, joint.Rotation);
			}
		}
	}

	public Vector3 getLHViewPosition ()
	{
		return leftHandViewport;
	}

	public Vector3 getRHViewPosition ()
	{
		return rightHandViewport;
	}
	
	public GameObject getLHInstance ()
	{
		return leftHandInstance;
	}
	
	public GameObject getRHInstance ()
	{
		return rightHandInstance;
	}

}


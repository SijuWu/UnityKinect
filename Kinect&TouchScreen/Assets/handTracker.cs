using UnityEngine;
using System.Collections;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using OpenNI;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

public class handTracker : MonoBehaviour
{
	
	private OpenNI.GestureGenerator gestureGenerator;
	private OpenNI.HandsGenerator handGenerator;
	private OpenNI.UserGenerator userGenerator;
	private SkeletonCapability skeletonCapbility;
	private PoseDetectionCapability poseDetectionCapability;
	private Dictionary<int, Dictionary<SkeletonJoint, SkeletonJointPosition>> joints;
	private bool shouldDrawPixels = true;
	private bool shouldDrawBackground = true;
	private bool shouldPrintID = true;
	private bool shouldPrintState = true;
	private bool shouldDrawSkeleton = true;
	private string calibPose;
	private Thread readerThread;
	private bool shouldRun;
	short[] rawDepthMap;
	Image<Gray,byte> originalDepth;
	Image<Gray,byte> kinectDepth;
	Image<Gray,byte> colorDepth;
	ZigInputOpenNI openNI;
	GameObject leftHand;
	GameObject rightHand;
	Transform leftTran;
	Transform rightTran;
	// Use this for initialization
	void Start ()
	{
		ZigInput.Instance.AddListener (gameObject);
		
		leftHand = GameObject.Find ("leftHand");
		rightHand = GameObject.Find ("rightHand");
		leftTran=leftHand.transform;
		rightTran=rightHand.transform;
	}
	
	void gestureGenerator_GestureRecognized (object sender, GestureRecognizedEventArgs e)
	{
//		print("Gesture recognized:"+e.Gesture+"\n");
		OpenNI.Point3D identifiedPosition = e.IdentifiedPosition;
		this.handGenerator.StartTracking (e.IdentifiedPosition);
		//		if (this.skeletonCapbility.DoesNeedPoseForCalibration) {
//			this.poseDetectionCapability.StartPoseDetection (this.calibPose, e.ID);
//		} else {
//			this.skeletonCapbility.RequestCalibration (e.ID, true);
//		}
	}
	
	void handGenerator_HandCreate (object sender, HandCreateEventArgs e)
	{
		
		print ("new Hand: " + e.UserID + " " + e.Position.X + " " + e.Position.Y + " " + e.Position.Z);
	
	}
	
	void handGenerator_HandUpdate (object sender, HandUpdateEventArgs e)
	{
		print ("Hand update: " + e.UserID + " " + e.Position.X + " " + e.Position.Y + " " + e.Position.Z);
		ZigDepthViewer depthViewer = gameObject.GetComponent<ZigDepthViewer> ();
		

//		float y = -e.Position.Y + 240;
//		float x = e.Position.X + 320;
//		
//		int Y = (int)y;
//		int X = (int)x;
//		for (int i=Y-10; i<Y+10; i++) {
//			for (int j=X-10; j<X+10; j++) {
//			
//				depthViewer.outputPixels [i * 640 + j].r = 255;
//				depthViewer.outputPixels [i * 640 + j].g = 255;
//				depthViewer.outputPixels [i * 640 + j].b = 255;
//				depthViewer.outputPixels [i * 640 + j].a = 255;
//			}
//		}
		

		
		
		
		GameObject screenDetection = GameObject.Find ("ScreenDetection");
		ScreenDetection screenScript = screenDetection.GetComponent<ScreenDetection> ();
		screenScript.drawHandPoint (e.Position);
		
	}
	
	void handGenerator_HandDestroy (object sender, HandDestroyEventArgs e)
	{
		
	}
	
	void skeletonCapbility_CalibrationComplete (object sender, CalibrationProgressEventArgs e)
	{
		if (e.Status == CalibrationStatus.OK) {
			this.skeletonCapbility.StartTracking (e.ID);
			this.joints.Add (e.ID, new Dictionary<SkeletonJoint, SkeletonJointPosition> ());
		} else if (e.Status != CalibrationStatus.ManualAbort) {
			if (this.skeletonCapbility.DoesNeedPoseForCalibration) {
				this.poseDetectionCapability.StartPoseDetection (calibPose, e.ID);
			} else {
				this.skeletonCapbility.RequestCalibration (e.ID, true);
			}
		}
	}
	
	void poseDetectionCapability_PoseDetected (object sender, PoseDetectedEventArgs e)
	{
		this.poseDetectionCapability.StopPoseDetection (e.ID);
		this.skeletonCapbility.RequestCalibration (e.ID, true);
	}

	void userGenerator_NewUser (object sender, NewUserEventArgs e)
	{
		if (this.skeletonCapbility.DoesNeedPoseForCalibration) {
			this.poseDetectionCapability.StartPoseDetection (this.calibPose, e.ID);
		} else {
			this.skeletonCapbility.RequestCalibration (e.ID, true);
		}
	}

	void userGenerator_LostUser (object sender, UserLostEventArgs e)
	{
		this.joints.Remove (e.ID);
	}
	
	private void GetJoint (int user, SkeletonJoint joint)
	{
		SkeletonJointPosition pos = this.skeletonCapbility.GetSkeletonJointPosition (user, joint);
		if (pos.Position.Z == 0) {
			pos.Confidence = 0;
		} else {
			Vector3 worldPosition = new Vector3 (pos.Position.X, pos.Position.Y, pos.Position.Z);
			Vector3 imagePosition = openNI.ConvertWorldToImageSpace (worldPosition);
			pos.Position = new Point3D (imagePosition.x, imagePosition.y, imagePosition.z);
		}
		this.joints [user] [joint] = pos;
	}
	
	private void GetJoints (int user)
	{
		GetJoint (user, SkeletonJoint.Head);
		GetJoint (user, SkeletonJoint.Neck);

		GetJoint (user, SkeletonJoint.LeftShoulder);
		GetJoint (user, SkeletonJoint.LeftElbow);
		GetJoint (user, SkeletonJoint.LeftHand);

		GetJoint (user, SkeletonJoint.RightShoulder);
		GetJoint (user, SkeletonJoint.RightElbow);
		GetJoint (user, SkeletonJoint.RightHand);

		GetJoint (user, SkeletonJoint.Torso);

//            GetJoint(user, SkeletonJoint.LeftHip);
//            GetJoint(user, SkeletonJoint.LeftKnee);
//            GetJoint(user, SkeletonJoint.LeftFoot);
//
//            GetJoint(user, SkeletonJoint.RightHip);
//            GetJoint(user, SkeletonJoint.RightKnee);
//            GetJoint(user, SkeletonJoint.RightFoot);
	}
	
	private void DrawSkeleton (int user)
	{
		GetJoints (user);
		Dictionary<SkeletonJoint, SkeletonJointPosition> dict = this.joints [user];
		
		DrawLine (dict, SkeletonJoint.Head, SkeletonJoint.Neck);

		DrawLine (dict, SkeletonJoint.LeftShoulder, SkeletonJoint.Torso);
		DrawLine (dict, SkeletonJoint.RightShoulder, SkeletonJoint.Torso);

		DrawLine (dict, SkeletonJoint.Neck, SkeletonJoint.LeftShoulder);
		DrawLine (dict, SkeletonJoint.LeftShoulder, SkeletonJoint.LeftElbow);
		DrawLine (dict, SkeletonJoint.LeftElbow, SkeletonJoint.LeftHand);

		DrawLine (dict, SkeletonJoint.Neck, SkeletonJoint.RightShoulder);
		DrawLine (dict, SkeletonJoint.RightShoulder, SkeletonJoint.RightElbow);
		DrawLine (dict, SkeletonJoint.RightElbow, SkeletonJoint.RightHand);
	
		string win2 = "ColorDepth";
		Emgu.CV.CvInvoke.cvNamedWindow (win2);
		CvInvoke.cvShowImage (win2, colorDepth);
	}

	private void DrawLine (Dictionary<SkeletonJoint, SkeletonJointPosition> dict, SkeletonJoint j1, SkeletonJoint j2)
	{
		
		Point3D pos1 = dict [j1].Position;
		Point3D pos2 = dict [j2].Position;
		
	
		if (dict [j1].Confidence == 0 || dict [j2].Confidence == 0)
			return;
		
		colorDepth.Draw (new LineSegment2D (new Point ((int)pos1.X, (int)pos1.Y), new Point ((int)pos1.X, (int)pos2.Y)), new Gray (255), 1);
			
	

	}

	private void ReaderThread ()
	{
		while (this.shouldRun==true) {
			int[] users = this.userGenerator.GetUsers ();
		
			foreach (int user in users) {
//			if (this.shouldPrintID) {
//				Point3D com=this.userGenerator.GetCoM(user);
//				Vector comVector=ZigInput.ConvertWorldToImageSpace(new Vector3(com.X,com.Y,com.Z));
//					com=new Point3D(comVector.x,comVector.y,comVector.z);
//				
//			}	
				if (this.shouldDrawSkeleton && this.skeletonCapbility.IsTracking (user))
//                        if (this.skeletonCapbility.IsTracking(user))
				
					DrawSkeleton (user);
			}
			
		}		
	}
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	void drawDepth (ZigDepth depth)
	{
		
		//Get the data of depth
		rawDepthMap = depth.data;
		
		
		
		originalDepth = new Image<Gray, byte> (depth.xres, depth.yres); 
		kinectDepth = new Image<Gray, byte> (depth.xres, depth.yres);
		
		byte[,,] originalDepthData = originalDepth.Data;
		byte[,,] kinectDepthData = kinectDepth.Data;
		for (int i=0; i<depth.xres; i++) {
			for (int j=0; j<depth.yres; j++) {
			
				originalDepthData [j, i, 0] = (byte)rawDepthMap [j * depth.xres + i];
				kinectDepthData [j, i, 0] = (byte)(rawDepthMap [j * depth.xres + i] * 255 / 4096);
			}
		}
		colorDepth = kinectDepth.Copy ();
		
//	
		
		string win1 = "KinectDepth";
		Emgu.CV.CvInvoke.cvNamedWindow (win1);
		CvInvoke.cvShowImage (win1, kinectDepth);
		
		
	}

	void Zig_Update (ZigInput input)
	{
		
		openNI = input.getOpenNI ();
		drawDepth (ZigInput.Depth);
		if (gestureGenerator == null) {
			gestureGenerator = openNI.Gestures;
		
			gestureGenerator.AddGesture ("MovingHand");
			gestureGenerator.AddGesture ("Wave");
			gestureGenerator.AddGesture ("Click");
			gestureGenerator.AddGesture ("RaiseHand");  
			
			gestureGenerator.GestureRecognized += gestureGenerator_GestureRecognized;
			gestureGenerator.StartGenerating ();
			
		}
		
		if (handGenerator == null) {
			handGenerator = openNI.Hands;
		
			handGenerator.HandCreate += handGenerator_HandCreate;
			handGenerator.HandUpdate += handGenerator_HandUpdate;
			handGenerator.HandDestroy += handGenerator_HandDestroy;
			handGenerator.StartGenerating ();
		}
		
		if (userGenerator == null) {
			userGenerator = openNI.Users;
			
			this.skeletonCapbility = this.userGenerator.SkeletonCapability;
			this.poseDetectionCapability = this.userGenerator.PoseDetectionCapability;
			this.calibPose = this.skeletonCapbility.CalibrationPose;
			
			this.userGenerator.NewUser += userGenerator_NewUser;
			this.userGenerator.LostUser += userGenerator_LostUser;
			this.poseDetectionCapability.PoseDetected += poseDetectionCapability_PoseDetected;
			this.skeletonCapbility.CalibrationComplete += skeletonCapbility_CalibrationComplete;
			
			this.skeletonCapbility.SetSkeletonProfile (SkeletonProfile.All);
			this.joints = new Dictionary<int,Dictionary<SkeletonJoint,SkeletonJointPosition>> ();
            
			this.userGenerator.StartGenerating ();
			
			this.shouldRun = true;
			this.readerThread = new Thread (ReaderThread);
			this.readerThread.Start ();
		}
	}
	
	public void OnDestroy ()
	{
		this.shouldRun = false;
		this.readerThread.Join ();
		this.userGenerator.StopGenerating ();
		this.handGenerator.StopGenerating ();
		this.gestureGenerator.StopGenerating ();
		
	}
}


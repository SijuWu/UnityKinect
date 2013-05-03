using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

public class ScreenDetection : MonoBehaviour
{
	private int leftUpIndex;
	private	int rightUpIndex;
	private	int rightDownIndex;
	private	int leftDownIndex;
	const int maxDepth = 4096;
	const int maxCorners = 4;
	double qualityLevel = 0.01;
	double minDistance = 10;
	int blockSize = 3;
	int userHarrisDetector = 0;
	double k = 0.04;
	List<Point>corners = new List<Point> ();
	List<Point>screenCorners = new List<Point> ();
	Vector3[] cornerCoordinates = new Vector3[maxCorners];
	bool screenCatch = false;
	int frameCount = 0;
	const int maxFrame = 20;
	List<int> leftUpDepth = new List<int> ();
	List<int> rightUpDepth = new List<int> ();
	List<int> rightDownDepth = new List<int> ();
	List<int> leftDownDepth = new List<int> ();
	int[]cornerDepth = new int[4];
	bool detectFinish = false;
	Image<Gray,byte> originalDepth;
	Image<Gray,byte> kinectDepth;
	Image<Bgr,byte> kinectImage;
	Color32[] rawImageMap;
	short[] rawDepthMap;
	// Use this for initialization
	void Start ()
	{	
		ZigInput.Instance.AddListener (gameObject);
	}

	public Image<Gray,byte> getKinectDepth ()
	{	
		return this.kinectDepth;
	}

	void displayImage (ZigImage image, ZigDepth depth)
	{
		//Get the data of image
		rawImageMap = image.data;
		//Get the data of depth
		rawDepthMap = depth.data;
		
		//Get the max depth
		//Some problem here, the value is not correct
		short max = 0;
		foreach (short depthValue in rawDepthMap) {
			if (depthValue > max)
				max = depthValue;
		}
		short min = 100;
		
		originalDepth = new Image<Gray, byte> (depth.xres, depth.yres); 
		kinectDepth = new Image<Gray, byte> (depth.xres, depth.yres);
		
		byte[,,] originalDepthData = originalDepth.Data;
		byte[,,] kinectDepthData = kinectDepth.Data;
		for (int i=0; i<depth.xres; i++) {
			for (int j=0; j<depth.yres; j++) {
			
				originalDepthData [j, i, 0] = (byte)rawDepthMap [j * depth.xres + i];
				kinectDepthData [j, i, 0] = (byte)(rawDepthMap [j * depth.xres + i] * 255 / maxDepth);
			}
		}
		
		
//		//Save the original image data in kinectImaga
		kinectImage = new Image<Bgr, byte> (image.xres, image.yres);
		byte[,,] kinectImageData = kinectImage.Data;
		for (int i=0; i<image.xres; i++) {
			for (int j=0; j<image.yres; j++) {
				
				kinectImageData [j, i, 0] = (byte)rawImageMap [j * image.xres + i].b;
				kinectImageData [j, i, 1] = (byte)rawImageMap [j * image.xres + i].g;
				kinectImageData [j, i, 2] = (byte)rawImageMap [j * image.xres + i].r;
			}
		}
		
		string win1 = "KinectImage";
		Emgu.CV.CvInvoke.cvNamedWindow (win1);
		CvInvoke.cvShowImage (win1, kinectImage);
		
		string win2 = "KinectDepth";
		Emgu.CV.CvInvoke.cvNamedWindow (win2);
		CvInvoke.cvShowImage (win2, kinectDepth);
	}

	void getDepthAndColorImage (ZigImage image, ZigDepth depth)
	{
		
		displayImage (image, depth);
		Image<Gray,byte> grayImage = kinectImage.Convert<Gray,byte> ();
		Image<Gray,byte> originalGray=grayImage.Copy();
		grayImage = kinectImage.Convert<Gray,byte> ();
		byte[,,] grayImageData = grayImage.Data;
		for (int i=0; i<grayImage.Width; i++)
			for (int j=0; j<grayImage.Height; j++) {
				if (grayImageData [j, i, 0] < 180)
					grayImageData [j, i, 0] = 0;
				else
					grayImageData [j, i, 0] = 255;
			}
		
		grayImage.SmoothBlur (3, 3);
		
		Image<Gray,byte> cannyImage = new Image<Gray, byte> (grayImage.Size);
		int thresh = 100;
		int ratio = 2;
		int apetureSize = 3;
		CvInvoke.cvCanny (grayImage, cannyImage, thresh, thresh * ratio, apetureSize);
		
		Image< Bgr,byte> contourImage = new Image<Bgr, byte> (grayImage.Size);
		
		Contour<Point> maxContour = null;
//		Contour<Point> longestContour = null;
//		double maxLength = 0;
		double maxArea = 0;
		using (MemStorage storage = new MemStorage ()) {
			for (Contour<Point>contours=cannyImage.FindContours(); contours!=null; contours=contours.HNext) {
				if (contours.Area > maxArea) {
					maxContour = contours;
					maxArea = contours.Area;
				}	
//				Point[] hull = maxContour.GetConvexHull (Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE, storage).ToArray ();
//				if(hull.GetLength()>maxLength)
//				{
//					longestContour=contours;
//					maxLength=hull.get
//				}
			}
		}
		
		using (MemStorage storage= new MemStorage()) {
			if (maxContour != null) {
				Point[] hull = maxContour.GetConvexHull (Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE, storage).ToArray ();
				contourImage.DrawPolyline (hull, true, new Bgr (255, 255, 255), 1);
			}
		}
		
		Image<Gray,byte> cornerImage = contourImage.Convert<Gray,byte> ();
		Image<Bgr,byte> eigImage = new Image<Bgr,byte> (cornerImage.Size);
		Image<Bgr,byte> temImage = new Image<Bgr,byte> (cornerImage.Size);
		

		
		if (maxContour != null) {
			PointF[][] cornersF = contourImage.GoodFeaturesToTrack (4, qualityLevel, minDistance, blockSize);


			foreach (PointF point in cornersF[0]) {
				corners.Add (Point.Round (point));
				contourImage.Draw (new CircleF (point, 2), new Bgr (200, 0, 200), 5);
			}
			if (cornersF [0].Length == 4) {
				contourImage.Draw (new CircleF (cornersF [0] [0], 2), new Bgr (200, 0, 200), 5);
				contourImage.Draw (new CircleF (cornersF [0] [1], 2), new Bgr (200, 0, 200), 5);
				contourImage.Draw (new CircleF (cornersF [0] [2], 2), new Bgr (200, 0, 200), 5);
				contourImage.Draw (new CircleF (cornersF [0] [3], 2), new Bgr (200, 0, 200), 5);
			}
			
		}
		
	
		if (Input.GetKeyDown (KeyCode.S)) {
			checkCornerOrder (ref leftUpIndex, ref rightUpIndex, ref rightDownIndex, ref leftDownIndex);

			screenCorners.Add (corners [leftUpIndex]);
			screenCorners.Add (corners [rightUpIndex]);
			screenCorners.Add (corners [rightDownIndex]);
			screenCorners.Add (corners [leftDownIndex]);
			screenCatch = true;
		}
		
		if (screenCatch == true) {
			if (frameCount < maxFrame) {
				checkCornerOrder (ref leftUpIndex, ref rightUpIndex, ref rightDownIndex, ref leftDownIndex);
			
				if (System.Math.Abs (corners [leftUpIndex].X - screenCorners [0].X) > 10 ||
					System.Math.Abs (corners [leftUpIndex].Y - screenCorners [0].Y) > 10) {
					return;
				}
				if (System.Math.Abs (corners [rightUpIndex].X - screenCorners [1].X) > 10 ||
					System.Math.Abs (corners [rightUpIndex].Y - screenCorners [1].Y) > 10) {
					return;
				}
				if (System.Math.Abs (corners [rightDownIndex].X - screenCorners [2].X) > 10 ||
					System.Math.Abs (corners [rightDownIndex].Y - screenCorners [2].Y) > 10) {
					return;
				}
				if (System.Math.Abs (corners [leftDownIndex].X - screenCorners [3].X) > 10 ||
					System.Math.Abs (corners [leftDownIndex].Y - screenCorners [3].Y) > 10) {
					return;
				}
				
				int depthLeftUp = rawDepthMap [corners [leftUpIndex].Y * depth.xres + corners [leftUpIndex].X];
				int depthRightUp = rawDepthMap [corners [rightUpIndex].Y * depth.xres + corners [rightUpIndex].X];
				int depthRightDown = rawDepthMap [corners [rightDownIndex].Y * depth.xres + corners [rightDownIndex].X];
				int depthLeftDown = rawDepthMap [corners [leftDownIndex].Y * depth.xres + corners [leftDownIndex].X];

				if (depthLeftUp != 0 && depthRightUp != 0 && depthRightDown != 0 && depthLeftDown != 0) {

					leftUpDepth.Add (depthLeftUp);
					rightUpDepth.Add (depthRightUp);
					rightDownDepth.Add (depthRightDown);
					leftDownDepth.Add (depthLeftDown);
					frameCount++;
				}
			} else {
				for (int i=0; i<maxFrame; i++) {
					
					cornerDepth [0] += leftUpDepth [i];
					cornerDepth [1] += rightUpDepth [i];
					cornerDepth [2] += rightDownDepth [i];
					cornerDepth [3] += leftDownDepth [i];
				}
				
				cornerDepth [0] /= maxFrame;
				cornerDepth [1] /= maxFrame;
				cornerDepth [2] /= maxFrame;
				cornerDepth [3] /= maxFrame;
				screenCatch = false;
			
				for (int i=0; i<maxCorners; i++) {
					
//					Vector3 imageCoor = new Vector3 (screenCorners [i].X, screenCorners [i].Y, cornerDepth [i]);
//					Vector3 realCoor=ZigInput.ConvertImageToWorldSpace (imageCoor);
//					cornerCoordinates.SetValue (new MCvPoint3D32f (realCoor.x * 0.001f, realCoor.y * 0.001f, realCoor.z * 0.001f), i);

					OpenNI.Point3D proj = new OpenNI.Point3D (screenCorners [i].X, screenCorners [i].Y, cornerDepth [i]);
					float F = 0.0019047619f;
					OpenNI.Point3D real = new OpenNI.Point3D ((proj.X - 320) * proj.Z * F, (240 - proj.Y) * proj.Z * F, proj.Z);		
//					cornerCoordinates.SetValue (new Vector3 (real.X * 0.001f, real.Y * 0.001f, real.Z * 0.001f), i);
					cornerCoordinates.SetValue (new Vector3 (real.X, real.Y, real.Z), i);
					detectFinish = true;
					
					// from mm to meters 
				}
			}
		}
	
		if (maxContour != null) {
			Point[] screen = new Point[4];

			screen [0] = corners [leftUpIndex];
			screen [1] = corners [rightUpIndex];
			screen [2] = corners [rightDownIndex];
			screen [3] = corners [leftDownIndex];
			contourImage.DrawPolyline (screen, true, new Bgr (100, 0, 0), 1);
			
		}
		Image<Bgr,byte> mixImage = new Image<Bgr, byte> (contourImage.Size);
		CvInvoke.cvAddWeighted (kinectImage.Convert<Bgr,byte> (), 0.5, contourImage.Convert<Bgr,byte> (), 0.5, 0, mixImage);
	 Image<Bgr,byte> mixImage2 = new Image<Bgr, byte> (contourImage.Size);
		CvInvoke.cvAddWeighted (kinectDepth.Convert<Bgr,byte> (), 0.5, contourImage.Convert<Bgr,byte> (), 0.5, 0, mixImage2);
		string win0 = "OriginalGray";
		Emgu.CV.CvInvoke.cvNamedWindow (win0);
		CvInvoke.cvShowImage (win0, originalGray);
		
		string win1 = "KinectImage";
		Emgu.CV.CvInvoke.cvNamedWindow (win1);
		CvInvoke.cvShowImage (win1, kinectImage);
		
		string win2 = "GrayImage";
		Emgu.CV.CvInvoke.cvNamedWindow (win2);
		CvInvoke.cvShowImage (win2, grayImage);
		
		string win3 = "CannyImage";
		Emgu.CV.CvInvoke.cvNamedWindow (win3);
		CvInvoke.cvShowImage (win3, cannyImage);
		
		string win4 = "ContourImage";
		Emgu.CV.CvInvoke.cvNamedWindow (win4);
		CvInvoke.cvShowImage (win4, contourImage);
		
		string win5 = "DepthImage";
		Emgu.CV.CvInvoke.cvNamedWindow (win5);
		CvInvoke.cvShowImage (win5, kinectDepth);
	    
		string win6 = "MixImage";
		Emgu.CV.CvInvoke.cvNamedWindow (win6);
		CvInvoke.cvShowImage (win6, mixImage);
		
		string win7 = "MixImage2";
		Emgu.CV.CvInvoke.cvNamedWindow (win7);
		CvInvoke.cvShowImage (win7, mixImage2);
	}
	
	void Zig_Update (ZigInput input)
	{
//		if (detectFinish == false)
//			getDepthAndColorImage (ZigInput.Image, ZigInput.Depth);
//		else {
//			displayImage (ZigInput.Image, ZigInput.Depth);
//			GameObject screen = GameObject.Find ("Screen");
//			CreatePlane createPlane = screen.GetComponent<CreatePlane> ();
//			createPlane.setPlane (cornerCoordinates);
//		}
		displayImage (ZigInput.Image, ZigInput.Depth);
	}
	
	void checkCornerOrder (ref int leftUpIndex, ref int rightUpIndex, ref int rightDownIndex, ref int leftDownIndex)
	{
		
		double[] dis = new double[4];
		
		dis.SetValue (System.Math.Sqrt (System.Math.Pow (corners [0].X, 2) + System.Math.Pow (corners [0].Y, 2)), 0);
		dis.SetValue (System.Math.Sqrt (System.Math.Pow (corners [1].X, 2) + System.Math.Pow (corners [1].Y, 2)), 1);
		dis.SetValue (System.Math.Sqrt (System.Math.Pow (corners [2].X, 2) + System.Math.Pow (corners [2].Y, 2)), 2);
		dis.SetValue (System.Math.Sqrt (System.Math.Pow (corners [3].X, 2) + System.Math.Pow (corners [3].Y, 2)), 3);
		
		double disMin = 999999;
		double disMax = 0;
      
		leftUpIndex = 100;
		rightUpIndex = 100;
		rightDownIndex = 100;
		leftDownIndex = 100;
		
		for (int i=0; i<4; i++) {
			if (dis [i] > disMax) {
				disMax = dis [i];
				rightDownIndex = i;
			}
			if (dis [i] < disMin) {
				disMin = dis [i];
				leftUpIndex = i;
			}	
		}
		
		for (int i=0; i<4; i++) {
			if (i != leftUpIndex && i != rightDownIndex) {
				if (leftDownIndex == 100)
					leftDownIndex = i;
				else
					rightUpIndex = i;
			}
		}
		
		if (corners [rightUpIndex].X < corners [leftDownIndex].X) {
			int changeIndex = leftDownIndex;
			leftDownIndex = rightUpIndex;
			rightUpIndex = changeIndex;
		}
	}
	
	public void drawHandPoint (OpenNI.Point3D point)
	{

//		kinectDepth.Draw(new CircleF(new PointF(point.X+640/2,-point.Y+480/2),2),new Gray(255),1);
//		string win5 = "DepthImage";
//		Emgu.CV.CvInvoke.cvNamedWindow (win5);
//		CvInvoke.cvShowImage (win5, kinectDepth);
		
		
		float F = 0.0019047619f;

		OpenNI.Point3D handPoint = new OpenNI.Point3D (point.X * point.Z * F, point.Y * point.Z * F, point.Z);		
//					
		GameObject cylinder = GameObject.Find ("Cylinder");
		
		cylinder.transform.position = new Vector3 (handPoint.X, handPoint.Y, handPoint.Z);
		Camera.main.transform.position = new Vector3 (0, 0, 0);
		Camera.main.transform.LookAt (new Vector3 (0, 0, 1));
	}
}


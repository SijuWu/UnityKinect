using UnityEngine;
using System.Collections;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;

public class EmguCV : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	void OnGUI ()
	{
//		//The name of the window
//string win1 = "Test Window";
// 
////Create the window using the specific name
//Emgu.CV.CvInvoke.cvNamedWindow(win1);
//		//Create an image of 400x200 of Blue color
//using (Image<Bgr, byte> img = new Image<Bgr, byte>(400, 200, new Bgr(255, 0, 0))) 
//{
//   //Create the font
//   MCvFont f = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX, 1.0, 1.0);
//   //Draw "Hello, world." on the image using the specific font
//   img.Draw("Hello, world", ref f, new Point(10, 80), new Bgr(0, 255, 0));
// 
//   //Show the image
//   CvInvoke.cvShowImage(win1, img.Ptr);
//   //Wait for the key pressing event
//   CvInvoke.cvWaitKey(0);
//					
//   //Destory the window
//   CvInvoke.cvDestroyWindow(win1); 
//
//}
	}
}

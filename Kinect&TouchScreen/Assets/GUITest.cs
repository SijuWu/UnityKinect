using UnityEngine;
using System.Collections;

public class GUITest : MonoBehaviour {
	
 
	bool picked=false;
	Vector3 lastMouse;
	Transform pickedTransform;
	
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		int i=Input.touchCount;
		Input.multiTouchEnabled=true;
		if(i>0)
		{
		     print (1);
		}
	}
	
	void OnGUI () {
		 
	
//		float x=Screen.currentResolution.width;
//		float y=Screen.currentResolution.height;
//		float a=Screen.width;
//		float b=Screen.height;
		
		int fingerCount=0;
		foreach(Touch touch in Input.touches)
		{
			if(touch.phase!=TouchPhase.Ended && touch.phase!=TouchPhase.Canceled)
				fingerCount++;
		}
		if(fingerCount>0)
		{
			print (fingerCount+"fingers");
		}
		 if(Input.GetButtonDown("Fire1"))
		{
			Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit))
			{
				print ("pick something");
				pickedTransform=hit.transform;

				picked=true;
				Vector3 mouseScreen=Input.mousePosition;
				mouseScreen.z=60;
				lastMouse=Camera.main.ScreenToWorldPoint(mouseScreen);
				 print (lastMouse.x+" "+lastMouse.y);
				
			}
        
		}
		if(Input.GetButton("Fire1"))
		{
//			if(picked==true)
//			{
				Vector3 mouseScreen=Input.mousePosition;
				mouseScreen.z=60;
				Vector3 worldMouse=Camera.main.ScreenToWorldPoint(mouseScreen);
				
//				Matrix4x4 worldToLocal=pickedTransform.worldToLocalMatrix;
//				Vector3 localMouse=worldToLocal.MultiplyVector(worldMouse);
//				Vector3 localLastMouse=worldToLocal.MultiplyVector(lastMouse);
				
//			    pickedTransform.position=new Vector3(worldMouse.x,worldMouse.y,worldMouse.z);
		        

			
				lastMouse=worldMouse;
				print (lastMouse.x+" "+lastMouse.y);
//			}
		}
		if(Input.GetButtonUp("Fire1"))
		{
			picked=false;
			pickedTransform=null;
		}
	}
	
}

using UnityEngine;
using System.Collections;

public class MultiTouchObject : MonoBehaviour
{

	private ArrayList thisFrameEvents = new ArrayList ();
	private ArrayList multiTouches = new ArrayList ();
	protected Camera renderingCamera;
	bool zTranslationBegin = false;
	private int zFingerID=-1;
	// Use this for initialization
	void Start ()
	{
		noTouches ();
		startup ();
		renderingCamera = Camera.main;
	
		if (renderingCamera == null) {
			// someone didnt tag their cameras properly!!
			// just grab the first one
			if (Camera.allCameras.Length == 0)
				return;
			renderingCamera = Camera.allCameras [0];
		}
	}

	virtual public void startup ()
	{
		// a place to put any post-start code	
	}
	
	void Update ()
	{
		checkForTouches ();
		if (thisFrameEvents.Count > 0||this.zTranslationBegin==true) {
			distributeTouches ();
		} else {
			noTouches ();
		}
	}
	
	public void checkForTouches ()
	{
		// some defensive programming
		// check to make sure that we have a collider and that we found a camera
		if (gameObject.collider == null) {
			Debug.Log ("Object: " + gameObject.name + " is trying to collect touches but has no collider");
			return;
		}
		if (renderingCamera == null) {
			Debug.Log ("Object: " + gameObject.name + " cannot find a camera.");
			return;			
		}
		
		// clear out our frame event buffer
		thisFrameEvents.Clear ();
		multiTouches.Clear ();
		RaycastHit hit = new RaycastHit (); // need one of these to check for hits
		
		GameObject sceneManager = GameObject.Find ("SceneManager");
		SceneManager sceneManagerScript = sceneManager.GetComponent<SceneManager> ();
			
		
		// step through each touch and see if any are hitting me
		int i;
		for (i = 0; i < iPhoneInput.touchCount; i++) {
			iPhoneTouch touch = iPhoneInput.GetTouch (i);
			this.multiTouches.Add (touch);
			Vector3 screenPosition = new Vector3 (touch.position.x, touch.position.y, 0.0f);
			
			Camera userCamera=GameObject.Find("UserCamera").camera;
			if (Physics.Raycast (userCamera.ScreenPointToRay (screenPosition), out hit, Mathf.Infinity)) { 
				// do we have a hit?
				if (hit.transform.gameObject == gameObject) {
					////////////////
//					GameObject kinectGUI=GameObject.Find("KinectGUI");
//					KinectGUIScript kinectScript=kinectGUI.GetComponent<KinectGUIScript>();
//					if(kinectScript.zTranActive==true)
//						gameObject.transform.parent=GameObject.Find("leftHand").transform;
					//////////////////////
					if (sceneManagerScript.getObjectSelected () == null) {
						sceneManagerScript.setObjectSelected (gameObject);
						
					}
					if (sceneManagerScript.getObjectSelected () == gameObject) {
						thisFrameEvents.Add (touch);
					}
				}
			}
		}
		if (this.multiTouches.Count == 0 && sceneManagerScript.getObjectSelected () == gameObject) {
			sceneManagerScript.clearObjectSelected ();
			this.zTranslationBegin = false;
			this.zFingerID = -1;
		}
	}
	
	public void distributeTouches ()
	{
		// how many touches do we have? 
		if (thisFrameEvents.Count == 1 && this.multiTouches.Count == 1) {
			handleSingleTouch (thisFrameEvents [0] as iPhoneTouch);
			return;
		}
		if (this.zTranslationBegin == false) {
			if (thisFrameEvents.Count == 1 && this.multiTouches.Count == 2) {
				iPhoneTouch zTranslationTouch = new iPhoneTouch ();
				foreach (iPhoneTouch touch in this.multiTouches) {
					if (touch != thisFrameEvents [0])
						zTranslationTouch = touch;
					this.zFingerID = touch.fingerId;
				}
				handleSingleComplexTouch (zTranslationTouch);	
				this.zTranslationBegin = true;
			}
		} else {
			if (this.multiTouches.Count != 0) {
				foreach (iPhoneTouch touch in this.multiTouches) {
					if (touch.fingerId == this.zFingerID)
						handleSingleComplexTouch (touch);
				}
			}
			
		}
		
		if (thisFrameEvents.Count == 2) {
			handleDoubleTouch (thisFrameEvents);
			return;
		}
		if (thisFrameEvents.Count == 3) {
			handleThreeTouch (thisFrameEvents);
		}
	}
		
	virtual public void handleThreeTouch (ArrayList touches)
	{
	} // three touches
	virtual public void handleDoubleTouch (ArrayList touches)
	{
	} // two touches
	virtual public void handleSingleTouch (iPhoneTouch aTouch)
	{
	} // just one touch 
	virtual public void handleSingleComplexTouch (iPhoneTouch zTouch)
	{
	}

	virtual public void noTouches ()
	{
	} // reset or clear the state
}

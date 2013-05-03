using UnityEngine;
using System.Collections;

public class BMultiTouchObject : MonoBehaviour
{
	private ArrayList thisFrameEvents = new ArrayList ();
	protected Camera renderingCamera;
	private ArrayList touchList = new ArrayList ();
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
		if (thisFrameEvents.Count > 0) {
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
		RaycastHit hit = new RaycastHit (); // need one of these to check for hits
		
		// step through each touch and see if any are hitting me
		int i;
		for (i = 0; i < iPhoneInput.touchCount; i++) {
			iPhoneTouch touch = iPhoneInput.GetTouch (i);
			this.touchList.Add (touch);
			Vector3 screenPosition = new Vector3 (touch.position.x, touch.position.y, 0.0f);
			
			if (Physics.Raycast (Camera.main.ScreenPointToRay (screenPosition), out hit, Mathf.Infinity)) { 
				// do we have a hit?
				if (hit.transform.gameObject == gameObject)
				{
					
					thisFrameEvents.Add (touch);
				}
					
			}
		}
	}
	
	public void distributeTouches ()
	{
		// how many touches do we have? 
		if (thisFrameEvents.Count == 1 && this.touchList.Count == 1) {
			handleSingleTouch (thisFrameEvents [0] as iPhoneTouch);
			return;
		}
		if (thisFrameEvents.Count == 1 && this.touchList.Count == 2) {
			iPhoneTouch anotherTouch;
			foreach (iPhoneTouch touch in touchList) {
				if (thisFrameEvents [0] != touch)
					anotherTouch = touch;
			}
			handleSingleComplexTouch (thisFrameEvents [0]as iPhoneTouch, anotherTouch);
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
	} // more than two
	virtual public void handleDoubleTouch (ArrayList touches)
	{
	} // two touches
	virtual public void handleSingleTouch (iPhoneTouch aTouch)
	{
	} // just one touch 
	
	virtual public void handleSingleComplexTouch (iPhoneTouch aTouch, iPhoneTouch anotherTouch)
	{
		//One touch on the object, the other not
	}

	virtual public void noTouches ()
	{
	} // reset or clear the state
}
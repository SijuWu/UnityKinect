using UnityEngine;
using System.Collections;

public class MultiTouchObject : MonoBehaviour
{
	enum MoveMode
	{
		None,
		XYTranslate,
		ZTranslate,
		Pinch
	};
	private ArrayList thisFrameEvents = new ArrayList ();
	private ArrayList multiTouches = new ArrayList ();
	protected ArrayList lastMultiTouches = new ArrayList ();
	protected Camera renderingCamera;
	bool selected = false;
//	bool zTranslationBegin = false;
	bool spaceMove = false;
	bool spaceMoveBegin = false;
	private int zFingerID = -1;
	MoveMode moveMode = MoveMode.None;
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
		if (this.multiTouches.Count != 0 && selected == true) {
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
		lastMultiTouches.Clear ();
		foreach (iPhoneTouch touch in multiTouches) {
			lastMultiTouches.Add (touch);
		}
		
		multiTouches.Clear ();
		RaycastHit hit = new RaycastHit (); // need one of these to check for hits
		
		GameObject sceneManager = GameObject.Find ("SceneManager");
		SceneManager sceneManagerScript = sceneManager.GetComponent<SceneManager> ();
			
		if (selected == true) {
			foreach (iPhoneTouch touch in iPhoneInput.touches) {
				this.multiTouches.Add (touch);
				Vector3 screenPosition = new Vector3 (touch.position.x, touch.position.y, 0.0f);
				Camera userCamera = GameObject.Find ("UserCamera").camera;
				if (Physics.Raycast (userCamera.ScreenPointToRay (screenPosition), out hit, Mathf.Infinity)) { 
					if (hit.transform.gameObject == gameObject) {		
						thisFrameEvents.Add (touch);
					}
				}
			}
			
			if (iPhoneInput.touchCount == 0) {
				if (spaceMove == false) {
					sceneManagerScript.clearObjectSelected ();
					gameObject.transform.parent = null;
					selected = false;
					spaceMoveBegin = false;
				} else {
					if (spaceMoveBegin == false) {
						KinectSkeleton kinectSkeleton = GameObject.Find ("User").GetComponent<KinectSkeleton> ();
						GameObject leftHandPoint = kinectSkeleton.getLHInstance ();
						GameObject rightHandPoint = kinectSkeleton.getRHInstance ();
						Camera userCamera = GameObject.Find ("UserCamera").camera;
						Vector3 objectPosition = userCamera.WorldToViewportPoint (gameObject.transform.position);
						if (Vector3.Distance (leftHandPoint.transform.position, objectPosition) < Vector3.Distance (rightHandPoint.transform.position, objectPosition)) {
							GameObject leftHand = GameObject.Find ("LeftHand");
						
							gameObject.transform.parent = leftHand.transform;
						} else {
							GameObject rightHand = GameObject.Find ("RightHand");
				
							gameObject.transform.parent = rightHand.transform;
						}
						spaceMoveBegin = true;
					}
				}	
			} else {
				gameObject.transform.parent = null;
				spaceMoveBegin = false;
			}
			
		} else {
			foreach (iPhoneTouch touch in iPhoneInput.touches) {
				this.multiTouches.Add (touch);
				Vector3 screenPosition = new Vector3 (touch.position.x, touch.position.y, 0.0f);
				Camera userCamera = GameObject.Find ("UserCamera").camera;
				
				if (Physics.Raycast (userCamera.ScreenPointToRay (screenPosition), out hit, Mathf.Infinity)) { 
					if (hit.transform.gameObject == gameObject) {
						if (sceneManagerScript.getObjectSelected () == null) {
							sceneManagerScript.setObjectSelected (gameObject);
							selected = true;
						}
						if (sceneManagerScript.getObjectSelected () == gameObject) {
							thisFrameEvents.Add (touch);
						}
					}
				}
			}
		}
		if (spaceMoveBegin == false) {
			if (lastMultiTouches.Count != multiTouches.Count) {
				checkMoveMode ();
			}
		}
		
		// step through each touch and see if any are hitting me
		int i;
		for (i = 0; i < iPhoneInput.touchCount; i++) {
//			iPhoneTouch touch = iPhoneInput.GetTouch (i);
//			this.multiTouches.Add (touch);
//			Vector3 screenPosition = new Vector3 (touch.position.x, touch.position.y, 0.0f);
//			
//			Camera userCamera = GameObject.Find ("UserCamera").camera;
//			if (Physics.Raycast (userCamera.ScreenPointToRay (screenPosition), out hit, Mathf.Infinity)) { 
//				// do we have a hit?
//				if (hit.transform.gameObject == gameObject) {
//					if (sceneManagerScript.getObjectSelected () == null) {
//						sceneManagerScript.setObjectSelected (gameObject);
//						touched = true;
//					}
//					if (sceneManagerScript.getObjectSelected () == gameObject) {
//						thisFrameEvents.Add (touch);
//					}
//				}
//			}
//		}
//		if (this.multiTouches.Count == 0 && sceneManagerScript.getObjectSelected () == gameObject) {
//			sceneManagerScript.clearObjectSelected ();
//			touched = false;
//			this.zTranslationBegin = false;
//			this.zFingerID = -1;
//		}
		}
	}
	
	public void distributeTouches ()
	{
		// how many touches do we have? 
//		if (thisFrameEvents.Count == 1 && this.multiTouches.Count == 1) {
//			handleSingleTouch (thisFrameEvents [0] as iPhoneTouch);
//			return;
//		}
		switch (moveMode) {
		case MoveMode.XYTranslate:
			handleSingleTouch (this.multiTouches [0] as iPhoneTouch);
			break;
		case MoveMode.ZTranslate:
			iPhoneTouch anotherTouch = new iPhoneTouch ();
			foreach (iPhoneTouch touch in this.multiTouches) {
				if (touch.fingerId == zFingerID)
					anotherTouch = touch;
			}
			handleSingleComplexTouch (anotherTouch);
			break;
		case MoveMode.Pinch:
			int b = 1;
			break;
		default:
			break;
			
		}
//		if (touched == true && this.multiTouches.Count == 1) {
//			handleSingleTouch (thisFrameEvents [0] as iPhoneTouch);
//			return;	
//		}
//	
//		if (this.zTranslationBegin == false) {
//			if (thisFrameEvents.Count == 1 && this.multiTouches.Count == 2) {
//				iPhoneTouch zTranslationTouch = new iPhoneTouch ();
//				foreach (iPhoneTouch touch in this.multiTouches) {
//					if (touch != thisFrameEvents [0])
//						zTranslationTouch = touch;
//					this.zFingerID = touch.fingerId;
//				}
//				handleSingleComplexTouch (zTranslationTouch);	
//				this.zTranslationBegin = true;
//			}
//		} else {
//			if (this.multiTouches.Count != 0) {
//				foreach (iPhoneTouch touch in this.multiTouches) {
//					if (touch.fingerId == this.zFingerID)
//						handleSingleComplexTouch (touch);
//				}
//			}
//			
//		}
//		
//		if (thisFrameEvents.Count == 2) {
//			handleDoubleTouch (thisFrameEvents);
//			return;
//		}
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
	
	void checkMoveMode ()
	{
		int touchCount = iPhoneInput.touchCount;
		int contactCount = this.thisFrameEvents.Count;
		if (touchCount == 0)
			moveMode = MoveMode.None;
		if (touchCount == 1)
			moveMode = MoveMode.XYTranslate;
		if (touchCount == 2 && contactCount == 1) {
			moveMode = MoveMode.ZTranslate;
			foreach (iPhoneTouch touch in iPhoneInput.touches) {
				if (touch != thisFrameEvents [0])
					zFingerID = touch.fingerId;
			}
		}
			
		if (touchCount == 2 && contactCount == 2)
			moveMode = MoveMode.Pinch;
	}
	
	public bool getSpaceMove ()
	{
		return spaceMove;
	}
	
	public void setSpaceMove (bool condition)
	{
		spaceMove = condition;
	}
}

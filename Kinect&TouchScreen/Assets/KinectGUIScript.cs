using UnityEngine;
using System.Collections;

public class KinectGUIScript : MonoBehaviour
{
	

	GameObject objectPicked;
	bool pickToggle = false;
	public bool spaceMovement = false;

	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	void OnGUI ()
	{
		Camera userCamera = GameObject.Find ("UserCamera").camera;
		KinectSkeleton kinectSkeleton = GameObject.Find ("User").GetComponent<KinectSkeleton> ();
		
		
		
		string buttonText = "Pick an item";
		pickToggle = GUI.Toggle (new Rect (0, 0, 200, 100), pickToggle, buttonText, "button");
		spaceMovement = GUI.Toggle (new Rect (200, 0, 200, 100), spaceMovement, "spaceMovement", "button");
		
		if (pickToggle == true) {
			if (objectPicked != null)
				return;
			Ray ray = userCamera.ViewportPointToRay (kinectSkeleton.getRHViewPosition ());
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit)) {
				objectPicked = hit.collider.gameObject;
				objectPicked.transform.parent = GameObject.Find ("RightHand").transform;
			}
			buttonText = "Picked";
		}
		if (pickToggle == false) {
			if (objectPicked != null)
				objectPicked.transform.parent = null;
			objectPicked = null;
			buttonText = "Pick an item";
		}
		MultiTouchManipulation multiTouchScript = new MultiTouchManipulation ();
		SceneManager sceneManager = GameObject.Find ("SceneManager").GetComponent<SceneManager> ();
		if (sceneManager.getObjectSelected () != null) {
			GameObject objectSelected = sceneManager.getObjectSelected ();
			multiTouchScript = objectSelected.GetComponent<MultiTouchManipulation> ();
				
		}
		
		if (spaceMovement == true) {
			multiTouchScript.setSpaceMove (true);		
		} else {
			multiTouchScript.setSpaceMove (false);		
		}
		
	}
}

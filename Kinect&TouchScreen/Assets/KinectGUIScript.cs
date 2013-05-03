using UnityEngine;
using System.Collections;

public class KinectGUIScript : MonoBehaviour
{
	public GameObject leftHandPrefab;
	public GameObject rightHandPrefab;
	bool leftHandTrack = false;
	bool rightHandTrack = false;
	// Use this for initialization
	GameObject rightHandInstance;
	GameObject leftHandInstance;
	GameObject objectPicked;
	bool pickToggle = false;
	public bool zTranActive = false;

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
		kinectSkeleton skeletonScript = GameObject.Find ("User").GetComponent<kinectSkeleton> ();
		if (leftHandTrack == false) {
			leftHandInstance = (GameObject)Instantiate (leftHandPrefab, userCamera.transform.position, userCamera.transform.rotation);
			leftHandTrack = true;
		}
		if (rightHandTrack == false) {
			rightHandInstance = (GameObject)Instantiate (rightHandPrefab, userCamera.transform.position, userCamera.transform.rotation);
			rightHandTrack = true;
		}
		
		Vector3 leftScreenPos = new Vector3 (skeletonScript.leftHandViewport.x * userCamera.pixelWidth, skeletonScript.leftHandViewport.y * userCamera.pixelHeight, 0);
		leftHandInstance.transform.position = userCamera.ScreenToViewportPoint (leftScreenPos);
		
		Vector3 rightScreenPos = new Vector3 (skeletonScript.rightHandViewport.x * userCamera.pixelWidth, skeletonScript.rightHandViewport.y * userCamera.pixelHeight, 0);
		rightHandInstance.transform.position = userCamera.ScreenToViewportPoint (rightScreenPos);
		
		string buttonText = "Pick an item";
		pickToggle = GUI.Toggle (new Rect (0, 0, 200, 100), pickToggle, buttonText, "button");
		zTranActive = GUI.Toggle (new Rect (200, 0, 200, 1000), zTranActive, "zTranslation", "button");
		
		if (pickToggle == true) {
			if (objectPicked != null)
				return;
			Ray ray = userCamera.ViewportPointToRay (skeletonScript.leftHandViewport);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit)) {
				objectPicked = hit.collider.gameObject;
				objectPicked.transform.parent = GameObject.Find ("leftHand").transform;
			}
			buttonText = "Picked";
		}
		if (pickToggle == false) {
			if (objectPicked != null)
				objectPicked.transform.parent = null;
			objectPicked = null;
			buttonText = "Pick an item";
		}
		
		
		
	}
}

using UnityEngine;
using System.Collections;

public class touch : MonoBehaviour
{
	public GameObject crosshairPrefab;
	// public BBInputDelegate eventManager;
	// 
	private ArrayList crosshairs = new ArrayList ();
	private Camera renderingCamera;
	// Use this for initialization
	void Start ()
	{
		renderingCamera = Camera.main;
		if (renderingCamera == null) {
			// someone didnt tag their cameras properly!!
			// just grab the first one
			if (Camera.allCameras.Length == 0)
				return;
			renderingCamera = Camera.allCameras [0];
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		int crosshairIndex = 0;
		int i;
		for (i = 0; i < iPhoneInput.touchCount; i++) {
			if (crosshairs.Count <= crosshairIndex) {
				// make a new crosshair and cache it
				GameObject newCrosshair = (GameObject)Instantiate (crosshairPrefab, Vector3.zero, Quaternion.identity);
				crosshairs.Add (newCrosshair);
			}
			iPhoneTouch touch = iPhoneInput.GetTouch (i);
			Vector3 screenPosition = new Vector3 (touch.position.x, touch.position.y, 0.0f);
			GameObject thisCrosshair = (GameObject)crosshairs [crosshairIndex];
			thisCrosshair.SetActiveRecursively (true);
			thisCrosshair.transform.position = renderingCamera.ScreenToViewportPoint (screenPosition);
			
			GameObject screen=GameObject.Find("Screen");
			CreatePlane createPlane=screen.GetComponent<CreatePlane>();
			createPlane.displayTouch(new Vector2(touch.position.x,touch.position.y));
			crosshairIndex++;
		}
		
		// if there are any extra ones, then shut them off
		for (i = crosshairIndex; i < crosshairs.Count; i++) {
			GameObject thisCrosshair = (GameObject)crosshairs [i];
			thisCrosshair.SetActiveRecursively (false);			
		}
	}
}

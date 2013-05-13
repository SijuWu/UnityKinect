using UnityEngine;
using System.Collections;

public class MultiTouchManipulation : MultiTouchObject
{
	public bool allowDrag = true;
	public bool allowScale = true;
	public bool allowRotate = true;
	public bool allowSpace = true;
	public bool allowTranslationZ = true;
	public float minimumScale = 1.0f;
	public float maximumScale = 50.0f;
	private Transform saveParent;
	private Vector3 xyMovement;
	private Vector3 zMovement;
	float zMulitiplitation = 3;
	protected GameObject pivot;
	Vector3 lastTouch;

	public override void handleSingleTouch (iPhoneTouch touch)
	{
		if (!allowDrag)
			return;
		// // we want to drag our object
//		print (touch.position.x+" "+touch.deltaPosition.x);
		
//		xyMovement = touchMovementVector (touch);
		foreach (iPhoneTouch lastTouch in lastMultiTouches) {
			if (lastTouch.fingerId == touch.fingerId) {
				xyMovement = touchMovementVector (touch, lastTouch);
			}
		}
//		if (movement.sqrMagnitude > 0.01) {
		this.startPivot (gameObject.transform.position); 
		pivot.transform.Translate (xyMovement, Space.World);
		this.endPivot ();			
//		}
	}

	public override void handleSingleComplexTouch (iPhoneTouch zTouch)
	{
		if (!allowTranslationZ)
			return;
		bool lift = false;
		foreach (iPhoneTouch lastTouch in lastMultiTouches) {
			if (lastTouch.fingerId == zTouch.fingerId) {
				zMovement = touchMovementVector (zTouch, lastTouch);
				if (lastTouch.position.x < zTouch.position.x)
					lift = true;
				else
					lift = false;
			}
		}
		GameObject screenReference = GameObject.Find ("ScreenReference");
		CreatePlane createPlane = GameObject.Find ("Screen").GetComponent<CreatePlane> ();
		Vector3 zDirection = createPlane.getVec3 ();
		if (lift == true)
			zMovement = -zDirection * zMovement.magnitude * zMulitiplitation;
		else
			zMovement = zDirection * zMovement.magnitude * zMulitiplitation;
		this.startPivot (gameObject.transform.position);
		pivot.transform.Translate (zMovement, Space.World);
		this.endPivot ();
	}
	
	public override void handleDoubleTouch (ArrayList events)
	{
		if (!allowRotate && !allowScale)
			return;
		// double touch can be a scale or a rotate, or both
		// 
		// let's do the rotate first
		// since this is a 2 touch gesture, we can only rotate in 2d, which in this case is in the camera plane
		// pivot on the lower touch index

		iPhoneTouch touch0 = (iPhoneTouch)events [0];
		iPhoneTouch touch1 = (iPhoneTouch)events [1];
		if (touch0.fingerId > touch1.fingerId) {
			// flip them, 0 should be the earlier index
			touch0 = (iPhoneTouch)events [1];
			touch1 = (iPhoneTouch)events [0];
		}

		this.startPivot (gameObject.transform.position); 

		// 	
		// 	//////////////////////////////// ROTATE
		// 	
		
		float zDistanceFromCamera = Vector3.Distance (renderingCamera.transform.position, gameObject.transform.position);

		Vector3 screenPosition0 = new Vector3 (touch0.position.x, touch0.position.y, zDistanceFromCamera);
		Vector3 lastScreenPosition0 = new Vector3 (touch0.position.x - touch0.deltaPosition.x, touch0.position.y - touch0.deltaPosition.y, zDistanceFromCamera);

		Vector3 screenPosition1 = new Vector3 (touch1.position.x, touch1.position.y, zDistanceFromCamera);
		Vector3 lastScreenPosition1 = new Vector3 (touch1.position.x - touch1.deltaPosition.x, touch1.position.y - touch1.deltaPosition.y, zDistanceFromCamera);


		float angleNow = Mathf.Atan2 (screenPosition0.x - screenPosition1.x, screenPosition0.y - screenPosition1.y) * Mathf.Rad2Deg;
		float angleThen = Mathf.Atan2 (lastScreenPosition0.x - lastScreenPosition1.x, lastScreenPosition0.y - lastScreenPosition1.y) * Mathf.Rad2Deg;
		
		float angleDelta = angleNow - angleThen;

		if (allowRotate)
			pivot.transform.RotateAround (gameObject.transform.position, renderingCamera.transform.position - gameObject.transform.position, angleDelta);

		// 
		// 	///////////////////////////  SCALE
		// 
		if (allowScale) {
			float distNow = (screenPosition0 - screenPosition1).magnitude;
			float distThen = (lastScreenPosition0 - lastScreenPosition1).magnitude;
		 
			float scale = distNow / distThen;
		
			// presume for the time being that our scales are uniform
			if (transform.localScale.x * scale < minimumScale)
				scale = minimumScale / transform.localScale.x;
			if (transform.localScale.x * scale > maximumScale)
				scale = maximumScale / transform.localScale.x;
	
			Vector3 local = pivot.transform.localScale;
			
			local.x *= scale;
			local.y *= scale;
			local.z *= scale;
			pivot.transform.localScale = local;
		}

		this.endPivot ();
	}
	
	virtual protected void startPivot (Vector3 pivotPosition)
	{			
		if (pivot == null) {
			pivot = new GameObject ();
			pivot.name = "BBBasicTouchManipulation Pivot";
			pivot.transform.position = pivotPosition;		
		}	

		saveParent = gameObject.transform.parent;
		gameObject.transform.parent = null;
		pivot.transform.parent = saveParent;
		gameObject.transform.parent = pivot.transform;
	}

	virtual protected void endPivot ()
	{
		gameObject.transform.parent = saveParent;		
		pivot.transform.parent = null;	
		Destroy (pivot);	
	}
	
	public Vector3 touchMovementVector (iPhoneTouch touch)
	{
		float zDistanceFromCamera = Vector3.Distance (renderingCamera.transform.position, gameObject.transform.position);

		Vector3 screenPosition = new Vector3 (touch.position.x, touch.position.y, zDistanceFromCamera);
		Vector3 lastScreenPosition = new Vector3 (touch.position.x - touch.deltaPosition.x, touch.position.y - touch.deltaPosition.y, zDistanceFromCamera);

		Vector3 cameraWorldPosition = this.renderingCamera.ScreenToWorldPoint (screenPosition);
		Vector3 lastCameraWorldPosition = this.renderingCamera.ScreenToWorldPoint (lastScreenPosition);

		return cameraWorldPosition - lastCameraWorldPosition;
	}
	
	public Vector3 touchMovementVector (iPhoneTouch touch, iPhoneTouch lastTouch)
	{
		float zDistanceFromCamera = Vector3.Distance (renderingCamera.transform.position, gameObject.transform.position);

		Vector3 screenPosition = new Vector3 (touch.position.x, touch.position.y, zDistanceFromCamera);
		Vector3 lastScreenPosition = new Vector3 (lastTouch.position.x, lastTouch.position.y, zDistanceFromCamera);

		Vector3 cameraWorldPosition = this.renderingCamera.ScreenToWorldPoint (screenPosition);
		Vector3 lastCameraWorldPosition = this.renderingCamera.ScreenToWorldPoint (lastScreenPosition);

		return cameraWorldPosition - lastCameraWorldPosition;
	}

}

using UnityEngine;
using System.Collections;

public class CreatePlane : MonoBehaviour
{
	//The width of the screen mesh
	float screenWidth;
	//The height of the screen mesh
	float screenHeight;
	//The corners of the screen mesh
	Vector3[] screenCornerCoordinates = new Vector3[4];
	//The corners in the local reference
	Vector3[] screenRef = new Vector3[4];
	//The center of the screen
	Vector3 screenCenter ;
	//Normalized vector in the direction of the height
	Vector3 vec1 ;
	//Normalized vector in the direction of the width
	Vector3 vec2 ;
	//The third normalized vector
	Vector3 vec3 ;
	
	void initializePlane ()
	{
		gameObject.AddComponent ("MeshFilter");
		gameObject.AddComponent ("MeshRenderer");
		
		//Set the corners of the plane
		Mesh mesh = new Mesh ();
		GetComponent<MeshFilter> ().mesh = mesh;
		screenCornerCoordinates [0] = new Vector3 (492.7f, -446.8f, 1268.0f);
		screenCornerCoordinates [1] = new Vector3 (-417.3f, -447.9f, 1336.0f);
		screenCornerCoordinates [2] = new Vector3 (-389.1f, -320.9f, 1792.0f);
		screenCornerCoordinates [3] = screenCornerCoordinates [2] + screenCornerCoordinates [0] - screenCornerCoordinates [1];
		
		//Add the corners to the mesh
		mesh.vertices = new Vector3[] {screenCornerCoordinates [0], screenCornerCoordinates [1], screenCornerCoordinates [2],screenCornerCoordinates [3]};
		//Set the triangle order
		mesh.triangles = new int[] {0, 1, 2, 0, 2, 3};
		
		//Set the mesh of the meshFilter
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter> ();
		meshFilter.mesh = mesh;
		
		//Set the mesh of the meshCollider
		MeshCollider meshCollider = gameObject.GetComponent<MeshCollider> ();
		meshCollider.sharedMesh = mesh;
	}
	
	void initializeEnvironment ()
	{
		GameObject sphere = GameObject.Find ("Sphere");
		sphere.transform.position = screenCenter;
		
		GameObject cube1 = GameObject.Find ("Cube1");
		cube1.transform.position = screenCenter - vec2 * (screenWidth * 0.2F);
		
		GameObject cube2 = GameObject.Find ("Cube2");
		cube2.transform.position = screenCenter + vec2 * (screenWidth * 0.3F);
	}
	
	void transformScreenPlane ()
	{
		//The heightVector and the widthVector
		Vector3 heightVector = screenCornerCoordinates [3] - screenCornerCoordinates [0];
		Vector3 widthVector = screenCornerCoordinates [1] - screenCornerCoordinates [0];
		
		screenWidth = widthVector.magnitude;
		screenHeight = heightVector.magnitude;
		
		//Three normalized axis of the screen plane, the original point is the top-left corner
		vec1 = heightVector.normalized;
		vec2 = widthVector.normalized;
		vec3 = Vector3.Cross (vec1, vec2);
		
		//Set the screenReference
		GameObject screenReference = GameObject.Find ("ScreenReference");
		screenReference.transform.position = screenCornerCoordinates [0];
		screenReference.transform.LookAt (screenCornerCoordinates [1], vec3);
		
		//Calculate the coordinates of four corners in the screenReference
		screenRef [0] = screenReference.transform.InverseTransformPoint (screenCornerCoordinates [0]);
		screenRef [1] = screenReference.transform.InverseTransformPoint (screenCornerCoordinates [1]);
		screenRef [2] = screenReference.transform.InverseTransformPoint (screenCornerCoordinates [2]);
		screenRef [3] = screenReference.transform.InverseTransformPoint (screenCornerCoordinates [3]);
	}
	
	void setCameraAndLight ()
	{
		//Set the kinectCamera
		Camera kinectCamera = GameObject.Find ("KinectCamera").camera;
		kinectCamera.transform.position = new Vector3 (0, 0, 0);
		kinectCamera.transform.LookAt (new Vector3 (0, 0, 1));
		kinectCamera.far = 2000;
		
		//Set the userCamera
		Camera userCamera = GameObject.Find ("UserCamera").camera;
		screenCenter = screenCornerCoordinates [0] + vec1 * screenHeight * 0.5F + vec2 * screenWidth * 0.5F;
		userCamera.transform.position = screenCenter - 1500.0F * vec3;
		userCamera.transform.LookAt (screenCenter, -vec1);
		
		
		//Set the light
		GameObject light = GameObject.Find ("Directional light");
		light.transform.position = userCamera.transform.position;
		light.transform.LookAt (screenCenter, -vec1);
	}
	
	// Use this for initialization
	void Start ()
	{
		//Initialize the screen mesh
		initializePlane ();
		//Get the coordinates of the mesh in the local reference
		transformScreenPlane ();
		//Set the cameras and the light in the environment
		setCameraAndLight ();
		//Initialize objects in the environment
		initializeEnvironment ();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	public void setPlane (Vector3[] screenCorners)
	{
		gameObject.AddComponent ("MeshFilter");
		gameObject.AddComponent ("MeshRenderer");
		Mesh mesh = new Mesh ();
		GetComponent<MeshFilter> ().mesh = mesh;
		
		screenCornerCoordinates [0] = screenCorners [0];
		screenCornerCoordinates [1] = screenCorners [1];
		screenCornerCoordinates [2] = screenCorners [2];
		screenCornerCoordinates [3] = screenCorners [2] + screenCorners [0] - screenCorners [1];

		mesh.vertices = new Vector3[] {screenCornerCoordinates [0], screenCornerCoordinates [1], screenCornerCoordinates [2],screenCornerCoordinates [3]};
		mesh.triangles = new int[] {0, 1, 2, 0, 2, 3};
		Camera.main.transform.position = new Vector3 (0, 0, 0);
		Camera.main.transform.LookAt (new Vector3 (0, 0, 1));
		Camera.main.far = 2000;

	}
	
	//Display a cube to represent to touch position on the screen
	public void displayTouch (Vector2 touchPosition)
	{
		Vector3 touchPos = new Vector3 ();
		
		//Get the corners of the screen
		Vector2[] planeScreen = new Vector2[4];
		planeScreen [0] = new Vector2 (0, Camera.main.pixelHeight);
		planeScreen [1] = new Vector2 (Camera.main.pixelWidth, Camera.main.pixelHeight);
		planeScreen [2] = new Vector2 (Camera.main.pixelWidth, 0);
		planeScreen [3] = new Vector2 (0, 0);
		
		//Get the relative position of the touch point
		float t1 = touchPosition.x / (planeScreen [2].x - planeScreen [3].x);
		float t2 = touchPosition.y / (planeScreen [0].y - planeScreen [3].y);
		
		//Get the position of touch on the virtual screen
		Vector3 v1 = screenCornerCoordinates [2] - screenCornerCoordinates [3];
		Vector3 v2 = screenCornerCoordinates [0] - screenCornerCoordinates [3];
		touchPos = t1 * v1 + t2 * v2 + screenCornerCoordinates [3];

		GameObject cube = GameObject.Find ("Cube");
		cube.transform.position = touchPos;
	}
	
	public double getScreenWidth ()
	{
		return screenWidth;
	}
	
	public double getScreenHeight ()
	{
		return screenHeight;
	}
	
	public Vector3[] getScreenRef ()
	{
		return screenRef;
	}
	
	public Vector2 getScreenCenter ()
	{
		return screenCenter;
	}
	
	public Vector3 getStartPoint ()
	{
		return screenRef [0];
	}
}


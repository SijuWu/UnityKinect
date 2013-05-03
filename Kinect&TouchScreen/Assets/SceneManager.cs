using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour
{
	
	private GameObject objectSelected;
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		
		
	}
	
	public void setObjectSelected (GameObject objectSelected)
	{
		this.objectSelected = objectSelected;
	}
	
	public GameObject getObjectSelected ()
	{
		return this.objectSelected;
	}

	public void clearObjectSelected ()
	{
		this.objectSelected = null;
	}
}

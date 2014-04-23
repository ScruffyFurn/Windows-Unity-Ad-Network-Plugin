using UnityEngine;
using System.Collections;

public class SceneChanger : MonoBehaviour {

	/*<Summary>
	 * A simple script that switches from the first scene to the second
	 * Meant to test the ad destruction
	 </Summary>*/

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI()
	{
		if (GUI.Button (new Rect(Screen.width * 0.25f,Screen.height *0.25f,100f,100f),"Next Scene")) 
		{
			if(Application.loadedLevel == 0)
				Application.LoadLevel(1);
			else
				Application.LoadLevel(0);
		}
	}
}

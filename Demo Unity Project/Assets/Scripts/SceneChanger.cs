/*Copyright (c) 2014 [Michael MacDonald]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
*/
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

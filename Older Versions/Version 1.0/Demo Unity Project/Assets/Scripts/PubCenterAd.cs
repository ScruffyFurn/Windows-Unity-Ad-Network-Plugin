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
using System.Collections.Generic;

#if UNITY_WP8 || UNITY_METRO || UNITY_METRO_8_1
using Windows_Ad_Plugin;
#endif

public class PubCenterAd : MonoBehaviour {
	
	#if UNITY_WP8 || UNITY_METRO || UNITY_METRO_8_1
	//PubCenter Ad
	public string AppId = "test_client";
	public string UnitId = "Image480_80";
	public float winWidth = 480;
	public float winHeight = 80;
	public bool AutoRefreshAd = true;
	
	//General Settings
	public bool useAdFiller = true;
	public Windows_Ad_Plugin.Helper.HORIZONTAL_ALIGNMENT horizontalAlignment = 
		Windows_Ad_Plugin.Helper.HORIZONTAL_ALIGNMENT.CENTER;
	public Windows_Ad_Plugin.Helper.VERTICAL_ALIGNMENT verticalAlignment = 
		Windows_Ad_Plugin.Helper.VERTICAL_ALIGNMENT.CENTER;
	
	public bool printDebug = true;
	
	//These are constants to deal with the different alignments
	//These are screen percentages for the ad filler position
	private const int H_CENTER = 50;
	private const int H_LEFT = 0;
	private const int H_RIGHT = 100;
	
	private const int V_BOTTOM = 100;
	private const int V_TOP = 0;
	private const int V_CENTER = 50;
	
	//Variables to store the ad filler position
	private float fillX;
	private float fillY;
	
	[System.Serializable]
	public class AdFiller
	{
		public string url="";
		public Texture2D image;
	}
	
	public List<AdFiller> AdFillers;
	
	private int curAd = 0;
	private bool showAdFiller = false;
	private GUISkin skin;
	private string error;
	
	
	void Awake()
	{
		#if UNITY_METRO_8_1 || UNITY_METRO
		UnityEngine.WSA.Application.windowSizeChanged += WindowSizeChange;
		#endif
	}
	
	#if UNITY_METRO_8_1 || UNITY_METRO
	/*<Summary>
	 * Resets the adfiller position if the window size is changed
	 * </Summary>*/
	private void WindowSizeChange (int width, int height)
	{
		SetAdFillerPosition ();
	}
	#endif
	
	
	// Use this for initialization
	void Start () 
	{
		if (Windows_Ad_Plugin.Helper.Instance.HasGrid ())
		{
			CreateAd();
		}
		
		CreateSkin ();
		
		//Set the intial adfiller position
		SetAdFillerPosition ();
	}
	
	/*<Summary>
	 * Wahoo for an update
	 * The helper plugin sends up its current error
	 * If the ad is not built then the script tries to make it again
	 <Summary>*/
	void Update () 
	{
		error = Windows_Ad_Plugin.Helper.Instance.GetErrorMesssage ();
		//if(error !="" && printDebug)
		Debug.Log ("Error:" + error);
		
		if (!Windows_Ad_Plugin.Helper.Instance.IsBuilt() && Windows_Ad_Plugin.Helper.Instance.HasGrid ()) 
		{
			CreateAd();
		}
		UpdateAdFiller ();
		
		#if UNITY_EDITOR
		SetAdFillerPosition();
		#endif
	}
	
	/*<Summary>
	 * Checks to see if its possible to show an adfiller
	 * Then check if an adfiller is supposed to be shown at all
	 * If there is an ad and adfiller's are supposed to be shown, they are turned off
	 * Vice versa, if there is no ad and no ad is shown. We show the next ad in the list
	 * </Summary>*/
	private void UpdateAdFiller()
	{
		if (AdFillers != null && AdFillers.Count > 0 && useAdFiller) 
		{
			if (Windows_Ad_Plugin.Helper.Instance.IsThereAnAd () ) 
			{
				showAdFiller = false;	
			}
			else if (!Windows_Ad_Plugin.Helper.Instance.IsThereAnAd () && !showAdFiller) 
			{
				showAdFiller = true;
				curAd++;
				if (curAd >= AdFillers.Count) 
					curAd = 0;
			}
		}
	}
	
	
	/*<Summary>
	 * Sets the adfiller position based on the horizontal and vertical alignment variables
	 * </Summary>*/
	private void SetAdFillerPosition()
	{
		//Set the AdFIllers x value
		switch (horizontalAlignment)
		{
		case Windows_Ad_Plugin.Helper.HORIZONTAL_ALIGNMENT.CENTER:
			fillX = H_CENTER;
			break;
		case Windows_Ad_Plugin.Helper.HORIZONTAL_ALIGNMENT.LEFT:
			fillX = H_LEFT;
			break;
		case Windows_Ad_Plugin.Helper.HORIZONTAL_ALIGNMENT.RIGHT:
			fillX = H_RIGHT;
			break;
		default:
			fillX = H_CENTER;
			break;
		}
		//Set the Adfillers y value
		switch (verticalAlignment)
		{
		case Windows_Ad_Plugin.Helper.VERTICAL_ALIGNMENT.CENTER:
			fillY = V_CENTER;
			break;
		case Windows_Ad_Plugin.Helper.VERTICAL_ALIGNMENT.BOTTOM:
			fillY = V_BOTTOM;
			break;
		case Windows_Ad_Plugin.Helper.VERTICAL_ALIGNMENT.TOP:
			fillY = V_TOP;
			break;
		default:
			fillY = V_CENTER;
			break;
		}
		
		//Now we apply the changes
		fillX = (Screen.width * fillX * 0.01f) - (winWidth * 0.5f);
		fillY = (Screen.height * fillY * 0.01f);
		//Have to make one small alteration when its at the bottom though
		if (verticalAlignment == Windows_Ad_Plugin.Helper.VERTICAL_ALIGNMENT.BOTTOM)
			fillY -= winHeight;
		//Make this correction as well so the ad filler stays on screen
		if (horizontalAlignment == Windows_Ad_Plugin.Helper.HORIZONTAL_ALIGNMENT.LEFT)
			fillX += winWidth * 0.5f;
		else if (horizontalAlignment == Windows_Ad_Plugin.Helper.HORIZONTAL_ALIGNMENT.RIGHT)
			fillX -= winWidth * 0.5f;
	}
	
	
	/*<Summary>
	 * Creates a skin that wipes out the default button settings
	 * </Summary>*/
	private void CreateSkin()
	{
		//Just makes a new guiskin that wipes out the textures on the button
		skin = new GUISkin ();
		skin.button.hover.background = null;
		skin.button.normal.background = null;
		skin.button.onActive.background = null;
		skin.button.onNormal.background = null;
		skin.button.onFocused.background = null;
		skin.button.onActive.background = null;
		skin.button.active.background = null;
	}
	
	
	/*<Summary>
	 * Function calls the Create Ad function in the plugin
	 * Based on what information is passed in, you will get an admob or windows ad
	 * Thats if you were to call it but in this case the code is separated into to scripts
	 <Summary>*/
	public void CreateAd()
	{
		
		Windows_Ad_Plugin.Helper.Instance.CreateAd(
			AppId,
			UnitId,
			(double)winHeight,
			(double)winWidth,
			AutoRefreshAd,
			horizontalAlignment,
			verticalAlignment
			);
	}
	
	/*<Summary>
	 * The OnGUI function is used to show the adFiller
	 * And handle the adFiller's click event (since its just a button
	 <Summary>*/
	void OnGUI()
	{
		GUI.skin = skin;
		if (showAdFiller && useAdFiller) 
		{
			GUI.skin = skin;
			GUI.DrawTexture(new Rect ( fillX,fillY, winWidth , winHeight),AdFillers [curAd].image,ScaleMode.StretchToFill);
			if (GUI.Button (new Rect (fillX,fillY, winWidth , winHeight),"")) {
				#if !UNITY_EDITOR
				if(AdFillers[curAd].url != null || AdFillers[curAd].url != "")
					Windows_Ad_Plugin.Helper.Instance.OpenWebPage(AdFillers[curAd].url);
				#else
				if (AdFillers [curAd].url != null || AdFillers [curAd].url != "")
					Application.OpenURL (AdFillers [curAd].url);
				#endif
				
			}
		}
	}
	
	/*<Summary>
	 * When the script is destroyed, the destruction handler within the windows helper plugin is called
	 * </Summary>*/
	void OnDestroy()
	{
		Windows_Ad_Plugin.Helper.Instance.HandleDestruction ();
	}
	#endif
}

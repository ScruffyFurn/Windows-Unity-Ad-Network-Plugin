using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_WP8 || UNITY_METRO || UNITY_METRO_8_1
using WindowsHelperPlugin;
#endif

public class WindowsAd : MonoBehaviour {

#if UNITY_WP8 || UNITY_METRO || UNITY_METRO_8_1
	//Windows Ad
	public string AppId = "test_client";
	public string UnitId = "Image480_80";
	public float winWidth = 480;
	public float winHeight = 80;
	public bool AutoRefreshAd = true;

	//General Settings
	public bool useAdFiller = true;
	public float positionX = 0;
	public float positionY = 0;

	public bool printDebug = true;
	
	//Cached ad location for the gui
	private Vector3 adLocation;
	
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
	
	// Use this for initialization
	void Start () 
	{
		if (WindowsHelperPlugin.Helper.Instance.HasGrid ())
		{
			CreateAd();
		}
		
		CreateSkin ();
	}
	
	/*<Summary>
	 * Wahoo for an update
	 * The helper plugin sends up its current error
	 * If the ad is not built then the script tries to make it again
	 <Summary>*/
	void Update () 
	{
		error = WindowsHelperPlugin.Helper.Instance.GetErrorMesssage ();
		if(error !="" && printDebug)
			Debug.Log ("Error:" + error);
		
		if (!WindowsHelperPlugin.Helper.Instance.IsBuilt() && WindowsHelperPlugin.Helper.Instance.HasGrid ()) 
		{
			CreateAd();
		}
		UpdateAdFiller ();
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
			if (WindowsHelperPlugin.Helper.Instance.IsThereAnAd () ) 
			{
				showAdFiller = false;	
			}
			else if (!WindowsHelperPlugin.Helper.Instance.IsThereAnAd () && !showAdFiller) 
			{
				showAdFiller = true;
				curAd++;
				if (curAd >= AdFillers.Count) 
					curAd = 0;
			}
		}
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
	 * Thats if you were to call it but in this case all you need to do is set the adType to 
	 * correct type
	 * Easy as pie
	 <Summary>*/
	public void CreateAd()
	{

		adLocation = new Vector3 ( (Screen.width * positionX * 0.01f),(Screen.height * positionY * 0.01f) + (winHeight) ,1f);

		WindowsHelperPlugin.Helper.Instance.CreateAd(
			AppId,
			UnitId,
			(double)winHeight,
			(double)winWidth,
			AutoRefreshAd,
			(double)adLocation.x,
			(double)adLocation.y
			);
	}
	
	/*<Summary>
	 * The OnGUI function is used to show the adFiller
	 * And handle the adFiller's click event (since its just a button
	 * The origin in windows apps is in the center of the screen
	 * So we need to do a conversion
	 <Summary>*/
	void OnGUI()
	{
		GUI.skin = skin;
		if (showAdFiller && useAdFiller) 
		{
			if (GUI.Button (new Rect ((Screen.width * positionX * 0.01f) + (Screen.width * 0.5f) - (winWidth * 0.5f), (Screen.height * positionY * 0.01f) + (Screen.height * 0.5f), winWidth, winHeight), AdFillers [curAd].image)) 
			{
				#if !UNITY_EDITOR
if(AdFillers[curAd].url != null || AdFillers[curAd].url != "")
WindowsHelperPlugin.Helper.Instance.OpenWebPage(AdFillers[curAd].url);
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
		WindowsHelperPlugin.Helper.Instance.HandleDestruction ();
	}
#endif
}

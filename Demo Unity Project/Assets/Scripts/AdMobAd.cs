using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdMobAd : MonoBehaviour {

#if UNITY_WP8
	//AdMob
	public string adMobId = "MY_UNIT_ID";
	public float mobWidth = 320f;
	public float mobHeight = 50f;
	public bool testAdMob = true;

	//General Settings
	public bool useAdFiller = true;
	public Windows_Ad_Plugin.Helper.HORIZONTAL_ALIGNMENT horizontalAlignment = 
		Windows_Ad_Plugin.Helper.HORIZONTAL_ALIGNMENT.CENTER;
	public Windows_Ad_Plugin.Helper.VERTICAL_ALIGNMENT verticalAlignment = 
		Windows_Ad_Plugin.Helper.VERTICAL_ALIGNMENT.CENTER;

	
	public bool printDebug = true;

	public Windows_Ad_Plugin.Helper.AD_FORMATS Format = Windows_Ad_Plugin.Helper.AD_FORMATS.BANNER;

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
		public string url ="";
		public Texture2D image;
	}
	
	public List<AdFiller> AdFillers;
	
	private int curAd = 0;
	private bool showAdFiller = false;
	private GUISkin skin;
	private string error;

	// Use this for initialization
	void Start () {
		if (Windows_Ad_Plugin.Helper.Instance.HasGrid ())
		{
			CreateAd();
		}
		
		CreateSkin ();
	
		//Set the adfiller position right away
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
		if(error !="" && printDebug)
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
	 * Function calls the Create Ad function in the plugin
	 * Based on what information is passed in, you will get an admob or windows ad
	 * Thats if you were to call it but in this case the code is separated into to scripts
	 <Summary>*/
	public void CreateAd()
	{
		Windows_Ad_Plugin.Helper.Instance.CreateAd(
			adMobId,
			Format,
			horizontalAlignment,
			verticalAlignment,
			(double)mobWidth,
			(double)mobHeight,
			testAdMob);
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
		fillX = (Screen.width * fillX * 0.01f) - (mobWidth * 0.5f);
		fillY = (Screen.height * fillY * 0.01f);
		//Have to make one small alteration when its at the bottom though
		if (verticalAlignment == Windows_Ad_Plugin.Helper.VERTICAL_ALIGNMENT.BOTTOM)
			fillY -= mobHeight;
		//Make this correction as well so the ad filler stays on screen
		if (horizontalAlignment == Windows_Ad_Plugin.Helper.HORIZONTAL_ALIGNMENT.LEFT)
			fillX += mobWidth * 0.5f;
		else if (horizontalAlignment == Windows_Ad_Plugin.Helper.HORIZONTAL_ALIGNMENT.RIGHT)
			fillX -= mobWidth * 0.5f;
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
	 * The OnGUI function is used to show the adFiller
	 * And handle the adFiller's click event (since its just a button
	 <Summary>*/
	void OnGUI()
	{
		GUI.skin = skin;
		if (showAdFiller && useAdFiller) 
		{
			GUI.skin = skin;
			GUI.DrawTexture(new Rect ( fillX,fillY, mobWidth , mobHeight),AdFillers [curAd].image,ScaleMode.StretchToFill);
			if (GUI.Button (new Rect (fillX,fillY, mobWidth , mobHeight),"")) {
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
#endif
}

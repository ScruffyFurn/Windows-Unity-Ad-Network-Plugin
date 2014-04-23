How To Use:

**********************************************UNITY*********************************************************
Import the package into your Unity project.
Add either the AdMobAd or WindowsAd prefab into your scene.
Once you have added them into your scene you must enter your correct credentials (AdUnitID, ApplicationID etc), the width/height of the ad and in the case of AdMob the format (Banner, or SmartBanner) 
See AdMobs documentation here: https://developers.google.com/mobile-ads-sdk/docs/admob/fundamentals

You position your ad via the positionX and positionY variables. These values are screen percentages, that are in the coordinate system of a WP8/Windows Store app.
Meaning they run from -50 to 50.

Testing
For Windows Ads you can leave the base values and you will get test ads.
Actual values will not work in the emulator, but will work on a device
For AdMob, you will need to put values in. If you check the testing bool it will do an adRequest (AdMob explanation https://developers.google.com/mobile-ads-sdk/docs/admob/intermediate#wp)
Don't leave testing on when you release though.

AdFiller
The adFiller is for when no ad is available. An adFiller consists of an image and an url, allowing you to direct traffic to your site or another app/game.
Use this responsibly, and comply with all EULAs and the like with publishers.

Once everything is set up, hit build.

Errors will be printed out, you can turn it off and on by un-checking the printDebug.

*********************************************BUILD**********************************************************


********************************************Windows Phone 8*************************************************
First off,
Open your WMAppManifest.xml and add these capabilities

Required Capabilities (adMob)
ID_CAP_NETWORKING	Access to network services is required when requesting ads.
ID_CAP_WEBBROWSERCOMPONENT	Required since the AdView is a web browser.
ID_CAP_MEDIALIB_PLAYBACK	Provides access for currently playing media items.
ID_CAP_MEDIALIB_AUDIO	Provides read access to audio items in media library.

Required Capabilites (Windows)
ID_CAP_IDENTITY_USER
ID_CAP_MEDIALIB_PHOTO
ID_CAP_NETWORKING
ID_CAP_PHONEDIALER
ID_CAP_WEBBROWSERCOMPONENT

Next, go to your references, we need to make some changes

Windows Phone 8:
	Remove both of the Microsoft.Advertising dlls
	Bring in the Windows Phone Ad sdk for XAML
	Add a reference to the WindowsHelperPlugin in the assets/plugins/wp8 folder

Within the MainPage.xaml.cs

To hook up the Dispatcher class to handle invokes on the App or UI thread

First off add these two functions, that tie into the Unity App thread and the UI thread of the solution
	public void InvokeOnAppThread(Action callback)
        {
            UnityApp.BeginInvoke(() => callback());
        }

        public void InvokeOnUIThread(Action callback)
        {
            Dispatcher.BeginInvoke(() => callback());
        }
Then add these lines of code, within the UnityLoaded function to wire it all up
            WindowsHelperPlugin.Dispatcher.InvokeOnAppThread = InvokeOnAppThread;
            WindowsHelperPlugin.Dispatcher.InvokeOnUIThread = InvokeOnUIThread;

Almost done, next you need to pass in some sort of grid for the ad to be added to
It is highly recommended that you use the DrawingSurfaceBackground

Example:
WindowsHelperPlugin.Helper.Instance.SetGrid(DrawingSurfaceBackground);

And hit run
*******************************************Windows Store Apps*****************************************************
Coming soon
*******************************************************************************************************************


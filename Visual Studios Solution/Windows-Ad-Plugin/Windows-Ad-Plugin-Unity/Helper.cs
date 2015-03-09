using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if WINDOWS_APP || WINDOWS_PHONE_APP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
#endif

#if WINDOWS_APP
using Microsoft.Advertising.WinRT.UI;
#endif

#if WINDOWS_PHONE_APP || WINDOWS_PHONE
using GoogleAds;
using Microsoft.Advertising.Mobile.UI;
#endif

#if WINDOWS_PHONE
using System.Windows.Controls;
using System.Windows;
#endif

namespace Windows_Ad_Plugin
{
    public class Helper
    {
        #region Fields
        //Our own enum replacement for 
        //System.Windows.VerticalAlignment & System.Windows.HorizontalAlignment 
        //since we will not have access to those within Unity
        public enum VERTICAL_ALIGNMENT { BOTTOM, CENTER, TOP };
        public enum HORIZONTAL_ALIGNMENT { CENTER, LEFT, RIGHT };

        //Our own enum for Google's AdFormats
        public enum AD_FORMATS { BANNER, SMART_BANNER };

        private static Helper _instance;
        private bool _isAMAd = false;
        private bool _isPCAd = false;
        private static readonly object _sync = new object();

        #if WINDOWS_APP || WINDOWS_PHONE_APP || WINDOWS_PHONE
        //Grid object to add our ad onto
        private Grid _baseGrid = null;

        private AdControl _Ad = null;


        #endif

#if  WINDOWS_PHONE
        private AdView _AdMobAd = null;
#endif

        //Error message
        public delegate void AdMessage(string msg);

        public AdMessage Messenger;

        //OnError and OnRefreshed delegate
        public delegate void AdUpdate();

        public AdUpdate OnErrorDelegate;
        public AdUpdate OnRefreshedDelegate;

        #endregion

        #region Properties
        //Returns true if either ad is built
        public bool IsAdBuilt
        {
            get { return (_isAMAd || _isPCAd); }
        }
        //Returns true if the admob ad is built
        public bool IsAdMobAdBuilt
        {
            get { return _isAMAd; }
        }
        //Returns true if the pubcenter ad is built
        public bool IsPubCenterAdBuilt
        {
            get { return _isPCAd; }
        }


#if WINDOWS_APP || WINDOWS_PHONE_APP || WINDOWS_PHONE
        public Grid BaseGrid
        {
            get
            {
                return _baseGrid;
            }

            set
            {
                _baseGrid = value;
            }
        }
#endif

        public bool HasGrid
        {
            get
            {
#if WINDOWS_APP || WINDOWS_PHONE_APP
                if (_baseGrid != null)
                    return true;
                else
                    return false;
#else
                return false;
#endif
            }
        }

        #endregion

        #region Initialization
        public static Helper Instance
        {
            get
            {
                lock (_sync)
                {
                    if (_instance == null)
                        _instance = new Helper();

                    //Set up the messenger
                    _instance.Messenger += Helper._instance.ShowMessage;
                }
                return _instance;
            }
        }
        #endregion

        #region Ad Creation

        #region PubCenter
        /// <summary>
        /// Used to create a pubcenter ad
        /// Pass in your apId, unitId, height/width, position and whether its a test ad or not 
        /// The testAd bool is important, it allows us to test an admob ad
        /// </summary>
        public void CreateAd(string apID, string unitId, double height, double width, bool autoRefresh, HORIZONTAL_ALIGNMENT hAlign, VERTICAL_ALIGNMENT vAlign)
        {
#if WINDOWS_APP || WINDOWS_PHONE_APP || WINDOWS_PHONE

            if(Dispatcher.InvokeOnUIThread == null)
                return;

            Dispatcher.InvokeOnUIThread(() =>
            {
                BuildAd(apID, unitId, height, width, autoRefresh, hAlign, vAlign);
            });
#endif

        }

        /// <summary>
        /// Actually goes and does the hard work of building the Pubcenter Ad
        /// </summary>
        private void BuildAd(string apID, string unitId, double height, double width, bool autoRefresh, HORIZONTAL_ALIGNMENT hAlign, VERTICAL_ALIGNMENT vAlign)
        {
#if WINDOWS_PHONE_APP || WINDOWS_APP || WINDOWS_PHONE
            //if the ad already exists we return
            if (_Ad != null)
            {
                if (Messenger != null)
                    Messenger("PubCenter Ad already created");
                return;
            }

            if(BaseGrid == null)
            {
                _isPCAd = false;
                if (Messenger != null)
                    Messenger("Missing grid");
               return;
            }

            try
            {

                //Create the AdControl object
                _Ad = new AdControl();
                _Ad.ApplicationId = apID; //Set the ap id
                _Ad.AdUnitId = unitId; //Set the unit id
                _Ad.IsAutoRefreshEnabled = autoRefresh; //Set the autorefresh property

                _Ad.Width = width; //Set its width
                _Ad.Height = height; //Set its height

                _Ad.VerticalAlignment = ConvertVerticalAlignment(vAlign); //Set its vertical position
                _Ad.HorizontalAlignment = ConvertHorizontalAlignment(hAlign); //Set its horizontal position


                //On error and on refreshed
                _Ad.ErrorOccurred += OnError;
                _Ad.AdRefreshed += OnRefreshed;

                _isPCAd = true;


                //Finally add it to our grid
                _baseGrid.Children.Add(_Ad);

                if (Messenger != null)
                    Messenger("PubCenter Ad Successfully Created");
            }
            catch(Exception e)
            {
                if (Messenger != null)
                    Messenger("Pubcenter Error: " + e.Message);
                _isPCAd = false;
            }
#endif

        }

        #endregion
        #region AdMob
        /// <summary>
        /// Used to create an AdMob ad
        /// Pass in your ad mob unit id, the format, height/width, position and whether its a test ad or not 
        /// The testAd bool is important, it allows us to test an admob ad
        /// </summary>
        public void CreateAd(string adUnit, AD_FORMATS format,HORIZONTAL_ALIGNMENT hAlign, VERTICAL_ALIGNMENT vAlign, bool testAd)
        {
#if WINDOWS_PHONE
            if (Dispatcher.InvokeOnUIThread == null && Messenger != null)
            {
                Messenger("Dispatcher Error: UI Thread not set up");
                return;
            }

            Dispatcher.InvokeOnUIThread(()=>
               {
                   BuildAd(adUnit, format,hAlign, vAlign, testAd);
               });
#endif
        }

        private void BuildAd(string adUnit, AD_FORMATS format, HORIZONTAL_ALIGNMENT hAlign, VERTICAL_ALIGNMENT vAlign, bool testAd)
        {
#if WINDOWS_PHONE

            //Check if an AdMob ad has already been created
            //If so we return, and display a message to the user
            if (_AdMobAd != null)
            {
                if (Messenger != null)
                    Messenger("AdMob Ad already present");
                return;
            }

            //Next we check if we have a grid
            if (_baseGrid == null)
            {
                _isAMAd = false;
                if (Messenger != null)
                    Messenger("Missing grid");
                return;
            }

            try
            {
                _AdMobAd = new AdView
                {
                    Format = ConvertAdFormat(format),
                    AdUnitID = adUnit
                };

                if (format != AD_FORMATS.SMART_BANNER)
                {

                }

                _AdMobAd.VerticalAlignment = ConvertVerticalAlignment(vAlign);

                _AdMobAd.ReceivedAd += OnAdReceived;
                _AdMobAd.FailedToReceiveAd += OnError;


                _baseGrid.Children.Add(_AdMobAd);
                //if (_baseGrid.Children.Count > 0)
                //    _adIndex = _baseGrid.Children.Count - 1;

                if (testAd)
                {
                    AdRequest adRequest = new AdRequest();
                    adRequest.ForceTesting = true;
                    _AdMobAd.LoadAd(adRequest);
                }


                //Finally add it to our grid
                _baseGrid.Children.Add(_AdMobAd);

                _isAMAd = true;
            }
            catch (Exception e)
            {
                if (Messenger != null)
                    Messenger("AdMob Error: " + e.Message);
                _isAMAd = false;
            }


#endif
        }

        #endregion

        #endregion

        #region Utilities (OnError, OnAdRefreshed, DebugMessages, Removal Of Ad)
#if WINDOWS_PHONE_APP 
        private void OnError(object sender, Microsoft.Advertising.Mobile.Common.AdErrorEventArgs e)
        {
            //Fire off the messenger
            if (Messenger != null)
                Messenger("PubCenter Ad Error: " + e.Error.Message);
            //Fire off the OnErrorEvent
            if (OnErrorDelegate != null)
                OnErrorDelegate();
        }

        private void OnRefreshed(object sender, RoutedEventArgs e)
        {
            //Fire off the messenger
            if (Messenger != null)
                Messenger("PubCenter Ad Refreshed");
            //Fire off the OnRefreshedEvent
            if (OnRefreshedDelegate != null)
                OnRefreshedDelegate();
        }

#endif

#if WINDOWS_PHONE
        private void OnRefreshed(object sender, EventArgs e)
        {
            //Fire off the messenger
            if (Messenger != null)
                Messenger("PubCenter Ad Refreshed");
            //Fire off the OnRefreshedEvent
            if (OnRefreshedDelegate != null)
                OnRefreshedDelegate();
        }

        private void OnError(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            //Fire off the messenger
            if (Messenger != null)
                Messenger("PubCenter Error: " + e.Error.Message);
            //Fire off the OnErrorEvent
            if (OnErrorDelegate != null)
                OnErrorDelegate();
        }

        private void OnAdReceived(object sender, AdEventArgs e)
        {
            //Fire off the messenger
            if (Messenger != null)
                Messenger("AdMob Ad Received");
            //Fire off the OnRefreshedEvent
            if (OnRefreshedDelegate != null)
                OnRefreshedDelegate();
        }

        private void OnError(object sender, AdErrorEventArgs e)
        {
            //Fire off the messenger
            if (Messenger != null)
                Messenger("AdMob Error: " + e.ErrorCode);
            //Fire off the OnErrorEvent
            if (OnErrorDelegate != null)
                OnErrorDelegate();

        }
#endif
#if WINDOWS_APP
        private void OnRefreshed(object sender, RoutedEventArgs e)
        {
            //Fire off the messenger
            if (Messenger != null)
                Messenger("PubCenter Ad Refreshed");
            //Fire off the OnRefreshedEvent
            if (OnRefreshedDelegate != null)
                OnRefreshedDelegate();
        }

        private void OnError(object sender, AdErrorEventArgs e)
        {
            //Fire off the messenger
            if (Messenger != null)
                Messenger("PubCenter Ad Error: " + e.Error.Message);
            //Fire off the OnErrorEvent
            if (OnErrorDelegate != null)
                OnErrorDelegate();
        }
#endif
        private void ShowMessage(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }



        //Removal of Ad
        public void HandleDestruction()
        {
#if WINDOWS_APP || WINDOWS_PHONE_APP


            if (Dispatcher.InvokeOnUIThread == null)
                return;

            Dispatcher.InvokeOnUIThread(() =>
            {
                RemoveAd();
            });
#endif
        }

#if WINDOWS_APP || WINDOWS_PHONE_APP
        private void RemoveAd()
        {
            foreach (UIElement child in _baseGrid.Children)
            {
                if (child == _Ad)
                {
                    _baseGrid.Children.Remove(child);
                    _Ad = null;

                    if (Messenger != null)
                        Messenger("Ad Successfully Removed");
                    return;
                }
            }



            if (Messenger != null)
                Messenger("Ad Object Not Found: Cannot Remove");
        }
#endif

        #endregion

        #region Helper Functions
#if WINDOWS_PHONE_APP || WINDOWS_PHONE
        /*<Summary>
         * Just our own converter for the enumerations
        </Summery>*/
        private GoogleAds.AdFormats ConvertAdFormat(AD_FORMATS f)
        {
            switch(f)
            {
                case AD_FORMATS.BANNER:
                    return AdFormats.Banner;
                case AD_FORMATS.SMART_BANNER:
                    return AdFormats.SmartBanner;
                default:
                    return AdFormats.Banner;
            }
        }
#endif

#if WINDOWS_PHONE_APP || WINDOWS_APP || WINDOWS_PHONE
        /*<Summary>
         * Just our own converter for the  alignment enumerations
        </Summery>*/
        private VerticalAlignment ConvertVerticalAlignment(VERTICAL_ALIGNMENT f)
        {
            switch (f)
            {
                case VERTICAL_ALIGNMENT.BOTTOM:
                    return VerticalAlignment.Bottom;
                case VERTICAL_ALIGNMENT.TOP:
                    return VerticalAlignment.Top;
                case VERTICAL_ALIGNMENT.CENTER:
                    return VerticalAlignment.Center;
                default:
                    return VerticalAlignment.Center;
            }
        }

        /*<Summary>
        * Just our own converter for the  alignment enumerations
        </Summery>*/
        private HorizontalAlignment ConvertHorizontalAlignment(HORIZONTAL_ALIGNMENT f)
        {
            switch (f)
            {
                case HORIZONTAL_ALIGNMENT.LEFT:
                    return HorizontalAlignment.Left;
                case HORIZONTAL_ALIGNMENT.RIGHT:
                    return HorizontalAlignment.Right;
                case HORIZONTAL_ALIGNMENT.CENTER:
                    return HorizontalAlignment.Center;
                default:
                    return HorizontalAlignment.Center;
            }
        }
#endif
        #endregion
    }
}

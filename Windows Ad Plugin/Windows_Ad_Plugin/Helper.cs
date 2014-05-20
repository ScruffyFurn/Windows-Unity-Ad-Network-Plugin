using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


#if NETFX_CORE
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.Graphics.Display;
using Windows.ApplicationModel;
using Microsoft.Advertising.WinRT.UI;
using Windows.UI.Xaml;
using System.Diagnostics;
#elif WINDOWS_PHONE
using Microsoft.Phone.Tasks;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Advertising.Mobile.UI;
using Microsoft.Advertising.Shared;
using Windows.Devices.Geolocation;
using GoogleAds;
#endif


namespace Windows_Ad_Plugin
{

    /// <summary>
    /// Allows for some common windows integration scenarops
    /// </summary>
    public class Helper : IDisposable
    {
#if WINDOWS_PHONE
        private DrawingSurfaceBackgroundGrid baseGrid = null;
#elif NETFX_CORE
        private SwapChainBackgroundPanel backPanel = null;
#endif

        //Windows Ads
#if WINDOWS_PHONE || NETFX_CORE
      private AdControl _ad = null;
#endif

        //AdMob Stuff
#if WINDOWS_PHONE
      private AdView _ad_Google = null;
#endif

        //Our own enum replacement for 
        //System.Windows.VerticalAlignment & System.Windows.HorizontalAlignment 
        //since we will not have access to those within Unity
        public enum VERTICAL_ALIGNMENT { BOTTOM, CENTER, TOP };
        public enum HORIZONTAL_ALIGNMENT { CENTER, LEFT, RIGHT };

        //Our own enum for Google's AdFormats
        public enum AD_FORMATS { BANNER, SMART_BANNER };

        private static Helper _instance;
        private bool _isBuilt = false;
        private static readonly object _sync = new object();

        private bool _isAdPresent = false;
        private string _errorMessage = "";

        private int _adIndex = 0;

        public static Helper Instance
        {
            get
            {
                lock (_sync)
                {
                    if (_instance == null)
                        _instance = new Helper();
                }
                return _instance;
            }
        }

        public Helper()
        {
        }

        public void Dispose()
        {
        }

        //This function is public and called from Unity
        //It then uses the dispatcher to invoke on the UI thread

        /*<Summary>
         * Windows ad service overload
        </Summery>*/
        public void CreateAd(string apId, string unitId, double height, double width, bool autoRefresh, HORIZONTAL_ALIGNMENT horizontalAlign, VERTICAL_ALIGNMENT vertAlign)
        {
#if WINDOWS_PHONE || NETFX_CORE
            Dispatcher.InvokeOnUIThread(() =>
                {
                   BuildAd(apId, unitId, height, width, autoRefresh,horizontalAlign,vertAlign);
                });
#endif
        }
        /*<Summary>
         * Builds a windows ad service ad
        </Summery>*/
        private void BuildAd(string apId, string unitId, double height, double width, bool autoRefresh, HORIZONTAL_ALIGNMENT horizontalAlign, VERTICAL_ALIGNMENT vertAlign)
        {
#if WINDOWS_PHONE || NETFX_CORE
           //Phone implementation
           //If the basegrid object is null it will just return, just a safety check
           //Creates the adControl object
           //Sets its height, width, appid, unitid and such and then uses the margin to position it on screen
           //Finally its added to the children of the basegrid

           //A check to make sure we do not get duplicate ads
           if(_ad != null)
            return;

           //This should be called prior to calling this function but just in case...
           //Lets check if the grid/panel exists
           if (!HasGrid())
           {
               _isBuilt = false;
               _errorMessage = "Missing grid to attach ad to";
           }
           try
           {
               _ad = new AdControl();
               _ad.ApplicationId = apId;
               _ad.AdUnitId = unitId;
               _ad.IsAutoRefreshEnabled = autoRefresh;

               _ad.Width = width;
               _ad.Height = height;

               _ad.VerticalAlignment = ConvertVerticalAlignment(vertAlign);
               _ad.HorizontalAlignment = ConvertHorizontalAlignment(horizontalAlign);


#if WINDOWS_PHONE

               baseGrid.Children.Add(_ad);

               if(baseGrid.Children.Count >0)
                    _adIndex = baseGrid.Children.Count - 1;
               

#elif NETFX_CORE
               backPanel.Children.Add(_ad);

               if (backPanel.Children.Count > 0)
                   _adIndex = backPanel.Children.Count - 1;
           
#endif
               _ad.ErrorOccurred += OnAdError_ErrorOccurred;

               _ad.AdRefreshed += OnAdRefreshed;

               _isBuilt = true;
               return;
           }
           catch (Exception e)
           {
               _errorMessage = "PubCenter Error: " + e.Message;
               _isBuilt = false;
           }
#else
            //function will return and do nothing if its in Unity editor
            _isBuilt = false;
#endif
        }

        /*<Summary>
         * AdMob overload
        </Summery>*/
        public void CreateAd(string adUnit, AD_FORMATS format, HORIZONTAL_ALIGNMENT horizontalAlign, VERTICAL_ALIGNMENT vertAlign, double width, double height, bool testAd)
        {
#if WINDOWS_PHONE
            Dispatcher.InvokeOnUIThread(() =>
            {
                BuildAd(adUnit, format,horizontalAlign,vertAlign,width,height,testAd);
            });
#endif
        }

        /*<Summary>
         * Builds an admob ad
        </Summery>*/
        private void BuildAd(string adUnit, AD_FORMATS format, HORIZONTAL_ALIGNMENT horizontalAlign, VERTICAL_ALIGNMENT vertAlign, double width, double height, bool testAd)
        {
#if WINDOWS_PHONE
           if(_ad_Google != null)
            return;

           if (!HasGrid())
           {
               _isBuilt = false;
               _errorMessage = "Missing grid to attach ad to";
           }

            try
            {
                _ad_Google = new AdView
                {
                    Format = ConvertAdFormat(format),
                    AdUnitID = adUnit
                };
                
                if (format != AD_FORMATS.SMART_BANNER)
                {
                    _ad_Google.Width = width;
                    _ad_Google.Height = height;
                    _ad_Google.HorizontalAlignment = ConvertHorizontalAlignment(horizontalAlign);
                }
   
                _ad_Google.VerticalAlignment = ConvertVerticalAlignment(vertAlign);
            
                _ad_Google.ReceivedAd += OnAdReceived;
                _ad_Google.FailedToReceiveAd += OnFailedToReceiveAd;


                baseGrid.Children.Add(_ad_Google);
                if (baseGrid.Children.Count > 0)
                    _adIndex = baseGrid.Children.Count - 1;

                if (testAd)
                {
                    AdRequest adRequest = new AdRequest();
                    adRequest.ForceTesting = true;
                    _ad_Google.LoadAd(adRequest);
                }
                
                _isBuilt = true;
            }
            catch(Exception e)
            {
                _errorMessage = "AdMob Error: " + e.Message;
                _isBuilt = false;
            }
#endif
        }


#if WINDOWS_PHONE
       /*<Summary>
        * Ad mob error control
        * Saves the error code
        * Changes the isAdPresent bool to false
        </Summery>*/
        private void OnFailedToReceiveAd(object sender, AdErrorEventArgs e)
        {
            _isAdPresent = false;
            _errorMessage = e.ErrorCode.ToString();
        }

        /*<Summary>
         * Ad mob received ad
        </Summery>*/
        private void OnAdReceived(object sender, AdEventArgs e)
        {
            _isAdPresent = true;
            _errorMessage = "";
        }
#endif

        public void SetGrid(object grid)
        {
#if WINDOWS_PHONE
           baseGrid = (DrawingSurfaceBackgroundGrid)grid;
#elif NETFX_CORE
            Debug.WriteLine(grid);
            backPanel = (SwapChainBackgroundPanel)grid;

#else
            //Unity or some such
            return;
#endif
        }

        public bool HasGrid()
        {
#if WINDOWS_PHONE
            //On device
            if (baseGrid == null)
                return false;
            else
                return true;
#elif NETFX_CORE
            if (backPanel == null)
            {
                return false;
            }
            else
            {
                return true;
            }
#else
            //Within unity
            return false;
#endif
        }
#if WINDOWS_PHONE
        /*<Summary>
         * Windows ad error control
        </Summery>*/
        private void OnAdError_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            _isAdPresent = false;
            _errorMessage = e.Error.Message;
        }
        /*<Summary>
         * Windows ad adRefreshed
        </Summery>*/
        private void OnAdRefreshed(object sender, EventArgs e)
        {
            _isAdPresent = true;
            _errorMessage = "";
        }
#endif

#if NETFX_CORE 
        private void OnAdError_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            _isAdPresent = false;
            _errorMessage = e.Error.Message;
        }

        private void OnAdRefreshed(object sender, RoutedEventArgs e)
        {
            _isAdPresent = true;
            _errorMessage = "";
        }
#endif
        //Helper functions

        public bool IsBuilt()
        {
            return _isBuilt;
        }
        public bool IsThereAnAd()
        {
#if WINDOWS_PHONE || NETFX_CORE
            return _isAdPresent;
#else
            return false;
#endif
        }

        public string GetErrorMesssage()
        {
#if WINDOWS_PHONE || NETFX_CORE
            return _errorMessage;
#else
            return "";
#endif
        }

        public void OpenWebPage(string link)
        {
#if WINDOWS_PHONE
            LaunchBrowserPhone(link);
#elif NETFX_CORE
            Dispatcher.InvokeOnUIThread(() =>
            {
                LaunchBrowserRT(link);
            });
#endif
        }

#if NETFX_CORE
        private async void LaunchBrowserRT(string link)
        {
            bool success = false;
            try
            {
                //Set the option to show a warning
                var options = new Windows.System.LauncherOptions();
                options.TreatAsUntrusted = true;

                Uri l = new Uri(link);
                success = await Windows.System.Launcher.LaunchUriAsync(l, options);
            }
            catch(Exception e)
            {
                _errorMessage = e.Message;
            }
        }
#endif
#if WINDOWS_PHONE
        private void LaunchBrowserPhone(string link)
        {
            WebBrowserTask wbtask = new WebBrowserTask();
            wbtask.Uri = new Uri(link);
            wbtask.Show();
        }
#endif




#if WINDOWS_PHONE
        /*<Summary>
         * Just our own converter for the enumerations
        </Summery>*/
        private GoogleAds.AdFormats ConvertAdFormat(AD_FORMATS f)
        {
            switch(f)
            {
                case AD_FORMATS.BANNER:
                    return AdFormats.Banner;
                    break;
                case AD_FORMATS.SMART_BANNER:
                    return AdFormats.SmartBanner;
                    break;
                default:
                    return AdFormats.Banner;
            }
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE
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



        /*<Summary>
         * Will destroy the current ad if need be
         * When the ad is created, the index of the ad is saved out
         * We find the child at the index of the ad and then remove it with extreme predjudice
        </Summery>*/
        public void HandleDestruction()
        {
            if (!_isAdPresent)
                return;

#if WINDOWS_PHONE
            Dispatcher.InvokeOnUIThread(() =>
            {
                baseGrid.Children.RemoveAt(_adIndex);
            });
            _ad = null;
            _ad_Google = null;
#elif NETFX_CORE
             Dispatcher.InvokeOnUIThread(() =>
            {
                backPanel.Children.RemoveAt(_adIndex);
            });
            _ad = null;
#endif

        }
    }
}


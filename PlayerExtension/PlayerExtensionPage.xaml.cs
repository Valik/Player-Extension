using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Основная страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=234237

namespace PlayerExtension
{
    /// <summary>
    /// Основная страница, которая обеспечивает характеристики, являющимися общими для большинства приложений.
    /// </summary>
    public sealed partial class PlayerExtensionPage : PlayerExtension.Common.LayoutAwarePage
    {
        private enum SyncState : int
        {
            READY = 0,
            STARTED = 1,
            STOPED = 2
        }

        private ObservableCollection<TrackName> mTracks;
        private SyncState mSyncState;

        private ResourceLoader mErrorLoader = new ResourceLoader("Errors");
        private ResourceLoader mResLoader = new ResourceLoader();

        public delegate void ConfigCompliteHandler(ConfigCompliteEvent e);
        public delegate void StartSyncHandler(StartSyncEvent e);
        public delegate void StopSyncHandler(StopSyncEvent e);

        public static event ConfigCompliteHandler ConfigComplite;
        public static event StartSyncHandler SyncStarted;
        public static event StopSyncHandler SyncStoped;

        private LicenseInformation mLicenseInfo = CurrentAppSimulator.LicenseInformation;
        private String mDonateURL = "http://donateme.net/479";

        public PlayerExtensionPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;

            mTracks = new ObservableCollection<TrackName>();
            mSyncState = SyncState.READY;
            TrackList.ItemsSource = mTracks;
        }

        private void CheckLicense()
        {
            if (mLicenseInfo.IsActive)
            {
                if (mLicenseInfo.IsTrial)
                {
                    if (buyGrid.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
                        buyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;


                    int remainingTrialTime = (mLicenseInfo.ExpirationDate - DateTime.Now).Days;
                }
                else
                {
                    if (buyGrid.Visibility == Windows.UI.Xaml.Visibility.Visible)
                        buyGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
            else
            {
                // A license is inactive only when there's an error.
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SettingPaneHelper.TryAddPolicyCommamd();

            if (ConfigComplite != null)
                ConfigComplite(new ConfigCompliteEvent(this));

            base.OnNavigatedTo(e);
        }

        public async Task ShowMessage(String message)
        {
            SetStopedStyleForInfoBox();
            MessageDialog dialog = new MessageDialog(message);
            dialog.Commands.Add(new UICommand("Ok"));
            await dialog.ShowAsync();
        }

        private void SetStartedStyleForInfoBox()
        {
            infoBox.Background = new SolidColorBrush(new Color() { A = 150, R = 0, G = 255, B = 0 });
            infoTextBlock.Text = mResLoader.GetString("infoStarted");
        }

        private void SetStopedStyleForInfoBox()
        {
            infoBox.Background = new SolidColorBrush(new Color() { A = 150, R = 255, G = 0, B = 0 });
            infoTextBlock.Text = mResLoader.GetString("infoStoped");
        }
        /// <summary>
        /// Заполняет страницу содержимым, передаваемым в процессе навигации. Также предоставляется любое сохраненное состояние
        /// при повторном создании страницы из предыдущего сеанса.
        /// </summary>
        /// <param name="navigationParameter">Значение параметра, передаваемое
        /// <see cref="Frame.Navigate(Type, Object)"/> при первоначальном запросе этой страницы.
        /// </param>
        /// <param name="pageState">Словарь состояния, сохраненного данной страницей в ходе предыдущего
        /// сеанса. Это значение будет равно NULL при первом посещении страницы.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState == null)
                return;
        }

        private void RecoverSyncState(int syncState)
        {
            switch ((SyncState)syncState)
            {
                case SyncState.READY:
                    { 
                        break;
                    }
                case SyncState.STARTED:
                    {
                        SetStartedStyleForInfoBox();
                        break;
                    }
                case SyncState.STOPED:
                    {
                        SetStopedStyleForInfoBox();
                        break;
                    }
            }
        }

        /// <summary>
        /// Сохраняет состояние, связанное с данной страницей, в случае приостановки приложения или
        /// удаления страницы из кэша навигации. Значения должны соответствовать требованиям сериализации
        /// <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">Пустой словарь, заполняемый сериализуемым состоянием.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            if (pageState == null)
                return;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            OnSyncStopped();
            SetStopedStyleForInfoBox();

            if (this.Frame != null)
                this.Frame.Navigate(typeof(DevicesInfoPage));
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            OnSyncStarted();

            mTracks.Insert(0, new TrackName() { trackName = mResLoader.GetString("messageLoading") });
            SetStartedStyleForInfoBox();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            OnSyncStopped();

            SetStopedStyleForInfoBox();
        }

        public void DisplaySynchronizedTrack(String track)
        {
            mTracks.Insert(0, new TrackName() { trackName = track } );
        }

        public void DisplayNoTracksToSync()
        {
            mTracks.Insert(0, new TrackName() { trackName = mResLoader.GetString("messageNoTracks") });
        }

        public void UpdateAccessToken()
        {
            String url = String.Format(VKConfig.VK_AUTHADDRESS, VKConfig.API_ID, VKScopeList.AUDIO);
            Uri uri = new Uri(url);

            myWebView.Navigate(uri);
            myWebView.LoadCompleted += new LoadCompletedEventHandler(NavigateComlited);
        }

        private async void NavigateComlited(object sender, NavigationEventArgs e)
        {
            String accessToken = GetAccessToken(e.Uri);

            if (accessToken != null)
            {
                ExtConfig.vkConfig = new VKConfig(accessToken);
                FakeStartSync();
            }
            else
            {
                await ShowMessage(mErrorLoader.GetString("troubleWithVk"));
                if(this.Frame != null)
                    this.Frame.Navigate(typeof(VKAuthPage));
            }
        }

        private void FakeStartSync()
        {
            StartButton_Click(null, null);
        }

        private String GetAccessToken(Uri uri)
        {
            if (uri.ToString().IndexOf("access_token") != -1)
            {
                string accessToken = "";
                int userId = 0;
                Regex myReg = new Regex(@"(?<name>[\w\d\x5f]+)=(?<value>[^\x26\s]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                foreach (Match m in myReg.Matches(uri.ToString()))
                {
                    if (m.Groups["name"].Value == "access_token")
                    {
                        accessToken = m.Groups["value"].Value;
                    }
                    else if (m.Groups["name"].Value == "user_id")
                    {
                        userId = Convert.ToInt32(m.Groups["value"].Value);
                    }
                    return accessToken;
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        private void Border_PointerPressed_1(object sender, PointerRoutedEventArgs e)
        {
            if (InfoBlock.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
                InfoBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
            else
                InfoBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void Border_PointerPressed_2(object sender, PointerRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(mDonateURL));
        }

        private async void purchasesButton_Click(object sender, RoutedEventArgs e)
        {
            if (mLicenseInfo.IsTrial)
            {
                try
                {
                    await CurrentAppSimulator.RequestAppPurchaseAsync(false);
                    if (!mLicenseInfo.IsTrial && mLicenseInfo.IsActive)
                    {
                        MessageDialog dialog = new MessageDialog("Покупка прошла успешно! Спасибо.");
                        dialog.Commands.Add(new UICommand("Ok"));
                        await dialog.ShowAsync();
                        buyGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                    else
                    {
                        
                    }
                }
                catch (Exception)
                {
                    
                }
            }
            else
            {
                
            }
        }

        private static void OnSyncStarted()
        {
            if (SyncStarted != null)
                SyncStarted.BeginInvoke(new StartSyncEvent(), null, null);
        }

        private static void OnSyncStopped()
        {
            if (SyncStoped != null)
                SyncStoped.BeginInvoke(new StopSyncEvent(), null, null);
        }
    }

    public class TrackName
    {
        public String trackName
        { get; set; }
    }
}

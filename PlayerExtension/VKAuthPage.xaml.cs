using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class VKAuthPage : PlayerExtension.Common.LayoutAwarePage
    {
        private ResourceLoader mErrorLoader = new ResourceLoader("Errors");
        public String mAccessToken;

        public VKAuthPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SettingPaneHelper.TryAddPolicyCommamd();

            InitWebView();

            base.OnNavigatedTo(e);
        }

        private void InitWebView()
        {
            WebView1.Visibility = Windows.UI.Xaml.Visibility.Visible;
            String url = String.Format(VKConfig.VK_AUTHADDRESS, VKConfig.API_ID, VKScopeList.AUDIO);
            Uri uri = new Uri(url);

            WebView1.Navigate(uri);
            WebView1.LoadCompleted += new LoadCompletedEventHandler(NavigateComlited);
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
        }
        /// <summary>
        /// Сохраняет состояние, связанное с данной страницей, в случае приостановки приложения или
        /// удаления страницы из кэша навигации. Значения должны соответствовать требованиям сериализации
        /// <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">Пустой словарь, заполняемый сериализуемым состоянием.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void NavigateComlited(object sender, NavigationEventArgs e)
        {
            String accessToken = GetAccessToken(e.Uri);

            if (accessToken != null)
            {
                mAccessToken = accessToken;
                WebView1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                forwardButton_Click(null, null);
            }
        }

        public String GetAccessToken(Uri uri)
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

        private async void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (mAccessToken == null)
            {
                MessageDialog dialog = new MessageDialog(mErrorLoader.GetString("vkError"));
                dialog.Commands.Add(new UICommand("Ok"));
                await dialog.ShowAsync();
                return;
            }

            VKConfig vkConfig = new VKConfig(mAccessToken);
            ExtConfig.vkConfig = vkConfig;

            GoToNextPage();
        }

        private void GoToNextPage()
        {
            if (this.Frame != null)
                this.Frame.Navigate(typeof(DevicesInfoPage));
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (mAccessToken != null)
            {
                VKConfig vkConfig = new VKConfig(mAccessToken);
                ExtConfig.vkConfig = vkConfig;
            }

            GoToPreviousPage();
        }

        private void GoToPreviousPage()
        {
            if (this.Frame != null)
                this.Frame.Navigate(typeof(LastFMPage));
        }

        private void Border_PointerPressed_1(object sender, PointerRoutedEventArgs e)
        {
            if (InfoBlock.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
                InfoBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
            else
                InfoBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}

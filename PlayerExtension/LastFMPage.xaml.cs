using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Resources;
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
    public sealed partial class LastFMPage : PlayerExtension.Common.LayoutAwarePage
    {
        private ResourceLoader mResLoader = new ResourceLoader();
        private ResourceLoader mErrorLoader = new ResourceLoader("Errors");
        private string mNickStr = "nickname";

        public LastFMPage()
        {
            this.InitializeComponent();
            //this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SettingPaneHelper.TryAddPolicyCommamd();

            base.OnNavigatedTo(e);
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
            if (ExtConfig.lastFMConfig != null)
            {
                userNameBox.Text = ExtConfig.lastFMConfig.userName;
                userNameBox.Background = new SolidColorBrush(Colors.BlueViolet);
            }
            else
            {
                mNickStr = mResLoader.GetString("Nickname");
                userNameBox.Text = mNickStr;
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
        }

        private async void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            String userName = userNameBox.Text.Replace("\r", "").Replace("\n", "").Trim();
            if (userName == null || userName == "" || userName == mNickStr)
            {
                userNameBox.Background = new SolidColorBrush(Colors.Red);
                MessageDialog dialog = new MessageDialog(mErrorLoader.GetString("lastError"));
                dialog.Commands.Add(new UICommand("Ok"));
                await dialog.ShowAsync();
                return;
            }

            userNameBox.Background = new SolidColorBrush(Colors.BlueViolet);

            int limit = 10;
            LastFMConfig lastConfig = new LastFMConfig(userName, LastFMSearchType.LOVED_TRACKS, limit);
            ExtConfig.lastFMConfig = lastConfig;

            GoToNextPage();
        }

        private void GoToNextPage()
        {
            if (ExtConfig.vkConfig != null)
            {
                if (this.Frame != null)
                    this.Frame.Navigate(typeof(DevicesInfoPage));
            }
            else if (this.Frame != null)
                this.Frame.Navigate(typeof(VKAuthPage));
        }

        private void userNameBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                String userNick = userNameBox.Text.Replace("\r", "").Replace("\n", "").Trim();
                if (userNick != "" && userNick != mNickStr)
                {
                    userNameBox.Text = userNick;
                    userNameBox.Background = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    userNameBox.Background = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void userNameBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (userNameBox.Text == mNickStr)
                userNameBox.Text = "";
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

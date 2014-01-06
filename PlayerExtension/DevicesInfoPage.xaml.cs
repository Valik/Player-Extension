using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using PlayerExtension.ExtensionModel;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI;
using Windows.ApplicationModel.Resources;
using PlayerExtension.Common;

// Документацию по шаблону элемента "Основная страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=234237

namespace PlayerExtension
{
    /// <summary>
    /// Основная страница, которая обеспечивает характеристики, являющимися общими для большинства приложений.
    /// </summary>
    public sealed partial class DevicesInfoPage : PlayerExtension.Common.LayoutAwarePage
    {
        private ResourceLoader mResLoader = new ResourceLoader();
        private ResourceLoader mErrorLoader = new ResourceLoader("Errors");
        private List<DeviceInfo> mDevInfo;
        private StorageFolder mLibrary;

        public DevicesInfoPage()
        {
            this.InitializeComponent();
            //this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            SettingPaneHelper.TryAddPolicyCommamd();

            await InitDevicesList();
            LoadSavedState();

            base.OnNavigatedTo(e);
        }

        private async Task InitDevicesList()
        {
            List<String> devices = await ModelController.GetAvailableDevices();
            devicesList.Items.Clear();

            foreach (String curDevice in devices)
            {
                GetDeviceViewItem(curDevice);
                devicesList.Items.Add(GetDeviceViewItem(curDevice));
            }
        }

        private ListViewItem GetDeviceViewItem(String deviceName)
        {
            ListViewItem deviceItem = new ListViewItem();
            deviceItem.BorderThickness = new Thickness(2);
            deviceItem.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 254, 254, 254));
            deviceItem.Height = 35;
            deviceItem.Margin = new Thickness(0, 0, 2, 2);
            deviceItem.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            deviceItem.Content = deviceName;
            return deviceItem;
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

        private void LoadSavedState()
        {
            if (ExtConfig.playerConnectorConfig == null)
            {
                libraryPathBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                return;
            }

            mLibrary = ExtConfig.playerConnectorConfig.musicLibraryFolder;
            libraryPathBox.Visibility = Windows.UI.Xaml.Visibility.Visible;
            pathTextBox.Text = mLibrary.Path;
            libraryButton.Background = new SolidColorBrush(Colors.BlueViolet);

            if (!ExtConfig.playerConnectorConfig.IsDevicesSelected)
                return;

            mDevInfo = ExtConfig.playerConnectorConfig.selectedDevices;

            String deviceName = ExtConfig.playerConnectorConfig.selectedDevices.First().deviceName;

            var items = devicesList.Items;
            for (int i = 0; i < items.Count; i++)
            {
                var curItem = items[i];
                if (((String)((ListViewItem)curItem).Content) == deviceName)
                {
                    devicesList.SelectedIndex = i;
                    ((ListViewItem)devicesList.SelectedItem).Background = new SolidColorBrush(Colors.BlueViolet);
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
        }

        private async void DevicesList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ListViewItem selectedItem = devicesList.SelectedItem as ListViewItem;
            if (selectedItem == null)
                return;

            String deviceName = selectedItem.Content as string;

            var currentState = Windows.UI.ViewManagement.ApplicationView.Value;
            if (currentState == Windows.UI.ViewManagement.ApplicationViewState.Snapped && !Windows.UI.ViewManagement.ApplicationView.TryUnsnap())
            {
                //"Could not unsnap (required to launch the file open picker). Please unsnap this app before proceeding", NotifyType.ErrorMessage);
            }
            else
            {
                FolderPicker folderPicker = new FolderPicker()
                {
                    CommitButtonText = mResLoader.GetString("folderOnPlayer"),
                    SuggestedStartLocation = PickerLocationId.ComputerFolder,
                    ViewMode = PickerViewMode.Thumbnail,
                    FileTypeFilter = { ".mp3" }
                };

                StorageFolder destinationFolder = await folderPicker.PickSingleFolderAsync();

                if (destinationFolder != null)
                {
                    if (mLibrary == null || destinationFolder.Path != mLibrary.Path)
                    {
                        mDevInfo = new List<DeviceInfo>();
                        mDevInfo.Add(new DeviceInfo(deviceName, destinationFolder));

                        foreach (var curItem in devicesList.Items)
                        {
                            ((ListViewItem)curItem).Background = null;
                        }
                        selectedItem.Background = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        selectedItem.Background = new SolidColorBrush(Colors.Red);
                        MessageDialog dialog = new MessageDialog(mErrorLoader.GetString("badPlayerFolder"));
                        dialog.Commands.Add(new UICommand("Ok"));
                        await dialog.ShowAsync();
                    }
                }
                else
                {
                    selectedItem.Background = new SolidColorBrush(Colors.Red);
                    MessageDialog dialog = new MessageDialog(mErrorLoader.GetString("playerFolderIsNotSelected"));
                    dialog.Commands.Add(new UICommand("Ok"));
                    await dialog.ShowAsync();
                }
            }

        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var currentState = Windows.UI.ViewManagement.ApplicationView.Value;
            if (currentState == Windows.UI.ViewManagement.ApplicationViewState.Snapped && !Windows.UI.ViewManagement.ApplicationView.TryUnsnap())
            {
                //"Could not unsnap (required to launch the file open picker). Please unsnap this app before proceeding", NotifyType.ErrorMessage);
            }
            else
            {
                FolderPicker folderPicker = new FolderPicker()
                {
                    CommitButtonText = mResLoader.GetString("folderOnComputer"),
                    SuggestedStartLocation = PickerLocationId.MusicLibrary,
                    ViewMode = PickerViewMode.Thumbnail,
                    FileTypeFilter = { ".mp3" }
                };

                StorageFolder musicLibraryFolder = await folderPicker.PickSingleFolderAsync();

                if (musicLibraryFolder != null)
                {
                    if (mDevInfo == null || mDevInfo.Count == 0 || musicLibraryFolder.Path != mDevInfo.First().deviceFolder.Path)
                    {
                        mLibrary = musicLibraryFolder;
                        libraryButton.Background = new SolidColorBrush(Colors.Green);
                        libraryPathBox.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        pathTextBox.Text = musicLibraryFolder.Path;
                    }
                    else
                    {
                        libraryButton.Background = new SolidColorBrush(Colors.Red);
                        libraryPathBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        MessageDialog dialog = new MessageDialog(mErrorLoader.GetString("badLibraryFolder"));
                        dialog.Commands.Add(new UICommand("Ok"));
                        await dialog.ShowAsync();
                    }
                }
                else
                {
                    libraryButton.Background = new SolidColorBrush(Colors.Red);
                    libraryPathBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    MessageDialog dialog = new MessageDialog(mErrorLoader.GetString("libraryFolderIsNotSelected"));
                    dialog.Commands.Add(new UICommand("Ok"));
                    await dialog.ShowAsync();
                }
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            GoToPreviousPage();
        }

        private void GoToPreviousPage()
        {
            if (ExtConfig.vkConfig != null)
            {
                if (this.Frame != null)
                    this.Frame.Navigate(typeof(LastFMPage));
            }
            else if (this.Frame != null)
                this.Frame.Navigate(typeof(VKAuthPage));
        }

        private async void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            //if (mDevInfo == null || mDevInfo.Count == 0)
            //{
            //    MessageDialog dialog = new MessageDialog(mErrorLoader.GetString("devicesPlayerError"));
            //    await dialog.ShowAsync();
            //    return;
            //}

            if (mLibrary == null)
            {
                MessageDialog dialog = new MessageDialog(mErrorLoader.GetString("devicesLibraryError"));
                await dialog.ShowAsync();
                return;
            }

            PlayerConnectorConfig connectorConfig = mDevInfo == null ? new PlayerConnectorConfig(mLibrary) :
                                                                       new PlayerConnectorConfig(mDevInfo, mLibrary);
            ExtConfig.playerConnectorConfig = connectorConfig;

            GoToNextPage();
        }

        private void GoToNextPage()
        {
            if (this.Frame != null)
                this.Frame.Navigate(typeof(PlayerExtensionPage));
        }
    }
}

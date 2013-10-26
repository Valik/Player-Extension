using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;

namespace PlayerExtension
{
    internal static class SettingPaneHelper
    {
        private static readonly String PolicyURL = "https://drive.google.com/folderview?id=0B1NX8leIQmHlOHZ5Qm5KYXVYSUE&usp=sharing";
        private static ResourceLoader resLoader = new ResourceLoader();
        private static bool policyCommandAdded = false;
 
        public static void TryAddPolicyCommamd()
        {
            if (!policyCommandAdded)
            {
                SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
                policyCommandAdded = true;
            }
        }

        private static void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            string policyStr = resLoader.GetString("privacyPolicy");
            UICommandInvokedHandler handler = new UICommandInvokedHandler(OnSettingsCommand);

            SettingsCommand policyCommand = new SettingsCommand("privacyPolicy", policyStr, handler);
            args.Request.ApplicationCommands.Add(policyCommand);
        }

        private static async void OnSettingsCommand(IUICommand command)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(PolicyURL));
        }
    }
}

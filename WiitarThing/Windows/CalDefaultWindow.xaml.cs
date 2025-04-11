using System.Windows;

namespace WiitarThing.Windows
{
    /// <summary>
    /// Interaction logic for CalDefaultWindow.xaml
    /// </summary>
    public partial class CalDefaultWindow : Window
    {
        public CalDefaultWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (UserPrefs.Instance.defaultProperty != null)
            {
                switch (UserPrefs.Instance.defaultProperty.calPref)
                {
                    case NintrollerLib.WiimoteSettings.CalibrationPreference.Minimal:
                        radioMin.IsChecked = true; break;
                    case NintrollerLib.WiimoteSettings.CalibrationPreference.Default:
                        radioDefault.IsChecked = true; break;
                    case NintrollerLib.WiimoteSettings.CalibrationPreference.More:
                        radioMod.IsChecked = true; break;
                    case NintrollerLib.WiimoteSettings.CalibrationPreference.Extra:
                        radioExt.IsChecked = true; break;
                }
            }

            foreach (var pref in UserPrefs.Instance.devicePrefs)
            {
                if (pref.hid != "all")
                    copyCombo.Items.Add(pref.name);
            }

            if (copyCombo.Items.Count > 0)
            {
                radioCopy.IsEnabled = true;
                copyCombo.IsEnabled = true;
                copyCombo.SelectedIndex = 0;
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            NintrollerLib.WiimoteSettings prop = new NintrollerLib.WiimoteSettings();
            prop.hid = "all";
            prop.name = "Default";
            
            if (radioDefault.IsChecked ?? false)
            {
                prop.calPref = NintrollerLib.WiimoteSettings.CalibrationPreference.Default;
            }
            else if (radioMin.IsChecked ?? false)
            {
                prop.calPref = NintrollerLib.WiimoteSettings.CalibrationPreference.Minimal;
            }
            else if (radioMod.IsChecked ?? false)
            {
                prop.calPref = NintrollerLib.WiimoteSettings.CalibrationPreference.More;
            }
            else if (radioExt.IsChecked ?? false)
            {
                prop.calPref = NintrollerLib.WiimoteSettings.CalibrationPreference.Extra;
            }
            else if (radioCopy.IsChecked ?? false)
            {
                prop.calPref = NintrollerLib.WiimoteSettings.CalibrationPreference.Custom;
                var copy = UserPrefs.Instance.devicePrefs[copyCombo.SelectedIndex];
                prop.autoConnect = copy.autoConnect;
                prop.autoNum = copy.autoNum;
                prop.calString = copy.calString;
                prop.rumbleIntensity = copy.rumbleIntensity;
                prop.useRumble = copy.useRumble;
                prop.enableJoystick = copy.enableJoystick;
            }

            UserPrefs.Instance.defaultProperty = prop;
            UserPrefs.SavePrefs();
            Close();
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            UserPrefs.Instance.devicePrefs.Remove(UserPrefs.Instance.defaultProperty);
            UserPrefs.Instance.defaultProfile = null;
            UserPrefs.SavePrefs();
            Close();
        }
    }
}

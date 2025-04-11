using System;
using System.Windows;
using System.Windows.Controls;

namespace WiitarThing
{
    /// <summary>
    /// Interaction logic for PropWindow.xaml
    /// </summary>
    public partial class PropWindow : Window
    {
        public bool doSave = false;
        public bool customCalibrate = false;
        public NintrollerLib.WiimoteSettings props;

        PropWindow(NintrollerLib.WiimoteSettings org) : this(org, "Controller") {
        }

        public PropWindow(NintrollerLib.WiimoteSettings org, string defaultName)
        {
            InitializeComponent();

            props = new NintrollerLib.WiimoteSettings(org);
            nameInput.Text = string.IsNullOrWhiteSpace(props.name) ? defaultName : props.name;
            enableTouchStrip.IsChecked = org.enableTouchStrip;
            enableJoystick.IsChecked = org.enableJoystick;
            defaultInput.Text = props.profile;
            autoCheckbox.IsChecked = props.autoConnect;
            if (props.autoNum >= 0 && props.autoNum <= autoConnectNumber.Items.Count)
            {
                autoConnectNumber.SelectedIndex = props.autoNum;
            }
            if (props.rumbleIntensity >= 0 && props.rumbleIntensity <= rumbleSelection.Items.Count)
            {
                rumbleSelection.SelectedIndex = props.rumbleIntensity;
            }
            switch (props.calPref)
            {
                case NintrollerLib.WiimoteSettings.CalibrationPreference.Default:
                    calibrationSelection.SelectedIndex = 0;
                    break;
                case NintrollerLib.WiimoteSettings.CalibrationPreference.Minimal:
                    calibrationSelection.SelectedIndex = 1;
                    break;
                case NintrollerLib.WiimoteSettings.CalibrationPreference.More:
                    calibrationSelection.SelectedIndex = 2;
                    break;
                case NintrollerLib.WiimoteSettings.CalibrationPreference.Extra:
                    calibrationSelection.SelectedIndex = 3;
                    break;
                case NintrollerLib.WiimoteSettings.CalibrationPreference.Custom:
                    calibrationSelection.SelectedIndex = 4;
                    break;
            }
            pointerSelection.SelectedIndex = (int)org.pointerMode;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            customCalibrate = false;
            Close();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            customCalibrate = false;
            doSave = true;
            Close();
        }

        private void autoCheckbox_Click(object sender, RoutedEventArgs e)
        {
            props.autoConnect = autoCheckbox.IsChecked == true;
        }

        private void enableJoystick_Click(object sender, RoutedEventArgs e)
        {
            //props.enableJoystick = enableJoystick.IsChecked == true;
            props.enableJoystick = enableJoystick.IsChecked ?? false;
        }

        private void enableTouchStrip_Click(object sender, RoutedEventArgs e)
        {
            //props.enableTouchStrip = enableTouchStrip.IsChecked == true;
            props.enableTouchStrip = enableTouchStrip.IsChecked ?? false;
        }

        private void nameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            props.name = nameInput.Text;
        }

        private void defaultInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            props.profile = defaultInput.Text;
        }

        private void defaultBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".wsp";
            dialog.Filter = App.PROFILE_FILTER;

            Nullable<bool> doLoad = dialog.ShowDialog();

            if (doLoad == true && dialog.CheckFileExists)
            {
                defaultInput.Text = dialog.FileName;
            }
        }

        private void AutoConnect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (props != null)
            {
                props.autoConnect = autoConnectNumber.SelectedIndex > 0;
                props.autoNum = autoConnectNumber.SelectedIndex;
            }
        }

        private void Rumble_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (props != null)
            {
                props.useRumble = rumbleSelection.SelectedIndex > 0;
                props.rumbleIntensity = rumbleSelection.SelectedIndex;
            }
        }

        private void Calibration_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (props != null)
            {
                switch (calibrationSelection.SelectedIndex)
                {
                    case 0:
                        props.calPref = NintrollerLib.WiimoteSettings.CalibrationPreference.Default;
                        customCalibrate = false;
                        break;

                    case 1:
                        props.calPref = NintrollerLib.WiimoteSettings.CalibrationPreference.Minimal;
                        customCalibrate = false;
                        break;

                    case 2:
                        props.calPref = NintrollerLib.WiimoteSettings.CalibrationPreference.More;
                        customCalibrate = false;
                        break;

                    case 3:
                        props.calPref = NintrollerLib.WiimoteSettings.CalibrationPreference.Extra;
                        customCalibrate = false;
                        break;

                    case 4:
                        //props.calPref = Property.CalibrationPreference.Custom;
                        //customCalibrate = true;
                        //Hide();
                        break;
                }
            }
        }

        private void calibrationSelection_DropDownClosed(object sender, EventArgs e)
        {
            if (props != null && calibrationSelection.SelectedIndex == 4)
            {
                props.calPref = NintrollerLib.WiimoteSettings.CalibrationPreference.Custom;
                customCalibrate = true;
                Hide();
            }
        }

        private void pointerSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (props != null)
            {
                props.pointerMode = (NintrollerLib.WiimoteSettings.PointerOffScreenMode)pointerSelection.SelectedIndex;
            }
        }
    }
}

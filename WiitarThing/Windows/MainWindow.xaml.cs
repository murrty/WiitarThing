﻿using Hardcodet.Wpf.TaskbarNotification;
using NintrollerLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using System.Diagnostics;
using Nefarius.ViGEm.Client;

namespace WiitarThing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>Returns true if the current application has focus, false otherwise</summary>
        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        public static MainWindow Instance { get; private set; }

        private List<DeviceInfo> hidList;
        private List<DeviceControl> deviceList;
        private Task _refreshTask;
        private CancellationTokenSource _refreshToken;
        private bool _refreshing;

        public MainWindow()
        {
            // Check for ViGEmBus upon startup
            try
            {
                var client = new ViGEmClient();
                client.Dispose();
                WiitarDebug.Log("ViGEmBus found");
            }
            catch (Nefarius.ViGEm.Client.Exceptions.VigemBusNotFoundException)
            {
                WiitarDebug.Log("ViGEmBus not found");
                if (MessageBox.Show(
                    "WiitarThing requires the ViGEmBus driver to function.\nClick OK to get it from its releases page.",
                    "ViGEmBus Not Found",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Error,
                    MessageBoxResult.Cancel
                  ) == MessageBoxResult.OK
                )
                {
                    Process.Start("https://github.com/ViGEm/ViGEmBus/releases");
                }
                Application.Current.Shutdown();
                return;
            }

            hidList = new List<DeviceInfo>();
            deviceList = new List<DeviceControl>();

            InitializeComponent();

            Instance = this;

            if (UserPrefs.Instance.lastPos.X != -32_000 && UserPrefs.Instance.lastPos.Y != -32_000)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = UserPrefs.Instance.lastPos.X;
                this.Top = UserPrefs.Instance.lastPos.Y;
            }

            Version version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;

#if LOW_BANDWIDTH
            Title = $"WiitarThing v{version} (Low Bandwidth)";
#else
            Title = $"WiitarThing v{version}";
#endif

#if DEBUG
            Title += " Debug Build";

            //var sb = new StringBuilder();
            //sb.AppendLine("You are running MEOW'S DEBUG BUILD of WiinUSoft.");
            //sb.AppendLine();
            //sb.AppendLine("WII REMOTE Debug Command List:");
            //sb.AppendLine("-A: Display the next data packet received from the GUITAR.");


            //MessageBox.Show(sb.ToString(), "Notice", MessageBoxButton.OK, MessageBoxImage.Information);


#else
            labelDebugBuild.Visibility = Visibility.Hidden;
#endif
        }

        public void HideWindow()
        {
            //if (WindowState == WindowState.Minimized)
            //{
            //    trayIcon.Visibility = Visibility.Visible;
            //    Hide();
            //}
        }

        public void ShowWindow()
        {
            trayIcon.Visibility = Visibility.Hidden;
            Show();
            WindowState = WindowState.Normal;
        }

        public void ShowBalloon(string title, string message, BalloonIcon icon)
        {
            ShowBalloon(title, message, icon, null);
        }

        public void ShowBalloon(string title, string message, BalloonIcon icon, SystemSound sound)
        {
            trayIcon.Visibility = Visibility.Visible;
            trayIcon.ShowBalloonTip(title, message, icon);

            if (sound != null)
            {
                sound.Play();
            }

            Task restoreTray = new Task(new Action(() =>
            {
                Thread.Sleep(7000);
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => trayIcon.Visibility = WindowState == WindowState.Minimized ? Visibility.Visible : Visibility.Hidden));
            }));
            restoreTray.Start();
        }

        private void Refresh()
        {
            hidList = DeviceInfo.GetPaths();
            var fileShare = UserPrefs.Instance.greedyMode || UserPrefs.Instance.toshibaMode ? FileShare.None : FileShare.ReadWrite;
            List<KeyValuePair<int, DeviceControl>> connectSeq = new List<KeyValuePair<int, DeviceControl>>();
            
            foreach (var hid in hidList)
            {
                DeviceControl existingDevice = null;

                foreach (DeviceControl d in deviceList)
                {
                    if (d.DevicePath == hid.DevicePath)
                    {
                        existingDevice = d;
                        break;
                    }
                }

                if (existingDevice != null)
                {
                    if (!existingDevice.Connected)
                    {
                        existingDevice.RefreshState();
                        if (existingDevice.properties.autoConnect && existingDevice.ConnectionState == DeviceState.Discovered)
                        {
                            connectSeq.Add(new KeyValuePair<int, DeviceControl>(existingDevice.properties.autoNum, existingDevice));
                        }
                    }
                }
                else
                {
                    if (!HidDeviceStream.TryCreate(hid.DevicePath, out var stream, fileShare))
                    {
                        Debug.WriteLine($"Failed to open {hid.DevicePath} in sharing mode {fileShare}");
                        if (fileShare != FileShare.None)
                        {
                            continue;
                        }
        
                        if (!HidDeviceStream.TryCreate(hid.DevicePath, out stream, FileShare.ReadWrite))
                        {
                            Debug.WriteLine($"Failed to open {hid.DevicePath} with sharing forced");
                            continue;
                        }
                    }

                    Nintroller n = new Nintroller(stream, hid.Type);

                    if (stream.CanRead)
                    {
                        deviceList.Add(new DeviceControl(n, hid.DevicePath));
                        deviceList[deviceList.Count - 1].OnConnectStateChange += DeviceControl_OnConnectStateChange;
                        deviceList[deviceList.Count - 1].OnConnectionLost += DeviceControl_OnConnectionLost;
                        deviceList[deviceList.Count - 1].RefreshState();
                        if (deviceList[deviceList.Count - 1].properties.autoConnect)
                        {
                            connectSeq.Add(new KeyValuePair<int, DeviceControl>(deviceList[deviceList.Count - 1].properties.autoNum, deviceList[deviceList.Count - 1]));
                        }
                    }
                }
            }

            int target = -1;

            for (int i = 0; i < 4; i++)
            {
                if (Holders.XInputHolder.availabe.Length > i)
                {
                    if (Holders.XInputHolder.availabe[i])
                    {
                        target = i;
                        break;
                    }
                }
            }
            
            if (target < 0)
            {
                return;
            }

            //while (!Holders.XInputHolder.availabe[target] && target < 4)
            //{
            //    target++;
            //}

            // Auto Connect First Available devices
            for (int a = 0; a < connectSeq.Count; a++)
            {
                var thingy = connectSeq[a];

                if (thingy.Key == 5)
                {
                    if (Holders.XInputHolder.availabe[target] && target < 4)
                    {
                        if (thingy.Value.Device.IsConnected || thingy.Value.Device.DataStream.Open())
                        {
                            thingy.Value.targetXDevice = target;
                            thingy.Value.ConnectionState = DeviceState.Connected_XInput;
                            thingy.Value.Device.BeginReading();
                            thingy.Value.Device.GetStatus();
                            thingy.Value.Device.SetPlayerLED(target + 1);
                            target++;
                        }
                    }

                    connectSeq.Remove(thingy);
                }
            }

            // Auto connect in preferred order
            for (int i = 1; i < connectSeq.Count; i++)
            {
                if (connectSeq[i].Key < connectSeq[i - 1].Key)
                {
                    var tmp = connectSeq[i];
                    connectSeq[i] = connectSeq[i - 1];
                    connectSeq[i - 1] = tmp;
                    i = 0;
                }
            }
            
            foreach(KeyValuePair<int, DeviceControl> d in connectSeq)
            {
                if (Holders.XInputHolder.availabe[target] && target < 4)
                {
                    if (d.Value.Device.IsConnected || d.Value.Device.DataStream.Open())
                    {
                        d.Value.targetXDevice = target;
                        d.Value.ConnectionState = DeviceState.Connected_XInput;
                        d.Value.Device.BeginReading();
                        d.Value.Device.GetStatus();
                        d.Value.Device.SetPlayerLED(target + 1);
                        target++;
                    }
                }
            }
        }

        private void AutoRefresh(bool set)
        {
            if (set && !_refreshing)
            {
                _refreshing = true;
                _refreshToken = new CancellationTokenSource();
                _refreshTask = new Task(new Action(() =>
                {
                    while (!_refreshToken.IsCancellationRequested)
                    {
                        Thread.Sleep(1000);
                        if (_refreshToken.IsCancellationRequested) break;
                        Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => Refresh()));
                    }

                    _refreshing = false;
                }), _refreshToken.Token);
                _refreshTask.Start();
            }
            else if (!set && _refreshing)
            {
                _refreshToken.Cancel();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Version version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
                menu_version.Header = "Version " + version.ToString();
            }
            catch { }

            if (UserPrefs.Instance.startMinimized)
            {
                menu_StartMinimized.IsChecked = true;
                WindowState = WindowState.Minimized;
            }
            
            menu_AutoStart.IsChecked = UserPrefs.Instance.autoStartup;
            menu_NoSharing.IsChecked = UserPrefs.Instance.greedyMode;
            menu_AutoRefresh.IsChecked = UserPrefs.Instance.autoRefresh;
            menu_MsBluetooth.IsChecked = !UserPrefs.Instance.toshibaMode;

            Refresh();
            AutoRefresh(menu_AutoRefresh.IsChecked && ApplicationIsActivated());
        }

        private void DeviceControl_OnConnectStateChange(DeviceControl sender, DeviceState oldState, DeviceState newState)
        {
            if (oldState == newState)
                return;

            switch (oldState)
            {
                case DeviceState.Discovered:
                    //if (groupAvailable.Children.Contains(sender))
                    groupAvailable.Children.Remove(sender);
                    break;

                case DeviceState.Connected_XInput:
                    //if (groupXinput.Children.Contains(sender))
                    groupXinput.Children.Remove(sender);
                    break;

                //case DeviceState.Connected_VJoy:
                //    groupXinput.Children.Remove(sender);
                //    break;
            }

            switch (newState)
            {
                case DeviceState.Discovered:
                    if (!groupAvailable.Children.Contains(sender))
                        groupAvailable.Children.Add(sender);
                    break;

                case DeviceState.Connected_XInput:
                    if (!groupXinput.Children.Contains(sender))
                        groupXinput.Children.Add(sender);
                    break;

                //case DeviceState.Connected_VJoy:
                //    groupXinput.Children.Add(sender);
                //    break;
            }
            
            if (menu_AutoRefresh.IsChecked)
            {
                AutoRefresh(ApplicationIsActivated());
            }
        }

        private void DeviceControl_OnConnectionLost(DeviceControl sender)
        {
            if (groupAvailable.Children.Contains(sender))
            {
                groupAvailable.Children.Remove(sender);
            }
            else if (groupXinput.Children.Contains(sender))
            {
                groupXinput.Children.Remove(sender);
            }

            deviceList.Remove(sender);

            AutoRefresh(menu_AutoRefresh.IsChecked);
        }
        
        private void btnDetatchAllXInput_Click(object sender, RoutedEventArgs e)
        {
            List<DeviceControl> detatchList = new List<DeviceControl>();
            foreach (DeviceControl d in groupXinput.Children)
            {
                detatchList.Add(d);
            }
            foreach (DeviceControl d in detatchList)
            {
                d.Detatch();
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        static System.Threading.Tasks.Task Delay(int milliseconds)
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();
            new System.Threading.Timer(_ => tcs.SetResult(null)).Change(milliseconds, -1);
            return tcs.Task;
        }

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            Windows.SyncWindow sync = new Windows.SyncWindow();

            //foreach (var c in groupAvailable.Children)
            //{
            //    DeviceControl devCtrl = (DeviceControl)c;
            //    devCtrl.
            //}

            //deviceList[0].Device.

            //sync.ConnectedDeviceAddresses

            sync.NewDeviceFound += Sync_NewDeviceFound;
            sync.Closed += Sync_Closed;

            sync.ShowDialog();

            //var timer = new DispatcherTimer();

            //timer.Interval = new TimeSpan(0, 0, 1);

            //timer.Tick += (sender2, e2) =>
            //{
            //    if (sync != null && sync.IsVisible)
            //    {
            //        //if (!sync.IsFocused)
            //        //{
            //        //    sync.Close();
            //        //}

            //       // Refresh();
            //    }
            //    else
            //    {
            //        timer.Stop();
            //    }
            //};
            //timer.Start();

        }

        private void Sync_Closed(object sender, EventArgs e)
        {
            ((Windows.SyncWindow)sender).NewDeviceFound -= Sync_NewDeviceFound;
            ((Windows.SyncWindow)sender).Closed -= Sync_Closed;
        }

        private void Sync_NewDeviceFound(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => Refresh()));
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            HideWindow();
        }

        private void MenuItem_Show_Click(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        private void MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            //foreach (DeviceControl dc in deviceList)
            //{
            //    if (dc.ConnectionState == DeviceState.Connected_XInput
            //     || dc.ConnectionState == DeviceState.Connected_VJoy)
            //    {
            //        dc.Detatch();
            //    }
            //}

            //Close();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (btnSettings.ContextMenu != null)
            {
                btnSettings.ContextMenu.IsOpen = true;
            }
        }

        private void menu_AutoStart_Click(object sender, RoutedEventArgs e)
        {
            menu_AutoStart.IsChecked = !menu_AutoStart.IsChecked;
            UserPrefs.AutoStart = menu_AutoStart.IsChecked;
            UserPrefs.SavePrefs();
        }

        private void menu_StartMinimized_Click(object sender, RoutedEventArgs e)
        {
            menu_StartMinimized.IsChecked = !menu_StartMinimized.IsChecked;
            UserPrefs.Instance.startMinimized = menu_StartMinimized.IsChecked;
            UserPrefs.SavePrefs();
        }

        private void menu_NoSharing_Click(object sender, RoutedEventArgs e)
        {
            menu_NoSharing.IsChecked = !menu_NoSharing.IsChecked;
            UserPrefs.Instance.greedyMode = menu_NoSharing.IsChecked;
            UserPrefs.SavePrefs();
        }

        private void menu_AutoRefresh_Click(object sender, RoutedEventArgs e)
        {
            menu_AutoRefresh.IsChecked = !menu_AutoRefresh.IsChecked;
            UserPrefs.Instance.autoRefresh = menu_AutoRefresh.IsChecked;
            UserPrefs.SavePrefs();
            AutoRefresh(menu_AutoRefresh.IsChecked && ApplicationIsActivated());
        }

        private void menu_SetDefaultCalibration_Click(object sender, RoutedEventArgs e)
        {
            var dWin = new Windows.CalDefaultWindow();
            dWin.ShowDialog();
        }

        private void menu_MsBluetooth_Click(object sender, RoutedEventArgs e)
        {
            menu_MsBluetooth.IsChecked = !menu_MsBluetooth.IsChecked;
            UserPrefs.Instance.toshibaMode = !menu_MsBluetooth.IsChecked;
            UserPrefs.SavePrefs();
        }

        private void menu_Github_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/TheNathannator/WiitarThing");
        }

        #region Shortcut Creation
        public void CreateShortcut(string path)
        {
            IShellLink link = (IShellLink)new ShellLink();

            string pPath = System.Reflection.Assembly.GetEntryAssembly().CodeBase;

            link.SetDescription("WiitarThing");
            link.SetPath(new Uri(pPath).LocalPath);
            link.SetWorkingDirectory(pPath);

            IPersistFile file = (IPersistFile)link;
            file.Save(Path.Combine(path, "WiitarThing.lnk"), false);
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }
        #endregion

        private void btnRemoveAllWiimotes_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove all Wii remotes from this PC?\n\nNote: this cannot be cancelled once it begins and may take a couple of minutes depending on how many bluetooth devices you have connected to your PC.", "Remove All Wiimotes?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var dlg = new Windows.RemoveAllWiimotesWindow();
                dlg.Owner = this;
                dlg.ShowDialog();
                MessageBox.Show("WiitarThing will now restart.\n\nDon't forget to reconnect your controllers afterward!", "Wiimotes Removed", MessageBoxButton.OK, MessageBoxImage.Information);

                //Refresh();

                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (groupXinput.Children.Count > 0)
            {
                var dlg = MessageBox.Show("Are you sure you want to close WiitarThing? Any devices connected will be disconnected.", "Close WiitarThing?", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (dlg != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                foreach (DeviceControl d in groupXinput.Children)
                {
                    d.Detatch();
                }
            }

            PointD pos = NativeImports.ActualPosition.ActualPos(this);

            if (UserPrefs.Instance.lastPos != pos)
            {
                UserPrefs.Instance.lastPos = pos;
                UserPrefs.SavePrefs();
            }
        }

        private void buttonTestInputs_Click(object sender, RoutedEventArgs e)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                MessageBox.Show("Cannot run input test on a non-Win32 NT OS. Figure it out yourself. If there's an equivalent to 'joy.cpl', open an issue and tell me about it.", "WiitarThing", MessageBoxButton.OK);
                return;
            }

            Process.Start("joy.cpl");
        }
    }

    class ShowWindowCommand : ICommand
    {
        public void Execute(object parameter)
        {
            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.ShowWindow();
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

#pragma warning disable 0067 // CanExecute will never change, this event is deliberately never used
        public event EventHandler CanExecuteChanged;
#pragma warning restore 0067
    }
}

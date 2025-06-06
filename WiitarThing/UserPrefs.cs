﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace WiitarThing
{
    public class UserPrefs
    {
        private static UserPrefs _instance;
        public static UserPrefs Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\prefs.config"))
                    {
                        DataPath = AppDomain.CurrentDomain.BaseDirectory + @"\prefs.config";
                        LoadPrefs();
                    }
                    else if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\WiitarThing_prefs.config"))
                    {
                        DataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\WiitarThing_prefs.config";
                        LoadPrefs();
                    }
                    else
                    {
                        _instance = new UserPrefs();
                        _instance.devicePrefs = new List<NintrollerLib.WiimoteSettings>();
                        _instance.defaultProfile = new Profile();
                        // we could, but just in case lets not
                        //_instance.greedyMode = Environment.OSVersion.Version.Major < 10; 
                        //_instance.toshibaMode = !Shared.Windows.NativeImports.BluetoothEnableDiscovery(IntPtr.Zero, true);
                        DataPath = AppDomain.CurrentDomain.BaseDirectory + @"\prefs.config";
                        
                        if (!SavePrefs())
                        {
                            DataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\WiitarThing_prefs.config";
                            SavePrefs();
                        }
                    }
                }

                return _instance;
            }
        }

        public static string DataPath { get; protected set; }
        public static bool AutoStart
        {
            get { return Instance.autoStartup; }
            set
            {
                try
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                    if (value)
                    {
                        if (key.GetValue("WiinUSoft") == null)
                        {
                            key.SetValue("WiinUSoft", (new Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase)).LocalPath);
                        }
                    }
                    else
                    {
                        key.DeleteValue("WiinUSoft", false);
                    }
                }
                catch
                {
                    string dir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

                    if (value)
                    {
                        if (!File.Exists(Path.Combine(dir, "WiinUSoft.lnk")))
                        {
                            MainWindow.Instance.CreateShortcut(dir);
                        }
                    }
                    else
                    {
                        if (File.Exists(Path.Combine(dir, "WiinUSoft.lnk")))
                        {
                            File.Delete(Path.Combine(dir, "WiinUSoft.lnk"));
                        }
                    }
                }

                Instance.autoStartup = value;
            }
        }

        public List<NintrollerLib.WiimoteSettings> devicePrefs;
        public Profile defaultProfile;
        public NintrollerLib.WiimoteSettings defaultProperty;
        public bool autoStartup;
        public bool startMinimized;
        public bool greedyMode;
        public bool toshibaMode;
        public bool autoRefresh = true;
        public PointD lastPos = new PointD(-32_000, -32_000);

        public static bool LoadPrefs()
        {
            bool successful = false;
            XmlSerializer X = new XmlSerializer(typeof(UserPrefs));

            try
            {
                if (File.Exists(DataPath))
                {
                    using (FileStream stream = File.OpenRead(DataPath))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        _instance = X.Deserialize(reader) as UserPrefs;
                        reader.Close();
                        stream.Close();
                    }

                    successful = true;

                    if (_instance?.devicePrefs != null)
                    {
                        _instance.defaultProperty = _instance.devicePrefs.Find((p) =>
                            p.hid.Equals("all", StringComparison.OrdinalIgnoreCase));
                    }
                }
            }
            catch (Exception e) 
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return successful;
        }

        public static bool SavePrefs()
        {
            bool successful = false;
            XmlSerializer X = new XmlSerializer(typeof(UserPrefs));

            try
            {
                if (File.Exists(DataPath))
                {
                    FileInfo prefs = new FileInfo(DataPath);
                    using (FileStream stream = File.Open(DataPath, FileMode.Create, FileAccess.ReadWrite))
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        X.Serialize(writer, _instance);
                        writer.Close();
                        stream.Close();
                    }
                }
                else
                {
                    using (FileStream stream = File.Create(DataPath))
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        X.Serialize(writer, _instance);
                        writer.Close();
                        stream.Close();
                    }
                }

                successful = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return successful;
        }

        public NintrollerLib.WiimoteSettings GetDevicePref(string hid)
        {
            foreach (var pref in devicePrefs)
            {
                if (pref.hid == hid)
                {
                    return pref;
                }
            }

            return defaultProperty;
        }

        public void AddDevicePref(NintrollerLib.WiimoteSettings property)
        {
            foreach (var pref in devicePrefs)
            {
                if (pref.hid == property.hid)
                {
                    pref.name               = property.name;
                    pref.autoConnect        = property.autoConnect;
                    pref.profile            = property.profile;
                    pref.connType           = property.connType;
                    pref.autoNum            = property.autoNum;
                    pref.rumbleIntensity    = property.rumbleIntensity;
                    pref.useRumble          = property.useRumble;
                    pref.enableTouchStrip   = property.enableTouchStrip;
                    pref.enableJoystick     = property.enableJoystick;
                    pref.calPref            = property.calPref;

                    return;
                }
            }

            devicePrefs.Add(property);
        }

        public void UpdateDeviceIcon(string path, string icon)
        {
            var prop = devicePrefs.FindIndex((p) => p.hid == path);

            if (prop >= 0)
            {
                devicePrefs[prop].lastIcon = icon;
                SavePrefs();
            }
        }

        public string GetDeviceIcon(string path)
        {
            var prop = devicePrefs.FindIndex((p) => p.hid == path);

            if (prop >= 0)
            {
                return devicePrefs[prop].lastIcon;
            }

            return "";
        }
    }

    public class Profile
    {
        public enum HolderType
        {
            XInput = 0,
            DInput = 1
        }

        public NintrollerLib.ControllerType profileType;
        public HolderType connType;
        public List<string> controllerMapKeys;
        public List<string> controllerMapValues;

        public Profile()
        {
            profileType = NintrollerLib.ControllerType.Wiimote;
            controllerMapKeys = new List<string>();
            controllerMapValues = new List<string>();
            connType = HolderType.XInput;
        }

        public Profile(NintrollerLib.ControllerType type)
        {
            profileType = type;
            controllerMapKeys = new List<string>();
            controllerMapValues = new List<string>();
            connType = HolderType.XInput;
        }
    }

}

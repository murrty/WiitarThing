namespace NintrollerLib {
    public class WiimoteSettings {
        public enum ProfHolderType {
            XInput = 0,
            DInput = 1
        }

        public enum CalibrationPreference {
            Raw = -2,
            Minimal = -1,
            Default = 0,
            More = 1,
            Extra = 2,
            Custom = 3
        }

        public enum PointerOffScreenMode {
            Center = 0,
            SnapX = 1,
            SnapY = 2,
            SnapXY = 3
        }

        public string lastIcon = "";

        public string hid = "";
        public string name = "";
        public bool autoConnect = false;
        public bool useRumble = true;
        public bool enableTouchStrip = false;
        public bool enableJoystick = false;
        public int autoNum = 0;
        public int rumbleIntensity = 2;
        public ProfHolderType connType;
        public string profile = "";
        public CalibrationPreference calPref;
        public string calString = ""; // not the best solution for saving the custom config but makes it easy
        public PointerOffScreenMode pointerMode = PointerOffScreenMode.Center;

        public WiimoteSettings() {
            connType = ProfHolderType.XInput;
            calPref = CalibrationPreference.Default;
            pointerMode = PointerOffScreenMode.Center;
        }

        public WiimoteSettings(string ID) {
            hid = ID;
            connType = ProfHolderType.XInput;
            calPref = CalibrationPreference.Default;
            pointerMode = PointerOffScreenMode.Center;
        }

        public WiimoteSettings(WiimoteSettings copy) {
            CopyFrom(copy);
        }

        public void CopyFrom(WiimoteSettings prop) {
            hid = prop.hid;
            name = prop.name;
            autoConnect = prop.autoConnect;
            autoNum = prop.autoNum;
            useRumble = prop.useRumble;
            enableTouchStrip = prop.enableTouchStrip;
            enableJoystick = prop.enableJoystick;
            rumbleIntensity = prop.rumbleIntensity;
            connType = prop.connType;
            profile = prop.profile;
            calPref = prop.calPref;
            calString = prop.calString;
            pointerMode = prop.pointerMode;
        }

        public void Reset() {
            lastIcon = "";
        }
    }
}

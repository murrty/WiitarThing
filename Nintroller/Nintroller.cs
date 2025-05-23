using System;
using System.Diagnostics;
using System.Threading;

namespace NintrollerLib
{
    /// <summary>
    /// Used to represent a Nintendo controller
    /// </summary>
    public class Nintroller : IDisposable
    {
        #region Members
        // Events
        /// <summary>
        /// Called with updated controller input states.
        /// </summary>
        public event EventHandler<NintrollerStateEventArgs>     StateUpdate     = delegate { };
        /// <summary>
        /// Called when an extension change is detected in the controller.
        /// </summary>
        public event EventHandler<NintrollerExtensionEventArgs> ExtensionChange = delegate { };
        /// <summary>
        /// Called when the controller's battery get low.
        /// </summary>
        public event EventHandler<LowBatteryEventArgs>          LowBattery      = delegate { };
        /// <summary>
        /// Called when the connection loss is detected.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs>        Disconnected    = delegate { };

        // General
        private bool               _connected                   = false;
        private INintrollerState   _state                       = new Wiimote();
        private CalibrationStorage _calibrations                = new CalibrationStorage();
        private ControllerType     _currentType                 = ControllerType.Unknown;
        private ControllerType     _forceType                   = ControllerType.Unknown;
        private IRCamMode          _irMode                      = IRCamMode.Off;
        private IRCamSensitivity   _irSensitivity               = IRCamSensitivity.Level3;
        private byte               _rumbleBit                   = 0x00;
        private byte               _battery                     = 0x00;
        private bool               _batteryLow                  = false;
        private bool               _led1, _led2, _led3, _led4;

        // Read/Writing Variables
        private HidDeviceStream  _stream;                    // Read and Write Stream
        private bool             _reading    = false;        // true if actively reading
        private readonly object  _readingObj = new object(); // for locking/blocking
        private readonly object  _writingObj = new object(); // for locking/blocking

        // help with parsing Reports
        private AcknowledgementType _ackType    = AcknowledgementType.NA;
        private StatusType          _statusType = StatusType.DiscoverExtension; // initial value was originally StatusType.Unknown, but it's a good idea to be able to distinguish the first received status report from subsequent ones
        private ReadReportType      _readType   = ReadReportType.Unknown;

        // boolean flag indicating whether guitar encryption has been fully set up and guitar bytes should be decrypted in every data report received
        private bool _encryptionEnabled    = false;
        private bool _enableGuitarJoystick = false;
        private bool _enableGuitarTouchStrip = false;
        #endregion

        #region Properties

        /// <summary>
        /// True if the controller is open to communication.
        /// </summary>
        public bool IsConnected { get { return _connected; } }
        /// <summary>
        /// The data stream to the controller.
        /// </summary>
        public HidDeviceStream DataStream { get { return _stream; } }
        /// <summary>
        /// The type of controller this has been identified as
        /// </summary>
        public ControllerType Type { get { return _currentType; } }
        /// <summary>
        /// The calibration settings applied to the respective controller types.
        /// </summary>
        public CalibrationStorage StoredCalibrations { get { return _calibrations; } }

        /// <summary>
        /// Gets or Sets the current IR Camera Mode.
        /// (will turn the camera on or off)
        /// </summary>
        public IRCamMode IRMode
        {
            get { return _irMode; }
            set
            {
                if (_irMode != value)
                {
                    switch (_currentType)
                    {
                        case ControllerType.Wiimote:
                            // this can be set to any mode
                            _irMode = value;
                            
                            if (value == IRCamMode.Off)
                            {
                                DisableIR();
                            }
                            else
                            {
                                EnableIR();
                            }
                            break;

                        case ControllerType.ClassicController:
                        case ControllerType.ClassicControllerPro:
                        case ControllerType.Nunchuk:
                        case ControllerType.NunchukB:
                            // only certian modes can be set
                            if (value == IRCamMode.Off)
                            {
                                _irMode = value;
                                DisableIR();
                            }
                            else if (value != IRCamMode.Full) // we won't use Full
                            {
                                _irMode = value;
                                EnableIR();
                            }
                            break;

                        default:
                            // do nothing, IR usage is invalid
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// Gets or Sets the IR Sensitivity Mode.
        /// (Only set if the IR Camera is On)
        /// </summary>
        public IRCamSensitivity IRSensitivity
        {
            get { return _irSensitivity; }
            set
            {
                if (_irSensitivity != value && _irMode != IRCamMode.Off)
                {
                    switch (_currentType)
                    {
                        case ControllerType.Wiimote:
                        case ControllerType.ClassicController:
                        case ControllerType.ClassicControllerPro:
                        case ControllerType.Nunchuk:
                        case ControllerType.NunchukB:
                            _irSensitivity = value;
                            EnableIR();
                            break;

                        default:
                            // do nothing
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or Sets the controller's force feedback
        /// </summary>
        public bool IsRumbleEnabled
        {
            get
            {
                return _rumbleBit == 0x01;
            }
            set
            {
                _rumbleBit = (byte)(value ? 0x01 : 0x00);
                ApplyLEDs();
            }
        }

        /// <summary>
        /// Gets or Sets the LED in position 1
        /// </summary>
        public bool IsLed1Enabled
        {
            get
            {
                return _led1;
            }
            set
            {
                if (_led1 != value)
                {
                    _led1 = value;
                    ApplyLEDs();
                }
            }
        }
        /// <summary>
        /// Gets or Sets the LED in position 2
        /// </summary>
        public bool IsLed2Enabled
        {
            get
            {
                return _led2;
            }
            set
            {
                if (_led2 != value)
                {
                    _led2 = value;
                    ApplyLEDs();
                }
            }
        }
        /// <summary>
        /// Gets or Sets the LED in position 3
        /// </summary>
        public bool IsLed3Enabled
        {
            get
            {
                return _led3;
            }
            set
            {
                if (_led3 != value)
                {
                    _led3 = value;
                    ApplyLEDs();
                }
            }
        }
        /// <summary>
        /// Gets or Sets the LED in position 4
        /// </summary>
        public bool IsLed4Enabled
        {
            get
            {
                return _led4;
            }
            set
            {
                if (_led4 != value)
                {
                    _led4 = value;
                    ApplyLEDs();
                }
            }
        }
        /// <summary>
        /// The controller's current approximate battery level.
        /// </summary>
        public BatteryStatus BatteryLevel
        {
            get
            {
                if (_batteryLow)
                {
                    return BatteryStatus.VeryLow;
                }
                else
                {
                    // Calculate the approximate battery level based on the controller type
                    if (_currentType == ControllerType.ProController)
                    {
                        var level = 2f * ((float)_battery - 205f);

                        if (level > 90f)
                            return BatteryStatus.VeryHigh;
                        else if (level > 80f)
                            return BatteryStatus.High;
                        else if (level > 70f)
                            return BatteryStatus.Medium;
                        else if (level > 60f)
                            return BatteryStatus.Low;
                        else
                            return BatteryStatus.VeryLow;
                    }
                    else
                    {
                        // better calculation method based on: https://github.com/dolphin-emu/dolphin/blob/master/Source/Core/Core/HW/WiimoteCommon/WiimoteReport.h#L207
                        var level = (float)_battery * 246f / 255f - 1.3f;
                        //var level = 100f * (float)_battery / 192f;

                        if (level > 80f)
                            return BatteryStatus.VeryHigh;
                        else if (level > 60f)
                            return BatteryStatus.High;
                        else if (level > 40f)
                            return BatteryStatus.Medium;
                        else if (level > 20f)
                            return BatteryStatus.Low;
                        else
                            return BatteryStatus.VeryLow;
                    }
                }
            }
        }

        // wrapping the encryption flag in a property so that each change in its value will automatically be followed by an appropriate debug message
        public bool IsEncryptionEnabled
        {
            get
            {
                return _encryptionEnabled;
            }
            set
            {
                if (value != _encryptionEnabled)
                {
                    Log("Guitar Encryption " + (value ? "enabled" : "disabled"));
                }
                _encryptionEnabled = value;
            }
        }

        public bool IsGuitarJoystickEnabled {
            get {
                return _enableGuitarJoystick;
            }
            set {
                if (value != _enableGuitarJoystick)
                {
                    this._enableGuitarJoystick = value;
                }

                if (_state is Guitar guitar)
                {
                    _state = guitar.SetJoystickState(value);
                }
            }
        }

        public bool IsGuitarTouchStripEnabled {
            get {
                return this._enableGuitarTouchStrip;
            }
            set {
                if (value != _enableGuitarTouchStrip)
                {
                    this._enableGuitarTouchStrip = value;
                }

                if (_state is Guitar guitar)
                {
                    _state = guitar.SetTouchStripState(value);
                }
            }
        }

        #endregion

        #region Necessities

        /// <summary>
        /// Creates an instance using the provided data stream.
        /// </summary>
        /// <param name="dataStream">Stream to the controller.</param>
        public Nintroller(HidDeviceStream dataStream)
        {
            _state = null; // overwrites the value given during declaration (new Wiimote())
            _stream = dataStream;
        }

        /// <summary>
        /// Creates an instance using the provided data stream and expected controller type.
        /// </summary>
        /// <param name="dataStream">Stream to the controller.</param>
        /// <param name="hintType">Expected type of the controller.</param>
        public Nintroller(HidDeviceStream dataStream, ControllerType hintType) : this(dataStream)
        {
            _currentType = hintType; // overwrites the value given during declaration (ControllerType.Unknown)
        }

        /// <summary>
        /// Disposes
        /// </summary>
        public void Dispose()
        {
            StopReading();
            GC.SuppressFinalize(this);

            if (_stream != null)
                _stream.Close();
        }

        internal static void Log(string message)
        {
            #if DEBUG
            Debug.WriteLine(message);
            #endif
        }

        #endregion

        #region Connectivity

        /// <summary>
        /// Opens a connection stream to the device.
        /// (Reading is not yet started)
        /// </summary>
        /// <returns>Success</returns>
        [Obsolete("Open the stream instead then use BeginReading instead.")]
        public bool Connect()
        {
            try
            {
                _connected = _stream != null && _stream.CanRead && _stream.CanWrite;
                Log("Connected to device");
            }
            catch (Exception ex)
            {
                Log("Error Connecting to device: " + ex.ToString());
            }

            return _connected;
        }

        /// <summary>
        /// Closes the connection stream to the device.
        /// </summary>
        [Obsolete("Use StopReading instead and then close the stream.")]
        public void Disconnect()
        {
            _reading = false;

            if (_stream != null)
                _stream.Close();

            _connected = false;

            Log("Disconnected device");
        }

        #endregion

        #region Data Requesting
        /// <summary>
        /// Starts asynchronously recieving data from the device.
        /// </summary>
        public void BeginReading()
        {
            _connected = true;
            
            // kickoff the reading process if it hasn't started already
            if (!_reading && _stream != null && _stream.Open())
            {
                _reading = true;
                var thread = new Thread(ReadThread);
                thread.Start();
            }
        }

        /// <summary>
        /// Sends a status request to the device.
        /// </summary>
        public void GetStatus()
        {
            byte[] buffer = new byte[2];

            buffer[0] = (byte)OutputReport.StatusRequest;
            buffer[1] = _rumbleBit;

            SendData(buffer);
        }

        /// <summary>
        /// Changes the device's reporting type.
        /// </summary>
        /// <param name="reportType">The report type to set to.</param>
        /// <param name="continuous">If data should be sent repeatingly or only on changes.</param>
        public void SetReportType(InputReport reportType, bool continuous = false)
        {
            if (reportType == InputReport.Acknowledge ||
                reportType == InputReport.ReadMem ||
                reportType == InputReport.Status)
            {
                Log("Can't Set the report type to: " + reportType.ToString());
            }
            else
            {
                ApplyReportingType(reportType, continuous);
            }
        }

        // Performs background reading
        private async void ReadThread()
        {
            int reportLength = _stream.InputLength;
            byte[] readBuffer = new byte[reportLength];
            var cancel = new CancellationTokenSource();
            while (_reading)
            {
                if (_stream == null || !_stream.CanRead)
                    break;

                try
                {
                    var readTask = _stream.ReadAsync(readBuffer, 0, readBuffer.Length, cancel.Token);
                    if (!readTask.Wait(3000))
                    {
                        // Cancel read and get status to make sure the wiimote is still connected
                        Log("Read timed out, are we still connected?");
                        cancel.Cancel();
                        cancel.Dispose();
                        cancel = new CancellationTokenSource();
                        GetStatus();
                        continue;
                    }
                    int bytesRead = await readTask;
                    if (bytesRead < 1)
                    {
                        Log("No data read!");
                        continue;
                    }
                    ParseReport(readBuffer);
                }
                catch (Exception e)
                {
                    Log("Error reading: " + e.ToString());
                    GetStatus();
                    continue;
                }
            }
            cancel.Dispose();
            StopReading();
        }

        // Request data from the device's memory
        private void ReadMemory(int address, short size)
        {
            byte[] buffer = new byte[7];

            buffer[0] = (byte)OutputReport.ReadMemory;

            buffer[1] = (byte)(((address & 0xFF000000) >> 24) | _rumbleBit);
            buffer[2] = (byte) ((address & 0x00FF0000) >> 16);
            buffer[3] = (byte) ((address & 0x0000FF00) >>  8);
            buffer[4] = (byte)  (address & 0x000000FF);

            buffer[5] = (byte)((size & 0xFF00) >> 8);
            buffer[6] = (byte) (size & 0xFF);

            SendData(buffer);
        }

        private void ReadMemory(int address, byte[] data)
        {
            byte[] buffer = new byte[Constants.REPORT_LENGTH];

            buffer[0] = (byte)OutputReport.ReadMemory;

            buffer[1] = (byte)(((address & 0xFF000000) >> 24) | _rumbleBit);
            buffer[2] = (byte)((address & 0x00FF0000) >> 16);
            buffer[3] = (byte)((address & 0x0000FF00) >> 8);
            buffer[4] = (byte)(address & 0x000000FF);
            buffer[5] = (byte)data.Length;

            Array.Copy(data, 0, buffer, 6, Math.Min(data.Length, 16));

            SendData(buffer);
        }

        // Read calibration from the controller
        private void GetCalibration()
        {
            // TODO: New: Test (possibly move)
            // don't attempt on Pro Controllers
            ReadMemory(0x0016, 7);
        }

        // Sets the reporting mode type
        private void ApplyReportingType(InputReport reportType, bool continuous = false)
        {
            byte[] buffer = new byte[3];

            buffer[0] = (byte)OutputReport.DataReportMode;
            buffer[1] = (byte)((continuous ? 0x04 : 0x00) | _rumbleBit);
            buffer[2] = (byte)reportType;

            SendData(buffer);
        }
        #endregion

        #region Data Sending
        // sends bytes to the device
        private void SendData(byte[] report)
        {
            if (!_connected)
            {
                Log("Can't Send data, we are not connected!");
                return;
            }
            
            lock (_writingObj)
            {
                try
                {
                    _stream.Write(report, 0, report.Length);
                }
                catch (Exception ex)
                {
                    Log("Error while writing to the stream: " + ex.ToString());
                    StopReading();
                    Disconnected?.Invoke(this, new DisconnectedEventArgs(ex));
                }
            }
        }

        // writes bytes to the device's memory
        private void WriteToMemory(int address, byte[] data)
        {
            byte[] buffer = new byte[Constants.REPORT_LENGTH];

            buffer[0] = (byte)OutputReport.WriteMemory;
            buffer[1] = (byte)(((address & 0xFF000000) >> 24) | _rumbleBit);
            buffer[2] = (byte) ((address & 0x00FF0000) >> 16);
            buffer[3] = (byte) ((address & 0x0000FF00) >>  8);
            buffer[4] = (byte)  (address & 0x000000FF);
            buffer[5] = (byte)data.Length;

            Array.Copy(data, 0, buffer, 6, Math.Min(data.Length, 16));

            SendData(buffer);
        }

        // set's the device's LEDs and Rumble states
        private void ApplyLEDs()
        {
            byte[] buffer = new byte[2];

            buffer[0] = (byte)OutputReport.LEDs;
            buffer[1] = (byte)
            (
                (_led1 ? 0x10 : 0x00) |
                (_led2 ? 0x20 : 0x00) |
                (_led3 ? 0x40 : 0x00) |
                (_led4 ? 0x80 : 0x00) |
                (_rumbleBit)
            );

            SendData(buffer);
        }

        // This function decrypts the 6 guitar bytes in the given buffer starting at the given offset, assuming the encryption key is 16 zero bytes.
        private void GuitarDecryptBuffer(byte[] data, int offset)
        {
            /* The standard decryption method when the key is 16 zero bytes is to use the transformation shown here:
               https://wiibrew.org/wiki/Wiimote/Extension_Controllers#Registers_.2F_Initialization, where table1[x] and table2[x] are all 0x97
               (or 0x17, which is equivalent). However, this value did NOT work for the Nyko Frontman, producing results not in line with the expected format.
               The guitar apparently reacts differently to an all-zeros key, as hinted here: https://github.com/dolphin-emu/dolphin/blob/master/Source/Core/Core/HW/WiimoteEmu/Encryption.cpp#L501
               Nevertheless, this function also includes the standard method, in case there are other guitars out there requiring encryption to work,
               whose responses can be decrypted correctly using 0x97.
               To distinguish the Nyko Frontman from those other guitars, we try decrypting the 5th byte using 0x97, then checking if the bits guaranteed to be 1
               by the format are actually 1. The 5th byte was chosen because it's very predictable - only 3 of its bits can vary (Down, -, +) so it has the
               smallest amount of possible values.
            */
            byte decrypted = (byte)(((data[offset + 4] ^ 0x97) + 0x97) & 0xFF);
            bool useCommonValue = (data[offset + 4] != 0xFF) && ((decrypted & 0xAB) == 0xAB);

            for (int i = 0; i < 6; i++)
            {
                if (useCommonValue)
                {
                    data[offset + i] = (byte)(((data[offset + i] ^ 0x97) + 0x97) & 0xFF);
                }
                else
                {
                    /* The Nyko Frontman uses the value 0x4D. Basically I found it by taking some examples of encrypted bytes I got from the guitar which don't decrypt
                       correctly with 0x97, then trying all possible values until reaching the one that decrypts all of them correctly.
                    */
                    data[offset + i] = (byte)(((data[offset + i] ^ 0x4D) + 0x4D) & 0xFF);
                }
            }

            Log("Decrypted guitar bytes: " + BitConverter.ToString(data, offset, 6));
        }
        #endregion

        #region Data Parsing

        private void ParseReport(byte[] report)
        {
            InputReport input = (InputReport)report[0];
            bool error = (report[4] & 0x0F) == 0x03;

            switch(input)
            {
                #region Status Reports
                case InputReport.Status:
                    #region Parse Status
                    Log("Status Report");
                    // core buttons can be parsed if desired

                    switch (_statusType)
                    {
                        case StatusType.Requested:
                            //
                            break;

                        case StatusType.IR_Enable:
                            EnableIR();
                            break;

                        case StatusType.Unknown:
                        case StatusType.DiscoverExtension:
                        default:
                            /* Based on observations, while the Nyko Frontman is connected to the Wiimote (before being configured properly by WiitarThing),
                               every few seconds the Wiimote sends status reports where the extension controller bit is alternating between 0 and 1
                               from one report to another. These contradicting reports disrupt the extension discovery process, causing it to toggle between
                               a Wiimote and a Guitar, all of this while the guitar remains physically connected.
                               They also disrupt further initialization steps (e.g. a status report claiming no extension is connected arrives while setting up guitar
                               encryption). To summarize, it's best to ignore a status report in two cases:
                               1. It's not the report that is specifically requested right after connecting the device (determined by _statusType == StatusType.Unknown,
                                  hence why _statusType is first initialized to StatusType.DiscoverExtension), and it's received when the current type
                                  is not known (determined by _state == null, _currentType is initialized to Wiimote in the constructor so it couldn't be used).
                               2. It's received during guitar encryption setup or IR camera setup (detectable via _ackType).
                            */
                            if ((_statusType == StatusType.Unknown && _state == null) ||
                                (_ackType.ToString().StartsWith("EncryptionSetup") || _ackType.ToString().StartsWith("IR_Step")))
                            {
                                break;
                            }

                            /* In case an unsolicited status report arrives when the controller type is already known to be a guitar,
                               it can mean one of two things:
                               1. The guitar was unplugged from the Wiimote (extension controller bit is 0). In this case we have to respond to this report
                                  like we normally would (perform steps to determine the updated controller type - Wiimote).
                               2. Normally, the Nyko Frontman is recognized as a guitar, then we request the Wiimote to stream data periodically, then it sends reports
                                  where all extension bytes are 0x00, prompting us to setup guitar encryption. But sometimes it sends reports where all extension bytes
                                  are 0xFF, and a few seconds later a status report with extension controller bit = 1 is received. Repeating the extension discovery process
                                  WITHOUT INTERRUPTIONS has turned out to solve this issue.
                            */
                            if (_statusType == StatusType.Unknown && _currentType == ControllerType.Guitar)
                            {
                                IsEncryptionEnabled = false; // in either case we start over, so no need to keep decrypting guitar bytes in data streams
                                if ((report[3] & 0x02) != 0)
                                {
                                    // in the second case, we don't ignore the current report but we make sure to ignore subsequent reports by setting _state to null
                                    _state = null;
                                }
                            }

                            // Battery Level
                            _battery = report[6];
                            bool lowBattery = (report[3] & 0x01) != 0;
                            
                            if (lowBattery && !_batteryLow)
                            {
                                //LowBattery(this, BatteryStatus.VeryLow);
                                LowBattery(this, new LowBatteryEventArgs(BatteryStatus.VeryLow));
                            }

                            // LED
                            // TODO: LED Check - we probably don't want this one
                            //_led1 = (report[3] & 0x10) != 0;
                            //_led2 = (report[3] & 0x20) != 0;
                            //_led3 = (report[3] & 0x40) != 0;
                            //_led4 = (report[3] & 0x80) != 0;

                            // Extension/Type
                            // Not relyable for Pro Controller U
                            bool ext = (report[3] & 0x02) != 0;
                            if (ext || true)
                            {
                                //lock (_readingObj)
                                //{
                                    _readType = ReadReportType.Extension_A;
                                    ReadMemory(Constants.REGISTER_EXTENSION_TYPE_2, 1);
                                // 16-04-A4-00-F0-01-55-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00
                                // 16-04-A4-00-FB-01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00
                                //}
                            }
                            else if (_currentType != ControllerType.Wiimote)
                            {
                                _currentType = ControllerType.Wiimote;
                                _state = new Wiimote();
                                if (_calibrations.WiimoteCalibration.CalibrationEmpty)
                                {
                                    _state.SetCalibration(Calibrations.CalibrationPreset.Default);
                                }
                                else
                                {
                                    _state.SetCalibration(_calibrations.WiimoteCalibration);
                                }
                                _state.Update(report);

                                // and Fire Event
                                //ExtensionChange(this, _currentType);
                                ExtensionChange(this, new NintrollerExtensionEventArgs(_currentType));

                                // and set report
                                SetReportType(InputReport.BtnsAccIR);
                            }
                            break;
                    }
                    _statusType = StatusType.Unknown; // all status reports after the first are classified as unknown
                    #endregion
                    break;

                case InputReport.ReadMem:
                    #region Parse ReadMem
                    Log("Read Memory Report | " + _readType.ToString());

                    bool noError = (report[3] & 0xF) == 0;
                    if (!noError)
                        Log("Possible ReadMem Error: " + (report[3] & 0x0F).ToString());

                    switch(_readType)
                    {
                        case ReadReportType.Extension_A:
                            // Initialize
                            lock (_readingObj)
                            {
                                // TODO: Can report[0] ever be equal to 0x04 here? Considering it has to be 0x21 to get here...
                                if (report[0] != 0x04)
                                {
                                    // second write might not be necessary according to lines 47-48 here: http://www.nerdkits.com/forum/thread/972/#:~:text=//**Above%20comment%20direct%20from%20wiibrew
                                    WriteToMemory(Constants.REGISTER_EXTENSION_INIT_1, StaticBuffers.ExtensionInit1);
                                    WriteToMemory(Constants.REGISTER_EXTENSION_INIT_2, StaticBuffers.ExtensionInit2);
                                }
                            }

                            _readType = ReadReportType.Extension_B;
                            ReadMemory(Constants.REGISTER_EXTENSION_TYPE, 6);
                            break;

                        case ReadReportType.Extension_B:
                            _readType = ReadReportType.Unknown;
                            if (report.Length < 6)
                            {
                                Log("Report length not long enough for Extension_B");
                                return;
                            }

                            byte[] r = new byte[6];
                            Array.Copy(report, 6, r, 0, 6);

                            long type = 
                                ((long)r[0] << 40) | 
                                ((long)r[1] << 32) | 
                                ((long)r[2] << 24) | 
                                ((long)r[3] << 16) | 
                                ((long)r[4] <<  8) | r[5];

                            bool typeChange = false;
                            ControllerType newType = ControllerType.PartiallyInserted;

                            if (_currentType != (ControllerType)type || _state == null)
                            {
                                typeChange = true;
                                newType = (ControllerType)type;
                            }
                            else if (_forceType != ControllerType.Unknown &&
                                     _forceType != ControllerType.PartiallyInserted &&
                                     _currentType != _forceType)
                            {
                                typeChange = true;
                                newType = _forceType;
                            }

                            if (typeChange)
                            {
                                Log("Controller type: " + newType.ToString());
                                // TODO: New: Check parsing after applying a report type (Pro is working, CC is not)
                                InputReport applyReport = InputReport.BtnsOnly;

//#if LOW_BANDWIDTH
//                                bool continuousReporting = false;
//#else
//                                bool continuousReporting = true;
//#endif

                                bool continuousReporting = true;

                                switch (newType)
                                {
                                    /* TODO: A Wiimote with no extension shouldn't actually be identified by the value 0x000000000000,
                                       but rather by whether or not error 7 is received when reading the expansion type, according to this paragraph:
                                       https://wiibrew.org/wiki/Wiimote/Extension_Controllers#The_New_Way:~:text=Contrary%20to%20previous%20documentation
                                    */
                                    case ControllerType.Wiimote:
                                        _state = new Wiimote();
                                        if (_calibrations.WiimoteCalibration.CalibrationEmpty)
                                        {
                                            _state.SetCalibration(Calibrations.CalibrationPreset.Default);
                                        }
                                        else
                                        {
                                            _state.SetCalibration(_calibrations.WiimoteCalibration);
                                        }
                                        applyReport = InputReport.BtnsAccIR;
                                        _irMode = IRCamMode.Basic;
                                        EnableIR();
                                        break;

                                    case ControllerType.ProController:
                                        _state = new ProController();
                                        if (_calibrations.ProCalibration.CalibrationEmpty)
                                        {
                                            _state.SetCalibration(Calibrations.CalibrationPreset.Default);
                                        }
                                        else
                                        {
                                            _state.SetCalibration(_calibrations.ProCalibration);
                                        }
                                        applyReport = InputReport.ExtOnly;
                                        break;

                                    case ControllerType.BalanceBoard:
                                        _state = new BalanceBoard();
                                        applyReport = InputReport.ExtOnly;
                                        break;

                                    case ControllerType.Nunchuk:
                                    case ControllerType.NunchukB:
                                        _state = new Nunchuk(_calibrations.WiimoteCalibration);

                                        if (_calibrations.NunchukCalibration.CalibrationEmpty)
                                        {
                                            _state.SetCalibration(Calibrations.CalibrationPreset.Default);
                                        }
                                        else
                                        {
                                            _state.SetCalibration(_calibrations.NunchukCalibration);
                                        }

                                        if (_irMode == IRCamMode.Off)
                                        {
                                            applyReport = InputReport.BtnsAccExt;
                                        }
                                        else
                                        {
                                            applyReport = InputReport.BtnsAccIRExt;
                                        }
                                        break;

                                    case ControllerType.ClassicController:
                                        _state = new ClassicController(_calibrations.WiimoteCalibration);

                                        if (_calibrations.ClassicCalibration.CalibrationEmpty)
                                        {
                                            _state.SetCalibration(Calibrations.CalibrationPreset.Default);
                                        }
                                        else
                                        {
                                            _state.SetCalibration(_calibrations.ClassicCalibration);
                                        }

                                        if (_irMode == IRCamMode.Off)
                                        {
                                            applyReport = InputReport.BtnsExt;
                                        }
                                        else
                                        {
                                            applyReport = InputReport.BtnsAccIRExt;
                                        }
                                        break;

                                    case ControllerType.ClassicControllerPro:
                                        _state = new ClassicControllerPro(_calibrations.WiimoteCalibration);

                                        if (_calibrations.ClassicProCalibration.CalibrationEmpty)
                                        {
                                            _state.SetCalibration(Calibrations.CalibrationPreset.Default);
                                        }
                                        else
                                        {
                                            _state.SetCalibration(_calibrations.ClassicProCalibration);
                                        }

                                        if (_irMode == IRCamMode.Off)
                                        {
                                            applyReport = InputReport.BtnsAccExt;
                                        }
                                        else
                                        {
                                            applyReport = InputReport.BtnsAccIRExt;
                                        }
                                        break;

                                    case ControllerType.Guitar:
                                        _state = new Guitar(wm: _calibrations.WiimoteCalibration) {
                                            CALIB_Enable_Joystick = IsGuitarJoystickEnabled,
                                            CALIB_Enable_TouchStrip = IsGuitarJoystickEnabled,
                                        };

                                        if (_calibrations.ClassicProCalibration.CalibrationEmpty)
                                        {
                                            _state.SetCalibration(Calibrations.CalibrationPreset.None);
                                        }
                                        else
                                        {
                                            _state.SetCalibration(_calibrations.ClassicProCalibration);
                                        }
#if LOW_BANDWIDTH
                                        applyReport = InputReport.BtnsExt;
#else
                                        applyReport = InputReport.BtnsAccExt;
#endif
                                        break;

                                    case ControllerType.Turntable:
                                        _state = new Turntable(_calibrations.WiimoteCalibration);

                                        if (_calibrations.ClassicProCalibration.CalibrationEmpty)
                                        {
                                            _state.SetCalibration(Calibrations.CalibrationPreset.None);
                                        }
                                        else
                                        {
                                            _state.SetCalibration(_calibrations.TurntableCalibration);
                                        }

                                        applyReport = InputReport.BtnsAccExt;

                                        break;

                                    case ControllerType.MotionPlus:
                                        _state = new WiimotePlus();
                                        // TODO: Calibration: apply stored motion plus calibration
                                        if (_irMode == IRCamMode.Off)
                                        {
                                            applyReport = InputReport.BtnsAccExt;
                                        }
                                        else
                                        {
                                            applyReport = InputReport.BtnsAccIRExt;
                                        }
                                        break;

                                    case ControllerType.PartiallyInserted:
                                        // try again
                                        // TODO: New: Make sure this works
                                        GetStatus();
                                        return;
                                        //break;

                                    case ControllerType.Drums:
                                        _state = new Drums(_calibrations.WiimoteCalibration);

                                        if (_calibrations.ClassicProCalibration.CalibrationEmpty) {
                                          _state.SetCalibration(Calibrations.CalibrationPreset.None);
                                        } else {
                                          _state.SetCalibration(_calibrations.ClassicProCalibration);
                                        }
                    
                                        #if LOW_BANDWIDTH
                                            applyReport = InputReport.BtnsIRExt;
                                        #else
                                            applyReport = InputReport.BtnsAccIRExt;
                                        #endif
                                        break;

                                    case ControllerType.TaikoDrum:
                                        // TODO: New: Musicals
                                        break;

                                    default:
                                        Log("Unhandled controller type");
                                        break;
                                }

                                _currentType = newType;

                                // TODO: Get calibration if PID != 330

                                //_state.SetCalibration(Calibrations.CalibrationPreset.Default);

                                // Fire ExtensionChange event
                                //ExtensionChange(this, _currentType);
                                ExtensionChange(this, new NintrollerExtensionEventArgs(_currentType));
                                
                                // set Report
                                ApplyReportingType(applyReport, continuousReporting);
                            }
                            break;

                        default:
                            Log("Unrecognized Read Memory report");
                            break;
                    }
#endregion
                    break;

                case InputReport.Acknowledge:
                    #region Parse Acknowledgement
                    Log("Output Acknowledged");

                    if (report[4] == 0x03)
                    {
                        Log("Possible Error with Operation");
                        return;
                    }

                    /* If one of the memory writes as part of encryption setup is acknowledged with a non-zero error code (meaning the write failed, which rarely happens),
                       it happens because a status report claiming no extension is connected was received just before and was (rightfully) ignored.
                       Based on observations with the Nyko Frontman, the fastest way to recover from this is to abort the encryption setup, wait for
                       the next status report (which will have extension controller bit = 1) and start over.
                    */
                    if (_ackType.ToString().StartsWith("EncryptionSetup") && report[4] != 0x00)
                    {
                        _ackType = (AcknowledgementType)((int)_ackType - 1); // the current _ackType is already the step following the one that failed, so we take it back
                        Log(_ackType.ToString() + " failed with error code " + report[4].ToString() + ". Aborting");
                        _ackType = AcknowledgementType.NA;
                        break;

                        // the original reaction was to retry the write that failed, but it just kept failing...
                        //_ackType = (AcknowledgementType)((int)_ackType - 1);
                        //Log(_ackType.ToString() + " failed with error code " + report[4].ToString() + ", retrying");
                    }

                    switch (_ackType)
                    {
                        case AcknowledgementType.NA:
                            #region Default Acknowledgement
                            Log("Acknowledgement Report");
                            // Core buttons can be parsed here
                            // 20 BB BB LF 00 00 VV
                            // 20 = Acknowledgement Report
                            // BB BB = Core Buttons
                            // LF = LED Status & Flags
                            //     0x01 = Battery very low
                            //     0x02 = Extension connected
                            //     0x04 = Speaker enabled
                            //     0x08 = IR camera enabled
                            //     0x10 = LED 1
                            //     0x20 = LED 2
                            //     0x40 = LED 3
                            //     0x80 = LED 4
                            // VV = current battery level

                            // Gather Flags
                            _batteryLow    = (report[3] & 0x01) == 1;
                            bool extension = (report[3] & 0x02) == 1;
                            bool speaker   = (report[3] & 0x04) == 1;
                            bool irOn      = (report[3] & 0x08) == 1;
                            
                            // Gather LEDs
                            // TODO: LED Check - we may want this one
                            //_led1 = (report[3] & 0x10) == 1;
                            //_led2 = (report[3] & 0x20) == 1;
                            //_led3 = (report[3] & 0x40) == 1;
                            //_led4 = (report[3] & 0x80) == 1;

                            //if (extension)
                            //{
                            //    _readType = ReadReportType.Extension_A;
                            //    ReadMemory(Constants.REGISTER_EXTENSION_TYPE_2, 1);
                            //}
                            //else if (_currentType != ControllerType.Wiimote)
                            //{
                            //    _currentType = ControllerType.Wiimote;
                            //    _state = new Wiimote();
                            //    _state.Update(report);
                            //
                            //    // Fire event
                            //    ExtensionChange(this, new NintrollerExtensionEventArgs(_currentType));
                            //
                            //    // and set report
                            //    SetReportType(InputReport.BtnsAccIR);
                            //}
                            #endregion
                            break;

                        case AcknowledgementType.IR_Step1:
                        #region IR Step 1
                            byte[] sensitivityBlock1 = null;

                            switch (_irSensitivity)
                            {
                                case IRCamSensitivity.Custom:
                                    sensitivityBlock1 = StaticBuffers.SensitivityBlock1_Custom;
                                    break;

                                case IRCamSensitivity.CustomHigh:
                                    sensitivityBlock1 = StaticBuffers.SensitivityBlock1_CustomHigh;
                                    break;

                                case IRCamSensitivity.CustomMax:
                                    sensitivityBlock1 = StaticBuffers.SensitivityBlock1_CustomMax;
                                    break;

                                case IRCamSensitivity.Level1:
                                    sensitivityBlock1 = StaticBuffers.SensitivityBlock1_Level1;
                                    break;
                                    
                                case IRCamSensitivity.Level2:
                                    sensitivityBlock1 = StaticBuffers.SensitivityBlock1_Level2;
                                    break;

                                case IRCamSensitivity.Level4:
                                    sensitivityBlock1 = StaticBuffers.SensitivityBlock1_Level4;
                                    break;

                                case IRCamSensitivity.Level5:
                                    sensitivityBlock1 = StaticBuffers.SensitivityBlock1_Level5;
                                    break;

                                case IRCamSensitivity.Level3:
                                default:
                                    sensitivityBlock1 = StaticBuffers.SensitivityBlock1_Level3;
                                    break;
                            }

                            _ackType = AcknowledgementType.IR_Step2;
                            WriteToMemory(Constants.REGISTER_IR_SENSITIVITY_1, sensitivityBlock1);
                            #endregion
                            break;

                        case AcknowledgementType.IR_Step2:
                            #region IR Step 2
                            byte[] sensitivityBlock2 = null;
                            
                            switch (_irSensitivity)
                            {
                                case IRCamSensitivity.Custom:
                                    sensitivityBlock2 = StaticBuffers.SensitivityBlock2_Custom;
                                    break;

                                case IRCamSensitivity.CustomHigh:
                                    sensitivityBlock2 = StaticBuffers.SensitivityBlock2_CustomHigh;
                                    break;

                                case IRCamSensitivity.CustomMax:
                                    sensitivityBlock2 = StaticBuffers.SensitivityBlock2_CustomMax;
                                    break;

                                case IRCamSensitivity.Level1:
                                    sensitivityBlock2 = StaticBuffers.SensitivityBlock2_Level1;
                                    break;
                                    
                                case IRCamSensitivity.Level2:
                                    sensitivityBlock2 = StaticBuffers.SensitivityBlock2_Level2;
                                    break;

                                case IRCamSensitivity.Level4:
                                    sensitivityBlock2 = StaticBuffers.SensitivityBlock2_Level4;
                                    break;

                                case IRCamSensitivity.Level5:
                                    sensitivityBlock2 = StaticBuffers.SensitivityBlock2_Level5;
                                    break;

                                case IRCamSensitivity.Level3:
                                default:
                                    sensitivityBlock2 = StaticBuffers.SensitivityBlock2_Level3;
                                    break;
                            }

                            _ackType = AcknowledgementType.IR_Step3;
                            WriteToMemory(Constants.REGISTER_IR_SENSITIVITY_2, sensitivityBlock2);
                            #endregion
                            break;

                        case AcknowledgementType.IR_Step3:
                            _ackType = AcknowledgementType.IR_Step4;
                            WriteToMemory(Constants.REGISTER_IR_MODE, new byte[] { (byte)_irMode });
                            break;

                        case AcknowledgementType.IR_Step4:
                            _ackType = AcknowledgementType.IR_Step5;
                            WriteToMemory(Constants.REGISTER_IR, StaticBuffers.SensitivityBlock4);
                            break;

                        case AcknowledgementType.IR_Step5:
                            #region Final IR Step
                            Log("IR Camera Enabled");
                            _ackType = AcknowledgementType.NA;

                            switch (_irMode)
                            {
                                case IRCamMode.Off:
                                    SetReportType(InputReport.BtnsAccExt);
                                    break;

                                case IRCamMode.Basic:
                                    SetReportType(InputReport.BtnsAccIRExt);
                                    break;

                                case IRCamMode.Wide:
                                    SetReportType(InputReport.BtnsAccIR);
                                    break;

                                case IRCamMode.Full:
                                    // not a supported report type right now
                                    SetReportType(InputReport.BtnsIRExt);
                                    break;
                            }
                            #endregion
                            break;

                        // The encryption key used is 16 zero bytes, since it makes decryption easier as seen in GuitarDecryptBuffer()
                        case AcknowledgementType.EncryptionSetup_Step1:
                            _ackType = AcknowledgementType.EncryptionSetup_Step2;
                            WriteToMemory(0x04a40040, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }); // writing first 6-byte block of the encryption key
                            break;
                        case AcknowledgementType.EncryptionSetup_Step2:
                            _ackType = AcknowledgementType.EncryptionSetup_Step3;
                            WriteToMemory(0x04a40046, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }); // writing second 6-byte block of the encryption key
                            break;
                        case AcknowledgementType.EncryptionSetup_Step3:
                            _ackType = AcknowledgementType.EncryptionSetup_Step4;
                            WriteToMemory(0x04a4004C, new byte[] { 0x00, 0x00, 0x00, 0x00 }); // writing last 4 bytes of the encryption key
                            break;
                        case AcknowledgementType.EncryptionSetup_Step4:
                            _ackType = AcknowledgementType.NA;
                            IsEncryptionEnabled = true; // having completed encryption setup, we turn on the flag to start decrypting guitar bytes in the incoming data reports
                            ApplyReportingType(InputReport.BtnsAccIRExt, true); // requesting the Wiimote to stream 0x37 data reports (the report type observed in GH3 Bluetooth traffic)
                            break;

                        default:
                            Log("Unhandled acknowledgement");
                            _ackType = AcknowledgementType.NA;
                            break;
                    }
                    #endregion
                    break;
                #endregion

                #region Data Reports
                case InputReport.BtnsOnly:
                case InputReport.BtnsAcc:
                case InputReport.BtnsExt:
                case InputReport.BtnsAccIR:
                case InputReport.BtnsExtB:
                case InputReport.BtnsAccExt:
                case InputReport.BtnsIRExt:
                case InputReport.BtnsAccIRExt:
                case InputReport.ExtOnly:
                    if (_state != null && _currentType != ControllerType.Unknown)
                    {
                        // ignoring input reports arriving while guitar encryption is being set up
                        if (_ackType.ToString().StartsWith("EncryptionSetup"))
                        {
                            break;
                        }

                        if (_currentType == ControllerType.Guitar)
                        {
                            // use the appropriate offset where extension bytes begin, based on the data report type
                            int offset;
                            switch (input)
                            {
                                case InputReport.BtnsExt:
                                case InputReport.BtnsExtB:
                                    offset = 3;
                                    break;
                                case InputReport.BtnsAccExt:
                                    offset = 6;
                                    break;
                                case InputReport.BtnsIRExt:
                                    offset = 13;
                                    break;
                                case InputReport.BtnsAccIRExt:
                                    offset = 16;
                                    break;
                                case InputReport.ExtOnly:
                                    offset = 1;
                                    break;
                                default:
                                    return;
                            }
                            if (_encryptionEnabled)
                            {
                                GuitarDecryptBuffer(report, offset); // modify the report to contain decrypted guitar bytes
                            }
                            else if (BitConverter.ToString(report, offset, 6) == "00-00-00-00-00-00") // checking this only if _encryptionEnabled=false
                            {
                                /* ASSUMPTION: if all guitar bytes are zero, it means the guitar will send its real data only after setting up encryption
                                   as described here: https://wiibrew.org/wiki/Wiimote/Extension_Controllers#Encryption_setup
                                   This conclusion was reached by sniffing Bluetooth traffic while playing Guitar Hero 3 on the Dolphin emulator with
                                   the Nyko Frontman guitar (where it worked just fine, hence why the capture was performed - to find out how).
                                   Note: the real guitar data can never be 6 zero bytes, based on the known format (which applies here since the received report
                                   is not encrypted): https://wiibrew.org/wiki/Wiimote/Extension_Controllers/Guitar_Hero_(Wii)_Guitars#Data_Format
                                */
                                _ackType = AcknowledgementType.EncryptionSetup_Step1;
                                WriteToMemory(Constants.REGISTER_EXTENSION_INIT_1, new byte[] { 0xAA }); // step 0 - write 0xAA to register 0xF0
                                return;
                                // continue other steps in Acknowledgement Reporting
                            }
                        }

                        _state.Update(report);
                        var arg = new NintrollerStateEventArgs(_currentType, _state, BatteryLevel);

                        try
                        {
                            StateUpdate(this, arg);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("State Update Exception: " + ex.ToString());
                        }
                    }
                    else if (_statusType != StatusType.DiscoverExtension &&
                            _readType != ReadReportType.Extension_A &&
                            _readType != ReadReportType.Extension_B)
                    {
                        Log("State is null! Requesting status");
                        _currentType = ControllerType.Unknown;
                        _statusType = StatusType.DiscoverExtension;
                        GetStatus();
                    }
                    break;
                #endregion

                default:
                    Log("Unexpected Report type: " + input.ToString("x"));
                    break;
            }
        }

        #endregion

        #region General

        internal static float Normalize(int raw, int min, int center, int max)
        {
            if (raw == center) return 0;

            float actual = raw - center;
            float range = 0;

            if (raw > center)
            {
                range = max - center;
            }
            else
            {
                range = center - min;
            }

            if (range == 0) return 0;

            return actual / range;
        }

        internal static float Normalize(int raw, int min, int center, int max, int dead)
        {
            float actual = 0;
            float range = 0;

            if (Math.Abs(center - raw) <= dead)
            {
                return 0f;
            }
            else
            {
                if (raw > center)
                {
                    actual = raw - (center + dead);
                    range = max - (center + dead);
                }
                else if (raw < center)
                {
                    actual = raw - (center - dead);
                    range = (center - dead) - min;
                }
            }

            if (range == 0)
                return 0f;

            return actual / range;
        }

        internal static float Normalize(int raw, int min, int center, int max, int deadP, int deadN)
        {
            float actual = 0;
            float range = 0;
            
            if (raw - center <= deadP && raw - center >= deadN)
            {
                return 0f;
            }
            else
            {
                if (raw > center)
                {
                    actual = raw - (center + deadP);
                    range = max - (center + deadP);
                }
                else if (raw < center)
                {
                    actual = raw - deadN - center;
                    range = (center + deadN) - min;
                }
            }

            if (range == 0)
                return 0f;

            return actual / range;
        }

        internal static float Normalize(float raw, float min, float center, float max, float dead)
        {
            float availableRange = 0f;
            float actualValue = 0f;

            if (Math.Abs(center - raw) < dead)
            {
                return 0f;
            }
            else if (raw - center > 0)
            {
                availableRange = max - (center + dead);
                actualValue = raw - (center + dead);

                return (actualValue / availableRange);
            }
            else
            {
                availableRange = center - dead - min;
                actualValue = raw - center;

                return (actualValue / availableRange) - 1f;
            }
        }

        private void EnableIR()
        {
            byte[] buffer = new byte[2];

            buffer[0] = (byte)OutputReport.IREnable;
            buffer[1] = (byte)(0x04);
            SendData(buffer);

            buffer[0] = (byte)OutputReport.IREnable2;
            buffer[1] = (byte)(0x04);
            SendData(buffer);

            _ackType = AcknowledgementType.IR_Step1;
            WriteToMemory(Constants.REGISTER_IR, StaticBuffers.SensitivityBlockInit);
            // continue other steps in Acknowledgement Reporting
        }

        private void DisableIR()
        {
            byte[] buffer = new byte[2];

            buffer[0] = (byte)OutputReport.IREnable;
            buffer[1] = (byte)(0x00);
            SendData(buffer);

            buffer[0] = (byte)OutputReport.IREnable2;
            buffer[1] = (byte)(0x00);
            SendData(buffer);

            // TODO: New: Check if we need to monitor the acknowledgment report
        }

        private void StartMotionPlus()
        {
            // TODO: New: Motion Plus
            // determine if we need to pass through Nunchuck or Classic Controller
            //WriteByte(Constants.REGISTER_MOTIONPLUS_INIT, 0x04);
            //WriteToMemory(Constants.REGISTER_MOTIONPLUS_INIT, StaticBuffers.MotionPlusInit);
        }

        /// <summary>
        /// Sets the LEDs to a reversed binary display.
        /// </summary>
        /// <param name="bin">Decimal binary value to use (0 - 15).</param>
        public void SetBinaryLEDs(int bin)
        {
            _led1 = (bin & 0x01) > 0;
            _led2 = (bin & 0x02) > 0;
            _led3 = (bin & 0x04) > 0;
            _led4 = (bin & 0x08) > 0;

            ApplyLEDs();
        }

        /// <summary>
        /// Sets the LEDs to correspond with the player number.
        /// (e.g. 1 = 1st LED &amp; 4 = 4th LED)
        /// </summary>
        /// <param name="num">Player LED to set (0 - 15)</param>
        public void SetPlayerLED(int num)
        {
            // 1st LED
            if (num == 1 || num == 5 || num == 8 || num == 10 || num == 11 || num > 12)
                _led1 = num == 1 || num == 5 || num == 8 || num == 10 || num == 11 || num > 12;
            else
                _led1 = false;

            // 2nd LED
            if (num == 2 || num == 5 || num == 6 || num == 9 || num == 11 || num == 12 || num > 13)
                _led2 = true;
            else
                _led2 = false;

            // 3rd LED
            if (num == 3 || num == 6 || num == 7 || num == 8 || num == 11 || num == 12 || num == 13 || num == 15)
                _led3 = true;
            else
                _led3 = false;

            // 4th LED
            if (num == 4 || num == 7 || num == 9 || num == 10 || num > 11)
                _led4 = true;
            else
                _led4 = false;

            ApplyLEDs();
        }

        public void SetPlayerLED(bool led1, bool led2, bool led3, bool led4) {
            _led1 = led1;
            _led2 = led2;
            _led3 = led3;
            _led4 = led4;

            ApplyLEDs();
        }

        /// <summary>
        /// Forces the controller to be read as the provided type.
        /// </summary>
        /// <param name="type">Type to be parsed as. Setting it to Unknown or Partially Inserted clears it.</param>
        public void ForceControllerType(ControllerType type)
        {
            _forceType = type;

            if (_connected)
            {
                GetStatus();
            }
        }

        /// <summary>
        /// Stop reading from the device
        /// </summary>
        public void StopReading()
        {
            _reading = false;
            _connected = false;
            _ackType = AcknowledgementType.NA;
            _statusType = StatusType.Unknown;
            _readType = ReadReportType.Unknown;
        }

        #endregion

        #region Calibration

        /// <summary>
        /// Sets the device's calibrations based on a preset.
        /// </summary>
        /// <param name="preset">Preset to be used.</param>
        public void SetCalibration(Calibrations.CalibrationPreset preset)
        {
            if (_state != null)
                _state.SetCalibration(preset);

            if (_calibrations != null)
                _calibrations.SetCalibrations(preset);
        }
        /// <summary>
        /// Sets the device's calibrations based on a string.
        /// </summary>
        /// <param name="calibrationStorageString">Calibration storage string to use.</param>
        public void SetCalibration(string calibrationStorageString)
        {
            _calibrations.SetCalibrations(calibrationStorageString);

            // TODO: apply 
        }
        /// <summary>
        /// Sets the controller calibration for the Wiimote
        /// </summary>
        /// <param name="wiimoteCalibration">The Wiimote Struct with the calibration values to use</param>
        public void SetCalibration(Wiimote wiimoteCalibration)
        {
            _calibrations.WiimoteCalibration = wiimoteCalibration;

            if (_state != null &&(
                _currentType == ControllerType.Wiimote ||
                _currentType == ControllerType.Nunchuk ||
                _currentType == ControllerType.NunchukB ||
                _currentType == ControllerType.ClassicController ||
                _currentType == ControllerType.ClassicControllerPro ||
                _currentType == ControllerType.Guitar ||
                _currentType == ControllerType.Turntable))
            {
                _state.SetCalibration(wiimoteCalibration);
            }
        }
        /// <summary>
        /// Sets the controller calibration for the Nunchuk
        /// </summary>
        /// <param name="nunchukCalibration">The Nunchuk Struct with the calibration values to use</param>
        public void SetCalibration(Nunchuk nunchukCalibration)
        {
            _calibrations.NunchukCalibration = nunchukCalibration;

            if (_currentType == ControllerType.Nunchuk || _currentType == ControllerType.NunchukB)
            {
                _state.SetCalibration(nunchukCalibration);
            }
        }
        /// <summary>
        /// Sets the controller calibration for the Classic Controller
        /// </summary>
        /// <param name="classicCalibration">The ClassicController Struct with the calibration values to use</param>
        public void SetCalibration(ClassicController classicCalibration)
        {
            _calibrations.ClassicCalibration = classicCalibration;

            if (_currentType == ControllerType.ClassicController)
            {
                _state.SetCalibration(classicCalibration);
            }
        }
        /// <summary>
        /// Sets the controller calibration for the Classic Controller Pro
        /// </summary>
        /// <param name="classicProCalibration">The ClassicControllerPro Struct with the calibration values to use</param>
        public void SetCalibration(ClassicControllerPro classicProCalibration)
        {
            _calibrations.ClassicProCalibration = classicProCalibration;

            if (_currentType == ControllerType.ClassicControllerPro)
            {
                _state.SetCalibration(classicProCalibration);
            }
        }
        /// <summary>
        /// Sets the controller calibration for the Pro Controller
        /// </summary>
        /// <param name="proCalibration">The ProController Struct with the calibration values to use</param>
        public void SetCalibration(ProController proCalibration)
        {
            _calibrations.ProCalibration = proCalibration;

            if (_state != null && _currentType == ControllerType.ProController)
            {
                _state.SetCalibration(proCalibration);
            }
        }
        /// <summary>
        /// Sets the controller calibration for the Turntable
        /// </summary>
        /// <param name="turntableCalibration">The Turntable Struct with the calibration values to use</param>
        public void SetCalibration(Turntable turntableCalibration)
        {
            _calibrations.TurntableCalibration = turntableCalibration;

            if (_state != null && _currentType == ControllerType.Turntable)
            {
                _state.SetCalibration(turntableCalibration);
            }
        }

        #endregion
    }

    #region New Event Args

    /// <summary>
    /// Class for controller state update.
    /// </summary>
    public class NintrollerStateEventArgs : EventArgs
    {
        /// <summary>
        /// The controller type being updated.
        /// </summary>
        public ControllerType controllerType;
        /// <summary>
        /// The controller's updated state.
        /// </summary>
        public INintrollerState state;
        /// <summary>
        /// The controller's last known battery level.
        /// </summary>
        public BatteryStatus batteryLevel;
        
        /// <summary>
        /// Create the event argument with provided parameters.
        /// </summary>
        /// <param name="type">The controller type.</param>
        /// <param name="newState">The updated controller state.</param>
        /// <param name="battery">The controller's last known battery level.</param>
        public NintrollerStateEventArgs(ControllerType type, INintrollerState newState, BatteryStatus battery)
        {
            controllerType = type;
            state          = newState;
            batteryLevel   = battery;
        }
    }

    /// <summary>
    /// Class for extension change event.
    /// </summary>
    public class NintrollerExtensionEventArgs : EventArgs
    {
        /// <summary>
        /// The updated controller type.
        /// </summary>
        public ControllerType controllerType;

        /// <summary>
        /// Create an instance with the provided new type.
        /// </summary>
        /// <param name="type">The new type of the controller</param>
        public NintrollerExtensionEventArgs(ControllerType type)
        {
            controllerType = type;
        }
    }

    /// <summary>
    /// Class for low battery event.
    /// </summary>
    public class LowBatteryEventArgs : EventArgs
    {
        /// <summary>
        /// The current battery level of the controller.
        /// </summary>
        public BatteryStatus batteryLevel;

        /// <summary>
        /// Create an instance with the changed battery level.
        /// </summary>
        /// <param name="level">Current battery level of the controller.</param>
        public LowBatteryEventArgs(BatteryStatus level)
        {
            batteryLevel = level;
        }
    }

    /// <summary>
    /// Class for disconnected event.
    /// </summary>
    public class DisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Exception thrown if applicable.
        /// </summary>
        public Exception error;

        /// <summary>
        /// Creates instance using provided exception.
        /// </summary>
        /// <param name="err">Exception thrown when disconnect detected.</param>
        public DisconnectedEventArgs(Exception err)
        {
            error = err;
        }
    }

    #endregion

}
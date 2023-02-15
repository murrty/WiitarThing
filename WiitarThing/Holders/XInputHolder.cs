using System;
using System.Collections.Generic;
using System.Linq;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using NintrollerLib;

namespace WiitarThing.Holders
{
    public class XInputHolder : Holder
    {
        static internal bool[] availabe = { true, true, true, true };

        internal int minRumble = 20;
        internal int rumbleLeft = 0;
        internal int rumbleDecrement = 10;

        private XBus bus;
        private bool connected;
        private int ID;
        private Dictionary<string, float> writeReport;

        public static Dictionary<string, string> GetDefaultMapping(ControllerType type)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            // TODO: finish default mapping (Acc, IR, Balance Board, ect) (not for 1st release)
            switch (type)
            {
                case ControllerType.ProController:
                    result.Add(ProController.InputNames.A, Inputs.Xbox360.A);
                    result.Add(ProController.InputNames.B, Inputs.Xbox360.B);
                    result.Add(ProController.InputNames.X, Inputs.Xbox360.X);
                    result.Add(ProController.InputNames.Y, Inputs.Xbox360.Y);

                    result.Add(ProController.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(ProController.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(ProController.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(ProController.InputNames.RIGHT, Inputs.Xbox360.RIGHT);

                    result.Add(ProController.InputNames.L, Inputs.Xbox360.LB);
                    result.Add(ProController.InputNames.R, Inputs.Xbox360.RB);
                    result.Add(ProController.InputNames.ZL, Inputs.Xbox360.LT);
                    result.Add(ProController.InputNames.ZR, Inputs.Xbox360.RT);

                    result.Add(ProController.InputNames.LUP, Inputs.Xbox360.LUP);
                    result.Add(ProController.InputNames.LDOWN, Inputs.Xbox360.LDOWN);
                    result.Add(ProController.InputNames.LLEFT, Inputs.Xbox360.LLEFT);
                    result.Add(ProController.InputNames.LRIGHT, Inputs.Xbox360.LRIGHT);

                    result.Add(ProController.InputNames.RUP, Inputs.Xbox360.RUP);
                    result.Add(ProController.InputNames.RDOWN, Inputs.Xbox360.RDOWN);
                    result.Add(ProController.InputNames.RLEFT, Inputs.Xbox360.RLEFT);
                    result.Add(ProController.InputNames.RRIGHT, Inputs.Xbox360.RRIGHT);

                    result.Add(ProController.InputNames.LS, Inputs.Xbox360.LS);
                    result.Add(ProController.InputNames.RS, Inputs.Xbox360.RS);
                    result.Add(ProController.InputNames.SELECT, Inputs.Xbox360.BACK);
                    result.Add(ProController.InputNames.START, Inputs.Xbox360.START);
                    result.Add(ProController.InputNames.HOME, Inputs.Xbox360.GUIDE);
                    break;

                case ControllerType.ClassicControllerPro:
                    result.Add(ClassicControllerPro.InputNames.A, Inputs.Xbox360.A);
                    result.Add(ClassicControllerPro.InputNames.B, Inputs.Xbox360.B);
                    result.Add(ClassicControllerPro.InputNames.X, Inputs.Xbox360.X);
                    result.Add(ClassicControllerPro.InputNames.Y, Inputs.Xbox360.Y);

                    result.Add(ClassicControllerPro.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(ClassicControllerPro.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(ClassicControllerPro.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(ClassicControllerPro.InputNames.RIGHT, Inputs.Xbox360.RIGHT);

                    result.Add(ClassicControllerPro.InputNames.L, Inputs.Xbox360.LB);
                    result.Add(ClassicControllerPro.InputNames.R, Inputs.Xbox360.RB);
                    result.Add(ClassicControllerPro.InputNames.ZL, Inputs.Xbox360.LT);
                    result.Add(ClassicControllerPro.InputNames.ZR, Inputs.Xbox360.RT);

                    result.Add(ClassicControllerPro.InputNames.LUP, Inputs.Xbox360.LUP);
                    result.Add(ClassicControllerPro.InputNames.LDOWN, Inputs.Xbox360.LDOWN);
                    result.Add(ClassicControllerPro.InputNames.LLEFT, Inputs.Xbox360.LLEFT);
                    result.Add(ClassicControllerPro.InputNames.LRIGHT, Inputs.Xbox360.LRIGHT);

                    result.Add(ClassicControllerPro.InputNames.RUP, Inputs.Xbox360.RUP);
                    result.Add(ClassicControllerPro.InputNames.RDOWN, Inputs.Xbox360.RDOWN);
                    result.Add(ClassicControllerPro.InputNames.RLEFT, Inputs.Xbox360.RLEFT);
                    result.Add(ClassicControllerPro.InputNames.RRIGHT, Inputs.Xbox360.RRIGHT);

                    result.Add(ClassicControllerPro.InputNames.SELECT, Inputs.Xbox360.BACK);
                    result.Add(ClassicControllerPro.InputNames.START, Inputs.Xbox360.START);
                    result.Add(ClassicControllerPro.InputNames.HOME, Inputs.Xbox360.GUIDE);

                    result.Add(Wiimote.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(Wiimote.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(Wiimote.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(Wiimote.InputNames.RIGHT, Inputs.Xbox360.RIGHT);
                    result.Add(Wiimote.InputNames.A, Inputs.Xbox360.A);
                    result.Add(Wiimote.InputNames.B, Inputs.Xbox360.B);
                    result.Add(Wiimote.InputNames.ONE, Inputs.Xbox360.LS);
                    result.Add(Wiimote.InputNames.TWO, Inputs.Xbox360.RS);
                    result.Add(Wiimote.InputNames.PLUS, Inputs.Xbox360.BACK);
                    result.Add(Wiimote.InputNames.MINUS, Inputs.Xbox360.START);
                    result.Add(Wiimote.InputNames.HOME, Inputs.Xbox360.GUIDE);
                    result.Add(Wiimote.InputNames.ACC_SHAKE_X, "");
                    result.Add(Wiimote.InputNames.ACC_SHAKE_Y, "");
                    result.Add(Wiimote.InputNames.ACC_SHAKE_Z, "");
                    result.Add(Wiimote.InputNames.TILT_RIGHT, "");
                    result.Add(Wiimote.InputNames.TILT_LEFT, "");
                    result.Add(Wiimote.InputNames.TILT_UP, "");
                    result.Add(Wiimote.InputNames.TILT_DOWN, "");
                    break;

                case ControllerType.ClassicController:
                    result.Add(ClassicController.InputNames.B, Inputs.Xbox360.B);
                    result.Add(ClassicController.InputNames.A, Inputs.Xbox360.A);
                    result.Add(ClassicController.InputNames.Y, Inputs.Xbox360.X);
                    result.Add(ClassicController.InputNames.X, Inputs.Xbox360.Y);

                    result.Add(ClassicController.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(ClassicController.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(ClassicController.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(ClassicController.InputNames.RIGHT, Inputs.Xbox360.RIGHT);

                    result.Add(ClassicController.InputNames.ZL, Inputs.Xbox360.LB);
                    result.Add(ClassicController.InputNames.ZR, Inputs.Xbox360.RB);
                    result.Add(ClassicController.InputNames.LT, Inputs.Xbox360.LT);
                    result.Add(ClassicController.InputNames.RT, Inputs.Xbox360.RT);

                    result.Add(ClassicController.InputNames.LUP, Inputs.Xbox360.LUP);
                    result.Add(ClassicController.InputNames.LDOWN, Inputs.Xbox360.LDOWN);
                    result.Add(ClassicController.InputNames.LLEFT, Inputs.Xbox360.LLEFT);
                    result.Add(ClassicController.InputNames.LRIGHT, Inputs.Xbox360.LRIGHT);

                    result.Add(ClassicController.InputNames.RUP, Inputs.Xbox360.RUP);
                    result.Add(ClassicController.InputNames.RDOWN, Inputs.Xbox360.RDOWN);
                    result.Add(ClassicController.InputNames.RLEFT, Inputs.Xbox360.RLEFT);
                    result.Add(ClassicController.InputNames.RRIGHT, Inputs.Xbox360.RRIGHT);

                    result.Add(ClassicController.InputNames.SELECT, Inputs.Xbox360.BACK);
                    result.Add(ClassicController.InputNames.START, Inputs.Xbox360.START);
                    result.Add(ClassicController.InputNames.HOME, Inputs.Xbox360.GUIDE);

                    result.Add(Wiimote.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(Wiimote.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(Wiimote.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(Wiimote.InputNames.RIGHT, Inputs.Xbox360.RIGHT);
                    result.Add(Wiimote.InputNames.A, Inputs.Xbox360.A);
                    result.Add(Wiimote.InputNames.B, Inputs.Xbox360.B);
                    result.Add(Wiimote.InputNames.ONE, Inputs.Xbox360.LS);
                    result.Add(Wiimote.InputNames.TWO, Inputs.Xbox360.RS);
                    result.Add(Wiimote.InputNames.PLUS, Inputs.Xbox360.BACK);
                    result.Add(Wiimote.InputNames.MINUS, Inputs.Xbox360.START);
                    result.Add(Wiimote.InputNames.HOME, Inputs.Xbox360.GUIDE);
                    result.Add(Wiimote.InputNames.ACC_SHAKE_X, "");
                    result.Add(Wiimote.InputNames.ACC_SHAKE_Y, "");
                    result.Add(Wiimote.InputNames.ACC_SHAKE_Z, "");
                    result.Add(Wiimote.InputNames.TILT_RIGHT, "");
                    result.Add(Wiimote.InputNames.TILT_LEFT, "");
                    result.Add(Wiimote.InputNames.TILT_UP, "");
                    result.Add(Wiimote.InputNames.TILT_DOWN, "");
                    break;

                //case ControllerType.Nunchuk:
                //case ControllerType.NunchukB:
                //    result.Add(Nunchuk.InputNames.UP,    Inputs.Xbox360.LUP);
                //    result.Add(Nunchuk.InputNames.DOWN,  Inputs.Xbox360.LDOWN);
                //    result.Add(Nunchuk.InputNames.LEFT,  Inputs.Xbox360.LLEFT);
                //    result.Add(Nunchuk.InputNames.RIGHT, Inputs.Xbox360.LRIGHT);
                //    result.Add(Nunchuk.InputNames.Z,     Inputs.Xbox360.RT);
                //    result.Add(Nunchuk.InputNames.C,     Inputs.Xbox360.LT);
                //    result.Add(Nunchuk.InputNames.TILT_RIGHT, "");
                //    result.Add(Nunchuk.InputNames.TILT_LEFT, "");
                //    result.Add(Nunchuk.InputNames.TILT_UP, "");
                //    result.Add(Nunchuk.InputNames.TILT_DOWN, "");
                //    result.Add(Nunchuk.InputNames.ACC_SHAKE_X, "");
                //    result.Add(Nunchuk.InputNames.ACC_SHAKE_Y, "");
                //    result.Add(Nunchuk.InputNames.ACC_SHAKE_Z, "");

                //    result.Add(Wiimote.InputNames.UP,    Inputs.Xbox360.UP);
                //    result.Add(Wiimote.InputNames.DOWN,  Inputs.Xbox360.DOWN);
                //    result.Add(Wiimote.InputNames.LEFT,  Inputs.Xbox360.LB);
                //    result.Add(Wiimote.InputNames.RIGHT, Inputs.Xbox360.RB);
                //    result.Add(Wiimote.InputNames.A,     Inputs.Xbox360.A);
                //    result.Add(Wiimote.InputNames.B,     Inputs.Xbox360.B);
                //    result.Add(Wiimote.InputNames.ONE,   Inputs.Xbox360.X);
                //    result.Add(Wiimote.InputNames.TWO,   Inputs.Xbox360.Y);
                //    result.Add(Wiimote.InputNames.PLUS,  Inputs.Xbox360.BACK);
                //    result.Add(Wiimote.InputNames.MINUS, Inputs.Xbox360.START);
                //    result.Add(Wiimote.InputNames.HOME,  Inputs.Xbox360.GUIDE);
                //    result.Add(Wiimote.InputNames.ACC_SHAKE_X, "");
                //    result.Add(Wiimote.InputNames.ACC_SHAKE_Y, "");
                //    result.Add(Wiimote.InputNames.ACC_SHAKE_Z, "");
                //    result.Add(Wiimote.InputNames.TILT_RIGHT, "");
                //    result.Add(Wiimote.InputNames.TILT_LEFT, "");
                //    result.Add(Wiimote.InputNames.TILT_UP, "");
                //    result.Add(Wiimote.InputNames.TILT_DOWN, "");
                //    break;

                case ControllerType.Nunchuk:
                case ControllerType.NunchukB:
                case ControllerType.Wiimote:
                    result.Add(Wiimote.InputNames.RIGHT, Inputs.Xbox360.UP);
                    result.Add(Wiimote.InputNames.LEFT, Inputs.Xbox360.DOWN);

                    result.Add(Wiimote.InputNames.B, Inputs.Xbox360.A); //Green
                    result.Add(Wiimote.InputNames.DOWN, Inputs.Xbox360.B); //Red
                    result.Add(Wiimote.InputNames.A, Inputs.Xbox360.Y); //Yellow
                    result.Add(Wiimote.InputNames.ONE, Inputs.Xbox360.X); //Blue
                    result.Add(Wiimote.InputNames.TWO, Inputs.Xbox360.LB); //Orange

                    result.Add(Wiimote.InputNames.UP, Inputs.Xbox360.BACK); //SP

                    result.Add(Wiimote.InputNames.PLUS, Inputs.Xbox360.START);
                    result.Add(Wiimote.InputNames.MINUS, Inputs.Xbox360.BACK);
                    result.Add(Wiimote.InputNames.HOME, Inputs.Xbox360.GUIDE);

                    //result.Add(Wiimote.InputNames.LEFT,  Inputs.Xbox360.DOWN);






                    result.Add(Wiimote.InputNames.ACC_SHAKE_X, Inputs.Xbox360.RRIGHT);
                    result.Add(Wiimote.InputNames.ACC_SHAKE_Y, Inputs.Xbox360.RRIGHT);
                    result.Add(Wiimote.InputNames.ACC_SHAKE_Z, Inputs.Xbox360.RRIGHT);
                    result.Add(Wiimote.InputNames.TILT_RIGHT, "");
                    result.Add(Wiimote.InputNames.TILT_LEFT, "");
                    result.Add(Wiimote.InputNames.TILT_UP, "");
                    result.Add(Wiimote.InputNames.TILT_DOWN, "");
                    result.Add(Wiimote.InputNames.IR_RIGHT, "");
                    result.Add(Wiimote.InputNames.IR_LEFT, "");
                    result.Add(Wiimote.InputNames.IR_UP, "");
                    result.Add(Wiimote.InputNames.IR_DOWN, "");
                    break;

                case ControllerType.Guitar:

                    result.Add(Guitar.InputNames.G, Inputs.Xbox360.A);
                    result.Add(Guitar.InputNames.R, Inputs.Xbox360.B);
                    result.Add(Guitar.InputNames.Y, Inputs.Xbox360.Y);
                    result.Add(Guitar.InputNames.B, Inputs.Xbox360.X);
                    result.Add(Guitar.InputNames.O, Inputs.Xbox360.LB);

                    result.Add(Guitar.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(Guitar.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(Guitar.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(Guitar.InputNames.RIGHT, Inputs.Xbox360.RIGHT);

                    result.Add(Guitar.InputNames.SELECT, Inputs.Xbox360.BACK);
                    result.Add(Guitar.InputNames.START, Inputs.Xbox360.START);
                    result.Add(Guitar.InputNames.HOME, Inputs.Xbox360.GUIDE);

                    result.Add(Guitar.InputNames.WHAMMYLOW, Inputs.Xbox360.RLEFT);
                    result.Add(Guitar.InputNames.WHAMMYHIGH, Inputs.Xbox360.RRIGHT);

                    result.Add(Guitar.InputNames.TILTLOW, Inputs.Xbox360.RDOWN);
                    result.Add(Guitar.InputNames.TILTHIGH, Inputs.Xbox360.RUP);

                    //result.Add(Wiimote.InputNames.UP, "");
                    //result.Add(Wiimote.InputNames.DOWN, "");
                    //result.Add(Wiimote.InputNames.LEFT, "");
                    //result.Add(Wiimote.InputNames.RIGHT, "");
                    //result.Add(Wiimote.InputNames.A, "");
                    //result.Add(Wiimote.InputNames.B, "");
                    //result.Add(Wiimote.InputNames.ONE, "");
                    //result.Add(Wiimote.InputNames.TWO, "");
                    //result.Add(Wiimote.InputNames.PLUS, "");
                    //result.Add(Wiimote.InputNames.MINUS, "");
                    //result.Add(Wiimote.InputNames.HOME, "");
                    //result.Add(Wiimote.InputNames.ACC_SHAKE_X, "");
                    //result.Add(Wiimote.InputNames.ACC_SHAKE_Y, "");
                    //result.Add(Wiimote.InputNames.ACC_SHAKE_Z, "");
                    //result.Add(Wiimote.InputNames.TILT_RIGHT, "");
                    //result.Add(Wiimote.InputNames.TILT_LEFT, "");
                    //result.Add(Wiimote.InputNames.TILT_UP, "");
                    //result.Add(Wiimote.InputNames.TILT_DOWN, "");
                    //result.Add(Wiimote.InputNames.IR_RIGHT, "");
                    //result.Add(Wiimote.InputNames.IR_LEFT, "");
                    //result.Add(Wiimote.InputNames.IR_UP, "");
                    //result.Add(Wiimote.InputNames.IR_DOWN, "");

                    break;

                case ControllerType.Drums:
                    result.Add(Drums.InputNames.G, Inputs.Xbox360.A);
                    result.Add(Drums.InputNames.R, Inputs.Xbox360.B);
                    result.Add(Drums.InputNames.Y, Inputs.Xbox360.Y);
                    result.Add(Drums.InputNames.B, Inputs.Xbox360.X);
                    result.Add(Drums.InputNames.O, Inputs.Xbox360.RB);
                    result.Add(Drums.InputNames.BASS, Inputs.Xbox360.LB);

                    result.Add(Drums.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(Drums.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(Drums.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(Drums.InputNames.RIGHT, Inputs.Xbox360.RIGHT);

                    result.Add(Drums.InputNames.SELECT, Inputs.Xbox360.BACK);
                    result.Add(Drums.InputNames.START, Inputs.Xbox360.START);
                    result.Add(Drums.InputNames.HOME, Inputs.Xbox360.GUIDE);
                    break;
            }

            return result;
        }

        public XInputHolder()
        {
            //Values = new Dictionary<string, float>();
            Values = new System.Collections.Concurrent.ConcurrentDictionary<string, float>();
            Mappings = new Dictionary<string, string>();
            Flags = new Dictionary<string, bool>();
            ResetReport();

            //if (!Flags.ContainsKey(Inputs.Flags.RUMBLE))
            //{
            //    Flags.Add(Inputs.Flags.RUMBLE, false);
            //}
            //
            //if (!Values.ContainsKey(Inputs.Flags.RUMBLE))
            //{
            //    Values.TryAdd(Inputs.Flags.RUMBLE, 0f);
            //}
        }

        private void ResetReport()
        {
            writeReport = new Dictionary<string, float>()
            {
                {Inputs.Xbox360.A, 0},
                {Inputs.Xbox360.B, 0},
                {Inputs.Xbox360.X, 0},
                {Inputs.Xbox360.Y, 0},
                {Inputs.Xbox360.UP, 0},
                {Inputs.Xbox360.DOWN, 0},
                {Inputs.Xbox360.LEFT, 0},
                {Inputs.Xbox360.RIGHT, 0},
                {Inputs.Xbox360.LB, 0},
                {Inputs.Xbox360.RB, 0},
                {Inputs.Xbox360.BACK, 0},
                {Inputs.Xbox360.START, 0},
                {Inputs.Xbox360.GUIDE, 0},
                {Inputs.Xbox360.LS, 0},
                {Inputs.Xbox360.RS, 0},
            };
        }

        public XInputHolder(ControllerType t) : this()
        {
            Mappings = GetDefaultMapping(t);
        }

        public override void Update()
        {
            if (!connected)
            {
                return;
            }

            var controller = bus.GetController(ID);
            if (controller == null)
            {
                return;
            }

            float LX = 0f;
            float LY = 0f;
            float RX = 0f;
            float RY = 0f;

            float LT = 0f;
            float RT = 0f;

            ResetReport();

            foreach (KeyValuePair<string, string> map in Mappings)
            {
                if (!Values.TryGetValue(map.Key, out float value))
                {
                    continue;
                }

                if (writeReport.ContainsKey(map.Value))
                {
                    writeReport[map.Value] += value;
                }
                else switch (map.Value)
                {
                    case Inputs.Xbox360.LLEFT: LX -= value; break;
                    case Inputs.Xbox360.LRIGHT: LX += value; break;
                    case Inputs.Xbox360.LUP: LY += value; break;
                    case Inputs.Xbox360.LDOWN: LY -= value; break;
                    case Inputs.Xbox360.RLEFT: RX -= value; break;
                    case Inputs.Xbox360.RRIGHT: RX += value; break;
                    case Inputs.Xbox360.RUP: RY += value; break;
                    case Inputs.Xbox360.RDOWN: RY -= value; break;
                    case Inputs.Xbox360.LT: LT += value; break;
                    case Inputs.Xbox360.RT: RT += value; break;
                }
            }

            controller.SetButtonState(Xbox360Button.A, writeReport[Inputs.Xbox360.A] > 0f);
            controller.SetButtonState(Xbox360Button.B, writeReport[Inputs.Xbox360.B] > 0f);
            controller.SetButtonState(Xbox360Button.X, writeReport[Inputs.Xbox360.X] > 0f);
            controller.SetButtonState(Xbox360Button.Y, writeReport[Inputs.Xbox360.Y] > 0f);

            controller.SetButtonState(Xbox360Button.Up, writeReport[Inputs.Xbox360.UP] > 0f);
            controller.SetButtonState(Xbox360Button.Down, writeReport[Inputs.Xbox360.DOWN] > 0f);
            controller.SetButtonState(Xbox360Button.Left, writeReport[Inputs.Xbox360.LEFT] > 0f);
            controller.SetButtonState(Xbox360Button.Right, writeReport[Inputs.Xbox360.RIGHT] > 0f);

            controller.SetButtonState(Xbox360Button.LeftShoulder, writeReport[Inputs.Xbox360.LB] > 0f);
            controller.SetButtonState(Xbox360Button.RightShoulder, writeReport[Inputs.Xbox360.RB] > 0f);
            controller.SetButtonState(Xbox360Button.LeftThumb, writeReport[Inputs.Xbox360.LS] > 0f);
            controller.SetButtonState(Xbox360Button.RightThumb, writeReport[Inputs.Xbox360.RS] > 0f);

            controller.SetButtonState(Xbox360Button.Start, writeReport[Inputs.Xbox360.START] > 0f);
            controller.SetButtonState(Xbox360Button.Back, writeReport[Inputs.Xbox360.BACK] > 0f);
            controller.SetButtonState(Xbox360Button.Guide, writeReport[Inputs.Xbox360.GUIDE] > 0f);
            
            controller.SetAxisValue(Xbox360Axis.LeftThumbX, GetRawAxis(LX));
            controller.SetAxisValue(Xbox360Axis.LeftThumbY, GetRawAxis(LY));
            controller.SetAxisValue(Xbox360Axis.RightThumbX, GetRawAxis(RX));
            controller.SetAxisValue(Xbox360Axis.RightThumbY, GetRawAxis(RY));

            controller.SetSliderValue(Xbox360Slider.LeftTrigger, GetRawTrigger(LT));
            controller.SetSliderValue(Xbox360Slider.RightTrigger, GetRawTrigger(RT));

            controller.SubmitReport();
        }

        private void OnRumble(object sender, Xbox360FeedbackReceivedEventArgs args)
        {
            int strength = (args.LargeMotor << 8) | args.SmallMotor;
            Flags[Inputs.Flags.RUMBLE] = strength > minRumble;
            RumbleAmount = strength > minRumble ? strength : 0;
        }

        public override void Close()
        {
            RemoveXInput(ID);
        }

        public override void AddMapping(ControllerType controller)
        {
            var additional = GetDefaultMapping(controller);

            foreach (KeyValuePair<string, string> map in additional)
            {
                if (!Mappings.ContainsKey(map.Key) && map.Key[0] != 'w')
                {
                    SetMapping(map.Key, map.Value);
                }
            }
        }

        public bool ConnectXInput(int id)
        {
            if (id < 0 || id > 3)
            {
                WiitarDebug.Log($"Attempted to connect invalid user index {id}!");
                return false;
            }

            availabe[id] = false;
            bus = XBus.Default;
            bus.Unplug(id);
            bus.Plugin(id);
            var controller = bus.GetController(id);
            if (controller == null)
            {
                RemoveXInput(id);
                return false;
            }
            controller.FeedbackReceived += OnRumble;

            ID = id;
            connected = true;
            return true;
        }

        public bool RemoveXInput(int id)
        {
            if (id < 0 || id > 3)
            {
                WiitarDebug.Log($"Attempted to remove invalid user index {id}!");
                return false;
            }

            availabe[id] = true;
            Flags[Inputs.Flags.RUMBLE] = false;
            if (bus.Unplug(id))
            {
                ID = -1;
                connected = false;
                return true;
            }

            return false;
        }

        public short GetRawAxis(double axis)
        {
            if (axis > 1.0)
            {
                return 32767;
            }
            if (axis < -1.0)
            {
                return -32767;
            }

            return (short)(axis * 32767);
        }

        public byte GetRawTrigger(double trigger)
        {
            if (trigger > 1.0)
            {
                return 255;
            }
            if (trigger < 0.0)
            {
                return 0;
            }

            return (Byte)(trigger * 255);
        }
    }

    public class XBus
    {
        private static XBus defaultInstance;
        private ViGEmClient viGEmClient;
        private Dictionary<int, IXbox360Controller> targets;
        private List<IXbox360Controller> connected;

        // Default Bus
        public static XBus Default
        {
            get
            {
                // if it hasn't been created create one
                if (defaultInstance == null)
                {
                    defaultInstance = new XBus();
                }

                return defaultInstance;
            }
        }

        public ViGEmClient Client
        {
            get
            {
                return viGEmClient;
            }
            private set
            {
                viGEmClient = value;
            }
        }

        public XBus()
        {
            Client = new ViGEmClient();
            targets = new Dictionary<int, IXbox360Controller>();
            connected = new List<IXbox360Controller>();
            App.Current.Exit += StopDevice;
        }

        private void StopDevice(object sender, System.Windows.ExitEventArgs e)
        {
            if (defaultInstance != null)
            {
                foreach (IXbox360Controller controller in targets.Values)
                {
                    if (connected.Contains(controller))
                    {
                        controller.Disconnect();
                        connected.Remove(controller);
                    }
                }
                Client.Dispose();
            }
        }

        public void Plugin(int id, ushort vid = 0, ushort pid = 0)
        {
            if (targets.ContainsKey(id))
            {
                return;
            }

            IXbox360Controller controller;
            if (vid != 0 && pid != 0)
            {
                controller = Client.CreateXbox360Controller(vid, pid);
            }
            else
            {
                controller = Client.CreateXbox360Controller();
            }

            controller.AutoSubmitReport = false;
            controller.Connect();
            targets.Add(id, controller);
            connected.Add(controller);
        }

        public bool Unplug(int id)
        {
            if (targets.ContainsKey(id) && targets[id] != null)
            {
                if (connected.Contains(targets[id]))
                {
                    targets[id].Disconnect();
                    connected.Remove(targets[id]);
                    targets.Remove(id);
                    return true;
                }
                return false;
            }

            return false;
        }

        public IXbox360Controller GetController(int id)
        {
            if (!targets.TryGetValue(id, out var controller))
            {
                return null;
            }

            return controller;
        }
    }
}

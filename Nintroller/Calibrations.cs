﻿namespace NintrollerLib
{
    /// <summary>
    /// Set of Preset Calibrations
    /// </summary>
    public static class Calibrations
    {
        /// <summary>
        /// Possible pre built calibration types
        /// </summary>
        public enum CalibrationPreset
        {
            /// <summary>
            /// Default (expected) calibration
            /// </summary>
            Default = 0,
            /// <summary>
            /// Calibration with smaller deadzones
            /// </summary>
            Minimum = 1,
            /// <summary>
            /// Calibration with more room for error
            /// </summary>
            Modest  = 2,
            /// <summary>
            /// Calibration with largest deadzones and shorter ranges.
            /// </summary>
            Extra   = 3,
            /// <summary>
            /// No deadzone and uses exact expected range
            /// </summary>
            None    = -1
        }

        #region Properties
        /// <summary>
        /// Calibrations without a deadzone and exact range and center settings.
        /// </summary>
        public static readonly Raw None = new Raw();
        /// <summary>
        /// Calibrations with small deadzones and larger active ranges.
        /// </summary>
        public static readonly Minimal Minimum = new Minimal();
        /// <summary>
        /// Default calibrations with some deadzone.
        /// </summary>
        public static readonly Default Defaults = new Default();
        /// <summary>
        /// Calibrations with larger deadzones and shorter active ranges.
        /// </summary>
        public static readonly Modest Moderate = new Modest();
        /// <summary>
        /// Calibrations with the largest deadzones and shortest active ranges.
        /// </summary>
        public static readonly Extra Extras = new Extra();
        #endregion

        #region Inner Classes (Calibration Types)
        /// <summary>
        /// Class containing all Raw calibrations
        /// </summary>
        public class Raw
        {
            /// <summary>
            /// Raw callibration for the Wii U Pro Controller
            /// </summary>
            public ProController ProControllerRaw = new ProController()
            {
                LJoy = new Joystick()
                {
                    centerX = 2047,
                    minX    = 1023,
                    maxX    = 3071,
                    deadX   = 0,

                    centerY = 2047,
                    minY    = 1023,
                    maxY    = 3071,
                    deadY   = 0
                },
                RJoy = new Joystick()
                {
                    centerX = 2047,
                    minX    = 1023,
                    maxX    = 3071,
                    deadX   = 0,

                    centerY = 2047,
                    minY    = 1023,
                    maxY    = 3071,
                    deadY   = 0
                }
            };

            /// <summary>
            /// Raw calibration for the Wii Remote
            /// </summary>
            public Wiimote WiimoteRaw = new Wiimote()
            {
                accelerometer = new Accelerometer()
                {
                    centerX = 127,
                    minX    = 95,
                    maxX    = 159,
                    deadX   = 0,

                    centerY = 127,
                    minY    = 95,
                    maxY    = 159,
                    deadY   = 0,

                    centerZ = 127,
                    minZ    = 95,
                    maxZ    = 159,
                    deadZ   = 0
                },

                irSensor = new IR()
                {
                    boundingArea = new SquareBoundry()
                    {
                        center_x = 512,
                        center_y = 512,
                        width = 0,
                        height = 0
                    }
                }
            };

            /// <summary>
            /// Raw calibration for the Wii Nunchuk
            /// </summary>
            public Nunchuk NunchukRaw = new Nunchuk()
            {
                joystick = new Joystick()
                {
                    centerX = 127,
                    minX    = 31,
                    maxX    = 223,
                    deadX   = 0,

                    centerY = 127,
                    minY    = 31,
                    maxY    = 223,
                    deadY   = 0
                },
                accelerometer = new Accelerometer()
                {
                    centerX = 127,
                    minX    = 71,
                    maxX    = 159,
                    deadX   = 0,

                    centerY = 127,
                    minY    = 71,
                    maxY    = 183,
                    deadY   = 0,

                    centerZ = 127,
                    minZ    = 71,
                    maxZ    = 183,
                    deadZ   = 0
                }
            };

            /// <summary>
            /// Raw calibration for the Wii Classic Controller
            /// </summary>
            public ClassicController ClassicControllerRaw = new ClassicController()
            {
                LJoy = new Joystick()
                {
                    centerX = 31,
                    minX    = 0,
                    maxX    = 63,
                    deadX   = 0,

                    centerY = 31,
                    minY    = 0,
                    maxY    = 63,
                    deadY   = 0
                },
                RJoy = new Joystick()
                {
                    centerX = 15,
                    minX    = 0,
                    maxX    = 31,
                    deadX   = 0,

                    centerY = 15,
                    minY    = 0,
                    maxY    = 31,
                    deadY   = 0
                },
                L = new Trigger()
                {
                    min     = 0,
                    max     = 31
                },
                R = new Trigger()
                {
                    min     = 0,
                    max     = 31
                }
            };

            /// <summary>
            /// Raw calibration for the Wii Classic Controller Pro
            /// </summary>
            public ClassicControllerPro ClassicControllerProRaw = new ClassicControllerPro()
            {
                LJoy = new Joystick()
                {
                    centerX = 31,
                    minX    = 0,
                    maxX    = 63,
                    deadX   = 0,

                    centerY = 31,
                    minY    = 0,
                    maxY    = 63,
                    deadY   = 0
                },
                RJoy = new Joystick()
                {
                    centerX = 15,
                    minX    = 0,
                    maxX    = 31,
                    deadX   = 0,

                    centerY = 15,
                    minY    = 0,
                    maxY    = 31,
                    deadY   = 0
                }
            };
        }

        /// <summary>
        /// Class containing all the Minimal calibrations
        /// </summary>
        public class Minimal
        {
            /// <summary>
            /// Minimal calibration for the Wii U Pro Controller
            /// </summary>
            public ProController ProControllerMinimal = new ProController()
            {
                LJoy = new Joystick()
                {
                    centerX = 2047,
                    minX    = 1023,
                    maxX    = 3071,
                    deadX   = 64,

                    centerY = 2047,
                    minY    = 1023,
                    maxY    = 3071,
                    deadY   = 64
                },
                RJoy = new Joystick()
                {
                    centerX = 2047,
                    minX    = 1023,
                    maxX    = 3071,
                    deadX   = 64,

                    centerY = 2047,
                    minY    = 1023,
                    maxY    = 3071,
                    deadY   = 64
                }
            };

            /// <summary>
            /// Minimum calibration for the Wii Remote
            /// </summary>
            public Wiimote WiimoteMinimal = new Wiimote()
            {
                accelerometer = new Accelerometer()
                {
                    centerX = 127,
                    minX = 79,
                    maxX = 175,
                    deadX = 4,

                    centerY = 127,
                    minY = 79,
                    maxY = 175,
                    deadY = 4,

                    centerZ = 127,
                    minZ = 79,
                    maxZ = 175,
                    deadZ = 4
                },

                irSensor = new IR()
                {
                    boundingArea = new SquareBoundry()
                    {
                        center_x = 512,
                        center_y = 512,
                        width = 64,
                        height = 64
                    }
                }
            };

            /// <summary>
            /// Minimum calibration for the Wii Nunchuk
            /// </summary>
            public Nunchuk NunchukMinimal = new Nunchuk()
            {
                joystick = new Joystick()
                {
                    centerX = 127,
                    minX    = 31,
                    maxX    = 223,
                    deadX   = 4,

                    centerY = 127,
                    minY    = 31,
                    maxY    = 223,
                    deadY   = 4
                },
                accelerometer = new Accelerometer()
                {
                    centerX = 127,
                    minX = 79,
                    maxX = 175,
                    deadX = 4,

                    centerY = 127,
                    minY = 79,
                    maxY = 175,
                    deadY = 4,

                    centerZ = 127,
                    minZ = 79,
                    maxZ = 175,
                    deadZ = 4
                }
            };

            /// <summary>
            /// Minimum calibration for the Wii Classic Controller
            /// </summary>
            public ClassicController ClassicControllerMinimal = new ClassicController()
            {
                LJoy = new Joystick()
                {
                    centerX = 31,
                    minX    = 0,
                    maxX    = 63,
                    deadX   = 2,

                    centerY = 31,
                    minY    = 0,
                    maxY    = 63,
                    deadY   = 2
                },
                RJoy = new Joystick()
                {
                    centerX = 15,
                    minX    = 0,
                    maxX    = 31,
                    deadX   = 1,

                    centerY = 15,
                    minY    = 0,
                    maxY    = 31,
                    deadY   = 1
                },
                L = new Trigger()
                {
                    min = 0,
                    max = 31
                },
                R = new Trigger()
                {
                    min = 0,
                    max = 31
                }
            };

            /// <summary>
            /// Minimum calibration for the Wii Classic Controller Pro
            /// </summary>
            public ClassicControllerPro ClassicControllerProMinimal = new ClassicControllerPro()
            {
                LJoy = new Joystick()
                {
                    centerX = 31,
                    minX    = 0,
                    maxX    = 63,
                    deadX   = 2,

                    centerY = 31,
                    minY    = 0,
                    maxY    = 63,
                    deadY   = 2
                },
                RJoy = new Joystick()
                {
                    centerX = 15,
                    minX    = 0,
                    maxX    = 31,
                    deadX   = 1,

                    centerY = 15,
                    minY    = 0,
                    maxY    = 31,
                    deadY   = 1
                }
            };
        }

        // TODO: Calibration - Optimize Defaults
        /// <summary>
        /// Class containing all the Default calibrations
        /// </summary>
        public class Default
        {
            /// <summary>
            /// Default calibration for the Wii U Pro Controller
            /// </summary>
            public ProController ProControllerDefault = new ProController()
            {
                LJoy = new Joystick()
                {
                    centerX = 2047,
                    minX    = 1023,
                    maxX    = 3071,
                    deadX   = 128,
                    deadXp  = 128,
                    deadXn  = -128,

                    centerY = 2047,
                    minY    = 1023,
                    maxY    = 3071,
                    deadY   = 128,
                    deadYp  = 128,
                    deadYn  = -128
                },
                RJoy = new Joystick()
                {
                    centerX = 2047,
                    minX    = 1023,
                    maxX    = 3071,
                    deadX   = 128,
                    deadXp  = 128,
                    deadXn  = -128,

                    centerY = 2047,
                    minY    = 1023,
                    maxY    = 3071,
                    deadY   = 128,
                    deadYp  = 128,
                    deadYn  = -128
                }
            };

            /// <summary>
            /// Default calibration for the Wii Remote
            /// </summary>
            public Wiimote WiimoteDefault = new Wiimote()
            {
                accelerometer = new Accelerometer()
                {
                    centerX = 127,
                    minX    = 79 + 32,
                    maxX    = 175 - 32,
                    deadX   = 4,

                    centerY = 127,
                    minY    = 79 + 32,
                    maxY    = 175 - 32,
                    deadY   = 4,

                    centerZ = 127,
                    minZ    = 79 + 32,
                    maxZ    = 175 - 32,
                    deadZ   = 4
                },

                irSensor = new IR()
                {
                    boundingArea = new SquareBoundry()
                    {
                        center_x = 512,
                        center_y = 512,
                        width = 128,
                        height = 128
                    }
                }
            };

            /// <summary>
            /// Default calibration for the Wii Nunchuk
            /// </summary>
            public Nunchuk NunchukDefault = new Nunchuk()
            {
                joystick = new Joystick()
                {
                    centerX = 127,
                    minX    = 31,
                    maxX    = 223,
                    deadX   = 8,

                    centerY = 127,
                    minY    = 31,
                    maxY    = 223,
                    deadY   = 8
                },
                accelerometer = new Accelerometer()
                {
                    centerX = 127,
                    minX    = 79 + 32,
                    maxX    = 175 - 32,
                    deadX   = 4,

                    centerY = 127,
                    minY    = 79 + 32,
                    maxY    = 175 - 32,
                    deadY   = 4,

                    centerZ = 127,
                    minZ    = 79 + 32,
                    maxZ    = 175 - 32,
                    deadZ   = 4
                }
            };

            /// <summary>
            /// Default calibration for the Wii Classic Controller
            /// </summary>
            public ClassicController ClassicControllerDefault = new ClassicController()
            {
                LJoy = new Joystick()
                {
                    centerX = 31,
                    minX    = 0,
                    maxX    = 63,
                    deadX   = 4,

                    centerY = 31,
                    minY    = 0,
                    maxY    = 63,
                    deadY   = 4
                },
                RJoy = new Joystick()
                {
                    centerX = 15,
                    minX    = 0,
                    maxX    = 31,
                    deadX   = 2,

                    centerY = 15,
                    minY    = 0,
                    maxY    = 31,
                    deadY   = 2
                },
                L = new Trigger()
                {
                    min = 0,
                    max = 31
                },
                R = new Trigger()
                {
                    min = 0,
                    max = 31
                }
            };

            /// <summary>
            /// Default calibration for the Wii Classic Controller Pro
            /// </summary>
            public ClassicControllerPro ClassicControllerProDefault = new ClassicControllerPro()
            {
                LJoy = new Joystick()
                {
                    centerX = 31,
                    minX    = 0,
                    maxX    = 63,
                    deadX   = 4,

                    centerY = 31,
                    minY    = 0,
                    maxY    = 63,
                    deadY   = 4
                },
                RJoy = new Joystick()
                {
                    centerX = 15,
                    minX    = 0,
                    maxX    = 31,
                    deadX   = 2,

                    centerY = 15,
                    minY    = 0,
                    maxY    = 31,
                    deadY   = 2
                }
            };

            public Guitar GuitarDefault = new Guitar()
            {
                Joy = new Joystick()
                {
                    centerX = 31,
                    minX = 0x10,
                    maxX = 0x30,
                    deadX = 0,

                    centerY = 31,
                    minY = 0x10,
                    maxY = 0x30,
                    deadY = 0
                }
            };

            public Turntable TurntableDefault = new Turntable()
            {
                Joy = new Joystick()
                {
                    centerX = 31,
                    minX = 0,
                    maxX = 63,
                    deadX = 4,

                    centerY = 31,
                    minY = 0,
                    maxY = 63,
                    deadY = 4
                },
                JoyTableLR = new Joystick()
                {
                    // Wii default turntable range
                    centerX = 32,
                    minX = 0,
                    maxX = 63,
                    deadX = 0,

                    centerY = 32,
                    minY = 0,
                    maxY = 63,
                    deadY = 0

                    // 360 LS is 0-65535, 360 turntable range is 0-255
                    // Wii turntable range is 0-63 so max Wii "LS" range should be 16383
                    //centerX = 8192,
                    //minX = 0,
                    //maxX = 16383,
                    //deadX = 0,

                    //centerY = 8192,
                    //minY = 0,
                    //maxY = 16383,
                    //deadY = 0
                },
                JoyDialCrossfade = new Joystick()
                {
                    centerX = 16,
                    minX = 0,
                    maxX = 31,
                    deadX = 0,

                    centerY = 8,
                    minY = 0,
                    maxY = 15,
                    deadY = 0
                },
                LButtons = new Trigger()
                {
                    // 360 trigger max range
                    min = 0,
                    max = 255
                },
                RButtons = new Trigger()
                {
                    // 360 trigger max range
                    min = 0,
                    max = 255
                },
                Dial = new Trigger()
                {
                    min = 0,
                    max = 31
                },
                Crossfader = new Trigger()
                {
                    min = 0,
                    max = 15
                }
            };
        }

        /// <summary>
        /// Class containing all the Modest Calibrations (larger deadzones)
        /// </summary>
        public class Modest
        {
            /// <summary>
            /// Modest calibration for the Wii U Pro Controller
            /// </summary>
            public ProController ProControllerModest = new ProController()
            {
                LJoy = new Joystick()
                {
                    centerX = 2047,
                    minX    = 1151,
                    maxX    = 2943,
                    deadX   = 192,

                    centerY = 2047,
                    minY    = 1151,
                    maxY    = 2943,
                    deadY   = 192
                },
                RJoy = new Joystick()
                {
                    centerX = 2047,
                    minX    = 1151,
                    maxX    = 2943,
                    deadX   = 192,

                    centerY = 2047,
                    minY    = 1151,
                    maxY    = 2943,
                    deadY   = 192
                }
            };

            /// <summary>
            /// Modest calibration for the Wii Remote
            /// </summary>
            public Wiimote WiimoteModest = new Wiimote()
            {
                accelerometer = new Accelerometer()
                {
                    centerX = 127,
                    minX    = 83 + 32,
                    maxX    = 171 - 32,
                    deadX   = 8,

                    centerY = 127,
                    minY    = 83 + 32,
                    maxY    = 171 - 32,
                    deadY   = 8,

                    centerZ = 127,
                    minZ    = 83 + 32,
                    maxZ    = 171 - 32,
                    deadZ   = 8
                },

                irSensor = new IR()
                {
                    boundingArea = new SquareBoundry()
                    {
                        center_x = 512,
                        center_y = 512,
                        width = 192,
                        height = 192
                    }
                }
            };

            /// <summary>
            /// Modest calibration for the Wii Nunchuk
            /// </summary>
            public Nunchuk NunchukModest = new Nunchuk()
            {
                joystick = new Joystick()
                {
                    centerX = 127,
                    minX    = 39,
                    maxX    = 215,
                    deadX   = 8,

                    centerY = 127,
                    minY    = 39,
                    maxY    = 215,
                    deadY   = 8
                },
                accelerometer = new Accelerometer()
                {
                    centerX = 127,
                    minX    = 75 + 32,
                    maxX    = 179 - 32,
                    deadX   = 8,

                    centerY = 127,
                    minY    = 75 + 32,
                    maxY    = 179 - 32,
                    deadY   = 8,

                    centerZ = 127,
                    minZ    = 75 + 32,
                    maxZ    = 179 - 32,
                    deadZ   = 8
                }
            };

            /// <summary>
            /// Modest calibration for the Wii Classic Controller
            /// </summary>
            public ClassicController ClassicControllerModest = new ClassicController()
            {
                LJoy = new Joystick()
                {
                    centerX = 31,
                    minX    = 8,
                    maxX    = 55,
                    deadX   = 8,

                    centerY = 31,
                    minY    = 8,
                    maxY    = 55,
                    deadY   = 8
                },
                RJoy = new Joystick()
                {
                    centerX = 15,
                    minX    = 4,
                    maxX    = 27,
                    deadX   = 4,

                    centerY = 15,
                    minY    = 4,
                    maxY    = 27,
                    deadY   = 4
                },
                L = new Trigger()
                {
                    min = 4,
                    max = 27
                },
                R = new Trigger()
                {
                    min = 4,
                    max = 27
                }
            };

            /// <summary>
            /// Modest calibration for the Wii Classic Controler Pro
            /// </summary>
            public ClassicControllerPro ClassicControllerProModest = new ClassicControllerPro()
            {
                LJoy = new Joystick()
                {
                    centerX = 31,
                    minX    = 8,
                    maxX    = 55,
                    deadX   = 8,

                    centerY = 31,
                    minY    = 8,
                    maxY    = 55,
                    deadY   = 8
                },
                RJoy = new Joystick()
                {
                    centerX = 15,
                    minX    = 4,
                    maxX    = 27,
                    deadX   = 4,

                    centerY = 15,
                    minY    = 4,
                    maxY    = 27,
                    deadY   = 4
                }
            };
        }

        /// <summary>
        /// Class containing all the Extra Calibrations (largets deadzones, shortest ranges)
        /// </summary>
        public class Extra
        {
            /// <summary>
            /// Extra calibration for the Wii U Pro Controller
            /// </summary>
            public ProController ProControllerExtra = new ProController()
            {
                LJoy = new Joystick()
                {
                    centerX = 2047,
                    minX    = 1183,
                    maxX    = 2911,
                    deadX   = 208,

                    centerY = 2047,
                    minY    = 1183,
                    maxY    = 2911,
                    deadY   = 208
                },
                RJoy = new Joystick()
                {
                    centerX = 2047,
                    minX    = 1183,
                    maxX    = 2911,
                    deadX   = 208,

                    centerY = 2047,
                    minY    = 1183,
                    maxY    = 2911,
                    deadY   = 208
                }
            };

            /// <summary>
            /// Extra calibration for the Wii Remote
            /// </summary>
            public Wiimote WiimoteExtra = new Wiimote()
            {
                accelerometer = new Accelerometer()
                {
                    centerX = 127,
                    minX    = 91 + 32,
                    maxX    = 163 - 32,
                    deadX   = 12,

                    centerY = 127,
                    minY    = 91 + 32,
                    maxY    = 163 - 32,
                    deadY   = 12,

                    centerZ = 127,
                    minZ    = 91 + 32,
                    maxZ    = 163 - 32,
                    deadZ   = 12
                },

                irSensor = new IR()
                {
                    boundingArea = new SquareBoundry()
                    {
                        center_x = 512,
                        center_y = 512,
                        width = 224,
                        height = 224
                    }
                }
            };

            /// <summary>
            /// Extra calibration for the Wii Nunchuk
            /// </summary>
            public Nunchuk NunchukExtra = new Nunchuk()
            {
                joystick = new Joystick()
                {
                    centerX = 127,
                    minX    = 47,
                    maxX    = 207,
                    deadX   = 12,

                    centerY = 127,
                    minY    = 47,
                    maxY    = 207,
                    deadY   = 12
                },
                accelerometer = new Accelerometer()
                {
                    centerX = 127,
                    minX    = 91 + 32,
                    maxX    = 163 - 32,
                    deadX   = 12,

                    centerY = 127,
                    minY    = 91 + 32,
                    maxY    = 163 - 32,
                    deadY   = 12,

                    centerZ = 127,
                    minZ    = 91 + 32,
                    maxZ    = 163 - 32,
                    deadZ   = 12
                }
            };

            /// <summary>
            /// Extra calibration for the Wii Classic Controller
            /// </summary>
            public ClassicController ClassicControllerExtra = new ClassicController()
            {
                LJoy = new Joystick()
                {
                    centerX = 31,
                    minX    = 12,
                    maxX    = 51,
                    deadX   = 10,

                    centerY = 31,
                    minY    = 12,
                    maxY    = 51,
                    deadY   = 10
                },
                RJoy = new Joystick()
                {
                    centerX = 15,
                    minX    = 6,
                    maxX    = 25,
                    deadX   = 5,

                    centerY = 15,
                    minY    = 6,
                    maxY    = 25,
                    deadY   = 5
                },
                L = new Trigger()
                {
                    min = 6,
                    max = 25
                },
                R = new Trigger()
                {
                    min = 6,
                    max = 25
                }
            };

            /// <summary>
            /// Extra calibration for the Classic Controller Pro
            /// </summary>
            public ClassicControllerPro ClassicControllerProExtra = new ClassicControllerPro()
            {
                LJoy = new Joystick()
                {
                    centerX = 31,
                    minX    = 12,
                    maxX    = 51,
                    deadX   = 10,

                    centerY = 31,
                    minY    = 12,
                    maxY    = 51,
                    deadY   = 10
                },
                RJoy = new Joystick()
                {
                    centerX = 15,
                    minX    = 6,
                    maxX    = 25,
                    deadX   = 5,

                    centerY = 15,
                    minY    = 6,
                    maxY    = 25,
                    deadY   = 5
                }
            };
        }
        #endregion
    }

    /// <summary>
    /// Class for easily storying calibrations.
    /// </summary>
    public class CalibrationStorage
    {
        /// <summary>
        /// Holder for Wii U Pro Controller Calibration
        /// </summary>
        public ProController ProCalibration;
        /// <summary>
        /// Holder for Wii Remote calibration
        /// </summary>
        public Wiimote WiimoteCalibration;
        /// <summary>
        /// Holder for Wii Nunchuk calibration
        /// </summary>
        public Nunchuk NunchukCalibration;
        /// <summary>
        /// Holder for Wii Classic Controller calibration
        /// </summary>
        public ClassicController ClassicCalibration;
        /// <summary>
        /// Holder for Wii Classic Controller Pro calibration
        /// </summary>
        public ClassicControllerPro ClassicProCalibration;
        /// <summary>
        /// Holder for Wii Turntable calibration
        /// </summary>
        public Turntable TurntableCalibration;

        /// <summary>
        /// Default constructor setting all calibrations to their defaults.
        /// </summary>
        public CalibrationStorage()
        {
            ProCalibration        = Calibrations.Defaults.ProControllerDefault;
            WiimoteCalibration    = Calibrations.Defaults.WiimoteDefault;
            NunchukCalibration    = Calibrations.Defaults.NunchukDefault;
            ClassicCalibration    = Calibrations.Defaults.ClassicControllerDefault;
            ClassicProCalibration = Calibrations.Defaults.ClassicControllerProDefault;
            TurntableCalibration  = Calibrations.Defaults.TurntableDefault;
        }

        /// <summary>
        /// Constructor that takes the ToString of this class and converts it to calibrations.
        /// </summary>
        /// <param name="storageString"></param>
        public CalibrationStorage(string storageString) : base()
        {
            SetCalibrations(storageString);
        }

        /// <summary>
        /// Sets calibrations based on the ToString of this class.
        /// </summary>
        /// <param name="storageString"></param>
        public void SetCalibrations(string storageString)
        {
            if (string.IsNullOrEmpty(storageString)) return;

            string[] chunks = storageString.Split(new char[] { '-' });

            foreach (string calStr in chunks)
            {
                if (calStr.StartsWith("wm"))
                {
                    WiimoteCalibration.SetCalibration(calStr);
                }
                else if (calStr.StartsWith("nun"))
                {
                    NunchukCalibration.SetCalibration(calStr);
                }
                else if (calStr.StartsWith("cls"))
                {
                    ClassicCalibration.SetCalibration(calStr);
                }
                else if (calStr.StartsWith("ccp"))
                {
                    ClassicProCalibration.SetCalibration(calStr);
                }
                else if (calStr.StartsWith("pro"))
                {
                    ProCalibration.SetCalibration(calStr);
                }
                else if (calStr.StartsWith("ttb"))
                {
                    TurntableCalibration.SetCalibration(calStr);
                }
            }
        }

        /// <summary>
        /// Sets calibrations based on a preset.
        /// </summary>
        /// <param name="preset"></param>
        public void SetCalibrations(Calibrations.CalibrationPreset preset)
        {
            // for turntables, only Default calibration is implemented
            switch (preset)
            {
                case Calibrations.CalibrationPreset.Default:
                    ProCalibration        = Calibrations.Defaults.ProControllerDefault;
                    WiimoteCalibration    = Calibrations.Defaults.WiimoteDefault;
                    NunchukCalibration    = Calibrations.Defaults.NunchukDefault;
                    ClassicCalibration    = Calibrations.Defaults.ClassicControllerDefault;
                    ClassicProCalibration = Calibrations.Defaults.ClassicControllerProDefault;
                    TurntableCalibration  = Calibrations.Defaults.TurntableDefault;
                    break;

                case Calibrations.CalibrationPreset.Modest:
                    ProCalibration        = Calibrations.Moderate.ProControllerModest;
                    WiimoteCalibration    = Calibrations.Moderate.WiimoteModest;
                    NunchukCalibration    = Calibrations.Moderate.NunchukModest;
                    ClassicCalibration    = Calibrations.Moderate.ClassicControllerModest;
                    ClassicProCalibration = Calibrations.Moderate.ClassicControllerProModest;
                    TurntableCalibration  = Calibrations.Defaults.TurntableDefault;
                    break;

                case Calibrations.CalibrationPreset.Extra:
                    ProCalibration        = Calibrations.Extras.ProControllerExtra;
                    WiimoteCalibration    = Calibrations.Extras.WiimoteExtra;
                    NunchukCalibration    = Calibrations.Extras.NunchukExtra;
                    ClassicCalibration    = Calibrations.Extras.ClassicControllerExtra;
                    ClassicProCalibration = Calibrations.Extras.ClassicControllerProExtra;
                    TurntableCalibration  = Calibrations.Defaults.TurntableDefault;
                    break;

                case Calibrations.CalibrationPreset.Minimum:
                    ProCalibration        = Calibrations.Minimum.ProControllerMinimal;
                    WiimoteCalibration    = Calibrations.Minimum.WiimoteMinimal;
                    NunchukCalibration    = Calibrations.Minimum.NunchukMinimal;
                    ClassicCalibration    = Calibrations.Minimum.ClassicControllerMinimal;
                    ClassicProCalibration = Calibrations.Minimum.ClassicControllerProMinimal;
                    TurntableCalibration  = Calibrations.Defaults.TurntableDefault;
                    break;

                case Calibrations.CalibrationPreset.None:
                    ProCalibration        = Calibrations.None.ProControllerRaw;
                    WiimoteCalibration    = Calibrations.None.WiimoteRaw;
                    NunchukCalibration    = Calibrations.None.NunchukRaw;
                    ClassicCalibration    = Calibrations.None.ClassicControllerRaw;
                    ClassicProCalibration = Calibrations.None.ClassicControllerProRaw;
                    TurntableCalibration  = Calibrations.Defaults.TurntableDefault;
                    break;
            }
        }

        /// <summary>
        /// Converts the calibrations in this class to a string format.
        /// </summary>
        /// <returns>String format of curent calibrations in this class</returns>
        public override string ToString()
        {
            string text = "";

            text += WiimoteCalibration.GetCalibrationString();
            text += NunchukCalibration.GetCalibrationString();
            text += ClassicCalibration.GetCalibrationString();
            text += ClassicProCalibration.GetCalibrationString();
            text += ProCalibration.GetCalibrationString();
            text += TurntableCalibration.GetCalibrationString();

            return text;
        }
    }
}

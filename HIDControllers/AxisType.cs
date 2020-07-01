﻿// Licensed under the Apache License, Version 2.0 (the "License").
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Reflection;
using HIDControllers.Controls;
using HidSharp.Reports;

namespace HIDControllers
{
    public sealed class AxisType
    {
        private static readonly Dictionary<Usage, AxisType> s_usages = new Dictionary<Usage, AxisType>
        {
            {
                Usage.GenericDesktopX /* 0x00010030 */, new AxisType(Usage.GenericDesktopX, "X-axis", typeof(Joystick),
                    Usage.GenericDesktopX,
                    Usage.GenericDesktopY)
            },
            {
                Usage.GenericDesktopY /* 0x00010031 */, new AxisType(Usage.GenericDesktopY, "Y-axis", typeof(Joystick),
                    Usage.GenericDesktopX,
                    Usage.GenericDesktopY)
            },
            {Usage.GenericDesktopZ /* 0x00010032 */, new AxisType(Usage.GenericDesktopZ, "Z-axis", typeof(Slider))},
            {
                Usage.GenericDesktopRx /* 0x00010033 */, new AxisType(Usage.GenericDesktopRx, "RX-axis",
                    typeof(Joystick),
                    Usage.GenericDesktopRx,
                    Usage.GenericDesktopRy)
            },
            {
                Usage.GenericDesktopRy /* 0x00010034 */, new AxisType(Usage.GenericDesktopRy, "RY-axis",
                    typeof(Joystick),
                    Usage.GenericDesktopRx,
                    Usage.GenericDesktopRy)
            },
            {Usage.GenericDesktopRz /* 0x00010035 */, new AxisType(Usage.GenericDesktopRz, "RZ-axis", typeof(Slider))},
            {
                Usage.GenericDesktopSlider /* 0x00010036 */,
                new AxisType(Usage.GenericDesktopSlider, "Slider", typeof(Slider))
            },
            {Usage.GenericDesktopDial /* 0x00010037 */, new AxisType(Usage.GenericDesktopDial, "Dial", typeof(Slider))},
            {
                Usage.GenericDesktopWheel /* 0x00010038 */,
                new AxisType(Usage.GenericDesktopWheel, "Wheel", typeof(Slider))
            },
            {
                Usage.GenericDesktopHatSwitch /* 0x00010039 */,
                new AxisType(Usage.GenericDesktopHatSwitch, "Hat", typeof(Hat))
            },
            {
                Usage.GenericDesktopStart /* 0x0001003D */,
                new AxisType(Usage.GenericDesktopStart, "Start", typeof(Button))
            },
            {
                Usage.GenericDesktopSelect /* 0x0001003E */,
                new AxisType(Usage.GenericDesktopSelect, "Select", typeof(Button))
            },
            {
                Usage.GenericDesktopSystemControl /* 0x00010080 */,
                new AxisType(Usage.GenericDesktopSystemControl, "System Control", typeof(Button))
            },
            {
                Usage.GenericDesktopDPadUp /* 0x00010090 */, new AxisType(Usage.GenericDesktopDPadUp, "D-Pad Up",
                    typeof(DPad),
                    Usage.GenericDesktopDPadUp, Usage.GenericDesktopDPadRight, Usage.GenericDesktopDPadDown,
                    Usage.GenericDesktopDPadLeft)
            },
            {
                Usage.GenericDesktopDPadDown /* 0x00010091 */, new AxisType(Usage.GenericDesktopDPadDown, "D-Pad Down",
                    typeof(DPad),
                    Usage.GenericDesktopDPadUp, Usage.GenericDesktopDPadRight, Usage.GenericDesktopDPadDown,
                    Usage.GenericDesktopDPadLeft)
            },
            {
                Usage.GenericDesktopDPadLeft /* 0x00010092 */, new AxisType(Usage.GenericDesktopDPadLeft, "D-Pad Left",
                    typeof(DPad),
                    Usage.GenericDesktopDPadUp, Usage.GenericDesktopDPadRight, Usage.GenericDesktopDPadDown,
                    Usage.GenericDesktopDPadLeft)
            },
            {
                Usage.GenericDesktopDPadRight /* 0x00010093 */, new AxisType(Usage.GenericDesktopDPadRight,
                    "D-Pad Right",
                    typeof(DPad),
                    Usage.GenericDesktopDPadUp, Usage.GenericDesktopDPadRight, Usage.GenericDesktopDPadDown,
                    Usage.GenericDesktopDPadLeft)
            }
        };

        static AxisType()
        {
            for (var i = 1; i <= 256; i++)
            {
                var usage = (Usage)(0x00090000 + i);
                // Add first 256 Buttons  (Spec. allows for up to 65535, but that's ridiculous)
                s_usages[usage] = new AxisType(usage, "Button " + i, typeof(Button));
            }
        }

        private AxisType(Usage usage, string name, Type type, params Usage[] group)
        {
            Usage = usage;
            Name = name;
            Type = type;
            Group = group;
        }

        public Usage Usage { get; }
        public string Name { get; }
        public Type Type { get; }
        public IReadOnlyList<Usage> Group { get; }

        public bool IsGrouped => Group.Count > 0;

        internal Axis Create(Controller controller, DataItem dataItem)
        {
            var type = Type;
            if (Group.Count > 0)
            {
                type = typeof(Axis);
            }

            return (Axis)Activator.CreateInstance(
                type,
                BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] {this, controller, dataItem}, null);
        }

        internal Control Create(Axis[] group)
        {
            if (Group.Count != group.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(group),
                    string.Format(Resources.ControlCreationFailure2, Group.Count, group.Length));
            }

            return (Control)Activator.CreateInstance(
                Type,
                BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] {group}, null);
        }

        public static AxisType? Get(Usage du) => s_usages.TryGetValue(du, out var t) ? t : null;

        public static bool TryGet(Usage du, out AxisType type) => s_usages.TryGetValue(du, out type);

        public static bool SupportedUsage(Usage du) => s_usages.ContainsKey(du);
    }
}

﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;

namespace osu.Game.Overlays.Volume
{
    public class VolumeControlReceptor : Container, IMouseWheelBindingHandler<GlobalAction>, IHandleGlobalInput
    {
        public Func<GlobalAction, bool> ActionRequested;
        public Func<GlobalAction, float, bool, bool> WheelActionRequested;

        public bool OnPressed(GlobalAction action) => ActionRequested?.Invoke(action) ?? false;
        public bool OnMouseWheel(GlobalAction action, float amount, bool isPrecise) => WheelActionRequested?.Invoke(action, amount, isPrecise) ?? false;
        public bool OnReleased(GlobalAction action) => false;
    }
}

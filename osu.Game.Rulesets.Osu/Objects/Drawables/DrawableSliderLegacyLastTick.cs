﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderLegacyLastTick : DrawableOsuHitObject, IRequireTracking
    {
        private readonly Slider slider;

        /// <summary>
        /// The judgement text is provided by the <see cref="DrawableSlider"/>.
        /// </summary>
        public override bool DisplayResult => false;

        public bool Tracking { get; set; }

        private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();
        private readonly IBindable<int> pathVersion = new Bindable<int>();

        public DrawableSliderLegacyLastTick(Slider slider, SliderLegacyLastTick hitCircle)
            : base(hitCircle)
        {
            this.slider = slider;

            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fit;

            AlwaysPresent = true;

            positionBindable.BindTo(hitCircle.PositionBindable);
            pathVersion.BindTo(slider.Path.Version);

            positionBindable.BindValueChanged(_ => updatePosition());
            pathVersion.BindValueChanged(_ => updatePosition(), true);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered && timeOffset >= 0)
                ApplyResult(r => r.Type = Tracking ? HitResult.Great : HitResult.Miss);
        }

        private void updatePosition() => Position = HitObject.Position - slider.Position;
    }
}

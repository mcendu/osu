// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderTail : DrawableOsuHitObject, IRequireTracking
    {
        private readonly Slider slider;

        /// <summary>
        /// The judgement text is provided by the <see cref="DrawableSlider"/>.
        /// </summary>
        public override bool DisplayResult => false;

        private readonly Bindable<bool> showTail = new Bindable<bool>(false);
        private double animDuration;
        public bool Tracking { get; set; }

        private readonly IBindable<float> scaleBindable = new Bindable<float>();
        private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();
        private readonly IBindable<int> pathVersion = new Bindable<int>();
        private readonly SkinnableDrawable circlePiece;
        private readonly Container scaleContainer;

        public DrawableSliderTail(Slider slider, SliderTailCircle hitCircle)
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

            InternalChildren = new Drawable[]
            {
                scaleContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        circlePiece = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.SliderTail), _ => null),
                    }
                },
            };

            animDuration = Math.Min(150, hitCircle.SpanDuration / 2);
        }

        [BackgroundDependencyLoader]
        private void load(OsuRulesetConfigManager rulesetConfig)
        {
            rulesetConfig?.BindWith(OsuRulesetSetting.ShowSliderTail, showTail);

            scaleBindable.BindValueChanged(scale => scaleContainer.Scale = new Vector2(scale.NewValue), true);

            scaleBindable.BindTo(HitObject.ScaleBindable);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered && timeOffset >= 0)
                ApplyResult(r => r.Type = Tracking ? HitResult.Great : HitResult.Miss);
        }

        protected override void UpdateInitialTransforms()
        {
            showTail.BindValueChanged(v => circlePiece.FadeTo(v.NewValue ? 1 : 0), true);

            this.FadeIn(HitObject.TimeFadeIn);
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            switch (state)
            {
                case ArmedState.Idle:
                    this.Delay(HitObject.TimePreempt).FadeOut();
                    break;

                case ArmedState.Miss:
                    this.FadeOut(animDuration);
                    break;

                case ArmedState.Hit:
                    this.FadeOut(animDuration, Easing.OutQuint)
                        .ScaleTo(Scale * 1.5f, animDuration, Easing.Out);
                    break;
            }
        }

        private void updatePosition() => Position = HitObject.Position - slider.Position;
    }
}

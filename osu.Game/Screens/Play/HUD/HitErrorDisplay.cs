﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD.HitErrorMeters;

namespace osu.Game.Screens.Play.HUD
{
    public class HitErrorDisplay : Container<FillFlowContainer<HitErrorMeter>>
    {
        private const int fade_duration = 200;
        private const int margin = 10;

        private readonly Bindable<ScoreMeterType> type = new Bindable<ScoreMeterType>();

        private readonly HitWindows hitWindows;

        private readonly ScoreProcessor processor;

        private readonly FillFlowContainer<HitErrorMeter> leftMeters;
        private readonly FillFlowContainer<HitErrorMeter> rightMeters;

        public HitErrorDisplay(ScoreProcessor processor, HitWindows hitWindows)
        {
            this.processor = processor;
            this.hitWindows = hitWindows;

            RelativeSizeAxes = Axes.Both;

            if (processor != null)
                processor.NewJudgement += onNewJudgement;

            Children = new[]
            {
                leftMeters = new FillFlowContainer<HitErrorMeter>
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding(margin),
                },
                rightMeters = new FillFlowContainer<HitErrorMeter>
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding(margin),
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.ScoreMeter, type);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            type.BindValueChanged(typeChanged, true);
        }

        private void onNewJudgement(JudgementResult result)
        {
            if (result.HitObject.HitWindows.WindowFor(HitResult.Miss) == 0)
                return;

            foreach (var c in Children)
            {
                foreach (var meter in c.Children)
                    meter.OnNewJudgement(result);
            }
        }

        private void typeChanged(ValueChangedEvent<ScoreMeterType> type)
        {
            Children.ForEach(c => c.ForEach(meter => meter.FadeOut(fade_duration, Easing.OutQuint)));

            if (hitWindows == null)
                return;

            switch (type.NewValue)
            {
                case ScoreMeterType.HitErrorBoth:
                    createBar(false);
                    createBar(true);
                    break;

                case ScoreMeterType.HitErrorLeft:
                    createBar(false);
                    break;

                case ScoreMeterType.HitErrorRight:
                    createBar(true);
                    break;

                case ScoreMeterType.ColourBoth:
                    createColour(false);
                    createColour(true);
                    break;

                case ScoreMeterType.ColourLeft:
                    createColour(false);
                    break;

                case ScoreMeterType.ColourRight:
                    createColour(true);
                    break;

                case ScoreMeterType.CombinedLeft:
                    createCombined(false);
                    break;

                case ScoreMeterType.CombinedRight:
                    createCombined(true);
                    break;

                case ScoreMeterType.CombinedBoth:
                    createCombined(false);
                    createCombined(true);
                    break;
            }
        }

        private void createBar(bool rightAligned)
        {
            var display = new BarHitErrorMeter(hitWindows, rightAligned)
            {
                Margin = new MarginPadding(margin),
                Anchor = rightAligned ? Anchor.CentreRight : Anchor.CentreLeft,
                Origin = rightAligned ? Anchor.CentreRight : Anchor.CentreLeft,
                Alpha = 0,
            };

            completeDisplayLoading(display, rightAligned);
        }

        private void createColour(bool rightAligned)
        {
            var display = new ColourHitErrorMeter(hitWindows)
            {
                Margin = new MarginPadding(margin),
                Anchor = rightAligned ? Anchor.CentreRight : Anchor.CentreLeft,
                Origin = rightAligned ? Anchor.CentreRight : Anchor.CentreLeft,
                Alpha = 0,
            };

            completeDisplayLoading(display, rightAligned);
        }

        private void createCombined(bool rightAligned)
        {
            createBar(rightAligned);
            createColour(rightAligned);
        }

        private void completeDisplayLoading(HitErrorMeter display, bool rightAligned)
        {
            if (rightAligned)
                rightMeters.Add(display);
            else
                leftMeters.Add(display);
            display.FadeInFromZero(fade_duration, Easing.OutQuint);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (processor != null)
                processor.NewJudgement -= onNewJudgement;
        }
    }
}

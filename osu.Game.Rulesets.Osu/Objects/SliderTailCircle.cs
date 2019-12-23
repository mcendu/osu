// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    /// <summary>
    /// Note that this should not be used for timing correctness.
    /// See <see cref="SliderEventType.LegacyLastTick"/> usage in <see cref="Slider"/> for more information.
    /// </summary>
    public class SliderTailCircle : SliderCircle
    {
        public int SpanIndex { get; set; }
        public double SpanDuration { get; set; }

        private readonly IBindable<int> pathVersion = new Bindable<int>();

        public SliderTailCircle(Slider slider)
        {
            pathVersion.BindTo(slider.Path.Version);
            pathVersion.BindValueChanged(_ => Position = slider.EndPosition);
        }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            if (SpanIndex > 0)
                TimePreempt = Math.Min(SpanDuration * 2, TimePreempt + SpanDuration);
        }

        public override Judgement CreateJudgement() => new OsuSliderTailJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}

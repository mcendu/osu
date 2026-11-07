// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Rulesets.Taiko.Skinning.Legacy;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual.Gameplay;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    public partial class TestSceneLegacyScoreIncrementDisplay : SkinnableHUDComponentTestScene
    {
        [Cached(typeof(ScoreProcessor))]
        private ScoreProcessor scoreProcessor = new TaikoScoreProcessor();

        protected override Drawable CreateDefaultImplementation() => new Container();

        protected override Drawable CreateLegacyImplementation() => new LegacyScoreIncreaseDisplay();

        public override void SetUpSteps()
        {
            base.SetUpSteps();
        }

        [Test]
        public void TestScoreIncrementOnHit()
        {
            var beatmap = new TestBeatmap(new RulesetInfo())
            {
                HitObjects = new List<HitObject>
                {
                    new Hit(),
                },
            };

            AddStep("reset score", () => scoreProcessor.ApplyBeatmap(beatmap));
            AddStep("apply GREAT judgement", () => scoreProcessor.ApplyResult(
                new JudgementResult(beatmap.HitObjects.Single(), new TaikoJudgement())
                {
                    Type = HitResult.Great,
                })
            );

            AddStep("reset score", () => scoreProcessor.ApplyBeatmap(beatmap));
            AddStep("apply OK judgement", () => scoreProcessor.ApplyResult(
                new JudgementResult(beatmap.HitObjects.Single(), new TaikoJudgement())
                {
                    Type = HitResult.Ok,
                })
            );
        }

        [Test]
        public void TestScoreIncrementOnStrongHit()
        {
            var beatmap = new TestBeatmap(new RulesetInfo())
            {
                HitObjects = new List<HitObject>
                {
                    new Hit { IsStrong = true },
                },
            };

            var hitObject = (beatmap.HitObjects.Single() as Hit)!;

            AddStep("reset score", () =>
            {
                scoreProcessor.ApplyBeatmap(beatmap);
                hitObject.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);
            });
            AddStep("apply GREAT judgement", () => scoreProcessor.ApplyResult(
                new JudgementResult(hitObject, new TaikoJudgement())
                {
                    Type = HitResult.Great,
                })
            );
            AddStep("hit strong nested object", () => scoreProcessor.ApplyResult(
                new JudgementResult(hitObject.NestedHitObjects.Single()!, new TaikoStrongJudgement())
                {
                    Type = HitResult.LargeBonus,
                })
            );

            AddStep("reset score", () =>
            {
                scoreProcessor.ApplyBeatmap(beatmap);
                hitObject.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);
            });
            AddStep("apply GREAT judgement", () => scoreProcessor.ApplyResult(
                new JudgementResult(hitObject, new TaikoJudgement())
                {
                    Type = HitResult.Great,
                })
            );
            AddStep("miss strong nested object", () => scoreProcessor.ApplyResult(
                new JudgementResult(hitObject.NestedHitObjects.Single()!, new TaikoStrongJudgement())
                {
                    Type = HitResult.IgnoreMiss,
                })
            );
        }
    }
}

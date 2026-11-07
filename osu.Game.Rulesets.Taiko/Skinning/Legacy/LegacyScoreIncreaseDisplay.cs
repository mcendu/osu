// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Configuration;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Pooling;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public partial class LegacyScoreIncreaseDisplay : PooledDrawableWithLifetimeContainer<LegacyScoreIncreaseEntry, LegacyScoreIncrease>, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.Colour))]
        public BindableColour4 AccentColour { get; } = new BindableColour4(Colour4.Red);

        [SettingSource("Fly in from right")]
        public BindableBool FlyInFromRight { get; } = new BindableBool(false);

        [SettingSource("Fly out downwards")]
        public BindableBool FlyOutDownwards { get; } = new BindableBool(false);

        private readonly IBindable<long> totalScore = new BindableLong { MinValue = 0 };

        private DrawablePool<LegacyScoreIncrease> scoreIncrementPool = null!;
        private Drawable sizeReference = null!;
        private Container<LegacyScoreIncrease> scoreIncrements = null!;

        private long oldScore;

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            InternalChildren = new Drawable[]
            {
                scoreIncrementPool = new DrawablePool<LegacyScoreIncrease>(15, 30),
                // hidden text serves as a size hint for the skin editor.
                sizeReference = new LegacySpriteText(LegacyFont.Score)
                {
                    Alpha = 0,
                    AlwaysPresent = true,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Text = "0000",
                },
                scoreIncrements = new Container<LegacyScoreIncrease>()
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                },
            };

            totalScore.BindTo(scoreProcessor.TotalScore);
            oldScore = scoreProcessor.TotalScore.Value;
            scoreProcessor.NewJudgement += newJudgement;
            scoreProcessor.JudgementReverted += judgementReverted;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Size = sizeReference.Size;
        }

        private void newJudgement(JudgementResult judgement)
        {
            long increment = totalScore.Value - oldScore;

            if (!IsLoaded)
            {
                oldScore = totalScore.Value;
                return;
            }

            if (judgement.HitObject is TaikoStrongableHitObject strongable && strongable.IsStrong)
                return; // wait until the nested strong hit is judged.

            oldScore = totalScore.Value;

            if (increment > 0)
            {
                Add(
                    new LegacyScoreIncreaseEntry(
                        Time.Current,
                        increment,
                        new Vector2(FlyInFromRight.Value ? 100f : -100f, 0f),
                        new Vector2(0f, FlyOutDownwards.Value ? 46f : -46f),
                        AccentColour.Value
                    )
                );
            }
        }

        private void judgementReverted(JudgementResult judgement)
        {
            oldScore = totalScore.Value;
        }

        protected override void AddDrawable(LegacyScoreIncreaseEntry entry, LegacyScoreIncrease drawable)
        {
            scoreIncrements.Add(drawable);
        }

        protected override void RemoveDrawable(LegacyScoreIncreaseEntry entry, LegacyScoreIncrease drawable)
        {
            scoreIncrements.Remove(drawable, false);
        }

        protected override LegacyScoreIncrease GetDrawable(LegacyScoreIncreaseEntry entry)
        {
            var increment = scoreIncrementPool.Get();
            increment.Apply(entry);
            return increment;
        }
    }
}

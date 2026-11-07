// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Pooling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public partial class LegacyScoreIncrease : PoolableDrawableWithLifetime<LegacyScoreIncreaseEntry>
    {
        private LegacySpriteText text = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Alpha = 0;

            InternalChild = text = new LegacySpriteText(LegacyFont.Score)
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
            };
        }

        protected override void OnApply(LegacyScoreIncreaseEntry entry)
        {
            text.Text = entry.Value.ToString(CultureInfo.InvariantCulture);
            text.Colour = entry.Colour;

            using (BeginAbsoluteSequence(entry.LifetimeStart, false))
            {
                this.FadeOut().MoveTo(entry.Source)
                    .FadeIn(50).MoveTo(Vector2.Zero, 100, Easing.OutBack)
                    .Then().Delay(200)
                    .MoveTo(entry.Destination, 200, Easing.InBack).FadeOut(200);

                Expire();
            }
        }

        protected override void OnFree(LegacyScoreIncreaseEntry entry)
        {
            ApplyTransformsAt(double.MinValue);
            ClearTransforms();
        }
    }
}

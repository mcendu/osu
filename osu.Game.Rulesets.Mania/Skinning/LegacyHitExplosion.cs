// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyHitExplosion : LegacyManiaElement
    {
        public LegacyHitExplosion()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo)
        {
            InternalChild = skin.GetAnimation(
                getTextureFromLookup(skin, LegacyManiaSkinConfigurationLookups.LightingN),
                true, false).With(d =>
            {
                d.Anchor = Anchor.Centre;
                d.Origin = Anchor.Centre;
                d.Blending = BlendingParameters.Additive;
            });
        }

        private string getTextureFromLookup(ISkin skin, LegacyManiaSkinConfigurationLookups lookup)
        {
            return GetManiaSkinConfig<string>(skin, lookup)?.Value ?? lookup.ToString();
        }

        protected override void LoadComplete()
        {
            const double duration = 200;

            this.FadeOut(duration, Easing.None);
            Expire(true);
        }
    }
}

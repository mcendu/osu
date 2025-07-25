// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public partial class TaikoModAllBig : Mod, IApplicableAfterBeatmapConversion
    {
        public override string Name => @"All Big";
        public override string Acronym => @"AB";
        public override LocalisableString Description => @"MAKE EVERY NOTE BIG.";

        public override double ScoreMultiplier => 1.0;
        public override ModType Type => ModType.Fun;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            // ApplyToHitObject does not affect scoring, hence IApplicableAfterBeatmapConversion.
            var taikoBeatmap = (TaikoBeatmap)beatmap;
            foreach (var obj in taikoBeatmap.HitObjects)
            {
                if (obj is TaikoStrongableHitObject strongable)
                {
                    strongable.IsStrong = true;
                }
            }
        }
    }
}

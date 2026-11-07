// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Performance;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public class LegacyScoreIncreaseEntry : LifetimeEntry
    {
        public readonly long Value;

        public readonly Vector2 Source;

        public readonly Vector2 Destination;

        public readonly Colour4 Colour;

        public LegacyScoreIncreaseEntry(double startTime, long value, Vector2 source, Vector2 destination, Colour4 colour)
        {
            LifetimeStart = startTime;
            Value = value;
            Source = source;
            Destination = destination;
            Colour = colour;
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    [TestFixture]
    public class TestSceneHitExplosion : SkinnableTestScene
    {
        private int runCount;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableNote),
            typeof(DrawableManiaHitObject),
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            SetContents(() => new ScrollingTestContainer(ScrollingDirection.Down)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativePositionAxes = Axes.Y,
                Y = -0.25f,
                Size = new Vector2(Column.COLUMN_WIDTH, DefaultNotePiece.NOTE_HEIGHT),
            });
        }

        [SetUp]
        public void SetUp()
        {
            runCount = 0;
        }

        [Test]
        public void Test()
        {
            AddRepeatStep("explode", () =>
            {
                runCount++;

                if (runCount % 15 > 12)
                    return;

                CreatedDrawables.ForEach(d => (d as ScrollingTestContainer)?.AddRange(new Drawable[]
                {
                    new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.HitExplosion),
                        _ => new DefaultHitExplosion((runCount / 15) % 2 == 0
                            ? new Color4(94, 0, 57, 255)
                            : new Color4(6, 84, 0, 255), runCount % 6 != 0))
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                }));
            }, 100);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public partial class LegacyTaikoComboCounter : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        public IBindable<int> Current = new BindableInt { MinValue = 0 };

        /// <summary>
        /// Value shown at the current moment.
        /// </summary>
        public int DisplayedCount
        {
            get => displayedCount;
            private set
            {
                if (displayedCount == value)
                    return;

                countText.FadeTo(value < 10 ? 0 : 1);
                countText.Text = value.ToString(CultureInfo.InvariantCulture);
                countContainer.Size = countText.Size;

                displayedCount = value;
            }
        }

        private int displayedCount;

        private Container countContainer = null!;
        private LegacySpriteText countText = null!;

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = countContainer = new Container
            {
                Child = countText = new LegacySpriteText(LegacyFont.Combo)
                {
                    Alpha = 0,
                    AlwaysPresent = true,
                    FixedWidth = true,
                    BypassAutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                }
            };

            Current.BindTo(scoreProcessor.Combo);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            countText.Text = Current.Value.ToString(CultureInfo.InvariantCulture);

            Current.BindValueChanged(updateCount, true);

            countContainer.Size = countText.Size;
        }

        private void updateCount(ValueChangedEvent<int> e)
        {
            if (!IsLoaded)
                return;

            FinishTransforms(false, nameof(DisplayedCount));
            DisplayedCount = e.NewValue;

            if (e.NewValue > e.OldValue)
            {
                countText.ScaleTo(new Vector2(1f, 1.2f))
                         .ScaleTo(new Vector2(1f), 300, Easing.Out);
            }
        }
    }
}

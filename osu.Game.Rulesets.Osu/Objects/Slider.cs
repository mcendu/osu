﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Game.Rulesets.Objects.Types;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using System.Linq;
using osu.Framework.Caching;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class Slider : OsuHitObject, IHasCurve
    {
        public double EndTime => StartTime + this.SpanCount() * Path.Distance / Velocity;
        public double Duration => EndTime - StartTime;

        private readonly Cached<Vector2> endPositionCache = new Cached<Vector2>();

        public override Vector2 EndPosition => endPositionCache.IsValid ? endPositionCache.Value : endPositionCache.Value = Position + this.CurvePositionAt(1);

        public Vector2 StackedPositionAt(double t) => StackedPosition + this.CurvePositionAt(t);

        private readonly SliderPath path = new SliderPath();

        public SliderPath Path
        {
            get => path;
            set
            {
                path.ControlPoints.Clear();
                path.ExpectedDistance.Value = null;

                if (value != null)
                {
                    path.ControlPoints.AddRange(value.ControlPoints);
                    path.ExpectedDistance.Value = value.ExpectedDistance.Value;
                }
            }
        }

        public double Distance => Path.Distance;

        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                updateNestedPositions();
            }
        }

        public double? LegacyLastTickOffset { get; set; }

        /// <summary>
        /// The position of the cursor at the point of completion of this <see cref="Slider"/> if it was hit
        /// with as few movements as possible. This is set and used by difficulty calculation.
        /// </summary>
        internal Vector2? LazyEndPosition;

        /// <summary>
        /// The distance travelled by the cursor upon completion of this <see cref="Slider"/> if it was hit
        /// with as few movements as possible. This is set and used by difficulty calculation.
        /// </summary>
        internal float LazyTravelDistance;

        public List<IList<HitSampleInfo>> NodeSamples { get; set; } = new List<IList<HitSampleInfo>>();

        private int repeatCount;

        public int RepeatCount
        {
            get => repeatCount;
            set
            {
                repeatCount = value;
                endPositionCache.Invalidate();
            }
        }

        /// <summary>
        /// The length of one span of this <see cref="Slider"/>.
        /// </summary>
        public double SpanDuration => Duration / this.SpanCount();

        /// <summary>
        /// Velocity of this <see cref="Slider"/>.
        /// </summary>
        public double Velocity { get; private set; }

        /// <summary>
        /// Spacing between <see cref="SliderTick"/>s of this <see cref="Slider"/>.
        /// </summary>
        public double TickDistance { get; private set; }

        /// <summary>
        /// An extra multiplier that affects the number of <see cref="SliderTick"/>s generated by this <see cref="Slider"/>.
        /// An increase in this value increases <see cref="TickDistance"/>, which reduces the number of ticks generated.
        /// </summary>
        public double TickDistanceMultiplier = 1;

        public HitCircle HeadCircle;
        public SliderLegacyLastTick LastTick;

        public Slider()
        {
            SamplesBindable.ItemsAdded += _ => updateNestedSamples();
            SamplesBindable.ItemsRemoved += _ => updateNestedSamples();
            Path.Version.ValueChanged += _ => updateNestedPositions();
        }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(StartTime);

            double scoringDistance = BASE_SCORING_DISTANCE * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

            Velocity = scoringDistance / timingPoint.BeatLength;
            TickDistance = scoringDistance / difficulty.SliderTickRate * TickDistanceMultiplier;
        }

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            foreach (var e in
                SliderEventGenerator.Generate(StartTime, SpanDuration, Velocity, TickDistance, Path.Distance, this.SpanCount(), LegacyLastTickOffset))
            {
                switch (e.Type)
                {
                    case SliderEventType.Tick:
                        AddNested(new SliderTick
                        {
                            SpanIndex = e.SpanIndex,
                            SpanStartTime = e.SpanStartTime,
                            StartTime = e.Time,
                            Position = Position + Path.PositionAt(e.PathProgress),
                            StackHeight = StackHeight,
                            Scale = Scale,
                        });
                        break;

                    case SliderEventType.Head:
                        AddNested(HeadCircle = new SliderCircle
                        {
                            StartTime = e.Time,
                            Position = Position,
                            StackHeight = StackHeight,
                            SampleControlPoint = SampleControlPoint,
                        });
                        break;

                    case SliderEventType.LegacyLastTick:
                        // we need to use the LegacyLastTick here for compatibility reasons (difficulty).
                        AddNested(LastTick = new SliderLegacyLastTick(this)
                        {
                            StartTime = e.Time,
                            Position = EndPosition,
                            StackHeight = StackHeight
                        });
                        break;

                    case SliderEventType.Tail:
                        AddNested(new SliderTailCircle(this)
                        {
                            StartTime = e.Time,
                            Position = EndPosition,
                            StackHeight = StackHeight
                        });
                        break;

                    case SliderEventType.Repeat:
                        AddNested(new RepeatPoint
                        {
                            RepeatIndex = e.SpanIndex,
                            SpanDuration = SpanDuration,
                            StartTime = StartTime + (e.SpanIndex + 1) * SpanDuration,
                            Position = Position + Path.PositionAt(e.PathProgress),
                            StackHeight = StackHeight,
                            Scale = Scale,
                        });
                        break;
                }
            }

            updateNestedSamples();
        }

        private void updateNestedPositions()
        {
            endPositionCache.Invalidate();

            if (HeadCircle != null)
                HeadCircle.Position = Position;

            if (LastTick != null)
                LastTick.Position = EndPosition;
        }

        private void updateNestedSamples()
        {
            var firstSample = Samples.FirstOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL)
                              ?? Samples.FirstOrDefault(); // TODO: remove this when guaranteed sort is present for samples (https://github.com/ppy/osu/issues/1933)
            var sampleList = new List<HitSampleInfo>();

            if (firstSample != null)
            {
                sampleList.Add(new HitSampleInfo
                {
                    Bank = firstSample.Bank,
                    Volume = firstSample.Volume,
                    Name = @"slidertick",
                });
            }

            foreach (var tick in NestedHitObjects.OfType<SliderTick>())
                tick.Samples = sampleList;

            foreach (var repeat in NestedHitObjects.OfType<RepeatPoint>())
                repeat.Samples = getNodeSamples(repeat.RepeatIndex + 1);

            if (HeadCircle != null)
                HeadCircle.Samples = getNodeSamples(0);
        }

        private IList<HitSampleInfo> getNodeSamples(int nodeIndex) =>
            nodeIndex < NodeSamples.Count ? NodeSamples[nodeIndex] : Samples;

        public override Judgement CreateJudgement() => new OsuJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}

using UnityEngine;
using UnityEngine.UIElements;
using PlaytestingReviewer.Tracks;
using System;
using UnityEditor.Graphs;

namespace PlaytestingReviewer.Editors
{
    /// <summary>
    /// Represents a specialized UITrack specifically designed to handle and display
    /// metric-related information within the Playtesting Reviewer system.
    /// </summary>
    /// <remarks>
    /// To initialize UI remember to call UIInitialize
    /// </remarks>
    public class UIMetricTrack : UITrack
    {
        private readonly Track _track;
        private bool _initialized;

        private const float Size = 20;
        private const float CornerRadius = 10;
        private const float BorderWidth = 0.3f;
        private static readonly StyleColor BorderBottomColor = new StyleColor(Color.white);

        public UIMetricTrack(
            VisualElement description,
            VisualElement information,
            ITimePositionTranslator timeRelations,
            Track track
        )
            : base(description, information, timeRelations)
        {
            if (track.type != TrackType.Metric)
            {
                Debug.LogError(track.type + " is not a metric track");
                return;
            }
            _track = track;
            OnResize += DelayedConfigureTrack;
        }
        public void DelayedConfigureTrack()
        {
            if (_initialized || TimeRelations.GetTotalTime() == 0f) return;
            
            _initialized = true;
            InformationContainer.style.position = Position.Relative;
            DescriptionLabel.text = _track.name;
            ChangeBarColor(_track.color);
            CreateTimeMarkers(_track.color);
        }

        protected override void InitializeDescription(VisualElement description)
        {
            BarColor = new StyleColor(new Color(0.9f, 0.2f, 0.2f));
            base.InitializeDescription(description);
        }

        private void CreateTimeMarkers(Color color)
        {
            if (_track.instances == null || _track.instances.Count == 0)
                return;

            float totalDuration = TimeRelations.GetTotalTime();

            foreach (var instance in _track.instances)
            {
                if (!instance.ToDictionary().TryGetValue("time", out object timeValue)) continue;
                if (!float.TryParse(timeValue.ToString(), out float time)) continue;

                float normalizedTime = time / totalDuration;

                VisualElement timeMarker = CreateMarkerVisualElement(normalizedTime, color);

                string tooltipText = "";
                foreach (var kvp in instance.ToDictionary())
                {
                    tooltipText += $"\n{kvp.Key}: {kvp.Value}";
                }

                timeMarker.tooltip = tooltipText;

                InformationContainer.Add(timeMarker);
            }
        }

        private static VisualElement CreateMarkerVisualElement(float normalizedTime, Color color)
        {
            VisualElement timeMarker = new VisualElement
            {
                style =
                {
                    width = Size,
                    height = Size,

                    borderTopLeftRadius = CornerRadius,
                    borderTopRightRadius = CornerRadius,
                    borderBottomLeftRadius = CornerRadius,
                    borderBottomRightRadius = CornerRadius,

                    backgroundColor = new StyleColor(color),
                    borderTopColor = BorderBottomColor,
                    borderBottomColor = BorderBottomColor,
                    borderLeftColor = BorderBottomColor,
                    borderRightColor = BorderBottomColor,
                    borderTopWidth = BorderWidth,
                    borderBottomWidth = BorderWidth,
                    borderLeftWidth = BorderWidth,
                    borderRightWidth = BorderWidth,

                    position = Position.Absolute,

                    left = new Length(normalizedTime * 100, LengthUnit.Percent),

                    top = new Length(50, LengthUnit.Percent),

                    marginLeft = -Size / 2,
                    marginTop = -Size / 2,

                    rotate = new Rotate(45f)
                }
            };
            return timeMarker;
        }
    }
}
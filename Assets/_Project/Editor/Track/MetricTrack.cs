using UnityEngine;
using UnityEngine.UIElements;
using PlaytestingReviewer.Tracks;
using System;

namespace PlaytestingReviewer.Editors
{
    public class MetricTrack : UITrack
    {
        private Track _track;
        private bool _initialized;

        public MetricTrack(
            VisualElement description,
            VisualElement information,
            ITimePositionTranslator timeRelations,
            Track track
        )
            : base(description, information, timeRelations)
        {
            if (track.type != TrackType.Metric)
            {
                Debug.LogError(
                    "Tried creating metric track from a track that is not Metric. Actual type: " + track.type);
                return;
            }

            _track = track;
            OnResize += StartInitialization;
        }

        public void StartInitialization()
        {
            if (_initialized == false)
            {
                if (_timeRelations.GetTotalTime() != 0f)
                {
                    if (_initialized || _timeRelations.GetTotalTime() == 0f) return;
                    _initialized = true;

                    _informationContainer.style.position = Position.Relative;

                    _descriptionLabel.text = _track.name;
                }
            }

            CreateTimeMarkers();
        }

        private void CreateTimeMarkers()
        {
            if (_track.instances == null || _track.instances.Count == 0)
                return;

            float totalDuration = _timeRelations.GetTotalTime();

            foreach (var instance in _track.instances)
            {
                Debug.Log(instance.ToDictionary());
                if (!instance.ToDictionary().TryGetValue("time", out object timeValue)) continue;

                if (!float.TryParse(timeValue.ToString(), out float time)) continue;

                float normalizedTime = time / totalDuration;

                var timeMarker = new VisualElement
                {
                    style =
                    {
                        // Circular shape
                        width = 20,
                        height = 20,
                        borderTopLeftRadius = 50,
                        borderTopRightRadius = 50,
                        borderBottomLeftRadius = 50,
                        borderBottomRightRadius = 50,

                        // Background with a soft glow effect
                        backgroundColor = new StyleColor(new Color(0.9f, 0.2f, 0.2f)), // Softer red
                        unityBackgroundImageTintColor = new Color(1f, 1f, 1f, 0.1f), // Slight glow effect

                        // Add a nice border
                        borderTopWidth = 0.3f,
                        borderBottomWidth = 0.3f,
                        borderLeftWidth = 0.3f,
                        borderRightWidth = 0.3f,
                        borderTopColor = new StyleColor(Color.white),
                        borderBottomColor = new StyleColor(Color.white),
                        borderLeftColor = new StyleColor(Color.white),
                        borderRightColor = new StyleColor(Color.white),

                        // Position absolutely inside the container
                        position = Position.Absolute,

                        // Horizontal positioning (normalized percentage)
                        left = new Length(normalizedTime * 100, LengthUnit.Percent),

                        // Centering it vertically
                        top = new Length(50, LengthUnit.Percent),

                        // Offset by half its size to center it properly
                        marginLeft = -10,
                        marginTop = -10,

                        // Soft shadow to make it pop
                        unityTextOutlineColor = new Color(0, 0, 0, 0.5f),
                        unityTextOutlineWidth = 1
                    }
                };

                // Create tooltip with all track values
                var tooltipText = "Time: " + time.ToString("F2") + "s";
                foreach (var kvp in instance.ToDictionary())
                {
                    tooltipText += $"\n{kvp.Key}: {kvp.Value}";
                }

                timeMarker.tooltip = tooltipText;
                _informationContainer.Add(timeMarker);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UIElements;
using PlaytestingReviewer.Tracks;
using System;

namespace PlaytestingReviewer.Editor
{
    public class MetricTrack : UITrack
    {
        private Track _track;
        private bool _initialized;
        public MetricTrack(VisualElement description, VisualElement information, ITimePositionTranslator timeRelations, Track track)
            : base(description, information, timeRelations)
        {
            if (track.type != TrackType.Metric)
            {
                Debug.LogError("Tried creating metric track from a track that is not a metric, track type:" + track.type);
                return;
            }

            _track = track;

            OnResize += StartInitialization;
        }

        private void StartInitialization()
        {
            if (_initialized || _timeRelations.GetTotalTime() == 0f) return;
            _initialized = true;

            InitializeBoxes();
        }

        private void InitializeBoxes()
        {
            Track track = _track;
            if (track.instances == null || track.instances.Count == 0)
                return;

            foreach (var instance in track.instances)
            {
                float time = 0f;
                if (instance.ToDictionary().TryGetValue("time", out object timeValue))
                {
                    float.TryParse(timeValue.ToString(), out time);
                }
                else
                {
                    return;
                }

                float totalDuration = _timeRelations.GetTotalTime();
                float normalizedTime = time / totalDuration;

                // Use the percentual position to set the box placement dynamically
                var redBox = new VisualElement
                {
                    style =
                    {
                        width = 10,
                        height = 30,
                        backgroundColor = new StyleColor(Color.red),
                        position = Position.Relative,
                        left = new Length(normalizedTime * 100, LengthUnit.Percent)
                    }
                };

                _informationContainer.Add(redBox);
            }
        }
    }
}

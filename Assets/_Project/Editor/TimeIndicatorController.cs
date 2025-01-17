using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor;
using PlaytestingReviewer.Video;
using Unity.VisualScripting;
using System;

namespace PlaytestingReviewer.Editor
{
    public class TimeIndicatorController
    {
        private const int InitialSpaceBetweenIndicators = 13;
        private const int MinimumSpaceBetweenIndicators = 3;
        private const float LabelSize = 20f;

        private List<Label> _timeIndicators;
        private ScrollView timeScroll;
        private VisualElement timeView;

        private float _currentSpaceBetweenIndicators = 10f;
        private float _videoLength;
        private IVideoPlayer _videoPlayer;

        private bool _mouseDown;

        public TimeIndicatorController(VisualElement root, IVideoPlayer videoPlayer)
        {
            SetUpTimeControl(root);
            _videoPlayer = videoPlayer;

            timeView.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }


        private void SetUpTimeControl(VisualElement root)
        {
            timeScroll = root.Q<ScrollView>("TimeScroll");
            timeView = root.Q<VisualElement>("TimeView");

            timeScroll.RegisterCallback<WheelEvent>(OnMouseScroll);
        }

        public void ReloadIndicators(float videoLength)
        {
            _videoLength = videoLength;
            _timeIndicators = new List<Label>();
            float trackViewWidth = timeScroll.resolvedStyle.width;
            int indicatorCount = Mathf.FloorToInt(trackViewWidth / (InitialSpaceBetweenIndicators + LabelSize));

            for (int i = 0; i < indicatorCount; i++)
            {
                Label indicator = new Label { text = i.ToString() };

                indicator.AddToClassList("timeControl");

                if (i == 0)
                {
                    indicator.style.paddingLeft = 0;

                    indicator.style.marginLeft = 0;
                }
                else
                {
                    indicator.style.paddingLeft = 0;
                    indicator.style.marginLeft = InitialSpaceBetweenIndicators;
                }

                timeView.contentContainer.Add(indicator);
                _timeIndicators.Add(indicator);
            }

            UpdateTimeIndicators();
        }


        private void UpdateTimeIndicators()
        {
            float videoLength = _videoLength;
            string[] labels = GetNumericalLabelsFromVideoLength(_timeIndicators.Count, videoLength);

            for (int i = 0; i < _timeIndicators.Count; i++)
            {
                _timeIndicators[i].text = labels[i];
            }
        }

        private string[] GetNumericalLabelsFromVideoLength(int indicatorCount, float videoLength)
        {
            string[] labels = new string[indicatorCount];
            for (int i = 0; i < indicatorCount; i++)
            {
                float t = (float)i / (indicatorCount - 1);
                labels[i] = Mathf.Lerp(0, videoLength, t).ToString("0");
            }
            return labels;
        }

        private void OnMouseScroll(WheelEvent evt)
        {
            if (evt.ctrlKey)
            {
                ZoomTimeIndicators(evt.delta.y);
            }
        }

        private void ZoomTimeIndicators(float amount)
        {
            _currentSpaceBetweenIndicators += amount;

            if (_currentSpaceBetweenIndicators < MinimumSpaceBetweenIndicators)
            {
                _currentSpaceBetweenIndicators = MinimumSpaceBetweenIndicators;
            }

            foreach (VisualElement indicator in _timeIndicators)
            {
                indicator.style.marginLeft = _currentSpaceBetweenIndicators;
                indicator.style.marginRight = _currentSpaceBetweenIndicators;
            }
        }

        public float GetStartLabelPosition()
        {
            return _timeIndicators[0].resolvedStyle.marginLeft;
        }

        public float GetWorldPositionOfTime(float time)
        {
            if (_timeIndicators == null) { return 0; }
            float initialLabel = _timeIndicators[0].worldBound.x;


            float lastLabel = _timeIndicators[_timeIndicators.Count - 1].worldBound.x;
            float videoLength = _videoLength;
            return Mathf.Lerp(initialLabel, lastLabel, time / videoLength);
        }
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                Vector2 mousePosition = evt.mousePosition;
                float time = GetTimeFromWorldPosition(mousePosition.x);
                int targetFrame = Mathf.FloorToInt(time * _videoPlayer.GetVideoLengthFrames() / _videoLength);
                _videoPlayer.SetFrame(targetFrame);
            }
        }

        private float GetTimeFromWorldPosition(float x)
        {
            float initialLabel = _timeIndicators[0].worldBound.x;
            float lastLabel = _timeIndicators[_timeIndicators.Count - 1].worldBound.x;
            float videoLength = _videoLength;
            return Mathf.Lerp(0, videoLength, (x - initialLabel) / (lastLabel - initialLabel));
        }
    }
}



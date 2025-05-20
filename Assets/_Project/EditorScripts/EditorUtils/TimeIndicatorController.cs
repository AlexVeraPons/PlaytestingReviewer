using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using PlaytestingReviewer.Video;

namespace PlaytestingReviewer.Editors
{
    /// <summary>
    /// Controls the visual time indicators in the editor timeline, providing functionality
    /// to map time values to screen positions and vice versa. It also handles the setup,
    /// management, and updating of the time indicator labels.
    /// </summary>
    public class TimeIndicatorController : ITimePositionTranslator
    {
        // CONSTANTS
        private const int InitialSpaceBetweenIndicators = 13;
        private const float LabelSize = 20f;

        //Vidoe related
        private float _videoLength;
        private readonly IVideoPlayer _videoPlayer;

        //UI elements
        private List<Label> _timeIndicators;
        private ScrollView timeScroll;
        private VisualElement timeView;

        // Zooming and positioning
        private float _currentSpaceBetweenIndicators = 10f;
        private readonly ZoomUpdater _zoomUpdater;


        // Constructor and setup

        public TimeIndicatorController(VisualElement root, IVideoPlayer videoPlayer, ZoomUpdater zoomUpdater)
        {
            _zoomUpdater = zoomUpdater;
            _videoPlayer = videoPlayer;

            SetUpTimeControl(root);

            timeView.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void SetUpTimeControl(VisualElement root)
        {
            timeScroll = root.Q<ScrollView>("TimeScroll");
            timeView = root.Q<VisualElement>("TimeView");

            timeScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _zoomUpdater.OnZoomed += ZoomTimeIndicators;
        }

        // UI management
        public void ReloadIndicators(float videoLength)
        {
            timeView.contentContainer.Clear();
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
                float normalizedIndex = (float)i / (indicatorCount - 1);
                labels[i] = Mathf.Round(Mathf.Lerp(0, videoLength, normalizedIndex) * 10) / 10f + "s";
            }

            return labels;
        }

        //User interaction
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0) //left click
            {
                Vector2 mousePosition = evt.mousePosition;
                float time = GetTimeFromScreenX(mousePosition.x);
                int targetFrame = Mathf.FloorToInt(time * _videoPlayer.GetVideoLengthFrames() / _videoLength); //find the frame closest to the mouse position
                _videoPlayer.SetFrame(targetFrame);
            }
        }

        private void ZoomTimeIndicators(float amount)
        {
            _currentSpaceBetweenIndicators += amount;

            foreach (VisualElement indicator in _timeIndicators)
            {
                indicator.style.marginLeft = _currentSpaceBetweenIndicators;
                indicator.style.marginRight = _currentSpaceBetweenIndicators;
            }
        }

        //Time calculations
        public float GetXPositionFromTime(float time)
        {
            if (_timeIndicators == null)
            {
                return 0;
            }

            float initialLabel = _timeIndicators[0].worldBound.x;


            float lastLabel = _timeIndicators[_timeIndicators.Count - 1].worldBound.x;
            float videoLength = _videoLength;
            return Mathf.Lerp(initialLabel, lastLabel, time / videoLength); // Map the time to the screen
        }

        public float GetTimeFromScreenX(float x)
        {
            float initialLabel = _timeIndicators[0].worldBound.x;
            float lastLabel = _timeIndicators[_timeIndicators.Count - 1].worldBound.x;
            float videoLength = _videoLength;

            float normalizedPosition = (x - initialLabel) / (lastLabel - initialLabel);

            // Map the normalized position to the video length
            return Mathf.Lerp(0, videoLength, normalizedPosition);
        }

        //Utility
        public bool IsSetupComplete()
        {
            if (_timeIndicators == null || _timeIndicators.Count == 0)
            {
                return false;
            }

            // if there is no video initialized then the labels will overlap
            if (_timeIndicators[0].worldBound.x == _timeIndicators[_timeIndicators.Count - 1].worldBound.x)
            {
                return false;
            } //

            return true;
        }

        public float GetTotalTime()
        {
            return _videoLength;
        }
    }
}
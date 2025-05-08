using System;
using PlaytestingReviewer.Video;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlaytestingReviewer.Editors
{
    /// <summary>
    /// Represents a visual time indicator (needle) within the video playback interface.
    /// This needle shows the current playback position of the video on the timeline
    /// and updates dynamically as the video plays or when the user interacts with the timeline.
    /// </summary>
    public class TimeNeedle
    {
        private ITimePositionTranslator _timeIndicatorController;
        
        // External visual elements
        private VisualElement _root;
        private IVideoPlayer _videoPlayer;
        private VisualElement _trackView;
        
        //Own visual elements
        private VisualElement _timeNeedle;
        private VisualElement _arrow;

        // Initialisation
        public TimeNeedle(VisualElement root, TimeIndicatorController timeIndicatorController, IVideoPlayer videoPlayer)
        {
            _timeIndicatorController = timeIndicatorController;
            _root = root;
            _videoPlayer = videoPlayer;
            EditorApplication.update += Update;
        }

        public void Initialize()
        {
            SetUpTimeNeedle(_root);
        }

        private void SetUpTimeNeedle(VisualElement root)
        {
            if (_timeNeedle != null)
            {
                return;
            }

            if (_trackView == null)
            {
                _trackView = root.Q<VisualElement>("TrackView");
            }

            InitializeTimeNeedleBase(_trackView);
            CreateArrow(_timeNeedle);
            _trackView.Add(_timeNeedle);
        }

        private void InitializeTimeNeedleBase(VisualElement trackView)
        {
            _timeNeedle = new VisualElement();
            _timeNeedle.AddToClassList("timeNeedle");
            _timeNeedle.style.position = Position.Absolute;
            _timeNeedle.style.left = 0;
            _timeNeedle.style.width = 3;
            _timeNeedle.style.height = trackView.resolvedStyle.height;

            _timeNeedle.style.backgroundColor = new StyleColor(new Color(0.7f, 0.7f, 0.7f, 1f));
        }

        private void CreateArrow(VisualElement parent)
        {
            _arrow = new VisualElement();
            _arrow.AddToClassList("timeNeedleArrow");
            _arrow.style.position = Position.Absolute;
            _arrow.style.left = -2;
            _arrow.style.width = 7;
            _arrow.style.height = 7;
            _arrow.style.backgroundColor = new StyleColor(new Color(0.7f, 0.7f, 0.7f, 1f));
            _arrow.style.rotate = new StyleRotate(new Rotate(45));
            parent.Add(_arrow);
        }

        //Updating the indicator 
        
        private void Update()
        {
            if (_videoPlayer == null || _timeNeedle == null)
            {
                return;
            }

            if (_trackView == null)
            {
                _trackView = _root.Q<VisualElement>("TrackView");
            }

            UpdatePosition();
            UpdateNeedleSize();
        }

        private void UpdatePosition()
        {
            float position = _timeIndicatorController.GetXPositionFromTime(_videoPlayer.GetCurrentTime()); // gets the position of the needle

            // if its outside of view place it to the left
            float desiredPosition = position - _trackView.worldBound.x;
            if (desiredPosition < 0)
            {
                desiredPosition = 0;
            }

            _timeNeedle.style.left = desiredPosition;
        }

        private void UpdateNeedleSize()
        {
            _timeNeedle.style.height = _trackView.resolvedStyle.height;
        }
    }
}
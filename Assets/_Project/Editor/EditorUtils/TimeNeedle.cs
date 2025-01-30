using System;
using Codice.Client.Common;
using PlaytestingReviewer.Video;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlaytestingReviewer.Editors
{
    public class TimeNeedle
    {
        private TimeIndicatorController _timeIndicatorController;
        private VisualElement _root;
        private IVideoPlayer _videoPlayer;
        private VisualElement _timeNeedle;

        public TimeNeedle(VisualElement root, TimeIndicatorController timeIndicatorController, IVideoPlayer videoPlayer)
        {
            _timeIndicatorController = timeIndicatorController;
            _root = root;
            _videoPlayer = videoPlayer;
            EditorApplication.update += updatePosition;
        }

        private void updatePosition()
        {
            if (_videoPlayer == null || _timeNeedle == null) { return; }

            float position = _timeIndicatorController.GetLeftWorldPositionOfTime(_videoPlayer.GetCurrentTime());
            //set the world position of the needle
            VisualElement TrackView = _root.Q<VisualElement>("TrackView");

            float desiredPosition = position - TrackView.worldBound.x;
            if (desiredPosition < 0) { desiredPosition = 0; }
            _timeNeedle.style.left = desiredPosition;

        }
        public void Initialize()
        {
            SetUpTimeNeedle(_root);
        }
        private void SetUpTimeNeedle(VisualElement root)
        {
            if (_timeNeedle != null) { return; }

            VisualElement TrackView = root.Q<VisualElement>("TrackView");

            _timeNeedle = new VisualElement();
            _timeNeedle.AddToClassList("timeNeedle");
            _timeNeedle.style.position = Position.Absolute;
            _timeNeedle.style.left = 0;
            _timeNeedle.style.width = 3;
            _timeNeedle.style.height = TrackView.resolvedStyle.height;

            _timeNeedle.style.backgroundColor = new StyleColor(new Color(0.7f, 0.7f, 0.7f, 1f));

            // Create the arrow
            VisualElement arrow = new VisualElement();
            arrow.AddToClassList("timeNeedleArrow");
            arrow.style.position = Position.Absolute;
            arrow.style.left = -2; // Adjust as needed
            arrow.style.width = 7;
            arrow.style.height = 7;
            arrow.style.backgroundColor = new StyleColor(new Color(0.7f, 0.7f, 0.7f, 1f));
            arrow.style.rotate = new StyleRotate(new Rotate(45));

            _timeNeedle.Add(arrow);
            TrackView.Add(_timeNeedle);
        }
    }
}
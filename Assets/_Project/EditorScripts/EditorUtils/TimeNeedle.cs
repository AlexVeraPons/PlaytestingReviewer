using System;
using UnityEngine;
using UnityEngine.UIElements;
using PlaytestingReviewer.Video;

#if UNITY_EDITOR                 // NEW
using UnityEditor;
#endif

namespace PlaytestingReviewer.Editors
{
    /// <summary>
    /// Shows the current playback position on the timeline.
    /// </summary>
    public class TimeNeedle
    {
        // -------------------------------------------------- fields
        readonly ITimePositionTranslator _timeTranslator;
        readonly VisualElement _root;
        readonly IVideoPlayer  _videoPlayer;

        VisualElement _trackView;
        VisualElement _timeNeedle;
        VisualElement _arrow;

        // -------------------------------------------------- ctor
        public TimeNeedle(VisualElement root,
                          TimeIndicatorController translator,
                          IVideoPlayer videoPlayer)
        {
            _root           = root;
            _timeTranslator = translator;
            _videoPlayer    = videoPlayer;

#if UNITY_EDITOR
            EditorApplication.update += Update;
#else
            RuntimeUpdateDispatcher.Tick += Update;      // NEW
#endif
        }

        // -------------------------------------------------- public API
        public void Initialize() => SetUpTimeNeedle(_root);

        // -------------------------------------------------- setup helpers
        void SetUpTimeNeedle(VisualElement root)
        {
            if (_timeNeedle != null) return;

            _trackView ??= root.Q<VisualElement>("TrackView");

            _timeNeedle = new VisualElement();
            _timeNeedle.AddToClassList("timeNeedle");
            _timeNeedle.style.position        = Position.Absolute;
            _timeNeedle.style.left            = 0;
            _timeNeedle.style.width           = 3;
            _timeNeedle.style.height          = _trackView.resolvedStyle.height;
            _timeNeedle.style.backgroundColor = new StyleColor(new Color(.7f, .7f, .7f, 1f));

            CreateArrow(_timeNeedle);
            _trackView.Add(_timeNeedle);
        }

        void CreateArrow(VisualElement parent)
        {
            _arrow = new VisualElement();
            _arrow.AddToClassList("timeNeedleArrow");
            _arrow.style.position        = Position.Absolute;
            _arrow.style.left            = -2;
            _arrow.style.width           = 7;
            _arrow.style.height          = 7;
            _arrow.style.backgroundColor = new StyleColor(new Color(.7f, .7f, .7f, 1f));
            _arrow.style.rotate          = new StyleRotate(new Rotate(45));
            parent.Add(_arrow);
        }

        // -------------------------------------------------- per-frame update
        void Update()
        {
            if (_videoPlayer == null || _timeNeedle == null) return;

            _trackView ??= _root.Q<VisualElement>("TrackView");

            UpdatePosition();
            UpdateNeedleSize();
        }

        void UpdatePosition()
        {
            float pos = _timeTranslator.GetXPositionFromTime(_videoPlayer.GetCurrentTime());

            // convert world-space X to local space of track view
            float desired = pos - _trackView.worldBound.x;
            if (desired < 0) desired = 0;

            _timeNeedle.style.left = desired;
        }

        void UpdateNeedleSize() =>
            _timeNeedle.style.height = _trackView.resolvedStyle.height;
    }
}

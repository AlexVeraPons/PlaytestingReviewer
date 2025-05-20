using System;
using UnityEngine;
using UnityEngine.UIElements;
using PlaytestingReviewer.Video;

#if UNITY_EDITOR
using UnityEditor;                       // editor-only
#endif

namespace PlaytestingReviewer.Editors
{
    /// <summary>
    /// Manages video playback UI and raises events when the length changes
    /// or the user hits Play.
    /// </summary>
    public class VideoController
    {
        public Action<float> OnNewVideoLengthAvailable;
        public Action        OnPlay;

        /* ------------------------------------------------------------ */
        public IVideoPlayer VideoPlayer => _videoPlayer;
        IVideoPlayer  _videoPlayer;
        IntegerField  _frameAmountField;

        bool  _isPlaying;
        float _videoLength;

        int  FrameAmount  => _frameAmountField?.value ?? 1;

        /* ------------------------------------------------------------ */
        public VideoController(VisualElement root)
        {
            SetUpVideoControls(root);

#if UNITY_EDITOR
            EditorApplication.update += Update;
#else
            RuntimeUpdateDispatcher.Tick += Update;          // NEW
#endif
        }

        /* ---------------- initial UI wiring ------------------------ */
        void SetUpVideoControls(VisualElement root)
        {
            _videoPlayer = root.Q<UIVideoPlayer>("VideoPlayer");

            AssignButton(root, "Play",          TogglePlayPause);
            AssignButton(root, "NextFrame",     () => _videoPlayer.NextFrame(FrameAmount));
            AssignButton(root, "PreviousFrame", () => _videoPlayer.PreviousFrame(FrameAmount));
            AssignButton(root, "Start",         _videoPlayer.GoToStart);
            AssignButton(root, "End",           _videoPlayer.GoToEnd);

            _frameAmountField = root.Q<IntegerField>("FrameSkipAmmount");
        }

        /* ---------------- heartbeat ------------------------ */
        void Update()
        {
            if (_videoPlayer == null || !_isPlaying) return;
            CheckVideoLength();
        }

        void CheckVideoLength()
        {
            float newLen = _videoPlayer.GetVideoLengthSeconds();
            if (Mathf.Abs(newLen - _videoLength) > 0.01f)
            {
                _videoLength = newLen;
                OnNewVideoLengthAvailable?.Invoke(_videoLength);
            }
        }

        /* ---------------- actions -------------------------- */
        void AssignButton(VisualElement root, string name, Action action)
        {
            var btn = root.Q<Button>(name);
            if (btn != null) btn.clicked += action;
        }

        void TogglePlayPause()
        {
            if (_videoPlayer.IsPlaying())
            {
                _videoPlayer.Pause();
                _isPlaying = false;
            }
            else
            {
                _videoPlayer.Play();
                _isPlaying = true;
                OnPlay?.Invoke();
            }
        }

        public void SetVideo(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("VideoController: empty path");
                return;
            }

            _videoPlayer.SetVideo(path);
            CheckVideoLength();
        }

        public void GoToVideoFrame(int frame)
        {
            if (frame < 0 || frame > _videoPlayer.GetVideoLengthFrames()) return;
            _videoPlayer.SetFrame(frame);
        }

        /* ---------------- getters -------------------------- */
        public float GetVideoLength() => _videoPlayer.GetVideoLengthSeconds();
        public bool  IsPlaying()      => _videoPlayer.IsPlaying();
    }
}

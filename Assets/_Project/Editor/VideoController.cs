using System;
using PlaytestingReviewer.Video;
using UnityEngine.UIElements;
using UnityEditor;
using static Codice.Client.BaseCommands.KnownCommandOptions;
using UnityEngine.Video;

namespace PlaytestingReviewer.Editor
{
    public class VideoController
    {
        public Action<float> OnNewVideoLengthAvailable;

        private IVideoPlayer _videoPlayer;
        private IntegerField _frameAmountField;
        private int FrameAmount => _frameAmountField.value;

        private bool _isPlaying = false;
        private float _videoLength = 0f;


        public VideoController(VisualElement root)
        {
            SetUpVideoControls(root);
            
            EditorApplication.update += Update;
        }

        private void Update()
        {
            if (_videoPlayer == null || _isPlaying == false) {return;}
            if(_videoPlayer.GetVideoLength() != _videoLength)
            {
                _videoLength = _videoPlayer.GetVideoLength();
                OnNewVideoLengthAvailable?.Invoke(_videoLength);
            }
        }

        private void SetUpVideoControls(VisualElement root)
        {
            _videoPlayer = root.Q<UIVideoPlayer>("VideoPlayer");
            AssignButton(root, "Play", TogglePlayPause);
            AssignButton(root, "NextFrame", () => _videoPlayer.NextFrame(FrameAmount));
            AssignButton(root, "PreviousFrame", () => _videoPlayer.PreviousFrame(FrameAmount));
            AssignButton(root, "Start", _videoPlayer.GoToStart);
            AssignButton(root, "End", _videoPlayer.GoToEnd);

            _frameAmountField = root.Q<IntegerField>("FrameSkipAmount");
        }

        private void AssignButton(VisualElement root, string buttonName, System.Action action)
        {
            Button button = root.Q<Button>(buttonName);
            if (button != null)
            {
                button.clicked += action;
            }
        }

        private void TogglePlayPause()
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

            }
        }

        public float GetVideoLength() => _videoPlayer.GetVideoLength();
        public bool IsPlaying() => _videoPlayer.IsPlaying();

    }
}

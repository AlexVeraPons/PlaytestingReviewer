using System;
using PlaytestingReviewer.Video;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace PlaytestingReviewer.Editors
{
    /// <summary>
    /// The VideoController class manages video playback, providing control functionalities
    /// and handling interactions with the video player. It also facilitates communication
    /// between related components in the context of video playback and editing.
    ///
    /// This also looks for the buttons and assigns them.
    /// </summary>
    public class VideoController
    {
        public Action<float> OnNewVideoLengthAvailable;
        public Action OnPlay;

        private IVideoPlayer _videoPlayer;
        public IVideoPlayer VideoPlayer => _videoPlayer;
        private IntegerField _frameAmountField;
        private int FrameAmount => _frameAmountField?.value ?? 1;

        private bool _isPlaying = false;
        private float _videoLength = 0f;
            
        // Initialisation
        
        public VideoController(VisualElement root)
        {
            SetUpVideoControls(root);
            EditorApplication.update += Update;
        }
        
        //TODO: the setup should be done by the editor, this way ths class is not dependant on all those buttons to exist
        private void SetUpVideoControls(VisualElement root)
        {
            _videoPlayer = root.Q<UIVideoPlayer>("VideoPlayer");
            AssignButton(root, "Play", TogglePlayPause);
            AssignButton(root, "NextFrame", () => _videoPlayer.NextFrame(FrameAmount));
            AssignButton(root, "PreviousFrame", () => _videoPlayer.PreviousFrame(FrameAmount));
            AssignButton(root, "Start", _videoPlayer.GoToStart);
            AssignButton(root, "End", _videoPlayer.GoToEnd);

            _frameAmountField = root.Q<IntegerField>("FrameSkipAmmount");
        }
        
        // Update 
        
        private void Update()
        {
            if (_videoPlayer == null || _isPlaying == false) { return; }
            
            CheckVideoLength();
        }
        
        private void CheckVideoLength()
        {
            float newLength = _videoPlayer.GetVideoLengthSeconds();
            if (Math.Abs(newLength - _videoLength) > 0.01f) 
            {
                _videoLength = newLength;
                OnNewVideoLengthAvailable?.Invoke(_videoLength);
            }
        }
        
        // Actions

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
                OnPlay?.Invoke();
            }
        }

        /// <summary>
        /// Sets the video to be played using the specified file path. 
        /// </summary>
        /// <param name="path">The file path to the video to be loaded.</param>
        public void SetVideo(string path)

        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("VideoController: Attempted to set video with an invalid path.");
                return;
            }
            
            _videoPlayer.SetVideo(path);
            CheckVideoLength();
        }

        /// <summary>
        /// Moves the playback to a specified video frame.
        /// </summary>
        /// <param name="frame">The frame index to seek to in the video.</param>
        public void GoToVideoFrame(int frame)
        {
            if (_videoPlayer.GetVideoLengthFrames() < frame) return;
            if (frame < 0) return;
            _videoPlayer.SetFrame(frame);
        }
        
        // Getters
        
        public float GetVideoLength() => _videoPlayer.GetVideoLengthSeconds();
        public bool IsPlaying() => _videoPlayer.IsPlaying();
    }
}

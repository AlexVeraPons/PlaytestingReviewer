using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlaytestingReviewer.Video
{
    [UxmlElement]
    public partial class UIVideoPlayer : VisualElement, IVideoPlayer
    {
        private string _defaultImagePath = PathManager.DefaultImagePath;
        private string _defaultVideoPath = PathManager.DefaultVideoPath;

        private VisualElement _buttonsContainer;
        private VisualElement _videoContainer;
        private Image _videoRenderer;
        private UnityEngine.Video.VideoPlayer _videoPlayer;
        private RenderTexture _renderTexture;
        private float _currentTime = 0;

        public UIVideoPlayer()
        {
            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;

            _videoContainer = new VisualElement();
            _videoContainer.style.alignItems = Align.FlexStart;
            _videoContainer.style.flexGrow = 0;


            Add(_videoContainer);

            _videoRenderer = new Image();
            _videoRenderer.image = AssetDatabase.LoadAssetAtPath<Texture2D>(_defaultImagePath);
            _videoContainer.Add(_videoRenderer);
            _videoContainer.style.alignSelf = Align.FlexStart;


            var defaultTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(_defaultImagePath);
            if (defaultTexture != null)
            {
                _videoRenderer.image = defaultTexture;
            }
            else
            {
                Debug.LogWarning($"Default image not found at '{_defaultImagePath}'");
            }

            InitializeVideoPlayer();
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void InitializeVideoPlayer()
        {
            GameObject videoObject = new GameObject("CustomVideoPlayer") { hideFlags = HideFlags.HideAndDontSave };

            _videoPlayer = videoObject.AddComponent<UnityEngine.Video.VideoPlayer>();
            _videoPlayer.playOnAwake = false;
            _videoPlayer.isLooping = false;

            _renderTexture = new RenderTexture(1920, 1080, 0);
            _videoPlayer.targetTexture = _renderTexture;

            _videoPlayer.url = _defaultVideoPath;

            EditorApplication.update += UpdateVideoFrame;

        }

        public void Play()
        {
            if (_videoPlayer != null && !_videoPlayer.isPlaying)
            {
                _videoPlayer.Play();
                _videoPlayer.time = _currentTime;
            }
        }

        public void Pause()
        {
            if (_videoPlayer != null && _videoPlayer.isPlaying)
            {
                _videoPlayer.Pause();
                _currentTime = (float)_videoPlayer.time;
            }
        }

        public void Stop()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();
            }
        }
        public void NextFrame(int frameCount = 1)
        {
            if (_videoPlayer == null || _videoPlayer.isPlaying) { return; }

            if ((ulong)(_videoPlayer.frame + frameCount) <= _videoPlayer.frameCount)
            {
                _videoPlayer.frame += frameCount;
            }

            UpdateVideoFrame();
        }

        public void PreviousFrame(int frameCount = 1)
        {
            if (_videoPlayer == null || _videoPlayer.isPlaying) { return; }

            if (_videoPlayer.frame - frameCount >= 0)
            {
                _videoPlayer.frame -= frameCount;
            }

            UpdateVideoFrame();
        }

        public void GoToStart()
        {
            _videoPlayer.frame = 0;
        }

        public void GoToEnd()
        {
            _videoPlayer.frame = (long)_videoPlayer.frameCount - 1;
        }

        public bool IsPlaying()
        {
            return _videoPlayer.isPlaying;
        }

        public float GetVideoLength()
        {
            if (_videoPlayer == null)
            {
                Debug.LogWarning("Video player is null");
                return 0;
            }

            return (float)_videoPlayer.length;
        }


        public void UpdateVideoFrame()
        {
            _videoPlayer.skipOnDrop = true;
            if (_videoPlayer.isPlaying == false)
            {
                return;
            }

            if (_videoPlayer != null && _videoPlayer.isPrepared && _renderTexture != null && _videoRenderer != null)
            {
                MarkDirtyRepaint();
                _videoRenderer.image = _videoPlayer.texture;
            }
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            EditorApplication.update -= UpdateVideoFrame;
            if (_renderTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(_renderTexture);
            }
            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();
                UnityEngine.Object.DestroyImmediate(_videoPlayer.gameObject);
            }
        }

        public float GetVideoLengthSeconds()
        {
            float videoLength = 0;
            if (_videoPlayer != null)
            {
                videoLength = (float)_videoPlayer.length;
            }

            return videoLength;
        }

        public float GetCurrentTime()
        {
            float currentTime = 0;
            if (_videoPlayer != null)
            {
                currentTime = (float)_videoPlayer.time;
            }

            return currentTime;
        }

        public int GetVideoLengthFrames()
        {
            int videoLengthFrames = 0;
            if (_videoPlayer != null)
            {
                videoLengthFrames = (int)_videoPlayer.frameCount;
            }

            return videoLengthFrames;
        }

        public int GetCurrentFrame()
        {
            int currentFrame = 0;
            if (_videoPlayer != null)
            {
                currentFrame = (int)_videoPlayer.frame;
            }

            return currentFrame;
        }

        public void SetFrame(int frame)
        {
            if (_videoPlayer == null) { return; }

            if (frame >= 0 && frame < (long)_videoPlayer.frameCount)
            {
                _videoPlayer.frame = frame;
            }
        }
    }
}

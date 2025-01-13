using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIAssets
{
    [UxmlElement]
    public partial class UIVideoPlayer : VisualElement
    {
        private string _defaultImagePath = PathManager.DefaultImagePath;
        private string _defaultVideoPath = PathManager.DefaultVideoPath;

        private Button _playButton, _pauseButton, _stopButton;
        private VisualElement _buttonsContainer;
        private VisualElement _videoContainer;
        private Image _videoRenderer;
        private UnityEngine.Video.VideoPlayer _videoPlayer;
        private RenderTexture _renderTexture;

        public UIVideoPlayer()
        {
            style.flexDirection = FlexDirection.Column;
            

            _videoContainer = new VisualElement();
            _videoContainer.style.width = Length.Percent(100);
            _videoContainer.RegisterCallback<GeometryChangedEvent>(OnVideoContainerResized);

            Add(_videoContainer);

            _videoRenderer = new Image();
            _videoRenderer.style.width = Length.Percent(100);
            _videoRenderer.style.height = Length.Percent(100);
            _videoRenderer.scaleMode = ScaleMode.ScaleToFit;
            _videoRenderer.image = AssetDatabase.LoadAssetAtPath<Texture2D>(_defaultImagePath);

            _videoContainer.Add(_videoRenderer);

            var defaultTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(_defaultImagePath);
            if (defaultTexture != null)
            {
                _videoRenderer.image = defaultTexture;
            }
            else
            {
                Debug.LogWarning($"Default image not found at '{_defaultImagePath}'");
            }

            _buttonsContainer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };
            Add(_buttonsContainer);
            _playButton = new Button(PlayVideo) { text = "Play" };
            _pauseButton = new Button(PauseVideo) { text = "Pause" };
            _stopButton = new Button(StopVideo) { text = "Stop" };

            var buttons = new[] { _playButton, _pauseButton, _stopButton };
            foreach (var button in buttons)
            {
                button.style.width = 40;
                button.style.height = 30;
                button.style.borderBottomWidth = 0;
                button.style.borderTopWidth = 0;
                button.style.borderLeftWidth = 0;
                button.style.borderRightWidth = 0;
                button.style.marginLeft = 1;
                button.style.marginRight = 1;
            }

            _buttonsContainer.Add(_playButton);
            _buttonsContainer.Add(_pauseButton);
            _buttonsContainer.Add(_stopButton);

            InitializeVideoPlayer();
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnVideoContainerResized(GeometryChangedEvent evt)
        {
            float containerWidth = _videoContainer.resolvedStyle.width;
            float desiredHeight = containerWidth * (9f / 16f);

            _videoContainer.style.height = desiredHeight;
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

        private void PlayVideo()
        {
            if (_videoPlayer != null && !_videoPlayer.isPlaying)
            {
                _videoPlayer.Play();
            }
        }

        private void PauseVideo()
        {
            if (_videoPlayer != null && _videoPlayer.isPlaying)
            {
                _videoPlayer.Pause();
            }
        }

        private void StopVideo()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();
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
    }
}

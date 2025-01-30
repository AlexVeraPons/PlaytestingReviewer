using System.Collections.Generic;
using System.IO;
using PlaytestingReviewer.Video;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;

namespace PlaytestingReviewer.Editors
{
    public class VideoPreviewTrack : UITrack
    {
        private IVideoPlayer _videoPlayer;

        private const int pixelHeight = 55;
        private const int pixelWidth = 80;
        private readonly List<Image> _previews = new List<Image>();

        private bool _inNeedOfImages = false;
        private bool _isRefreshing = false; 

        private bool _awaitingRefresh = false;
        private float _timeSinceLastRequest = 0f;
        private const float RefreshDelay = 0.5f;

        public VideoPreviewTrack(
            VisualElement description,
            VisualElement information,
            ITimePositionTranslator timeRelations,
            IVideoPlayer videoPlayer)
            : base(description, information, timeRelations)
        {
            _videoPlayer = videoPlayer;
            _timeRelations = timeRelations;
        }

        protected override void Initialization(VisualElement description, VisualElement information, ITimePositionTranslator timeRelations)
        {
            _trackHeight = 60;
            _title = "VideoPreview";
            base.Initialization(description, information, timeRelations);
            OnResize += Resized;
            _resizeDebounceDuration = 2f;

        }


        protected void Resized()
        {
            int amountOfPreviews = (int)_informationContainer.resolvedStyle.width / pixelWidth;

            _informationContainer.Clear();
            _previews.Clear();

            for (int i = 0; i < amountOfPreviews; i++)
            {
                var previewBox = new Image
                {
                    style =
                    {
                        width = pixelWidth,
                        height = pixelHeight,
                        backgroundColor = new StyleColor(Color.black)
                    }
                };

                _informationContainer.Add(previewBox);
                _previews.Add(previewBox);
            }

            _inNeedOfImages = true;
        }

        protected override void TrackUpdate()
        {
            base.TrackUpdate();

            if (_inNeedOfImages && _timeRelations.SetupComplete() && !_isRefreshing)
            {
                _awaitingRefresh = true;
                _timeSinceLastRequest = 0f;
                
                _inNeedOfImages = false;
            }

            if (_awaitingRefresh)
            {
                _timeSinceLastRequest += Time.deltaTime;
                if (_timeSinceLastRequest >= RefreshDelay)
                {
                    _awaitingRefresh = false;
                    RefreshPreviewsBatch();
                }
            }
        }

        /// <summary>
        /// Gathers times for each preview frame, calls FFmpeg for them in one batch,
        /// then applies the resulting textures to the preview boxes.
        /// </summary>
        private async void RefreshPreviewsBatch()
        {
            _isRefreshing = true;

            var times = new List<float>();
            foreach (var preview in _previews)
            {
                float time = _timeRelations.GetTimeFromLeftWorldPosition(preview.worldBound.x);
                times.Add(time);
            }

            string videoPath = _videoPlayer.GetVideoPath();
            var framePaths = await FFmpegUtils.ExtractBatchFramesAsync(videoPath, times, "previewBatch");

            for (int i = 0; i < framePaths.Count; i++)
            {
                string filePath = framePaths[i];
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    continue;

                // Create and assign the texture
                byte[] fileBytes = File.ReadAllBytes(filePath);
                var texture = new Texture2D(2, 2);
                texture.LoadImage(fileBytes);
                _previews[i].style.backgroundImage = new StyleBackground(texture);

                File.Delete(filePath);
            }

            _isRefreshing = false;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using PlaytestingReviewer.Video;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;

namespace PlaytestingReviewer.Editor
{
    public class VideoPreviewTrack : Track
    {
        private IVideoPlayer _videoPlayer;

        private const int pixelHeight = 55;
        private const int pixelWidth = 80;
        private readonly List<Image> _previews;

        private bool _inNeedOfImages = false;
        private bool _isRefreshing = false; // Prevents multiple simultaneous updates

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
            _previews = new List<Image>();
            _timeRelations = timeRelations;

            OnResize += Resized;

            _resizeDebounceDuration = 2f;
        }

        protected override void PreInitialization()
        {
            // Adjust track height as needed
            _trackHeight = 60;
        }

        protected void Resized()
        {
            int amountOfPreviews = (int)_informationContainer.resolvedStyle.width / pixelWidth;

            // Clear existing previews
            _informationContainer.Clear();
            _previews.Clear();

            // Create new preview boxes
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

            // Mark that we need new images
            _inNeedOfImages = true;
        }

        protected override void TrackUpdate()
        {
            base.TrackUpdate();

            // If we need a refresh and the video & time setup are valid, queue a refresh.
            if (_inNeedOfImages && _timeRelations.SetupComplete() && !_isRefreshing)
            {
                // Reset for a delayed refresh
                _awaitingRefresh = true;
                _timeSinceLastRequest = 0f;
                
                // We don't want to repeatedly trigger 
                // the "inNeed" flag until the refresh actually happens.
                _inNeedOfImages = false;
            }

            // If a refresh is queued, count up until we hit the half-second mark.
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

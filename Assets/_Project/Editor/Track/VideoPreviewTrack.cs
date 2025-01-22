using System.Collections.Generic;
using PlaytestingReviewer.Video;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlaytestingReviewer.Editor
{
    public class VideoPreviewTrack : Track
    {
        private IVideoPlayer _videoPlayer;

        private const int pixelHeight = 55;
        private const int pixelWidth = 80;
        private List<Image> _previews;

        private bool _inNeedOfImages = false;
        public VideoPreviewTrack(VisualElement description, VisualElement information, IProvideTimeRelations timeRelations, IVideoPlayer videoPlayer)
        : base(description, information, timeRelations)
        {
            _videoPlayer = videoPlayer;
            _previews = new List<Image>();
            _timeRelations = timeRelations;
            EditorApplication.update += Update;
        }

        private void Update()
        {
            if (_inNeedOfImages == false) { return; }
            if (_timeRelations.SetupComplete() == false) { return; }

            _inNeedOfImages = false;

            foreach (Image preview in _previews)
            {
                float time = _timeRelations.GetTimeFromLeftWorldPosition(preview.resolvedStyle.left);
            }
        }

        protected override void PreInitialization()
        {
            _trackHeight = 60;
        }
        protected override void OnResize(VisualElement informationZone)
        {
            int ammountOfPreviews = (int)informationZone.resolvedStyle.width / pixelWidth;
            for (int i = 0; i < ammountOfPreviews; i++)
            {
                Image previewBox = new Image();
                previewBox.style.width = pixelWidth;
                previewBox.style.height = pixelHeight;
                previewBox.style.backgroundColor = new StyleColor(UnityEngine.Color.black);
                previewBox.style.backgroundColor = new StyleColor(new UnityEngine.Color(
                    UnityEngine.Random.value,
                    UnityEngine.Random.value,
                    UnityEngine.Random.value));

                informationZone.Add(previewBox);
                _previews.Add(previewBox);
            }

            _inNeedOfImages = true;
        }
    }
}
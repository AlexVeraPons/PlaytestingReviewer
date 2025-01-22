using System.Collections.Generic;
using PlaytestingReviewer.Video;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlaytestingReviewer.Editor
{
    public class VideoPreviewTrack : Track
    {
        private IVideoPlayer _videoPlayer;

        private const int pixelHeight = 55;
        private const int pixelWidth = 80;
        private List<VisualElement> previews = new List<VisualElement>();
        public VideoPreviewTrack(VisualElement description, VisualElement information, IProvideTimeRelations timeRelations, IVideoPlayer videoPlayer)
        : base(description, information, timeRelations)
        {
            _videoPlayer = videoPlayer;
        }

        protected override void PreInitialization()
        {
            _trackHeight = 60;
        }
        protected override void OnResize(VisualElement informationZone)
        {
            // Create Previews
            int ammountOfPreviews = (int)informationZone.resolvedStyle.width / pixelWidth;
            for (int i = 0; i < ammountOfPreviews; i++)
            {
                VisualElement previewBox = new VisualElement();
                previewBox.style.width = pixelWidth;
                previewBox.style.height = pixelHeight;
                previewBox.style.backgroundColor = new StyleColor(UnityEngine.Color.black);
                previewBox.style.backgroundColor = new StyleColor(new UnityEngine.Color(
                    UnityEngine.Random.value,
                    UnityEngine.Random.value,
                    UnityEngine.Random.value));

                informationZone.Add(previewBox);
                previews.Add(previewBox);
                // Populate the Preview
            }

            foreach (VisualElement preview in previews)
            {
                float time = _timeRelations.GetTimeFromLeftWorldPosition(preview.resolvedStyle.left);
                Texture2D backgroundTexture = _videoPlayer.GetTextureInTime(time);

                preview.style.backgroundImage = backgroundTexture;
            }

        }
    }
}
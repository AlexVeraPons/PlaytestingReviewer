using UnityEngine;

namespace PlaytestingReviewer
{
    public static class PathManager
    {
        private static readonly string _mainFolder = "Assets/_Project/";

        public static readonly string DefaultImagePath = _mainFolder + "Textures/defaultImage.png";
        public static string FFmpegPath => _mainFolder + "FFmpeg/ffmpeg.exe";
        public static readonly string FrameOutputPath = Application.streamingAssetsPath + "/Frames";
        public static readonly string ReviewOutputPath = _mainFolder + "/ReviewOutput";

        public static readonly string ImageVideoLoadedPath = _mainFolder + "Textures/videoLoaded.png";

        public static readonly string PlaytestReviewerUxmlPath = _mainFolder + "Editor/PlaytestReviewer.uxml";
        public static readonly string PlaytestReviewerUSSPath = _mainFolder + "Editor/PlaytestReviewer.uss";
        public static readonly string VideoPreviewIcon = _mainFolder + "Icons/VideoPreviewIcon.png";
        public static readonly string MetricIcon = Application.streamingAssetsPath + "/MetricIcon";
    }
}
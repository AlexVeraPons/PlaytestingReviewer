using UnityEngine;
using System.IO;

namespace PlaytestingReviewer
{
    public static class PathManager
    {
        public static readonly string DefaultImagePath = "Assets/_Project/Textures/defaultImage.png";
        public static string FFmpegPath => Path.Combine(Application.streamingAssetsPath, "FFmpeg", "ffmpeg.exe");
        public static string VideoOutputPath => Application.streamingAssetsPath + "/Output";
        public static string FrameOutputPath => Application.streamingAssetsPath + "/Frames";
        public static string ReviewOutputPath => Application.streamingAssetsPath + "/Review";

        public static readonly string ImageVideoLoadedPath = "Assets/_Project/Textures/videoLoaded.png";

        public static readonly string PlaytestReviewerUxmlPath = "Assets/_Project/Editor/PlaytestReviewer.uxml";
        public static readonly string PlaytestReviewerUSSPath = "Assets/_Project/Editor/PlaytestReviewer.uss";
        public static readonly string VideoPreviewIcon = "Assets/_Project/Icons/VideoPreviewIcon.png";
        public static readonly string MetricIcon = Application.streamingAssetsPath + "/MetricIcon";
    }
}
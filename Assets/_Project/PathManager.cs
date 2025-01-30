using UnityEngine;
using System.IO;

namespace PlaytestingReviewer
{
    public static class PathManager
    {
        public static readonly string DefaultImagePath = "Assets/_Project/Textures/defaultImage.png";
        public static readonly string DefaultVideoPath = "Assets/_Project/Videos/sample1Video.mp4";
        public static string FFmpegPath => Path.Combine(Application.streamingAssetsPath, "FFmpeg", "ffmpeg.exe");
        public static string VideoOutputPath => Application.streamingAssetsPath + "/Output";
        public static string FrameOutputPath => Application.streamingAssetsPath + "/Frames";

        public static readonly string ImageVideoLoadedPath = "Assets/_Project/Textures/videoLoaded.png";

        public static readonly string PlaytestReviewerUXMLPath = "Assets/_Project/Editor/PlaytestReviewer.uxml";
        public static readonly string PlaytestReviewerUSSPath = "Assets/_Project/Editor/PlaytestReviewer.uss";
        public static readonly string VideoPreviewIcon = "Assets/_Project/Icons/VideoPreviewIcon.png";
        public static readonly string MetricIcon = Application.streamingAssetsPath + "/MetricIcon";
    }
}
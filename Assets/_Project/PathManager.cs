using UnityEngine;
using System.IO;

namespace PlaytestingReviewer
{
    public static class PathManager
    {
        public static string DefaultImagePath => "Assets/_Project/Textures/defaultImage.png";
        public static string DefaultVideoPath => "Assets/_Project/Videos/sample1Video.mp4";
        public static string FFmpegPath =>  Path.Combine(Application.streamingAssetsPath, "FFmpeg", "ffmpeg.exe");
        public static string VideoOutputPath => Application.streamingAssetsPath + "/Output";
        public static string FrameOutputPath =>  Application.streamingAssetsPath + "/Frames";
        public static string PlaytestReviewerUXMLPath => "Assets/_Project/Editor/PlaytestReviewer.uxml";
        public static string PlaytestReviewerUSSPath => "Assets/_Project/Editor/PlaytestReviewer.uss";
        public static string VideoPreviewIcon => "Assets/_Project/Icons/VideoPreviewIcon.png";
    }
}
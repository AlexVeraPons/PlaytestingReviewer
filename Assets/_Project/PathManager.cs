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

    }
}
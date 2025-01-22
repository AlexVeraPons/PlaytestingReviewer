using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PlaytestingReviewer
{
    public class FFmpegUtils : MonoBehaviour
    {
        public Action<string> FrameFromVideoReady;
        private string FFmpegPath => PathManager.FFmpegPath;
        /// <summary>
        /// Initiates a process of extracting a frame from a video depending on its time (in seconds).
        /// </summary>
        /// <param name="videoPath">Path to the video you want to extract from.</param>
        /// <param name="timeInSeconds">Time in seconds into the video from which to extract a frame.</param>
        /// <param name="ID">An ID used for the event callback or filename differentiation.</param>
        public void ExtractFrameFromVideo(string videoPath, float timeInSeconds, string ID)
        {
            StartCoroutine(ExtractFrameFromVideoCoroutine(videoPath, timeInSeconds, ID));
        }

        private IEnumerator ExtractFrameFromVideoCoroutine(string videoPath, float timeInSeconds, string ID)
        {
            // Convert the float timeInSeconds to a TimeSpan for robust formatting (handles hh:mm:ss.mmm)
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeInSeconds);

            // Format the time in HH:MM:SS.milliseconds (FFmpeg accepts fractional seconds as well)
            string timeArg = timeSpan.ToString(@"hh\:mm\:ss\.fff");

            // Choose an output file name (adjust output path to your needs)
            // Example: ExtractedFrame_ID_20250122_153045.png
            string outFileName = $"ExtractedFrame_{ID}.png";
            string outFilePath = Path.Combine(PathManager.FrameOutputPath, outFileName);

            // FFmpeg arguments for seeking to the timestamp (-ss) and extracting 1 frame (-frames:v 1)
            // -y will overwrite any existing file without asking
            string args = $"-y -ss {timeArg} -i \"{videoPath}\" -frames:v 1 \"{outFilePath}\"";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = FFmpegPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using (Process ffmpegProcess = new Process())
            {
                ffmpegProcess.StartInfo = startInfo;
                ffmpegProcess.Start();

                string ffmpegOutput = ffmpegProcess.StandardError.ReadToEnd();
                ffmpegProcess.WaitForExit();

                if (ffmpegProcess.ExitCode == 0)
                {
                    FrameFromVideoReady?.Invoke(outFilePath);
                }
                else
                {
                    Debug.LogWarning($"FFmpeg exited with error code {ffmpegProcess.ExitCode}");
                    Debug.LogWarning(ffmpegOutput);
                }
            }
            yield return null;
        }
    }
}
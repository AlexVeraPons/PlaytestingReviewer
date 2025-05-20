using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace PlaytestingReviewer
{
    /// <summary>
    /// A static utility class for extracting frames from videos using FFmpeg.
    /// </summary>
    public static class FFmpegUtils
    {
        /// <summary>
        /// Static event invoked when a frame is successfully extracted from a video.
        /// The string parameter will be the file path of the extracted frame.
        /// </summary>
        public static event Action<string> FrameFromVideoReady;

        /// <summary>
        /// Private helper method that synchronously extracts a single frame using FFmpeg.
        /// </summary>
        private static bool ExtractFrameInternal(string videoPath, float timeInSeconds, string outputPath)
        {
            string ffmpegPath = PathManager.FFmpegPath;
            string timeArg = TimeSpan.FromSeconds(timeInSeconds).ToString(@"hh\:mm\:ss\.fff");

            string args = $"-y -ss {timeArg} -i \"{videoPath}\" -frames:v 1 \"{outputPath}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                // Read any potential error or info from FFmpeg
                string ffmpegOutput = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Debug.LogWarning($"FFmpeg exited with code {process.ExitCode}: {ffmpegOutput}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Asynchronously extracts a single frame from a video at a specific time.
        /// If successful, raises <see cref="FrameFromVideoReady"/> with the final file path.
        /// Returns the file path, or null if extraction failed.
        /// </summary>
        /// <param name="videoPath">The full path to the video file.</param>
        /// <param name="timeInSeconds">Time (in seconds) from which to extract the frame.</param>
        /// <param name="id">String identifier to help name the extracted frame.</param>
        /// <returns>The file path of the extracted frame if successful; otherwise null.</returns>
        private static async Task<string> ExtractFrameAsync(string videoPath, float timeInSeconds, string id)
        {
            string outFileName = $"ExtractedFrame_{id}.png";
            string outFilePath = Path.Combine(PathManager.FrameOutputPath, outFileName);
            
            if (!Directory.Exists(PathManager.FrameOutputPath))
                Directory.CreateDirectory(PathManager.FrameOutputPath);

            bool success = await Task.Run(() =>
                ExtractFrameInternal(videoPath, timeInSeconds, outFilePath)
            );

            if (!success) return null;
            
            FrameFromVideoReady?.Invoke(outFilePath);
            return outFilePath;

        }

        /// <summary>
        /// Asynchronously extracts multiple frames from a video at the specified times, 
        /// returning a list of file paths to the successfully extracted frames.
        /// </summary>
        /// <param name="videoPath">The full path to the video file.</param>
        /// <param name="times">A list of times (in seconds) at which to extract frames.</param>
        /// <param name="idPrefix">Prefix for naming the output frames (e.g. "Batch").</param>
        /// <returns>List of output file paths for all successfully extracted frames.</returns>
        public static async Task<List<string>> ExtractBatchFramesAsync(
            string videoPath,
            List<float> times,
            string idPrefix = "Batch")
        {
            var extractedPaths = new List<string>();
            var tasks = new List<Task<string>>();

            // Create and start one Task per frame
            for (int i = 0; i < times.Count; i++)
            {
                float time = times[i];
                string frameId = $"{idPrefix}_{i}";
                tasks.Add(ExtractFrameAsync(videoPath, time, frameId));
            }

            string[] results = await Task.WhenAll(tasks);

            // Filter out any nulls
            foreach (string framePath in results)
            {
                if (!string.IsNullOrEmpty(framePath))
                    extractedPaths.Add(framePath);
            }

            return extractedPaths;
        }

        /// <summary>
        /// A quick, blocking call to extract a single frame from the video at a specific time.
        /// This call is synchronous and will block until FFmpeg finishes.
        /// </summary>
        /// <param name="videoPath">Video file path.</param>
        /// <param name="timeInSeconds">Time in seconds from which to extract the frame.</param>
        /// <param name="outputPath">Where to save the PNG frame.</param>
        public static void ExtractFrame(string videoPath, float timeInSeconds, string outputPath)
        {
            ExtractFrameInternal(videoPath, timeInSeconds, outputPath);
        }

        /// <summary>
        /// Deletes all extracted PNG frames (and any associated .meta files) from the configured output folder.
        /// </summary>
        public static void DeleteAllFrames()
        {
            string frameOutputPath = PathManager.FrameOutputPath;

            if (!Directory.Exists(frameOutputPath))
            {
                Debug.LogWarning($"Frame output directory does not exist: {frameOutputPath}");
                return;
            }

            try
            {
                string[] frameFiles = Directory.GetFiles(frameOutputPath, "*.png");

                foreach (string file in frameFiles)
                {
                    File.Delete(file);
                    Debug.Log($"Deleted frame: {file}");

                    string metaFile = file + ".meta";
                    if (File.Exists(metaFile))
                    {
                        File.Delete(metaFile);
                        Debug.Log($"Deleted .meta file: {metaFile}");
                    }
                }

                Debug.Log("All frames and .meta files deleted successfully.");
            }
            catch (Exception ex)
            {
                
                
                
                Debug.LogError($"Error deleting frames or .meta files: {ex.Message}");
            }
        }
    }
}
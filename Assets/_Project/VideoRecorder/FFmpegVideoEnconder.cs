using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace PlaytestingReviewer.Video
{
    public class FFmpegVideoEncoder : IDisposable
    {
        private Process _ffmpegProcess;
        private Stream _stdin;

        private readonly int _width;
        private readonly int _height;
        private readonly int _framerate;
        private readonly string _outputPath;
        private readonly string _ffmpegPath;

        private Thread _encodeThread;
        private readonly ConcurrentQueue<byte[]> _frameQueue = new ConcurrentQueue<byte[]>();
        private volatile bool _stopRequested = false;

        public FFmpegVideoEncoder(int width, int height, int framerate, string outputPath, string ffmpegPath)
        {
            _width = width;
            _height = height;
            _framerate = framerate;
            _outputPath = outputPath;
            _ffmpegPath = ffmpegPath;

            StartFFmpeg();
            StartBackgroundEncoder();
        }

        private void StartFFmpeg()
        {
            string args = $"-y -f rawvideo -pix_fmt rgb24 -s {_width}x{_height} -r {_framerate} -i - " +
                          $"-vf vflip -c:v libx264 -preset ultrafast -pix_fmt yuv420p \"{_outputPath}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            _ffmpegProcess = new Process { StartInfo = startInfo };

            _ffmpegProcess.Start();
            _ffmpegProcess.BeginErrorReadLine();
            _ffmpegProcess.BeginOutputReadLine();

            _stdin = _ffmpegProcess.StandardInput.BaseStream;
        }

        private void StartBackgroundEncoder()
        {
            _encodeThread = new Thread(EncodeLoop);
            _encodeThread.IsBackground = true;
            _encodeThread.Start();
        }

        private void EncodeLoop()
        {
            while (!_stopRequested)
            {
                if (_frameQueue.TryDequeue(out byte[] frameData))
                {
                    _stdin.Write(frameData, 0, frameData.Length);
                    _stdin.Flush();
                }
                else
                {
                    // If there’s no frame to write, just yield CPU briefly
                    Thread.Sleep(1);
                }
            }

            _stdin?.Close();

            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                _ffmpegProcess.WaitForExit();
            }
        }

        public void EnqueueFrame(byte[] rawFrameData)
        {
            _frameQueue.Enqueue(rawFrameData);
        }

        public void Dispose()
        {
            _stopRequested = true;
            _encodeThread?.Join();

            _ffmpegProcess?.Dispose();
        }
    }
}
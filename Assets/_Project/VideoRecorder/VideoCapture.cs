using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace PlaytestingReviewer.Video
{
    public class VideoCapture : MonoBehaviour
    {
        [Header("Camera & Capture Settings")] public Camera captureCamera;
        private Camera mainCamera;

        public int width = 1280;
        public int height = 720;
        public int targetFramerate = 30;

        // Adjust this to control how many frames to buffer before writing.
        // Lower = less memory usage but more frequent disk writes.
        // Higher = potentially better performance but uses more memory.
        [Tooltip("Adjust this to control how many frames to buffer before writing.")] [SerializeField]
        private int writeBufferThreshold = 30;

        private string _ffmpegPath = PathManager.FFmpegPath;
        private static string FfmpegExePath => Path.Combine(Application.streamingAssetsPath, "FFmpeg", "ffmpeg.exe");

        private RenderTexture _renderTexture;
        private Texture2D _readTexture;

        private bool _isCapturing = false;
        public bool IsCapturing => _isCapturing;
        private float _captureDeltaTime;
        private float _timeSinceLastFrame;

        private string _folderPath;
        private int _frameCount;

        [HideInInspector] public string outputPath;
        [HideInInspector] public string outputFileName;


        private float _currentVideoTime = 0f;
        public float CurrentVideoTime => _currentVideoTime;

        [Header("Capture Conditions")] [SerializeField]
        private bool captureOnStart = false;

        /// <summary>
        /// Simple container for storing the frame data in memory.
        /// </summary>
        private class FrameData
        {
            public byte[] data;
            public string fileName;
        }

        // Buffer to hold frames before writing to disk
        private List<FrameData> frameBuffer = new List<FrameData>();

        void Start()
        {
            _ffmpegPath = FfmpegExePath;
            mainCamera = Camera.main;

            if (captureCamera == null)
            {
                GameObject captureCamObj = new GameObject("HiddenCaptureCamera");
                captureCamera = captureCamObj.AddComponent<Camera>();

                if (mainCamera != null)
                {
                    captureCamera.CopyFrom(mainCamera);
                }

                captureCamera.enabled = false;
            }

            if (captureOnStart)
            {
                StartCapture();
            }
        }

        void OnApplicationQuit()
        {
            if (_isCapturing)
            {
                StopCapture();
            }
        }

        public void StartCapture()
        {
            if (_isCapturing)
            {
                Debug.LogWarning("Already capturing!");
                return;
            }

            _folderPath = Path.Combine(Application.dataPath,
                "RecordedFrames_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(_folderPath);
            _frameCount = 0;

            _renderTexture = new RenderTexture(width, height, 24);
            _renderTexture.Create();

            _readTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

            _captureDeltaTime = 1f / targetFramerate;
            _timeSinceLastFrame = 0f;

            _isCapturing = true;
            Debug.Log("Capture started. Saving frames to: " + _folderPath);
        }

        public void StopCapture()
        {
            if (!_isCapturing)
            {
                Debug.LogWarning("Not currently capturing!");
                return;
            }

            _isCapturing = false;

            // Ensure any buffered frames get written
            FlushBufferToDisk();

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
                _renderTexture = null;
            }

            if (_readTexture != null)
            {
                Destroy(_readTexture);
                _readTexture = null;
            }

            StartCoroutine(EncodeToMP4());
        }

        void LateUpdate()
        {
            if (_isCapturing)
            {
                SyncCaptureCamera();

                _timeSinceLastFrame += Time.deltaTime;

                if (_timeSinceLastFrame >= _captureDeltaTime)
                {
                    _timeSinceLastFrame -= _captureDeltaTime;
                    CaptureFrame();
                }
            }
        }

        /// <summary>
        /// Synchronizes the hidden capture camera with the main camera.
        /// </summary>
        private void SyncCaptureCamera()
        {
            if (mainCamera == null || captureCamera == null)
                return;

            captureCamera.transform.position = mainCamera.transform.position;
            captureCamera.transform.rotation = mainCamera.transform.rotation;
            captureCamera.fieldOfView = mainCamera.fieldOfView;
            captureCamera.orthographic = mainCamera.orthographic;
            captureCamera.orthographicSize = mainCamera.orthographicSize;
            captureCamera.nearClipPlane = mainCamera.nearClipPlane;
            captureCamera.farClipPlane = mainCamera.farClipPlane;
            captureCamera.clearFlags = mainCamera.clearFlags;
            captureCamera.backgroundColor = mainCamera.backgroundColor;
            captureCamera.cullingMask = mainCamera.cullingMask;
        }

        private void CaptureFrame()
        {
            RenderTexture prevTarget = captureCamera.targetTexture;
            RenderTexture prevActive = RenderTexture.active;

            captureCamera.targetTexture = _renderTexture;
            RenderTexture.active = _renderTexture;

            captureCamera.Render();

            _readTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            _readTexture.Apply();

            captureCamera.targetTexture = prevTarget;
            RenderTexture.active = prevActive;

            byte[] jpgData = _readTexture.EncodeToJPG(90);
            string fileName = $"frame_{_frameCount:D04}.jpg";

            // Store in buffer instead of writing immediately
            frameBuffer.Add(new FrameData
            {
                data = jpgData,
                fileName = fileName
            });

            _frameCount++;
            _currentVideoTime += _captureDeltaTime;

            // If we've reached the threshold, flush to disk
            if (frameBuffer.Count >= writeBufferThreshold)
            {
                FlushBufferToDisk();
            }
        }

        /// <summary>
        /// Writes all buffered frames to disk and clears the buffer.
        /// </summary>
        private void FlushBufferToDisk()
        {
            foreach (var frame in frameBuffer)
            {
                string filePath = Path.Combine(_folderPath, frame.fileName);
                File.WriteAllBytes(filePath, frame.data);
            }

            frameBuffer.Clear();
        }

        private IEnumerator EncodeToMP4()
        {
            Debug.Log("Starting FFmpeg encode...");

            if (_frameCount < 1)
            {
                Debug.LogWarning("No frames captured. Skipping FFmpeg.");
                yield break;
            }

            string outFileName = !string.IsNullOrEmpty(outputFileName)
                ? outputFileName
                : "RecordedVideo_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4";

            string outFilePath = !string.IsNullOrEmpty(outputPath)
                ? Path.Combine(outputPath, outFileName)
                : Path.Combine(PathManager.VideoOutputPath, outFileName);

            string args = $"-y -framerate {targetFramerate} -i \"{Path.Combine(_folderPath, "frame_%04d.jpg")}\" " +
                          $"-c:v libx264 -pix_fmt yuv420p \"{outFilePath}\"";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
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
                    Debug.Log($"FFmpeg finished encoding: {outFilePath}");
                }
                else
                {
                    Debug.LogWarning($"FFmpeg exited with error code {ffmpegProcess.ExitCode}");
                    Debug.LogWarning(ffmpegOutput);
                }
            }

            try
            {
                Directory.Delete(_folderPath, true);
                Debug.Log($"Deleted temporary frames folder: {_folderPath}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to delete frames folder: {e.Message}");
            }

            AssetDatabase.Refresh();
            yield return null;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(VideoCapture))]
    public class VideoCaptureEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            VideoCapture vc = (VideoCapture)target;

            GUILayout.Space(10);
            GUILayout.BeginVertical("box");
            GUILayout.Label("Capture Controls", UnityEditor.EditorStyles.boldLabel);

            if (vc.IsCapturing)
            {
                GUI.backgroundColor = Color.red;
                GUILayout.Label("Status: Capturing frames...", UnityEditor.EditorStyles.helpBox);
                GUILayout.Space(10);
            }
            else
            {
                GUI.backgroundColor = Color.green;
                GUILayout.Label("Status: Ready to capture.", UnityEditor.EditorStyles.helpBox);
                GUILayout.Space(10);
            }

            GUI.backgroundColor = Color.white;

            if (vc.IsCapturing)
            {
                if (GUILayout.Button("Stop Capture"))
                {
                    vc.StopCapture();
                }
            }
            else
            {
                if (GUILayout.Button("Start Capture"))
                {
                    vc.StartCapture();
                }
            }

            GUILayout.Space(10);
            GUILayout.EndVertical();
        }
    }
#endif
}
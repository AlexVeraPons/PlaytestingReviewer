using System;
using System.IO;
using UnityEditor;
using UnityEngine;
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

        private string _ffmpegPath = PathManager.FFmpegPath;

        private RenderTexture _renderTexture;
        private Texture2D _readTexture;

        private bool _isCapturing;
        public bool IsCapturing => _isCapturing;

        private float _captureDeltaTime;
        private float _timeSinceLastFrame;

        [HideInInspector] public string outputPath;
        [HideInInspector] public string outputFileName;

        private float _currentVideoTime = 0f;
        public float CurrentVideoTime => _currentVideoTime;

        public bool isPaused;

        [Header("Capture Conditions")] [SerializeField]
        private bool captureOnStart = false;

        private FFmpegVideoEncoder _encoder;

        void Start()
        {
            GetCamera();


            if (captureOnStart)
                StartCapture();
        }

        void OnApplicationQuit()
        {
            if (_isCapturing)
                StopCapture();
        }

        public void StartCapture()
        {
            if (_isCapturing)
            {
                Debug.LogWarning("Already capturing!");
                return;
            }

            _renderTexture = new RenderTexture(width, height, 24);
            _readTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
            _renderTexture.Create();

            _captureDeltaTime = 1f / targetFramerate;
            _timeSinceLastFrame = 0f;
            _currentVideoTime = 0f;
            
            string outFileName = "Review" + ".mp4";
            string outFilePath = Path.Combine(outputPath, outFileName);

            _encoder = new FFmpegVideoEncoder(width, height, targetFramerate, outFilePath, _ffmpegPath);

            _isCapturing = true;
            Debug.Log($"Capture started. Writing to: {outFilePath}");
        }

        public void StopCapture()
        {
            if (!_isCapturing)
            {
                Debug.LogWarning("Not currently capturing!");
                return;
            }

            _isCapturing = false;

            // Release Unity textures
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
            }

            if (_readTexture != null)
                Destroy(_readTexture);

            try
            {
                _encoder?.Dispose();
                _encoder = null;
            }
            catch (Exception e)
            {
                Debug.LogError("Error finalizing FFmpeg encoder: " + e.Message);
            }
        }

        void LateUpdate()
        {
            if (_isCapturing && !isPaused)
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

        private void SyncCaptureCamera()
        {
            if (mainCamera == null || captureCamera == null) return;

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
            // Render from capture camera
            if (captureCamera == null)
            {
                GetCamera();
            }

            RenderTexture prevTarget = captureCamera.targetTexture;
            RenderTexture prevActive = RenderTexture.active;

            captureCamera.targetTexture = _renderTexture;
            RenderTexture.active = _renderTexture;
            captureCamera.Render();

            // Read back the frame into _readTexture
            _readTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            _readTexture.Apply();

            // Restore previous render targets
            captureCamera.targetTexture = prevTarget;
            RenderTexture.active = prevActive;

            // Enqueue frame for FFmpeg encoding (RGB24 data) 
            byte[] rawData = _readTexture.GetRawTextureData();
            _encoder?.EnqueueFrame(rawData);

            _currentVideoTime += _captureDeltaTime;
        }

        void OnApplicationPause(bool isPaused)
        {
            this.isPaused = isPaused;
        }

        private void OnLevelWasLoaded(int level)
        {
            GetCamera();
        }

        private void GetCamera()
        {
            mainCamera = Camera.main;

            if (captureCamera == null)
            {
                GameObject captureCamObj = new GameObject("HiddenCaptureCamera");
                captureCamera = captureCamObj.AddComponent<Camera>();

                if (mainCamera != null)
                    captureCamera.CopyFrom(mainCamera);

                captureCamera.enabled = false;

                if (captureCamera == null)
                {
                    Debug.LogError("VIDEO CAPTURE>>> NO CAMERA COULD BE FOUND");
                }
            }
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(VideoCapture))]
    public class VideoCaptureEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            VideoCapture vc = (VideoCapture)target;

            GUILayout.Space(10);
            GUILayout.BeginVertical("box");
            GUILayout.Label("Capture Controls", EditorStyles.boldLabel);

            if (vc.IsCapturing)
            {
                GUI.backgroundColor = Color.red;
                GUILayout.Label("Status: Capturing...", EditorStyles.helpBox);
                if (GUILayout.Button("Stop Capture")) vc.StopCapture();
            }
            else
            {
                GUI.backgroundColor = Color.green;
                GUILayout.Label("Status: Ready.", EditorStyles.helpBox);
                if (GUILayout.Button("Start Capture")) vc.StartCapture();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.EndVertical();
        }
    }
#endif
}
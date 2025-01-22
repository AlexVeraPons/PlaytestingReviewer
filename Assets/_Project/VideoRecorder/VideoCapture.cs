using UnityEngine;
using System.Collections;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace PlaytestingReviewer.Video
{
    public class VideoCapture : MonoBehaviour
    {
        [Header("Camera & Capture Settings")]
        public Camera captureCamera;  
        private Camera mainCamera;    

        public int width = 1280;
        public int height = 720;
        public int targetFramerate = 30;

        private string ffmpegPath = PathManager.FFmpegPath;
        private string ffmpegExePath => Path.Combine(Application.streamingAssetsPath, "FFmpeg", "ffmpeg.exe");

        private RenderTexture renderTexture;
        private Texture2D readTexture;

        private bool isCapturing = false;
        public bool IsCapturing => isCapturing;
        private float captureDeltaTime;
        private float timeSinceLastFrame;

        private string folderPath;
        private int frameCount;

        [Header("Capture Conditions")]
        [SerializeField] private bool captureOnStart = false;

        void Start()
        {
            ffmpegPath = ffmpegExePath;
            mainCamera = Camera.main;

            // If no capture camera is assigned, create a hidden one
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
            if (isCapturing)
            {
                StopCapture();
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isCapturing)
                {
                    StopCapture();
                }
                else
                {
                    StartCapture();
                }
            }
        }

        public void StartCapture()
        {
            if (isCapturing)
            {
                Debug.LogWarning("Already capturing!");
                return;
            }

            folderPath = Path.Combine(Application.dataPath, "RecordedFrames_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(folderPath);
            frameCount = 0;

            renderTexture = new RenderTexture(width, height, 24);
            renderTexture.Create();

            readTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

            captureDeltaTime = 1f / targetFramerate;
            timeSinceLastFrame = 0f;

            isCapturing = true;
            Debug.Log("Capture started. Saving frames to: " + folderPath);
        }

        public void StopCapture()
        {
            if (!isCapturing)
            {
                Debug.LogWarning("Not currently capturing!");
                return;
            }

            isCapturing = false;

            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
                renderTexture = null;
            }

            if (readTexture != null)
            {
                Destroy(readTexture);
                readTexture = null;
            }

            StartCoroutine(EncodeToMP4());
        }

        void LateUpdate()
        {
            if (isCapturing)
            {
                SyncCaptureCamera();  

                timeSinceLastFrame += Time.deltaTime;

                if (timeSinceLastFrame >= captureDeltaTime)
                {
                    timeSinceLastFrame -= captureDeltaTime;
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

            captureCamera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;

            captureCamera.Render();

            readTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            readTexture.Apply();

            captureCamera.targetTexture = prevTarget;
            RenderTexture.active = prevActive;

            byte[] pngData = readTexture.EncodeToPNG();
            string fileName = $"frame_{frameCount:D04}.png";
            string filePath = Path.Combine(folderPath, fileName);
            File.WriteAllBytes(filePath, pngData);

            frameCount++;
        }

        private IEnumerator EncodeToMP4()
        {
            Debug.Log("Starting FFmpeg encode...");

            if (frameCount < 1)
            {
                Debug.LogWarning("No frames captured. Skipping FFmpeg.");
                yield break;
            }

            string outFileName = "RecordedVideo_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4";
            string outFilePath = Path.Combine(PathManager.VideoOutputPath, outFileName);

            string args = $"-y -framerate {targetFramerate} -i \"{Path.Combine(folderPath, "frame_%04d.png")}\" " +
                          $"-c:v libx264 -pix_fmt yuv420p \"{outFilePath}\"";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
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
                Directory.Delete(folderPath, true);
                Debug.Log($"Deleted temporary frames folder: {folderPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to delete frames folder: {e.Message}");
            }

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

            //addspace
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

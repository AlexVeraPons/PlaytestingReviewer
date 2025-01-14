using UnityEngine;
using System.Collections;
using System.IO;
using System.Diagnostics;        // For Process
using Debug = UnityEngine.Debug; // Avoid collision with System.Diagnostics.Debug

public class VideoCapture : MonoBehaviour
{
    [Header("Camera & Capture Settings")]
    public Camera captureCamera;           // Assign your Camera in Inspector
    public int width = 1280;
    public int height = 720;
    public int targetFramerate = 30;

    [Header("FFmpeg Settings")]
    public string ffmpegPath = "StreamingAset/FFmpeg/ffmpeg.exe";

    string ffmpegExePath => System.IO.Path.Combine(
    Application.streamingAssetsPath,
    "FFmpeg",
    "ffmpeg.exe"
);

    private RenderTexture renderTexture;
    private Texture2D readTexture;

    private bool isCapturing = false;
    private float captureDeltaTime;
    private float timeSinceLastFrame;

    private string folderPath;     // Where we store the PNGs
    private int frameCount;

    // Example usage:
    // 1. Press play, then call StartCapture().
    // 2. After some seconds, call StopCapture().

    void Start()
    {
        ffmpegPath = ffmpegExePath;
        // Optionally set up camera here if not assigned
        if (captureCamera == null)
        {
            captureCamera = Camera.main;
        }

    }

    void  OnApplicationQuit()
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
                return;
            }
            StartCapture();
        }
    }

    public void StartCapture()
    {
        if (isCapturing)
        {
            Debug.LogWarning("Already capturing!");
            return;
        }

        // Create a folder to store our PNG frames
        folderPath = Path.Combine(Application.dataPath, "RecordedFrames_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(folderPath);
        frameCount = 0;

        // Create a RenderTexture
        renderTexture = new RenderTexture(width, height, 24);
        renderTexture.Create();

        // Create a Texture2D to read from the RenderTexture
        readTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Assign the RenderTexture to the capture camera
        if (captureCamera != null)
        {
            captureCamera.targetTexture = renderTexture;
        }

        // Set up timing for capturing frames at targetFramerate
        captureDeltaTime = 1f / targetFramerate;
        timeSinceLastFrame = 0f;

        // Start capturing
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

        // Release camera target
        if (captureCamera != null)
        {
            captureCamera.targetTexture = null;
        }

        // Release resources
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

        // Once capture is stopped, call FFmpeg to compile PNGs into an MP4
        StartCoroutine(EncodeToMP4());
    }

    void LateUpdate()
    {
        if (!isCapturing) return;

        // Accumulate time
        timeSinceLastFrame += Time.deltaTime;
        // If enough time for the next frame at targetFramerate
        if (timeSinceLastFrame >= captureDeltaTime)
        {
            timeSinceLastFrame -= captureDeltaTime;
            CaptureFrame();
        }
    }

    private void CaptureFrame()
    {
        // Render the camera to the RenderTexture
        captureCamera.Render();

        // Copy RenderTexture contents into readTexture
        RenderTexture prevActive = RenderTexture.active;
        RenderTexture.active = renderTexture;
        readTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        readTexture.Apply();
        RenderTexture.active = prevActive;

        // Encode the texture to PNG
        byte[] pngData = readTexture.EncodeToPNG();
        // Save to disk
        string fileName = $"frame_{frameCount:D04}.png";
        string filePath = Path.Combine(folderPath, fileName);
        File.WriteAllBytes(filePath, pngData);

        frameCount++;
    }

    private IEnumerator EncodeToMP4()
    {
        Debug.Log("Starting FFmpeg encode...");

        // Make sure there’s at least 1 frame
        if (frameCount < 1)
        {
            Debug.LogWarning("No frames captured. Skipping FFmpeg.");
            yield break;
        }

        // Construct output file path
        // E.g.: ".../RecordedVideo_20230725_153000.mp4"
        string outFileName = "RecordedVideo_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4";
        string outFilePath = Path.Combine(Application.dataPath, outFileName);

        // Example FFmpeg command:
        //   ffmpeg -framerate 30 -i frame_%04d.png -c:v libx264 -pix_fmt yuv420p output.mp4
        // We specify:
        //   -framerate   : must match our targetFramerate
        //   -i frame_%04d.png : input sequence name pattern
        //   -c:v libx264 : use H.264
        //   -pix_fmt yuv420p   : widely compatible pixel format

        // On Windows, you might need quotes around paths with spaces
        string args = $"-y -framerate {targetFramerate} -i \"{Path.Combine(folderPath, "frame_%04d.png")}\" " +
                      $"-c:v libx264 -pix_fmt yuv420p \"{outFilePath}\"";

        // Run as a background process
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

            // Read all output but do not block
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

        // (Optional) Delete the PNG frames to save space
        // Comment this out if you want to keep the individual frames
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

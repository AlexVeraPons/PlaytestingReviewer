using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class TimelineEditorWindow : EditorWindow
{
    private VisualElement trackContainer;
    private Button addTrackButton, playButton, pauseButton, stopButton;
    private FloatField playheadField;
    private VideoPlayer videoPlayer;
    private Image videoPreviewImage;
    private RenderTexture renderTexture;


    [MenuItem("Window/Custom Timeline Tool (Video Preview)")]
    public static void ShowWindow()
    {
        var window = GetWindow<TimelineEditorWindow>();
        window.titleContent = new GUIContent("Timeline with Video");
        window.minSize = new Vector2(800, 600);
    }

    private void OnEnable()
    {
        // Load UXML and USS
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TimelineEditor/TimelineEditor.uxml");
        VisualElement root = visualTree.Instantiate();
        rootVisualElement.Add(root);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/TimelineEditor/TimelineEditor.uss");
        rootVisualElement.styleSheets.Add(styleSheet);

        // Get UI References
        addTrackButton = root.Q<Button>("add-track-button");
        playButton = root.Q<Button>("play-button");
        pauseButton = root.Q<Button>("pause-button");
        stopButton = root.Q<Button>("stop-button");
        playheadField = root.Q<FloatField>("playhead-field");
        videoPreviewImage = root.Q<Image>("video-preview");
        trackContainer = root.Q<ScrollView>("track-container");

        // Button Events
        addTrackButton.clicked += AddTrack;
        playButton.clicked += PlayVideo;
        pauseButton.clicked += PauseVideo;
        stopButton.clicked += StopVideo;

        // Initialize Video Player
        InitializeVideoPlayer();
    }

    public void OnGUI()
    {
        if(videoPlayer.isPlaying == false)
        {
            return;
        }

        if (videoPlayer != null && videoPlayer.isPrepared && renderTexture != null && videoPreviewImage != null)
        {
            // Directly assign the RenderTexture to the Image component
            videoPreviewImage.image = renderTexture;
        }
        else
        {
            if (videoPlayer == null)
                Debug.LogError("VideoPlayer is null.");
            if (videoPreviewImage == null)
                Debug.LogError("VideoPreviewImage is null.");
            if (renderTexture == null)
                Debug.LogError("RenderTexture is null.");
        }
    }

    private void InitializeVideoPlayer()
    {
        GameObject videoObject = new GameObject("VideoPlayer");
        videoObject.hideFlags = HideFlags.HideAndDontSave;

        videoPlayer = videoObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

        // Create RenderTexture for the video
        renderTexture = new RenderTexture(512, 512, 0);
        videoPlayer.targetTexture = renderTexture;

        // Load video file (replace with your path)
        videoPlayer.url = "Assets/TimelineEditor/TestVideo.mp4";
    }

    private void PlayVideo()
    {
        if (!videoPlayer.isPrepared)
        {
            videoPlayer.Prepare();
        }

        videoPlayer.Play();
        EditorApplication.update += SyncTimelineWithVideo;
    }

    private void PauseVideo()
    {
        videoPlayer.Pause();
        EditorApplication.update -= SyncTimelineWithVideo;
    }

    private void StopVideo()
    {
        videoPlayer.Stop();
        EditorApplication.update -= SyncTimelineWithVideo;
        playheadField.value = 0;
    }

    private void SyncTimelineWithVideo()
    {
        if (videoPlayer.isPlaying)
        {
            playheadField.value = (float)videoPlayer.time;
        }
    }

    private void AddTrack()
    {
        VisualElement track = new VisualElement();
        track.AddToClassList("track");

        Label trackLabel = new Label($"Track {trackContainer.childCount + 1}");
        trackLabel.AddToClassList("track-label");
        track.Add(trackLabel);

        VisualElement markerArea = new VisualElement();
        markerArea.AddToClassList("marker-area");
        markerArea.RegisterCallback<MouseDownEvent>(evt => AddMarker(evt, markerArea));
        track.Add(markerArea);

        trackContainer.Add(track);
    }

    private void AddMarker(MouseDownEvent evt, VisualElement markerArea)
    {
        VisualElement marker = new VisualElement();
        marker.AddToClassList("marker");
        marker.style.left = evt.localMousePosition.x;
        markerArea.Add(marker);
    }
}

using PlaytestingReviewer.Editors;
using UnityEngine;
using UnityEngine.UIElements;
using PlaytestingReviewer.Tracks; // your namespace

[RequireComponent(typeof(UIDocument))]
public class PlaytestReviewerRuntime : MonoBehaviour
{
    [SerializeField] private Review _review;

    private VideoController _videoController;
    private TimeIndicatorController _timeIndicatorController;
    private TimeNeedle _timeNeedle;
    private ZoomUpdater _zoomUpdater;
    private TracksButton _tracksButton;

    void Awake()
    {
        // 1) Grab the UXML root
        var root = GetComponent<UIDocument>().rootVisualElement;

        // 2) Wire up your controllers just like in the EditorWindow
        _zoomUpdater = new ZoomUpdater();
        _zoomUpdater.SubscribeToZoom(root);

        _videoController = new VideoController(root);
        _timeIndicatorController = new TimeIndicatorController(root, _videoController.VideoPlayer, _zoomUpdater);
        _timeNeedle = new TimeNeedle(root, _timeIndicatorController, _videoController.VideoPlayer);

        _videoController.OnNewVideoLengthAvailable += _timeIndicatorController.ReloadIndicators;
        _videoController.OnPlay += _timeNeedle.Initialize;

        InitializeTracks(root);
    }

    private void InitializeTracks(VisualElement root)
    {
        var desc = root.Q<ScrollView>("TrackDescriptions");
        var info = root.Q<ScrollView>("TrackInformation");
        var timeView = root.Q<ScrollView>("TimeScroll");
        var timeViewVE = root.Q<VisualElement>("TimeView");

        var icon = Resources.Load<Texture2D>("VideoPreviewIcon"); // or load however you package it
        var previewTrack = new VideoPreviewTrack(desc, info, _timeIndicatorController, _videoController.VideoPlayer);
        previewTrack.AdaptToWidth(timeViewVE);
        previewTrack.SetTrackIcon(icon);

        // Sync scrollbars:
        info.horizontalScroller.valueChanged += v => timeView.horizontalScroller.value = v;
        timeView.horizontalScroller.valueChanged += v => info.horizontalScroller.value = v;
        desc.verticalScroller.valueChanged += v => info.verticalScroller.value = v;
        info.verticalScroller.valueChanged += v => desc.verticalScroller.value = v;
    }

    /// <summary>
    /// Call this at runtime when you have a Review object to load.
    /// </summary>
    [ContextMenu("LoadReview")]
    public void LoadReview()
    {
        Review review = _review;
        _tracksButton.SetTrackCollection(review.GetTrackCollecion());
        _videoController.SetVideo(review.videoPath);

        var nameLabel = GetComponent<UIDocument>().rootVisualElement.Q<Label>("RecordingName");
        nameLabel.text = review.name;
    }

    public void LoadReview(string trackCollection, string videoPath)
    {
        Review review = _review;
        TrackCollection collection = new TrackCollection(trackCollection);
        _videoController.SetVideo(videoPath);

        var nameLabel = GetComponent<UIDocument>().rootVisualElement.Q<Label>("RecordingName");
        nameLabel.text = "review";
    }
}
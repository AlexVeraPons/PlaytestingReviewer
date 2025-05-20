using PlaytestingReviewer.Editors;
using UnityEngine;
using UnityEngine.UIElements;
using PlaytestingReviewer.Tracks;

[RequireComponent(typeof(UIDocument))]
public class PlaytestReviewerRuntime : MonoBehaviour
{
    [SerializeField] private Review _review;

    private VideoController _videoController;
    private TimeIndicatorController M;
    private TimeNeedle _timeNeedle;
    private ZoomUpdater _zoomUpdater;
    private TimeIndicatorController _timeIndicatorController;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

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

        var icon = Resources.Load<Texture2D>("VideoPreviewIcon");
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
        _videoController.SetVideo(review.videoPath);

        var nameLabel = GetComponent<UIDocument>().rootVisualElement.Q<Label>("RecordingName");
        nameLabel.text = review.name;
    }

    public void LoadReview(string trackCollectionPath, string videoPath)
    {
        // 1. Clear existing visual tracks (optional safety)
        var root = GetComponent<UIDocument>().rootVisualElement;
        var desc = root.Q<ScrollView>("TrackDescriptions");
        var info = root.Q<ScrollView>("TrackInformation");
        var timeView = root.Q<ScrollView>("TimeScroll");
        var timeViewVE = root.Q<VisualElement>("TimeView");

        desc.Clear();
        info.Clear();
        timeViewVE.Clear();

        // 2. Load video
        _videoController.SetVideo(videoPath);

        // 3. Load tracks from JSON
        TrackCollection collection = new TrackCollection(trackCollectionPath);
        _review = new Review
        {
            videoPath = videoPath,
            tracksPath = trackCollectionPath,
            name = System.IO.Path.GetFileNameWithoutExtension(videoPath),
            trackCollection = collection
        };

        var icon = Resources.Load<Texture2D>("VideoPreviewIcon"); // assumes icon is in Resources
        var previewTrack = new VideoPreviewTrack(
            desc,
            info,
            _timeIndicatorController,
            _videoController.VideoPlayer
        );
        previewTrack.AdaptToWidth(timeViewVE);
        previewTrack.SetTrackIcon(icon);
        // 4. Create tracks from factory
        foreach (var track in collection.tracks)
        {
            var trackUI = UITrackFactory.CreateUITrack(
                track,
                desc,
                info,
                _timeIndicatorController,
                timeViewVE
            );
            trackUI.AdaptToWidth(timeViewVE);
        }

        info.horizontalScroller.valueChanged += v => timeView.horizontalScroller.value = v;
        timeView.horizontalScroller.valueChanged += v => info.horizontalScroller.value = v;
        desc.verticalScroller.valueChanged += v => info.verticalScroller.value = v;
        info.verticalScroller.valueChanged += v => desc.verticalScroller.value = v;

    }
}
using PlaytestingReviewer.Tracks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlaytestingReviewer.Editors
{
    /// <summary>
    /// provides a custom Unity Editor Window for reviewing playtest sessions.
    /// It integrates video playback, timeline control, and track management to facilitate analysis.
    /// This tool allows users to load a recorded playtest session, inspect different event tracks
    /// and interact with a time-based representation of the session.
    /// </summary>
    public class PlaytestReviewerEditor : EditorWindow
    {
        private readonly string _ussPath = PathManager.PlaytestReviewerUSSPath;
        private readonly string _iconPath = PathManager.VideoPreviewIcon;

        public VisualTreeAsset visualTree;
        public Texture2D scriptableObjectIcon;

        private VideoController _videoController;
        private TimeIndicatorController _timeIndicatorController;
        private TimeNeedle _timeNeedle;
        private ZoomUpdater _zoomUpdater;
        private TracksButton _tracksButton;

        [MenuItem("Tools/Playtest Reviewer")]
        public static void ShowEditorWindow()
        {
            var window = GetWindow<PlaytestReviewerEditor>();
            window.titleContent = new GUIContent("Playtest Reviewer");
        }

        public void CreateGUI()
        {
            InitializeUI();
            InitializeControllers();
        }

        private void InitializeUI()
        {
            var root = visualTree.CloneTree();
            rootVisualElement.Add(root);
        }

        private void InitializeControllers()
        {
            _zoomUpdater = new ZoomUpdater();
            _zoomUpdater.SubscribeToZoom(rootVisualElement);

            _videoController = new VideoController(rootVisualElement);
            _timeIndicatorController = new TimeIndicatorController(rootVisualElement, _videoController.VideoPlayer, _zoomUpdater);
            _timeNeedle = new TimeNeedle(rootVisualElement, _timeIndicatorController, _videoController.VideoPlayer);

            _videoController.OnNewVideoLengthAvailable += _timeIndicatorController.ReloadIndicators;
            _videoController.OnPlay += _timeNeedle.Initialize;

            InitializeTracks();
        }

        private void InitializeTracks()
        {
            var trackDescription = rootVisualElement.Q<VisualElement>("TrackDescriptions");
            var trackInformation = rootVisualElement.Q<ScrollView>("TrackInformation");
            var timeView = rootVisualElement.Q<ScrollView>("TimeScroll");

            _tracksButton = new TracksButton(rootVisualElement.Q<Button>("AddTracksButton"), trackDescription, trackInformation, _timeIndicatorController,rootVisualElement.Q<VisualElement>("TimeView"));

            var previewTrackIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(_iconPath);
            var videoPreview = new VideoPreviewTrack(trackDescription, trackInformation, _timeIndicatorController, _videoController.VideoPlayer);
            videoPreview.AdaptToWidth(rootVisualElement.Q<VisualElement>("TimeView"));
            videoPreview.SetTrackIcon(previewTrackIcon);

            // Synchronize horizontal scrolling
            trackInformation.horizontalScroller.valueChanged += value => timeView.horizontalScroller.value = value;
            timeView.horizontalScroller.valueChanged += value => trackInformation.horizontalScroller.value = value;
        }

        public void OpenWindow(Review review)
        {
            ShowEditorWindow();
            PopulateTracks(review.GetTrackCollecion());
            InitializeVideo(review.videoPath);

            Label nameLabel = rootVisualElement.Q<Label>("RecordingName");
            nameLabel.text = review.name;
        }

        private void InitializeVideo(string path)
        {
            _videoController.SetVideo(path);
        }

        private void PopulateTracks(TrackCollection collection)
        {
            _tracksButton.SetTrackCollection(collection);
        }
    }
}

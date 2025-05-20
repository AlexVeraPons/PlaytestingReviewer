using PlaytestingReviewer.Tracks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
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
            // this is your editor window's root
            var root = rootVisualElement;

            // query each element by its name (as defined in your UXML USS)
            Button addTracksButton = root.Q<Button>("AddTracksButton");
            ScrollView trackDescription = root.Q<ScrollView>("TrackDescriptions");
            ScrollView trackInformation = root.Q<ScrollView>("TrackInformation");
            VisualElement timeView = root.Q<VisualElement>("TimeView");
            ScrollView timeScroll = root.Q<ScrollView>("TimeScroll");




            // synchronize scrolling exactly as before
            trackInformation.horizontalScroller.valueChanged += v => timeScroll.horizontalScroller.value = v;
            timeScroll.horizontalScroller.valueChanged += v => trackInformation.horizontalScroller.value = v;
            trackDescription.verticalScroller.valueChanged += v => trackInformation.verticalScroller.value = v;
            trackInformation.verticalScroller.valueChanged += v => trackDescription.verticalScroller.value = v;
        }

        public void OpenWindow(Review review)
        {
            ShowEditorWindow();
            InitializeVideo(review.videoPath);

            Label nameLabel = rootVisualElement.Q<Label>("RecordingName");
            nameLabel.text = review.name;
        }

        private void InitializeVideo(string path)
        {
            _videoController.SetVideo(path);
        }
    }
}
#endif
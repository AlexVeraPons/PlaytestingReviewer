using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using PlaytestingReviewer.Tracks;

namespace PlaytestingReviewer.Editor
{
    public class PlaytestReviewerEditor : EditorWindow
    {
        private string _ussPath = PathManager.PlaytestReviewerUSSPath;
        private string _iconPath = PathManager.VideoPreviewIcon;
        public VisualTreeAsset visualTree;
        public Texture2D scriptableObjectIcon;

        private VideoController _videoController;
        private TimeIndicatorController _timeIndicatorController;
        private TimeNeedle _timeNeedle;
        private ZoomUpdater _zoomUpdater;

        [MenuItem("Tools/PlaytestReviewerEditor")]
        public static void ShowEditorWindow()
        {
            PlaytestReviewerEditor window = GetWindow<PlaytestReviewerEditor>();
            window.titleContent = new GUIContent("Playtest Reviewer");
        }

        public void CreateGUI()
        {
            InitializeUI();
            LoadStyleSheet();
            InitializeControllers();
        }

        private void InitializeUI()
        {
            VisualElement root = visualTree.CloneTree();
            rootVisualElement.Add(root);
        }

        private void LoadStyleSheet()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(_ussPath);
            if (styleSheet == null)
            {
                Debug.LogError($"Failed to load USS at path: {_ussPath}");
                return;
            }
            rootVisualElement.styleSheets.Add(styleSheet);
        }

        private void InitializeControllers()
        {
            _zoomUpdater = new ZoomUpdater();
            _zoomUpdater.SubscribeToZoom(rootVisualElement);


            _videoController = new VideoController(rootVisualElement);
            _timeIndicatorController = new TimeIndicatorController(rootVisualElement, _videoController.VideoPlayer, _zoomUpdater);
            _videoController.OnNewVideoLengthAvailable += _timeIndicatorController.ReloadIndicators;
            _timeNeedle = new TimeNeedle(rootVisualElement, _timeIndicatorController, _videoController.VideoPlayer);
            _videoController.OnPlay += _timeNeedle.Initialize;


            var trackDescription = rootVisualElement.Q<VisualElement>("TrackDescriptions");
            var trackInformation = rootVisualElement.Q<ScrollView>("TrackInformation");

            // var track = new VideoPreviewTrack(trackDescription,trackInformation,_timeIndicatorController,_videoController.VideoPlayer);
            // track.AdaptToWidth(rootVisualElement.Q<VisualElement>("TimeView"));
            Texture2D _previewTrackIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(_iconPath);
            // if (_previewTrackIcon == null)
            // {
            //     Debug.LogError($"Failed to load icon at path: {_iconPath}");
            //     return;
            // }
            // track.SetTrackIcon(_previewTrackIcon);
            TrackCollection tracks = TrackConverter.JsonToTracks(PathManager.VideoOutputPath + "/Mouse.json");
            MetricTrack metricTrack = new MetricTrack(trackDescription,trackInformation,_timeIndicatorController,tracks.tracks[0]);
            metricTrack.AdaptToWidth(rootVisualElement.Q<VisualElement>("TimeView"));
            metricTrack.SetTrackIcon(_previewTrackIcon);
            var timeView = rootVisualElement.Q<ScrollView>("TimeScroll");

            // Synchronize horizontal scroll values
            trackInformation.horizontalScroller.valueChanged += (value) => timeView.horizontalScroller.value = value;
            timeView.horizontalScroller.valueChanged += (value) => trackInformation.horizontalScroller.value = value;

        }
    }
}

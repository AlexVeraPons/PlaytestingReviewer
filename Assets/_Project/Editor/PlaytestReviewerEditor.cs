using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlaytestingReviewer.Editor
{
    public class PlaytestReviewerEditor : EditorWindow
    {
        private string _ussPath = PathManager.PlaytestReviewerUSSPath;
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
            var trackInformation = rootVisualElement.Q<VisualElement>("TrackInformation");

            var track = new VideoPreviewTrack(trackDescription,trackInformation,_timeIndicatorController,_videoController.VideoPlayer);
        }
    }
}

using System;
using PlaytestingReviewer.Video;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace PlaytestingReviewer.Editor
{
    public class PlaytestReviewerEditor : EditorWindow
    {
        private string _USSPath = PathManager.PlaytestReviewerUSSPath;
        public VisualTreeAsset visualTree;
        public Texture2D ScriptableObjectIcon;


        private IVideoPlayer _videoPlayer;
        private IntegerField _frameAmmountField;
        private int _frameAmmount => _frameAmmountField.value; 

        [MenuItem("Tools/PlaytestReviewerEditor")]
        public static void ShowEditorWindow()
        {
            PlaytestReviewerEditor window = GetWindow<PlaytestReviewerEditor>();
            window.titleContent = new GUIContent("Playtest Reviewer");
        }

        public void CreateGUI()
        {
            
            VisualElement root = visualTree.CloneTree();
            rootVisualElement.Add(root);

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(_USSPath);
            if (styleSheet == null)
            {
                Debug.LogError($"Failed to load USS at path: {_USSPath}");
                return;
            }

            rootVisualElement.styleSheets.Add(styleSheet);

            _videoPlayer = root.Q<UIVideoPlayer>("VideoPlayer");

            Button playButton = root.Q<Button>("Play");
            playButton.clicked += PlayClicked;

            Button nextFrameButton = root.Q<Button>("NextFrame");
            nextFrameButton.clicked += NextFrameClicked;

            Button previousFrameButton = root.Q<Button>("PreviousFrame");
            previousFrameButton.clicked += PreviousFrameClicked;

            Button goToStartButton = root.Q<Button>("Start");
            goToStartButton.clicked += GoToStartClicked;

            Button goToEndButton = root.Q<Button>("End");
            goToEndButton.clicked += GoToEndClicked;

            _frameAmmountField = root.Q<IntegerField>("FrameSkipAmmount");
        }

        private void GoToEndClicked()
        {
            _videoPlayer.GoToEnd();
        }

        private void GoToStartClicked()
        {
            _videoPlayer.GoToStart();
        }

        private void PreviousFrameClicked()
        {
            _videoPlayer.PreviousFrame(_frameAmmount);
        }

        private void NextFrameClicked()
        {
            _videoPlayer.NextFrame(_frameAmmount);
        }

        private void PlayClicked()
        {
            if(_videoPlayer.IsPlaying())
            {
                _videoPlayer.Pause();
            }
            else
            {
                _videoPlayer.Play();
            }
        }
    }
}
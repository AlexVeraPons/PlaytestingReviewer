using PlaytestingReviewer.Video;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using PlasticPipe.PlasticProtocol.Messages;


namespace PlaytestingReviewer.Editor
{
    public class PlaytestReviewerEditor : EditorWindow
    {
        private string _USSPath = PathManager.PlaytestReviewerUSSPath;
        public VisualTreeAsset visualTree;
        public Texture2D ScriptableObjectIcon;

        // Video Variables
        private IVideoPlayer _videoPlayer;
        private IntegerField _frameAmmountField;
        private int _frameAmmount => _frameAmmountField.value;


        // Time indicator vairables 
        List<Label> timeIndicators;
        private int _minimumSpaceBetweenIndicators = 10;
        private int _maximumSpaceBetweenIndicators = 20;
        private float currentSpaceBetweenIndicators = 10;
        private float _labelSize = 20;

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

            SetUpVideo(root);
            SetUpTimeControl();
        }

        private void SetUpTimeControl()
        {
            Debug.Log("SetupControl reached");

            ScrollView timeScroll = rootVisualElement.Q<ScrollView>("TimeScroll");
            VisualElement trackView = rootVisualElement.Q<VisualElement>("TrackView");
            VisualElement timeView = rootVisualElement.Q<VisualElement>("TimeView");
            timeScroll.RegisterCallback<WheelEvent>(OnMouseScroll);
            trackView.RegisterCallback<GeometryChangedEvent>(evt => ReloadIndicators(timeScroll, trackView, timeView));
        }

        private void ReloadIndicators(ScrollView timeScroll, VisualElement trackView, VisualElement timeView)
        {

            if (timeIndicators != null)
            {
                return;
            }
            
            timeIndicators = new List<Label>();
            float trackViewWidth = timeScroll.resolvedStyle.width;
            int indicatorCount = Mathf.FloorToInt(trackViewWidth / (_minimumSpaceBetweenIndicators + _labelSize));

            Debug.Log(timeIndicators.Count);

            for (int i = 0; i < indicatorCount; i++)
            {
                Label indicator = new Label();
                indicator.text = i.ToString();
                indicator.AddToClassList("timeControl");
                indicator.style.marginLeft = _minimumSpaceBetweenIndicators;
                timeView.contentContainer.Add(indicator);

                timeIndicators.Add(indicator);
            }

            UpdateTimeIndicators();
        }

        private string[] GetNumericalLabelsFromVideoLength(int indicatorCount, float videoLength = 60)
        {
            string[] labels = new string[indicatorCount];
            for (int i = 0; i < indicatorCount; i++)
            {
                float t = (float)i / (indicatorCount - 1);
                labels[i] = Mathf.Lerp(0, videoLength, t).ToString("0");
            }
            return labels;
        }


        #region TimeIndicators

        private void OnMouseScroll(WheelEvent evt)
        {
            if (evt.ctrlKey)
            {
                float zoomFactor = evt.delta.y;
                Debug.Log("Zoomby: " + zoomFactor);
                ZoomTimeIndicators(zoomFactor);

                //detect if the _maximumSpaceBetweenIndicators is reached
                if (currentSpaceBetweenIndicators > _maximumSpaceBetweenIndicators)
                {
                    UpdateTimeIndicators();
                }
            }


        }

        private void ZoomTimeIndicators(float ammount)
        {
            currentSpaceBetweenIndicators += ammount;
            foreach (VisualElement indicator in timeIndicators)
            {
                indicator.style.marginLeft = currentSpaceBetweenIndicators;
                indicator.style.marginRight = currentSpaceBetweenIndicators;
            }


        }

        #endregion

        #region Video
        private void SetUpVideo(VisualElement root)
        {
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
            if (_videoPlayer.IsPlaying())
            {
                _videoPlayer.Pause();
            }
            else
            {
                _videoPlayer.Play();
                EditorApplication.update += UpdateTimeIndicators;
            }
        }

        private void UpdateTimeIndicators()
        {
            float videoLength = _videoPlayer.GetVideoLength();
            string[] labels = GetNumericalLabelsFromVideoLength(timeIndicators.Count, videoLength);

            for (int i = 0; i < timeIndicators.Count; i++)
            {
                timeIndicators[i].text = labels[i];
            }
        }
    }
    #endregion 

}
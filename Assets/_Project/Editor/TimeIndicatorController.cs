using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor;

namespace PlaytestingReviewer.Editor
{
    public class TimeIndicatorController
    {
        private const int MinimumSpaceBetweenIndicator = 10;
        private const int InitialSpaceBetweenIndicators = 10;
        private const int MaxSpaceBeforeSplit = 50;
        private const float LabelSize = 20f;

        private List<Label> _timeIndicators;
        private ScrollView timeScroll;
        private VisualElement timeView;


        private float _currentSpaceBetweenIndicators = 10f;
        private float _videoLength;

        private float _currentZoom = 0f;
        private const float MaxZoom = 8f;
        private const float MinZoom = -8f;



        public TimeIndicatorController(VisualElement root)
        {
            SetUpTimeControl(root);
        }

        private void SetUpTimeControl(VisualElement root)
        {
            timeScroll = root.Q<ScrollView>("TimeScroll");
            timeView = root.Q<VisualElement>("TimeView");

            timeScroll.RegisterCallback<WheelEvent>(OnMouseScroll);
        }

        public void ReloadIndicators(float videoLength)
        {
            _videoLength = videoLength;
            _timeIndicators = new List<Label>();
            
            float trackViewWidth = timeView.layout.width;


            int indicatorCount = Mathf.FloorToInt(trackViewWidth / (InitialSpaceBetweenIndicators + LabelSize));
            _currentSpaceBetweenIndicators = InitialSpaceBetweenIndicators;

            timeView.Clear();

            for (int i = 0; i < indicatorCount; i++)
            {
                Label indicator = new Label { text = i.ToString() };
                indicator.AddToClassList("timeControl");
                indicator.style.marginLeft = InitialSpaceBetweenIndicators;
                timeView.contentContainer.Add(indicator);
                _timeIndicators.Add(indicator);
            }

            UpdateTimeIndicators();
        }


        private void UpdateTimeIndicators()
        {
            float videoLength = _videoLength;
            string[] labels = GetNumericalLabelsFromVideoLength(_timeIndicators.Count, videoLength);

            for (int i = 0; i < _timeIndicators.Count; i++)
            {
                _timeIndicators[i].text = labels[i];
            }
        }

        private string[] GetNumericalLabelsFromVideoLength(int indicatorCount, float videoLength)
        {
            string[] labels = new string[indicatorCount];
            for (int i = 0; i < indicatorCount; i++)
            {
                float t = (float)i / (indicatorCount - 1);
                labels[i] = Mathf.Lerp(0, videoLength, t).ToString("0");
            }
            return labels;
        }

        private void OnMouseScroll(WheelEvent evt)
        {
            if (evt.ctrlKey)
            {
                ZoomTimeIndicators(evt.delta.y);

                if (_currentSpaceBetweenIndicators > MaxSpaceBeforeSplit)
                {
                    ReloadIndicators(_videoLength);
                }

                if(_currentSpaceBetweenIndicators < MinimumSpaceBetweenIndicator)
                {
                    ReloadIndicators(_videoLength);
                }
            }
        }

        private void ZoomTimeIndicators(float amount)
        {
            Debug.Log(_currentZoom);
            _currentZoom = Mathf.Clamp(_currentZoom + amount * 0.1f, MinZoom, MaxZoom);


            _currentSpaceBetweenIndicators += amount;
            foreach (VisualElement indicator in _timeIndicators)
            {
                indicator.style.marginLeft = _currentSpaceBetweenIndicators;
                indicator.style.marginRight = _currentSpaceBetweenIndicators;
            }
        }

    }
}

using UnityEngine;
using UnityEngine.UIElements;
using PlaytestingReviewer.Tracks;

namespace PlaytestingReviewer.Editors
{
    public class TracksButton
    {
        private TrackCollection _trackCollection;
        private readonly VisualElement _descriptionContainer;
        private readonly VisualElement _informationContainer;
        private readonly ITimePositionTranslator _timeTranslator;
        private readonly VisualElement _adaptToWidth;
        private readonly VisualElement _root;


        public TracksButton(
            VisualElement root,
            TrackCollection trackCollection,
            ITimePositionTranslator timeTranslator)
        {
            _root = root;
            _trackCollection = trackCollection;
            _timeTranslator = timeTranslator;
            _descriptionContainer = root.Q<ScrollView>("TrackDescriptions");
            _informationContainer = root.Q<ScrollView>("TrackInformation");
            _adaptToWidth = root.Q<VisualElement>("TimeView");

            var addButton = root.Q<Button>("AddTracksButton");
            addButton.text = "Add Track";

            // Register a manipulator that builds the context menu on right-click (or long-press)
            addButton.clicked += OnClicked;
        }

        private void OnClicked()
        {
            foreach (var track in _trackCollection.tracks)
            {
                UITrackFactory.CreateUITrack(
                    track,
                    _descriptionContainer,
                    _informationContainer,
                    _timeTranslator,
                    _adaptToWidth
                );
            }

        }

        private void OnTrackSelected(Track track)
        {
            // Exactly the same as your editor version:
            UITrackFactory.CreateUITrack(
                track,
                _descriptionContainer,
                _informationContainer,
                _timeTranslator,
                _adaptToWidth
            );
        }

        /// <summary>
        /// If you ever need to swap in a new set of tracks at runtime:
        /// </summary>
        public void SetTrackCollection(TrackCollection newCollection)
        {
            _trackCollection = newCollection;
        }
    }
}

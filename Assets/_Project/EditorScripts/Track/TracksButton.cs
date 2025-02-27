using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using PlaytestingReviewer.Tracks;

namespace PlaytestingReviewer.Editors
{
    public class TracksButton
    {
        private TrackCollection _trackCollection;
        private VisualElement _descriptionContainer;
        private VisualElement _informationContainer;
        private ITimePositionTranslator _time;
        private VisualElement _adaptToWidth;

        public TracksButton(Button button,
        VisualElement descriptionContainer,
        VisualElement informationContainer,
        ITimePositionTranslator time,
        VisualElement adaptToWidth)
        {
            _descriptionContainer = descriptionContainer;
            _informationContainer = informationContainer;
            _time = time;
            _adaptToWidth = adaptToWidth;
            button.clicked += OnButtonClicked;
        }

        private void OnButtonClicked()
        {
            var menu = new GenericMenu();

            if (_trackCollection?.tracks != null && _trackCollection.tracks.Length > 0)
            {
                foreach (var track in _trackCollection.tracks)
                {
                    var label = $"{track.name} ({track.type})";
                    menu.AddItem(new GUIContent(label), false, () => OnTrackSelected(track));
                }
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("No tracks available"));
            }

            menu.ShowAsContext();
        }

        private void OnTrackSelected(Track track)
        {
            UITrackFactory.CreateUITrack(track, _descriptionContainer, _informationContainer, _time,_adaptToWidth);
        }

        public void SetTrackCollection(TrackCollection collection)
        {
            _trackCollection = collection;
        }
    }
}

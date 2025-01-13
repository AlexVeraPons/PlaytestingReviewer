using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;


namespace UIAssets
{
    [UxmlElement]
    public partial class Tracks : VisualElement
    {

        VisualElement trackContainer;
        public Tracks()
        {
            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;

            VisualElement leftPanel = new VisualElement();
            leftPanel.style.flexDirection = FlexDirection.Column;

            Add(leftPanel);

            trackContainer = new VisualElement();
            trackContainer.style.flexDirection = FlexDirection.Column;
            trackContainer.style.flexGrow = 1;

            Add(trackContainer);

            Button AddTrackButton = new Button { text = "Add Track" };
            AddTrackButton.style.width = 100;
            AddTrackButton.style.height = 30;

            AddTrackButton.clicked += () => AddTrack();

            leftPanel.Add(AddTrackButton);
        }

        private void AddTrack()
        {
            VisualElement track = new VisualElement();
            track.style.flexDirection = FlexDirection.Column;
            track.style.height = 60;
            track.style.marginBottom = 5;
            track.style.backgroundColor = new Color(0.23f, 0.23f, 0.23f, 1);
            track.AddToClassList("track");

            Label trackLabel = new Label($"Track {trackContainer.childCount + 1}");
            trackLabel.AddToClassList("track-label");
            track.Add(trackLabel);

            VisualElement markerArea = new VisualElement();
            markerArea.AddToClassList("marker-area");
            markerArea.RegisterCallback<MouseDownEvent>(evt => AddMarker(evt, markerArea));
            markerArea.style.height = 30;
            markerArea.style.backgroundColor = new Color(0.27f, 0.27f, 0.27f, 1);
            markerArea.style.position = Position.Relative;

            track.Add(markerArea);


            trackContainer.Add(track);
        }



        private void AddMarker(MouseDownEvent evt, VisualElement markerArea)
        {
            VisualElement marker = new VisualElement();
            marker.AddToClassList("marker");
            marker.style.width = 5;
            marker.style.height = 20;
            marker.style.backgroundColor = Color.red;  
            marker.style.position = Position.Absolute;
            marker.style.top = 0;
            marker.style.left = new Length(evt.localMousePosition.x, LengthUnit.Pixel); 
            markerArea.Add(marker);
        }

    }
}
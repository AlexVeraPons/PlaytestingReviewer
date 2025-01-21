using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlaytestingReviewer.Editor
{
    public class Track
    {
        protected VisualElement _infoRoot;
        protected VisualElement _descriptionRoot;

        protected VisualElement _descriptionContainer;
        protected VisualElement _informationContainer;

        /// <summary>
        /// Constructor for the track class
        /// </summary>
        /// <param name="description"> where you want the label and what the track does</param>
        /// <param name="information">where you want the information to be visualized</param>
        public Track(VisualElement description, VisualElement information)
        {
            _infoRoot = information;
            _descriptionRoot = description;

            InitializeDescription(description);
            InitializeInformation(information);
        }

        protected virtual void InitializeInformation(VisualElement information)
        {
            _informationContainer = new VisualElement();
            _informationContainer.style.backgroundColor = new StyleColor(Color.gray);
            _informationContainer.style.flexDirection = FlexDirection.Row;
            _informationContainer.style.alignItems = Align.Center;
            _informationContainer.style.height = 40;
            _informationContainer.style.marginBottom = 5;
            _informationContainer.style.marginTop = 5;

            information.Add(_informationContainer);
        }

        protected virtual void InitializeDescription(VisualElement description)
        {
            //create main container
            _descriptionContainer = new VisualElement();
            _descriptionContainer.style.maxWidth = 200;
            _descriptionContainer.style.backgroundColor = new StyleColor(Color.gray);
            _descriptionContainer.style.flexDirection = FlexDirection.Row;
            _descriptionContainer.style.alignItems = Align.Center;
            _descriptionContainer.style.height = 40;
            _descriptionContainer.style.marginBottom = 5;
            _descriptionContainer.style.marginTop = 5;

            //create a spacer
            var spaceElementt = new VisualElement();
            spaceElementt.style.width = 20;
            spaceElementt.style.height = 20;
            _descriptionContainer.Add(spaceElementt);

            // Create square visual element with black background
            var squareElement = new VisualElement();
            squareElement.style.width = 20;
            squareElement.style.height = 20;
            squareElement.style.backgroundColor = new StyleColor(Color.black);
            _descriptionContainer.Add(squareElement);

            // Create visual element with label inside and grey background
            var labelContainer = new VisualElement();
            labelContainer.style.backgroundColor = new StyleColor(Color.grey);
            labelContainer.style.paddingTop = 10;
            labelContainer.style.paddingBottom = 10;
            labelContainer.style.paddingLeft = 10;
            labelContainer.style.paddingRight = 10;

            var label = new Label("Description");
            labelContainer.Add(label);
            _descriptionContainer.Add(labelContainer);

            // Create button
            var button = new Button() { text = ":" };
            button.clicked += OnButtonClicked;
            _descriptionContainer.Add(button);

            description.Add(_descriptionContainer);
        }

        protected virtual void OnButtonClicked()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Delete Track"), false, DeleteTrack);
            menu.AddItem(new GUIContent("ToggleVisibility"), false, ToggleVisibility);
            menu.ShowAsContext();
        }

        private void ToggleVisibility()
        {
            if (_informationContainer == null)
            {
                InitializeInformation(_infoRoot);
            }
            else
            {
                _informationContainer.RemoveFromHierarchy();
                _informationContainer.Clear();
                _informationContainer = null;
            }
        }

        private void DeleteTrack()
        {
            _descriptionContainer.RemoveFromHierarchy();
            _informationContainer.RemoveFromHierarchy();
        }
    }
}
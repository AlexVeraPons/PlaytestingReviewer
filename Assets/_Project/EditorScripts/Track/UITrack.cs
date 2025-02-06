using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlaytestingReviewer.Editors
{
    /// <summary>
    /// An abstract base class for implementing visual UI tracks within the Unity Editor environment.
    /// This class provides a framework for managing UI layouts, descriptions, and actions related to the visualization of information in time-based interactions.
    /// </summary>
    public abstract class UITrack
    {
        protected Action OnResize;

        protected ITimePositionTranslator TimeRelations;

        protected VisualElement InfoRoot;
        protected VisualElement DescriptionRoot;
        protected VisualElement InformationContainer;
        protected VisualElement DescriptionContainer;
        private VisualElement _coloredBar;

        protected float TrackHeight = 40;

        private float _resizeDebounceTimer = 0f;
        protected float ResizeDebounceDuration = 1f;
        private bool _shouldStartResizeTimer = false;

        protected StyleColor BarColor = new StyleColor(new Color(0.90f, 0.90f, 1f));

        private VisualElement _elementToAdaptToWidth;
        private VisualElement _imageContainer;
        protected string Title = "Description";
        protected Label DescriptionLabel;

        protected UITrack(VisualElement description, VisualElement information, ITimePositionTranslator timeRelations)
        {
            TimeRelations = timeRelations;

            DescriptionRoot = description;
            InfoRoot = information;
            EditorApplication.update += TrackUpdate;

            Initialization();
        }

        private void Initialization()
        {
            PreInitialization();

            InitializeDescription(DescriptionRoot);
            InitializeInformation(InfoRoot);
        }

        /// <summary>
        /// Called during the initial setup phase of the UITrack, before initializing other components such as
        /// the description or information sections. This can be overriden to additional data, establish configurations,
        /// or execute tasks required prior to the main initialization process.
        /// </summary>
        protected virtual void PreInitialization()
        {
        }

        /// <summary>
        /// Executes frame-based updates for managing timing-related operations and UI element adjustments within the track.
        /// </summary>
        protected virtual void TrackUpdate()
        {
            if (_shouldStartResizeTimer == true)
            {
                _resizeDebounceTimer -= Time.deltaTime;
                if (_resizeDebounceTimer <= 0)
                {
                    _resizeDebounceTimer = 0;
                    _shouldStartResizeTimer = false;
                    OnResize?.Invoke();
                }
            }
        }
        
        /// <summary>
        /// Creates the information container where all the data is going to be visualized 
        /// </summary>
        protected virtual void InitializeInformation(VisualElement information)
        {
            InformationContainer = new VisualElement
            {
                style =
                {
                    // Background color
                    backgroundColor = new StyleColor(new Color(0.20f, 0.20f, 0.20f)),

                    // Borders
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = new StyleColor(Color.black),
                    borderBottomColor = new StyleColor(Color.black),
                    borderLeftColor = new StyleColor(Color.black),
                    borderRightColor = new StyleColor(Color.black),

                    // Rounding
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6,

                    // Layout
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    height = TrackHeight,
                    marginBottom = 5,
                    marginTop = 5,
                    paddingLeft = 10,
                    paddingRight = 10
                }
            };

            information.Add(InformationContainer);
        }

        /// <summary>
        /// Sets up the description section of the UITrack. This method can be overridden to customize
        /// the visual layout, content, or features of the description area within the UI track.
        /// </summary>
        protected virtual void InitializeDescription(VisualElement description)
        {
            DescriptionContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    height = TrackHeight,
                    marginBottom = 5,
                    marginTop = 5,

                    backgroundColor = new StyleColor(new Color(0.35f, 0.35f, 0.35f)),

                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = new StyleColor(Color.black),
                    borderBottomColor = new StyleColor(Color.black),
                    borderLeftColor = new StyleColor(Color.black),
                    borderRightColor = new StyleColor(Color.black),
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6,

                    paddingLeft = 5,
                    paddingRight = 5,
                }
            };


             _coloredBar = new VisualElement
            {
                style =
                {
                    width = 8,
                    height = new Length(100, LengthUnit.Percent),
                    backgroundColor = BarColor,
                    marginLeft = -5f,
                    borderBottomLeftRadius = 6,
                    borderTopLeftRadius = 6
                }
            };
            DescriptionContainer.Add(_coloredBar);

            var spacerElement = new VisualElement
            {
                style =
                {
                    width = 5,
                    height = 20
                }
            };
            DescriptionContainer.Add(spacerElement);

            _imageContainer = new VisualElement
            {
                style =
                {
                    width = 20,
                    height = 20,
                    marginLeft = 10,
                    marginRight = 10
                }
            };
            DescriptionContainer.Add(_imageContainer);

            var labelContainer = new VisualElement
            {
                style =
                {
                    backgroundColor = new StyleColor(Color.grey),
                    paddingTop = 5,
                    paddingBottom = 5,
                    paddingLeft = 10,
                    paddingRight = 10,

                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,

                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = new StyleColor(Color.black),
                    borderBottomColor = new StyleColor(Color.black),
                    borderLeftColor = new StyleColor(Color.black),
                    borderRightColor = new StyleColor(Color.black)
                }
            };

            DescriptionLabel = new Label(Title)
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    fontSize = 14
                }
            };

            labelContainer.Add(DescriptionLabel);
            DescriptionContainer.Add(labelContainer);

            // Button for context menu
            var button = new Button() { text = ":" };
            button.clicked += OnThreeDotsClicked;
            button.style.marginLeft = 10;
            DescriptionContainer.Add(button);

            description.Add(DescriptionContainer);
        }

        
        private void OnThreeDotsClicked()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Delete Track"), false, DeleteTrack);
            menu.AddItem(new GUIContent("Toggle Visibility"), false, ToggleVisibility);
            AddMenuItems(menu);
            menu.ShowAsContext();
        }

        /// <summary>
        /// Override this method to include additional menu options specific to the implementation.
        /// </summary>
        /// <param name="menu">The GenericMenu instance to which new menu items can be added to.</param>
        protected virtual void AddMenuItems(GenericMenu menu)
        {
        }

        /// <summary>
        /// Toggles the visibility of the information container associated with the UI track.
        /// Executes all logic needed to do this.
        /// </summary>
        protected virtual void ToggleVisibility()
        {
            if (InformationContainer == null)
            {
                InitializeInformation(InfoRoot);
            }
            else
            {
                InformationContainer.RemoveFromHierarchy();
                InformationContainer.Clear();
                InformationContainer = null;
            }
        }

        private void DeleteTrack()
        {
            DescriptionContainer?.RemoveFromHierarchy();
            InformationContainer?.RemoveFromHierarchy();

            DescriptionContainer = null;
            InformationContainer = null;
        }

        public void AdaptToWidth(VisualElement element)
        {
            _elementToAdaptToWidth = element;
            element.RegisterCallback<GeometryChangedEvent>(_ => ElementResized());
        }

        /// <summary>
        /// Handles the resize event when the associated visual element's dimensions change.
        /// This method can be overridden for custom behavior in derived classes.
        /// </summary>
        protected virtual void ElementResized()
        {
            _shouldStartResizeTimer = true;
            _resizeDebounceTimer = ResizeDebounceDuration;

            if (InformationContainer != null && _elementToAdaptToWidth != null)
            {
                InformationContainer.style.width = _elementToAdaptToWidth.resolvedStyle.width;
            }
        }
        public void SetTrackIcon(Texture2D image)
        {
            _imageContainer.style.backgroundImage = image;
        }

        protected void ChangeBarColor(Color color)
        {
            BarColor = color;
            _coloredBar.style.backgroundColor = BarColor;
        }
    }
}
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

public class TimelineExampleWindow : EditorWindow
{
    private ScrollView pinnedPanel;
    private ScrollView timelineScrollView;
    private VisualElement pinnedTracksContainer;
    private VisualElement timelineTracksContainer;
    private Button addTrackButton;
    private Button randomizeButton;

    private float baseTrackWidth = 2000f;
    private float horizontalZoomFactor = 1.0f;
    private float verticalZoomFactor = 1.0f;
    private float baseTrackHeight = 40f;

    private System.Random random = new System.Random();
    private int trackCount = 0;

    [MenuItem("Window/Timeline Example")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<TimelineExampleWindow>();
        wnd.titleContent = new GUIContent("Timeline Example");
        wnd.Show();
    }

    private void CreateGUI()
    {
        // Load the UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Project/TrackBehaviour/TimelineExample.uxml");
        visualTree.CloneTree(rootVisualElement);

        // Get references
        pinnedPanel = rootVisualElement.Q<ScrollView>("pinnedPanel");
        pinnedTracksContainer = rootVisualElement.Q<VisualElement>("pinnedTracksContainer");
        timelineScrollView = rootVisualElement.Q<ScrollView>("timelineScrollView");
        timelineTracksContainer = rootVisualElement.Q<VisualElement>("timelineTracksContainer");

        addTrackButton = rootVisualElement.Q<Button>("addTrackButton");
        randomizeButton = rootVisualElement.Q<Button>("randomizeButton");

        // Set button actions
        addTrackButton.clicked += AddTrack;
        randomizeButton.clicked += RandomizeClips;

        // Enable zooming
        timelineScrollView.RegisterCallback<WheelEvent>(OnWheelEvent);

        // Synchronize vertical scrolling
        SyncVerticalScrolling();

        // Add initial tracks
        for (int i = 0; i < 5; i++)
            AddTrack();
    }

    private void SyncVerticalScrolling()
    {
        // Timeline to pinned panel
        timelineScrollView.verticalScroller.valueChanged += (value) =>
        {
            pinnedPanel.verticalScroller.value = value;
        };

        // Pinned panel to timeline
        pinnedPanel.verticalScroller.valueChanged += (value) =>
        {
            timelineScrollView.verticalScroller.value = value;
        };
    }

    private void AddTrack()
    {
        trackCount++;

        // Pinned panel label
        var trackLabel = new Label($"Track {trackCount}")
        {
            style =
            {
                paddingLeft = 8,
                height = baseTrackHeight * verticalZoomFactor,
                unityTextAlign = TextAnchor.MiddleLeft,
                color = Color.white
            }
        };
        pinnedTracksContainer.Add(trackLabel);

        // Timeline track
        var trackRow = new VisualElement
        {
            style =
            {
                height = baseTrackHeight * verticalZoomFactor,
                width = baseTrackWidth * horizontalZoomFactor,
                flexDirection = FlexDirection.Row,
                borderBottomWidth = 1,
                borderBottomColor = Color.gray
            }
        };

        // Add randomized clips
        int clipCount = random.Next(2, 6);
        for (int c = 0; c < clipCount; c++)
        {
            var clip = new VisualElement
            {
                style =
                {
                    backgroundColor = RandomColor(),
                    flexGrow = random.Next(1, 4),
                    marginLeft = 5,
                    marginRight = 5,
                    height = Length.Percent(100)
                }
            };
            trackRow.Add(clip);
        }

        timelineTracksContainer.Add(trackRow);
    }

    private void RandomizeClips()
    {
        foreach (var trackRow in timelineTracksContainer.Children())
        {
            foreach (var clip in trackRow.Children())
            {
                clip.style.backgroundColor = RandomColor();
                clip.style.flexGrow = random.Next(1, 4);
            }
        }
    }

    private void ApplyZoom()
    {
        // Apply horizontal and vertical zoom to both panels
        for (int i = 0; i < timelineTracksContainer.childCount; i++)
        {
            // Timeline track
            var timelineTrack = timelineTracksContainer.ElementAt(i);
            timelineTrack.style.width = new Length(baseTrackWidth * horizontalZoomFactor, LengthUnit.Pixel);
            timelineTrack.style.height = new Length(baseTrackHeight * verticalZoomFactor, LengthUnit.Pixel);

            // Pinned panel track
            var pinnedTrack = pinnedTracksContainer.ElementAt(i);
            pinnedTrack.style.height = new Length(baseTrackHeight * verticalZoomFactor, LengthUnit.Pixel);
        }

        // Force refresh
        timelineTracksContainer.MarkDirtyRepaint();
        pinnedTracksContainer.MarkDirtyRepaint();
    }

    private void OnWheelEvent(WheelEvent evt)
    {
            Debug.Log(evt.delta.y);
        if (evt.ctrlKey && evt.altKey)
        {
            // Ctrl + Shift + Scroll = Vertical Zoom
            if (evt.delta.y > 0)
                VerticalZoomIn();
            else
                VerticalZoomOut();

            evt.StopPropagation();
        }
        else if (evt.ctrlKey)
        {
            // Ctrl + Scroll = Horizontal Zoom
            if (evt.delta.y < 0)
                HorizontalZoomIn();
            else
                HorizontalZoomOut();

            evt.StopPropagation();
        }
    }

    private void HorizontalZoomIn()
    {
        horizontalZoomFactor *= 1.1f;
        ApplyZoom();
    }

    private void HorizontalZoomOut()
    {
        horizontalZoomFactor /= 1.1f;
        ApplyZoom();
    }

    private void VerticalZoomIn()
    {
        verticalZoomFactor *= 1.1f;
        ApplyZoom();
    }

    private void VerticalZoomOut()
    {
        verticalZoomFactor /= 1.1f;
        ApplyZoom();
    }

    private Color RandomColor()
    {
        return new Color(
            (float)random.NextDouble(),
            (float)random.NextDouble(),
            (float)random.NextDouble());
    }
}

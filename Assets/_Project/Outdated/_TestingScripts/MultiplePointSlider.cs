using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class MultiPointSlider : VisualElement
{
    private float minValue = 0f;
    private float maxValue = 1f;
    private int pointCount = 3;
    private float[] pointValues;

    private VisualElement track;
    private VisualElement[] handles;

    public MultiPointSlider()
    {
        // Initialize point values evenly spaced
        pointValues = new float[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            pointValues[i] = Mathf.Lerp(minValue, maxValue, (float)i / (pointCount - 1));
        }

        // Create the track
        track = new VisualElement();
        track.style.height = 4;
        track.style.backgroundColor = Color.gray;
        track.style.flexGrow = 1;
        track.style.marginTop = 10;
        track.style.marginBottom = 10;
        track.style.position = Position.Relative;

        Add(track);

        // Create draggable handles
        handles = new VisualElement[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            var handle = new VisualElement();
            handle.style.width = 16;
            handle.style.height = 16;
            handle.style.backgroundColor = Color.white;
            handle.style.position = Position.Absolute;
            handle.style.top = -6;

            int index = i;
            handle.RegisterCallback<PointerDownEvent>(evt => OnHandleDragStart(evt, index));
            track.Add(handle);
            handles[i] = handle;
        }

        RegisterCallback<GeometryChangedEvent>(_ => UpdateHandles());
    }

    // Update handle positions based on their values
    private void UpdateHandles()
    {
        float trackWidth = track.resolvedStyle.width;
        for (int i = 0; i < pointCount; i++)
        {
            float normalizedValue = Mathf.InverseLerp(minValue, maxValue, pointValues[i]);
            handles[i].style.left = normalizedValue * (trackWidth - handles[i].resolvedStyle.width);
        }
    }

    private void OnHandleDragStart(PointerDownEvent evt, int index)
    {
        this.CapturePointer(evt.pointerId);
        RegisterCallback<PointerMoveEvent>(e => OnHandleDrag(e, index));
        RegisterCallback<PointerUpEvent>(e => OnHandleDragEnd(e, index));
    }

    private void OnHandleDrag(PointerMoveEvent evt, int index)
    {
        float localX = evt.localPosition.x;
        float trackWidth = track.resolvedStyle.width;
        float normalizedValue = Mathf.Clamp01(localX / trackWidth);
        pointValues[index] = Mathf.Lerp(minValue, maxValue, normalizedValue);

        UpdateHandles();
    }

    private void OnHandleDragEnd(PointerUpEvent evt, int index)
    {
        this.ReleasePointer(evt.pointerId);
        UnregisterCallback<PointerMoveEvent>(e => OnHandleDrag(e, index));
        UnregisterCallback<PointerUpEvent>(e => OnHandleDragEnd(e, index));
    }
}

using UnityEngine.UIElements;
using PlaytestingReviewer.Tracks;

namespace PlaytestingReviewer.Editors
{
    /// <summary>
    /// Provides a factory for creating UI tracks from different track types.
    /// </summary>
    public static class UITrackFactory
    {
        /// <summary>
        /// Creates a UI track based on the specified track type.
        /// </summary>
        /// <param name="track">The track to create a UI track for.</param>
        /// <param name="descriptionContainer">The container for the track's description.</param>
        /// <param name="informationContainer">The container for the track's information.</param>
        /// <param name="timeTranslator">The time position translator for the track.</param>
        /// <param name="adaptToWidth">The VisualElement that you have to copy it's size</param>
        /// <returns>A UI track corresponding to the specified track type, or null if the track type is unsupported.</returns>
        public static UITrack CreateUITrack(
            Track track,
            VisualElement descriptionContainer,
            VisualElement informationContainer,
            ITimePositionTranslator timeTranslator,
            VisualElement adaptToWidth)
        {
            switch (track.type)
            {
                case TrackType.Metric:
                    var metricTrack = new UIMetricTrack(descriptionContainer, informationContainer, timeTranslator, track);
                    metricTrack.AdaptToWidth(adaptToWidth);
                    metricTrack.DelayedConfigureTrack();
                    return metricTrack;
                default:
                    UnityEngine.Debug.LogError($"Unsupported TrackType: {track.type}");
                    return null;
            }
        }
    }
}

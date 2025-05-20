namespace PlaytestingReviewer.Editors
{
    /// <summary>
    /// Defines a contract for translating between time values and screen-space x-coordinates
    /// in the context of a video timeline or similar visual representation.
    /// </summary>
    public interface ITimePositionTranslator
    {
        /// <summary>
        /// Calculates and returns the corresponding timestamp in a video timeline
        /// based on a given screen-space x-coordinate position.
        /// </summary>
        /// <param name="x">The screen-space x-coordinate position.</param>
        /// <returns>The time in seconds corresponding to the given x-coordinate.</returns>
        public float GetTimeFromScreenX(float x);

        /// <summary>
        /// Calculates and returns the corresponding screen-space x-coordinate position
        /// based on a given timestamp within a video timeline.
        /// </summary>
        /// <param name="time">The time in seconds within the video timeline.</param>
        /// <returns>The x-coordinate corresponding to the given time.</returns>
        public float GetXPositionFromTime(float time);

        /// <summary>
        /// Determines whether the current setup of the instance is complete and valid.
        /// </summary>
        /// <returns>True if the setup is complete and valid, otherwise false.</returns>
        public bool IsSetupComplete();

        /// <summary>
        /// Retrieves the total duration of the associated video or timeline.
        /// </summary>
        /// <returns>The total time in seconds.</returns>
        public float GetTotalTime();
    }
}

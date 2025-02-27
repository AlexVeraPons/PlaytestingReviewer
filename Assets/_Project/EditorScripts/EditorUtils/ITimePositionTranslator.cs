namespace PlaytestingReviewer.Editors
{
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
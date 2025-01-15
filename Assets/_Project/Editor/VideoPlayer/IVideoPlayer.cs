namespace PlaytestingReviewer.Video
{
    public interface IVideoPlayer
    {
        /// <summary>
        /// Play the video
        /// </summary>
        void Play();

        /// <summary>
        /// Pause the video
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Stop the video
        /// </summary>
        void Stop();

        /// <summary>
        /// Go to the next frame of the video
        /// </summary>
        void NextFrame(int frameCount = 1);

        /// <summary>
        /// Go to the previous frame of the video
        /// </summary>
        void PreviousFrame(int frameCount = 1);

        /// <summary>
        /// Go to the start of the video
        /// </summary>
        void GoToStart();


        /// <summary>
        /// Go to the end of the video
        /// </summary>
        void GoToEnd();

        bool IsPlaying();
    }
}
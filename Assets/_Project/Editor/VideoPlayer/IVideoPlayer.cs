using UnityEngine;

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
        /// Set the frame of the video
        /// </summary>
        /// <param name="frame">The frame to set</param>
        void SetFrame(int frame);

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
        
        /// <summary>
        /// Check if the video is playing
        /// </summary>
        /// <returns>True if the video is playing, false otherwise</returns>
        bool IsPlaying();

        /// <summary>
        /// Get the length of the video
        /// </summary>
        /// <returns>The length of the video in seconds</returns>
        float GetVideoLengthSeconds();

        /// <summary>
        /// Get the current time of the video
        /// </summary>
        float GetCurrentTime();

        /// <summary>
        /// Get the length of the video in frames
        /// </summary>
        int GetVideoLengthFrames();

        /// <summary>
        /// Get the current frame of the video
        /// </summary>
        int GetCurrentFrame();
        string GetVideoPath();
    }
}
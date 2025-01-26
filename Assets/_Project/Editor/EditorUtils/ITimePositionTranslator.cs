namespace PlaytestingReviewer.Editor
{
    public interface ITimePositionTranslator
    {
        public float GetTimeFromWorldPosition(float x);

        public float GetTimeFromLeftWorldPosition(float x);

        public bool SetupComplete();
        public float GetTotalTime();
    }
}
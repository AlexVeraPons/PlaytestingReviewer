namespace PlaytestingReviewer.Editor
{
    public interface IProvideTimeRelations
    {
        public float GetTimeFromWorldPosition(float x);

        public float GetTimeFromLeftWorldPosition(float x);

        public bool SetupComplete();
    }
}
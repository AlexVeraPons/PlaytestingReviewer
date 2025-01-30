using UnityEngine;
using UnityEditor;
using PlaytestingReviewer.Tracks;
using PlasticPipe.PlasticProtocol.Messages;

namespace PlaytestingReviewer.Editors
{
    [CreateAssetMenu(menuName = "PlaytestReviwer/Review")]
    public class Review : ScriptableObject
    {
        public string Name;
        public string videoPath;
        public string tracksPath;

        public TrackCollection GetTrackCollecion()
        {
            return new TrackCollection(tracksPath);
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(Review))]
    public class ReviewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Review review = (Review)target;
            DrawDefaultInspector();
        }

        private void OnEnable()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        private void OnDisable()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
        }

        //on double click
        private void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.clickCount == 2 && selectionRect.Contains(e.mousePosition))
            {
                Debug.Log("Double-clicked on Review asset:");
            }

            if (e.type == EventType.MouseDown && e.clickCount == 2 && selectionRect.Contains(e.mousePosition))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Review review = AssetDatabase.LoadAssetAtPath<Review>(path);

                if (review != null)
                {
                    PlaytestReviewerEditor editor = ScriptableObject.CreateInstance<PlaytestReviewerEditor>();
                    editor.OpenWindow(review);
                }
            }
        }
    }
#endif
}
using UnityEngine;
using UnityEngine.UIElements;
using SFB;    

[RequireComponent(typeof(UIDocument))]
public class ReviewLoaderUI : MonoBehaviour
{
    private Button _openReviewButton;
    private PlaytestReviewerRuntime _runtimeUi;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // 1) Find your button in the UXML (give it name="OpenReviewButton")
        _openReviewButton = root.Q<Button>("OpenReviewButton");
        _openReviewButton.text = "Open Review";
        _openReviewButton.clicked += OnOpenReviewClicked;

        // 2) Cache your runtime UI driver so we can call LoadReview on it
        _runtimeUi = GetComponent<PlaytestReviewerRuntime>();
    }

    private void OnOpenReviewClicked()
    {
        // Open a file‐picker for the track JSON
        var trackPaths = StandaloneFileBrowser.OpenFilePanel(
            "Select Track JSON",
            "",       // default path
            "json",   // extension filter
            false     // allow multiple?
        );
        if (trackPaths == null || trackPaths.Length == 0)
            return;

        // Then open one for the video
        var videoPaths = StandaloneFileBrowser.OpenFilePanel(
            "Select Video File",
            "",
            "mp4,avi,mov",  // you can comma‐separate multiple exts
            false
        );
        if (videoPaths == null || videoPaths.Length == 0)
            return;

        // 3) Feed both into your LoadReview
        _runtimeUi.LoadReview(trackPaths[0], videoPaths[0]);
    }
}
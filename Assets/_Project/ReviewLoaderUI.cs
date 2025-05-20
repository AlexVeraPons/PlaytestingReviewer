using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ReviewLoaderUI : MonoBehaviour
{
    Button _openFolder;
    Button _refresh;
    ListView _reviewList;

    struct ReviewItem
    {
        public string name;
        public string trackPath;
        public string videoPath;
        public override string ToString() => name;
    }

    readonly List<ReviewItem> _items = new();
    PlaytestReviewerRuntime _runtime;
    private ReviewFolder _folder;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _openFolder = root.Q<Button>("OpenReviewsFolderButton");
        _refresh = root.Q<Button>("RefreshButton");
        _reviewList = root.Q<ListView>("ReviewListView");

        _openFolder.clicked += OnOpenFolderClicked;
        _refresh.clicked += PopulateList;

        _runtime = GetComponent<PlaytestReviewerRuntime>();
        _folder = FindObjectOfType<ReviewFolder>();

        _reviewList.makeItem = () =>
        {
            var label = new Label();

            label.style.flexGrow  = 1;                                 
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.fontSize  = 14;
            label.style.color     = new StyleColor(Color.white);       

            return label;
        };

        _reviewList.bindItem = (ve, i) => // CHANGE THIS
            ((Label)ve).text = _items[i].name;
        _reviewList.selectionType = SelectionType.Single;
        _reviewList.selectionChanged += OnSelection;

        PopulateList();
    }

    void OnOpenFolderClicked()
    {
        Application.OpenURL($"file://{_folder.ReviewsRoot}");
    }

    void PopulateList()
    {
        _items.Clear();

        string root = _folder.ReviewsRoot;
        if (string.IsNullOrEmpty(root)) return;

        foreach (string dir in Directory.GetDirectories(root))
        {
            string[] jsons = Directory.GetFiles(dir, "*.json",
                SearchOption.AllDirectories);
            string[] mp4s  = Directory.GetFiles(dir, "*.mp4",
                SearchOption.AllDirectories);

            if (jsons.Length == 0 || mp4s.Length == 0)
                continue;                                

            _items.Add(new ReviewItem
            {
                name      = Path.GetFileName(dir),
                trackPath = jsons[0],                    
                videoPath = mp4s[0]                    
            });
        }

        _reviewList.itemsSource = _items;
        _reviewList.Rebuild();
    }

    void OnSelection(IEnumerable<object> sel)
    {
        foreach (object o in sel)
        {
            var item = (ReviewItem)o;
            _runtime.LoadReview(item.trackPath, item.videoPath);
            Debug.Log($"[ReviewLoaderUI] Loaded review “{item.name}”");
        }
        
        foreach (object o in sel)
        {
            var item = (ReviewItem)o;
            _runtime.LoadReview(item.trackPath, item.videoPath);
            Debug.Log($"[ReviewLoaderUI] Loaded review “{item.name}”");
        }
    }
}
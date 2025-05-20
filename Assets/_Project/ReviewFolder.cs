using System.IO;
using UnityEngine;

public class ReviewFolder : MonoBehaviour
{
    public string ReviewsRoot => _reviewsRoot;
    string _reviewsRoot;

    void Awake()
    {
        _reviewsRoot = Path.Combine(Application.persistentDataPath, "Reviews");
        Directory.CreateDirectory(_reviewsRoot);
        Debug.Log($"[ReviewBootstrap] Reviews folder: {_reviewsRoot}");
    }
}
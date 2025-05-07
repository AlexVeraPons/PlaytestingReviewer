using System.Collections.Generic;
using UnityEngine;

namespace PlaytestingReviewer
{
    /// <summary>
    /// Transforms a game object into a Singleton (by ID) that survives
    /// scene loads and immediately kills any later duplicates—even
    /// before those duplicates can run Awake/Start.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class PersistentSingleton : MonoBehaviour
    {
        [Tooltip("A short name that describes the object.\n" +
                 "Objects with the same ID behave like a singleton.")]
        [SerializeField]
        private string persistenceID = "NO_ID";

        private static readonly Dictionary<string, PersistentSingleton> registry
            = new Dictionary<string, PersistentSingleton>();

        public string PersistenceID => persistenceID;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void PreloadCleanup()
        {
            var all = Object.FindObjectsOfType<PersistentSingleton>(true);
            var seenIDs = new HashSet<string>();

            foreach (var inst in all)
            {
                if (string.IsNullOrWhiteSpace(inst.persistenceID))
                    continue;

                if (seenIDs.Add(inst.persistenceID))
                {
                    Object.DontDestroyOnLoad(inst.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(inst.gameObject);
                }
            }
        }

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(persistenceID))
            {
                Debug.LogError($"{gameObject.name} has a blank persistenceID — destroying immediately.");
                Object.DestroyImmediate(gameObject);
                return;
            }

            if (registry.TryGetValue(persistenceID, out var existing))
            {
                if (existing != this)
                {
                    Object.DestroyImmediate(gameObject);
                    return;
                }
            }
            else
            {
                registry[persistenceID] = this;
            }
        }

        private void OnDestroy()
        {
            if (registry.TryGetValue(persistenceID, out var keeper) && keeper == this)
                registry.Remove(persistenceID);
        }

        public static GameObject GetObject(string id)
            => registry.TryGetValue(id, out var keeper) ? keeper.gameObject : null;

        public static T GetComponent<T>(string id) where T : Component
            => registry.TryGetValue(id, out var keeper) ? keeper.GetComponent<T>() : null;
    }
}
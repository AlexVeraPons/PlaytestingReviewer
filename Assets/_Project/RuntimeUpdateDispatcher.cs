using System;
using UnityEngine;

public class RuntimeUpdateDispatcher : MonoBehaviour
{
    public static event Action Tick;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("[RuntimeUpdateDispatcher]");
        DontDestroyOnLoad(go);
        go.AddComponent<RuntimeUpdateDispatcher>();
    }

    void Update() => Tick?.Invoke();
}
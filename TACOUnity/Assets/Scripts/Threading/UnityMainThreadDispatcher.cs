// UnityMainThreadDispatcher.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    static readonly Queue<Action> queue = new Queue<Action>();
    static UnityMainThreadDispatcher instance;

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        lock (queue)
            while (queue.Count > 0)
                queue.Dequeue()();
    }

    public static void Enqueue(Action action)
    {
        lock (queue) queue.Enqueue(action);
    }
}
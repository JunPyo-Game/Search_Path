using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

abstract public class PathSearch : MonoBehaviour
{
    static protected readonly WaitForSeconds _waitForSeconds_1 = new(.01f);
    static protected readonly Vector3[] directions = {
        Vector3.forward, Vector3.back, Vector3.right, Vector3.left,
        Vector3.forward + Vector3.right,   // 북동
        Vector3.forward + Vector3.left,    // 북서
        Vector3.back + Vector3.right,      // 남동
        Vector3.back + Vector3.left        // 남서
    };

    public class PathNode
    {
        public Vector3 position;
        public PathNode prevNode;
        public GameObject marker;
    }
    private readonly Stack<PathNode> path = new();
    protected readonly HashSet<Vector3> visited = new();

    // 실제 적용 시 삭제 예정
    [SerializeField] protected GameObject visitedMark;

    public Vector3 Destination { get; set; }
    public UnityEvent<IEnumerable<PathNode>> OnSearchPath { get; set; } = new();
    public Func<Vector3, Vector3, bool> IsValidMove { get; set; }

    protected virtual void SetPath(PathNode node)
    {
        for (var current = node; current != null; current = current.prevNode)
        {
            path.Push(current);
        }

        OnSearchPath?.Invoke(path);
    }

    abstract protected IEnumerator SearchRoutine(float moveDistance);
    abstract public void SearchPath(float moveDistance);
}
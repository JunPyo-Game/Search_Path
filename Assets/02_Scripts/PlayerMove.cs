using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PathSearch;

public enum SearchMode { DFS, BFS, AStar }

[RequireComponent(typeof(DepthFirstSearch))]
[RequireComponent(typeof(BreadthFirstSearch))]
[RequireComponent(typeof(AStar))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private SearchMode searchMode = SearchMode.DFS;
    [SerializeField] protected Transform destination;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float moveDistance = 2.0f;

    private DepthFirstSearch dfs;
    private BreadthFirstSearch bfs;
    private AStar aStar;
    private AStar_IndexedPriorityQueueOnly aStar_IndexedPriorityQueueOnly;

    private void Awake()
    {
        dfs = GetComponent<DepthFirstSearch>();
        dfs.IsValidMove = IsValidMove;
        dfs.OnSearchPath.AddListener((path) => StartCoroutine(MoveRoutine(path)));
        dfs.Destination = destination.position;

        bfs = GetComponent<BreadthFirstSearch>();
        bfs.IsValidMove = IsValidMove;
        bfs.OnSearchPath.AddListener((path) => StartCoroutine(MoveRoutine(path)));
        bfs.Destination = destination.position;

        aStar = GetComponent<AStar>();
        aStar.IsValidMove = IsValidMove;
        aStar.OnSearchPath.AddListener((path) => StartCoroutine(MoveRoutine(path)));
        aStar.Destination = destination.position;
    }

    private void Start()
    {
        switch (searchMode)
        {
            case SearchMode.DFS:
                dfs.SearchPath(moveDistance);
                break;

            case SearchMode.BFS:
                bfs.SearchPath(moveDistance);
                break;

            case SearchMode.AStar:
                aStar.SearchPath(moveDistance);
                break;
        }
    }

    private IEnumerator MoveRoutine(IEnumerable<PathNode> path)
    {
        foreach (PathNode node in path)
        {
            node.marker.GetComponent<MeshRenderer>().material.color = Color.red;

            yield return null;
        }

        foreach (PathNode node in path)
        {
            transform.position = node.position;

            yield return null;
        }
    }

    private bool IsValidMove(Vector3 pos, Vector3 dir)
    {
        Vector3 normDir = dir.normalized;

        return !Physics.CheckSphere(pos + normDir * moveDistance, moveDistance, obstacleLayer);
    }
}

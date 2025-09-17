using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 너비 우선 탐색(BFS) 기반 경로 탐색 알고리즘
// - 큐를 사용하여 가까운 노드부터 차례로 탐색
// - 최단 경로를 항상 보장 (가중치 동일 가정)
public class BreadthFirstSearch : PathSearch
{
    // BFS용 큐 (경로 후보 노드 저장)
    private readonly Queue<PathNode> queue = new();

    // 경로 탐색 시작 (외부에서 호출)
    // moveDistance: 한 번에 이동할 거리(격자 크기)
    public override void SearchPath(float moveDistance)
    {
        StartCoroutine(SearchRoutine(moveDistance));
    }

    // BFS 탐색 루프 (코루틴)
    // 1. 시작 노드 큐에 삽입 및 방문 처리
    // 2. 큐가 빌 때까지 반복
    //    2-1. 큐에서 노드 꺼냄
    //    2-2. 목적지 도달 시 경로 반환 및 종료
    //    2-3. 8방향 인접 노드에 대해
    //         - 방문하지 않았고 이동 가능하면 큐에 삽입
    //    2-4. 1프레임 대기(yield return null)
    protected override IEnumerator SearchRoutine(float moveDistance)
    {
        // 시작 위치 방문 처리 및 큐에 삽입
        visited.Add(transform.position);
        queue.Enqueue(new() { prevNode = null, position = transform.position });

        // 큐가 빌 때까지 반복
        while (queue.Count > 0)
        {
            PathNode node = queue.Dequeue();
            // 방문 마커 표시(디버깅용)
            node.marker = Instantiate(visitedMark, node.position, Quaternion.identity);

            // 목적지 도달 시 경로 재구성 및 종료
            if (node.position == Destination)
            {
                SetPath(node);
                yield break;
            }

            // 8방향 인접 노드 탐색
            foreach (Vector3 dir in directions)
            {
                Vector3 nextPos = node.position + dir * moveDistance;
                // 방문하지 않았고 이동 가능하면 큐에 삽입
                if (!visited.Contains(nextPos) && IsValidMove(node.position, dir))
                {
                    visited.Add(nextPos);
                    queue.Enqueue(new PathNode { prevNode = node, position = nextPos });
                }
            }

            yield return null;
        }
    }

}

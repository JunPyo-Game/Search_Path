using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 깊이 우선 탐색(DFS) 기반 경로 탐색 알고리즘
// - 스택을 사용하여 한 방향으로 끝까지 파고드는 방식
// - 최단 경로 보장은 안 되지만, 구현이 단순하고 빠름
public class DepthFirstSearch : PathSearch
{
    // DFS용 스택 (경로 후보 노드 저장)
    private readonly Stack<PathNode> stack = new();

    // 경로 탐색 시작 (외부에서 호출)
    // moveDistance: 한 번에 이동할 거리(격자 크기)
    public override void SearchPath(float moveDistance)
    {
        StartCoroutine(SearchRoutine(moveDistance));
    }

    // DFS 탐색 루프 (코루틴)
    // 1. 시작 노드 스택에 삽입 및 방문 처리
    // 2. 스택이 빌 때까지 반복
    //    2-1. 스택에서 노드 꺼냄
    //    2-2. 목적지 도달 시 경로 반환 및 종료
    //    2-3. 8방향 인접 노드에 대해
    //         - 방문하지 않았고 이동 가능하면 스택에 삽입
    //    2-4. 1프레임 대기(yield return null)
    protected override IEnumerator SearchRoutine(float moveDistance)
    {
        // 시작 위치 방문 처리 및 스택에 삽입
        visited.Add(transform.position);
        stack.Push(new() { prevNode = null, position = transform.position });

        // 스택이 빌 때까지 반복
        while (stack.TryPop(out PathNode node))
        {
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

                // 방문하지 않았고 이동 가능하면 스택에 삽입
                if (!visited.Contains(nextPos) && IsValidMove(node.position, dir))
                {
                    visited.Add(nextPos);
                    stack.Push(new PathNode { prevNode = node, position = nextPos });
                }
            }

            yield return null;
        }
    }
}

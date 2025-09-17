using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PriorityQueue + ClosedSet 기반의 A* 경로 탐색 알고리즘 구현
// - 중복 방문 방지(ClosedSet)
// - 우선순위 큐로 g+h가 최소인 노드부터 탐색
public class AStar_PriorityQueueWithClosedSet : PathSearch
{
    // A*에서 사용하는 노드 정보 (누적 비용, 휴리스틱, 경로 추적)
    // - g: 시작점~현재까지 누적 이동 비용
    // - h: 목적지까지의 휴리스틱(추정 비용)
    // - prevNode: 경로 재구성용 부모 노드
    public class AStarNode : PathNode
    {
        public float g; // 누적 이동 비용
        public float h; // 목적지까지의 휴리스틱(추정 비용)
    }

    // 대각선 이동 시 곱해주는 비용 (루트2)
    static private readonly float DIAGONAL_COST_MULTIPLIER = Mathf.Sqrt(2);
    // g+h가 최소인 노드를 먼저 꺼내는 우선순위 큐
    private readonly PriorityQueue<AStarNode> queue = new();

    // 경로 탐색 시작 (외부에서 호출)
    // moveDistance: 한 번에 이동할 거리(격자 크기)
    public override void SearchPath(float moveDistance)
    {
        StartCoroutine(SearchRoutine(moveDistance)); // 비동기 탐색 루프 시작
    }

    // 목적지까지의 휴리스틱(거리) 계산
    // - 유클리드 거리(Vector3.Distance) 사용
    private float CalcDistToDest(Vector3 position)
    {
        return Vector3.Distance(position, Destination);
    }

    // 실제 A* 탐색 루프 (코루틴)
    // 1. 시작 노드 초기화 및 큐에 삽입
    // 2. 큐가 빌 때까지 반복
    //    2-1. 큐에서 g+h가 최소인 노드 꺼냄
    //    2-2. ClosedSet에 있으면 중복 방문이므로 continue
    //    2-3. 목적지 도달 시 경로 반환 및 종료
    //    2-4. 8방향 인접 노드에 대해
    //         - 이동 가능하면 g/h/prevNode 갱신 및 큐에 삽입
    //         - 더 짧은 경로 발견 시만 갱신
    //    2-5. 1프레임 대기(yield return null)
    // 3. 경로를 찾지 못하면 종료
    protected override IEnumerator SearchRoutine(float moveDistance)
    {
        // 각 위치별로 AStarNode를 1개만 관리 (중복 삽입 방지)
        Dictionary<Vector3, AStarNode> nodeMap = new()
        {
            [transform.position] = new AStarNode
            {
                position = transform.position,
                g = 0.0f,
                h = CalcDistToDest(transform.position),
                prevNode = null
            }
        };

        // 이미 최적 경로가 확정된 위치를 저장하는 집합 (중복 방문 방지)
        HashSet<Vector3> closedSet = new();

        // 탐색한 노드 수 카운트 및 시간 측정용
        int searchCount = 0;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // 시작 노드를 우선순위 큐에 삽입 (g+h가 최소)
        queue.Enqueue(nodeMap[transform.position], nodeMap[transform.position].g + nodeMap[transform.position].h);

        // 큐가 빌 때까지 반복
        while (queue.Dequeue(out AStarNode node))
        {
            // 이미 최적 경로가 확정된 위치라면 무시 (중복 방문 방지)
            if (closedSet.Contains(node.position))
                continue;
            closedSet.Add(node.position);

            // 방문 카운트 증가 및 마커 표시(디버깅용)
            searchCount++;
            node.marker = node.marker != null ? node.marker : Instantiate(visitedMark, node.position, Quaternion.identity);

            // 목적지 도달 시 경로 재구성 및 종료
            if (node.position == Destination)
            {
                SetPath(node);
                stopwatch.Stop();
                Debug.Log($"A* (PQ+ClosedSet) 탐색 완료: 탐색한 노드 수 = {searchCount}, 소요 시간 = {stopwatch.ElapsedMilliseconds} ms");
                yield break;
            }

            // 8방향 인접 노드 탐색
            foreach (Vector3 dir in directions)
            {
                Vector3 nextPos = node.position + dir * moveDistance;
                float stepCost = (Mathf.Abs(dir.x) + Mathf.Abs(dir.z) == 2) ? moveDistance * DIAGONAL_COST_MULTIPLIER : moveDistance;
                float tentativeG = node.g + stepCost;
                // 이동 불가(장애물 등) 시 스킵
                if (!IsValidMove(node.position, dir))
                    continue;
                // nodeMap에 없으면 새로 생성, 있으면 g값 비교 후 갱신
                if (!nodeMap.TryGetValue(nextPos, out AStarNode nextNode))
                {
                    nextNode = new AStarNode
                    {
                        position = nextPos,
                        g = tentativeG,
                        h = CalcDistToDest(nextPos),
                        prevNode = node
                    };
                    nodeMap[nextPos] = nextNode;
                    queue.Enqueue(nextNode, nextNode.g + nextNode.h);
                }
                else if (tentativeG < nextNode.g)
                {
                    // 이미 방문한 위치라도 더 짧은 경로라면 갱신
                    nextNode.g = tentativeG;
                    nextNode.prevNode = node;
                    queue.Enqueue(nextNode, nextNode.g + nextNode.h);
                }
            }

            yield return null;
        }
        // 목적지에 도달하지 못한 경우에도 탐색 카운트 및 시간 출력
        stopwatch.Stop();
        Debug.Log($"A* (PQ+ClosedSet) 탐색 종료(경로 없음): 탐색한 노드 수 = {searchCount}, 소요 시간 = {stopwatch.ElapsedMilliseconds} ms");
    }
}

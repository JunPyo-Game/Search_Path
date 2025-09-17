using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A* 경로 탐색 알고리즘 (IndexedPriorityQueue 버전)
public class AStar : PathSearch
{
    public class AStarNode : PathNode
    {
        // A*에서 사용하는 노드 정보 (누적 비용, 휴리스틱, 경로 추적)
        public float g; // 시작점~현재까지 누적 비용
        public float h; // 휴리스틱(목적지까지 추정 비용)
    }

    // 대각선 이동 비용 (루트2)
    static private readonly float DIAGONAL_COST_MULTIPLIER = Mathf.Sqrt(2);

    // 우선순위 큐 (g+h가 최소인 노드를 먼저 탐색)
    private readonly IndexedPriorityQueue<AStarNode> queue = new();

    public override void SearchPath(float moveDistance)
    {
        // 경로 탐색 시작 (외부에서 호출)
        StartCoroutine(SearchRoutine(moveDistance)); // 비동기 탐색 루프 시작
    }

    // 목적지까지의 휴리스틱(거리) 계산 함수
    // 아래 세 가지 방식 중 하나를 선택해서 사용하세요.

    // 1. 맨해튼 거리(Manhattan Distance) - 격자 기반, 상하좌우 이동에 적합
    /*
    private float CalcDistToDest(Vector3 position)
    {
        // 맨해튼 거리: x, y, z축 거리의 합
        return Mathf.Abs(Destination.x - position.x) + Mathf.Abs(Destination.y - position.y) + Mathf.Abs(Destination.z - position.z);
    }
    */

    // 2. 유클리드 거리(Euclidean Distance) - 대각선 이동, 3D 공간에 적합
    /*
    private float CalcDistToDest(Vector3 position)
    {
        // 유클리드 거리: 두 점 사이의 직선 거리(피타고라스)
        return Vector3.Distance(position, Destination);
    }
    */

    // 3. 제곱 거리(Squared Euclidean Distance) - 제곱근 연산 없이 빠른 근사
    private float CalcDistToDest(Vector3 position)
    {
        // 제곱 거리: 실제 거리의 제곱값만 비교 (최적 경로 보장 X, 성능 중시)
        float dx = Destination.x - position.x;
        float dy = Destination.y - position.y;
        float dz = Destination.z - position.z;
        return dx * dx + dy * dy + dz * dz;
    }

    protected override IEnumerator SearchRoutine(float moveDistance)
    {
        // 실제 A* 탐색 루프 (코루틴)
        // 1. 시작 노드 초기화 및 큐에 삽입
        // 2. 큐가 빌 때까지 반복
        //    2-1. 큐에서 g+h가 최소인 노드 꺼냄
        //    2-2. 목적지 도달 시 경로 반환 및 종료
        //    2-3. 8방향 인접 노드에 대해
        //         - 이동 가능하면 g/h/prevNode 갱신 및 큐에 삽입
        //         - 더 짧은 경로 발견 시만 갱신
        //    2-4. 1프레임 대기(yield return null)
        // 3. 경로를 찾지 못하면 종료
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

        // 탐색한 노드 수 카운트 및 시간 측정용
        int searchCount = 0;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        queue.Enqueue(nodeMap[transform.position], nodeMap[transform.position].g + nodeMap[transform.position].h);

        while (queue.Dequeue(out AStarNode node))
        {
            // 방문 카운트 증가
            searchCount++;
            // 방문 마커 표시(디버깅용)
            node.marker = node.marker != null ? node.marker : Instantiate(visitedMark, node.position, Quaternion.identity);

            if (node.position == Destination)
            {
                SetPath(node);
                stopwatch.Stop();
                Debug.Log($"A* 탐색 완료: 탐색한 노드 수 = {searchCount}, 소요 시간 = {stopwatch.ElapsedMilliseconds} ms");
                yield break;
            }

            foreach (Vector3 dir in directions)
            {
                Vector3 nextPos = node.position + dir * moveDistance;
                float stepCost = (Mathf.Abs(dir.x) + Mathf.Abs(dir.z) == 2) ? moveDistance * DIAGONAL_COST_MULTIPLIER : moveDistance;
                float tentativeG = node.g + stepCost;

                if (!IsValidMove(node.position, dir))
                    continue;

                // nodeMap에 없으면 새로 생성, 있으면 g값 비교 후 갱신
                if (!nodeMap.TryGetValue(nextPos, out AStarNode nextNode))
                {
                    // 처음 방문하는 위치: 새 노드 생성 및 등록
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
        Debug.Log($"A* 탐색 종료(경로 없음): 탐색한 노드 수 = {searchCount}, 소요 시간 = {stopwatch.ElapsedMilliseconds} ms");
    }
}

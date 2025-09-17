using System.Collections.Generic;

/// <summary>
/// MinHeap 기반 우선순위 큐 구현 (삽입/삭제 모두 O(log n))
/// </summary>
public class PriorityQueue<T>
{
    private readonly List<(T item, float priority)> list = new();

    /// <summary>
    /// 큐에 저장된 요소 개수
    /// </summary>
    public int Count => list.Count;

    /// <summary>
    /// 우선순위 큐에 요소를 추가합니다. (Heapify Up)
    /// </summary>
    /// <param name="item">추가할 요소</param>
    /// <param name="priority">우선순위(작을수록 먼저 나옴)</param>
    public void Enqueue(T item, float priority = 0.0f)
    {
        list.Add((item, priority));

        int curIdx = list.Count - 1;

        while (curIdx > 0)
        {
            int parentIdx = (curIdx - 1) / 2;

            if (list[curIdx].priority >= list[parentIdx].priority)
                break;

            (list[curIdx], list[parentIdx]) = (list[parentIdx], list[curIdx]);
            curIdx = parentIdx;
        }
    }

    /// <summary>
    /// 우선순위가 가장 낮은(최소) 요소를 꺼냅니다. (Heapify Down)
    /// </summary>
    /// <param name="item">꺼낸 요소 반환(out)</param>
    /// <returns>큐가 비어있으면 false, 아니면 true</returns>
    public bool Dequeue(out T item)
    {
        if (list.Count == 0)
        {
            item = default;

            return false;
        }

        item = list[0].item;
        (list[0], list[^1]) = (list[^1], list[0]);
        list.RemoveAt(list.Count - 1);

        int curIdx = 0;
        int count = list.Count;
        while (true)
        {
            int left = curIdx * 2 + 1;
            int right = curIdx * 2 + 2;
            int smallest = curIdx;

            if (left < count && list[left].priority < list[smallest].priority)
                smallest = left;

            if (right < count && list[right].priority < list[smallest].priority)
                smallest = right;

            if (smallest == curIdx)
                break;

            (list[curIdx], list[smallest]) = (list[smallest], list[curIdx]);
            curIdx = smallest;
        }

        return true;
    }
}

// List Sort를 활용한 우선순위 큐
// public class PriorityQueue<T>
// {
//     private readonly List<(T item, float priority)> heap = new();

//     public int Count => heap.Count;

//     public void Enqueue(T item, float priority = 0.0f)
//     {
//         heap.Add((item, priority));
//         heap.Sort((a, b) => a.priority.CompareTo(b.priority));
//     }

//     public T Dequeue()
//     {
//         (T item, float _) = heap[0];
//         heap.RemoveAt(0);
//         return item;
//     }
// }
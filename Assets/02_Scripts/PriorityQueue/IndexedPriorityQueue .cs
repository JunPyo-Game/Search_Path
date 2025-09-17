using System.Collections.Generic;

public class IndexedPriorityQueue<T>
{
    private readonly List<(T item, float priority)> heap = new();
    private readonly Dictionary<T, int> indexMap = new();

    public int Count => heap.Count;

    public void Enqueue(T item, float priority)
    {
        if (indexMap.ContainsKey(item))
        {
            UpdatePriority(item, priority);

            return;
        }

        heap.Add((item, priority));

        int idx = heap.Count - 1;
        indexMap[item] = idx;
        HeapifyUp(idx);
    }

    public bool Dequeue(out T item)
    {
        if (heap.Count == 0)
        {
            item = default;

            return false;
        }

        item = heap[0].item;
        indexMap.Remove(item);

        if (heap.Count == 1)
        {
            heap.RemoveAt(0);

            return true;
        }

        heap[0] = heap[^1];
        indexMap[heap[0].item] = 0;
        heap.RemoveAt(heap.Count - 1);
        HeapifyDown(0);

        return true;
    }

    private void HeapifyUp(int idx)
    {
        while (idx > 0)
        {
            int parentIdx = (idx - 1) / 2;

            if (heap[idx].priority >= heap[parentIdx].priority)
                break;

            Swap(idx, parentIdx);
            idx = parentIdx;
        }
    }

    private void HeapifyDown(int idx)
    {
        int count = heap.Count;

        while (true)
        {
            int left = idx * 2 + 1;
            int right = idx * 2 + 2;
            int smallest = idx;

            if (left < count && heap[left].priority < heap[smallest].priority)
                smallest = left;

            if (right < count && heap[right].priority < heap[smallest].priority)
                smallest = right;

            if (smallest == idx)
                break;

            Swap(idx, smallest);
            idx = smallest;
        }
    }

    private void UpdatePriority(T item, float newPriority)
    {
        if (!indexMap.TryGetValue(item, out int idx))
            return;

        float oldPriority = heap[idx].priority;
        heap[idx] = (item, newPriority);

        if (newPriority > oldPriority)
            HeapifyUp(idx);
        else
            HeapifyDown(idx);
    }

    private void Swap(int i, int j)
    {
        (heap[i], heap[j]) = (heap[j], heap[i]);
        indexMap[heap[i].item] = i;
        indexMap[heap[j].item] = j;
    }
} 
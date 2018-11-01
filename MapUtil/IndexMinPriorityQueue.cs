using System;

namespace CopperBend.MapUtil
{
    public class IndexMinPriorityQueue<T> where T : IComparable<T>
    {
        private readonly T[] _keys;
        private readonly int _maxSize;
        private readonly int[] _pq;
        private readonly int[] _qp;

        public IndexMinPriorityQueue(int maxSize)
        {
            _maxSize = maxSize;
            Size = 0;
            _keys = new T[_maxSize + 1];
            _pq = new int[_maxSize + 1];
            _qp = new int[_maxSize + 1];
            for (int i = 0; i < _maxSize; i++)
            {
                _qp[i] = -1;
            }
        }

        public int Size { get; private set; }

        public bool IsEmpty()
        {
            return Size == 0;
        }

        public bool Contains(int i)
        {
            return _qp[i] != -1;
        }

        public void Insert(int index, T key)
        {
            Size++;
            _qp[index] = Size;
            _pq[Size] = index;
            _keys[index] = key;
            Swim(Size);
        }

        public int MinIndex()
        {
            return _pq[1];
        }

        public T MinKey()
        {
            return _keys[_pq[1]];
        }

        public int DeleteMin()
        {
            int min = _pq[1];
            Exchange(1, Size--);
            Sink(1);
            _qp[min] = -1;
            _keys[_pq[Size + 1]] = default(T);
            _pq[Size + 1] = -1;
            return min;
        }

        public T KeyAt(int index)
        {
            return _keys[index];
        }

        public void ChangeKey(int index, T key)
        {
            _keys[index] = key;
            Swim(_qp[index]);
            Sink(_qp[index]);
        }

        public void DecreaseKey(int index, T key)
        {
            _keys[index] = key;
            Swim(_qp[index]);
        }

        public void IncreaseKey(int index, T key)
        {
            _keys[index] = key;
            Sink(_qp[index]);
        }

        public void Delete(int index)
        {
            int i = _qp[index];
            Exchange(i, Size--);
            Swim(i);
            Sink(i);
            _keys[index] = default(T);
            _qp[index] = -1;
        }

        private bool Greater(int i, int j)
        {
            return _keys[_pq[i]].CompareTo(_keys[_pq[j]]) > 0;
        }

        private void Exchange(int i, int j)
        {
            int swap = _pq[i];
            _pq[i] = _pq[j];
            _pq[j] = swap;
            _qp[_pq[i]] = i;
            _qp[_pq[j]] = j;
        }

        private void Swim(int k)
        {
            while (k > 1 && Greater(k / 2, k))
            {
                Exchange(k, k / 2);
                k = k / 2;
            }
        }

        private void Sink(int k)
        {
            while (2 * k <= Size)
            {
                int j = 2 * k;
                if (j < Size && Greater(j, j + 1))
                {
                    j++;
                }
                if (!Greater(k, j))
                {
                    break;
                }
                Exchange(k, j);
                k = j;
            }
        }
    }
}


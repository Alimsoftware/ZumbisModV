using System;
using System.Collections;
using System.Collections.Generic;
using GTA.Math;

namespace ZumbisModV.DataClasses
{
    public class SpawnBlocker
        : IList<Vector3>,
            ICollection<Vector3>,
            IEnumerable<Vector3>,
            IEnumerable
    {
        private readonly List<Vector3> _blockers = new List<Vector3>();

        public int Count => _blockers.Count;

        public bool IsReadOnly => ((ICollection<Vector3>)_blockers).IsReadOnly;

        public IEnumerator<Vector3> GetEnumerator()
        {
            return _blockers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_blockers).GetEnumerator();

        public void Add(Vector3 item) => _blockers.Add(item);

        public void Clear() => _blockers.Clear();

        public bool Contains(Vector3 item) => _blockers.Contains(item);

        public void CopyTo(Vector3[] array, int arrayIndex) => _blockers.CopyTo(array, arrayIndex);

        public bool Remove(Vector3 item) => _blockers.Remove(item);

        public int IndexOf(Vector3 item) => _blockers.IndexOf(item);

        public void Insert(int index, Vector3 item) => _blockers.Insert(index, item);

        public void RemoveAt(int index) => _blockers.RemoveAt(index);

        public Vector3 this[int index]
        {
            get => _blockers[index];
            set => _blockers[index] = value;
        }

        public int FindIndex(Predicate<Vector3> match)
        {
            if (match == null)
                return -1;
            for (int index = 0; index < Count; index++)
            {
                if (match(this[index]))
                    return index;
            }
            return -1;
        }
    }
}

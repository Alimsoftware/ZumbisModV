using System;
using System.Collections;
using System.Collections.Generic;

namespace ZumbisModV
{
    [Serializable]
    public class PedCollection
        : IList<PedData>,
            ICollection<PedData>,
            IEnumerable<PedData>,
            IEnumerable
    {
        private readonly List<PedData> _peds;
        public PedData this[int index]
        {
            get => _peds[index];
            set => _peds[index] = value;
        }

        [field: NonSerialized]
        public event ListChangedEvent ListChanged;

        public PedCollection() => _peds = new List<PedData>();

        public int Count => _peds.Count;

        public bool IsReadOnly => ((ICollection<PedData>)_peds).IsReadOnly;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<PedData> GetEnumerator()
        {
            return _peds.GetEnumerator();
        }

        public void Add(PedData item)
        {
            _peds.Add(item);
            ListChanged?.Invoke(this);
        }

        public void Clear()
        {
            _peds.Clear();
            ListChanged?.Invoke(this);
        }

        public bool Contains(PedData item) => _peds.Contains(item);

        public void CopyTo(PedData[] array, int arrayIndex) => _peds.CopyTo(array, arrayIndex);

        public int IndexOf(PedData item) => _peds.IndexOf(item);

        public void Insert(int index, PedData item) => _peds.Insert(index, item);

        public void RemoveAt(int index) => _peds.RemoveAt(index);

        public bool Remove(PedData item)
        {
            bool flag = _peds.Remove(item);
            ListChanged?.Invoke(this);
            return flag;
        }

        public delegate void ListChangedEvent(PedCollection sender);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GTA;

namespace ZumbisModV
{
    [Serializable]
    public class Map : ICollection<MapProp>, IEnumerable<MapProp>, IEnumerable
    {
        public List<MapProp> Props;

        [field: NonSerialized]
        public event OnListChangedEvent ListChanged;

        public Map()
        {
            Props = new List<MapProp>();
            IsReadOnly = false;
        }

        public int Count => Props.Count;

        public bool IsReadOnly { get; }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<MapProp> GetEnumerator()
        {
            return Props.GetEnumerator();
        }

        public void Add(MapProp item)
        {
            Props.Add(item);
            ListChanged?.Invoke(Props.Count);
        }

        public void Clear()
        {
            foreach (var prop in Props)
            {
                prop.Delete();
            }
            Props.Clear();
        }

        public bool Contains(MapProp item) => Props.Contains(item);

        public void CopyTo(MapProp[] array, int arrayIndex) => Props.CopyTo(array, arrayIndex);

        public bool Remove(MapProp item)
        {
            if (!Props.Remove(item))
                return false;

            ListChanged?.Invoke(Props.Count);
            return true;
        }

        public bool Contains(Prop prop)
        {
            return Props.Any(m => m.Handle == prop.Handle);
        }

        public void NotifyListChanged()
        {
            ListChanged?.Invoke(Count);
        }

        public delegate void OnListChangedEvent(int count);
    }
}

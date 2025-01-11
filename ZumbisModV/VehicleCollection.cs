using System;
using System.Collections;
using System.Collections.Generic;

namespace ZumbisModV
{
    [Serializable]
    public class VehicleCollection
        : IList<VehicleData>,
            ICollection<VehicleData>,
            IEnumerable<VehicleData>,
            IEnumerable
    {
        public int IndexOf(VehicleData item) => _vehicles.IndexOf(item);

        public void Insert(int index, VehicleData item) => _vehicles.Insert(index, item);

        public void RemoveAt(int index) => _vehicles.RemoveAt(index);

        private readonly List<VehicleData> _vehicles;

        public VehicleData this[int index]
        {
            get => _vehicles[index];
            set => _vehicles[index] = value;
        }
        public delegate void ListChangedEvent(VehicleCollection sender);

        [field: NonSerialized]
        public event ListChangedEvent ListChanged;

        public VehicleCollection() => _vehicles = new List<VehicleData>();

        public int Count => _vehicles.Count;

        //public bool IsReadOnly => ((ICollection<VehicleData>)_vehicles).IsReadOnly;
        public bool IsReadOnly => (_vehicles as ICollection<VehicleData>)?.IsReadOnly ?? false;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<VehicleData> GetEnumerator()
        {
            return _vehicles.GetEnumerator();
        }

        public void Add(VehicleData item)
        {
            _vehicles.Add(item);
            ListChanged?.Invoke(this);
        }

        public void Clear()
        {
            _vehicles.Clear();
            ListChanged?.Invoke(this);
        }

        public bool Contains(VehicleData item) => _vehicles.Contains(item);

        public void CopyTo(VehicleData[] array, int arrayIndex)
        {
            _vehicles.CopyTo(array, arrayIndex);
        }

        public bool Remove(VehicleData item)
        {
            bool flag = _vehicles.Remove(item);
            ListChanged?.Invoke(this);
            return flag;
        }
    }
}

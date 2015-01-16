using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CraigerEightOhEighter.Models
{
    [DataContract]
    public class TrackCollection<T> : ICollection<T> where T: Track
    {
        public TrackCollection()
        {
            InnerList = new List<Track>();
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new TrackEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TrackEnumerator<T>(this);
        }

        public void Add(T item)
        {
            InnerList.Add(item);
        }

        public void Clear()
        {
            InnerList.Clear();
        }

        public bool Contains(T item)
        {
            return InnerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            array.CopyTo(InnerList.ToArray(),arrayIndex);
        }

        public bool Remove(T item)
        {
            var result = false;
            //loop through the inner array's indices
            for (var i = 0; i < InnerList.Count; i++)
            {
                //store current index being checked
                var obj = (T)InnerList[i];

                //compare the BusinessObjectBase UniqueId property
                if (obj.Name == item.Name)
                {
                    //remove item from inner ArrayList at index i
                    InnerList.RemoveAt(i);
                    result = true;
                    break;
                }
            }

            return result;

        }

        public virtual T this[int index]
        {
            get
            {
                return (T)InnerList[index];
            }
            set
            {
                InnerList[index] = value;
            }
        }
        

        protected List<Track> InnerList;

        public virtual int Count
        {
            get { return InnerList.Count; }
        }
        public bool IsReadOnly { get; private set; }
    }
}
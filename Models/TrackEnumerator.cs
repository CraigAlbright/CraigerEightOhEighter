using System.Collections;
using System.Collections.Generic;

namespace CraigerEightOhEighter.Models
{
    public class TrackEnumerator<T> : IEnumerator<T> where T : Track
    {

        protected TrackCollection<T> Collection; //enumerated collection
        protected int Index; //current index
        protected T current; //current enumerated object in the collection
        public TrackEnumerator(TrackCollection<T> trackCollection)
        {
            Collection = trackCollection;
            Index = -1;
            current = default(T);
        }

        public void Dispose()
        {
            Collection = null;
            current = default(T);
            Index = -1;
        }

        public bool MoveNext()
        {
            //make sure we are within the bounds of the collection
            if (++Index >= Collection.Count)
            {
                //if not return false
                return false;
            }
            //if we are, then set the current element
            //to the next object in the collection
            current = (T) Collection[Index];
            //return true
            return true;
        }

        public void Reset()
        {
            current = default(T); //reset current object
            Index = -1;
        }

        public T Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
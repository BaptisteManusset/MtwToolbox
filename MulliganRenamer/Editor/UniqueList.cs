/* MIT License

Copyright (c) 2016 Edward Rowe, RedBlueGames

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace RedBlueGames.MulliganRenamer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// UniqueList enforces all elements in the list are unique, allowing for constant time Contains
    /// via a hashset.
    /// </summary>
    public class UniqueList<T> : IList<T>
    {
        private List<T> objects;
        private HashSet<T> cachedObjects;

        public UniqueList()
        {
            objects = new List<T>();
            cachedObjects = new HashSet<T>();
        }

        public T this[int index]
        {
            get
            {
                return objects[index];
            }
            set
            {
                if (cachedObjects.Contains(value))
                {
                    var exceptionMessage = string.Format(
                        "Tried to add a repeat of an item to UniqueList. Item: {0}", value);
                    throw new InvalidOperationException(exceptionMessage);
                }

                // if we are replacing an element in the list, we need to replace it in the cache as well
                var existingObject = objects[index];
                if (typeof(T).IsValueType || existingObject != null)
                {
                    cachedObjects.Remove(existingObject);
                }

                objects[index] = value;
                cachedObjects.Add(value);
            }
        }

        public int Count
        {
            get
            {
                return objects.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(T item)
        {
            if (cachedObjects.Contains(item))
            {
                var exceptionMessage = string.Format(
                    "Tried to add a repeat of an item to UniqueList. Item: {0}", item);
                throw new InvalidOperationException(exceptionMessage);
            }

            objects.Add(item);
            cachedObjects.Add(item);
        }

        public void Clear()
        {
            objects.Clear();
            cachedObjects.Clear();
        }

        public bool Contains(T item)
        {
            return cachedObjects.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            objects.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return objects.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return objects.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (cachedObjects.Contains(item))
            {
                var exceptionMessage = string.Format(
                    "Tried to insert a repeat of an item to UniqueList. Item: {0}", item);
                throw new InvalidOperationException(exceptionMessage);
            }

            objects.Insert(index, item);
            cachedObjects.Add(item);
        }

        public bool Remove(T item)
        {
            cachedObjects.Remove(item);
            return objects.Remove(item);
        }

        public void RemoveAt(int index)
        {
            var objectToRemove = objects[index];
            cachedObjects.Remove(objectToRemove);

            objects.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return objects.GetEnumerator();
        }

        public void RemoveNullObjects()
        {
            // There are never nulls if T is a value type.
            if (typeof(T).IsValueType)
            {
                return;
            }

            objects.RemoveNullObjects();
            cachedObjects.RemoveWhere(item => item == null || item.Equals(null));
        }

        public List<T> ToList()
        {
            // Return a copy so that users can't manipulate the order or contents of the internal list
            var copy = new List<T>();
            copy.AddRange(objects);
            return copy;
        }

        public void AddRange(List<T> collection)
        {
            foreach (var obj in collection)
            {
                Add(obj);
            }
        }
    }
}

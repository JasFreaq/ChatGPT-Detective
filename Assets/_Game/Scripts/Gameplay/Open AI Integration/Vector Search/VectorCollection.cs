using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    public class VectorCollection<T> where T : IVectorObject
    {
        private readonly int _dimensions;
        private List<T> _objects = new List<T>();

        public VectorCollection(int dimensions)
        {
            _dimensions = dimensions;
        }

        public int Dimensions => _dimensions;

        public void Add(T obj)
        {
            _objects.Add(obj);
        }

        public void AddRange(IEnumerable<T> objects)
        {
            _objects.AddRange(objects);
        }

        public IVectorObject GetItem(int index)
        {
            return _objects[index];
        }

        private T FindNearest(float[] query)
        {
            float maxDotProduct = 0;
            int bestIndex = 0;

            for (int i = 0; i < _objects.Count; i++)
            {
                float dotProd = VectorMath.DotProduct(_objects[i].GetVector(), query);

                if (dotProd > maxDotProduct)
                {
                    maxDotProduct = dotProd;
                    bestIndex = i;
                }
            }

            return _objects[bestIndex];
        }
        
        public List<T> FindNearest(float[] query, int resultCount)
        {
            List<T> results = new List<T>();

            for (int i = 0; i < resultCount; i++)
            {
                results.Add(FindNearest(query));
            }

            return results;
        }
    }
}
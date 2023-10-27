using System.Collections.Generic;

namespace ChatGPT_Detective
{
    public class VectorCollection<T> where T : IVectorObject
    {
        private List<T> m_objects = new List<T>();

        public int Count
        {
            get { return m_objects.Count; }
        }
        
        public void Add(T obj)
        {
            m_objects.Add(obj);
        }

        public void AddRange(IEnumerable<T> objects)
        {
            m_objects.AddRange(objects);
        }

        public IVectorObject GetItem(int index)
        {
            return m_objects[index];
        }

        private T FindNearest(double[] query)
        {
            double maxDotProduct = 0;
            int bestIndex = 0;

            for (int i = 0; i < m_objects.Count; i++)
            {
                double dotProd = VectorMath.DotProduct(m_objects[i].GetVector(), query);

                if (dotProd > maxDotProduct)
                {
                    maxDotProduct = dotProd;
                    bestIndex = i;
                }
            }

            return m_objects[bestIndex];
        }

        public List<T> FindNearest(double[] query, int resultCount)
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
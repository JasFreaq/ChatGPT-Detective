using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    public static class VectorMath 
    {
        public static float[] NormalizeVector(float[] vector)
        {
            float sum = 0;

            foreach (float v in vector)
            {
                sum += v * v;
            }
            
            float length = Mathf.Sqrt(sum);

            for (int i = 0, l = vector.Length; i < l; i++)
            {
                vector[i] /= length;
            }

            return vector;
        }

        public static float DotProduct(float[] a, float[] b)
        {
            float sum = 0;

            for (int i = 0, l = a.Length; i < l; i++)
            {
                sum += a[i] * b[i];
            }

            return sum;
        }
    }
}
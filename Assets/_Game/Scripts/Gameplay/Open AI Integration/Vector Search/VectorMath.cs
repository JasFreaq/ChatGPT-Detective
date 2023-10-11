using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    public static class VectorMath 
    {
        public static double[] NormalizeVector(double[] vector)
        {
            double sum = 0;

            foreach (double v in vector)
            {
                sum += v * v;
            }
            
            double length = Math.Sqrt(sum);

            for (int i = 0, l = vector.Length; i < l; i++)
            {
                vector[i] /= length;
            }

            return vector;
        }

        public static double DotProduct(double[] a, double[] b)
        {
            double sum = 0;

            for (int i = 0, l = a.Length; i < l; i++)
            {
                sum += a[i] * b[i];
            }

            return sum;
        }
    }
}
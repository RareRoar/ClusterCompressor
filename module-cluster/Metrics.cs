using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMeansPP
{
    public delegate double MetricFunc(IVector vector1, IVector vector2);

    public static class Metrics
    {     
        public static MetricFunc Euclidian { get => EuclidianMetric; }
        public static MetricFunc Manhattan { get => ManhattanMetric; }
        public static MetricFunc Chebyshev { get => ChebyshevMetric; }

        private static double EuclidianMetric(IVector vector1, IVector vector2)
        {
            if (vector1.Dimension != vector2.Dimension)
            {
                throw new ArgumentException("Vector dimensions are not equal.");
            }

            double squaresSum = 0.0;
            object lockObject = new object();
            List<int> indexList = new List<int>();
            int simdVectorSize = Vector<double>.Count;
            var accumVector = Vector<double>.Zero;
            int i = 0;
            for (; i < vector1.Dimension - simdVectorSize; i += simdVectorSize)
            {
                indexList.Add(i);
            }

            Parallel.ForEach(indexList, (index) => 
            {
                var sliceVector1 = new Vector<double>(vector1.CoordinatesArray, index);
                var sliceVector2 = new Vector<double>(vector2.CoordinatesArray, index);
                var diffVector = Vector.Subtract(sliceVector1, sliceVector2);
                var squaredDiffVector = Vector.Multiply(diffVector, diffVector);

                lock (lockObject)
                {
                    accumVector = Vector.Add(accumVector, squaredDiffVector);
                }
            });

            Parallel.For(i, vector1.Dimension,
                () => 0.0,
                (j, localState, partialSum) =>
                {
                    double diff = vector1[j] - vector2[j];
                    return diff * diff + partialSum;
                },
                localPartialSum =>
                {
                    squaresSum += localPartialSum;
                });

            return Math.Sqrt(squaresSum);
        }
        private static double ManhattanMetric(IVector vector1, IVector vector2)
        {
            if (vector1.Dimension != vector2.Dimension)
            {
                throw new ArgumentException("Vector dimensions are not equal.");
            }

            object lockObject = new object();
            List<int> indexList = new List<int>();
            int simdVectorSize = Vector<double>.Count;
            var accumVector = Vector<double>.Zero;
            int i = 0;
            for (; i < vector1.Dimension - simdVectorSize; i += simdVectorSize)
            {
                indexList.Add(i);
            }
            
            Parallel.ForEach(indexList, (index) =>
            {
                var sliceVector1 = new Vector<double>(vector1.CoordinatesArray, index);
                var sliceVector2 = new Vector<double>(vector2.CoordinatesArray, index);
                var diffVector = Vector.Abs(Vector.Subtract(sliceVector1, sliceVector2));

                lock (lockObject)
                {
                    accumVector = Vector.Add(accumVector, diffVector);
                }
            });

            double absSum = Vector.Dot(accumVector, Vector<double>.One);
            Parallel.For(i, vector1.Dimension,
                () => 0.0,
                (j, localState, partialSum) =>
                {
                    double diff = Math.Abs(vector1[j] - vector2[j]);
                    return diff + partialSum;
                },
                localPartialSum =>
                {
                    lock (lockObject)
                    {
                        absSum += localPartialSum;
                    }
                });

            return absSum;
        }
        private static double ChebyshevMetric(IVector vector1, IVector vector2)
        {
            if (vector1.Dimension != vector2.Dimension)
            {
                throw new ArgumentException("Vector dimensions are not equal.");
            }

            object lockObject = new object();
            List<int> indexList = new List<int>();
            int simdVectorSize = Vector<double>.Count;
            var maxVector = new Vector<double>(double.MinValue);
            int i = 0;
            for (; i < vector1.Dimension - simdVectorSize; i += simdVectorSize)
            {
                indexList.Add(i);
            }

            Parallel.ForEach(indexList, (index) =>
            {
                var sliceVector1 = new Vector<double>(vector1.CoordinatesArray, i);
                var sliceVector2 = new Vector<double>(vector2.CoordinatesArray, i);
                var diffVector = Vector.Abs(Vector.Subtract(sliceVector1, sliceVector2));
                lock (lockObject)
                {
                    maxVector = Vector.Max(maxVector, diffVector);
                }
            });

            double result = double.MinValue;
            for (int j = 0; j < simdVectorSize; j++)
            {
                result = Math.Max(result, maxVector[j]);
            }
            Parallel.For(i, vector1.Dimension, j =>
            {
                double diff = Math.Abs(vector1[j] - vector2[j]);
                lock (lockObject)
                {
                    result = Math.Max(result, diff);
                }
            });

            return result;
        }
    }
}

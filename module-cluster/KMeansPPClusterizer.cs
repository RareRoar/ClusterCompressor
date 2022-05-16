using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMeansPP
{
    public class KMeansPPClusterizer<T> where T : IVector, IClusterable<IVector>, new()
    {
        private readonly MetricFunc metricFunc_;
        private readonly T[] vectors_;
        private readonly Tuple<double, double>[] dimensionIntervals_;
        private readonly int dimensions_;
        private readonly int maxIterations_;

        private T[] centroids_;
        private int centroidCount_;

        private Dictionary<IVector, List<IVector>> clusterDict_ = null;

        public KMeansPPClusterizer(int centroidCount, IEnumerable<T> vectors, MetricFunc metricFunc,
            int maxIterations = 1000)
        {
            if (centroidCount <= 0)
            {
                throw new ArgumentException("Centroid/cluster count can only be positive.");
            }
            if (maxIterations <= 0)
            {
                throw new ArgumentException("Maximum iterations count can only be positive.");
            }

            if (vectors.Count() < centroidCount)
            {
                throw new ArgumentException("Vector collection size must be bigger than centroids count.");
            }

            centroidCount_ = centroidCount;
            maxIterations_ = maxIterations;
            metricFunc_ = metricFunc;

            dimensions_ = vectors.First().Dimension;
            dimensionIntervals_ = new Tuple<double, double>[dimensions_];

            vectors_ = new T[vectors.Count()];
            int index = 0;
            object lockObject = new object();
            Parallel.ForEach(vectors, vector =>
            {
                Parallel.For(0, vector.Dimension, i => 
                { 
                    if (index == 0)
                    {
                        dimensionIntervals_[i] = new Tuple<double, double>(vector[i], vector[i]);
                    }
                    else
                    {
                        double newMin = Math.Min(dimensionIntervals_[i].Item1, vector[i]);
                        double newMax = Math.Max(dimensionIntervals_[i].Item2, vector[i]);
                        dimensionIntervals_[i] = new Tuple<double, double>(newMin, newMax);
                    }
                });
                lock(lockObject)
                {
                    vectors_[index++] = vector;
                }
            });

            InitializeCentroids();
        }

        private void InitializeCentroids()
        {
            centroids_ = new T[centroidCount_];
            Parallel.For(0, centroidCount_, i => 
            {
                centroids_[i] = GetNewCentroid();
            });
        }

        private T GetFirstCentroid()
        {
            if (centroidCount_ > 1)
            {
                Random rng = new Random(new Random().Next());

                double[] centroidCoordinates = new double[dimensions_];
                Parallel.For(0, dimensions_, i =>
                {
                    double scaleCoeff = dimensionIntervals_[i].Item2 - dimensionIntervals_[i].Item1;
                    double translationCoeff = dimensionIntervals_[i].Item1;
                    centroidCoordinates[i] = scaleCoeff * rng.NextDouble() + translationCoeff;
                });

                T randomCentroid = new T();
                randomCentroid.UpdateCoordinates(centroidCoordinates);

                Parallel.ForEach(vectors_, (vector) =>
                {
                    vector.ClusterCentroid = randomCentroid;
                });

                return randomCentroid;
            }
            else
            {
                double[] newCentroidCoords = new double[dimensions_];
                int onlyClusterSize = vectors_.Length;
                Parallel.For(0, onlyClusterSize, i => 
                {
                    Parallel.For(0, dimensions_, j => 
                    {
                        newCentroidCoords[j] += vectors_[i][j] / onlyClusterSize;
                    });
                });

                T newCentroid = new T();
                newCentroid.UpdateCoordinates(newCentroidCoords);
                return newCentroid;
            }
        }

        private T GetNewCentroid()
        {
            if (centroids_[0] == null)
            {
                return GetFirstCentroid();
            }

            double[] distancesToClosestCentroid = new double[vectors_.Length];
            double distanceSum = 0.0;
            object lockObject = new object();
            Parallel.For(0, vectors_.Length,
                () => 0.0,
                (i, loopState, partiallDistanceSum) => {
                distancesToClosestCentroid[i] = metricFunc_(vectors_[i], vectors_[i].ClusterCentroid);
                return distancesToClosestCentroid[i];
                },
                localPartialDistanceSum =>
                {
                    lock(lockObject)
                    {
                        distanceSum += localPartialDistanceSum;
                    }
                });

            double newCentroidDeterminant = new Random(new Random().Next()).NextDouble() * distanceSum;
            double accumulator = 0.0;
            int newCentroidIndex = 0;
            while (accumulator < newCentroidDeterminant)
            {
                accumulator += distancesToClosestCentroid[newCentroidIndex++];
            }
            if (newCentroidIndex > 0)
                newCentroidIndex--;
            

            Parallel.ForEach(vectors_, vector => 
            {
                double distanceToOldCentroid = metricFunc_(vector, vector.ClusterCentroid);
                double distanceToNewCentroid = metricFunc_(vector, vectors_[newCentroidIndex]);
                if (distanceToNewCentroid < distanceToOldCentroid)
                {
                    vector.ClusterCentroid = vectors_[newCentroidIndex];
                }
            });

            return vectors_[newCentroidIndex];
        }

        //assing vectors to clusters with closest centroids
        private void UpdateClusters()
        {
            Parallel.For(0, vectors_.Length, i =>
            {
                int closestCentroidIndex = -1;
                double distanceToClosestCentroid = double.MaxValue;
                Parallel.For(0, centroidCount_, j =>
                {
                    double distanceToCurrentCentroid = metricFunc_(vectors_[i], centroids_[j]);
                    if (distanceToCurrentCentroid < distanceToClosestCentroid)
                    {
                        closestCentroidIndex = j;
                        distanceToClosestCentroid = distanceToCurrentCentroid;
                    }
                });
                vectors_[i].ClusterCentroid = centroids_[closestCentroidIndex];
            });
        }

        //reassign centroids and return sum of distances between old and new centroids
        private double UpdateCentroids()
        {
            UpdateClusters();
            
            List<int>[] centroidVectorIndexMapping = new List<int>[centroidCount_];
            Parallel.For(0, centroidCount_, i => 
            {
                centroidVectorIndexMapping[i] = new List<int>();
                for (int j = 0; j < vectors_.Length; j++)
                {
                    if (centroids_[i] as IVector == vectors_[j].ClusterCentroid)
                    {
                        centroidVectorIndexMapping[i].Add(j);
                    }
                }
            });

            double residual = 0.0;
            object lockObject = new object();
            Parallel.For(0, centroidCount_, () => 0.0, (i, loopState, partialResidual) =>
            {
                double[] newCentroidCoordinates = new double[dimensions_];
                int clusterSize = centroidVectorIndexMapping[i].Count();
                foreach (var index in centroidVectorIndexMapping[i])
                {
                    Parallel.For(0, dimensions_, j =>
                    {
                        newCentroidCoordinates[j] += vectors_[index][j] / clusterSize;
                    });
                }

                var newCentroid = new T();
                newCentroid.UpdateCoordinates(newCentroidCoordinates);
                T oldCentroid = centroids_[i];
                centroids_[i] = newCentroid;
                return partialResidual + metricFunc_(newCentroid, oldCentroid);
            }, 
            localPartialResidual => {
                lock (lockObject)
                {
                    residual += localPartialResidual;
                }
            });

            return residual;
        }

        //perform algorithms iterations and return true is it has converged
        private bool DefineClusters()
        {
            double lastResidual = 0.0;
            for (int i = 0; i < maxIterations_; i++)
            {

                //Console.WriteLine(i.ToString() + ")");
                double residual = UpdateCentroids();

                //Console.WriteLine(residual);
                if (i == 0)
                {
                    lastResidual = residual;
                }
                else
                {
                    if (lastResidual == residual && lastResidual < 10)
                    {
                        //residual = 0;
                    }
                    else
                    {
                        lastResidual = residual;
                    }
                }
                if (residual == 0.0)
                {
                    UpdateClusters();
                    return true;
                }
            }
            return false;
        }

        //return dictionary of (cluster centroid, cluster vectors) pairs
        public Dictionary<IVector, List<IVector>> GetClusters()
        {
            //Console.WriteLine("Starting");
            if (DefineClusters())
            {
                var clusterDict = new Dictionary<IVector, List<IVector>>();
                for (int i = 0; i < centroids_.Length; i++)
                {
                    clusterDict[centroids_[i]] = new List<IVector>();
                }
                foreach (var vector in vectors_)
                {
                    clusterDict[vector.ClusterCentroid].Add(vector);
                }
                clusterDict_ = clusterDict;
                return clusterDict;
            }
            else
            {
                return null;
            }
        }

        public double[] GetSilhouetteCoeffs()
        {
            if (clusterDict_ == null)
            {
                if (GetClusters() == null)
                {
                    return null;
                }
            }            

            double[] silhouetteCoeffs = new double[vectors_.Length];
            Parallel.For(0, vectors_.Length, i =>
            {
                double avgIntraclusterDistance = 0.0;
                int clusterSize = clusterDict_[vectors_[i].ClusterCentroid].Count();
                object lockObject1 = new object();

                Parallel.ForEach(clusterDict_[vectors_[i].ClusterCentroid],
                    () => 0.0,
                    (vector, localState, partialResult) =>
                    {
                        return (metricFunc_(vectors_[i], vector) / clusterSize) + partialResult;
                    },
                    (localPartialResult) =>
                    {
                        lock (lockObject1)
                        {
                            avgIntraclusterDistance += localPartialResult;
                        }
                    });

                double avgNeighborClusterDistance = double.MaxValue;
                object lockObject2 = new object();
                Parallel.ForEach(centroids_, centroid =>
                {
                    if (centroid as IVector != vectors_[i].ClusterCentroid)
                    {
                        int currentClusterSize = clusterDict_[centroid].Count();
                        double avgInterclusterDistance = 0.0;
                        Parallel.ForEach(clusterDict_[centroid],
                            () => 0.0,
                            (vector, localState, partialResult) =>
                            {
                                return (metricFunc_(vectors_[i], vector) / currentClusterSize) + partialResult;
                            },
                            (localPartialResult) =>
                            {
                                lock (lockObject2)
                                {
                                    avgInterclusterDistance += localPartialResult;
                                }
                            });
                        lock (lockObject2)
                        {
                            avgNeighborClusterDistance = Math.Min(avgNeighborClusterDistance, avgInterclusterDistance);
                        }
                    }
                });

                silhouetteCoeffs[i] = (avgNeighborClusterDistance - avgIntraclusterDistance);
                silhouetteCoeffs[i] /= Math.Max(avgNeighborClusterDistance, avgIntraclusterDistance);

            });

            return silhouetteCoeffs;
        }

        //return true if centroids have converged and
        //caclulates persentage of negative silhouette coefficients
        public bool PerformSilhouetteAnalysis(out double negSilhouetteCoeffPercent)
        {
            double[] silhouetteCoeffs = GetSilhouetteCoeffs();
            if (silhouetteCoeffs == null)
            {
                negSilhouetteCoeffPercent = -1.0;
                return false;
            }

            double outResult = 0.0;
            object lockObject = new object();
            int coeffCount = silhouetteCoeffs.Length;
            Parallel.For(0, coeffCount,
                () => 0.0,
                (i, localState, partialSum) => {
                    return silhouetteCoeffs[i] / coeffCount + partialSum;
                },
                localPartialSum => { 
                    lock(lockObject)
                    {
                        outResult += localPartialSum;
                    }
                });
            negSilhouetteCoeffPercent = outResult;
            return true;
        }
    }
}
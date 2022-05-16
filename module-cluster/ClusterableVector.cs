using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMeansPP
{
    public class ClusterableVector : IVector, IClusterable<IVector>
    {
        private double[] valueSequence_;

        public IVector ClusterCentroid { get; set; }
        public double[] CoordinatesArray { get => valueSequence_; }

        public ClusterableVector() {
            valueSequence_ = new double[0];
        }

        public ClusterableVector(IEnumerable<double> valueCollection)
        {
            valueSequence_ = valueCollection.ToArray();
        }
        public ClusterableVector(IEnumerable<object> objectCollection)
        {
            valueSequence_ = objectCollection.Select(item => (double)item).ToArray();
        }
        public ClusterableVector(IEnumerable<IConvertible> objectCollection)
        {
            valueSequence_ = objectCollection.Select(item => Convert.ToDouble(item)).ToArray();
        }

        public double this[int dimention]
        {
            get
            {
                if (dimention >= 0 && dimention < valueSequence_.Length)
                {
                    return valueSequence_[dimention];
                }
                throw new IndexOutOfRangeException("Requested coordinate out of vector dimension range.");
            }
        }

        public int Dimension { get => valueSequence_.Length; }

        public int X => throw new NotImplementedException();

        public int Y => throw new NotImplementedException();

        public void UpdateCoordinates(IEnumerable<double> valueCollection)
        {
            valueSequence_ = valueCollection.ToArray();
        }
    }
}

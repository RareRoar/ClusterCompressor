using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMeansPP
{
    public class ByteVector : IVector, IClusterable<IVector>
    {
        private byte[] valueSequence_;
        public int X { get; }
        public int Y { get; }

        public IVector ClusterCentroid { get; set; }
        public double[] CoordinatesArray { get => valueSequence_.Select(el => Convert.ToDouble(el)).ToArray(); }

        public ByteVector()
        {
            valueSequence_ = new byte[0];
        }

        public ByteVector(int x, int y, IEnumerable<byte> valueCollection)
        {
            X = x;
            Y = y;
            valueSequence_ = valueCollection.ToArray();
        }

        public ByteVector(IEnumerable<byte> valueCollection)
        {
            valueSequence_ = valueCollection.ToArray();
        }
        public ByteVector(IEnumerable<object> objectCollection)
        {
            valueSequence_ = objectCollection.Select(item => (byte)item).ToArray();
        }
        public ByteVector(IEnumerable<IConvertible> objectCollection)
        {
            valueSequence_ = objectCollection.Select(item => Convert.ToByte(item)).ToArray();
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

        public void UpdateCoordinates(IEnumerable<double> valueCollection)
        {
            valueSequence_ = new byte[valueCollection.Count()];
            int i = 0;
            foreach (var value in valueCollection)
            {
                valueSequence_[i++] = Convert.ToByte(value);
                
            }
        }
    }
}

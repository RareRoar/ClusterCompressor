using System.Collections.Generic;

namespace KMeansPP
{
    public interface IVector
    {
        void UpdateCoordinates(IEnumerable<double> valueCollection);

        int X { get; }
        int Y { get; }

        int Dimension { get;  }
        double this[int dimention] { get; }
        double[] CoordinatesArray { get; }
    }
}

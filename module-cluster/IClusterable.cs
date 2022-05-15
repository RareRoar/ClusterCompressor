using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMeansPP
{
    public interface IClusterable<T> where T: IVector
    {
        T ClusterCentroid { get; set; }
    }
}

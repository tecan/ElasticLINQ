using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticLinq.Response.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class Total
    {
        /// <summary>
        /// The total number of hits available on the server.
        /// </summary>
        public long value;
        /// <summary>
        ///  Indicates whether the value is accurate (eq) or a lower bound (gte)
        /// </summary>
        public string relation;
    }
}

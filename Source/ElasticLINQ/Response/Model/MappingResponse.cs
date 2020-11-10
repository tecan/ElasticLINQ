using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class MappingResponse
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("_doc")]
        public Doc Doc { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Doc
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("properties")]
        public JObject Properties { get; set; }
    }
}

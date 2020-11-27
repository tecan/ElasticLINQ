using ElasticLinq.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticLinq.Request.Criteria
{
    /// <summary>
    /// Criteria that specifies a wildcard pattern to be passed to Elasticsearch.
    /// </summary>
    public class WildCardCriteria : ICriteria
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WildCardCriteria"/> class.
        /// </summary>
        /// <param name="value">Value to be found within the fields.</param>
        /// <param name="field">Field to be searched.</param>
        public WildCardCriteria(string value, string field)
        {
            Argument.EnsureNotBlank(nameof(value), value);

            Value = value;
            Field = field;
        }

        /// <summary>
        /// Collection of fields to be searched.
        /// </summary>
        public string Field { get; }

        /// <summary>
        /// Value to be found within the fields.
        /// </summary>
        public string Value { get; }

        /// <inheritdoc/>
        public string Name => "wildcard";
    }
}

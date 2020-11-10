// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using System;

namespace ElasticLinq.Mapping
{
    /// <summary>
    /// Trivial mapping implementation that will pluralize type names, camel-case
    /// type names and field names, and lowercase field values.
    /// </summary>
    public class TrivialElasticMapping : ElasticMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrivialElasticMapping"/> class.
        /// </summary>
        public TrivialElasticMapping() : base(false, false, false, false, EnumFormat.String, null) { }
       
        /// <inheritdoc/>
        public override string GetDocumentType(Type type) { return null; }
    }
}
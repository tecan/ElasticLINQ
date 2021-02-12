// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using ElasticLinq.Request.Criteria;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ElasticLinq.Mapping
{
    /// <summary>
    /// Wraps an elastic mapping with one that also handles the built-in
    /// ElasticFields class that contains properties for _score etc.
    /// </summary>
    class ElasticFieldsMappingWrapper : IElasticMapping
    {
        readonly IElasticMapping wrapped;

        /// <inheritdoc/>
        public ElasticFieldsMappingWrapper(IElasticMapping wrapped)
        {
            this.wrapped = wrapped;
        }

        /// <inheritdoc/>
        public JToken FormatValue(MemberInfo member, object value)
        {
            return wrapped.FormatValue(member, value);
        }

        /// <inheritdoc/>
        public string GetIndexType(Type type)
        {
            return wrapped.GetIndexType(type);
        }

        /// <inheritdoc/>
        public ICriteria GetTypeSelectionCriteria(Type type)
        {
            return wrapped.GetTypeSelectionCriteria(type);
        }

        /// <inheritdoc/>
        public object Materialize(JToken sourceDocument, Type objectType)
        {
            return wrapped.Materialize(sourceDocument, objectType);
        }

        /// <inheritdoc/>
        public object Materialize(string id,JToken sourceDocument, Type objectType)
        {
            return wrapped.Materialize(id,sourceDocument, objectType);
        }

        /// <inheritdoc/>
        public string GetElasticFieldType(Type type)
        {
            return wrapped.GetElasticFieldType(type);
        }

        /// <inheritdoc/>
        public IDictionary<string, string> ElasticPropertyMappings()
        {
            return wrapped.ElasticPropertyMappings(); 
        }

        public bool TryGetFieldName(Type type, Expression expression, out string fieldName)
        {
            fieldName = null;
            var isElasticType = expression is MemberExpression memberExpression &&
                                memberExpression.Member.DeclaringType == typeof(ElasticFields);

            var isFieldNameAvailable= isElasticType || wrapped.TryGetFieldName(type, expression, out fieldName);

            fieldName= isElasticType
                    ? "_" + ((MemberExpression)expression).Member.Name.ToLowerInvariant()
                    : fieldName;

            return isFieldNameAvailable;
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace ElasticLinq.Request.Criteria
{
    /// <summary>
    /// Criteria that specifies one possible value that a
    /// field must match in order to select a document.
    /// </summary>
    public class MatchCriteria : SingleFieldCriteria, ITermsCriteria
    {
        readonly ReadOnlyCollection<object> values;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchCriteria"/> class.
        /// </summary>
        /// <param name="field">Field to be checked for this match.</param>
        /// <param name="member">Property or field being checked for this match.</param>
        /// <param name="value">Value to be checked for this match.</param>
        public MatchCriteria(string field, MemberInfo member, object value)
            : base(field)
        {
            Member = member;
            values = new ReadOnlyCollection<object>(new[] { value });
        }

        // "match" is always implicitly combinable by OrCriteria.Combine
        bool ITermsCriteria.IsOrCriteria => true;

        /// <summary>
        /// Property or field being checked for this match.
        /// </summary>
        public MemberInfo Member { get; }

        /// <inheritdoc/>
        public override string Name => "match";

        /// <summary>
        /// Constant value being checked.
        /// </summary>
        public object Value => Convert.ToString(values[0]).ToLower();

        /// <summary>
        /// List of constant values being checked for.
        /// </summary>
        ReadOnlyCollection<object> ITermsCriteria.Values => values;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"match {Field} {Value}";
        }
    }
}
// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using ElasticLinq.Utility;
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ElasticLinq.Request.Visitors
{
    /// <summary>
    /// A pair containing one Expression and one ConstantExpression that might be used
    /// in a test or assignment.
    /// </summary>
    [DebuggerDisplay("{Expression,nq}, {ConstantExpression.Value}")]
    class ConstantMemberPair
    {
        public static ConstantMemberPair Create(Expression a, Expression b)
        {
            if (a is ConstantExpression constantExpressionA && !(b is ConstantExpression ))
                return new ConstantMemberPair(constantExpressionA, b);

            if (b is ConstantExpression constantExpressionB && !(a is ConstantExpression))
                return new ConstantMemberPair(constantExpressionB, a);

            return null;
        }

        public ConstantMemberPair(ConstantExpression constantExpression, Expression expression)
        {
            ConstantExpression = constantExpression;
            Expression = expression;
        }

        public ConstantExpression ConstantExpression { get; }

        public Expression Expression { get; }

        public bool IsNullTest
        {
            get
            {

                var memberExpression = Expression as MemberExpression;
                if (memberExpression == null) return false;
                // someProperty.HasValue
                if (memberExpression?.Member.Name == "HasValue"
                    && ConstantExpression.Type == typeof(bool)
                    && memberExpression.Member.DeclaringType.IsGenericOf(typeof(Nullable<>)))
                    return true;

                // something == null (for reference types or Nullable<T>)
                if (ConstantExpression.Value == null)
                    return memberExpression.Type.IsNullable();

                return false;
            }
        }

        public MemberInfo GetMemberFromExpression()
        {
            return Expression is MemberExpression ? ((MemberExpression) Expression).Member : null;
        }
    }
}

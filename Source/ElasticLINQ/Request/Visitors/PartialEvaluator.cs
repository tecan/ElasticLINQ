﻿// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace ElasticLinq.Request.Visitors
{
    /// <summary>
    /// Determines which part of the tree can be locally
    /// evaluated before execution and substitutes those parts
    /// with constant values obtained from local execution of that part.
    /// </summary>
    internal static class PartialEvaluator
    {
        private static readonly Type[] doNotEvaluateMembersDeclaredOn = { typeof(ElasticFields) };
        private static readonly Type[] doNotEvaluateMethodsDeclaredOn = { typeof(Enumerable), typeof(ElasticQueryExtensions), typeof(Queryable) };

        public static Expression Evaluate(Expression e)
        {
            var chosenForEvaluation = BranchSelectExpressionVisitor.Select(e, ShouldEvaluate);
            return EvaluatingExpressionVisitor.Evaluate(e, chosenForEvaluation);
        }

        internal static bool ShouldEvaluate(Expression e)
        {
            if (e.NodeType == ExpressionType.Parameter || e.NodeType == ExpressionType.Lambda)
                return false;

            if (e is MemberExpression && doNotEvaluateMembersDeclaredOn.Contains(((MemberExpression)e).Member.DeclaringType) ||
               (e is MethodCallExpression && doNotEvaluateMethodsDeclaredOn.Contains(((MethodCallExpression)e).Method.DeclaringType)))
                return false;

            return true;
        }
    }
}
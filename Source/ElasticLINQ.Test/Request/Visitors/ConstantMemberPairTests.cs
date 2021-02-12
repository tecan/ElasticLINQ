﻿// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using ElasticLinq.Request.Visitors;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace ElasticLinq.Test.Request.Visitors
{
    public class ConstantMemberPairTests
    {
        class Sample
        {
            public bool SomeProperty { get; set; }
        }

        static readonly MemberInfo sampleMember = typeof(Sample).GetProperty("SomeProperty", BindingFlags.Instance | BindingFlags.Public);
        readonly ConstantExpression constantExpression = Expression.Constant(true);
        readonly MemberExpression memberExpression = Expression.MakeMemberAccess(Expression.Constant(new Sample()), sampleMember);

        [Fact]
        public void CreateReturnsNullIfParametersAreNull()
        {
            Assert.Null(ConstantMemberPair.Create(null, null));
            Assert.Null(ConstantMemberPair.Create(constantExpression, null));
            Assert.Null(ConstantMemberPair.Create(null, constantExpression));
        }

        [Fact]
        public void CreateReturnsNullIfBothParametersAreConstants()
        {
            var result = ConstantMemberPair.Create(Expression.Constant(1), Expression.Constant(2));

            Assert.Null(result);
        }

        [Fact]
        public void CreateReturnsNullIfBothParametersAreMembers()
        {
            var result = ConstantMemberPair.Create(memberExpression, memberExpression);

            Assert.Null(result);
        }

        [Fact]
        public void CreateReturnsValidPairIfConstantThenMemberOrderedArguments()
        {
            var result = ConstantMemberPair.Create(constantExpression, memberExpression);

            Assert.NotNull(result);
            Assert.Same(constantExpression, result.ConstantExpression);
            Assert.Same(memberExpression, result.Expression);
        }

        [Fact]
        public void CreateReturnsValidPairIfMemberThenConstantOrderedArguments()
        {
            var result = ConstantMemberPair.Create(memberExpression, constantExpression);

            Assert.NotNull(result);
            Assert.Same(constantExpression, result.ConstantExpression);
            Assert.Same(memberExpression, result.Expression);
        }
    }
}
﻿// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using ElasticLinq.Request;
using ElasticLinq.Request.Visitors;
using ElasticLinq.Response.Materializers;
using Xunit;

namespace ElasticLinq.Test.Request.Visitors
{
    public class ElasticTranslateResultTests
    {
        [Fact]
        public void ConstructorSetsProperties()
        {
            var expectedSearch = new SearchRequest { IndexType = "someType" };
            var expectedMaterializer = new ListHitsElasticMaterializer(o => o, typeof(ElasticConnectionTests));

            var result = new ElasticTranslateResult(expectedSearch, expectedMaterializer);

            Assert.Same(expectedSearch, result.SearchRequest);
            Assert.Same(expectedMaterializer, result.Materializer);
        }
    }
}
// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using System;
using System.Linq;
using ElasticLinq.IntegrationTest.Models;
using Xunit;

namespace ElasticLinq.IntegrationTest.WhereTests
{
    public class StringWhereTests
    {
        const string Surname = "Morales";
        const string Surname2nd = "Lewis";

        [Fact]
        public void EqualToConstant()
        {
            DataAssert.Same<WebUser>(q => q.Where(w => w.Surname.Equals(Surname, StringComparison.InvariantCultureIgnoreCase)));
            DataAssert.Same<WebUser>(q => q.Where(w => Surname == w.Surname));
            DataAssert.Same<WebUser>(q => q.Where(w => w.Surname == Surname));
            
            DataAssert.Same<WebUser>(q => q.Where(w => Surname.Equals(w.Surname)));
            DataAssert.Same<WebUser>(q => q.Where(w => Equals(w.Surname, Surname)));
            DataAssert.Same<WebUser>(q => q.Where(w => Equals(Surname, w.Surname)));
        }

        [Fact]
        public void NotEqualToConstant()
        {
            DataAssert.Same<WebUser>(q => q.Where(w => Surname != w.Surname));
            DataAssert.Same<WebUser>(q => q.Where(w => w.Surname != Surname));
            DataAssert.Same<WebUser>(q => q.Where(w => !w.Surname.Equals(Surname)));
            DataAssert.Same<WebUser>(q => q.Where(w => !Surname.Equals(w.Surname)));
            DataAssert.Same<WebUser>(q => q.Where(w => !Equals(w.Surname, Surname)));
            DataAssert.Same<WebUser>(q => q.Where(w => !Equals(Surname, w.Surname)));
        }
    }
}
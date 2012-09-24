﻿/* Copyright 2010-2012 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace MongoDB.BsonUnitTests.Jira
{
    [TestFixture]
    public class CSharp310Tests
    {
        private class C
        {
            public int Id;
            public Guid G = Guid.Empty;
        }

        private class EmptyGuidDefaultValueConvention : IBsonMemberMapConvention
        {
            public string Name
            {
                get { return "EmptyGuidDefaultValue"; }
            }

            public void Apply(BsonMemberMap memberMap)
            {
                if (memberMap.MemberType == typeof(Guid))
                {
                    memberMap.SetDefaultValue(Guid.Empty);
                }
            }
        }

        private class AlwaysIgnoreDefaultValueConvention : IBsonMemberMapConvention
        {
            public string Name
            {
                get { return "AlwaysIngoreDefaultValueConvention"; }
            }

            public void Apply(BsonMemberMap memberMap)
            {
                memberMap.SetIgnoreIfDefault(true);
            }
        }

        private static void InitializeSerialization()
        {
            var conventions = new ConventionPack();
            conventions.Add(new EmptyGuidDefaultValueConvention());
            conventions.Add(new AlwaysIgnoreDefaultValueConvention());
            BsonClassMap.RegisterConventions("CSharp310", conventions, type => type.FullName.StartsWith("MongoDB.BsonUnitTests.Jira.CSharp310Tests", StringComparison.Ordinal));
        }

        [Test]
        public void TestNeverSerializeDefaultValueConvention()
        {
            InitializeSerialization();

            var c = new C { Id = 1 };
            var json = c.ToJson<C>();
            var expected = "{ '_id' : 1 }".Replace("'", "\"");
            Assert.AreEqual(expected, json);

            var bson = c.ToBson<object>();
            var rehydrated = BsonSerializer.Deserialize<C>(bson);
            Assert.IsTrue(bson.SequenceEqual(rehydrated.ToBson<object>()));
        }
    }
}

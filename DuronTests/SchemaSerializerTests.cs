/*
 * Copyright (c) 2010-2012 Achim 'ahzf' Friedland <achim@graph-database.org>
 * This file is part of Duron <http://www.github.com/Vanaheimr/Duron>
 *
 * Licensed under the Affero GPL license, Version 3.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.gnu.org/licenses/agpl.html
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#region Usings

using System;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;
using eu.Vanaheimr.Styx;

#endregion

namespace eu.Vanaheimr.Duron.UnitTests
{

    [TestFixture]
    public class SchemaSerializerTests
    {

        #region SchemaSerializerTestStruct1()

        [Test]
        public void SchemaSerializerTestStruct1()
        {

            var Serializer = new SchemaSerializer<TestDataStruct>();
            Assert.AreEqual("{\r\n\"data1\" : { \"type\" : \"System.Int32\", \"position\": \"0\", \"size\": \"4\" },\r\n\"data2\" : { \"type\" : \"System.Int64\", \"position\": \"5\", \"size\": \"8\" },\r\n\"data3\" : { \"type\" : \"System.String\", \"position\": \"12\", \"size\": \"50\" },\r\n\"data4\" : { \"type\" : \"System.Byte[]\", \"position\": \"62\", \"size\": \"30\" }\r\n}\r\n", Serializer.Schema);
            Assert.AreEqual(96, Serializer.StructSize);

            var x1 = Serializer.Serialize(new TestDataStruct() { data1 = 1024, data2 = -1024 });
            Assert.AreEqual(96, x1.Length);
            Assert.AreEqual("000400000000FCFFFFFFFFFFFF0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", BitConverter.ToString(x1).Replace("-",""));

            var x2 = Serializer.Serialize(new TestDataStruct() { data1 = Int32.MaxValue, data2 = -13 });
            Assert.AreEqual(96, x2.Length);
            Assert.AreEqual("FFFFFF7F00F3FFFFFFFFFFFFFF0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", BitConverter.ToString(x2).Replace("-", ""));

        }

        #endregion


        #region SchemaSerializerTestClass1()

        [Test]
        public void SchemaSerializerTestClass1()
        {

            var Serializer = new SchemaSerializer<TestDataClass>();
            Assert.AreEqual("{\r\n\"data1\" : { \"type\" : \"System.Int32\", \"position\": \"0\", \"size\": \"4\" },\r\n\"data2\" : { \"type\" : \"System.Int64\", \"position\": \"5\", \"size\": \"8\" },\r\n\"data3\" : { \"type\" : \"System.String\", \"position\": \"12\", \"size\": \"50\" },\r\n\"data4\" : { \"type\" : \"System.Byte[]\", \"position\": \"62\", \"size\": \"30\" }\r\n}\r\n", Serializer.Schema);
            Assert.AreEqual(96, Serializer.StructSize);

            var x1 = Serializer.Serialize(new TestDataClass() { data1 = 1024, data2 = -1024 });
            Assert.AreEqual(96, x1.Length);
            Assert.AreEqual("000400000000FCFFFFFFFFFFFF0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", BitConverter.ToString(x1).Replace("-",""));

            var x2 = Serializer.Serialize(new TestDataClass() { data1 = Int32.MaxValue, data2 = -13 });
            Assert.AreEqual(96, x2.Length);
            Assert.AreEqual("FFFFFF7F00F3FFFFFFFFFFFFFF0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", BitConverter.ToString(x2).Replace("-", ""));

        }

        #endregion

    }

}

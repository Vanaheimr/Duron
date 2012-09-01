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

using de.ahzf.Vanaheimr.Duron;
using System.Diagnostics;

#endregion

namespace de.ahzf.Vanaheimr.Duron.UnitTests
{

    public struct TestSubdata
    {
        public Int32  subdata1;
        public UInt64 subdata2;
    }

    public struct TestDataStruct
    {

        private Byte PrivateData;

        public Int32 data1;

        [FixedPosition(5)]
        public Int64 data2;

        // Currently out-of-order :(
        //public myData2 data2;

        [FixedSize(50)]
        public String data3;

        [FixedSize(30)]
        public Byte[] data4;

        [NonSerialized]
        public UInt32 data5;

    }

    public class TestDataClass
    {

        private Byte PrivateData;

        public Int32 data1;

        [FixedPosition(5)]
        public Int64 data2;

        // Currently out-of-order :(
        //public myData2 data2;

        [FixedSize(50)]
        public String data3;

        [FixedSize(30)]
        public Byte[] data4;

        [NonSerialized]
        public UInt32 data5;

    }

}

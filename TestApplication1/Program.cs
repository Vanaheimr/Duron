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

#endregion

namespace TestApplication1
{


    public struct myData2
    {
        public int subdata1;
        public UInt64 subdata2;
    }

    public struct Data
    {

        private byte PrivateData;

        public int data1;

        [FixedPosition(5)]
        public Int64 data2;

        // Currently out-of-order :(
        //public myData2 data2;

        [FixedSize(50)]
        public String data3;

        [FixedSize(30)]
        public Byte[] data4;

        //[NonSerialized]
        //public Complex data5;

    }

    public class Program
    {

        public static void StartMMF()
        {

            var filename = @"sensordata.bin";
            var mmf = new MemoryMappedFile<Data>(filename, 1024);
            mmf.Open();
            //var StructSize = mmf.StructSize;

            //var x1 = mmf.Serialize(new myData() { data1 = 12, data2 = -12 });
            //var x2 = mmf.Serialize(new myData() { data1 = 13, data2 = -13 });

            //using (var view = mmf.CreateViewAccessor(0, 1024, MemoryMappedFileAccess.Write))
            //{
            //    //view.WriteArray(0, whiteRow, 0, whiteRow.Length);
            //}

        }

        public static void StartStructSer()
        {

            var structser = new StructSerializer<Data>();
            var StructSize = structser.StructSize;

            var x1 = structser.Serialize(new Data() { data1 = 12, data2 = -12 });
            var x2 = structser.Serialize(new Data() { data1 = 13, data2 = -13 });

        }

        public static void StartRRD()
        {

            // Get all configuration information from
            // struct attributes and via reflection
            var rrd1 = new RRDBuilder<SensorData>(ReflectStructure: true);

            var rrd2 = new RRDBuilder<SensorData>("my first rrd", TimeSpan.FromSeconds(2), TimeSpan.FromDays(30));

            var rrd3 = new RRDBuilder<SensorData>().SetName("my sec rrd").
                                                    SetStepping_FromSeconds(2).
                                                    SetKeep_FromDays(30);

            //var x1 = mmf.Serialize(new myData() { data1 = 12, data2 = -12 });
            //var x2 = mmf.Serialize(new myData() { data1 = 13, data2 = -13 });

        }


        public static void Main(string[] args)
        {
            StartMMF();
            StartStructSer();
            StartRRD();
        }

    }

}

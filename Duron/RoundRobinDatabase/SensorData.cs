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

#endregion

namespace de.ahzf.Vanaheimr.Duron
{

    /// <summary>
    /// Voltage and current of a reactive power sensor.
    /// </summary>
    [RRDatabase(Name:        "Voltage and current of a reactive power sensor",
                Description: "Sekundenwerte für 14 Tage",
                Interval:   1000, Keep: 1209600, Timeout: 2000)]
    [RRArchive (Aggregate:    60, Keep:   43200, Description: "Minutenwerte für 30 Tage")]
    [RRArchive (Aggregate:   900, Keep:   35040, Description: "15-Minutenwerte für 365 Tage")]
    [RRArchive (Aggregate: 86400, Keep:     365, Description: "Tageswerte für 365 Tage")]
    public struct SensorData
    {

        private byte PrivateData;

        /// <summary>
        /// The identification of the sensor.
        /// </summary>
        [RRSample(RRType.Id), FixedSize(50)]
        public String SensorId;

        /// <summary>
        /// The real timestamp of the measurement.
        /// </summary>
        [RRSample(RRType.Timestamp)]
        public UInt64 Timestamp;

        /// <summary>
        /// The paket counter.
        /// </summary>
        [RRSample(RRType.Counter), NonSerialized]
        public UInt64 PacketCounter;

        /// <summary>
        /// The voltage.
        /// </summary>
        [RRSample(RRType.Absolute, "The voltage", AggregationFunction.AVERAGE)]
        public Double Voltage;

        /// <summary>
        /// The current.
        /// </summary>
        [RRSample(RRType.Absolute, "The current")]
        [RRAggregate(AggregationFunction.AVERAGE)]
        [RRAggregate(AggregationFunction.STDDEV)]
        public Double Current;

        /// <summary>
        /// Non serialized data.
        /// </summary>
        [NonSerialized]
        public String NonSerializedData;

    }

}

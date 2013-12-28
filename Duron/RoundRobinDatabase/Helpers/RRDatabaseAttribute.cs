/*
 * Copyright (c) 2010-2012 Achim 'ahzf' Friedland <achim@graphdefined.org>
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

namespace eu.Vanaheimr.Duron
{

    /// <summary>
    /// Define a new round robin database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class RRDatabaseAttribute : Attribute
    {

        #region Properties

        /// <summary>
        /// The name of the round robin database.
        /// </summary>
        public String Name         { get; private set; }

        /// <summary>
        /// A description of the round robin database.
        /// </summary>
        public String Description  { get; private set; }

        /// <summary>
        /// The timespan in milliseconds between two measurements.
        /// </summary>
        public UInt64 Interval     { get; private set; }

        /// <summary>
        /// The number of measurements to keep within the round robin database.
        /// </summary>
        public UInt64 Keep         { get; private set; }

        /// <summary>
        /// The amount of time in milliseconds a measurement is assumed to be lost.
        /// </summary>
        public UInt64 Timeout      { get; private set; }

        #endregion

        #region Constructor(s)

        #region RRDatabaseAttribute(Name, Description, Interval, Keep, Timeout)

        /// <summary>
        /// Define a new round robin database.
        /// </summary>
        /// <param name="Name">The name of the round robin database.</param>
        /// <param name="Description">A description of the round robin database.</param>
        /// <param name="Interval">The timespan in milliseconds between two measurements.</param>
        /// <param name="Keep">The number of measurements to keep within the round robin database.</param>
        /// <param name="Timeout">The amount of time in milliseconds a measurement is assumed to be lost.</param>
        public RRDatabaseAttribute(String Name,
                                   String Description,
                                   UInt64 Interval,
                                   UInt64 Keep,
                                   UInt64 Timeout)
        {

            #region Initial checks

            if (Name == null || Name.Trim() == String.Empty)
                throw new ArgumentNullException("Name");

            if (Interval == 0)
                throw new ArgumentNullException("Interval");

            if (Keep < 10)
                throw new ArgumentException("Keep");

            if (Timeout <= Interval)
                throw new ArgumentException("Timeout");

            #endregion

            this.Name        = Name;
            this.Description = Description;
            this.Interval    = Interval;
            this.Keep        = Keep;
            this.Timeout     = Timeout;

        }

        #endregion

        #endregion

    }

}

﻿/*
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

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class RRSampleAttribute : Attribute
    {

        public RRType              Type                 { get; private set; }
        public String              Description          { get; private set; }
        public AggregationFunction AggregationFunction  { get; private set; }

        public RRSampleAttribute(RRType Type)
        {
            this.Type                = Type;
            this.Description         = String.Empty;
            this.AggregationFunction = AggregationFunction.AVERAGE;
        }

        public RRSampleAttribute(RRType Type, String Description)
        {
            this.Type                = Type;
            this.Description         = Description;
            this.AggregationFunction = AggregationFunction.AVERAGE;
        }

        public RRSampleAttribute(RRType Type, AggregationFunction AggregationFunction)
        {
            this.Type                = Type;
            this.Description         = String.Empty;
            this.AggregationFunction = AggregationFunction;
        }

        public RRSampleAttribute(RRType Type, String Description, AggregationFunction AggregationFunction)
        {
            this.Type                = Type;
            this.Description         = Description;
            this.AggregationFunction = AggregationFunction;
        }

    }

}
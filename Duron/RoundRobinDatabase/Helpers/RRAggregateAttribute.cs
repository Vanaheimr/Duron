/*
 * Copyright (c) 2010-2014 Achim 'ahzf' Friedland <achim@graphdefined.org>
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

namespace org.GraphDefined.Vanaheimr.Duron
{

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class RRAggregateAttribute : Attribute
    {

        public AggregationFunction AggregationFunction  { get; private set; }
        public String              Description          { get; private set; }

        public RRAggregateAttribute(AggregationFunction AggregationFunction)
        {
            this.AggregationFunction = AggregationFunction;
            this.Description         = String.Empty;
        }

        public RRAggregateAttribute(AggregationFunction AggregationFunction, String Description)
        {
            this.AggregationFunction = AggregationFunction;
            this.Description = Description;
        }

    }

}

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
    /// Defines the position of the serialized value within the serialization byte array.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FixedPositionAttribute : Attribute
    {

        #region Position

        /// <summary>
        /// The position of the serialized value within the serialization byte array.
        /// </summary>
        public UInt32 Position { get; private set; }

        #endregion

        #region FixedPositionAttribute(Position)

        /// <summary>
        /// Defines the position of the serialized value within the serialization byte array.
        /// </summary>
        /// <param name="Position">The position of the serialized value within the serialization byte array.</param>
        public FixedPositionAttribute(UInt32 Position)
        {
            this.Position = Position;
        }

        #endregion

    }

}

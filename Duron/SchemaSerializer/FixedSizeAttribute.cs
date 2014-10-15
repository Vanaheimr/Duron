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

    /// <summary>
    /// Defines the size of the serialized value within the serialization byte array.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FixedSizeAttribute : Attribute
    {

        #region Position

        /// <summary>
        /// The size of the serialized value within the serialization byte array.
        /// </summary>
        public UInt32 Size { get; private set; }

        #endregion

        #region FixedPositionAttribute(Position)

        /// <summary>
        /// Defines the size of the serialized value within the serialization byte array.
        /// </summary>
        /// <param name="Size">The size of the serialized value within the serialization byte array.</param>

        public FixedSizeAttribute(UInt32 Size)
        {
            this.Size = Size;
        }

        #endregion

    }

}

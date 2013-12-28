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
    /// A delegate to serialize a field of the given type.
    /// </summary>
    /// <typeparam name="T">A type.</typeparam>
    /// <param name="Instance">An instance of the type T.</param>
    /// <param name="ByteArray">The byte array for serialization.</param>
    public delegate void FieldSerializer<in T>(T Instance, Byte[] ByteArray);

}

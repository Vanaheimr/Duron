﻿/*
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
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
//using System.Numerics;
using System.Linq.Expressions;
using System.Diagnostics;

using org.GraphDefined.Vanaheimr.Styx;
using org.GraphDefined.Vanaheimr.Styx.Arrows;

#endregion

namespace org.GraphDefined.Vanaheimr.Duron
{

    /// <summary>
    /// Create a serializer for the given struct or class.
    /// </summary>
    /// <typeparam name="T">A struct or class to serialize.</typeparam>
    public class SchemaSerializer<T> : AbstractArrow<T, Byte[]>
    {

        #region Data

        private List<FieldSerializer<T>> FieldSerializers;
        private readonly Byte[] InternalCache;

        #endregion

        #region Properties

        #region Padding

        /// <summary>
        /// The padding used for every serialized block in bytes.
        /// Generally it is usefull to pad everything to 8 byte boundaries.
        /// </summary>
        public UInt32 Padding { get; private set; }

        #endregion

        #region Schema

        private StringBuilder _Schema;

        public String Schema
        {
            get
            {
                return _Schema.ToString();
            }
        }

        #endregion

        #region StructSize

        private UInt32 _StructSize;

        public UInt32 StructSize
        {
            get
            {

                if (_StructSize % Padding == 0)
                    return _StructSize;

                return (_StructSize / Padding + 1) * Padding;

            }
        }

        #endregion

        #endregion

        #region Constructor(s)

        #region SchemaSerializer(Padding = 8, UseReflection = true)

        /// <summary>
        /// Create a serializer for the given struct or class.
        /// </summary>
        /// <param name="Padding">The serialized struct will have a total size equals to a multiply of this value.</param>
        /// <param name="UseReflection">Use reflection to create the serializator.</param>
        public SchemaSerializer(UInt32 Padding = 8, Boolean UseReflection = true)
        {

            this.Padding           = Padding;
            this._Schema           = new StringBuilder();
            this.FieldSerializers  = new List<FieldSerializer<T>>();

            if (UseReflection)
                ReflectStruct(typeof(T));

            InternalCache          = new Byte[StructSize];

        }

        #endregion

        #endregion



        #region (private) CreateFieldSerializer<TSource, TValue>(TypeOfStruct, FieldName, Serializator, Position, Length)

        /// <summary>
        /// Create a delegate to read the given field value from the declaring type and serialize it to the given byte array.</returns>
        /// </summary>
        /// <typeparam name="TSource">The declaring type (the struct or an embedded type).</typeparam>
        /// <typeparam name="TValue">The type of the field to read.</typeparam>
        /// <param name="TypeOfStruct">The type of the field to read.</typeparam>
        /// <param name="FieldName">The name of the field to read.</param>
        /// <param name="Serializator">A delegate to serialize the type of the given field.</param>
        /// <param name="Position">The position within the resulting array of bytes where to start the serialization.</param>
        /// <param name="Length">The number of bytes of the field value to serialize.</param>
        private FieldSerializer<TSource> CreateFieldSerializer<TSource, TValue>(Type                 DeclaringType,
                                                                                String               FieldName,
                                                                                Func<TValue, Byte[]> Serializator,
                                                                                UInt32               Position,
                                                                                UInt32               Length)
        {

            Debug.WriteLine("CreateFieldSerializer(" + FieldName + ")... and not too often...");

            //var GetValueOfField = DeclaringType.GetField(FieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            //return (Instance, Serialized) => Array.Copy(Serializator((TValue) GetValueOfField.GetValue(Instance)), 0, Serialized, Position, Length);

            // Seems to be twice as fast, as it avoid useless castings ;)
            var GetValueOfFieldDelegate = CreateGetValueOfFieldDelegate<TSource, TValue>(DeclaringType.GetField(FieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public));

            return (Instance, Serialized) => Array.Copy(Serializator(GetValueOfFieldDelegate(Instance)), 0, Serialized, Position, Length);

        }

        #endregion

        #region (private) CreateGetValueOfFieldDelegate<TSource, TValue>(FieldInfo)

        /// <summary>
        /// Create a delegate to return the value of a field within a struct.
        /// </summary>
        /// <typeparam name="TSource">The type of the struct.</typeparam>
        /// <typeparam name="TValue">The type of the field to read.</typeparam>
        /// <param name="FieldInfo"></param>
        private Func<TSource, TValue> CreateGetValueOfFieldDelegate<TSource, TValue>(FieldInfo FieldInfo)
        {

            Debug.WriteLine("CreateGetValueOfFieldDelegate(" + FieldInfo.Name + ")... and not too often...");

            var SourceParameterExpression = Expression.Parameter(typeof(TSource), "source");
            var FieldMemberExpression     = Expression.Field(GetCastOrConvertExpression(SourceParameterExpression, FieldInfo.DeclaringType), FieldInfo);
            var LambdaExpression          = Expression.Lambda(typeof(Func<TSource, TValue>),
                                                              this.GetCastOrConvertExpression(FieldMemberExpression, typeof(TValue)),
                                                              SourceParameterExpression);
            
            return (Func<TSource, TValue>) LambdaExpression.Compile();

        }

        #endregion

        #region (private) GetCastOrConvertExpression(expression, targetType)

        private Expression GetCastOrConvertExpression(Expression expression, Type targetType)
        {

            // Check if a cast or conversion is required.
            if (targetType.IsAssignableFrom(expression.Type))
                return expression;

            else
            {

                // Check if we can use the as operator for casting or if we must use the convert method
                if (targetType.IsValueType && !IsNullableType(targetType))
                    return Expression.Convert(expression, targetType);

                else
                    return Expression.TypeAs(expression, targetType);

            }

        }

        #endregion

        #region (private) IsNullableType(type)

        public static Boolean IsNullableType(Type type)
        {

            if (type == null) throw new ArgumentNullException("type");

            bool result = false;

            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                result = true;
            }

            return result;

        }

        #endregion


        private Boolean FieldIs<T2>(Type DeclaringType, FieldInfo FieldInfo, UInt32 Position, Func<T2, Byte[]> Serializer)
        {
            
            if (FieldInfo.FieldType.Equals(typeof(T2)))
                    {

                        _Schema.AppendFieldInfo(FieldInfo, Position, (UInt32) Marshal.SizeOf(FieldInfo.FieldType));

                        FieldSerializers.Add(CreateFieldSerializer<T, T2>(DeclaringType,
                                                                          FieldInfo.Name,
                                                                          Serializer,
                                                                          Position,
                                                                          (UInt32) Marshal.SizeOf(FieldInfo.FieldType)));

                        _StructSize += (UInt32) Marshal.SizeOf(FieldInfo.FieldType);

                        return true;

                    }

            return false;

        }



        #region ReflectStruct(DeclaringType)

        private void ReflectStruct(Type DeclaringType)
        {

            var Position = 0U;

            _Schema.AppendLine("{");

            #region for...

            foreach (var FieldInfo in DeclaringType.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {

                var Attributes = FieldInfo.GetCustomAttributes(false);

                #region Find and process FixedPositionAttribute

                var _FixedPositionAttribute = (from   CustomAttribute
                                               in     Attributes
                                               where  CustomAttribute as FixedPositionAttribute != null
                                               select CustomAttribute as FixedPositionAttribute).FirstOrDefault();

                if (_FixedPositionAttribute != null)
                    Position = _FixedPositionAttribute.Position;
                else
                    Position = _StructSize;

                #endregion

                if (Attributes.Any(a => (a as NonSerializedAttribute) != null))
                { }

                else
                {

                         if (FieldIs<Byte>    (DeclaringType, FieldInfo, Position, value => BitConverter.GetBytes(value))) { }
                    else if (FieldIs<Int16>   (DeclaringType, FieldInfo, Position, value => BitConverter.GetBytes(value))) { }
                    else if (FieldIs<Int32>   (DeclaringType, FieldInfo, Position, value => BitConverter.GetBytes(value))) { }
                    else if (FieldIs<Int64>   (DeclaringType, FieldInfo, Position, value => BitConverter.GetBytes(value))) { }
                    else if (FieldIs<Int64>   (DeclaringType, FieldInfo, Position, value => BitConverter.GetBytes(value))) { }

                    else if (FieldIs<SByte>   (DeclaringType, FieldInfo, Position, value => BitConverter.GetBytes(value))) { }
                    else if (FieldIs<UInt16>  (DeclaringType, FieldInfo, Position, value => BitConverter.GetBytes(value))) { }
                    else if (FieldIs<UInt32>  (DeclaringType, FieldInfo, Position, value => BitConverter.GetBytes(value))) { }
                    else if (FieldIs<UInt64>  (DeclaringType, FieldInfo, Position, value => BitConverter.GetBytes(value))) { }

                    else if (FieldIs<Single>  (DeclaringType, FieldInfo, Position, value => BitConverter.GetBytes(value))) { }
                    else if (FieldIs<Double>  (DeclaringType, FieldInfo, Position, value => BitConverter.GetBytes(value))) { }
                    else if (FieldIs<Char>    (DeclaringType, FieldInfo, Position, value => BitConverter.GetBytes(value))) { }

                    else if (FieldIs<Guid>    (DeclaringType, FieldInfo, Position, value => value.ToByteArray())) { }
                    else if (FieldIs<DateTime>(DeclaringType, FieldInfo, Position, value => BitConverter.GetBytes(value.ToBinary()))) { }
                    else if (FieldIs<TimeSpan>(DeclaringType, FieldInfo, Position, value => BitConverter.GetBytes(value.Ticks))) { }

                    else if (
                             (FieldInfo.FieldType.Equals(typeof(Boolean))) ||
                             (FieldInfo.FieldType.Equals(typeof(Decimal)))                             
                             //(FieldInfo.FieldType.Equals(typeof(Complex)))
                             )
                    {

                        _Schema.AppendFieldInfo(FieldInfo, Position, (UInt32) Marshal.SizeOf(FieldInfo.FieldType));

                    }

                    else if ((FieldInfo.FieldType.Equals(typeof(String))) ||
                             (FieldInfo.FieldType.Equals(typeof(Uri))) ||
                             (FieldInfo.FieldType.Equals(typeof(Byte[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(Int16[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(UInt16[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(Int32[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(UInt32[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(Int64[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(UInt64[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(Boolean[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(Char[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(DateTime[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(Decimal[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(Double[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(Guid[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(SByte[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(Single[]))) ||
                             (FieldInfo.FieldType.Equals(typeof(TimeSpan[])))
                            )
                    {

                        var _FixedSizeAttribute = FieldInfo.GetCustomAttributes(false).
                                                            Select(x => x as FixedSizeAttribute).
                                                            Where(x => x != null).
                                                            First();

                        if (_FixedSizeAttribute != null)
                        {

                            _Schema.AppendFieldInfo(FieldInfo, Position, _FixedSizeAttribute.Size);

                            _StructSize += _FixedSizeAttribute.Size;

                        }

                        else
                            throw new Exception("Serializing the '" + FieldInfo.Name + "' field of type '" + FieldInfo.FieldType.FullName.ToString() + "' demands the 'FixedSize(...)'-attribute!");

                    }

                    else
                    {
                        ReflectStruct(FieldInfo.FieldType);
                    }

                }

            }

            #endregion

            _Schema.Remove(_Schema.Length - 3, 1);
            _Schema.AppendLine("}");

        }

        #endregion



        #region SerializeCached(ValueT)

        public Byte[] SerializeCached(T ValueT, Boolean ClearCache = true)
        {

            if (ClearCache)
                for (var i = InternalCache.Length - 1; i > 0; i--)
                    InternalCache[i] = 0x00;

            foreach (var FieldSerializer in FieldSerializers)
                FieldSerializer(ValueT, InternalCache);

            return InternalCache;

        }

        #endregion

        #region Serialize(ValueT)

        public Byte[] Serialize(T ValueT)
        {

            var ByteArray = new Byte[StructSize];

            foreach (var FieldSerializer in FieldSerializers)
                FieldSerializer(ValueT, ByteArray);

            return ByteArray;

        }

        #endregion

        #region Serialize(ValueT, ByteArray)

        public Byte[] SerializeNew(T ValueT, Byte[] ByteArray)
        {

            if (ByteArray.Length < _StructSize)
                throw new ArgumentException("The given array of bytes is too small!", "ByteArray");

            foreach (var FieldSerializer in FieldSerializers)
                FieldSerializer(ValueT, ByteArray);

            return ByteArray;

        }

        #endregion



        protected override Boolean ProcessMessage(T MessageIn, out Byte[] MessageOut)
        {

            MessageOut = new Byte[StructSize];

            foreach (var FieldSerializer in FieldSerializers)
                FieldSerializer(MessageIn, MessageOut);

            return true;

        }

    }

}

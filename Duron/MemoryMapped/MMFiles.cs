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

#endregion

namespace de.ahzf.Vanaheimr.Duron
{

    public class SharedMemory<T> : IDisposable
        where T : struct
    {

        #region Data

        private MemoryMappedFile mmf;
        private MemoryMappedViewAccessor accessor;

        private UInt64  WindowOffset;
        private UInt32  WindowSize;

        private readonly UInt32 ItemSize;

        private Mutex smLock;
        private Boolean locked;

        private List<Func<T, Byte[]>> _Serializer;

        #endregion

        #region Properties

        /// <summary>
        /// The name of the underlying file.
        /// </summary>
        public String Filename { get; private set; }

        /// <summary>
        /// The padding used for every serialized block in bytes.
        /// Generally it is usefull to pad everything to 8 byte boundaries.
        /// </summary>
        public UInt32 Padding { get; private set; }


        private StringBuilder _Schema;

        public String Schema
        {
            get
            {
                return _Schema.ToString();
            }
        }


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

        #region Constructor(s)

        public SharedMemory(String Filename, UInt32 WindowSize, UInt32 Padding = 8)
        {

            this.Filename   = Filename;
            this.WindowSize = WindowSize;
            this.ItemSize   = 0;
            this.Padding    = Padding;

            _Schema = new StringBuilder();
            _Serializer = new List<Func<T, Byte[]>>();

            ReflectStruct(typeof(T));//, ref ST, ref LT);

        }

        #endregion



        public Func<TSource, Byte[]> GetGetFieldDelegate2<TSource, TValue>(Type fieldDeclaringType, String fieldName, Func<TValue, Byte[]> Func)
        {

            if (fieldName == null) throw new ArgumentNullException("fieldName");
            if (fieldDeclaringType == null) throw new ArgumentNullException("fieldDeclaringType");

            return vv => Func(this.GetGetFieldDelegate<TSource, TValue>(
                                  fieldDeclaringType.GetField(fieldName,
                                                              BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                             )(vv));

        }


        public Func<TSource, TValue> GetGetFieldDelegate<TSource, TValue>(FieldInfo fieldInfo)
        {

            if (fieldInfo == null) throw new ArgumentNullException("fieldInfo");

            var sourceParameterExpression = Expression.Parameter(typeof(TSource), "source");
            var fieldMemberExpression     = Expression.Field(this.GetCastOrConvertExpression(sourceParameterExpression, fieldInfo.DeclaringType), fieldInfo);
            var lambdaExpression          = Expression.Lambda(typeof(Func<TSource, TValue>),
                                                              this.GetCastOrConvertExpression(fieldMemberExpression, typeof(TValue)),
                                                              sourceParameterExpression);
            
            return (Func<TSource, TValue>) lambdaExpression.Compile();

        }

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

        public static bool IsNullableType(Type type)
        {

            if (type == null) throw new ArgumentNullException("type");

            bool result = false;

            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                result = true;
            }

            return result;

        }


        private void ReflectStruct(Type DeclaringType)//, ref StringBuilder _Schema, ref List<Func<T, Byte[]>> _Serializer)
        {

            _Schema.AppendLine("{");

            #region for...

            foreach (var FieldInfo in DeclaringType.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {

                var Attributes = FieldInfo.GetCustomAttributes(false);

                if (Attributes.Any(a => (a as NonSerializedAttribute) != null))
                { }

                else
                {

                    if (FieldInfo.FieldType.Equals(typeof(Int32)))
                    {

                        _Schema.AppendLine("\"" + FieldInfo.Name + "\"" + " : " + "\"" + FieldInfo.FieldType + " of " + Marshal.SizeOf(FieldInfo.FieldType) + " bytes\",");

                        _Serializer.Add(GetGetFieldDelegate2<T, Int32>(DeclaringType,
                                                                       FieldInfo.Name,
                                                                       value => BitConverter.GetBytes(value)));

                        _StructSize += (UInt32) Marshal.SizeOf(FieldInfo.FieldType);

                    }

                    else if (FieldInfo.FieldType.Equals(typeof(Int64)))
                    {

                        _Schema.AppendLine("\"" + FieldInfo.Name + "\"" + " : " + "\"" + FieldInfo.FieldType + " of " + Marshal.SizeOf(FieldInfo.FieldType) + " bytes\",");

                        _Serializer.Add(GetGetFieldDelegate2<T, Int64>(DeclaringType,
                                                                       FieldInfo.Name,
                                                                       value => BitConverter.GetBytes(value)));

                        _StructSize += (UInt32) Marshal.SizeOf(FieldInfo.FieldType);

                    }

                    else if ((FieldInfo.FieldType.Equals(typeof(Byte))) ||
                             (FieldInfo.FieldType.Equals(typeof(Int16))) ||
                             (FieldInfo.FieldType.Equals(typeof(UInt16))) ||
                             (FieldInfo.FieldType.Equals(typeof(UInt32))) ||
                             (FieldInfo.FieldType.Equals(typeof(UInt64))) ||
                             (FieldInfo.FieldType.Equals(typeof(Boolean))) ||
                             (FieldInfo.FieldType.Equals(typeof(Char))) ||
                             (FieldInfo.FieldType.Equals(typeof(DateTime))) ||
                             (FieldInfo.FieldType.Equals(typeof(Decimal))) ||
                             (FieldInfo.FieldType.Equals(typeof(Double))) ||
                             (FieldInfo.FieldType.Equals(typeof(Guid))) ||
                             (FieldInfo.FieldType.Equals(typeof(SByte))) ||
                             (FieldInfo.FieldType.Equals(typeof(Single))) ||
                             (FieldInfo.FieldType.Equals(typeof(TimeSpan)))
                             //(FieldInfo.FieldType.Equals(typeof(Complex)))
                             )
                    {

                        _Schema.AppendLine("\"" + FieldInfo.Name + "\"" + " : " + "\"" + FieldInfo.FieldType + " of " + Marshal.SizeOf(FieldInfo.FieldType) + " bytes\",");

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
                            _Schema.AppendLine("\"" + FieldInfo.Name + "\"" + " : " + "\"" + FieldInfo.FieldType + " of " + _FixedSizeAttribute.Size + " bytes\",");
                        }

                        else
                            throw new Exception("Serializing the '" + FieldInfo.Name + "' field of type '" + FieldInfo.FieldType.FullName.ToString() + "' demands the 'FixedSize(...)'-attribute!");

                    }

                    else
                    {
                        ReflectStruct(FieldInfo.FieldType);//, ref _Schema, ref _Serializer);
                    }

                }

            }

            #endregion

            _Schema.Remove(_Schema.Length - 3, 1);
            _Schema.AppendLine("}");

        }



        public Boolean Open()
        {
            return Open(MemoryMappedFileAccess.ReadWrite);
        }

        public Boolean Open(MemoryMappedFileAccess MemoryMappedFileAccess)
        {

            try
            {

                // Create named MMF
                mmf = MemoryMappedFile.CreateOrOpen(Filename, WindowSize);

                // Create accessors to MMF
                accessor = mmf.CreateViewAccessor(0, WindowSize, MemoryMappedFileAccess);

                // Create lock
                smLock = new Mutex(true, "SM_LOCK", out locked);

            }
            catch
            {
                return false;
            }

            return true;

        }

        public void Close()
        {
            accessor.Dispose();
            mmf.Dispose();
            smLock.Close();
        }

        public T Data
        {

            get
            {
                T dataStruct;
                accessor.Read<T>(0, out dataStruct);
                return dataStruct;
            }

            set
            {
                smLock.WaitOne();
                accessor.Write<T>(0, ref value);
                smLock.ReleaseMutex();
            }

        }



        public Byte[] Serialize(T myT)
        {

            var bb = new Byte[StructSize];
            var i = 0;

            foreach (var bytearray in from func in _Serializer select func(myT))
            {
                Array.Copy(bytearray, 0, bb, i, bytearray.Length);
                i += bytearray.Length;
            }

            return bb;

        }



        public void Dispose()
        {
            mmf.Dispose();
        }

    }

}

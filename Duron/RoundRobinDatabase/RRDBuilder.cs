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

    public class RRDBuilder<T> : IDisposable
        where T : struct
    {

        #region Data
        
        private List<Func<T, Byte[]>> _Serializer;

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

        #region Properties

        #region Name

        private String _Name;

        /// <summary>
        /// The name or identification of the round-robin-database.
        /// </summary>
        public String Name
        {

            get
            {
                return _Name;
            }

            private set
            {

                if (value != null && value.Trim() != String.Empty)
                    _Name = value.Trim();

                else
                    throw new ArgumentNullException("Name", "The value must not be null or empty!");

            }

        }

        #endregion

        #region Description

        private String _Description;

        /// <summary>
        /// The description of the round-robin-database.
        /// </summary>
        public String Description
        {

            get
            {
                return _Description;
            }

            private set
            {

                if (value != null && value.Trim() != String.Empty)
                    _Description = value.Trim();

                else
                    throw new ArgumentNullException("Description", "The value must not be null or empty!");

            }

        }

        #endregion

        #region Interval

        /// <summary>
        /// The amount of time within the round-robin-database expects new values.
        /// </summary>
        public TimeSpan Interval { get; private set; }

        #endregion

        #region Keep

        /// <summary>
        /// The amount of time the round-robin-database should store the primary data.
        /// </summary>
        public UInt64 Keep { get; private set; }

        #endregion

        #region Timeout

        private Nullable<TimeSpan> _Timeout;

        /// <summary>
        /// The amount of time within the round-robin-database expects new values.
        /// </summary>
        public TimeSpan Timeout
        {

            get
            {
                return _Timeout.Value;
            }

            private set
            {
                _Timeout = new Nullable<TimeSpan>(value);
            }

        }

        #endregion

        // DoubleValues

        #region StartingTime

        public DateTime StartingTime { get; private set; }

        #endregion

        #region ForetimeUpdates

        public AllowDeny ForetimeUpdates { get; private set; }

        #endregion

        #endregion

        #region Constructor(s)

        public RRDBuilder()
            : this(false)
        { }

        public RRDBuilder(Boolean ReflectStructure)
        {
            if (ReflectStructure)
            {
                _Schema     = new StringBuilder();
                _Serializer = new List<Func<T, Byte[]>>();
                ReflectStruct();
            }
        }

        public RRDBuilder(String    Name,
                          TimeSpan  Stepping,
                          TimeSpan  KeepTime,
                          AllowDeny ForetimeUpdates = AllowDeny.Deny)
            : this(Name, Stepping, KeepTime, DateTime.Now, ForetimeUpdates)
        { }

        public RRDBuilder(String    Name,
                          TimeSpan  Stepping,
                          TimeSpan  KeepTime,
                          DateTime  StartingTime,
                          AllowDeny ForetimeUpdates = AllowDeny.Deny)
        {
            this.Name            = Name;
            this.StartingTime    = StartingTime;
            this.Interval        = Stepping;
            this.ForetimeUpdates = ForetimeUpdates;
        }

        #endregion


        #region Name

        public RRDBuilder<T> SetName(String Name)
        {
            this.Name = Name;
            return this;
        }

        #endregion

        #region Stepping

        public RRDBuilder<T> SetStepping(TimeSpan Stepping)
        {
            this.Interval = Stepping;
            return this;
        }

        public RRDBuilder<T> SetStepping_FromMilliseconds(UInt64 Milliseconds)
        {
            this.Interval = TimeSpan.FromMilliseconds(Milliseconds);
            return this;
        }

        public RRDBuilder<T> SetStepping_FromSeconds(UInt64 Seconds)
        {
            this.Interval = TimeSpan.FromSeconds(Seconds);
            return this;
        }

        public RRDBuilder<T> SetStepping_FromMinutes(UInt64 Minutes)
        {
            this.Interval = TimeSpan.FromMinutes(Minutes);
            return this;
        }

        public RRDBuilder<T> SetStepping_FromHours(UInt64 Hours)
        {
            this.Interval = TimeSpan.FromHours(Hours);
            return this;
        }

        public RRDBuilder<T> SetStepping_FromDays(UInt64 Days)
        {
            this.Interval = TimeSpan.FromDays(Days);
            return this;
        }

        #endregion

        #region Keep

        public RRDBuilder<T> SetKeep(TimeSpan KeepTime)
        {

            if (KeepTime.TotalMilliseconds == 0)
                throw new ArgumentException();

            if (KeepTime < Interval)
                throw new ArgumentException();

            this.Keep = (UInt64) KeepTime.TotalMilliseconds / (UInt64) Interval.TotalMilliseconds;
            
            return this;

        }

        public RRDBuilder<T> SetKeep_FromMilliseconds(UInt64 Milliseconds)
        {

            if (Milliseconds == 0)
                throw new ArgumentException();

            this.Keep = Milliseconds;
            
            return this;

        }

        public RRDBuilder<T> SetKeep_FromSeconds(UInt64 Seconds)
        {

            if (Seconds == 0)
                throw new ArgumentException();

            this.Keep = 1000 * Seconds;

            return this;

        }

        public RRDBuilder<T> SetKeep_FromMinutes(UInt64 Minutes)
        {

            if (Minutes == 0)
                throw new ArgumentException();

            this.Keep = 60000 * Minutes;

            return this;

        }

        public RRDBuilder<T> SetKeep_FromHours(UInt64 Hours)
        {

            if (Hours == 0)
                throw new ArgumentException();

            this.Keep = 3600000 * Hours;

            return this;

        }

        public RRDBuilder<T> SetKeep_FromDays(UInt64 Days)
        {

            if (Days == 0)
                throw new ArgumentException();

            this.Keep = 86400000 * Days;

            return this;

        }

        #endregion

        // Timeout

        // DoubleValues

        #region StartingTime

        public RRDBuilder<T> SetStartingTime(DateTime StartingTime)
        {
            this.StartingTime = StartingTime;
            return this;
        }

        public RRDBuilder<T> StartNow()
        {
            this.StartingTime = DateTime.Now;
            return this;
        }

        #endregion

        #region ForetimeUpdates

        public RRDBuilder<T> SetForetimeUpdates(AllowDeny ForetimeUpdates)
        {
            this.ForetimeUpdates = ForetimeUpdates;
            return this;
        }

        public RRDBuilder<T> AllowForetimeUpdates()
        {
            this.ForetimeUpdates = AllowDeny.Allow;
            return this;
        }

        public RRDBuilder<T> DenyForetimeUpdates()
        {
            this.ForetimeUpdates = AllowDeny.Deny;
            return this;
        }

        #endregion


        #region Aggregate

        public RRDBuilder<T> Aggregate(UInt64              NumberOfValues,
                                       AggregationFunction AggregationFunction = AggregationFunction.AVERAGE)
        {
            return this;
        }

        public RRDBuilder<T> Aggregate(TimeSpan            TimeSpan,
                                       AggregationFunction AggregationFunction = AggregationFunction.AVERAGE)
        {
            return this;
        }


        public RRDBuilder<T> Aggregate(UInt64                  NumberOfValues,
                                       Func<IEnumerable<T>, T> AggregationFunction)
        {
            return this;
        }

        public RRDBuilder<T> Aggregate(TimeSpan                TimeSpan,
                                       Func<IEnumerable<T>, T> AggregationFunction)
        {
            return this;
        }

        #endregion

        //rrdtool create test.rrd             \
        //    --start 920804400          \
        //    DS:speed:COUNTER:600:U:U   \
        //    RRA:AVERAGE:0.5:1:24       \
        //    RRA:AVERAGE:0.5:6:10



        
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




        private void ReflectStruct()
        {

            var StructType           = typeof(T);
            var TypeAttributes       = StructType.GetCustomAttributes(false);

            #region Analyse RRDatabaseAttribute

            var _RRDatabaseAttribute = (from   CustomAttribute
                                        in     TypeAttributes
                                        where  CustomAttribute as RRDatabaseAttribute != null
                                        select CustomAttribute as RRDatabaseAttribute).First();

            this.Name                = _RRDatabaseAttribute.Name;
            this.Interval            = TimeSpan.FromMilliseconds(_RRDatabaseAttribute.Interval);
            this.Timeout             = TimeSpan.FromMilliseconds(_RRDatabaseAttribute.Timeout);
            this.Keep                = _RRDatabaseAttribute.Keep;

            #endregion

            #region Analyse RRArchiveAttributes

            var _RRArchiveAttributes = (from   CustomAttribute
                                        in     TypeAttributes
                                        where  CustomAttribute as RRArchiveAttribute != null
                                        select CustomAttribute as RRArchiveAttribute);

            #endregion

            ReflectStruct(StructType);

        }

        private void ReflectStruct(Type DeclaringType)
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







        
        


        public void Dispose()
        {
            //mmf.Dispose();
        }

    }

}

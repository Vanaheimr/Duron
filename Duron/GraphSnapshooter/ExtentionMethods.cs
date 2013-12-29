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
using System.IO;
using System.Web;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using eu.Vanaheimr.Illias.Commons;
using eu.Vanaheimr.Illias.Commons.Collections;
using eu.Vanaheimr.Balder;
using eu.Vanaheimr.Styx;

#endregion

namespace eu.Vanaheimr.Duron
{

    public static class Ext
    {

        #region AttachSnapshooter(Graph, ...)

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TVertexLabel">The type of the vertex label.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the edge label.</typeparam>
        /// <typeparam name="TMultiEdgeLabel">The type of the multiedge label.</typeparam>
        /// <typeparam name="THyperEdgeLabel">The type of the hyperedge label.</typeparam>
        public static GraphSnapshooter<TVertexLabel, TEdgeLabel, TMultiEdgeLabel, THyperEdgeLabel>

                          AttachSnapshooter<TVertexLabel, TEdgeLabel, TMultiEdgeLabel, THyperEdgeLabel>(

                              this IGenericPropertyGraph<String, Int64, TVertexLabel,    String, Object,
                                                         String, Int64, TEdgeLabel,      String, Object,
                                                         String, Int64, TMultiEdgeLabel, String, Object,
                                                         String, Int64, THyperEdgeLabel, String, Object> Graph,
                              String                                                 Prefix,
                              String                                                 Suffix,
                              VertexLabelParserDelegate   <String, TVertexLabel>     VertexLabelParser,
                              EdgeLabelParserDelegate     <String, TEdgeLabel>       EdgeLabelParser,
                              MultiEdgeLabelParserDelegate<String, TMultiEdgeLabel>  MultiEdgeLabelParser,
                              HyperEdgeLabelParserDelegate<String, THyperEdgeLabel>  HyperEdgeLabelParser,
                              UInt32                                                 NumberOfBackupFiles  = 10,
                              UInt64                                                 SaveEveryMSec        = 5000,
                              String                                                 WorkingDirectory     = null)


            where TVertexLabel     : IEquatable<TVertexLabel>,    IComparable<TVertexLabel>,    IComparable
            where TEdgeLabel       : IEquatable<TEdgeLabel>,      IComparable<TEdgeLabel>,      IComparable
            where TMultiEdgeLabel  : IEquatable<TMultiEdgeLabel>, IComparable<TMultiEdgeLabel>, IComparable
            where THyperEdgeLabel  : IEquatable<THyperEdgeLabel>, IComparable<THyperEdgeLabel>, IComparable

        {

            return new GraphSnapshooter<TVertexLabel,
                                        TEdgeLabel,
                                        TMultiEdgeLabel,
                                        THyperEdgeLabel>(Graph,
                                                         Prefix,
                                                         Suffix,
                                                         VertexLabelParser,
                                                         EdgeLabelParser,
                                                         MultiEdgeLabelParser,
                                                         HyperEdgeLabelParser,
                                                         NumberOfBackupFiles,
                                                         SaveEveryMSec,
                                                         WorkingDirectory);

        }

        #endregion

        #region SerializeProperties(Properties)

        public static String SerializeProperties(this IReadOnlyProperties<String, Object> Properties)
        {

            var VertexPropertyList  = new List<String>();
            var VertexPropertyValue = "";

            foreach (var p in Properties)
            {

                var _JSONString = p.Value as JSONString;

                if (_JSONString != null)
                    VertexPropertyValue = _JSONString.JSONString;
                else
                    VertexPropertyValue = @"""" + HttpUtility.UrlEncode(p.Value.ToString()) + @"""";

                VertexPropertyList.Add(String.Concat(@"""", p.Key, @""": ", VertexPropertyValue));

            }

            return VertexPropertyList.CSVAggregate(@"""Properties"": { ", " }");

        }

        #endregion

        #region SerializeVertex(Vertex)

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TVertexLabel">The type of the vertex label.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the edge label.</typeparam>
        /// <typeparam name="TMultiEdgeLabel">The type of the multiedge label.</typeparam>
        /// <typeparam name="THyperEdgeLabel">The type of the hyperedge label.</typeparam>
        /// <param name="Vertex"></param>
        /// <returns></returns>
        public static String SerializeVertex<TVertexLabel, TEdgeLabel, TMultiEdgeLabel, THyperEdgeLabel>(
                                 this IReadOnlyGenericPropertyVertex<String, Int64, TVertexLabel, String, Object,
                                                                     String, Int64, TEdgeLabel,      String, Object,
                                                                     String, Int64, TMultiEdgeLabel, String, Object,
                                                                     String, Int64, THyperEdgeLabel, String, Object> Vertex)

            where TVertexLabel     : IEquatable<TVertexLabel>,    IComparable<TVertexLabel>,    IComparable
            where TEdgeLabel       : IEquatable<TEdgeLabel>,      IComparable<TEdgeLabel>,      IComparable
            where TMultiEdgeLabel  : IEquatable<TMultiEdgeLabel>, IComparable<TMultiEdgeLabel>, IComparable
            where THyperEdgeLabel  : IEquatable<THyperEdgeLabel>, IComparable<THyperEdgeLabel>, IComparable

        {

            var Result   = new StringBuilder(@"{ ""AddVertex"": { ").Append(Vertex.SerializeProperties());
            var EdgeList = new List<String>();

            Vertex.OutEdges().
                   ForEach(e => EdgeList.Add(String.Concat(@"{ ""InVertex"": """, HttpUtility.UrlEncode(e.InVertex.Id.ToString()), @""", ", e.SerializeProperties(), " }")));

            if (EdgeList.Any())
                Result.AppendCSV(@", ""OutEdges"": [ ", EdgeList, " ]");

            return Result.Append("} }").ToString();

        }

        #endregion

    }

}

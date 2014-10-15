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

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Illias.Collections;
using org.GraphDefined.Vanaheimr.Balder;
using org.GraphDefined.Vanaheimr.Styx;

#endregion

namespace org.GraphDefined.Vanaheimr.Duron
{

    public delegate Boolean VertexLabelParserDelegate   <T, TVertexLabel>    (T Label, out TVertexLabel     VertexLabel)
        where TVertexLabel : IEquatable<TVertexLabel>, IComparable<TVertexLabel>, IComparable;

    public delegate Boolean EdgeLabelParserDelegate     <T, TEdgeLabel>      (T Label, out TEdgeLabel       EdgeLabel)
        where TEdgeLabel : IEquatable<TEdgeLabel>, IComparable<TEdgeLabel>, IComparable;

    public delegate Boolean MultiEdgeLabelParserDelegate<T, TMultiEdgeLabel> (T Label, out TMultiEdgeLabel MultiEdgeLabel)
        where TMultiEdgeLabel  : IEquatable<TMultiEdgeLabel>, IComparable<TMultiEdgeLabel>, IComparable;

    public delegate Boolean HyperEdgeLabelParserDelegate<T, THyperEdgeLabel> (T Label, out THyperEdgeLabel HyperEdgeLabel)
        where THyperEdgeLabel  : IEquatable<THyperEdgeLabel>, IComparable<THyperEdgeLabel>, IComparable;

}

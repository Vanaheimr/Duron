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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Balder;
using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Illias.Collections;
using org.GraphDefined.Vanaheimr.Styx;
using org.GraphDefined.Vanaheimr.Walkyr;

#endregion

namespace org.GraphDefined.Vanaheimr.Duron
{

    /// <summary>
    /// Handels graph snap shots.
    /// </summary>
    /// <typeparam name="TIdVertex">The type of the vertex identifiers.</typeparam>
    /// <typeparam name="TRevIdVertex">The type of the vertex revision identifiers.</typeparam>
    /// <typeparam name="TVertexLabel">The type of the vertex type.</typeparam>
    /// <typeparam name="TKeyVertex">The type of the vertex property keys.</typeparam>
    /// <typeparam name="TValueVertex">The type of the vertex property values.</typeparam>
    /// 
    /// <typeparam name="TIdEdge">The type of the edge identifiers.</typeparam>
    /// <typeparam name="TRevIdEdge">The type of the edge revision identifiers.</typeparam>
    /// <typeparam name="TEdgeLabel">The type of the edge label.</typeparam>
    /// <typeparam name="TKeyEdge">The type of the edge property keys.</typeparam>
    /// <typeparam name="TValueEdge">The type of the edge property values.</typeparam>
    /// 
    /// <typeparam name="TIdMultiEdge">The type of the multiedge identifiers.</typeparam>
    /// <typeparam name="TRevIdMultiEdge">The type of the multiedge revision identifiers.</typeparam>
    /// <typeparam name="TMultiEdgeLabel">The type of the multiedge label.</typeparam>
    /// <typeparam name="TKeyMultiEdge">The type of the multiedge property keys.</typeparam>
    /// <typeparam name="TValueMultiEdge">The type of the multiedge property values.</typeparam>
    /// 
    /// <typeparam name="TIdHyperEdge">The type of the hyperedge identifiers.</typeparam>
    /// <typeparam name="TRevIdHyperEdge">The type of the hyperedge revision identifiers.</typeparam>
    /// <typeparam name="THyperEdgeLabel">The type of the hyperedge label.</typeparam>
    /// <typeparam name="TKeyHyperEdge">The type of the hyperedge property keys.</typeparam>
    /// <typeparam name="TValueHyperEdge">The type of the hyperedge property values.</typeparam>
    public class GraphSnapshooter<TIdVertex,    TRevIdVertex,    TVertexLabel,    TKeyVertex,    TValueVertex,
                                  TIdEdge,      TRevIdEdge,      TEdgeLabel,      TKeyEdge,      TValueEdge,
                                  TIdMultiEdge, TRevIdMultiEdge, TMultiEdgeLabel, TKeyMultiEdge, TValueMultiEdge,
                                  TIdHyperEdge, TRevIdHyperEdge, THyperEdgeLabel, TKeyHyperEdge, TValueHyperEdge>

        where TIdVertex        : IEquatable<TIdVertex>,       IComparable<TIdVertex>,       IComparable, TValueVertex
        where TIdEdge          : IEquatable<TIdEdge>,         IComparable<TIdEdge>,         IComparable, TValueEdge
        where TIdMultiEdge     : IEquatable<TIdMultiEdge>,    IComparable<TIdMultiEdge>,    IComparable, TValueMultiEdge
        where TIdHyperEdge     : IEquatable<TIdHyperEdge>,    IComparable<TIdHyperEdge>,    IComparable, TValueHyperEdge

        where TRevIdVertex     : IEquatable<TRevIdVertex>,    IComparable<TRevIdVertex>,    IComparable, TValueVertex
        where TRevIdEdge       : IEquatable<TRevIdEdge>,      IComparable<TRevIdEdge>,      IComparable, TValueEdge
        where TRevIdMultiEdge  : IEquatable<TRevIdMultiEdge>, IComparable<TRevIdMultiEdge>, IComparable, TValueMultiEdge
        where TRevIdHyperEdge  : IEquatable<TRevIdHyperEdge>, IComparable<TRevIdHyperEdge>, IComparable, TValueHyperEdge

        where TVertexLabel     : IEquatable<TVertexLabel>,    IComparable<TVertexLabel>,    IComparable, TValueVertex
        where TEdgeLabel       : IEquatable<TEdgeLabel>,      IComparable<TEdgeLabel>,      IComparable, TValueEdge
        where TMultiEdgeLabel  : IEquatable<TMultiEdgeLabel>, IComparable<TMultiEdgeLabel>, IComparable, TValueMultiEdge
        where THyperEdgeLabel  : IEquatable<THyperEdgeLabel>, IComparable<THyperEdgeLabel>, IComparable, TValueHyperEdge

        where TKeyVertex       : IEquatable<TKeyVertex>,      IComparable<TKeyVertex>,      IComparable
        where TKeyEdge         : IEquatable<TKeyEdge>,        IComparable<TKeyEdge>,        IComparable
        where TKeyMultiEdge    : IEquatable<TKeyMultiEdge>,   IComparable<TKeyMultiEdge>,   IComparable
        where TKeyHyperEdge    : IEquatable<TKeyHyperEdge>,   IComparable<TKeyHyperEdge>,   IComparable

    {

        #region Data

        private readonly IGraphIO<TIdVertex,    TRevIdVertex,    TVertexLabel,    TKeyVertex,    TValueVertex,
                                  TIdEdge,      TRevIdEdge,      TEdgeLabel,      TKeyEdge,      TValueEdge,
                                  TIdMultiEdge, TRevIdMultiEdge, TMultiEdgeLabel, TKeyMultiEdge, TValueMultiEdge,
                                  TIdHyperEdge, TRevIdHyperEdge, THyperEdgeLabel, TKeyHyperEdge, TValueHyperEdge,
                                  String>                                       GraphSerializer;

        public  const    String                                                 DefaultPrefix = "graphbackup";
        public  const    String                                                 DefaultSuffix = "graph";

        public  readonly Boolean                                                SerializeVertices;
        public  readonly Boolean                                                SerializeEdges;
        public  readonly Boolean                                                SerializeMultiEdges;
        public  readonly Boolean                                                SerializeHyperEdges;

        private readonly Timer                                                  SerializationTimer;

        private readonly VertexLabelParserDelegate   <String, TVertexLabel>     VertexLabelParser;
        private readonly EdgeLabelParserDelegate     <String, TEdgeLabel>       EdgeLabelParser;
        private readonly MultiEdgeLabelParserDelegate<String, TMultiEdgeLabel>  MultiEdgeLabelParser;
        private readonly HyperEdgeLabelParserDelegate<String, THyperEdgeLabel>  HyperEdgeLabelParser;

        private readonly Func<String, TKeyVertex>                               TKeyVertexParser;
        private readonly Func<String, TValueVertex>                             TValueVertexParser;
        private readonly Func<String, TKeyEdge>                                 TKeyEdgeParser;
        private readonly Func<String, TValueEdge>                               TValueEdgeParser;

        #endregion

        #region Properties

        public IGenericPropertyGraph<TIdVertex,    TRevIdVertex,    TVertexLabel,    TKeyVertex,    TValueVertex,
                                     TIdEdge,      TRevIdEdge,      TEdgeLabel,      TKeyEdge,      TValueEdge,
                                     TIdMultiEdge, TRevIdMultiEdge, TMultiEdgeLabel, TKeyMultiEdge, TValueMultiEdge,
                                     TIdHyperEdge, TRevIdHyperEdge, THyperEdgeLabel, TKeyHyperEdge, TValueHyperEdge> Graph { get; private set; }

        public String Prefix { get; private set; }

        public String Suffix { get; private set; }

        #region NumberOfBackupFiles

        private readonly UInt32 _NumberOfBackupFiles;

        public UInt32 NumberOfBackupFiles
        {
            get
            {
                return _NumberOfBackupFiles;
            }
        }

        #endregion

        #region LastSavePointId

        private volatile UInt32 _LastSavePointId;

        public UInt32 LastSavePointId
        {
            get
            {
                return _LastSavePointId;
            }
        }
 
        #endregion

        #region TimerEnabled

        private volatile Boolean _StorageTimerEnabled;

        public Boolean StorageTimerEnabled
        {
            get
            {
                return _StorageTimerEnabled;
            }
        }

        #endregion

        public UInt64 SaveEveryMSec { get; private set; }

        public String WorkingDirectory { get; private set; }

        #endregion

        #region Events

        #region OnSavePointLoading

        public delegate void SavePointLoadingEventHandler(String FileName);

        public event SavePointLoadingEventHandler OnSavePointLoading;

        #endregion

        #region OnSavePointLoaded

        public delegate void SavePointLoadedEventHandler(String FileName,
                                                         UInt64 NumberOfVertices,
                                                         UInt64 NumberOfEdges,
                                                         UInt64 NumberOfMultiEdges,
                                                         UInt64 NumberOfHyperEdges,
                                                         UInt64 ElapsedMilliseconds);

        public event SavePointLoadedEventHandler OnSavePointLoaded;

        #endregion

        #region OnSavePointStored

        public delegate void SavePointStoredEventHandler(String FileName,
                                                         UInt64 NumberOfVertices,
                                                         UInt64 NumberOfEdges,
                                                         UInt64 NumberOfMultiEdges,
                                                         UInt64 NumberOfHyperEdges,
                                                         UInt64 ElapsedMilliseconds);

        public event SavePointStoredEventHandler OnSavePointStored;

        #endregion

        #endregion

        #region Constructor(s)

        #region Read-only graph...

        public GraphSnapshooter(IReadOnlyGenericPropertyGraph<TIdVertex,    TRevIdVertex,    TVertexLabel,    TKeyVertex,    TValueVertex,
                                                              TIdEdge,      TRevIdEdge,      TEdgeLabel,      TKeyEdge,      TValueEdge,
                                                              TIdMultiEdge, TRevIdMultiEdge, TMultiEdgeLabel, TKeyMultiEdge, TValueMultiEdge,
                                                              TIdHyperEdge, TRevIdHyperEdge, THyperEdgeLabel, TKeyHyperEdge, TValueHyperEdge> Graph,

                                IGraphIO<TIdVertex,    TRevIdVertex,    TVertexLabel,    TKeyVertex,    TValueVertex,
                                                 TIdEdge,      TRevIdEdge,      TEdgeLabel,      TKeyEdge,      TValueEdge,
                                                 TIdMultiEdge, TRevIdMultiEdge, TMultiEdgeLabel, TKeyMultiEdge, TValueMultiEdge,
                                                 TIdHyperEdge, TRevIdHyperEdge, THyperEdgeLabel, TKeyHyperEdge, TValueHyperEdge,
                                                 String>                                GraphSerializer,

                                String                                                  Prefix                  = DefaultPrefix,
                                String                                                  Suffix                  = DefaultSuffix,

                                Boolean                                                 SerializeVertices       = true,
                                Boolean                                                 SerializeEdges          = false,
                                Boolean                                                 SerializeMultiEdges     = false,
                                Boolean                                                 SerializeHyperEdges     = false,

                                Func<TIdVertex,    String>                              VertexIdSerializer      = null,
                                Func<TIdEdge,      String>                              EdgeIdSerializer        = null,
                                Func<TIdMultiEdge, String>                              MultiEdgeIdSerializer   = null,
                                Func<TIdHyperEdge, String>                              HyperEdgeIdSerializer   = null,

                                Func<TKeyVertex,   String>                              TKeyVertexSerializer    = null,
                                Func<TValueVertex, String>                              TValueVertexSerializer  = null,
                                Func<TKeyEdge,     String>                              TKeyEdgeSerializer      = null,
                                Func<TValueEdge,   String>                              TValueEdgeSerializer    = null,

                                UInt32                                                  NumberOfBackupFiles     = 10,
                                UInt64                                                  SaveEveryMSec           = 5000,
                                String                                                  WorkingDirectory        = null)

            : this(Graph.AsMutable(),
                   GraphSerializer,
                   Prefix,
                   Suffix,

                   SerializeVertices,
                   SerializeEdges,
                   SerializeMultiEdges,
                   SerializeHyperEdges,

                   VertexIdSerializer,
                   EdgeIdSerializer,
                   MultiEdgeIdSerializer,
                   HyperEdgeIdSerializer,

                   TKeyVertexSerializer,
                   TValueVertexSerializer,
                   TKeyEdgeSerializer,
                   TValueEdgeSerializer,

                   null,
                   null,
                   null,
                   null,

                   null,
                   null,
                   null,
                   null,

                   NumberOfBackupFiles,
                   SaveEveryMSec,
                   WorkingDirectory,
                   false)

        { }

        #endregion

        #region R/W

        public GraphSnapshooter(IGenericPropertyGraph<TIdVertex,    TRevIdVertex,    TVertexLabel,    TKeyVertex,    TValueVertex,
                                                      TIdEdge,      TRevIdEdge,      TEdgeLabel,      TKeyEdge,      TValueEdge,
                                                      TIdMultiEdge, TRevIdMultiEdge, TMultiEdgeLabel, TKeyMultiEdge, TValueMultiEdge,
                                                      TIdHyperEdge, TRevIdHyperEdge, THyperEdgeLabel, TKeyHyperEdge, TValueHyperEdge> Graph,

                                IGraphIO<TIdVertex,    TRevIdVertex,    TVertexLabel,    TKeyVertex,    TValueVertex,
                                                 TIdEdge,      TRevIdEdge,      TEdgeLabel,      TKeyEdge,      TValueEdge,
                                                 TIdMultiEdge, TRevIdMultiEdge, TMultiEdgeLabel, TKeyMultiEdge, TValueMultiEdge,
                                                 TIdHyperEdge, TRevIdHyperEdge, THyperEdgeLabel, TKeyHyperEdge, TValueHyperEdge,
                                                 String>                                GraphSerializer,

                                String                                                  Prefix                  = DefaultPrefix,
                                String                                                  Suffix                  = DefaultSuffix,

                                Boolean                                                 SerializeVertices       = true,
                                Boolean                                                 SerializeEdges          = false,
                                Boolean                                                 SerializeMultiEdges     = false,
                                Boolean                                                 SerializeHyperEdges     = false,

                                Func<TIdVertex,    String>                              VertexIdSerializer      = null,
                                Func<TIdEdge,      String>                              EdgeIdSerializer        = null,
                                Func<TIdMultiEdge, String>                              MultiEdgeIdSerializer   = null,
                                Func<TIdHyperEdge, String>                              HyperEdgeIdSerializer   = null,

                                Func<TKeyVertex,   String>                              TKeyVertexSerializer    = null,
                                Func<TValueVertex, String>                              TValueVertexSerializer  = null,
                                Func<TKeyEdge,     String>                              TKeyEdgeSerializer      = null,
                                Func<TValueEdge,   String>                              TValueEdgeSerializer    = null,

                                VertexLabelParserDelegate   <String, TVertexLabel>      VertexLabelParser       = null,
                                EdgeLabelParserDelegate     <String, TEdgeLabel>        EdgeLabelParser         = null,
                                MultiEdgeLabelParserDelegate<String, TMultiEdgeLabel>   MultiEdgeLabelParser    = null,
                                HyperEdgeLabelParserDelegate<String, THyperEdgeLabel>   HyperEdgeLabelParser    = null,

                                Func<String, TKeyVertex>                                TKeyVertexParser        = null,
                                Func<String, TValueVertex>                              TValueVertexParser      = null,
                                Func<String, TKeyEdge>                                  TKeyEdgeParser          = null,
                                Func<String, TValueEdge>                                TValueEdgeParser        = null,

                                UInt32                                                  NumberOfBackupFiles     = 10,
                                UInt64                                                  SaveEveryMSec           = 5000,
                                String                                                  WorkingDirectory        = null,
                                Boolean                                                 TryToLoadPreviousBackup = true)

        {

            if (GraphSerializer == null)
                throw new ArgumentNullException("GraphSerializer!");

            this.GraphSerializer            = GraphSerializer;

            this.Graph                      = Graph;
            this.Prefix                     = Prefix;
            this.Suffix                     = Suffix;

            this.SerializeVertices          = SerializeVertices;
            this.SerializeEdges             = SerializeEdges;
            this.SerializeMultiEdges        = SerializeMultiEdges;
            this.SerializeHyperEdges        = SerializeHyperEdges;

            this.VertexLabelParser          = VertexLabelParser;
            this.EdgeLabelParser            = EdgeLabelParser;
            this.MultiEdgeLabelParser       = MultiEdgeLabelParser;
            this.HyperEdgeLabelParser       = HyperEdgeLabelParser;

            this.TKeyVertexParser           = (TKeyVertexParser       != null) ? TKeyVertexParser       : VertexKey   => (TKeyVertex)   (Object) HttpUtility.UrlDecode(VertexKey);
            this.TValueVertexParser         = (TValueVertexParser     != null) ? TValueVertexParser     : VertexValue => (TValueVertex) (Object) HttpUtility.UrlDecode(VertexValue);
            this.TKeyEdgeParser             = (TKeyEdgeParser         != null) ? TKeyEdgeParser         : EdgeKey     => (TKeyEdge)     (Object) HttpUtility.UrlDecode(EdgeKey);
            this.TValueEdgeParser           = (TValueEdgeParser       != null) ? TValueEdgeParser       : EdgeValue   => (TValueEdge)   (Object) HttpUtility.UrlDecode(EdgeValue);

            this._NumberOfBackupFiles       = NumberOfBackupFiles;
            this.SaveEveryMSec              = SaveEveryMSec;
            this.WorkingDirectory           = (WorkingDirectory != null)
                                                  ? WorkingDirectory
                                                  : Directory.GetCurrentDirectory();

            if (!Directory.Exists(this.WorkingDirectory))
                throw new ArgumentException("The directory '" + this.WorkingDirectory + "' seems to be invalid!");

            this._StorageTimerEnabled       = false;
            this.SerializationTimer         = new Timer(_ => Store((Boolean) _), _StorageTimerEnabled, 0, (Int64) SaveEveryMSec); 

        }

        #endregion

        #endregion


        #region (private) Store(_TimerEnabled)

        private void Store(Boolean _TimerEnabled)
        {

            // Avoid multiple concurrent runs!
            if (_StorageTimerEnabled && Monitor.TryEnter(Graph))
            {

                try
                {

                    var FileName  = Prefix + LastSavePointId + "." + Suffix;
                    var StopWatch = Stopwatch.StartNew();

                    using (var OutFile = new StreamWriter(FileName, append: false, encoding: Encoding.UTF8))
                    {

                        // For BalderSON this (currently) includes the outedges!
                        if (SerializeVertices)
                            Graph.Vertices().  ForEach(vertex    => OutFile.WriteLine(GraphSerializer.Serialize(vertex)));

                        if (SerializeEdges)
                            Graph.Edges().     ForEach(edge      => OutFile.WriteLine(GraphSerializer.Serialize(edge)));

                        if (SerializeMultiEdges)
                            Graph.MultiEdges().ForEach(multiedge => OutFile.WriteLine(GraphSerializer.Serialize(multiedge)));

                        if (SerializeHyperEdges)
                            Graph.HyperEdges().ForEach(hyperedge => OutFile.WriteLine(GraphSerializer.Serialize(hyperedge)));

                    }

                    var OnSavePointStoredLocal = OnSavePointStored;
                    if (OnSavePointStoredLocal != null)
                        OnSavePointStored(FileName,
                                          Graph.NumberOfVertices(),
                                          Graph.NumberOfEdges(),
                                          Graph.NumberOfMultiEdges(),
                                          Graph.NumberOfHyperEdges(),
                                          (UInt64) StopWatch.ElapsedMilliseconds);

                }
                finally
                {

                    _LastSavePointId++;
                    _LastSavePointId %= _NumberOfBackupFiles;

                    Monitor.Exit(Graph);

                }

            }

        }

        #endregion


        #region TryLoad(SavePointId = null)

        public void TryLoad(Nullable<UInt32> SavePointId = null)
        {

            _StorageTimerEnabled = false;

            lock (Graph)
            {

                #region Check given SavePointId...

                String LastSavePointFile;

                if (SavePointId.HasValue)
                {

                    LastSavePointFile = WorkingDirectory + Path.DirectorySeparatorChar + Prefix + SavePointId + "." + Suffix;

                    if (!File.Exists(LastSavePointFile))
                        throw new FileNotFoundException("File '" + LastSavePointFile + "' was not found!");

                    this._LastSavePointId = SavePointId.Value;

                    var OnSavePointLoadingLocal = OnSavePointLoading;
                    if (OnSavePointLoadingLocal != null)
                        OnSavePointLoadingLocal(LastSavePointFile);

                }

                #endregion

                #region ... or try to find latest SavePointId in the working directory...

                else
                {

                    var LastSavePointString = (from    _FileInfo
                                               in       new DirectoryInfo(WorkingDirectory).
                                                        GetFiles(Prefix + "*." + Suffix)
                                               where   _FileInfo.Length > 0
                                               orderby _FileInfo.LastWriteTime descending
                                               select  _FileInfo.Name.
                                                                 Replace(Prefix,       "").
                                                                 Replace("." + Suffix, "")).
                                               FirstOrDefault();

                    UInt32 _LastSavePointId = 0U;

                    if (LastSavePointString != null &&
                        UInt32.TryParse(LastSavePointString, out _LastSavePointId))
                    {

                        this._LastSavePointId = _LastSavePointId;

                        LastSavePointFile = WorkingDirectory + Path.DirectorySeparatorChar + Prefix + this.LastSavePointId + "." + Suffix;

                        if (OnSavePointLoading != null)
                            OnSavePointLoading(LastSavePointFile);

                    }

                    // DEFAULT: SnapShot0.graph => End of process...
                    else
                    {

                        this._LastSavePointId = 0;

                        LastSavePointFile = Prefix + this.LastSavePointId + "." + Suffix;

                        _StorageTimerEnabled = true;

                        return;

                    }

                }

                #endregion


                JObject       _JObject;
                JObject       AddVertexCommand;
                List<JToken>  DelayedEdges;
                JObject       AddEdgeCommand;
                JObject       AddGroupCommand;

                var StopWatch = Stopwatch.StartNew();

                try
                {

                    DelayedEdges = new List<JToken>();

                    foreach (var CurrentLine in File.ReadLines(LastSavePointFile, Encoding.UTF8))
                    {

                        // Ignore comments and empty lines
                        if (CurrentLine == null                  ||
                            CurrentLine.Trim() == ""             ||
                            CurrentLine.Trim().StartsWith("#")   ||
                            CurrentLine.Trim().StartsWith("//"))
                            continue;

                        _JObject = null;

                        try
                        {
                            _JObject = JObject.Parse(CurrentLine);
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Could not parse JSON: " + CurrentLine, e);
                        }

                        if      ((AddVertexCommand = _JObject["AddVertex"] as JObject) != null)
                            ParseVertex(Graph, AddVertexCommand, DelayedEdges);

                        else if ((AddEdgeCommand   = _JObject["AddEdge"  ] as JObject) != null)
                            ParseEdge  (Graph, AddEdgeCommand);

                        else if ((AddGroupCommand  = _JObject["AddGroup" ] as JObject) != null)
                        {

                            foreach (var GroupedCommand in AddGroupCommand.Children())
                            {

                                if ((AddVertexCommand = GroupedCommand["AddVertex"] as JObject) != null)
                                    ParseVertex(Graph, AddVertexCommand, DelayedEdges);

                                else if ((AddEdgeCommand = GroupedCommand["AddEdge"] as JObject) != null)
                                    ParseEdge(Graph, AddEdgeCommand);

                            }

                        }

                    }

                    foreach (var DelayedEdgeJSON in DelayedEdges)
                        ParseEdge(Graph, DelayedEdgeJSON);

                }

                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                var OnSavePointLoadedLocal = OnSavePointLoaded;
                if (OnSavePointLoadedLocal != null)
                    OnSavePointLoadedLocal(LastSavePointFile,
                                           Graph.NumberOfVertices(),
                                           Graph.NumberOfEdges(),
                                           Graph.NumberOfMultiEdges(),
                                           Graph.NumberOfHyperEdges(),
                                           (UInt64) StopWatch.ElapsedMilliseconds);

                this._LastSavePointId++;
                _StorageTimerEnabled = true;

            }

        }

        #endregion


        #region ParseVertex(...)

        public void ParseVertex(IGenericPropertyGraph<TIdVertex,    TRevIdVertex,    TVertexLabel,    TKeyVertex,    TValueVertex,
                                                      TIdEdge,      TRevIdEdge,      TEdgeLabel,      TKeyEdge,      TValueEdge,
                                                      TIdMultiEdge, TRevIdMultiEdge, TMultiEdgeLabel, TKeyMultiEdge, TValueMultiEdge,
                                                      TIdHyperEdge, TRevIdHyperEdge, THyperEdgeLabel, TKeyHyperEdge, TValueHyperEdge> graph,
                                JObject      VertexJSON,
                                List<JToken> DelayedEdges)

        {

            TVertexLabel  VertexLabel;
            JProperty     Property;
            JArray        PropertyArray;
            JObject       PropertyObject;

            JToken        InVertexId;
            TEdgeLabel    EdgeLabel;
            JToken        EdgeProperties;
            IReadOnlyGenericPropertyVertex<TIdVertex,    TRevIdVertex,    TVertexLabel,    TKeyVertex,    TValueVertex,
                                           TIdEdge,      TRevIdEdge,      TEdgeLabel,      TKeyEdge,      TValueEdge,
                                           TIdMultiEdge, TRevIdMultiEdge, TMultiEdgeLabel, TKeyMultiEdge, TValueMultiEdge,
                                           TIdHyperEdge, TRevIdHyperEdge, THyperEdgeLabel, TKeyHyperEdge, TValueHyperEdge> InVertex;

            var VertexProperties = VertexJSON["Properties"];
            if (VertexProperties == null)
                return;


            // 1) Parse vertex label...
            if (VertexLabelParser(HttpUtility.UrlDecode(VertexProperties["Label"].Value<String>()), out VertexLabel))
            {

                // 2) Try to crate the vertex...
                graph.AddVertex(VertexProperties["Id"].Value<TIdVertex>(), VertexLabel, Vertex => {


                    #region 3) Add vertex properties...

                    VertexProperties.TryForEach(VertexPropertyJSON => {

                        Property = VertexPropertyJSON as JProperty;

                        if (Property.Name != "Id"    &&
                            Property.Name != "RevId" &&
                            Property.Name != "Label") {

                            if (Property.Value is JValue)
                                Vertex.SetProperty(TKeyVertexParser  (Property.Name),
                                                   TValueVertexParser(HttpUtility.UrlDecode(Property.Value.ToString())));

                            else if ((PropertyArray  = Property.Value as JArray)  != null)
                                Vertex.SetProperty(TKeyVertexParser(Property.Name),
                                                   (TValueVertex) (Object) PropertyArray.Select(v => TValueVertexParser((v as JValue).Value.ToString())).ToList());

                            else if ((PropertyObject = Property.Value as JObject) != null)
                                Vertex.SetProperty(TKeyVertexParser(Property.Name),
                                                   (TValueVertex) (Object) PropertyObject.Properties().ToDictionary(v => TKeyVertexParser  (v.Name),
                                                                                                                    v => TValueVertexParser(v.Value.ToString())));

                        }

                    });

                    #endregion

                    #region 4) Add outedges...

                    VertexJSON["OutEdges"].TryForEach(OutEdgeJSON => {

                        InVertexId      = OutEdgeJSON["InVertex"];
                        EdgeProperties  = OutEdgeJSON["Properties"];

                        if (EdgeLabelParser(HttpUtility.UrlDecode(EdgeProperties["Label"].Value<String>()), out EdgeLabel))
                        {

                            if (graph.TryGetVertexById(InVertexId.Value<TIdVertex>(), out InVertex))
                            {

                                Vertex.AddOutEdge(EdgeProperties["Id"].Value<TIdEdge>(),
                                                  EdgeLabel,
                                                  InVertex,
                                                  e => EdgeProperties.TryForEach(EdgePropertyJSON => {

                                                       Property = EdgePropertyJSON as JProperty;

                                                       if (Property.Name != "Id"    &&
                                                           Property.Name != "RevId" &&
                                                           Property.Name != "Label") {

                                                           if (Property.Value is JValue)
                                                               e.SetProperty(TKeyEdgeParser  (Property.Name),
                                                                             TValueEdgeParser(Property.Value.ToString()));

                                                           else if ((PropertyArray  = Property.Value as JArray)  != null)
                                                               e.SetProperty(TKeyEdgeParser(Property.Name),
                                                                             (TValueEdge) (Object) PropertyArray.Select(v => TValueEdgeParser((v as JValue).Value.ToString())).ToList());

                                                           else if ((PropertyObject = Property.Value as JObject) != null)
                                                               e.SetProperty(TKeyEdgeParser(Property.Name),
                                                                             (TValueEdge) (Object) PropertyObject.Properties().ToDictionary(v => TKeyEdgeParser  (v.Name),
                                                                                                                                            v => TValueEdgeParser(v.Value.ToString())));

                                                       }

                                                   }));

                            }

                            else
                            {
                                OutEdgeJSON["OutVertex"] = Vertex.Id.ToString();
                                DelayedEdges.Add(OutEdgeJSON);
                            }

                        }
                        else
                            throw new Exception("Unknown EdgeLabel '" + EdgeProperties["Label"].Value<String>() + "'!");

                    });

                    #endregion

                });

            }

            else
                throw new Exception("Unknown VertexLabel '" + VertexJSON["Label"].Value<String>() + "'!");

        }

        #endregion

        #region ParseEdge(...)

        public void ParseEdge(IGenericPropertyGraph<TIdVertex,    TRevIdVertex,    TVertexLabel,    TKeyVertex,    TValueVertex,
                                                    TIdEdge,      TRevIdEdge,      TEdgeLabel,      TKeyEdge,      TValueEdge,
                                                    TIdMultiEdge, TRevIdMultiEdge, TMultiEdgeLabel, TKeyMultiEdge, TValueMultiEdge,
                                                    TIdHyperEdge, TRevIdHyperEdge, THyperEdgeLabel, TKeyHyperEdge, TValueHyperEdge> graph,
                              JToken EdgeJSON)

        {

            TEdgeLabel _EdgeLabel;

            String EnglishText;
            String GermanText;

            var EdgeProperties = EdgeJSON["Properties"];

            if (EdgeLabelParser(HttpUtility.UrlDecode(EdgeProperties["Label"].Value<String>()), out _EdgeLabel))
            {

                graph.AddEdge(EdgeProperties["Id"].Value<TIdEdge>(),
                              graph.VertexById(EdgeJSON["OutVertex"].Value<TIdVertex>()),
                              _EdgeLabel,
                              graph.VertexById(EdgeJSON["InVertex" ].Value<TIdVertex>()),
                              e => {

                                  foreach (var _JToken in EdgeProperties)
                                  {

                                      if      ((_JToken as JProperty).Name == "OutVertex")
                                          continue;

                                      else if ((_JToken as JProperty).Name == "InVertex")
                                          continue;

                                      else if ((_JToken as JProperty).Name == "Id")
                                          continue;

                                      else if ((_JToken as JProperty).Name == "RevId")
                                          continue;

                                      else if ((_JToken as JProperty).Name == "Label")
                                          continue;

                                      else
                                      {

                                          var _E = (_JToken as JProperty).Value;

                                          //if (_E.HasValues)
                                          //{

                                          //    EnglishText = _E["en"].Value<String>();
                                          //    GermanText  = _E["de"].Value<String>();

                                          //    e.SetProperty(TKeyEdgeParser  ((_JToken as JProperty).Name),
                                          //                  TValueEdgeParser(new I18N(new English(EnglishText),
                                          //                                            new German (GermanText))));

                                          //}

                                          //else
                                              e.SetProperty(TKeyEdgeParser  (HttpUtility.UrlDecode((_JToken as JProperty).Name)),
                                                            TValueEdgeParser(HttpUtility.UrlDecode(_E.ToString())));

                                      }


                                  }

                                });

            }

            else
                throw new Exception("EdgeLabel '" + HttpUtility.UrlDecode(EdgeJSON["Label"].Value<String>()) + "' unknown!");

        }

        #endregion

    }

}

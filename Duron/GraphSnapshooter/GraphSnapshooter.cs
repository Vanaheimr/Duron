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

    /// <summary>
    /// Handels graph snap shots.
    /// </summary>
    /// <typeparam name="TVertexLabel">The type of the vertex label.</typeparam>
    /// <typeparam name="TEdgeLabel">The type of the edge label.</typeparam>
    /// <typeparam name="TMultiEdgeLabel">The type of the multiedge label.</typeparam>
    /// <typeparam name="THyperEdgeLabel">The type of the hyperedge label.</typeparam>
    public class GraphSnapshooter<TVertexLabel, TEdgeLabel, TMultiEdgeLabel, THyperEdgeLabel>

        where TVertexLabel     : IEquatable<TVertexLabel>,    IComparable<TVertexLabel>,    IComparable
        where TEdgeLabel       : IEquatable<TEdgeLabel>,      IComparable<TEdgeLabel>,      IComparable
        where TMultiEdgeLabel  : IEquatable<TMultiEdgeLabel>, IComparable<TMultiEdgeLabel>, IComparable
        where THyperEdgeLabel  : IEquatable<THyperEdgeLabel>, IComparable<THyperEdgeLabel>, IComparable

    {

        #region Data

        private readonly Timer                                                  SerializationTimer;
        private readonly VertexLabelParserDelegate   <String, TVertexLabel>     VertexLabelParser;
        private readonly EdgeLabelParserDelegate     <String, TEdgeLabel>       EdgeLabelParser;
        private readonly MultiEdgeLabelParserDelegate<String, TMultiEdgeLabel>  MultiEdgeLabelParser;
        private readonly HyperEdgeLabelParserDelegate<String, THyperEdgeLabel>  HyperEdgeLabelParser;

        #endregion

        #region Properties

        public IGenericPropertyGraph<String, Int64, TVertexLabel,    String, Object,
                                     String, Int64, TEdgeLabel,      String, Object,
                                     String, Int64, TMultiEdgeLabel, String, Object,
                                     String, Int64, THyperEdgeLabel, String, Object> Graph { get; private set; }

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

        public delegate void SavePointLoadedEventHandler(String FileName, UInt64 NumberOfVertices, UInt64 NumberOfEdges, UInt64 NumberOfMultiEdges, UInt64 NumberOfHyperEdges);

        public event SavePointLoadedEventHandler OnSavePointLoaded;

        #endregion

        #endregion

        #region Constructor(s)

        public GraphSnapshooter(IGenericPropertyGraph<String, Int64, TVertexLabel,    String, Object,
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

        {

            this.Graph                  = Graph;
            this.Prefix                 = Prefix;
            this.Suffix                 = Suffix;
            this.VertexLabelParser      = VertexLabelParser;
            this.EdgeLabelParser        = EdgeLabelParser;
            this.MultiEdgeLabelParser   = MultiEdgeLabelParser;
            this.HyperEdgeLabelParser   = HyperEdgeLabelParser;
            this._NumberOfBackupFiles   = NumberOfBackupFiles;
            this.SaveEveryMSec          = SaveEveryMSec;
            this.WorkingDirectory       = (WorkingDirectory != null)
                                              ? WorkingDirectory
                                              : Directory.GetCurrentDirectory();

            if (!Directory.Exists(this.WorkingDirectory))
                throw new ArgumentException("The directory '" + this.WorkingDirectory + "' seems to be invalid!");

            this._StorageTimerEnabled   = false;
            this.SerializationTimer     = new Timer(_ => Store((Boolean) _), _StorageTimerEnabled, 0, (Int64) SaveEveryMSec); 

        }

        #endregion


        #region (private) Store(_TimerEnabled)

        private void Store(Boolean _TimerEnabled)
        {

            // Avoid multiple concurrent runs!
            if (_StorageTimerEnabled && Monitor.TryEnter(Graph))
            {

                try
                {

                    using (var OutFile = new StreamWriter(Prefix + LastSavePointId + "." + Suffix))
                    {
                        Graph.Vertices().
                              ForEach(vertex => OutFile.WriteLine(vertex.SerializeVertex()));
                    }

                    Console.WriteLine(Graph.NumberOfVertices() + " vertices and " + Graph.NumberOfEdges() + " edges stored to file '" + Prefix + LastSavePointId + "." + Suffix + "'!");

                    _LastSavePointId++;
                    _LastSavePointId %= _NumberOfBackupFiles;

                }
                finally
                {
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

                    if (OnSavePointLoading != null)
                        OnSavePointLoading(LastSavePointFile);

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


                List<JToken> DelayedEdges;
                JToken       AddVertexCommand;
                JToken       AddEdgeCommand;
                JToken       AddGroupCommand;
                JObject      _JObject;

                try
                {

                    DelayedEdges = new List<JToken>();

                    using (var InputFile = new StreamReader(LastSavePointFile))
                    {

                        foreach (var CurrentLine in InputFile.GetLines())
                        {

                            if (CurrentLine == null ||
                                CurrentLine.Trim() == "" ||
                                CurrentLine.StartsWith("#") ||
                                CurrentLine.StartsWith("//"))
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

                            if ((AddVertexCommand = _JObject["AddVertex"]) != null)
                                ParseVertex(Graph, AddVertexCommand, DelayedEdges);

                            else if ((AddEdgeCommand = _JObject["AddEdge"]) != null)
                                ParseEdge(Graph, AddEdgeCommand);

                            else if ((AddGroupCommand = _JObject["AddGroup"]) != null)
                            {

                                foreach (var GroupedCommand in AddGroupCommand.Children())
                                {

                                    if ((AddVertexCommand = GroupedCommand["AddVertex"]) != null)
                                        ParseVertex(Graph, AddVertexCommand, DelayedEdges);

                                    else if ((AddEdgeCommand = GroupedCommand["AddEdge"]) != null)
                                        ParseEdge(Graph, AddEdgeCommand);

                                }

                            }

                        }

                        InputFile.Close();

                    }

                    foreach (var DelayedEdgeJSON in DelayedEdges)
                        ParseEdge(Graph, DelayedEdgeJSON);

                }

                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                if (OnSavePointLoaded != null)
                    OnSavePointLoaded(LastSavePointFile,
                                      Graph.NumberOfVertices(),
                                      Graph.NumberOfEdges(),
                                      Graph.NumberOfMultiEdges(),
                                      Graph.NumberOfHyperEdges());

                this._LastSavePointId++;
                _StorageTimerEnabled = true;

            }

        }

        #endregion


        #region ParseVertex(...)

        public void ParseVertex(IGenericPropertyGraph<String, Int64, TVertexLabel,    String, Object,
                                                      String, Int64, TEdgeLabel,      String, Object,
                                                      String, Int64, TMultiEdgeLabel, String, Object,
                                                      String, Int64, THyperEdgeLabel, String, Object> graph,
                                JToken       VertexJSON,
                                List<JToken> DelayedEdges)

        {

            TVertexLabel _VertexLabel;

            String EnglishText;
            String GermanText;

            var VertexProperties = VertexJSON["Properties"];
            

            if (VertexProperties == null)
                throw new ArgumentException("No vertex properties found!");


            if (VertexLabelParser(HttpUtility.UrlDecode(VertexProperties["Label"].Value<String>()), out _VertexLabel))
            {
                // VertexProperties["RevId"].Value<String>(), 
                graph.AddVertex(HttpUtility.UrlDecode(VertexProperties["Id"].Value<String>()), _VertexLabel, v =>
                {

                    foreach (var VertexPropertyJSON in VertexProperties)
                    {

                        if      ((VertexPropertyJSON as JProperty).Name == "Id")
                            continue;

                        else if ((VertexPropertyJSON as JProperty).Name == "RevId")
                            continue;

                        else if ((VertexPropertyJSON as JProperty).Name == "Label")
                            continue;

                        else
                        {

                            var _V = (VertexPropertyJSON as JProperty).Value;

                            if (_V.HasValues)
                            {

                                EnglishText = _V["en"].Value<String>();
                                GermanText  = _V["de"].Value<String>();

                                v.SetProperty((VertexPropertyJSON as JProperty).Name, new I18N(new English(EnglishText),
                                                                                               new German (GermanText)));

                            }

                            else
                                v.SetProperty(HttpUtility.UrlDecode((VertexPropertyJSON as JProperty).Name),
                                              HttpUtility.UrlDecode(_V.ToString()));

                        }


                    }


                    VertexJSON["OutEdges"].TryForEach(OutEdgeJSON =>
                    {

                        TEdgeLabel _EdgeLabel;
                        IReadOnlyGenericPropertyVertex<String, Int64, TVertexLabel,    String, Object,
                                                       String, Int64, TEdgeLabel,      String, Object,
                                                       String, Int64, TMultiEdgeLabel, String, Object,
                                                       String, Int64, THyperEdgeLabel, String, Object> _InVertex;

                        var InVertexId     = OutEdgeJSON["InVertex"];
                        var EdgeProperties = OutEdgeJSON["Properties"];

                        if (EdgeLabelParser(HttpUtility.UrlDecode(EdgeProperties["Label"].Value<String>()), out _EdgeLabel))
                        {
                            if (graph.TryGetVertexById(HttpUtility.UrlDecode(InVertexId.Value<String>()), out _InVertex))
                            {

                                var NewEdge = v.AddOutEdge(HttpUtility.UrlDecode(EdgeProperties["Id"].Value<String>()),
                                                                      _EdgeLabel,
                                                                      _InVertex,
                                                                      e =>

                                EdgeProperties.TryForEach(EdgePropertyJSON =>
                                {

                                    if ((EdgePropertyJSON as JProperty).Name != "Id"    &&
                                        (EdgePropertyJSON as JProperty).Name != "RevId" &&
                                        (EdgePropertyJSON as JProperty).Name != "Label")
                                        e.SetProperty(HttpUtility.UrlDecode((EdgePropertyJSON as JProperty).Name),
                                                      HttpUtility.UrlDecode((EdgePropertyJSON as JProperty).Value.ToString()));

                                }));

                            }
                            else
                            {
                                OutEdgeJSON["OutVertex"] = v.Id;
                                DelayedEdges.Add(OutEdgeJSON);
                            }
                        }
                        else
                            throw new Exception("Unknown EdgeLabel '" + EdgeProperties["Label"].Value<String>() + "'!");

                    });

                });

            }

            else
                throw new Exception("Unknown VertexLabel '" + VertexJSON["Label"].Value<String>() + "'!");

        }

        #endregion

        #region ParseEdge(...)

        public void ParseEdge(IGenericPropertyGraph<String, Int64, TVertexLabel,    String, Object,
                                                    String, Int64, TEdgeLabel,      String, Object,
                                                    String, Int64, TMultiEdgeLabel, String, Object,
                                                    String, Int64, THyperEdgeLabel, String, Object> graph, JToken EdgeJSON)
        {

            TEdgeLabel _EdgeLabel;

            String EnglishText;
            String GermanText;

            var EdgeProperties = EdgeJSON["Properties"];

            if (EdgeLabelParser(HttpUtility.UrlDecode(EdgeProperties["Label"].Value<String>()), out _EdgeLabel))
            {

                graph.AddEdge(HttpUtility.UrlDecode(EdgeProperties["Id"].Value<String>()),
                              graph.VertexById(HttpUtility.UrlDecode(EdgeJSON["OutVertex"].Value<String>())),
                              _EdgeLabel,
                              graph.VertexById(HttpUtility.UrlDecode(EdgeJSON["InVertex" ].Value<String>())),
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

                                          if (_E.HasValues)
                                          {

                                              EnglishText = _E["en"].Value<String>();
                                              GermanText  = _E["de"].Value<String>();

                                              e.SetProperty((_JToken as JProperty).Name, new I18N(new English(EnglishText),
                                                                                                  new German (GermanText)));

                                          }

                                          else
                                              e.SetProperty(HttpUtility.UrlDecode((_JToken as JProperty).Name),
                                                            HttpUtility.UrlDecode(_E.ToString()));

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

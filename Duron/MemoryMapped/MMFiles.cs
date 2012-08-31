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

    public class MemoryMappedFile<T> : IDisposable
        where T : struct
    {

        #region Data

        private MemoryMappedFile mmf;
        private MemoryMappedViewAccessor accessor;

        private UInt64  WindowOffset;

        private Mutex smLock;
        private Boolean locked;

        #endregion

        #region Properties

        /// <summary>
        /// The name of the underlying file.
        /// </summary>
        public String Filename    { get; private set; }

        public UInt32 WindowSize  { get; private set; }

        #endregion

        #region Constructor(s)

        public MemoryMappedFile(String Filename, UInt32 WindowSize, UInt32 Padding = 8)
        {
            this.Filename   = Filename;
            this.WindowSize = WindowSize;
        }

        #endregion



        



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



        



        public void Dispose()
        {
            mmf.Dispose();
        }

    }

}

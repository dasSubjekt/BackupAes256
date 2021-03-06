namespace BackupAes256.Model
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Collections.Generic;
    using System.Collections.Concurrent;


    /// <summary>This class performs all tasks that may take longer than a second. Running them in the main window Thread would freeze the user interface.</summary>
    public class BackgroundThread
    {
        /// <summary>Enumerated type with the execution states of <c>BackgroundThread</c>.</summary>
        public enum nState { Idle, Working, CancelRequested };

        private readonly object StateLock = new object();

        private nState _eState;
        private readonly byte[] _abCopyBuffer;
        private readonly TextConverter _TextConverter;
        private Thread _AsyncThread;
        private readonly CryptoServices _Cryptography;
        private readonly ConcurrentQueue<BackgroundMessage> _quCommands, _quReturn;


        #region constructors

        /// <summary>The constructor to initialize a <c>new BackgroundThread</c>.</summary>
        /// <param name=""></param>
        public BackgroundThread(CryptoServices Cryptography)
        {
            _Cryptography = Cryptography;
            _abCopyBuffer = new byte[PairOfFiles.ciBytesPerProgressUnit];
            _TextConverter = new TextConverter();
            _quCommands = new ConcurrentQueue<BackgroundMessage>();
            _quReturn = new ConcurrentQueue<BackgroundMessage>();
            Reset();
        }
        #endregion

        #region properties

        /// <summary></summary>
        public ConcurrentQueue<BackgroundMessage> quReturn
        {
            get { return _quReturn; }
        }

        /// <summary></summary>
        public nState eState
        {
            get { return _eState; }
            private set
            {
                lock (StateLock)
                {
                    _eState = value;
                }
            }
        }
        #endregion

        #region methods

        /// <summary></summary>
        private void AsynchronousThreadMethod()
        {
            while (_quCommands.TryDequeue(out BackgroundMessage UserInterfaceMessage))
            {
                switch (UserInterfaceMessage.eType)
                {
                    case BackgroundMessage.nType.Compare: ExecuteCompare(UserInterfaceMessage); break;
                    case BackgroundMessage.nType.Synchronize: ExecuteSynchronize(UserInterfaceMessage); break;
                    default: throw new NotImplementedException("command not implemented: " + UserInterfaceMessage.eType.ToString());
                }
                if (_eState == nState.CancelRequested)
                    Reset();
            }
            _quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.Stop));
            eState = nState.Idle;
            _AsyncThread = null;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private void Copy(PairOfFiles PairToCopy)
        {
            if (PairToCopy.isDirectory)
                CopyDirectory(PairToCopy);
            else
                CopyFile(PairToCopy);
        }


        /// <summary></summary>
        /// <param name=""></param>
        private void CopyDirectory(PairOfFiles PairToCopy)
        {
            if (PairToCopy.eComparison == PairOfFiles.nComparison.DestinationOnly)
            {
                PairToCopy.eComparison = PairOfFiles.nComparison.Error;
                PairToCopy.sErrorMessage = "No source directory to copy at '" + PairToCopy.sRelativePath + "'.";
            }
            else if (PairToCopy.DestinationDrive.eEncryptionType == Drive.nEncryptionType.DirectoryUnencrypted)
            {
                DirectoryInfo DirectoryInfoDestination = new DirectoryInfo(PairToCopy.sDestinationPath);

                try
                {
                    if (!DirectoryInfoDestination.Exists)
                        DirectoryInfoDestination.Create();
                }
                catch (Exception ex)
                {
                    PairToCopy.eComparison = PairOfFiles.nComparison.Error;
                    PairToCopy.sErrorMessage = ex.Message;
                }
                PairToCopy.CopyProperties();
                try
                {
                    DirectoryInfoDestination.Attributes = (FileAttributes)PairToCopy.uAttributesSource;
                    DirectoryInfoDestination.CreationTime = Directory.GetCreationTime(PairToCopy.sSourcePath);
                    DirectoryInfoDestination.LastAccessTime = Directory.GetLastAccessTime(PairToCopy.sSourcePath);
                    DirectoryInfoDestination.LastWriteTime = PairToCopy.LastWriteTimeSource;
                }
                catch { }
                PairToCopy.eComparison = PairOfFiles.nComparison.Identical;
                _quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.ReportProgress, PairToCopy));
            }
            else
            {
                throw new NotImplementedException("File system encryption is not yet implemented.");
            }
        }


        /// <summary></summary>
        /// <param name=""></param>
        private void CopyFile(PairOfFiles PairToCopy)
        {
            FileInfo FileInfoDestination;

            if (PairToCopy.eComparison == PairOfFiles.nComparison.DestinationOnly)
            {
                PairToCopy.eComparison = PairOfFiles.nComparison.Error;
                PairToCopy.sErrorMessage = "No source file to copy: '" + PairToCopy.sRelativePath + "'.";
            }
            else if (PairToCopy.DestinationDrive.eEncryptionType == Drive.nEncryptionType.DirectoryUnencrypted)
            {
                FileInfoDestination = new FileInfo(PairToCopy.sDestinationPath);
                try
                {
                    if (PairToCopy.SourceDrive.eEncryptionType == Drive.nEncryptionType.DirectoryUnencrypted)
                    {
                        if (PairToCopy.DestinationDrive.kFreeSpace < PairToCopy.kSourceSize)
                        {
                            PairToCopy.eComparison = PairOfFiles.nComparison.Error;
                            PairToCopy.sErrorMessage = "Zu wenig Speicherplatz auf dem Ziellaufwerk.";
                        }
                        else
                        {
                            using (FileStream SourceStream = new FileStream(PairToCopy.sSourcePath, FileMode.Open, FileAccess.Read))
                            {
                                using (FileStream DestinationStream = new FileStream(PairToCopy.sDestinationPath, FileMode.Create, FileAccess.Write))
                                    CopyWithProgress(SourceStream, DestinationStream);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    PairToCopy.eComparison = PairOfFiles.nComparison.Error;
                    PairToCopy.sErrorMessage = ex.Message;
                }

                if (_eState == nState.Working)
                {
                    if (PairToCopy.eComparison != PairOfFiles.nComparison.Error)
                    {
                        PairToCopy.CopyProperties();
                        try
                        {
                            FileInfoDestination.Attributes = (FileAttributes)PairToCopy.uAttributesSource;
                            FileInfoDestination.CreationTime = PairToCopy.CreationTimeSource;
                            FileInfoDestination.LastAccessTime = PairToCopy.LastAccessTimeSource;
                            FileInfoDestination.LastWriteTime = PairToCopy.LastWriteTimeSource;

                            // synchronize the write time of the containing directory back to its original value
                            // there is no point of resetting LastAccessTime as it also gets changed by reading access
                            if (PairToCopy.ParentDirectory != null)
                                Directory.SetLastWriteTime(PairToCopy.ParentDirectory.sDestinationPath, PairToCopy.ParentDirectory.LastWriteTimeDestination);
                        }
                        catch { }
                        PairToCopy.eComparison = PairOfFiles.nComparison.Identical;
                    }
                }
                else   // _eState == nState.CancelRequested
                {
                    try
                    {
                        if (FileInfoDestination.Exists)
                            FileInfoDestination.Delete();
                    }
                    catch (Exception ex)
                    {
                        PairToCopy.eComparison = PairOfFiles.nComparison.Error;
                        PairToCopy.sErrorMessage = ex.Message;
                    }
                    PairToCopy.eComparison = PairOfFiles.nComparison.Error;
                    PairToCopy.sErrorMessage = "Das Kopieren der Datei wurde vom Benutzer abgebrochen.";
                }
                _quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.ReportProgress, PairToCopy));
            }
            else
            {
                throw new NotImplementedException("File system encryption is not yet implemented.");
            }
        }


        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void CopyWithProgress(Stream SourceStream, Stream DestinationStream)
        {
            int iBytesRead;

            while (((iBytesRead = SourceStream.Read(_abCopyBuffer, 0, PairOfFiles.ciBytesPerProgressUnit)) > 0) && (_eState == nState.Working))
            {
                DestinationStream.Write(_abCopyBuffer, 0, iBytesRead);
                _quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.ReportProgress, 1));
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void CopyWithProgress(Stream SourceStream, Stream DestinationStream, long kBytesToCopy)
        {
            int iBytesRead, iBytesToRead;

            iBytesToRead = kBytesToCopy > PairOfFiles.ciBytesPerProgressUnit ? PairOfFiles.ciBytesPerProgressUnit : (int)kBytesToCopy;
            while ((iBytesToRead > 0) && ((iBytesRead = SourceStream.Read(_abCopyBuffer, 0, iBytesToRead)) > 0) && (_eState == nState.Working))
            {
                DestinationStream.Write(_abCopyBuffer, 0, iBytesRead);
                _quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.ReportProgress, 1));
                kBytesToCopy -= iBytesRead;
                iBytesToRead = kBytesToCopy > PairOfFiles.ciBytesPerProgressUnit ? PairOfFiles.ciBytesPerProgressUnit : (int)kBytesToCopy;
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        private void Delete(PairOfFiles PairToDelete)
        {
            if (PairToDelete.isDirectory)
                DeleteDirectory(PairToDelete);
            else
                DeleteFile(PairToDelete);

            _quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.ReportProgress, 1));
        }


        /// <summary></summary>
        /// <param name=""></param>
        private void DeleteDirectory(PairOfFiles PairToDelete)
        {
            if (PairToDelete.eComparison == PairOfFiles.nComparison.DestinationOnly)
            {
                if (PairToDelete.DestinationDrive.eEncryptionType == Drive.nEncryptionType.DirectoryUnencrypted)
                {
                    try
                    {
                        Directory.Delete(PairToDelete.sDestinationPath);
                        PairToDelete.eComparison = PairOfFiles.nComparison.Deleted;
                    }
                    catch (Exception ex)
                    {
                        PairToDelete.eComparison = PairOfFiles.nComparison.Error;
                        PairToDelete.sErrorMessage = ex.Message;
                    }
                }
            }
            else
            {
                PairToDelete.eComparison = PairOfFiles.nComparison.Error;
                PairToDelete.sErrorMessage = "Deleting directory '" + PairToDelete.sRelativePath + "' is not allowed.";
            }
        }


        /// <summary></summary>
        /// <param name=""></param>
        private void DeleteFile(PairOfFiles PairToDelete)
        {
            if (PairToDelete.eComparison == PairOfFiles.nComparison.DestinationOnly)
            {
                if (PairToDelete.DestinationDrive.eEncryptionType == Drive.nEncryptionType.DirectoryUnencrypted)
                {
                    try
                    {
                        File.Delete(PairToDelete.sDestinationPath);
                        PairToDelete.eComparison = PairOfFiles.nComparison.Deleted;
                    }
                    catch (Exception ex)
                    {
                        PairToDelete.eComparison = PairOfFiles.nComparison.Error;
                        PairToDelete.sErrorMessage = ex.Message;
                    }
                }
            }
            else
            {
                PairToDelete.eComparison = PairOfFiles.nComparison.Error;
                PairToDelete.sErrorMessage = "Deleting file '" + PairToDelete.sRelativePath + "' is not allowed.";
            }
        }


        /// <summary></summary>
        /// <param name=""></param>
        private void DestinationNewer(PairOfFiles PairToSynchronize)
        {
            switch (PairToSynchronize.eSynchronizationMode)
            {
                case PairOfFiles.nSynchronizationMode.NoDelete: Skip(PairToSynchronize); break;
                case PairOfFiles.nSynchronizationMode.WithDelete: Copy(PairToSynchronize); break;
                case PairOfFiles.nSynchronizationMode.TwoWay: PairToSynchronize.SwapSourceAndDestination(); Copy(PairToSynchronize); break;
            }
        }


        /// <summary></summary>
        /// <param name=""></param>
        private void DestinationOnly(PairOfFiles PairToSynchronize)
        {
            switch (PairToSynchronize.eSynchronizationMode)
            {
                case PairOfFiles.nSynchronizationMode.NoDelete: Skip(PairToSynchronize); break;
                case PairOfFiles.nSynchronizationMode.WithDelete: Delete(PairToSynchronize); break;
                case PairOfFiles.nSynchronizationMode.TwoWay: PairToSynchronize.SwapSourceAndDestination(); Copy(PairToSynchronize); break;
            }
        }


        /// <summary></summary>
        public void Dispose()
        {
            RequestCancel();
        }

        /// <summary></summary>
        /// <param name=""></param>
        public void Enqueue(BackgroundMessage BackgroundMessage)
        {
            _quCommands.Enqueue(BackgroundMessage);
        }


        /// <summary></summary>
        /// <param name=""></param>
        private void ExecuteCompare(BackgroundMessage UserInterfaceMessage)
        {
            List<PairOfFiles> ltKnownPairs = new List<PairOfFiles>();
            Drive SourceDrive = UserInterfaceMessage.PairProperty.SourceDrive;

            _quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.UserMessage, BackgroundMessage.nReturnCode.StartCompare));

            // first, find out as quickly as possible how many directories and files there are
            switch (SourceDrive.eEncryptionType)
            {
                case Drive.nEncryptionType.DirectoryUnencrypted: ReadSourceDirectories(UserInterfaceMessage.PairProperty, ltKnownPairs); break;
            }

            switch (UserInterfaceMessage.PairProperty.DestinationDrive.eEncryptionType)
            {
                case Drive.nEncryptionType.DirectoryUnencrypted: ReadDestinationDirectories(UserInterfaceMessage.PairProperty, ltKnownPairs); break;
            }
                
            if (_eState == nState.Working)
            {
                // now we know how many directories and files there are and can set up the progress bar
                _quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.SetupProgress, 0, ltKnownPairs.Count()));

                foreach (PairOfFiles FileInfoPair in ltKnownPairs)
                {
                    if (_eState == nState.Working)
                    {   // we can report progess to the user, now do the time-consuming reading of the file size, attributes and dates:
                        FileInfoPair.ReadProperties();
                        _quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.NewPair, FileInfoPair));   // send the PairOfFiles to the MainViewModel
                    }
                }
            }
            _quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.UserMessage, BackgroundMessage.nReturnCode.FinishCompare));
        }

        /// <summary></summary>
        /// <param name=""></param>
        private void ExecuteSynchronize(BackgroundMessage UserInterfaceMessage)
        {
            if (UserInterfaceMessage.PairProperty.eComparison != PairOfFiles.nComparison.Identical)
            {
                UserInterfaceMessage.eType = BackgroundMessage.nType.Status;
                _quReturn.Enqueue(UserInterfaceMessage);
            }

            switch (UserInterfaceMessage.PairProperty.eComparison)
            {
                case PairOfFiles.nComparison.SourceOnly:
                case PairOfFiles.nComparison.SourceNewer: Copy(UserInterfaceMessage.PairProperty); break;
                case PairOfFiles.nComparison.DestinationOnly: DestinationOnly(UserInterfaceMessage.PairProperty); break;
                case PairOfFiles.nComparison.DestinationNewer: DestinationNewer(UserInterfaceMessage.PairProperty); break;
                case PairOfFiles.nComparison.Identical: UserInterfaceMessage.PairProperty.DestinationDrive.AddPair(UserInterfaceMessage.PairProperty); break;
            }
        }


        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void ReadDestinationDirectories(PairOfFiles ParentDirectory, List<PairOfFiles> ltPairsRead)
        {
            PairOfFiles FoundPair, NewPair;

            if (_eState == nState.Working)   // test if cancel was requested
            {
                _quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.Status, ParentDirectory.sDestinationPath));
                ReadDestinationFiles(ParentDirectory, ltPairsRead);   // read all files in this directory

                try
                {
                    foreach (string DirectoryName in Directory.GetDirectories(ParentDirectory.sDestinationPath))
                    {
                        NewPair = new PairOfFiles(ParentDirectory, ParentDirectory.DestinationDrive.RemoveRootPath(DirectoryName), PairOfFiles.nComparison.UnknownSource, true, _TextConverter);
                        FoundPair = ltPairsRead.SingleOrDefault(p => (p.sRelativePath == NewPair.sRelativePath) && p.isDirectory);

                        if (FoundPair == null)   // there is no source directory with this relative path
                        {
                            ltPairsRead.Add(NewPair);
                            if (NewPair.eComparison != PairOfFiles.nComparison.Error)
                            {
                                NewPair.eComparison = PairOfFiles.nComparison.DestinationOnly;
                                ReadDestinationDirectories(NewPair, ltPairsRead);     // recurse to the next level and search for directories, files and access errors there
                            }
                        }
                        else if (NewPair.eComparison == PairOfFiles.nComparison.Error)
                            FoundPair.eComparison = NewPair.eComparison;
                        else
                        {
                            FoundPair.eComparison = PairOfFiles.nComparison.BothExist;
                            ReadDestinationDirectories(FoundPair, ltPairsRead);   // recurse to the next level and search for directories, files and access errors there
                        }
                    }
                }
                catch (Exception ex)   // if something goes wrong, we still have the parent pair to store the error message for us
                {
                    ParentDirectory.eComparison = PairOfFiles.nComparison.Error;
                    ParentDirectory.sErrorMessage = ex.Message;
                }
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void ReadDestinationFiles(PairOfFiles ParentDirectory, List<PairOfFiles> ltPairsRead)
        {
            PairOfFiles FoundPair, NewPair;

            try
            {
                foreach (string FileName in Directory.GetFiles(ParentDirectory.sDestinationPath))
                {
                    NewPair = new PairOfFiles(ParentDirectory, ParentDirectory.DestinationDrive.RemoveRootPath(FileName), PairOfFiles.nComparison.UnknownSource, false, _TextConverter);
                    FoundPair = ltPairsRead.SingleOrDefault(p => (p.sRelativePath == NewPair.sRelativePath) && !p.isDirectory);

                    if (FoundPair == null)   // there is no source file with this relative path
                    {
                        ltPairsRead.Add(NewPair);
                        if (NewPair.eComparison != PairOfFiles.nComparison.Error)
                            NewPair.eComparison = PairOfFiles.nComparison.DestinationOnly;
                    }
                    else if (NewPair.eComparison == PairOfFiles.nComparison.Error)
                        FoundPair.eComparison = NewPair.eComparison;
                    else
                        FoundPair.eComparison = PairOfFiles.nComparison.BothExist;
                }
            }
            catch (Exception ex)
            {
                ParentDirectory.eComparison = PairOfFiles.nComparison.Error;
                ParentDirectory.sErrorMessage = ex.Message;
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void ReadSourceDirectories(PairOfFiles ParentDirectory, List<PairOfFiles> ltPairsRead)
        {
            PairOfFiles NewPair;

            if (_eState == nState.Working)   // test if cancel was requested
            {
                _quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.Status, ParentDirectory.sSourcePath));
                ReadSourceFiles(ParentDirectory, ltPairsRead);   // read all files in this directory

                try
                {
                    foreach (string DirectoryName in Directory.GetDirectories(ParentDirectory.sSourcePath))
                    {
                        NewPair = new PairOfFiles(ParentDirectory, ParentDirectory.SourceDrive.RemoveRootPath(DirectoryName), PairOfFiles.nComparison.UnknownDestination, true, _TextConverter);
                        ltPairsRead.Add(NewPair);
                        if (NewPair.eComparison != PairOfFiles.nComparison.Error)
                            ReadSourceDirectories(NewPair, ltPairsRead);   // recurse to the next level and search for directories, files and access errors there
                    }
                }
                catch (Exception ex)   // if something goes wrong, we still have the parent pair to store the error message for us
                {
                    ParentDirectory.eComparison = PairOfFiles.nComparison.Error;
                    ParentDirectory.sErrorMessage = ex.Message;
                }
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void ReadSourceFiles(PairOfFiles ParentDirectory, List<PairOfFiles> ltPairsRead)
        {
            PairOfFiles NewPair;

            try
            {
                foreach (string FileName in Directory.GetFiles(ParentDirectory.sSourcePath))
                {
                    NewPair = new PairOfFiles(ParentDirectory, ParentDirectory.SourceDrive.RemoveRootPath(FileName), PairOfFiles.nComparison.UnknownDestination, false, _TextConverter);
                    ltPairsRead.Add(NewPair);
                }
            }
            catch (Exception ex)
            {
                ParentDirectory.eComparison = PairOfFiles.nComparison.Error;
                ParentDirectory.sErrorMessage = ex.Message;
            }
        }

        /// <summary></summary>
        public void RequestCancel()
        {
            eState = nState.CancelRequested;
        }

        /// <summary>Reset all variables.</summary>
        private void Reset()
        {
#pragma warning disable IDE0059   // Suppres warnings that the value assigned to variable is never used
            if (!_quCommands.IsEmpty)
                while (_quCommands.TryDequeue(out BackgroundMessage MessageToDiscard)) ;

            if (!_quReturn.IsEmpty)
                while (_quReturn.TryDequeue(out BackgroundMessage MessageToDiscard)) ;
#pragma warning restore IDE0059   // Value assigned to variable is never used

            eState = nState.Idle;
            _AsyncThread = null;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private void Skip(PairOfFiles PairToSkip)
        {
            // PairToSkip.eComparison = PairOfFiles.nComparison.Skipped;
            PairToSkip.DestinationDrive.AddPair(PairToSkip);
        }


        /// <summary></summary>
        public bool Start()
        {
            if ((_eState != nState.Idle) || _quCommands.IsEmpty)
            {
                return false;
            }
            else
            {
                // Task t = new Task(() => { _Cryptography.FillKey(new CryptoKey("", CryptoKey.nKeyFormat.KeePass, CryptoKey.nKeyType.Symmetric, 32)); });
                // t.Start();

                eState = nState.Working;
                _AsyncThread = new Thread(() => AsynchronousThreadMethod());
                _AsyncThread.Start();
                return true;
            }
        }


        /// <summary></summary>
        /// <param name=""></param>
        public bool Start(BackgroundMessage BackgroundMessage)
        {
            _quCommands.Enqueue(BackgroundMessage);
            return Start();
        }
        #endregion
    }
}

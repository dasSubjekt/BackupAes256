namespace BackupAes256.Model
{
    using System;
    using System.IO;


    /// <summary>A pair of files to be synchronized.</summary>
    public class PairOfFiles : IEquatable<PairOfFiles>
    {
        public const int ciBytesPerProgressUnit = 0x1000000;   // one unit on the progress bar represents 16 megabytes

        private const long ckTicksNewYear1969 = 621040608000000000L;
        private const long ckTicksInOneSecond = 10000000L;
        private const long ckTicksInTwoSeconds = 20000000L;
        private const long ckTicksInOneHour = 36000000000L;
        private const long ckOneKB = 0x400L;
        private const long ckOneMB = 0x100000L;
        private const long ckOneGB = 0x40000000L;
        private const long ckOneTB = 0x10000000000L;

        public const int ciMinimumAttributesLength = 30;   // The minimum number of bytes written by WriteSourceAttributes() and WriteDestinationAttributes().
        public const int ciExtraFileAttributesLength = 8;   // The number of bytes needed to store the size of a file. A directory has no size and does not need this.

        public readonly string[] casSkipDirectories = { "$RECYCLE.BIN", "System Volume Information" };


        /// <summary>Enumerated type of possible relations within the <c>PairOfFiles</c>. The predefined numbers are for <c>TabControl.SelectedIndex</c>.</summary>
        public enum nComparison { SourceOnly = 0, DestinationOnly = 1, SourceNewer = 2, DestinationNewer = 3, Identical = 4, Error = 5, UnknownSource, UnknownDestination, BothExist, Deleted };
        public enum nSynchronizationMode { NoDelete, WithDelete, TwoWay };

        private readonly object ComparisonLock = new object();

        private bool _isEncryptedDestination, _isEncryptedSource;
        private readonly bool _isDirectory;
        private uint _uAttributesDestination, _uAttributesSource, _uFirstBlockDestination, _uFirstBlockSource, _uLastWriteTimeDestination, _uLastWriteTimeSource;
        private long _kDestinationSize, _kSourceSize;
        private string _sErrorMessage, _sLastWriteTimeDestination, _sLastWriteTimeSource, _sRelativePath, _sDestinationSize, _sSourceSize;
        private nComparison _eComparison;
        private readonly TextConverter _TextConverter;
        private nSynchronizationMode _eSynchronizationMode;
        private Drive _SourceDrive, _DestinationDrive;
        private DateTime _CreationTimeDestination, _CreationTimeSource, _LastAccessTimeDestination, _LastAccessTimeSource, _LastWriteTimeDestination, _LastWriteTimeSource;
        private PairOfFiles _ParentDirectory;


        #region constructors

        /// <summary>A constructor to initialize a <c>new PairOfFiles</c>.</summary>
        protected PairOfFiles()
        {
            _isDirectory = _isEncryptedDestination = _isEncryptedSource = false;
            _uAttributesDestination = _uAttributesSource = _uFirstBlockDestination = _uFirstBlockSource = _uLastWriteTimeDestination = _uLastWriteTimeSource = 0;
            _kDestinationSize = _kSourceSize = -1;
            _sErrorMessage = _sLastWriteTimeDestination = _sLastWriteTimeSource = _sRelativePath = _sDestinationSize = _sSourceSize = string.Empty;
            _eComparison = nComparison.Error;
            _TextConverter = null;
            _eSynchronizationMode = nSynchronizationMode.WithDelete;
            _SourceDrive = _DestinationDrive = null;
            _ParentDirectory = null;
            _CreationTimeDestination = _CreationTimeSource = _LastAccessTimeDestination = _LastAccessTimeSource = _LastWriteTimeDestination = _LastWriteTimeSource = DateTime.MinValue;
        }

        /// <summary>A constructor to initialize a <c>new PairOfFiles</c>.</summary>
        /// <param name="SourceDrive"></param>
        /// <param name="DestinationDrive"></param>
        public PairOfFiles(Drive SourceDrive, Drive DestinationDrive) : this()
        {
            _SourceDrive = SourceDrive;
            _DestinationDrive = DestinationDrive;
        }

        /// <summary>A constructor to initialize a <c>new PairOfFiles</c>.</summary>
        /// <param name="FromStream"></param>
        /// <param name="SourceOrDestinationDrive"></param>
        /// <param name="TextEncoder"></param>
        public PairOfFiles(Stream FromStream, PairOfFiles ParentDirectory, bool isSource, TextConverter TextConverter) : this()
        {
            byte[] abBuffer, abTextBytes;
            ushort tTextLength;
            uint uAttributes, uFirstBlock = 0;
            long kCreationTimeTicks, kLastAccessTimeTicks, kLastWriteTimeTicks, kSize = -1;

            _ParentDirectory = ParentDirectory;
            _SourceDrive = _ParentDirectory.SourceDrive;
            _DestinationDrive = _ParentDirectory.DestinationDrive;
            _TextConverter = TextConverter;
            abBuffer = new byte[8];

            FromStream.Read(abBuffer, 0, 2);
            tTextLength = BitConverter.ToUInt16(abBuffer, 0);
            abTextBytes = new byte[tTextLength];
            FromStream.Read(abTextBytes, 0, tTextLength);
            _sRelativePath = _TextConverter.BytesToString(abTextBytes);

            FromStream.Read(abBuffer, 0, 4);
            uAttributes = BitConverter.ToUInt32(abBuffer, 0);
            _isDirectory = ((uAttributes & (uint)FileAttributes.Directory) > 0);
            FromStream.Read(abBuffer, 0, 8);
            kCreationTimeTicks = BitConverter.ToInt64(abBuffer, 0);
            FromStream.Read(abBuffer, 0, 8);
            kLastAccessTimeTicks = BitConverter.ToInt64(abBuffer, 0);
            FromStream.Read(abBuffer, 0, 8);
            kLastWriteTimeTicks = BitConverter.ToInt64(abBuffer, 0);

            if (!_isDirectory)
            {
                FromStream.Read(abBuffer, 0, 8);
                kSize = BitConverter.ToInt64(abBuffer, 0);
                if ((isSource && (_SourceDrive.eEncryptionType == Drive.nEncryptionType.DirectorySymmetric)) || (!isSource && (_DestinationDrive.eEncryptionType == Drive.nEncryptionType.DirectorySymmetric)))
                {
                    FromStream.Read(abBuffer, 0, 4);
                    uFirstBlock = BitConverter.ToUInt32(abBuffer, 0);
                }
            }

            if (isSource)
            {
                _uAttributesSource = uAttributes;
                _CreationTimeSource = new DateTime(kCreationTimeTicks);
                _LastAccessTimeSource = new DateTime(kLastAccessTimeTicks);
                LastWriteTimeSource = new DateTime(kLastWriteTimeTicks);
                _kSourceSize = kSize;
                _sSourceSize = FileSizeToString(kSize);
                _uFirstBlockSource = uFirstBlock;
                _eComparison = nComparison.UnknownDestination;
            }
            else
            {
                _uAttributesDestination = uAttributes;
                _CreationTimeDestination = new DateTime(kCreationTimeTicks);
                _LastAccessTimeDestination = new DateTime(kLastAccessTimeTicks);
                LastWriteTimeDestination = new DateTime(kLastWriteTimeTicks);
                _kDestinationSize = kSize;
                _sDestinationSize = FileSizeToString(kSize);
                _uFirstBlockDestination = uFirstBlock;
                _eComparison = nComparison.UnknownSource;
            }
        }


        /// <summary>A constructor to initialize a <c>new PairOfFiles</c>.</summary>
        /// <param name="SourceDrive"></param>
        /// <param name="DestinationDrive"></param>
        /// <param name="sRelativePath">The relative 'end' path is identical for both files, even though they are stored in different directories.</param>
        /// <param name="eComparison">Type of synchronization that needs to be performed within this <c>PairOfFiles</c>.</param>
        /// <param name="isDirectory">True if this is a pair of directories. Technically, a directory is a special case of a file.</param>
        public PairOfFiles(PairOfFiles ParentDirectory, string sRelativePath, nComparison eComparison, bool isDirectory, TextConverter TextConverter) : this()
        {
            _ParentDirectory = ParentDirectory;
            _SourceDrive = _ParentDirectory.SourceDrive;
            _DestinationDrive = _ParentDirectory.DestinationDrive;
            _eComparison = eComparison;
            _isDirectory = isDirectory;
            _TextConverter = TextConverter;

            if (string.IsNullOrEmpty(sRelativePath))
                throw new ArgumentException("Relative path must not be empty.");
            else if ((sRelativePath[0] == '\\') || (sRelativePath[0] == '/'))
                throw new ArgumentException("Relative path '" + sRelativePath + "' must not start with a slash.");
            else
                _sRelativePath = sRelativePath;

            foreach (string sDirectory in casSkipDirectories)
            {
                if ((_sRelativePath.Length >= sDirectory.Length) && (_sRelativePath.Substring(0, sDirectory.Length) == sDirectory))
                {
                    _eComparison = nComparison.Error;
                    _sErrorMessage = "This directory is excluded by default.";
                }
            }
        }
        #endregion

        #region operators

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public static bool operator ==(PairOfFiles First, PairOfFiles Second)
        {
            if (((object)First) == null || ((object)Second) == null)
                return Equals(First, Second);
            else
                return First.Equals(Second);
        }

        /// <summary></summary>
        /// <param name="First"></param>
        /// <param name="Second"></param>
        public static bool operator !=(PairOfFiles First, PairOfFiles Second)
        {
            if (((object)First) == null || ((object)Second) == null)
                return !Equals(First, Second);
            else
                return !(First.Equals(Second));
        }

        #endregion

        #region properties

        /// <summary></summary>
        public uint uAttributesDestination
        {
            get { return _uAttributesDestination; }
            set { _uAttributesDestination = value; }
        }

        /// <summary></summary>
        public uint uAttributesSource
        {
            get { return _uAttributesSource; }
            set { _uAttributesSource = value; }
        }

        /// <summary>Comparison result determining the type of synchronization that needs to be performed on this <c>PairOfFiles</c>.</summary>
        public nComparison eComparison
        {
            get { return _eComparison; }
            set
            {
                lock (ComparisonLock)
                {
                    _eComparison = value;
                }
            }
        }

        /// <summary></summary>
        public DateTime CreationTimeDestination
        {
            get { return _CreationTimeDestination; }
        }
        
        /// <summary></summary>
        public DateTime CreationTimeSource
        {
            get { return _CreationTimeSource; }
        }

        /// <summary></summary>
        public Drive DestinationDrive
        {
            get { return _DestinationDrive; }
        }

        /// <summary></summary>
        public string sDestinationPath
        {
            get
            {
                if ((_DestinationDrive.iLevelsInRootPath == 1) || string.IsNullOrEmpty(_sRelativePath))
                    return _DestinationDrive.sRootPath + _sRelativePath;
                else
                    return _DestinationDrive.sRootPath + _DestinationDrive.cDirectorySeparator + _sRelativePath;
            }
        }

        /// <summary></summary>
        public long kDestinationSize
        {
            get { return _kDestinationSize; }
            set { _kDestinationSize = value; }
        }

        /// <summary></summary>
        public string sDestinationSize
        {
            get
            {
                if (!isWithDestination)
                    return string.Empty;
                else if (_isDirectory)
                    return "Verzeichnis";
                else
                    return _sDestinationSize;
            }
        }

        /// <summary>True if this represents a a pair of directories, false if it represents a pair of files.</summary>
        public bool isDirectory
        {
            get { return _isDirectory; }
        }

        /// <summary></summary>
        public bool isEncryptedDestination
        {
            get { return _isEncryptedDestination; }
        }

        /// <summary></summary>
        public bool isEncryptedSource
        {
            get { return _isEncryptedSource; }
        }

        /// <summary></summary>
        public string sErrorMessage
        {
            get { return _sErrorMessage; }
            set { _sErrorMessage = value; }
        }

        /// <summary></summary>
        public uint uFirstBlockDestination
        {
            get { return _uFirstBlockDestination; }
            set { _uFirstBlockDestination = value; }
        }

        /// <summary></summary>
        public uint uFirstBlockSource
        {
            get { return _uFirstBlockSource; }
            set { _uFirstBlockSource = value; }
        }

        /// <summary></summary>
        public DateTime LastAccessTimeDestination
        {
            get { return _LastAccessTimeDestination; }
        }
        
        /// <summary></summary>
        public DateTime LastAccessTimeSource
        {
            get { return _LastAccessTimeSource; }
        }

        /// <summary></summary>
        public DateTime LastWriteTimeDestination
        {
            get { return _LastWriteTimeDestination; }
            set
            {
                _LastWriteTimeDestination = value;
                _uLastWriteTimeDestination = DateTimeToUint(value);
                _sLastWriteTimeDestination = DateTimeToString(value);
            }
        }

        /// <summary></summary>
        public string sLastWriteTimeDestination
        {
            get { return _sLastWriteTimeDestination; }
        }

        /// <summary></summary>
        public uint uLastWriteTimeDestination
        {
            get { return _uLastWriteTimeDestination; }
        }

        /// <summary></summary>
        public DateTime LastWriteTimeSource
        {
            get { return _LastWriteTimeSource; }
            set
            {
                _LastWriteTimeSource = value;
                _uLastWriteTimeSource = DateTimeToUint(value);
                _sLastWriteTimeSource = DateTimeToString(value);
            }
        }

        /// <summary></summary>
        public string sLastWriteTimeSource
        {
            get { return _sLastWriteTimeSource; }
        }

        /// <summary></summary>
        public uint uLastWriteTimeSource
        {
            get { return _uLastWriteTimeSource; }
        }

        /// <summary></summary>
        public int iMaximumProgress
        {
            get
            {
                int iReturn = 0;

                switch (_eComparison)
                {
                    case nComparison.SourceOnly:
                    case nComparison.SourceNewer: iReturn = iMaximumProgressSource; break;
                    case nComparison.DestinationOnly:
                                                        switch (_eSynchronizationMode)
                                                        {
                                                            case nSynchronizationMode.NoDelete: iReturn = 0; break;
                                                            case nSynchronizationMode.WithDelete: iReturn = 1; break;
                                                            case nSynchronizationMode.TwoWay: iReturn = iMaximumProgressDestination; break;
                                                        }
                                                        break;
                    case nComparison.DestinationNewer:
                                                        switch (_eSynchronizationMode)
                                                        {
                                                            case nSynchronizationMode.NoDelete: iReturn = 0; break;
                                                            case nSynchronizationMode.WithDelete: iReturn = iMaximumProgressSource; break;
                                                            case nSynchronizationMode.TwoWay: iReturn = iMaximumProgressDestination; break;
                                                        }
                                                        break;
                }
                return iReturn;
            }
        }

        /// <summary></summary>
        private int iMaximumProgressDestination
        {
            get
            {
                if (_isDirectory)
                    return 1;
                else if (_kDestinationSize < 0)
                    return 0;
                else
                    return (int)(_kDestinationSize / ciBytesPerProgressUnit) + 1;
            }
        }

        /// <summary></summary>
        private int iMaximumProgressSource
        {
            get
            {
                if (_isDirectory)
                    return 1;
                else if (_kSourceSize < 0)
                    return 0;
                else
                    return (int)(_kSourceSize / ciBytesPerProgressUnit) + 1;
            }
        }

        /// <summary></summary>
        public PairOfFiles ParentDirectory
        {
            get { return _ParentDirectory; }
            set { _ParentDirectory = value; }
        }

        /// <summary></summary>
        public string sParentDirectoryPath
        {
            get
            {
                int iLastIndexOfDirectorySeparator = _sRelativePath.LastIndexOf(_SourceDrive.cDirectorySeparator);

                if (iLastIndexOfDirectorySeparator > 0)
                    return _sRelativePath.Substring(0, iLastIndexOfDirectorySeparator);
                else
                    return string.Empty;
            }
        }

        /// <summary>Independent of drive and identical for both files.</summary>
        public string sRelativePath
        {
            get { return _sRelativePath; }
        }

        /// <summary></summary>
        public byte[] abRelativePathBytes
        {
            get { return _TextConverter?.StringToBytes(_sRelativePath); }
        }

        /// <summary></summary>
        public Drive SourceDrive
        {
            get { return _SourceDrive; }
        }

        /// <summary></summary>
        public string sSourcePath
        {
            get
            {
                if ((_SourceDrive.iLevelsInRootPath == 1) || string.IsNullOrEmpty(_sRelativePath))
                    return _SourceDrive.sRootPath + _sRelativePath;
                else
                    return _SourceDrive.sRootPath + _SourceDrive.cDirectorySeparator + _sRelativePath;
            }
        }

        /// <summary></summary>
        public long kSourceSize
        {
            get { return _kSourceSize; }
            set { _kSourceSize = value; }
        }

        /// <summary></summary>
        public string sSourceSize
        {
            get
            {
                if (!isWithSource)
                    return string.Empty;
                else if (_isDirectory)
                    return "Verzeichnis";
                else
                    return _sSourceSize;
            }
        }

        /// <summary></summary>
        public nSynchronizationMode eSynchronizationMode
        {
            get { return _eSynchronizationMode; }
            set { _eSynchronizationMode = value; }
        }

        /// <summary>True if the destination directory or file exists.</summary>
        public bool isWithDestination
        {
            get { return (_eComparison == nComparison.DestinationOnly) || (_eComparison == nComparison.SourceNewer) || (_eComparison == nComparison.DestinationNewer) || (_eComparison == nComparison.Identical) || (_eComparison == nComparison.UnknownSource) || (_eComparison == nComparison.BothExist); }
        }

        /// <summary>True if the source directory or file exists.</summary>
        public bool isWithSource
        {
            get { return (_eComparison == nComparison.SourceOnly) || (_eComparison == nComparison.SourceNewer) || (_eComparison == nComparison.DestinationNewer) || (_eComparison == nComparison.Identical) || (_eComparison == nComparison.UnknownDestination) || (_eComparison == nComparison.BothExist); }
        }

        /// <summary>True if both the source and destination directory or file exist.</summary>
        public bool isWithSourceAndDestination
        {
            get { return (_eComparison == nComparison.SourceNewer) || (_eComparison == nComparison.DestinationNewer) || (_eComparison == nComparison.Identical) || (_eComparison == nComparison.BothExist); }
        }
        #endregion

        #region methods

        /// <summary></summary>
        private void CompareLastWriteTimes()
        {
            long kTicksDiff, kTicksDiffOneHourOff;

            if (isWithSourceAndDestination)
            {
                kTicksDiff = _LastWriteTimeSource.Ticks - _LastWriteTimeDestination.Ticks;
                kTicksDiffOneHourOff = 0;

                if ((kTicksDiff >= -ckTicksInTwoSeconds) && (kTicksDiff <= ckTicksInTwoSeconds))
                    kTicksDiff = 0;   // if the time difference is two seconds or less, this is probably a storage artefact to be ignored

                if (kTicksDiff > 0)
                    kTicksDiffOneHourOff = kTicksDiff - ckTicksInOneHour;
                else if (kTicksDiff < 0)
                    kTicksDiffOneHourOff = kTicksDiff + ckTicksInOneHour;

                if ((kTicksDiffOneHourOff >= -ckTicksInTwoSeconds) && (kTicksDiffOneHourOff <= ckTicksInTwoSeconds))
                    kTicksDiff = 0;   // if the time difference is exactly one hour, this is probably due to summer vs. winter time, so we ignore it

                if (kTicksDiff < 0)
                    eComparison = nComparison.DestinationNewer;
                else if (kTicksDiff > 0)
                    eComparison = nComparison.SourceNewer;
                else
                    eComparison = nComparison.Identical;
            }
        }

        /// <summary></summary>
        public void CopyProperties()
        {
            _uAttributesDestination = _uAttributesSource;
            _CreationTimeDestination = _CreationTimeSource;
            _LastAccessTimeDestination = _LastAccessTimeSource;
            _LastWriteTimeDestination = _LastWriteTimeSource;
            _uLastWriteTimeDestination = _uLastWriteTimeSource;
            _sLastWriteTimeDestination = _sLastWriteTimeSource;
            _kDestinationSize = _kSourceSize;
            _sDestinationSize = _sSourceSize;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private string DateTimeToString(DateTime DateTimeValue)
        {
            return (DateTimeValue.Ticks < ckTicksNewYear1969 ? "???" : DateTimeValue.ToString("dd.MM.yyyy HH:mm:ss"));
        }

        /// <summary></summary>
        /// <param name=""></param>
        protected uint DateTimeToUint(DateTime DateTimeValue)
        {
            long kTicks = DateTimeValue.Ticks;

            if (kTicks < ckTicksNewYear1969)
                return 0;
            else
                return (uint)((kTicks - ckTicksNewYear1969) / ckTicksInOneSecond);
        }

        /// <summary></summary>
        /// <param name=""></param>
        public bool Equals(PairOfFiles Other)
        {
            return (Other != null) && (_sRelativePath == Other.sRelativePath) && (_isDirectory == Other.isDirectory);
        }

        /// <summary></summary>
        /// <param name=""></param>
        public override bool Equals(object Other)
        {
            if (Other == null)
                return false;
            else
            {
                PairOfFiles OtherPair = Other as PairOfFiles;
                if (OtherPair == null)
                    return false;
                else
                    return Equals(OtherPair);
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        private string FileSizeToString(long kSize)
        {
            if (kSize < 0)
                return "???";
            else if (kSize < ckOneKB)
                return String.Format("{0:d}   B", kSize);
            else if (kSize < ckOneMB)
                return String.Format("{0:f1} KB", (double)kSize / (double)ckOneKB);
            else if (kSize < ckOneGB)
                return String.Format("{0:f1} MB", (double)kSize / (double)ckOneMB);
            else if (kSize < ckOneTB)
                return String.Format("{0:f1} GB", (double)kSize / (double)ckOneGB);
            else
                return String.Format("{0:f1} TB", (double)kSize / (double)ckOneTB);
        }

        /// <summary></summary>
        public override int GetHashCode()
        {
            return _sRelativePath.GetHashCode();
        }

        /// <summary></summary>
        private void ReadDirectoryProperties()
        {
            if (isWithSource)
            {
                if (_SourceDrive.eEncryptionType == Drive.nEncryptionType.DirectoryUnencrypted)
                {
                    DirectoryInfo DirectoryInfoSource = new DirectoryInfo(sSourcePath);

                    if (DirectoryInfoSource.Exists)
                    {
                        try
                        {
                            _uAttributesSource = (uint)DirectoryInfoSource.Attributes;
                            _CreationTimeSource = DirectoryInfoSource.CreationTime;
                            _LastAccessTimeSource = DirectoryInfoSource.LastAccessTime;
                            _LastWriteTimeSource = DirectoryInfoSource.LastWriteTime;
                            _uLastWriteTimeSource = DateTimeToUint(_LastWriteTimeSource);
                            _sLastWriteTimeSource = DateTimeToString(_LastWriteTimeSource);

                            if (_isDirectory != (_uAttributesSource & (uint)FileAttributes.Directory) > 0)
                            {
                                _eComparison = PairOfFiles.nComparison.Error;
                                _sErrorMessage = "Contradictory informaton on whether this is a directory or a file.";
                            }
                        }
                        catch
                        {
                            _eComparison = PairOfFiles.nComparison.Error;
                            _sErrorMessage = "Could not read properties of source directory.";
                        }
                    }
                    else
                    {
                        _eComparison = PairOfFiles.nComparison.Error;
                        _sErrorMessage = "Source directory not found.";
                    }
                }
            }
            else if (_eComparison != nComparison.Error)
                eComparison = nComparison.DestinationOnly;

            if (isWithDestination)
            {
                if (_DestinationDrive.eEncryptionType == Drive.nEncryptionType.DirectoryUnencrypted)
                {
                    DirectoryInfo DirectoryInfoDestination = new DirectoryInfo(sDestinationPath);

                    if (DirectoryInfoDestination.Exists)
                    {
                        try
                        {
                            _uAttributesDestination = (uint)DirectoryInfoDestination.Attributes;
                            _CreationTimeDestination = DirectoryInfoDestination.CreationTime;
                            _LastAccessTimeDestination = DirectoryInfoDestination.LastAccessTime;
                            _LastWriteTimeDestination = DirectoryInfoDestination.LastWriteTime;
                            _uLastWriteTimeDestination = DateTimeToUint(_LastWriteTimeDestination);
                            _sLastWriteTimeDestination = DateTimeToString(_LastWriteTimeDestination);

                            if (_isDirectory != (_uAttributesDestination & (uint)FileAttributes.Directory) > 0)
                            {
                                _eComparison = PairOfFiles.nComparison.Error;
                                _sErrorMessage = "Contradictory informaton on whether this is a directory or a file.";
                            }
                        }
                        catch
                        {
                            _eComparison = PairOfFiles.nComparison.Error;
                            _sErrorMessage = "Could not read properties of destination directory.";
                        }
                    }
                    else
                    {
                        _eComparison = PairOfFiles.nComparison.Error;
                        _sErrorMessage = "Destination directory not found.";
                    }
                }
            }
            else if (_eComparison != nComparison.Error)
                eComparison = nComparison.SourceOnly;

            CompareLastWriteTimes();
        }

        /// <summary></summary>
        private void ReadFileProperties()
        {
            if (isWithSource)
            {
                if (_SourceDrive.eEncryptionType == Drive.nEncryptionType.DirectoryUnencrypted)
                {
                    FileInfo FileInfoSource = new FileInfo(sSourcePath);

                    if (FileInfoSource.Exists)
                    {
                        try
                        {
                            _uAttributesSource = (uint)FileInfoSource.Attributes;
                            _kSourceSize = FileInfoSource.Length;
                            _sSourceSize = FileSizeToString(_kSourceSize);
                            _CreationTimeSource = FileInfoSource.CreationTime;
                            _LastAccessTimeSource = FileInfoSource.LastAccessTime;
                            _LastWriteTimeSource = FileInfoSource.LastWriteTime;
                            _uLastWriteTimeSource = DateTimeToUint(_LastWriteTimeSource);
                            _sLastWriteTimeSource = DateTimeToString(_LastWriteTimeSource);

                            if (_isDirectory != (_uAttributesSource & (uint)FileAttributes.Directory) > 0)
                            {
                                _eComparison = PairOfFiles.nComparison.Error;
                                _sErrorMessage = "Contradictory informaton on whether this is a directory or a file.";
                            }
                        }
                        catch
                        {
                            _eComparison = PairOfFiles.nComparison.Error;
                            _sErrorMessage = "Could not read properties of source file.";
                        }
                    }
                    else
                    {
                        _eComparison = PairOfFiles.nComparison.Error;
                        _sErrorMessage = "Source file not found.";
                    }
                }
            }
            else if (_eComparison != nComparison.Error)
                eComparison = nComparison.DestinationOnly;

            if (isWithDestination)
            {
                if (_DestinationDrive.eEncryptionType == Drive.nEncryptionType.DirectoryUnencrypted)
                {
                    FileInfo FileInfoDestination = new FileInfo(sDestinationPath);

                    if (FileInfoDestination.Exists)
                    {
                        try
                        {
                            _uAttributesDestination = (uint)FileInfoDestination.Attributes;
                            _kDestinationSize = FileInfoDestination.Length;
                            _sDestinationSize = FileSizeToString(_kDestinationSize);
                            _CreationTimeDestination = FileInfoDestination.CreationTime;
                            _LastAccessTimeDestination = FileInfoDestination.LastAccessTime;
                            _LastWriteTimeDestination = FileInfoDestination.LastWriteTime;
                            _uLastWriteTimeDestination = DateTimeToUint(_LastWriteTimeDestination);
                            _sLastWriteTimeDestination = DateTimeToString(_LastWriteTimeDestination);

                            if (_isDirectory != (_uAttributesDestination & (uint)FileAttributes.Directory) > 0)
                            {
                                _eComparison = PairOfFiles.nComparison.Error;
                                _sErrorMessage = "Contradictory informaton on whether this is a directory or a file.";
                            }
                        }
                        catch
                        {
                            _eComparison = PairOfFiles.nComparison.Error;
                            _sErrorMessage = "Could not read properties of destination file.";
                        }
                    }
                    else
                    {
                        _eComparison = PairOfFiles.nComparison.Error;
                        _sErrorMessage = "Destination file not found.";
                    }
                }
            }
            else if (_eComparison != nComparison.Error)
                eComparison = nComparison.SourceOnly;

            if (isWithSourceAndDestination)
            {
                CompareLastWriteTimes();
                if ((eComparison == nComparison.Identical) && (_kSourceSize != _kDestinationSize))
                {
                    eComparison = nComparison.Error;
                    _sErrorMessage = "Different file sizes on identical file dates should not happen.";
                }
            }
        }

        /// <summary></summary>
        public void ReadProperties()
        {
            if (_isDirectory)
                ReadDirectoryProperties();
            else
                ReadFileProperties();
        }

        /// <summary></summary>
        public void SwapSourceAndDestination()
        {
            bool isSwap;
            uint uSwap;
            long kSwap;
            string sSwap;
            DateTime dtSwap;
            Drive Swap;

            isSwap = _isEncryptedSource;
            _isEncryptedSource = _isEncryptedDestination;
            _isEncryptedDestination = isSwap;

            uSwap = _uAttributesSource;
            _uAttributesSource = _uAttributesDestination;
            _uAttributesDestination = uSwap;

            uSwap = _uLastWriteTimeSource;
            _uLastWriteTimeSource = _uLastWriteTimeDestination;
            _uLastWriteTimeDestination = uSwap;

            kSwap = _kSourceSize;
            _kSourceSize = _kDestinationSize;
            _kDestinationSize = kSwap;

            sSwap = _sLastWriteTimeSource;
            _sLastWriteTimeSource = _sLastWriteTimeDestination;
            _sLastWriteTimeDestination = sSwap;

            sSwap = _sSourceSize;
            _sSourceSize = _sDestinationSize;
            _sDestinationSize = sSwap;

            dtSwap = _CreationTimeSource;
            _CreationTimeSource = _CreationTimeDestination;
            _CreationTimeDestination = dtSwap;
            
            dtSwap = _LastAccessTimeSource;
            _LastAccessTimeSource = _LastAccessTimeDestination;
            _LastAccessTimeDestination = dtSwap;

            dtSwap = _LastWriteTimeSource;
            _LastWriteTimeSource = _LastWriteTimeDestination;
            _LastWriteTimeDestination = dtSwap;

            Swap = _SourceDrive;
            _SourceDrive = _DestinationDrive;
            _DestinationDrive = Swap;

            switch (_eComparison)
            {
                case nComparison.SourceOnly: eComparison = nComparison.DestinationOnly; break;
                case nComparison.DestinationOnly: eComparison = nComparison.SourceOnly; break;
                case nComparison.SourceNewer: eComparison = nComparison.DestinationNewer; break;
                case nComparison.DestinationNewer: eComparison = nComparison.SourceNewer; break;
                case nComparison.UnknownSource : eComparison = nComparison.UnknownDestination; break;
                case nComparison.UnknownDestination: eComparison = nComparison.UnknownSource; break;
            }
        }

        /// <summary></summary>
        public override string ToString()
        {
            return _sRelativePath;
        }

        public void WriteDestinationAttributes(Stream ToStream)
        {
            byte[] abTextBytes = abRelativePathBytes;
            ToStream.Write(BitConverter.GetBytes((ushort)abTextBytes.Length), 0, 2);
            ToStream.Write(abTextBytes, 0, abTextBytes.Length);
            ToStream.Write(BitConverter.GetBytes(_uAttributesDestination), 0, 4);
            ToStream.Write(BitConverter.GetBytes(_CreationTimeDestination.Ticks), 0, 8);
            ToStream.Write(BitConverter.GetBytes(_LastAccessTimeDestination.Ticks), 0, 8);
            ToStream.Write(BitConverter.GetBytes(_LastWriteTimeDestination.Ticks), 0, 8);
            if (!_isDirectory)
            {
                ToStream.Write(BitConverter.GetBytes(_kDestinationSize), 0, 8);
                if ((_uFirstBlockDestination > 0) && (_DestinationDrive != null) && (_DestinationDrive.eEncryptionType == Drive.nEncryptionType.DirectorySymmetric))
                    ToStream.Write(BitConverter.GetBytes(_uFirstBlockDestination), 0, 4);
            }
        }

        public void WriteSourceAttributes(Stream ToStream)
        {
            byte[] abTextBytes = abRelativePathBytes;
            ToStream.Write(BitConverter.GetBytes((ushort)abTextBytes.Length), 0, 2);
            ToStream.Write(abTextBytes, 0, abTextBytes.Length);
            ToStream.Write(BitConverter.GetBytes(_uAttributesSource), 0, 4);
            ToStream.Write(BitConverter.GetBytes(_CreationTimeSource.Ticks), 0, 8);
            ToStream.Write(BitConverter.GetBytes(_LastAccessTimeSource.Ticks), 0, 8);
            ToStream.Write(BitConverter.GetBytes(_LastWriteTimeSource.Ticks), 0, 8);
            if (!_isDirectory)
            {
                ToStream.Write(BitConverter.GetBytes(_kSourceSize), 0, 8);
                if ((_uFirstBlockSource > 0) && (_SourceDrive != null) && (_SourceDrive.eEncryptionType == Drive.nEncryptionType.DirectorySymmetric))
                    ToStream.Write(BitConverter.GetBytes(_uFirstBlockSource), 0, 4);
            }
        }
        #endregion
    }
}

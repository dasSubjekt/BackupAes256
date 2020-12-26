namespace BackupAes256.Model
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Collections.Concurrent;


    /// <summary>A drive.</summary>
    public class Drive
    {
        public enum nHeaderType { Trial, Index, File };
        public enum nEncryptionType { DirectorySymmetric, DirectoryUnencrypted };
        private enum nParserState { AfterFirstTag, AfterSecondTag, AuthenticationOpenTag, AuthenticationCloseTag, Encoding, Error, FileCloseTag, FromCloseTag, FromOpenTag, FromParameters, HmacTag, HybridFileOpenTag, InFirstTag, InSecondTag, MetaOpenTag, MetaCloseTag, Start, SymmetricFileOpenTag, ToCloseTag, ToOpenTag, ToParameters };

        private const int ciDefaultFileSystemBlockSize = 0x1000;   //    4 KB
        public const int ciFileSizeLimitForTesting = 1048576000;   // 1000 MB

        private const string csAppDataSubdirectory = "\\BackupAes256";
        private const string csProgramDataDirectory = "ProgramData";
        private const string csWindowsDirectory = "Windows";

        public readonly char[] acTrimAndSplitCharacters = { '\\', '/' };

        private bool _isCanSetupEncryptedDirectory, _isReady, _isSource;
        private readonly bool _isReadOnly;
        private readonly byte[] _abCopyBuffer;
        private char _cDirectorySeparator;
        private int _iLevelsInRootPath;
        private long _kTotalSize;
        private string _sFormat, _sName, _sRootPath, _sSettingsDirectory, _sTemporaryDirectory, _sTemporaryFilePath, _sVolumeLabel;
        private DriveType _Type;
        private nEncryptionType _eEncryptionType;
        private readonly List<PairOfFiles> _ltEncryptedPairs;
        private readonly TextConverter _TextConverter;
        private CryptoServices _Cryptography;

        #region constructors

        /// <summary></summary>
        protected Drive()
        {
            _isCanSetupEncryptedDirectory = _isReadOnly = _isReady = _isSource = false;
            _cDirectorySeparator = '\\';
            _abCopyBuffer = new byte[PairOfFiles.ciBytesPerProgressUnit];
            _iLevelsInRootPath = -1;
            _kTotalSize = -1;
            _sFormat = _sName = _sRootPath = _sSettingsDirectory = _sTemporaryDirectory = _sTemporaryFilePath = _sVolumeLabel = string.Empty;
            _Type = DriveType.Unknown;
            _eEncryptionType = nEncryptionType.DirectoryUnencrypted;
            _ltEncryptedPairs = new List<PairOfFiles>();
            _TextConverter = new TextConverter();
            _Cryptography = null;
        }

        /// <summary></summary>
        /// <param name=""></param>
        public Drive(string sName) : this()
        {
            _sName = sName;
            _isReadOnly = true;
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public Drive(CryptoServices Cryptography, bool isSource) : this()
        {
            _Cryptography = Cryptography;
            _isSource = isSource;
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public Drive(DriveInfo Info) : this()
        {
            if (Info == null)
                throw new ArgumentNullException("DriveInfo required in class Drive");

            GetDriveInfo(Info);
        }
        #endregion

        #region properties

        /// <summary></summary>
        public bool isCanSetupEncryptedDirectory
        {
            get { return _isCanSetupEncryptedDirectory; }
        }

        /// <summary></summary>
        public char cDirectorySeparator
        {
            get { return _cDirectorySeparator; }
        }

        /// <summary></summary>
        public List<PairOfFiles> ltEncryptedPairs
        {
            get { return _ltEncryptedPairs; }
        }

        /// <summary></summary>
        public nEncryptionType eEncryptionType
        {
            get { return _eEncryptionType; }
            set { _eEncryptionType = value; }
        }

        /// <summary></summary>
        public long kFreeSpace
        {
            get
            {
                DriveInfo Info = new DriveInfo(_sName);

                return Info.AvailableFreeSpace;
            }
        }

        /// <summary></summary>
        public int iLevelsInRootPath
        {
            get
            {
                if ((_iLevelsInRootPath < 0) && (!string.IsNullOrEmpty(_sRootPath)))
                {
                    string[] asSubdirectories = _sRootPath.Split(acTrimAndSplitCharacters, StringSplitOptions.RemoveEmptyEntries);
                    _iLevelsInRootPath = asSubdirectories.Length;
                }
                return _iLevelsInRootPath;
            }
        }

        /// <summary></summary>
        public string sName
        {
            get { return _sName; }
        }

        /// <summary></summary>
        public bool isReadOnly
        {
            get { return _isReadOnly; }
        }

        /// <summary></summary>
        public bool isReady
        {
            get { return _isReady; }
        }

        /// <summary></summary>
        public string sRootPath
        {
            get { return _sRootPath; }
            set
            {
                string sNormalizedRootPath = NormalizeDirectory(value);
                string[] asSubdirectories;

                if (sNormalizedRootPath != _sRootPath)
                {
                    _sRootPath = sNormalizedRootPath;

                    if (string.IsNullOrEmpty(_sName) || ((_sRootPath.Length >= _sName.Length) && (_sRootPath.Substring(0, _sName.Length) != _sName)))
                    {
                        asSubdirectories = value.Split(acTrimAndSplitCharacters, StringSplitOptions.RemoveEmptyEntries);
                        GetDriveInfo(new DriveInfo(asSubdirectories[0]));
                    }
                    _iLevelsInRootPath = -1;
                }
            }
        }

        /// <summary></summary>
        public string sSettingsDirectory
        {
            get { return _sSettingsDirectory; }
        }

        /// <summary></summary>
        public bool isSource
        {
            get { return _isSource; }
            set { _isSource = value; }
        }

        /// <summary></summary>
        public string sTemporaryDirectory
        {
            get { return _sTemporaryDirectory; }
            set { _sTemporaryDirectory = value; }
        }

        /// <summary></summary>
        public long kTotalSize
        {
            get { return _kTotalSize; }
        }

        #endregion

        #region methods

        /// <summary></summary>
        /// <param name=""></param>
        public string AdaptPath(string sExternalPath)
        {
            string sReturn = string.Empty;

            if (!string.IsNullOrEmpty(_sName) && !string.IsNullOrEmpty(sExternalPath) && sExternalPath.Length >= _sName.Length)
                sReturn = _sName + sExternalPath.Substring(_sName.Length);

            return sReturn;
        }

        /// <summary></summary>
        /// <param name=""></param>
        public void AddPair(PairOfFiles PairToAdd)
        {
            if ((_eEncryptionType != nEncryptionType.DirectoryUnencrypted) && (_ltEncryptedPairs != null) && !_ltEncryptedPairs.Contains(PairToAdd))
                _ltEncryptedPairs.Add(PairToAdd);
        }

        /// <summary></summary>
        public void ClearPairs()
        {
            if (_ltEncryptedPairs != null)
                _ltEncryptedPairs.Clear();
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public string ConcatenatePath(string sFirst, string sSecond)
        {
            if (string.IsNullOrEmpty(sFirst) || string.IsNullOrEmpty(sSecond) || (sFirst[sFirst.Length - 1] == cDirectorySeparator) || (sSecond[0] == cDirectorySeparator))
                    return sFirst + sSecond;
            else
                return sFirst + cDirectorySeparator + sSecond;
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        private long CopyWithProgress(Stream SourceStream, Stream DestinationStream, ConcurrentQueue<BackgroundMessage> quReturn)
        {
            int iBytesRead;
            long kReturn = 0;

            while (((iBytesRead = SourceStream.Read(_abCopyBuffer, 0, PairOfFiles.ciBytesPerProgressUnit)) > 0))
            {
                DestinationStream.Write(_abCopyBuffer, 0, iBytesRead);
                kReturn += iBytesRead;

                if (quReturn != null)
                    quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.ReportProgress, 1));
            }
            return kReturn;
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void CopyWithProgress(Stream SourceStream, Stream DestinationStream, long kBytesToCopy)
        {
            int iBytesRead, iBytesToRead;

            iBytesToRead = kBytesToCopy > PairOfFiles.ciBytesPerProgressUnit ? PairOfFiles.ciBytesPerProgressUnit : (int)kBytesToCopy;
            while ((iBytesToRead > 0) && ((iBytesRead = SourceStream.Read(_abCopyBuffer, 0, iBytesToRead)) > 0))
            {
                DestinationStream.Write(_abCopyBuffer, 0, iBytesRead);
                // _quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.ReportProgress, 1));
                kBytesToCopy -= iBytesRead;
                iBytesToRead = kBytesToCopy > PairOfFiles.ciBytesPerProgressUnit ? PairOfFiles.ciBytesPerProgressUnit : (int)kBytesToCopy;
            }
        }

        /// <summary></summary>
        public void Dispose()
        {
            DisposeEncryption();
        }

        /// <summary></summary>
        public void DisposeEncryption()
        {
            if (!string.IsNullOrEmpty(_sTemporaryFilePath) && File.Exists(_sTemporaryFilePath))
            {
                try
                {
                    File.Delete(_sTemporaryFilePath);
                }
                catch { }
                _sTemporaryFilePath = string.Empty;
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        protected void GetDriveInfo(DriveInfo Info)
        {
            _isReady = Info.IsReady;
            if (_isReady)
            {
                _sFormat = Info.DriveFormat;
                _Type = Info.DriveType;
                _sName = Info.Name;
                _kTotalSize = Info.TotalSize;
                _sVolumeLabel = Info.VolumeLabel;

                if ((Directory.Exists(Info.RootDirectory.FullName + csProgramDataDirectory)) && (Directory.Exists(Info.RootDirectory.FullName + csWindowsDirectory)))
                    _sSettingsDirectory = NormalizeDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + csAppDataSubdirectory);
                else
                    _sSettingsDirectory = NormalizeDirectory(Info.RootDirectory.FullName);
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        public string NormalizeDirectory(string sDirectory)
        {
            int i;
            string sReturn = string.Empty;
            string[] asSubdirectories = sDirectory.Split(acTrimAndSplitCharacters, StringSplitOptions.RemoveEmptyEntries);

            _cDirectorySeparator = sDirectory[sDirectory.IndexOfAny(acTrimAndSplitCharacters)];

            for (i = 0; i < asSubdirectories.Length; i++)
                sReturn += asSubdirectories[i] + _cDirectorySeparator;

            if (asSubdirectories.Length == 1)
                return sReturn;
            else
                return sReturn.TrimEnd(acTrimAndSplitCharacters);
        }

        /// <summary></summary>
        /// <param name=""></param>
        public void RemovePair(PairOfFiles PairToRemove)
        {
            if ((_ltEncryptedPairs != null) && _ltEncryptedPairs.Contains(PairToRemove))
                _ltEncryptedPairs.Remove(PairToRemove);
        }

        /// <summary></summary>
        /// <param name=""></param>
        public string RemoveRootPath(string sFilePath)
        {
            int i, iPos = -1;
            char[] acSearchCharacters = { '\\', '/' };

            for (i = 0; i < iLevelsInRootPath; i++)
                iPos = sFilePath.IndexOfAny(acSearchCharacters, iPos + 1);

            return sFilePath.Substring(iPos + 1).TrimStart(acSearchCharacters);
        }

        /// <summary></summary>
        public override string ToString()
        {
            return _sName;
        }

        #endregion
    }
}
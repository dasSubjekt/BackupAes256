namespace BackupAes256.Model
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Collections.Concurrent;


    /// <summary>A drive.</summary>
    public class Drive
    {
        public enum nHeaderType { Trial, Index, File };
        public enum nEncryptionType { FileAsymmetric, FileSymmetric, DirectorySymmetric, DirectoryUnencrypted };
        private enum nParserState { AfterFirstTag, AfterSecondTag, AuthenticationOpenTag, AuthenticationCloseTag, Encoding, Error, FileCloseTag, FromCloseTag, FromOpenTag, FromParameters, HmacTag, HybridFileOpenTag, InFirstTag, InSecondTag, MetaOpenTag, MetaCloseTag, Start, SymmetricFileOpenTag, ToCloseTag, ToOpenTag, ToParameters };

        private const int ciBlockUsagePageSize = 0x100000;         // 1    MB
        private const int ciDefaultFileSystemBlockSize = 0x1000;   // 4    KB
        private const int ciHeaderBlockSize = 0x100;               // 0.25 KB
        private const int ciDefaultFileSystemLevel = 4;
        private const int ciDirectoryAttributesLength = 30;  // fixed length without the length of the UTF-8 representation of sRootPath
        private const int ciBlockPaddingExtraBytes = 2;      // space to store the number of block padding bytes (2 bytes)
        private const int ciSymmetricExtraBytes = 8;         // space to store the number of block padding bytes (4 bytes) + number of block ids (4 bytes)
        private const int ciFileSystemVersion = 1;
        private const int ciVerificationBytesLength = CryptoServices.ciIvOrSaltBytesLength;
        private const int ciBlockFileSystemBase = 36;
        private const int ciPkcs1PaddingByteDifference = 11;
        private const uint cuDefaultReserveBlocks = 0;

        public const int ciFileSizeLimitForTesting = 1048576000;     // 1000 MB
        public const int ciDefaultWorkingMemoryLimit = 1572864000;   // 1500 MB
        private const int ciTotalTextBytesNotAuthenticated = 109;

        public const string csAsymmetricFileExtension = ".hyb";
        public const string csSymmetricFileExtension = ".aes";
        private const string csAppDataSubdirectory = "\\BackupAes256";
        private const string csProgramDataDirectory = "ProgramData";
        private const string csWindowsDirectory = "Windows";

        private const string csFileEncoding = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
        private const string csHybridFileOpenTag = "<Hybrid>";
        private const string csFileMetaOpenTag = "<Meta>";
        private const string csFileVersionOpenTag = "<Version>";
        private const string csFileVersion = "1.00";
        private const string csFileVersionCloseTag = "</Version>";
        private const string csFileDescriptionOpenTag = "<Description>";
        private const string csFileDescription = "file from project https://github.com/dasSubjekt/BackupAes256";
        private const string csFileDescriptionCloseTag = "</Description>";
        private const string csFileSymmetricAlgorithmOpenTag = "<SymmetricAlgorithm>";
        private const string csFileSymmetricAlgorithm = "AES256";
        private const string csFileSymmetricAlgorithmCloseTag = "</SymmetricAlgorithm>";
        private const string csFileSymmetricCipherModeOpenTag = "<SymmetricCipherMode>";
        private const string csFileSymmetricCipherMode = "CBC";
        private const string csFileSymmetricCipherModeCloseTag = "</SymmetricCipherMode>";
        private const string csFileSymmetricPaddingModeOpenTag = "<SymmetricPaddingMode>";
        private const string csFileSymmetricPaddingMode = "PKCS7";
        private const string csFileSymmetricPaddingModeCloseTag = "</SymmetricPaddingMode>";
        private const string csHybridFileAsymmetricAlgorithmOpenTag = "<AsymmetricAlgorithm>";
        private const string csHybridFileAsymmetricAlgorithm = "RSA";
        private const string csHybridFileAsymmetricAlgorithmCloseTag = "</AsymmetricAlgorithm>";
        private const string csHybridFileAsymmetricPaddingModeOpenTag = "<AsymmetricPaddingMode>";
        private const string csHybridFileAsymmetricPaddingMode = "PKCS1";
        private const string csHybridFileAsymmetricPaddingModeCloseTag = "</AsymmetricPaddingMode>";
        private const string csFileHashAlgorithmOpenTag = "<HashAlgorithm>";
        private const string csFileHashAlgorithm = "SHA256";
        private const string csFileHashAlgorithmCloseTag = "</HashAlgorithm>";
        private const string csHybridFileSignaturePaddingOpenTag = "<SignaturePadding>";
        private const string csHybridFileSignaturePadding = "PKCS1";
        private const string csHybridFileSignaturePaddingCloseTag = "</SignaturePadding>";
        private const string csFileMetaCloseTag = "</Meta>";
        private const string csHybridFileExponentOpenTag = "<Exponent>";
        private const string csHybridFileExponentCloseTag = "</Exponent>";
        private const string csHybridFileFromOpenTag = "<From>";
        private const string csHybridFileFromCloseTag = "</From>";
        private const string csHybridFileModulusOpenTag = "<Modulus>";
        private const string csHybridFileModulusCloseTag = "</Modulus>";
        private const string csHybridFileSignatureOpenTag = "<Signature>";
        private const string csHybridFileSignatureCloseTag = "</Signature>";
        private const string csHybridFileToOpenTag = "<To>";
        private const string csHybridFileToCloseTag = "</To>";
        private const string csHybridFileWrappedKeyOpenTag = "<WrappedKey>";
        private const string csHybridFileWrappedKeyCloseTag = "</WrappedKey>";
        private const string csHybridFileCloseTag = "</Hybrid>";

        private const string csSymmetricFileOpenTag = "<Symmetric>";
        private const string csSymmetricFileAuthenticationOpenTag = "<Authentication>";
        private const string csSymmetricFileAuthenticationCloseTag = "</Authentication>";
        private const string csSymmetricFileHmacOpenTag = "<Hmac>";
        private const string csSymmetricFileHmacCloseTag = "</Hmac>";
        private const string csSymmetricFileCloseTag = "</Symmetric>";

        public readonly char[] acTrimAndSplitCharacters = { '\\', '/' };

        private bool _isCanSetupEncryptedDirectory, _isReady, _isSource;
        private readonly bool _isReadOnly;
        private byte[] _abBlockUsage, _abFileBlockBuffer, _abTemporaryExponent, _abTemporaryModulus, _abTemporarySignature, _abTemporaryWrappedKey;
        private readonly byte[] _abCopyBuffer, _abHmacPlaceholder, _abInitializationVector;
        private char _cDirectorySeparator;
        private int _iFileSystemBlockSize, _iFileSystemLevel, _iLevelsInRootPath, _iMaxUsedBlockIndex, _iMinFreeBlockIndex, _iWorkingMemoryLimit;
        private uint _uBlocksUsed, _uReserveBlocks;
        private long _kFreeSpace, _kTotalSize;
        private string _sEncryptedFileName, _sFormat, _sName, _sRootPath, _sSettingsDirectory, _sTemporaryDirectory, _sTemporaryFilePath, _sVolumeLabel;
        private readonly DateTime _ConstantFileDateTime;
        private DriveType _Type;
        private nEncryptionType _eEncryptionType;
        private ICryptoTransform _AesEncryptor;
        private CryptoStream _AesEncryptionStream;
        private Stream _AuthenticationStream;
        private List<CryptoKey> _ltAllKeys, _ltKeysStored;
        private CryptoKey _SelectedAuthenticationKey, _SelectedEncryptionKey;
        private CryptoKey[] _aAsymmetricEncryptionKeys;
        private readonly List<PairOfFiles> _ltEncryptedPairs;
        private readonly TextConverter _TextConverter;
        private readonly Queue<uint> _quBlockIds;
        private CryptoServices _Cryptography;

        #region constructors

        /// <summary></summary>
        protected Drive()
        {
            _isCanSetupEncryptedDirectory = _isReadOnly = _isReady = _isSource = false;
            _abBlockUsage = null;
            _cDirectorySeparator = '\\';
            _iFileSystemBlockSize = ciDefaultFileSystemBlockSize;
            _abCopyBuffer = new byte[PairOfFiles.ciBytesPerProgressUnit];
            _abFileBlockBuffer = new byte[_iFileSystemBlockSize];
            _abHmacPlaceholder = new byte[CryptoServices.ciAesKeyBytesLength];
            for (int i = 0; i < CryptoServices.ciAesKeyBytesLength; i++)
                _abHmacPlaceholder[i] = 0;
            _abInitializationVector = new byte[CryptoServices.ciIvOrSaltBytesLength];
            ResetTemporaryKeys();
            _iFileSystemLevel = ciDefaultFileSystemLevel;
            _uBlocksUsed = 0;
            _uReserveBlocks = cuDefaultReserveBlocks;
            _iLevelsInRootPath = -1;
            _iMaxUsedBlockIndex = _iMinFreeBlockIndex = 0;
            _iWorkingMemoryLimit = ciDefaultWorkingMemoryLimit;
            _kFreeSpace = _kTotalSize = -1;
            _sEncryptedFileName = _sFormat = _sName = _sRootPath = _sSettingsDirectory = _sTemporaryDirectory = _sTemporaryFilePath = _sVolumeLabel = string.Empty;
            _ConstantFileDateTime = new DateTime(2000, 1, 1, 12, 0, 0);
            _Type = DriveType.Unknown;
            _eEncryptionType = nEncryptionType.DirectoryUnencrypted;
            _ltAllKeys = null;
            _ltKeysStored = new List<CryptoKey>();
            _AesEncryptor = null;
            _AesEncryptionStream = null;
            _AuthenticationStream = null;
            _SelectedAuthenticationKey = _SelectedEncryptionKey = null;
            _aAsymmetricEncryptionKeys = null;
            _ltEncryptedPairs = new List<PairOfFiles>();
            _TextConverter = new TextConverter();
            _Cryptography = null;
            _quBlockIds = null;
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
        public Drive(CryptoServices Cryptography, List<CryptoKey> ltAllKeys, bool isSource) : this()
        {
            _Cryptography = Cryptography;
            _ltAllKeys = ltAllKeys;
            _isSource = isSource;
            _quBlockIds = new Queue<uint>();
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public Drive(DriveInfo Info) : this()
        {
            if (Info == null)
                throw new ArgumentNullException("DriveInfo required in class Drive");

            GetDriveInfo(Info);
            if (_isReady)
            {
                ReadKeys();
                ReadSettings();
            }
        }
        #endregion

        #region properties

        /// <summary></summary>
        public List<CryptoKey> ltAllKeys
        {
            get { return _ltAllKeys; }
            set { _ltAllKeys = value; }
        }

        /// <summary></summary>
        public CryptoKey[] aAsymmetricEncryptionKeys
        {
            get { return _aAsymmetricEncryptionKeys; }
            set { _aAsymmetricEncryptionKeys = value; }
        }

        /// <summary></summary>
        public uint uBlocksUsed
        {
            get { return _uBlocksUsed; }
        }

        /// <summary></summary>
        public bool isCanSetupEncryptedDirectory
        {
            get { return _isCanSetupEncryptedDirectory; }
        }

        /// <summary></summary>
        public Stream DecryptionStream
        {
            get { return _AuthenticationStream; }
        }

        /// <summary></summary>
        public char cDirectorySeparator
        {
            get { return _cDirectorySeparator; }
        }

        /// <summary></summary>
        public string sEncryptedFileName
        {
            get { return _sEncryptedFileName; }
            set
            {
                if (string.IsNullOrEmpty(value) || (value.Length < 5))
                    _sEncryptedFileName = string.Empty;
                else
                {
                    _sEncryptedFileName = value;
                    OpenEncryptedFile(false);
                }
            }
        }

        /// <summary></summary>
        public Stream EncryptionStream
        {
            get
            {
                Stream Return = null;

                switch (_eEncryptionType)
                {
                    case nEncryptionType.FileAsymmetric: Return = _AuthenticationStream; break;
                    case nEncryptionType.FileSymmetric: Return = _AesEncryptionStream; break;
                }
                return Return;
            }
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
        public byte[] abFileBlockBuffer
        {
            get { return _abFileBlockBuffer; }
        }

        /// <summary></summary>
        public int iFileSystemBlockSize
        {
            get { return _iFileSystemBlockSize; }
            set
            {
                if (value != _iFileSystemBlockSize)
                {
                    _iFileSystemBlockSize = value;
                    _abFileBlockBuffer = new byte[_iFileSystemBlockSize];
                }
            }
        }

        /// <summary></summary>
        public int iFileSystemLevel
        {
            get { return _iFileSystemLevel; }
            set { _iFileSystemLevel = value; }
        }

        /// <summary></summary>
        public uint uFileSystemMaxBlocks
        {
            get
            {
                uint uReturn;

                if ((_iFileSystemLevel < 1) || (_iFileSystemLevel > 6))
                {
                    uReturn = 0;
                }
                else
                {
                    uReturn = ciBlockFileSystemBase;
                    if (_iFileSystemLevel > 1)
                    {
                        for (int i = 1; i < _iFileSystemLevel; i++)
                            uReturn *= ciBlockFileSystemBase;
                    }
                }
                return uReturn;
            }
        }

        public List<CryptoKey> ltKeysStored
        {
            get { return _ltKeysStored; }
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
        public uint uReserveBlocks
        {
            get { return _uReserveBlocks; }
            set { _uReserveBlocks = value; }
        }

        /// <summary></summary>
        public string sReserveBlocks
        {
            get { return _uReserveBlocks.ToString(); }
            set { }
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

                    if (string.IsNullOrEmpty(_sEncryptedFileName))
                        CheckForDirectoryEncryption();
                    else
                        OpenEncryptedFile(false);

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
        public string sRootPathAndFile
        {
            get { return ConcatenatePath(_sRootPath, _sEncryptedFileName); }
            set
            {
                if (Path.HasExtension(value) && !Directory.Exists(value))
                {
                    sEncryptedFileName = Path.GetFileName(value);
                    sRootPath = Path.GetDirectoryName(value);
                }
                else
                {
                    _sEncryptedFileName = string.Empty;
                    sRootPath = value;
                }
            }
        }

        /// <summary></summary>
        public CryptoKey SelectedAuthenticationKey
        {
            get { return _SelectedAuthenticationKey; }
            set { _SelectedAuthenticationKey = value; }
        }

        /// <summary></summary>
        public CryptoKey SelectedEncryptionKey
        {
            get { return _SelectedEncryptionKey; }
            set { _SelectedEncryptionKey = value; }
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

        /// <summary></summary>
        public int iWorkingMemoryLimit
        {
            get { return _iWorkingMemoryLimit; }
            set { _iWorkingMemoryLimit = value; }
        }

        #endregion

        #region methods

        /// <summary></summary>
        /// <param name=""></param>
        public void AddPair(PairOfFiles PairToAdd)
        {
            if ((_eEncryptionType != nEncryptionType.DirectoryUnencrypted) && (_ltEncryptedPairs != null) && !_ltEncryptedPairs.Contains(PairToAdd))
                _ltEncryptedPairs.Add(PairToAdd);
        }


        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public BackgroundMessage.nReturnCode AuthenticateHybridStream()
        {
            byte[] abFromTo, abReadBuffer = new byte[4];
            int iFromToLength;
            string sFromTo;
            BackgroundMessage.nReturnCode eReturn;

            _AuthenticationStream.Position = 0;
            if (_Cryptography.VerifyRsa(_AuthenticationStream, _SelectedAuthenticationKey))
            {
                _AuthenticationStream.Position = 0;
                _AuthenticationStream.Read(abReadBuffer, 0, 4);
                iFromToLength = BitConverter.ToInt32(abReadBuffer, 0);
                abFromTo = new byte[iFromToLength];
                _AuthenticationStream.Read(abFromTo, 0, iFromToLength);
                sFromTo = _TextConverter.BytesToString(abFromTo);
                // TODO check if sFromTo is identical to the corresponding unencrypted entries in the header
                eReturn = BackgroundMessage.nReturnCode.RsaAuthenticated;
            }
            else
                eReturn = BackgroundMessage.nReturnCode.NoAuthenticationKey;

            return eReturn;
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public BackgroundMessage.nReturnCode AuthenticateSymmetricStream(Stream SourceStream, long kStartToAuthenticate, List<CryptoKey> ltSymmetricKeys)
        {
            byte[] abHmacComputed;
            bool isAuthentic;

            BackgroundMessage.nReturnCode eReturn = BackgroundMessage.nReturnCode.NoAuthenticationKey;

            foreach (CryptoKey AuthenticationKey in ltSymmetricKeys)
            {
                if (eReturn != BackgroundMessage.nReturnCode.AesAuthenticated)
                {
                    isAuthentic = true;
                    SourceStream.Position = kStartToAuthenticate;
                    abHmacComputed = _Cryptography.ComputeHmac(SourceStream, AuthenticationKey);
                    for (int i = 0; i < CryptoServices.ciAesKeyBytesLength; i++)
                        isAuthentic = isAuthentic && (abHmacComputed[i] == _abTemporarySignature[i]);

                    if (isAuthentic)
                    {
                        _SelectedAuthenticationKey = AuthenticationKey;
                        eReturn = BackgroundMessage.nReturnCode.AesAuthenticated;
                    }
                }
            }
            return eReturn;
        }

        /// <summary></summary>
        private void CheckForDirectoryEncryption()
        {
            bool isWithDirectories, isWithFiles;
            int iLevel = 2;
            string sPathIndex0, sPathIndex1;
            DirectoryInfo RootPathInfo = new DirectoryInfo(_sRootPath);
            FileInfo IndexInfo = null;
            IEnumerable<CryptoKey> qySymmetricKeys;

            ClearPairs();
            sPathIndex0 = sPathIndex1 = string.Empty;
            if (RootPathInfo.Exists)
            {
                isWithDirectories = (RootPathInfo.GetDirectories().Length > 0);   // TODO directory access exception handling
                isWithFiles = (RootPathInfo.GetFiles().Length > 0);
                _eEncryptionType = nEncryptionType.DirectoryUnencrypted;
                _isCanSetupEncryptedDirectory = !(isWithDirectories || isWithFiles);

                if (isWithDirectories)
                {
                    sPathIndex0 = cDirectorySeparator + "0" + cDirectorySeparator + "0" + csSymmetricFileExtension;
                    sPathIndex1 = cDirectorySeparator + "0" + cDirectorySeparator + "1" + csSymmetricFileExtension;
                    do
                    {
                        if (File.Exists(_sRootPath + sPathIndex0))
                        {
                            IndexInfo = new FileInfo(_sRootPath + sPathIndex0);
                            _eEncryptionType = nEncryptionType.DirectorySymmetric;
                        }
                        else if (File.Exists(_sRootPath + sPathIndex1))
                        {
                            IndexInfo = new FileInfo(_sRootPath + sPathIndex1);
                            _eEncryptionType = nEncryptionType.DirectorySymmetric;
                        }

                        sPathIndex0 = cDirectorySeparator + "0" + sPathIndex0;
                        sPathIndex1 = cDirectorySeparator + "0" + sPathIndex1;

                    } while ((_eEncryptionType == nEncryptionType.DirectoryUnencrypted) && (++iLevel < 7));
                }
            }
            else
            {
                _isCanSetupEncryptedDirectory = false;
                _eEncryptionType = nEncryptionType.DirectoryUnencrypted;   // TODO if the path extends into an encrypted directory
            }

            if (_eEncryptionType == nEncryptionType.DirectorySymmetric)
            {
                _iFileSystemLevel = iLevel;

                if (IndexInfo != null)
                    _iFileSystemBlockSize = (int)IndexInfo.Length;

                qySymmetricKeys = from k in _ltAllKeys where k.eType == CryptoKey.nKeyType.Symmetric select k;
                DecryptIndex(qySymmetricKeys.ToList());
            }
        }

        /// <summary></summary>
        public void ClearPairs()
        {
            if (_ltEncryptedPairs != null)
                _ltEncryptedPairs.Clear();
        }

        /// <summary></summary>
        public void CloseFileSystem()
        {
            using (MemoryStream SourceStream = new MemoryStream())
            {
                WriteEncryptionIndex(SourceStream, _ltEncryptedPairs);

                SourceStream.Position = 0;
                using (MemoryStream EncryptionStream = new MemoryStream())
                    EncryptToFileSystem(SourceStream, EncryptionStream, 0, null);

                // SourceStream.Position = 0;
                // using (MemoryStream EncryptionStream = new MemoryStream())
                //     _Cryptography.Encrypt(this, 1, SourceStream, EncryptionStream);
            }
        }

        /// <summary></summary>
        public void CloseHybridFile(ConcurrentQueue<BackgroundMessage> quReturn)
        {
            short hBlockPaddingBytes;
            int i, iTotalTextBytesLength;
            long kLengthOfData;
            byte[] abRandomBytes, abSignature, abTextBytes, abVerificationBytes, abWrappedKey;
            CryptoKey[] aRecipientsSymmetric = new CryptoKey[_aAsymmetricEncryptionKeys.Length];
            StringBuilder HeaderBuilder = new StringBuilder();

            _AuthenticationStream.Position = 0;
            abSignature = _Cryptography.SignRsa(_AuthenticationStream, _SelectedAuthenticationKey);
            abVerificationBytes = new byte[ciVerificationBytesLength];
            _Cryptography.GetRandomBytes(abVerificationBytes);

            HeaderBuilder.AppendLine(csFileEncoding);
            HeaderBuilder.AppendLine(csHybridFileOpenTag);
            HeaderBuilder.AppendLine("\t" + csFileMetaOpenTag);
            HeaderBuilder.AppendLine("\t\t" + csFileVersionOpenTag + csFileVersion + csFileVersionCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csFileDescriptionOpenTag + csFileDescription + csFileDescriptionCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csFileSymmetricAlgorithmOpenTag + csFileSymmetricAlgorithm + csFileSymmetricAlgorithmCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csFileSymmetricCipherModeOpenTag + csFileSymmetricCipherMode + csFileSymmetricCipherModeCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csFileSymmetricPaddingModeOpenTag + csFileSymmetricPaddingMode + csFileSymmetricPaddingModeCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csHybridFileAsymmetricAlgorithmOpenTag + csHybridFileAsymmetricAlgorithm + csHybridFileAsymmetricAlgorithmCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csHybridFileAsymmetricPaddingModeOpenTag + csHybridFileAsymmetricPaddingMode + csHybridFileAsymmetricPaddingModeCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csFileHashAlgorithmOpenTag + csFileHashAlgorithm + csFileHashAlgorithmCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csHybridFileSignaturePaddingOpenTag + csHybridFileSignaturePadding + csHybridFileSignaturePaddingCloseTag);
            HeaderBuilder.AppendLine("\t" + csFileMetaCloseTag);
            HeaderBuilder.AppendLine("\t" + csHybridFileFromOpenTag);
            HeaderBuilder.AppendLine("\t\t" + csHybridFileExponentOpenTag + _SelectedAuthenticationKey.sRsaExponentBase64 + csHybridFileExponentCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csHybridFileModulusOpenTag + _SelectedAuthenticationKey.sRsaModulusBase64 + csHybridFileModulusCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csHybridFileSignatureOpenTag + _TextConverter.BytesToBase64String(abSignature) + csHybridFileSignatureCloseTag);
            HeaderBuilder.AppendLine("\t" + csHybridFileFromCloseTag);

            for (i = 0; i < _aAsymmetricEncryptionKeys.Length; i++)
            {
                abRandomBytes = new byte[_aAsymmetricEncryptionKeys[i].iBytes - ciPkcs1PaddingByteDifference];
                _Cryptography.GetRandomBytes(abRandomBytes);
                abWrappedKey = _Cryptography.EncryptRsa(abRandomBytes, _aAsymmetricEncryptionKeys[i]);
                aRecipientsSymmetric[i] = new CryptoKey(_aAsymmetricEncryptionKeys[i].sName, CryptoKey.nKeyFormat.KeePass, CryptoKey.nKeyType.Symmetric, _Cryptography.ComputeSHA256(abRandomBytes));

                HeaderBuilder.AppendLine("\t" + csHybridFileToOpenTag);
                HeaderBuilder.AppendLine("\t\t" + csHybridFileExponentOpenTag + _aAsymmetricEncryptionKeys[i].sRsaExponentBase64 + csHybridFileExponentCloseTag);
                HeaderBuilder.AppendLine("\t\t" + csHybridFileModulusOpenTag + _aAsymmetricEncryptionKeys[i].sRsaModulusBase64 + csHybridFileModulusCloseTag);
                HeaderBuilder.AppendLine("\t\t" + csHybridFileWrappedKeyOpenTag + _TextConverter.BytesToBase64String(abWrappedKey) + csHybridFileWrappedKeyCloseTag);
                HeaderBuilder.AppendLine("\t" + csHybridFileToCloseTag);
            }

            HeaderBuilder.Append(csHybridFileCloseTag);
            abTextBytes = _TextConverter.StringToBytes(HeaderBuilder.ToString());

            using (FileStream DestinationFileStream = new FileStream(sRootPathAndFile, FileMode.Create, FileAccess.Write))
            {
                DestinationFileStream.Write(abTextBytes, 0, abTextBytes.Length);
                iTotalTextBytesLength = abTextBytes.Length;

                while ((iTotalTextBytesLength & (ciHeaderBlockSize - 1)) != 0)
                {
                    DestinationFileStream.WriteByte(0);
                    iTotalTextBytesLength++;
                }
            }

            kLengthOfData = ciVerificationBytesLength + CryptoServices.ciIvOrSaltBytesLength + ciBlockPaddingExtraBytes + ciVerificationBytesLength + _AuthenticationStream.Length;
            hBlockPaddingBytes = (short)ComputeBlockPaddingBytes(kLengthOfData, ciHeaderBlockSize);

            for (i = 0; i < aRecipientsSymmetric.Length; i++)
            {
                _AuthenticationStream.Position = 0;

                using (FileStream DestinationFileStream = new FileStream(sRootPathAndFile, FileMode.Append, FileAccess.Write))
                {
                    DestinationFileStream.Write(abVerificationBytes, 0, ciVerificationBytesLength);                  
                    _Cryptography.GetRandomBytes(_abInitializationVector);
                    DestinationFileStream.Write(_abInitializationVector, 0, CryptoServices.ciIvOrSaltBytesLength);

                    using (ICryptoTransform AesEncryptor = _Cryptography.CreateAesEncryptor(_abInitializationVector, aRecipientsSymmetric[i]))
                    {
                        using (CryptoStream AesEncryptionStream = new CryptoStream(DestinationFileStream, AesEncryptor, CryptoStreamMode.Write))
                        {
                            AesEncryptionStream.Write(BitConverter.GetBytes(hBlockPaddingBytes), 0, 2);             // store the size of the padding
                            if (hBlockPaddingBytes > 0)
                            {
                                abRandomBytes = new byte[hBlockPaddingBytes];
                                _Cryptography.GetRandomBytes(abRandomBytes, hBlockPaddingBytes);
                                AesEncryptionStream.Write(abRandomBytes, 0, hBlockPaddingBytes);
                            }
                            AesEncryptionStream.Write(abVerificationBytes, 0, ciVerificationBytesLength);
                            CopyWithProgress(_AuthenticationStream, AesEncryptionStream, quReturn);
                        }
                    }
                }
            }
            DisposeEncryption();
        }

        /// <summary></summary>
        public void CloseSymmetricFile(ConcurrentQueue<BackgroundMessage> quReturn)
        {
            byte[] abHmac, abTextBytes;
            StringBuilder HeaderBuilder = new StringBuilder();

            _AesEncryptionStream.FlushFinalBlock();
            _AuthenticationStream.Position = 0;
            abHmac = _Cryptography.ComputeHmac(_AuthenticationStream, _SelectedAuthenticationKey);

            HeaderBuilder.AppendLine(csFileEncoding);
            HeaderBuilder.AppendLine(csSymmetricFileOpenTag);
            HeaderBuilder.AppendLine("\t" + csSymmetricFileAuthenticationOpenTag);
            HeaderBuilder.AppendLine("\t\t" + csSymmetricFileHmacOpenTag + _TextConverter.BytesToBase64String(abHmac) + csSymmetricFileHmacCloseTag);
            HeaderBuilder.AppendLine("\t" + csSymmetricFileAuthenticationCloseTag);

            abTextBytes = _TextConverter.StringToBytes(HeaderBuilder.ToString());

            using (FileStream DestinationFileStream = new FileStream(sRootPathAndFile, FileMode.Create, FileAccess.Write))
            {
                DestinationFileStream.Write(abTextBytes, 0, abTextBytes.Length);
                _AuthenticationStream.Position = 0;
                CopyWithProgress(_AuthenticationStream, DestinationFileStream, quReturn);
            }
            DisposeEncryption();
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// /// <param name=""></param>
        private int ComputeBlockPaddingBytes(long kSourceStreamLength, int iBlockSize)
        {
            int iBlockBytesFilled;
            long kAesPaddingBytes = kSourceStreamLength & (CryptoServices.ciIvOrSaltBytesLength - 1);

            // AES encryption adds between 1 and 16 bytes to the length of the original data
            kSourceStreamLength += (CryptoServices.ciIvOrSaltBytesLength - kAesPaddingBytes);

            // but if then the encrypted data is exactly the size of one storage block, we are not adding anything on top
            iBlockBytesFilled = (int)(kSourceStreamLength & ((long)iBlockSize - 1));
            return iBlockBytesFilled == 0 ? 0 : iBlockSize - iBlockBytesFilled;
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
        private uint CountBlockIds(long kSourceStreamLength, int iBlockSize)
        {
            return kSourceStreamLength < iBlockSize ? 0 : (uint)((kSourceStreamLength - 5) / (iBlockSize - 4));
        }

        /// <summary></summary>
        /// <param name=""></param>
        public void CreateHybridFile(int iTotalBytesToEncrypt)
        {
            byte[] abTextBytes;
            string sFromTo = _SelectedAuthenticationKey.sPublicFileContent;

            if (iTotalBytesToEncrypt > _iWorkingMemoryLimit)
            {
                _sTemporaryFilePath = GetTemporaryFilePath(csSymmetricFileExtension);
                _AuthenticationStream = new FileStream(_sTemporaryFilePath, FileMode.Create, FileAccess.Write);
            }
            else
            {
                _sTemporaryFilePath = string.Empty;
                _AuthenticationStream = new MemoryStream();
            }
            _AesEncryptionStream = null;

            for (int i = 0; i < _aAsymmetricEncryptionKeys.Length; i++)
                sFromTo += "\r\n" + _aAsymmetricEncryptionKeys[i].sPublicFileContent;

            abTextBytes = _TextConverter.StringToBytes(sFromTo);
            // the user will never see this text, so it is easier to prepend its length than to parse for it later
            _AuthenticationStream.Write(BitConverter.GetBytes(abTextBytes.Length), 0, 4);
            _AuthenticationStream.Write(abTextBytes, 0, abTextBytes.Length);
        }

        /// <summary></summary>
        /// <param name=""></param>
        public void CreateSymmetricFile(int iTotalBytesToEncrypt)
        {
            byte[] abRandomBytes, abTextBytes, abVerificationBytes = new byte[ciVerificationBytesLength];
            short hBlockPaddingBytes;
            int iTotalTextBytesLength = ciTotalTextBytesNotAuthenticated + CryptoKey.ciAesKeyBase64Length;
            long kLengthOfData;
            StringBuilder HeaderBuilder = new StringBuilder();

            if (iTotalBytesToEncrypt > _iWorkingMemoryLimit)
            {
                _sTemporaryFilePath = GetTemporaryFilePath(csSymmetricFileExtension);
                _AuthenticationStream = new FileStream(_sTemporaryFilePath, FileMode.Create, FileAccess.Write);
            }
            else
            {
                _sTemporaryFilePath = string.Empty;
                _AuthenticationStream = new MemoryStream();
            }

            HeaderBuilder.AppendLine("\t" + csFileMetaOpenTag);
            HeaderBuilder.AppendLine("\t\t" + csFileVersionOpenTag + csFileVersion + csFileVersionCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csFileDescriptionOpenTag + csFileDescription + csFileDescriptionCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csFileSymmetricAlgorithmOpenTag + csFileSymmetricAlgorithm + csFileSymmetricAlgorithmCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csFileSymmetricCipherModeOpenTag + csFileSymmetricCipherMode + csFileSymmetricCipherModeCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csFileSymmetricPaddingModeOpenTag + csFileSymmetricPaddingMode + csFileSymmetricPaddingModeCloseTag);
            HeaderBuilder.AppendLine("\t\t" + csFileHashAlgorithmOpenTag + csFileHashAlgorithm + csFileHashAlgorithmCloseTag);
            HeaderBuilder.AppendLine("\t" + csFileMetaCloseTag);
            HeaderBuilder.Append(csSymmetricFileCloseTag);

            abTextBytes = _TextConverter.StringToBytes(HeaderBuilder.ToString());
            _AuthenticationStream.Write(abTextBytes, 0, abTextBytes.Length);
            iTotalTextBytesLength += abTextBytes.Length;

            while ((iTotalTextBytesLength & (ciHeaderBlockSize - 1)) != 0)
            {
                _AuthenticationStream.WriteByte(0);
                iTotalTextBytesLength++;
            }

            _Cryptography.GetRandomBytes(abVerificationBytes);
            _AuthenticationStream.Write(abVerificationBytes, 0, ciVerificationBytesLength);
            _Cryptography.GetRandomBytes(_abInitializationVector);
            _AuthenticationStream.Write(_abInitializationVector, 0, CryptoServices.ciIvOrSaltBytesLength);   // if encrypted with key, store the initialization vector; if encrypted with password, store the salt

            _AesEncryptor = _Cryptography.CreateAesEncryptor(_abInitializationVector, _SelectedEncryptionKey);
            _AesEncryptionStream = new CryptoStream(_AuthenticationStream, _AesEncryptor, CryptoStreamMode.Write);

            kLengthOfData = ciVerificationBytesLength + CryptoServices.ciIvOrSaltBytesLength + ciBlockPaddingExtraBytes + ciVerificationBytesLength + iTotalBytesToEncrypt;
            hBlockPaddingBytes = (short)ComputeBlockPaddingBytes(kLengthOfData, ciHeaderBlockSize);

            _AesEncryptionStream.Write(BitConverter.GetBytes(hBlockPaddingBytes), 0, 2);             // store the size of the padding
            if (hBlockPaddingBytes > 0)
            {
                abRandomBytes = new byte[hBlockPaddingBytes];
                _Cryptography.GetRandomBytes(abRandomBytes, hBlockPaddingBytes);
                _AesEncryptionStream.Write(abRandomBytes, 0, hBlockPaddingBytes);
            }
            _AesEncryptionStream.Write(abVerificationBytes, 0, ciVerificationBytesLength);
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public BackgroundMessage.nReturnCode DecryptFromFileSystem(Stream DecryptionStream, Stream DestinationStream, byte[] abHmacStored, CryptoKey AuthenticationKey, CryptoKey DecryptionKey, ConcurrentQueue<BackgroundMessage> quReturn)
        {
            BackgroundMessage.nReturnCode eReturn = BackgroundMessage.nReturnCode.UnspecifiedSuccess;
            bool isAuthentic = true;
            byte[] abHmacComputed;
            int i, iBlockPaddingBytes;
            uint uBlockIds, uNewBlockId;

            DecryptionStream.Position = CryptoServices.ciAesKeyBytesLength;   // skip the stored HMAC before computing its value across IV and encrypted data
            abHmacComputed = _Cryptography.ComputeHmac(DecryptionStream, AuthenticationKey);
            for (i = 0; i < CryptoServices.ciAesKeyBytesLength; i++)
                isAuthentic = isAuthentic && (abHmacComputed[i] == abHmacStored[i]);

            // Console.WriteLine("Computed HMAC Bytes:");
            // for (i = 0; i < CryptoServices.ciAesKeyBytesLength; i++)
            //     Console.Write(abHmacComputed[i].ToString("x2") + " ");
            // Console.WriteLine();
            // 
            // Console.WriteLine("Stored HMAC Bytes:");
            // for (i = 0; i < CryptoServices.ciAesKeyBytesLength; i++)
            //     Console.Write(abHmacStored[i].ToString("x2") + " ");
            // Console.WriteLine();

            if (isAuthentic)
            {
                DecryptionStream.Position = CryptoServices.ciAesKeyBytesLength + CryptoServices.ciIvOrSaltBytesLength;

                // in the background, ReadFromBlocks() has copied the stored IV to _abInitializationVector, so use it:                
                using (ICryptoTransform AesDecryptor = _Cryptography.CreateAesDecryptor(_abInitializationVector, DecryptionKey))
                {
                    try
                    {
                        using (CryptoStream AesCryptoStream = new CryptoStream(DecryptionStream, AesDecryptor, CryptoStreamMode.Read))
                        {
                            AesCryptoStream.Read(_abFileBlockBuffer, 0, 4);
                            iBlockPaddingBytes = BitConverter.ToInt32(_abFileBlockBuffer, 0);
                            if ((iBlockPaddingBytes < 0) || (iBlockPaddingBytes >= _iFileSystemBlockSize))
                                eReturn = BackgroundMessage.nReturnCode.NoSymmetricKey;
                            else
                            {
                                AesCryptoStream.Read(_abFileBlockBuffer, 0, iBlockPaddingBytes);
                                AesCryptoStream.Read(_abFileBlockBuffer, 0, 4);
                                uBlockIds = BitConverter.ToUInt32(_abFileBlockBuffer, 0);
                                while ((uBlockIds-- > 0) && (eReturn == BackgroundMessage.nReturnCode.UnspecifiedSuccess))
                                {
                                    AesCryptoStream.Read(_abFileBlockBuffer, 0, 4);
                                    uNewBlockId = BitConverter.ToUInt32(_abFileBlockBuffer, 0);
                                    if (GetBlockUsed(uNewBlockId))
                                        _quBlockIds.Enqueue(uNewBlockId);
                                    else
                                        eReturn = BackgroundMessage.nReturnCode.NoSymmetricKey;
                                }
                                if (eReturn == BackgroundMessage.nReturnCode.UnspecifiedSuccess)
                                    CopyWithProgress(AesCryptoStream, DestinationStream, quReturn);
                            }
                        }
                    }
                    catch (CryptographicException)
                    {
                        eReturn = BackgroundMessage.nReturnCode.NoSymmetricKey;
                    }
                }
            }
            else
                eReturn = BackgroundMessage.nReturnCode.NoAuthenticationKey;

            return eReturn;
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
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public BackgroundMessage.nReturnCode DecryptHybridStream(Stream SourceStream, long kStartToDecrypt, long kLengthToDecrypt, ConcurrentQueue<BackgroundMessage> quReturn)
        {
            bool isVerified = true;
            byte[] abPaddingBuffer, abUnwrappedRandomBytes, abVerificationEncrypted, abVerificationUnencrypted;
            short hBlockPaddingBytes;
            long kBytesRead;
            CryptoKey SymmetricKey;
            BackgroundMessage.nReturnCode eReturn = BackgroundMessage.nReturnCode.WrongFileFormat;

            // foreach (CryptoKey DecryptionKey in ltPublicKeys)
            // {
            SourceStream.Position = kStartToDecrypt;
            abVerificationUnencrypted = new byte[ciVerificationBytesLength];

            using (MemoryStream IntermediateStream = new MemoryStream())
            {
                CopyWithProgress(SourceStream, IntermediateStream, kLengthToDecrypt);
                IntermediateStream.Position = 0;
                kBytesRead = IntermediateStream.Read(abVerificationUnencrypted, 0, ciVerificationBytesLength);

                if (kBytesRead != ciVerificationBytesLength)
                {
                    eReturn = BackgroundMessage.nReturnCode.WrongFileFormat;
                }
                else
                {
                    kBytesRead = IntermediateStream.Read(_abInitializationVector, 0, CryptoServices.ciIvOrSaltBytesLength);

                    if (kBytesRead != CryptoServices.ciIvOrSaltBytesLength)
                    {
                        eReturn = BackgroundMessage.nReturnCode.WrongFileFormat;
                    }
                    else
                    {
                        abUnwrappedRandomBytes = _Cryptography.DecryptRsa(_SelectedEncryptionKey);
                        if (abUnwrappedRandomBytes != null)
                        {
                            SymmetricKey = new CryptoKey(_SelectedEncryptionKey.sName, CryptoKey.nKeyFormat.KeePass, CryptoKey.nKeyType.Symmetric, _Cryptography.ComputeSHA256(abUnwrappedRandomBytes));
                            using (ICryptoTransform AesDecryptor = _Cryptography.CreateAesDecryptor(_abInitializationVector, SymmetricKey))
                            {
                                try
                                {
                                    using (CryptoStream AesCryptoStream = new CryptoStream(IntermediateStream, AesDecryptor, CryptoStreamMode.Read))
                                    {
                                        AesCryptoStream.Read(_abFileBlockBuffer, 0, 2);
                                        hBlockPaddingBytes = BitConverter.ToInt16(_abFileBlockBuffer, 0);
                                        if ((hBlockPaddingBytes < 0) || (hBlockPaddingBytes >= ciHeaderBlockSize))
                                            eReturn = BackgroundMessage.nReturnCode.NoAsymmetricKey;
                                        else
                                        {
                                            kBytesRead = ciVerificationBytesLength + CryptoServices.ciIvOrSaltBytesLength + hBlockPaddingBytes + 2;
                                            abPaddingBuffer = new byte[hBlockPaddingBytes];
                                            AesCryptoStream.Read(abPaddingBuffer, 0, hBlockPaddingBytes);
                                            abVerificationEncrypted = new byte[ciVerificationBytesLength];
                                            kBytesRead += AesCryptoStream.Read(abVerificationEncrypted, 0, ciVerificationBytesLength);

                                            for (int i = 0; i < ciVerificationBytesLength; i++)
                                                isVerified = isVerified && (abVerificationUnencrypted[i] == abVerificationEncrypted[i]);

                                            if (isVerified)
                                            {
                                                // TODO (kBytesRead + CryptoServices.ciIvOrSaltBytesLength > kLengthToDecrypt) && (kBytesRead < kLengthToDecrypt)
                                                eReturn = BackgroundMessage.nReturnCode.DecryptionSuccessful;
                                                quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.UserMessage, BackgroundMessage.nReturnCode.FoundPrivateKey, _SelectedEncryptionKey.sName));
                                                kBytesRead += CopyWithProgress(AesCryptoStream, _AuthenticationStream, quReturn);
                                            }
                                            else
                                            {
                                                eReturn = BackgroundMessage.nReturnCode.NoSymmetricKey;
                                            }
                                        }
                                    }
                                }
                                catch (CryptographicException)
                                {
                                    eReturn = BackgroundMessage.nReturnCode.NoAsymmetricKey;
                                    AesDecryptor.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            // }
            return eReturn;
        }


        /// <summary></summary>
        /// <param name=""></param>
        public BackgroundMessage.nReturnCode DecryptIndex(List<CryptoKey> ltSymmetricKeys)
        {
            // byte[] abHmacStored;
            BackgroundMessage.nReturnCode eReturn = BackgroundMessage.nReturnCode.UnspecifiedError;

            // _SelectedAuthenticationKey = _SelectedEncryptionKey = null;
            // 
            // using (MemoryStream DestinationStream = new MemoryStream())
            // {
            //     using (MemoryStream DecryptionStream = new MemoryStream())
            //     {
            //         _quBlockIds.Enqueue(0);
            //         abHmacStored = ReadFromBlocks(DecryptionStream);
            // 
            //         if (abHmacStored == null)
            //         {
            //             _quBlockIds.Clear();
            //         }
            //         else
            //         {
            //             foreach (CryptoKey EncryptionKey in ltSymmetricKeys)
            //             {
            //                 if (EncryptionKey.eType == CryptoKey.nKeyType.Symmetric)
            //                 {
            //                     foreach (CryptoKey AuthenticationKey in ltSymmetricKeys)
            //                     {
            //                         if (AuthenticationKey.eType == CryptoKey.nKeyType.Symmetric)
            //                         {
            //                             if (eReturn != BackgroundMessage.nReturnCode.Success)
            //                             {
            //                                 eReturn = DecryptFromFileSystem(DecryptionStream, DestinationStream, abHmacStored, AuthenticationKey, EncryptionKey, null);
            //                                 if (eReturn == BackgroundMessage.nReturnCode.NoSymmetricKey)
            //                                 {
            //                                     _SelectedAuthenticationKey = AuthenticationKey;
            //                                 }
            //                                 else if (eReturn == BackgroundMessage.nReturnCode.Success)
            //                                 {
            //                                     _SelectedAuthenticationKey = AuthenticationKey;
            //                                     _SelectedEncryptionKey = EncryptionKey;
            //                                 }
            //                             }
            //                         }
            //                     }
            //                 }
            //             }
            //         }
            //     }
            // }
            // 
            // if ((_SelectedAuthenticationKey != null) && (_SelectedEncryptionKey != null))
            // {
            //     ClearPairs();
            //     using (MemoryStream DestinationStream = new MemoryStream())
            //     {
            //         using (MemoryStream DecryptionStream = new MemoryStream())
            //         {
            //             _quBlockIds.Enqueue(0);
            //             abHmacStored = ReadFromBlocks(DecryptionStream);
            // 
            //             if (abHmacStored == null)
            //             {
            //                 _quBlockIds.Clear();
            //             }
            //             else
            //             {
            //                 eReturn = DecryptFromFileSystem(DecryptionStream, DestinationStream, abHmacStored, _SelectedAuthenticationKey, _SelectedEncryptionKey, null);
            // 
            //                 // Console.WriteLine("eDecryptionResult=" + eDecryptionResult.ToString());
            //                 if (eReturn == BackgroundMessage.nReturnCode.Success)
            //                 {
            //                     // DriveToDecrypt.CreationTime = new DateTime(_kCreationTimeTicks);
            //                     DestinationStream.Position = 0;
            //                     if (!ReadFileSystemIndex(DestinationStream, _ltEncryptedPairs))
            //                         eReturn = BackgroundMessage.nReturnCode.NoSymmetricKey;
            //                 }
            //             }
            //         }
            //     }
            // }
            return eReturn;
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public BackgroundMessage.nReturnCode DecryptSymmetricStream(Stream SourceStream, long kStartToDecrypt, List<CryptoKey> ltSymmetricKeys, ConcurrentQueue<BackgroundMessage> quReturn)
        {
            bool isVerified = true;
            byte[] abPaddingBuffer, abVerificationEncrypted, abVerificationUnencrypted;
            short hBlockPaddingBytes;
            long kBytesRead;
            BackgroundMessage.nReturnCode eReturn = BackgroundMessage.nReturnCode.NoSymmetricKey;

            foreach (CryptoKey DecryptionKey in ltSymmetricKeys)
            {
                if (eReturn != BackgroundMessage.nReturnCode.AuthenticationAndDecryptionSuccessful)
                {
                    using (FileStream DecryptionFileStream = new FileStream(sRootPathAndFile, FileMode.Open, FileAccess.Read))
                    {
                        DecryptionFileStream.Position = kStartToDecrypt;
                        abVerificationUnencrypted = new byte[ciVerificationBytesLength];
                        kBytesRead = DecryptionFileStream.Read(abVerificationUnencrypted, 0, ciVerificationBytesLength);

                        if (kBytesRead != ciVerificationBytesLength)
                        {
                            eReturn = BackgroundMessage.nReturnCode.WrongFileFormat;
                        }
                        else
                        {
                            kBytesRead = DecryptionFileStream.Read(_abInitializationVector, 0, CryptoServices.ciIvOrSaltBytesLength);
                            if (kBytesRead != CryptoServices.ciIvOrSaltBytesLength)
                            {
                                eReturn = BackgroundMessage.nReturnCode.WrongFileFormat;
                            }
                            else
                            {
                                using (ICryptoTransform AesDecryptor = _Cryptography.CreateAesDecryptor(_abInitializationVector, DecryptionKey))
                                {
                                    try
                                    {
                                        using (CryptoStream AesCryptoStream = new CryptoStream(DecryptionFileStream, AesDecryptor, CryptoStreamMode.Read))
                                        {
                                            AesCryptoStream.Read(_abFileBlockBuffer, 0, 2);
                                            hBlockPaddingBytes = BitConverter.ToInt16(_abFileBlockBuffer, 0);

                                            if ((hBlockPaddingBytes < 0) || (hBlockPaddingBytes >= ciHeaderBlockSize))
                                            {
                                                eReturn = BackgroundMessage.nReturnCode.NoSymmetricKey;
                                            }
                                            else
                                            {
                                                kBytesRead = ciVerificationBytesLength + CryptoServices.ciIvOrSaltBytesLength + hBlockPaddingBytes + 2;
                                                abPaddingBuffer = new byte[hBlockPaddingBytes];
                                                AesCryptoStream.Read(abPaddingBuffer, 0, hBlockPaddingBytes);
                                                abVerificationEncrypted = new byte[ciVerificationBytesLength];
                                                kBytesRead += AesCryptoStream.Read(abVerificationEncrypted, 0, ciVerificationBytesLength);

                                                for (int i = 0; i < ciVerificationBytesLength; i++)
                                                    isVerified = isVerified && (abVerificationUnencrypted[i] == abVerificationEncrypted[i]);

                                                if (isVerified)
                                                {
                                                    // TODO (kBytesRead + CryptoServices.ciIvOrSaltBytesLength > kLengthToDecrypt) && (kBytesRead < kLengthToDecrypt)
                                                    _SelectedEncryptionKey = DecryptionKey;
                                                    eReturn = BackgroundMessage.nReturnCode.AuthenticationAndDecryptionSuccessful;
                                                    quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.UserMessage, BackgroundMessage.nReturnCode.FoundSymmetricKey, DecryptionKey.sName));
                                                    kBytesRead += CopyWithProgress(AesCryptoStream, _AuthenticationStream, quReturn);
                                                    _AuthenticationStream.Position = 0;
                                                }
                                                else
                                                    eReturn = BackgroundMessage.nReturnCode.NoSymmetricKey;
                                            }
                                        }
                                    }
                                    catch (CryptographicException)
                                    {
                                        eReturn = BackgroundMessage.nReturnCode.NoSymmetricKey;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return eReturn;
        }

        /// <summary></summary>
        public void Dispose()
        {
            DisposeEncryption();
        }

        /// <summary></summary>
        public void DisposeEncryption()
        {
            if (_AesEncryptionStream != null)
            {
                _AesEncryptionStream.Close();
                _AesEncryptionStream.Dispose();
                _AesEncryptionStream = null;
            }
            if (_AesEncryptor != null)
            {
                _AesEncryptor.Dispose();
                _AesEncryptor = null;
            }
            if (_AuthenticationStream != null)
            {
                _AuthenticationStream.Close();
                _AuthenticationStream.Dispose();
                _AuthenticationStream = null;
            }
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

        public void EncryptToFileSystem(FileStream SourceStream, long kSourceSize, ConcurrentQueue<BackgroundMessage> quReturn)
        {
            uint uFirstBlockId = GetFreeBlockId();
            string sTemporaryFilePath;

            if (kSourceSize > _iWorkingMemoryLimit)
            {
                sTemporaryFilePath = GetTemporaryFilePath(csSymmetricFileExtension);
                using (FileStream AuthenticationStream = new FileStream(sTemporaryFilePath, FileMode.Create, FileAccess.Write))
                    EncryptToFileSystem(SourceStream, AuthenticationStream, uFirstBlockId, quReturn);
                if (File.Exists(sTemporaryFilePath))
                    File.Delete(sTemporaryFilePath);
            }
            else
            {
                using (MemoryStream AuthenticationStream = new MemoryStream())
                    EncryptToFileSystem(SourceStream, AuthenticationStream, uFirstBlockId, quReturn);
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public void EncryptToFileSystem(Stream SourceStream, Stream AuthenticationStream, uint uFirstBlockId, ConcurrentQueue<BackgroundMessage> quReturn)
        {
            byte[] abHmac;
            int iBlockPaddingBytes;
            uint u, uBlockIdsBefore, uBlockIdsAfter, uNewBlockId;
            long kLengthOfData;

            _quBlockIds.Clear();
            _quBlockIds.Enqueue(uFirstBlockId);
            kLengthOfData = CryptoServices.ciAesKeyBytesLength + CryptoServices.ciIvOrSaltBytesLength + ciSymmetricExtraBytes + SourceStream.Length;

            uBlockIdsBefore = uBlockIdsAfter = 0;
            do
            {
                if (uBlockIdsBefore < uBlockIdsAfter)
                    uBlockIdsBefore = uBlockIdsAfter;

                iBlockPaddingBytes = ComputeBlockPaddingBytes(kLengthOfData + (uBlockIdsBefore << 2), _iFileSystemBlockSize);
                uBlockIdsAfter = CountBlockIds(kLengthOfData + iBlockPaddingBytes + (uBlockIdsBefore << 2), _iFileSystemBlockSize);
                // Console.WriteLine("uBlockIdsBefore=" + uBlockIdsBefore.ToString() + ", uBlockIdsAfter=" + uBlockIdsAfter.ToString() + ", iBlockPaddingBytes=" + iBlockPaddingBytes.ToString());
            }
            while (uBlockIdsBefore != uBlockIdsAfter);

            AuthenticationStream.Write(_abHmacPlaceholder, 0, CryptoServices.ciAesKeyBytesLength);
            _Cryptography.GetRandomBytes(_abInitializationVector);
            AuthenticationStream.Write(_abInitializationVector, 0, CryptoServices.ciIvOrSaltBytesLength);   // if encrypted with key, store the initialization vector; if encrypted with password, store the salt

            using (ICryptoTransform AesEncryptor = _Cryptography.CreateAesEncryptor(_abInitializationVector, _SelectedEncryptionKey))
            {
                using (CryptoStream AesEncryptionStream = new CryptoStream(AuthenticationStream, AesEncryptor, CryptoStreamMode.Write))
                {
                    AesEncryptionStream.Write(BitConverter.GetBytes(iBlockPaddingBytes), 0, 4);   // we are not storing the file size here, so we must store the size of the padding
                    _Cryptography.GetRandomBytes(_abFileBlockBuffer, iBlockPaddingBytes);
                    AesEncryptionStream.Write(_abFileBlockBuffer, 0, iBlockPaddingBytes);
                    AesEncryptionStream.Write(BitConverter.GetBytes(uBlockIdsAfter), 0, 4);      // store the number of block ids

                    for (u = 0; u < uBlockIdsAfter; u++)
                    {
                        uNewBlockId = GetFreeBlockId();
                        _quBlockIds.Enqueue(uNewBlockId);
                        AesEncryptionStream.Write(BitConverter.GetBytes(uNewBlockId), 0, 4);   // and the block ids themselves
                    }

                    CopyWithProgress(SourceStream, AesEncryptionStream, quReturn);
                    AesEncryptionStream.FlushFinalBlock();   // closing the AesEncryptionStream would close the AuthenticationStream, so instead we do this in order not to lose any data

                    AuthenticationStream.Position = CryptoServices.ciAesKeyBytesLength;   // rewind to the position from where on to compute the HMAC
                    abHmac = _Cryptography.ComputeHmac(AuthenticationStream, _SelectedAuthenticationKey);

                    // Console.WriteLine("Computed HMAC Bytes:");
                    // for (int i = 0; i < CryptoServices.ciAesKeyBytesLength; i++)
                    //     Console.Write(abHmac[i].ToString("x2") + " ");
                    // Console.WriteLine();

                    AuthenticationStream.Position = 0;   // rewind to the start before writing to disk
                    WriteToBlocks(AuthenticationStream, abHmac);   // closing the AesEncryptionStream closes the AuthenticationStream, so we must do this here, not two } further down
                }
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        public string GetBlockPath(uint uBlockId)
        {
            char[] acDigits = new char[_iFileSystemLevel];
            int i, iDigit;
            uint uQuotient, uRemainder = uBlockId;
            DirectoryInfo BlockPathDirectoryInfo;
            StringBuilder BlockPathStringBuilder = new StringBuilder(_sRootPath);

            for (i = 0; i < _iFileSystemLevel; i++)
            {
                uQuotient = uRemainder / ciBlockFileSystemBase;
                iDigit = (int)(uRemainder - ciBlockFileSystemBase * uQuotient);
                uRemainder = uQuotient;

                if (iDigit < 10)
                    acDigits[i] = (char)(iDigit + '0');
                else
                    acDigits[i] = (char)(iDigit + 'W');
            }

            for (i = _iFileSystemLevel - 1; i > 0; i--)
            {
                BlockPathStringBuilder.Append(_cDirectorySeparator);
                BlockPathStringBuilder.Append(acDigits[i]);
                BlockPathDirectoryInfo = new DirectoryInfo(BlockPathStringBuilder.ToString());

                if (!BlockPathDirectoryInfo.Exists)
                    BlockPathDirectoryInfo.Create();

                try
                {
                    BlockPathDirectoryInfo.CreationTime = _ConstantFileDateTime;
                    BlockPathDirectoryInfo.LastAccessTime = _ConstantFileDateTime;
                    BlockPathDirectoryInfo.LastWriteTime = _ConstantFileDateTime;
                }
                catch { }
                }

            BlockPathStringBuilder.Append(_cDirectorySeparator);
            BlockPathStringBuilder.Append(acDigits[0]);
            BlockPathStringBuilder.Append(csSymmetricFileExtension);

            return BlockPathStringBuilder.ToString();
        }

        /// <summary></summary>
        /// <param name=""></param>
        public bool GetBlockUsed(uint uBlockId)
        {
            int iBlockIndex = (int)(uBlockId >> 3);

            if ((_abBlockUsage != null) && (iBlockIndex < _abBlockUsage.Length))
                return (_abBlockUsage[iBlockIndex] & (byte)(1 << (int)(uBlockId & 0x7))) > 0;
            else
                return false;
        }

        /// <summary></summary>
        /// <param name=""></param>
        protected void GetDriveInfo(DriveInfo Info)
        {
            _isReady = Info.IsReady;
            if (_isReady)
            {
                _kFreeSpace = Info.AvailableFreeSpace;
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
        public uint GetFreeBlockId()
        {
            uint uNewBlockId;

            while ((_iMinFreeBlockIndex < _abBlockUsage.Length) && (_abBlockUsage[_iMinFreeBlockIndex] == 0xff))
                _iMinFreeBlockIndex++;

            uNewBlockId = (uint)_iMinFreeBlockIndex << 3;

            while (GetBlockUsed(uNewBlockId))
                uNewBlockId++;

            Console.WriteLine("GetFreeBlockId(" + uNewBlockId.ToString() + ")");
            SetBlockUsed(uNewBlockId, true);
            return uNewBlockId;
        }

        /// <summary></summary>
        /// <param name=""></param>
        public string GetTemporaryFilePath(string sExtension)
        {
            byte[] abRandomNumber = new byte[6];
            string sReturn;

            do
            {
                _Cryptography.GetRandomBytes(abRandomNumber);
                sReturn = _sTemporaryDirectory + _cDirectorySeparator + Convert.ToBase64String(abRandomNumber) + sExtension;
            } while (File.Exists(sReturn));

            return sReturn;
        }

        private int FindHeaderLength(byte[] abHeader)
        {
            bool isFound;
            int i, iCloseTagLength, iPosition, iReturn;
            byte bCharacter;
            byte[] abCloseTag = new byte[12];
            nParserState eParserState = nParserState.Start;

            iCloseTagLength = iPosition = 0;
            iReturn = -1;
            abCloseTag[iCloseTagLength++] = (byte)'/';

            while ((eParserState != nParserState.Error) && (eParserState != nParserState.AfterSecondTag) && (iPosition < abHeader.Length))
            {
                bCharacter = abHeader[iPosition++];

                if (bCharacter == (byte)'<')
                {
                    if (eParserState == nParserState.Start)
                        eParserState = nParserState.InFirstTag;
                    else if (eParserState == nParserState.AfterFirstTag)
                        eParserState = nParserState.InSecondTag;
                    else
                        eParserState = nParserState.Error;
                }
                else if (bCharacter == (byte)'>')
                {
                    if (eParserState == nParserState.InFirstTag)
                        eParserState = nParserState.AfterFirstTag;
                    else if (eParserState == nParserState.InSecondTag)
                    {
                        abCloseTag[iCloseTagLength++] = bCharacter;
                        eParserState = nParserState.AfterSecondTag;
                    }
                    else
                        eParserState = nParserState.Error;
                }
                else if (eParserState == nParserState.InSecondTag)
                {
                    if (iCloseTagLength < abCloseTag.Length)
                        abCloseTag[iCloseTagLength++] = bCharacter;
                    else
                        eParserState = nParserState.Error;
                }
            }

            if (eParserState == nParserState.AfterSecondTag)
            {
                iPosition = 1;
                while ((iPosition > 0) && (iPosition < abHeader.Length))
                {
                    bCharacter = abHeader[iPosition];

                    if ((bCharacter == 0) || (bCharacter > 0x7f))   // error, this is data, not text
                    {
                        iPosition = iReturn = 0;
                    }
                    else if ((bCharacter == (byte)'<') && ((abHeader[iPosition - 1] == (byte)'\n') || (abHeader[iPosition - 1] == (byte)'\r')) && ((iPosition + iCloseTagLength) < abHeader.Length))
                    {
                        isFound = true;

                        for (i = 0; i < iCloseTagLength; i++)
                            isFound = isFound && (abHeader[iPosition + i + 1] == abCloseTag[i]);

                        if (isFound)
                        {
                            iReturn = iPosition + iCloseTagLength + 1;
                            iPosition = 0;
                        }
                        else
                            iPosition++;
                    }
                    else
                        iPosition++;
                }
            }
            return iReturn;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private byte Log2(int iNumber)
        {
            byte bReturn = 0;
            bool isError = (iNumber == 0);

            if (!isError)
            {
                while ((iNumber & 1) == 0)
                {
                    iNumber >>= 1;
                    bReturn++;
                }
                isError = (iNumber > 1);
            }

            if (isError)
                throw new NotImplementedException("Function Log2 does not work for argument " + iNumber.ToString() + ".");
            else
                return bReturn;
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
        /// <param name="isOpenStream">False if only the header is to be read, true if then the FileStream is to be decrypted.</param>
        /// <param name="quReturn">Return queue for asynchronous messages.</param>
        public BackgroundMessage.nReturnCode OpenEncryptedFile(bool isOpenStream, ConcurrentQueue<BackgroundMessage> quReturn = null)
        {
            byte[] abEncryptionHeader;
            long kLengthToDecrypt, kStartToDecrypt;
            IEnumerable<CryptoKey> qyKeys;
            BackgroundMessage.nReturnCode eReturn = BackgroundMessage.nReturnCode.ProgrammingError;
            BackgroundMessage NewMessage = new BackgroundMessage(BackgroundMessage.nType.UserMessage);

            _eEncryptionType = nEncryptionType.DirectoryUnencrypted;

            if (!string.IsNullOrEmpty(_sEncryptedFileName))
            {
                if (File.Exists(sRootPathAndFile))
                {
                    using (FileStream SourceFileStream = new FileStream(sRootPathAndFile, FileMode.Open, FileAccess.Read))
                    {
                        abEncryptionHeader = ReadEncryptionHeader(SourceFileStream);

                        if ((abEncryptionHeader != null) && ParseEncryptionHeader(_TextConverter.BytesToString(abEncryptionHeader)))
                        {
                            if ((isOpenStream) && (quReturn != null))
                            {
                                kStartToDecrypt = SourceFileStream.Position;
                                kLengthToDecrypt = SourceFileStream.Length - kStartToDecrypt;

                                if (_eEncryptionType == nEncryptionType.FileAsymmetric)
                                    kLengthToDecrypt /= _aAsymmetricEncryptionKeys.Length;

                                if (kLengthToDecrypt > _iWorkingMemoryLimit)
                                {
                                    _sTemporaryFilePath = GetTemporaryFilePath(csSymmetricFileExtension);
                                    _AuthenticationStream = new FileStream(_sTemporaryFilePath, FileMode.Create, FileAccess.Write);
                                }
                                else
                                {
                                    _sTemporaryFilePath = string.Empty;
                                    _AuthenticationStream = new MemoryStream();
                                }

                                if (_eEncryptionType == nEncryptionType.FileAsymmetric)
                                {
                                    if (_SelectedEncryptionKey == null)
                                        eReturn = BackgroundMessage.nReturnCode.NoAsymmetricKey;
                                    else
                                    {
                                        for (int i = 0; i < _aAsymmetricEncryptionKeys.Length; i++)
                                        {
                                            if (_aAsymmetricEncryptionKeys[i].HashIdEquals(_SelectedEncryptionKey.abHashId))
                                            {
                                                // qyKeys = from k in _ltAllKeys where (k.eType == CryptoKey.nKeyType.AsymmetricPrivate) || (k.eType == CryptoKey.nKeyType.AsymmetricPublic) select k;

                                                eReturn = DecryptHybridStream(SourceFileStream, kStartToDecrypt, kLengthToDecrypt, quReturn);
                                                if (eReturn == BackgroundMessage.nReturnCode.DecryptionSuccessful)
                                                {
                                                    eReturn = AuthenticateHybridStream();
                                                    if (eReturn == BackgroundMessage.nReturnCode.RsaAuthenticated)
                                                    {
                                                        quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.UserMessage, BackgroundMessage.nReturnCode.FoundAuthenticationKey, _SelectedAuthenticationKey.sName));
                                                        eReturn = BackgroundMessage.nReturnCode.AuthenticationAndDecryptionSuccessful;
                                                    }
                                                }
                                            }
                                            else
                                                kStartToDecrypt += kLengthToDecrypt;
                                        }
                                    }
                                }
                                else if (_eEncryptionType == nEncryptionType.FileSymmetric)
                                {
                                    qyKeys = from k in _ltAllKeys where k.eType == CryptoKey.nKeyType.Symmetric select k;

                                    _sTemporaryFilePath = string.Empty;
                                    _AuthenticationStream = new MemoryStream();

                                    eReturn = AuthenticateSymmetricStream(SourceFileStream, ciTotalTextBytesNotAuthenticated + CryptoKey.ciAesKeyBase64Length, qyKeys.ToList());
                                    if (eReturn == BackgroundMessage.nReturnCode.AesAuthenticated)
                                    {
                                        quReturn.Enqueue(new BackgroundMessage(BackgroundMessage.nType.UserMessage, BackgroundMessage.nReturnCode.FoundAuthenticationKey, _SelectedAuthenticationKey.sName));
                                        eReturn = DecryptSymmetricStream(SourceFileStream, kStartToDecrypt, qyKeys.ToList(), quReturn);
                                    }
                                }
                            }
                            else
                                eReturn = BackgroundMessage.nReturnCode.ParsingSuccessful;
                        }
                    }
                }
                else if ((_sEncryptedFileName.Length > csAsymmetricFileExtension.Length) && (_sEncryptedFileName.Substring(_sEncryptedFileName.Length - csAsymmetricFileExtension.Length) == csAsymmetricFileExtension))
                    _eEncryptionType = nEncryptionType.FileAsymmetric;
                else if ((_sEncryptedFileName.Length > csSymmetricFileExtension.Length) && (_sEncryptedFileName.Substring(_sEncryptedFileName.Length - csSymmetricFileExtension.Length) == csSymmetricFileExtension))
                    _eEncryptionType = nEncryptionType.FileSymmetric;
            }
            return eReturn;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private bool ParseEncryptionHeader(string EncryptionHeader)
        {
            nParserState eParserState = nParserState.Start;
            string sCurrentLine;
            string[] asDelimiters = { "\r\n", "\r", "\n" };
            string[] asLines = EncryptionHeader.Split(asDelimiters, StringSplitOptions.RemoveEmptyEntries);

            if ((asLines != null) && (asLines.Length > 5))
            {
                _aAsymmetricEncryptionKeys = null;

                for (int i = 0; i < asLines.Length; i++)
                {
                    sCurrentLine = asLines[i].Trim();

                    switch (eParserState)
                    {
                        case nParserState.Start: if (sCurrentLine == csFileEncoding) eParserState = nParserState.Encoding; else eParserState = nParserState.Error; break;
                        case nParserState.Encoding: if (sCurrentLine == csHybridFileOpenTag) eParserState = nParserState.HybridFileOpenTag; else if (sCurrentLine == csSymmetricFileOpenTag) eParserState = nParserState.SymmetricFileOpenTag; else eParserState = nParserState.Error; break;
                        case nParserState.HybridFileOpenTag: if (sCurrentLine == csFileMetaOpenTag) eParserState = nParserState.MetaOpenTag; else eParserState = nParserState.Error; break;
                        case nParserState.SymmetricFileOpenTag: if (sCurrentLine == csSymmetricFileAuthenticationOpenTag) eParserState = nParserState.AuthenticationOpenTag; else eParserState = nParserState.Error; break;
                        case nParserState.AuthenticationOpenTag: eParserState = ParseHmacBase64(sCurrentLine); break;
                        case nParserState.HmacTag: if (sCurrentLine == csSymmetricFileAuthenticationCloseTag) eParserState = nParserState.AuthenticationCloseTag; else eParserState = nParserState.Error; break;
                        case nParserState.AuthenticationCloseTag: if (sCurrentLine == csFileMetaOpenTag) eParserState = nParserState.MetaOpenTag; else eParserState = nParserState.Error; break;
                        case nParserState.MetaOpenTag: if (sCurrentLine == csFileMetaCloseTag) eParserState = nParserState.MetaCloseTag; break;   // no else: for the moment ignore the lines in between
                        case nParserState.MetaCloseTag: if (sCurrentLine == csHybridFileFromOpenTag) eParserState = nParserState.FromOpenTag; else if (sCurrentLine == csSymmetricFileCloseTag) eParserState = nParserState.FileCloseTag; else eParserState = nParserState.Error; break;
                        case nParserState.FromOpenTag:
                        case nParserState.FromParameters: if (sCurrentLine == csHybridFileFromCloseTag) { ResetTemporaryKeys(); eParserState = nParserState.FromCloseTag; } else eParserState = ParseFromParameters(sCurrentLine); break;
                        case nParserState.FromCloseTag: if (sCurrentLine == csHybridFileToOpenTag) eParserState = nParserState.ToOpenTag; else eParserState = nParserState.Error; break;
                        case nParserState.ToOpenTag:
                        case nParserState.ToParameters: if (sCurrentLine == csHybridFileToCloseTag) { ResetTemporaryKeys(); eParserState = nParserState.ToCloseTag; } else eParserState = ParseToParameters(sCurrentLine); break;
                        case nParserState.ToCloseTag: if (sCurrentLine == csHybridFileToOpenTag) eParserState = nParserState.ToOpenTag; else if (sCurrentLine == csHybridFileCloseTag) eParserState = nParserState.FileCloseTag; else eParserState = nParserState.Error; break;
                        case nParserState.FileCloseTag: eParserState = nParserState.Error; break;   // nothing should come after the close tag
                    }
                }
            }
            return (eParserState == nParserState.FileCloseTag) && ((_eEncryptionType == nEncryptionType.FileSymmetric) || ((_SelectedAuthenticationKey != null) && (_aAsymmetricEncryptionKeys != null)));
        }

        /// <summary></summary>
        /// <param name=""></param>
        private nParserState ParseFromParameters(string sLine)
        {
            bool isOk = !string.IsNullOrEmpty(sLine);
            IEnumerable<CryptoKey> qyFoundKeys;
            CryptoKey NewKey;

            if (isOk)
            {
                if (_TextConverter.IsTag(sLine, csHybridFileExponentOpenTag, csHybridFileExponentCloseTag))
                    _abTemporaryExponent = _TextConverter.ParseBase64Tag(sLine, csHybridFileExponentOpenTag, csHybridFileExponentCloseTag);
                else if (_TextConverter.IsTag(sLine, csHybridFileModulusOpenTag, csHybridFileModulusCloseTag))
                    _abTemporaryModulus = _TextConverter.ParseBase64Tag(sLine, csHybridFileModulusOpenTag, csHybridFileModulusCloseTag);
                else if (_TextConverter.IsTag(sLine, csHybridFileSignatureOpenTag, csHybridFileSignatureCloseTag))
                    _abTemporarySignature = _TextConverter.ParseBase64Tag(sLine, csHybridFileSignatureOpenTag, csHybridFileSignatureCloseTag);
            }

            if ((_abTemporaryExponent != null) && (_abTemporaryModulus != null) && (_abTemporarySignature != null))
            {
                NewKey = new CryptoKey(string.Empty, CryptoKey.nKeyFormat.Public, CryptoKey.nKeyType.AsymmetricPublic, _abTemporaryExponent, _abTemporaryModulus);
                qyFoundKeys = from k in _ltAllKeys where k.HashIdEquals(NewKey.abHashId) select k;
                if (qyFoundKeys.Count() == 0)
                    _SelectedAuthenticationKey = NewKey;
                else
                    _SelectedAuthenticationKey = qyFoundKeys.First();
                _SelectedAuthenticationKey.abSignature = _abTemporarySignature;
                _abTemporaryExponent = _abTemporaryModulus = null;

                _SelectedAuthenticationKey.abSignature = _abTemporarySignature;
                ResetTemporaryKeys();
            }

            return isOk ? nParserState.FromParameters : nParserState.Error;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private nParserState ParseHmacBase64(string sLine)
        {
            nParserState eReturn;

            if (string.IsNullOrEmpty(sLine) || (sLine.Length != (csSymmetricFileHmacOpenTag.Length + CryptoKey.ciAesKeyBase64Length + csSymmetricFileHmacCloseTag.Length)) || (sLine.Substring(0, csSymmetricFileHmacOpenTag.Length) != csSymmetricFileHmacOpenTag) || (sLine.Substring(sLine.Length - csSymmetricFileHmacCloseTag.Length) != csSymmetricFileHmacCloseTag))
            {
                eReturn = nParserState.Error;
            }
            else
            {
                _abTemporarySignature = _TextConverter.Base64StringToBytes(sLine.Substring(csSymmetricFileHmacOpenTag.Length, CryptoKey.ciAesKeyBase64Length));
                if ((_abTemporarySignature != null) && (_abTemporarySignature.Length == CryptoServices.ciAesKeyBytesLength))
                {
                    _eEncryptionType = nEncryptionType.FileSymmetric;
                    eReturn = nParserState.HmacTag;
                }
                else
                    eReturn = nParserState.Error;
            }
            return eReturn;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private nParserState ParseToParameters(string sLine)
        {
            bool isOk = !string.IsNullOrEmpty(sLine);            
            IEnumerable<CryptoKey> qyFoundKeys;
            CryptoKey[] aNewAsymmetricEncryptionKeys;
            CryptoKey NewKey;

            if (isOk)
            {
                if (_TextConverter.IsTag(sLine, csHybridFileExponentOpenTag, csHybridFileExponentCloseTag))
                    _abTemporaryExponent = _TextConverter.ParseBase64Tag(sLine, csHybridFileExponentOpenTag, csHybridFileExponentCloseTag);
                else if (_TextConverter.IsTag(sLine, csHybridFileModulusOpenTag, csHybridFileModulusCloseTag))
                    _abTemporaryModulus = _TextConverter.ParseBase64Tag(sLine, csHybridFileModulusOpenTag, csHybridFileModulusCloseTag);
                else if (_TextConverter.IsTag(sLine, csHybridFileWrappedKeyOpenTag, csHybridFileWrappedKeyCloseTag))
                    _abTemporaryWrappedKey = _TextConverter.ParseBase64Tag(sLine, csHybridFileWrappedKeyOpenTag, csHybridFileWrappedKeyCloseTag);
            }

            if ((_abTemporaryExponent != null) && (_abTemporaryModulus != null) && (_abTemporaryWrappedKey != null))
            {
                _eEncryptionType = nEncryptionType.FileAsymmetric;

                NewKey = new CryptoKey(string.Empty, CryptoKey.nKeyFormat.Private, CryptoKey.nKeyType.AsymmetricPrivate, _abTemporaryExponent, _abTemporaryModulus);
                qyFoundKeys = from k in _ltAllKeys where k.HashIdEquals(NewKey.abHashId) select k;

                foreach (CryptoKey Key in qyFoundKeys)
                {
                    if ((Key.eFormat == CryptoKey.nKeyFormat.Private) && (Key.eType == CryptoKey.nKeyType.AsymmetricPrivate))
                    {
                        NewKey = qyFoundKeys.First();
                        NewKey.abWrappedKey = _abTemporaryWrappedKey;
                        _SelectedEncryptionKey = NewKey;

                        // if (_SelectedAuthenticationKey != null)
                        //     _eEncryptionType = nEncryptionType.FileAsymmetric;
                    }
                }
                ResetTemporaryKeys();

                if (_aAsymmetricEncryptionKeys == null)
                {
                    aNewAsymmetricEncryptionKeys = new CryptoKey[1];
                    aNewAsymmetricEncryptionKeys[0] = NewKey;
                }
                else
                {
                    aNewAsymmetricEncryptionKeys = new CryptoKey[_aAsymmetricEncryptionKeys.Length + 1];
                    for (int i = 0; i < _aAsymmetricEncryptionKeys.Length; i++)
                        aNewAsymmetricEncryptionKeys[i] = _aAsymmetricEncryptionKeys[i];
                    aNewAsymmetricEncryptionKeys[_aAsymmetricEncryptionKeys.Length] = NewKey;
                }
                _aAsymmetricEncryptionKeys = aNewAsymmetricEncryptionKeys;
            }
            return isOk ? nParserState.ToParameters : nParserState.Error;
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public void ReadEncryptedIndex(PairOfFiles ParentDirectory, List<PairOfFiles> ltPairsRead, ConcurrentQueue<BackgroundMessage> quReturn)
        {
            byte[] abReadBuffer = new byte[4];
            int iIndexCount;
            PairOfFiles NewPair;
            BackgroundMessage.nReturnCode eDecryptionResult;
            IEnumerable<PairOfFiles> qyParentDirectory;

            eDecryptionResult = OpenEncryptedFile(true, quReturn);

            if ((eDecryptionResult == BackgroundMessage.nReturnCode.AuthenticationAndDecryptionSuccessful))
            {
                _AuthenticationStream.Read(abReadBuffer, 0, 4);
                iIndexCount = BitConverter.ToInt32(abReadBuffer, 0);
            
                for (int i = 0; i < iIndexCount; i++)
                {
                    NewPair = new PairOfFiles(_AuthenticationStream, ParentDirectory, _isSource, _TextConverter);
                    qyParentDirectory = from p in ltPairsRead where p.isDirectory && p.sRelativePath == NewPair.sParentDirectoryPath select p;
                    if (qyParentDirectory.Count() > 0)
                        NewPair.ParentDirectory = qyParentDirectory.First();
                    else
                        NewPair.ParentDirectory = ParentDirectory;

                    ltPairsRead.Add(NewPair);
                }
            }
            SendUserMessage(eDecryptionResult, quReturn);
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public PairOfFiles[] ReadEncryptedIndex(PairOfFiles ParentDirectory, ConcurrentQueue<BackgroundMessage> quReturn)
        {
            byte[] abReadBuffer = new byte[4];
            int iIndexCount;
            PairOfFiles NewPair;
            List<PairOfFiles> ltPairsRead = new List<PairOfFiles>();
            PairOfFiles[] Return = null;
            BackgroundMessage.nReturnCode eDecryptionResult;
            IEnumerable<PairOfFiles> qyParentDirectory;

            eDecryptionResult = OpenEncryptedFile(true, quReturn);

            if ((eDecryptionResult == BackgroundMessage.nReturnCode.AuthenticationAndDecryptionSuccessful))
            {
                _AuthenticationStream.Read(abReadBuffer, 0, 4);
                iIndexCount = BitConverter.ToInt32(abReadBuffer, 0);

                if (iIndexCount > 0)
                {
                    Return = new PairOfFiles[iIndexCount];

                    for (int i = 0; i < iIndexCount; i++)
                    {
                        NewPair = new PairOfFiles(_AuthenticationStream, ParentDirectory, _isSource, _TextConverter);

                        qyParentDirectory = from p in ltPairsRead where p.isDirectory && p.sRelativePath == NewPair.sParentDirectoryPath select p;
                        if (qyParentDirectory.Count() > 0)
                            NewPair.ParentDirectory = qyParentDirectory.First();
                        else
                            NewPair.ParentDirectory = ParentDirectory;

                        ltPairsRead.Add(NewPair);
                        Return[i] = NewPair;
                    }
                }
            }
            SendUserMessage(eDecryptionResult, quReturn);

            return Return;
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public byte[] ReadEncryptionHeader(Stream FromStream)
        {
            bool isReadingHeader = true;
            byte[] abHeader, abHeaderBlock, abNewHeader, abReturn;
            int i, iToIndex, iBlockBytesRead, iHeaderLength;

            abHeader = abReturn = null;
            abHeaderBlock = new byte[ciHeaderBlockSize];
            while (isReadingHeader && (iBlockBytesRead = FromStream.Read(abHeaderBlock, 0, ciHeaderBlockSize)) > 0)
            {
                if (iBlockBytesRead == ciHeaderBlockSize)
                {
                    if (abHeader == null)
                    {
                        abNewHeader = new byte[ciHeaderBlockSize];
                        iToIndex = 0;
                    }
                    else
                    {
                        abNewHeader = new byte[abHeader.Length + ciHeaderBlockSize];
                        for (i = 0; i < abHeader.Length; i++)
                            abNewHeader[i] = abHeader[i];
                        iToIndex = abHeader.Length;
                    }
                    for (i = 0; i < ciHeaderBlockSize; i++)
                        abNewHeader[iToIndex++] = abHeaderBlock[i];

                    abHeader = abNewHeader;
                    iHeaderLength = FindHeaderLength(abHeader);
                    if (iHeaderLength >= 0)
                    {
                        isReadingHeader = false;
                        if (iHeaderLength > 0)
                        {
                            abReturn = new byte[iHeaderLength];
                            for (i = 0; i < iHeaderLength; i++)
                                abReturn[i] = abHeader[i];
                        }
                    }
                }
                else
                    isReadingHeader = false;
            }
            return abReturn;
        }

        public bool ReadFileSystemIndex(Stream FromStream, List<PairOfFiles> ltEncryptedPairs)
        {
            bool isReturn = true;
            int i, iInventoryCount;
            // PairOfFiles NewPair;

            FromStream.Read(_abFileBlockBuffer, 0, ciVerificationBytesLength << 1);
            for (i = 0; i < ciVerificationBytesLength; i++)
                isReturn = isReturn && (_abFileBlockBuffer[i] == _abFileBlockBuffer[i + ciVerificationBytesLength]);

            if (isReturn)
            {
                FromStream.Read(_abFileBlockBuffer, 0, 28);
                _iFileSystemLevel = _abFileBlockBuffer[1];
                _iFileSystemBlockSize = (1 << _abFileBlockBuffer[2]);
                isReturn = (_abFileBlockBuffer[0] == (byte)ciFileSystemVersion) && (_iFileSystemLevel > 2) && (_iFileSystemLevel < 7) && (_abFileBlockBuffer[3] == (byte)ciDirectoryAttributesLength);

                if (isReturn)
                {
                    _uBlocksUsed = BitConverter.ToUInt32(_abFileBlockBuffer, 12);
                    _uReserveBlocks = BitConverter.ToUInt32(_abFileBlockBuffer, 16);
                    _iMinFreeBlockIndex = BitConverter.ToInt32(_abFileBlockBuffer, 20);
                    _iMaxUsedBlockIndex = BitConverter.ToInt32(_abFileBlockBuffer, 24);

                    if (ltEncryptedPairs != null)
                    {
                        _abBlockUsage = new byte[_iMaxUsedBlockIndex + 1];
                        FromStream.Read(_abBlockUsage, 0, _iMaxUsedBlockIndex + 1);

                        FromStream.Read(_abFileBlockBuffer, 0, 4);
                        iInventoryCount = BitConverter.ToInt32(_abFileBlockBuffer, 0);
                        // Console.WriteLine("iInventoryCount=" + iInventoryCount.ToString());

                        // while (iInventoryCount-- > 0)
                        // {
                        //     NewPair = new PairOfFiles(FromStream, ParentDirectory, _isSource, _TextConverter);
                        //     ltEncryptedPairs.Add(NewPair);
                        // }
                    }
                }
            }
           return isReturn;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private byte[] ReadFromBlocks(Stream DecryptionStream)
        {
            byte[] abHmacStored = null;
            int i, iBlockBytesRead, iBlockCount, iFilePosition;
            uint uThisBlockId;
            string sBlockPath;

            iBlockCount = 1;
            iFilePosition = 0;
            do
            {
                uThisBlockId = _quBlockIds.Dequeue();
                sBlockPath = GetBlockPath(uThisBlockId);

                if (!File.Exists(sBlockPath))
                {
                    _quBlockIds.Clear();
                }
                else
                {
                    using (FileStream SourceFileStream = new FileStream(sBlockPath, FileMode.Open, FileAccess.Read))
                    {
                        iBlockBytesRead = SourceFileStream.Read(_abFileBlockBuffer, 0, _iFileSystemBlockSize);
                        if (iBlockBytesRead != _iFileSystemBlockSize)
                            throw new FormatException("Read " + iBlockBytesRead.ToString() + " bytes in ReadFromBlocks() where it should have been " + _iFileSystemBlockSize.ToString() + " bytes.");

                        if (iBlockCount == 1)
                        {
                            abHmacStored = new byte[CryptoServices.ciAesKeyBytesLength];
                            for (i = 0; i < CryptoServices.ciAesKeyBytesLength; i++)
                                abHmacStored[i] = _abFileBlockBuffer[iFilePosition++];
                            for (i = 0; i < CryptoServices.ciIvOrSaltBytesLength; i++)
                                _abInitializationVector[i] = _abFileBlockBuffer[iFilePosition++];
                        }
                        DecryptionStream.Write(_abFileBlockBuffer, 0, iBlockBytesRead);
                        DecryptionStream.Flush();
                        iBlockCount++;
                    }
                }
            } while (_quBlockIds.Count > 0);
            return abHmacStored;
        }

        /// <summary></summary>
        protected void ReadKeys()
        {
            DirectoryInfo SettingsDirectoryInfo;

            if (Directory.Exists(_sSettingsDirectory))
            {
                SettingsDirectoryInfo = new DirectoryInfo(_sSettingsDirectory);
                ReadKeys(SettingsDirectoryInfo, CryptoKey.csBitLockerKeyFileExtension, CryptoKey.nKeyFormat.BitLocker);
                ReadKeys(SettingsDirectoryInfo, CryptoKey.csKeePassKeyFileExtension, CryptoKey.nKeyFormat.KeePass);
                ReadKeys(SettingsDirectoryInfo, CryptoKey.csPrivateKeyFileExtension, CryptoKey.nKeyFormat.Private);
                ReadKeys(SettingsDirectoryInfo, CryptoKey.csPublicKeyFileExtension, CryptoKey.nKeyFormat.Public);
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        protected void ReadKeys(DirectoryInfo SettingsDirectoryInfo, string sFileExtension, CryptoKey.nKeyFormat eKeyFormat)
        {
            CryptoKey NewKey;

            foreach (FileInfo KeyFileInfo in SettingsDirectoryInfo.GetFiles("*" + sFileExtension, SearchOption.TopDirectoryOnly))
            {
                NewKey = new CryptoKey(KeyFileInfo.FullName, eKeyFormat, this);
                if (NewKey.eType != CryptoKey.nKeyType.Invalid)
                    _ltKeysStored.Add(NewKey);
            }
        }

        /// <summary></summary>
        protected void ReadSettings()
        {

        }

        /// <summary></summary>
        /// <param name=""></param> 
        public string RemoveLastLevel(string sPath)
        {
            int iPos;
            string sReturn = string.Empty;

            if (!string.IsNullOrEmpty(sPath))
            {
                if (sPath[sPath.Length - 1] == _cDirectorySeparator)
                    sPath = sPath.Substring(0, sPath.Length - 1);

                iPos = sPath.LastIndexOf(_cDirectorySeparator);
                if (iPos > 0)
                    sReturn = sPath.Substring(0, iPos);
            }
            return sReturn;
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
        public void ResetTemporaryKeys()
        {
            _abTemporaryExponent = _abTemporaryModulus = _abTemporarySignature = _abTemporaryWrappedKey = null;
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void SendUserMessage(BackgroundMessage.nReturnCode eReturnCode, ConcurrentQueue<BackgroundMessage> quReturn)
        {
            BackgroundMessage NewMessage;

            if (eReturnCode != BackgroundMessage.nReturnCode.Empty)
            {
                NewMessage = new BackgroundMessage(BackgroundMessage.nType.UserMessage, eReturnCode);

                if ((eReturnCode == BackgroundMessage.nReturnCode.AuthenticationAndDecryptionSuccessful) ||
                     (eReturnCode == BackgroundMessage.nReturnCode.FileNotFound) ||
                     (eReturnCode == BackgroundMessage.nReturnCode.WrongFileFormat))
                    NewMessage.sText = sRootPathAndFile;

                if ((eReturnCode == BackgroundMessage.nReturnCode.FileNotFound) || (eReturnCode == BackgroundMessage.nReturnCode.NoAsymmetricKey) ||
                    (eReturnCode == BackgroundMessage.nReturnCode.NoAuthenticationKey) || (eReturnCode == BackgroundMessage.nReturnCode.NoSymmetricKey) ||
                    (eReturnCode == BackgroundMessage.nReturnCode.ProgrammingError) || (eReturnCode == BackgroundMessage.nReturnCode.UnspecifiedError) ||
                    (eReturnCode == BackgroundMessage.nReturnCode.WrongFileFormat))
                    NewMessage.iValue = 1;

                quReturn.Enqueue(NewMessage);
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public void SetBlockUsed(uint uBlockId, bool isUsed)
        {
            int i, iNewBaseIndex, iBitPosition, iBlockIndex;
            byte[] _abNewBlockUsage = null;


            iBitPosition = (int)(uBlockId & 0x7);
            iBlockIndex = (int)(uBlockId >> 3);

            if (_abBlockUsage == null)
            {
                iNewBaseIndex = 0;
                _abNewBlockUsage = new byte[ciBlockUsagePageSize];
            }
            else if (_abBlockUsage.Length <= iBlockIndex)
            {
                iNewBaseIndex = _abBlockUsage.Length;
                _abNewBlockUsage = new byte[iNewBaseIndex + ciBlockUsagePageSize];
                for (i = 0; i < iNewBaseIndex; i++)
                    _abNewBlockUsage[i] = _abBlockUsage[i];
            }
            else
                iNewBaseIndex = -1;

            if (iNewBaseIndex > -1)
            {
                for (i = iNewBaseIndex; i < _abNewBlockUsage.Length; i++)
                    _abNewBlockUsage[i] = 0;

                _abBlockUsage = _abNewBlockUsage;
            }

            if (isUsed)
            {
                _abBlockUsage[iBlockIndex] |= (byte)(1 << iBitPosition);
                _uBlocksUsed++;
                if (iBlockIndex > _iMaxUsedBlockIndex)
                    _iMaxUsedBlockIndex = iBlockIndex;
            }
            else
            {
                _abBlockUsage[iBlockIndex] &= (byte)~(1 << iBitPosition);
                _uBlocksUsed--;
                if (iBlockIndex == _iMaxUsedBlockIndex)
                {
                    while (_abBlockUsage[_iMaxUsedBlockIndex] == 0)
                        _iMaxUsedBlockIndex--;
                }
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        public void SetupFileSystem()
        {
            if (_isCanSetupEncryptedDirectory)
            {
                _isCanSetupEncryptedDirectory = false;

                if (_uBlocksUsed < 2)
                {
                    SetBlockUsed(0, true);
                    SetBlockUsed(1, true);
                }

                using (MemoryStream SourceStream = new MemoryStream())
                {
                    WriteEncryptionIndex(SourceStream, null);

                    SourceStream.Position = 0;
                    using (MemoryStream EncryptionStream = new MemoryStream())
                        EncryptToFileSystem(SourceStream, EncryptionStream, 0, null);

                    SourceStream.Position = 0;
                    using (MemoryStream EncryptionStream = new MemoryStream())
                        EncryptToFileSystem(SourceStream, EncryptionStream, 1, null);
                }
            }
        }

        /// <summary></summary>
        public override string ToString()
        {
            return _sName;
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public void WriteEncryptionIndex(Stream ToStream, List<PairOfFiles> ltEncryptedPairs)
        {
            byte[] abTrialHeader = new byte[ciVerificationBytesLength];

            _Cryptography.GetRandomBytes(abTrialHeader, ciVerificationBytesLength);
            ToStream.Write(abTrialHeader, 0, ciVerificationBytesLength);
            ToStream.Write(abTrialHeader, 0, ciVerificationBytesLength);

            ToStream.WriteByte((byte)ciFileSystemVersion);
            ToStream.WriteByte((byte)_iFileSystemLevel);
            ToStream.WriteByte(Log2(_iFileSystemBlockSize));
            ToStream.WriteByte((byte)ciDirectoryAttributesLength);   // in case we need to add more attributes later

            ToStream.Write(BitConverter.GetBytes(DateTime.Now.Ticks), 0, 8);
            ToStream.Write(BitConverter.GetBytes(_uBlocksUsed), 0, 4);
            ToStream.Write(BitConverter.GetBytes(_uReserveBlocks), 0, 4);
            ToStream.Write(BitConverter.GetBytes(_iMinFreeBlockIndex), 0, 4);
            ToStream.Write(BitConverter.GetBytes(_iMaxUsedBlockIndex), 0, 4);
            ToStream.Write(_abBlockUsage, 0, _iMaxUsedBlockIndex + 1);
            ToStream.Write(BitConverter.GetBytes(ltEncryptedPairs == null ? -1 : ltEncryptedPairs.Count), 0, 4);

            if (ltEncryptedPairs != null)
            {
                foreach (PairOfFiles Pair in ltEncryptedPairs)
                {
                    if (_isSource)
                        Pair.WriteSourceAttributes(ToStream);
                    else
                        Pair.WriteDestinationAttributes(ToStream);
                }
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void WriteToBlocks(Stream AuthenticationStream, byte[] abHmac)
        {
            int i, iBlockBytesRead, iBlockCount = 1;
            string sBlockPath;
            FileInfo BlockFileInfo;

            while ((iBlockBytesRead = AuthenticationStream.Read(_abFileBlockBuffer, 0, _iFileSystemBlockSize)) > 0)   // get block-sized chunks of data from SourceStream
            {
                if (iBlockBytesRead != _iFileSystemBlockSize)
                    throw new FormatException("Read " + iBlockBytesRead.ToString() + " bytes in WriteToBlocks() where it should have been " + _iFileSystemBlockSize.ToString() + " bytes.");

                if (iBlockCount == 1)   // if this is the first block, fill in the HMAC
                {
                    for (i = 0; i < CryptoServices.ciAesKeyBytesLength; i++)
                        _abFileBlockBuffer[i] = abHmac[i];
                }

                sBlockPath = GetBlockPath(_quBlockIds.Dequeue());
                using (FileStream DestinationFileStream = new FileStream(sBlockPath, FileMode.Create, FileAccess.Write))
                    DestinationFileStream.Write(_abFileBlockBuffer, 0, iBlockBytesRead);   // and write the block to disk

                BlockFileInfo = new FileInfo(sBlockPath);
                try
                {
                    BlockFileInfo.CreationTime = _ConstantFileDateTime;
                    BlockFileInfo.LastAccessTime = _ConstantFileDateTime;
                    BlockFileInfo.LastWriteTime = _ConstantFileDateTime;
                }
                catch { }
                iBlockCount++;
            }
        }
        #endregion
    }
}
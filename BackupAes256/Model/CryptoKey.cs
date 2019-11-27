namespace BackupAes256.Model
{
    using System;
    using System.IO;
    using System.Text;
    using System.Security.Cryptography;


    /// <summary>A cryptographic key.</summary>
    public class CryptoKey : IEquatable<CryptoKey>
    {
        public const int ciAesKeyBase64Length = (CryptoServices.ciAesKeyBytesLength + 2) / 3 * 4;   // length of an AES key in base-64 notation
        public const int ciMinimumBitLockerFileSize = 124;
        public const int ciBitLockerEntryHeaderSize = 32;
        public const int ciBitLockerEntrySize = 12;

        public const string csBitLockerKeyFileExtension = ".BEK";
        public const string csKeePassKeyFileExtension = ".key";
        public const string csPrivateKeyFileExtension = ".key";
        public const string csPublicKeyFileExtension = ".txt";

        private const string csKeyFileEncoding = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
        private const string csKeyFileOpenTag = "<KeyFile>";
        private const string csKeyFileMetaOpenTag = "<Meta>";
        private const string csKeyFileVersionOpenTag = "<Version>";
        private const string csKeyFileVersion = "1.00";
        private const string csKeyFileVersionCloseTag = "</Version>";
        private const string csKeyFileDescriptionOpenTag = "<Description>";
        private const string csKeyFileDescription = " key from project https://github.com/dasSubjekt/BackupAes256";
        private const string csKeyFileDescriptionCloseTag = "</Description>";
        private const string csKeyFileOwnerOpenTag = "<Owner>";
        private const string csKeyFileOwnerCloseTag = "</Owner>";
        private const string csKeyFileEmailOpenTag = "<Email>";
        private const string csKeyFileEmailCloseTag = "</Email>";
        private const string csKeyFileHomepageOpenTag = "<Homepage>";
        private const string csKeyFileHomepageCloseTag = "</Homepage>";
        private const string csKeyFileMetaCloseTag = "</Meta>";
        private const string csKeyFileKeyOpenTag = "<Key>";
        private const string csKeyFileDataOpenTag = "<Data>";
        private const string csKeyFileDataCloseTag = "</Data>";
        private const string csKeyFileDOpenTag = "<D>";
        private const string csKeyFileDCloseTag = "</D>";
        private const string csKeyFileDpOpenTag = "<DP>";
        private const string csKeyFileDpCloseTag = "</DP>";
        private const string csKeyFileDqOpenTag = "<DQ>";
        private const string csKeyFileDqCloseTag = "</DQ>";
        private const string csKeyFileExponentOpenTag = "<Exponent>";
        private const string csKeyFileExponentCloseTag = "</Exponent>";
        private const string csKeyFileInverseQOpenTag = "<InverseQ>";
        private const string csKeyFileInverseQCloseTag = "</InverseQ>";
        private const string csKeyFileModulusOpenTag = "<Modulus>";
        private const string csKeyFileModulusCloseTag = "</Modulus>";
        private const string csKeyFilePOpenTag = "<P>";
        private const string csKeyFilePCloseTag = "</P>";
        private const string csKeyFileQOpenTag = "<Q>";
        private const string csKeyFileQCloseTag = "</Q>";
        private const string csKeyFileKeyCloseTag = "</Key>";
        private const string csKeyFileCloseTag = "</KeyFile>";

        public enum nKeyFormat { BitLocker, KeePass, Password, Private, Public };
        public enum nKeyParameter { Symmetric, D, DP, DQ, Exponent, InverseQ, Modulus, P, Q, Owner, Email, Homepage };
        public enum nKeyType { Invalid, AsymmetricPrivate, AsymmetricPublic, Symmetric };
        private enum nParserState { Start, Header, FileOpenTag, MetaOpenTag, PropertyTags, MetaCloseTag, KeyOpenTag, ParameterTags, KeyCloseTag, FileCloseTag, Error };

        private bool _isNotSaved;
        private byte[] _abAesKey, _abHashId, _abSignature, _abWrappedKey;
        private int _iBytes;
        private string _sFileName, _sEmail, _sHomepage, _sName, _sOwner;
        private Drive _SavedOnDrive;
        private nKeyFormat _eFormat;
        private nKeyType _eType;
        private RSAParameters _RsaKey;
        private readonly TextConverter _TextConverter;

        #region constructors

        /// <summary></summary>
        private CryptoKey()
        {
            _isNotSaved = true;
            _abAesKey = _abHashId = _abSignature = _abWrappedKey = null;
            _iBytes = 0;
            _sFileName = _sName = string.Empty;
            _SavedOnDrive = null;
            _eFormat = nKeyFormat.KeePass;
            _eType = nKeyType.Invalid;
            _TextConverter = new TextConverter();
            _RsaKey = new RSAParameters();
            ResetAsymmetricKey();
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public CryptoKey(string sName, nKeyFormat eFormat, nKeyType eType, byte[] abAesKey256Bit) : this()
        {
            _sName = sName;
            _eFormat = eFormat;
            _eType = eType;

            if (abAesKey256Bit == null)
                throw new ArgumentNullException("Key required in class CryptoKey.");
            else
                abAesKey = abAesKey256Bit;
        }


        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public CryptoKey(string sName, nKeyFormat eFormat, nKeyType eType, int iBytes) : this()
        {
            _sName = sName;
            _eFormat = eFormat;
            _eType = eType;
            _iBytes = iBytes;
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public CryptoKey(string sName, nKeyFormat eFormat, nKeyType eType, byte[] abRsaExponent, byte[] abRsaModulus) : this()
        {
            if ((eFormat != nKeyFormat.Private) && (eFormat != nKeyFormat.Public))
                throw new ArgumentException("Variable eFormat=" + eFormat.ToString() + " incompatible with an asymmetric key in constructor for class CryptoKey.");
            else if ((eType !=  nKeyType.AsymmetricPrivate) && (eType != nKeyType.AsymmetricPublic))
                throw new ArgumentException("Variable eType=" + eType.ToString() + " incompatible with an asymmetric key in constructor for class CryptoKey.");
            else if (abRsaExponent == null)
                throw new ArgumentNullException("RSA exponent required in constructor for class CryptoKey.");
            else if (abRsaModulus == null)
                throw new ArgumentNullException("RSA modulus required in constructor for class CryptoKey.");

            _sName = sName;
            _eFormat = eFormat;
            _eType = eType;
            _RsaKey.Exponent = abRsaExponent;
            _RsaKey.Modulus = abRsaModulus;
            _iBytes = abRsaModulus.Length;
            CreateHashId();
        }


        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public CryptoKey(string sFilePath, nKeyFormat eFormat, Drive SavedOnDrive) : this()
        {
            if (string.IsNullOrEmpty(sFilePath))
                throw new ArgumentNullException("Path of the key file required in class CryptoKey.");

            _eFormat = eFormat;
            switch (_eFormat)
            {
                case nKeyFormat.BitLocker: ReadBitLockerKey(sFilePath); break;
                case nKeyFormat.KeePass: ReadKeePassKey(sFilePath); break;
                case nKeyFormat.Private: ReadAsymmetricKey(sFilePath, true); break;
                case nKeyFormat.Public: ReadAsymmetricKey(sFilePath, false); break;
            }
            _SavedOnDrive = SavedOnDrive;
        }
        #endregion

        #region operators
        
        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public static bool operator ==(CryptoKey First, CryptoKey Second)
        {
            if (((object)First) == null || ((object)Second) == null)
                return Equals(First, Second);
            else
                return First.Equals(Second);
        }
        
        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public static bool operator !=(CryptoKey First, CryptoKey Second)
        {
            if (((object)First) == null || ((object)Second) == null)
                return !Equals(First, Second);
            else
                return !(First.Equals(Second));
        }
        
        #endregion

        #region properties

        /// <summary></summary>
        public byte[] abAesKey
        {
            get { return GetKeyParameter(nKeyParameter.Symmetric); }
            set
            {
                if (_eFormat == nKeyFormat.Password)
                {
                    throw new ArgumentException("Cannot set abAesKey for key format " + _eFormat.ToString() + ".");
                }
                else if ((value == null) || (value.Length != CryptoServices.ciAesKeyBytesLength))
                {
                    _iBytes = 0;
                    _abAesKey = null;
                    _eType = nKeyType.Invalid;
                }
                else
                {
                    _iBytes = CryptoServices.ciAesKeyBytesLength;
                    _abAesKey = value;
                    _isNotSaved = true;
                    _eType = nKeyType.Symmetric;
                }
                ResetAsymmetricKey();
                CreateHashId();
            }
        }

        /// <summary></summary>
        public string sAesKeyBase64
        {
            get { return GetKeyParameter(nKeyParameter.Symmetric, 64); }
            set
            {
                byte[] abNewAesKey = _TextConverter.Base64StringToBytes(value);

                if ((abNewAesKey == null) || (abNewAesKey.Length != CryptoServices.ciAesKeyBytesLength))
                    throw new FormatException("'" + value + "' is not a base-64 256-bit key.");
                else
                    abAesKey = abNewAesKey;
            }
        }

        /// <summary></summary>
        public string sAesKeyBinary
        {
            get { return GetKeyParameter(nKeyParameter.Symmetric, 2); }
            set
            {
                bool isValid = true;
                byte[] abNewKey = null;
                int i, iByteValue;

                if (string.IsNullOrEmpty(value))
                {
                    abNewKey = null;
                }
                else if (value.Length == 8 * CryptoServices.ciAesKeyBytesLength)
                {
                    abNewKey = new byte[CryptoServices.ciAesKeyBytesLength];

                    for (i = 0; i < CryptoServices.ciAesKeyBytesLength; i++)
                    {
                        iByteValue = ParseBinary(value.Substring(i << 3, 8));

                        if ((iByteValue < byte.MinValue) || (iByteValue > byte.MaxValue))
                            isValid = false;
                        else
                            abNewKey[i] = (byte)iByteValue;
                    }
                }
                else
                    isValid = false;

                if (isValid)
                    abAesKey = abNewKey;
                else
                    throw new FormatException("'" + value + "' is not a binary 256-bit key.");
            }
        }

        /// <summary></summary>
        public string sAesKeyDecimal
        {
            get { return GetKeyParameter(nKeyParameter.Symmetric, 10); }
            set
            {
                bool isValid = true;
                byte[] abNewKey = null;
                DecimalInt KeyDecimal = null;

                if (string.IsNullOrEmpty(value))
                {
                    abNewKey = null;
                }
                else if (value.Length == ((int)(CryptoServices.ciAesKeyBytesLength * TextConverter.cdDecimalDigitsPerByte) + 1))
                {
                    try
                    {
                        KeyDecimal = new DecimalInt(value);
                    }
                    catch (FormatException)
                    {
                        isValid = false;
                    }

                    if (isValid)
                    {
                        abNewKey = KeyDecimal.GetHexadecimal(CryptoServices.ciAesKeyBytesLength);
                        isValid = (abNewKey != null) && (abNewKey.Length == CryptoServices.ciAesKeyBytesLength);
                    }
                }
                else
                    isValid = false;

                if (isValid)
                    abAesKey = abNewKey;
                else
                    throw new FormatException("'" + value + "' is not a decimal 256-bit key.");
            }
        }

        /// <summary></summary>
        public string sAesKeyHexadecimal
        {
            get { return GetKeyParameter(nKeyParameter.Symmetric, 16); }
            set
            {
                bool isValid = true;
                byte[] abNewKey = null;
                int i, iByteValue;

                if (string.IsNullOrEmpty(value))
                {
                    abNewKey = null;
                }
                else if (value.Length == (CryptoServices.ciAesKeyBytesLength << 1))
                {
                    abNewKey = new byte[CryptoServices.ciAesKeyBytesLength];

                    for (i = 0; i < CryptoServices.ciAesKeyBytesLength; i++)
                    {
                        iByteValue = ParseHexadecimal(value.Substring(i << 1, 2));

                        if ((iByteValue < byte.MinValue) || (iByteValue > byte.MaxValue))
                            isValid = false;
                        else
                            abNewKey[i] = (byte)iByteValue;
                    }
                }
                else
                    isValid = false;

                if (isValid)
                    abAesKey = abNewKey;
                else
                    throw new FormatException("'" + value + "' is not a hexadecimal 256-bit key.");
            }
        }

        /// <summary></summary>
        public int iBits
        {
            get { return _iBytes << 3; }
        }

        /// <summary></summary>
        public string sBits
        {
            get { return iBits.ToString(); }
        }

        /// <summary></summary>
        public int iBytes
        {
            get { return _iBytes; }
        }

        /// <summary></summary>
        public string sDrive
        {
            get
            {
                if (_SavedOnDrive == null)
                    return "---";
                else
                    return _SavedOnDrive.sName;
            }
        }

        /// <summary></summary>
        public string sEmail
        {
            get { return _sEmail; }
            set
            {
                _isNotSaved = true;
                _sEmail = value;
            }
        }

        /// <summary></summary>
        public string  sFileExtension
        {
            get
            {
                string sReturn = string.Empty;

                switch (_eFormat)
                {
                    case nKeyFormat.KeePass: sReturn = csKeePassKeyFileExtension; break;
                    case nKeyFormat.Private: sReturn = csPrivateKeyFileExtension; break;
                    case nKeyFormat.Public: sReturn = csPublicKeyFileExtension; break;
                }
                return sReturn;
            }
        }

        /// <summary></summary>
        public string sFileName
        {
            get { return _sFileName; }
            set { _sFileName = value; }
        }

        /// <summary></summary>
        public nKeyFormat eFormat
        {
            get { return _eFormat; }
        }

        /// <summary></summary>
        public byte[] abHashId
        {
            get { return _abHashId; }
        }

        /// <summary></summary>
        public string sHomepage
        {
            get { return _sHomepage; }
            set
            {
                _isNotSaved = true;
                _sHomepage = value;
            }
        }

        /// <summary></summary>
        public string sKeePassFileContent
        {
            get
            {
                StringBuilder KeyFileBuilder = new StringBuilder();

                if (_eType != nKeyType.Invalid)
                {
                    KeyFileBuilder.AppendLine(csKeyFileEncoding);
                    KeyFileBuilder.AppendLine(csKeyFileOpenTag);
                    KeyFileBuilder.AppendLine("\t" + csKeyFileMetaOpenTag);
                    KeyFileBuilder.AppendLine("\t\t" + csKeyFileVersionOpenTag + csKeyFileVersion + csKeyFileVersionCloseTag);
                    KeyFileBuilder.AppendLine("\t" + csKeyFileMetaCloseTag);
                    KeyFileBuilder.AppendLine("\t" + csKeyFileKeyOpenTag);
                    KeyFileBuilder.AppendLine("\t\t" + csKeyFileDataOpenTag + sAesKeyBase64 + csKeyFileDataCloseTag);
                    KeyFileBuilder.AppendLine("\t" + csKeyFileKeyCloseTag);
                    KeyFileBuilder.Append(csKeyFileCloseTag);
                }
                return KeyFileBuilder.ToString();
            }
        }

        /// <summary></summary>
        public string sName
        {
            get { return _sName; }
            set
            {
                _isNotSaved = true;
                _sName = value;
            }
        }

        /// <summary></summary>
        public bool isNotSaved
        {
            get { return _isNotSaved; }
            set { _isNotSaved = value; }
        }


        /// <summary></summary>
        public string sOwner
        {
            get { return _sOwner; }
            set
            {
                _isNotSaved = true;
                _sOwner = value;
            }
        }

        /// <summary></summary>
        public string sPrivateFileContent
        {
            get { return GetRsaFileContent(true); }
        }

        /// <summary></summary>
        public string sPublicFileContent
        {
            get { return GetRsaFileContent(false); }
        }

        /// <summary></summary>
        public byte[] abRsaD
        {
            get { return _RsaKey.D; }
            set { _RsaKey.D = value; }
        }

        /// <summary></summary>
        public string sRsaDBase64
        {
            get { return _TextConverter.BytesToBase64String(_RsaKey.D); }
            set { _RsaKey.D = _TextConverter.Base64StringToBytes(value); }
        }

        /// <summary></summary>
        public byte[] abRsaDp
        {
            get { return _RsaKey.DP; }
            set { _RsaKey.DP = value; }
        }

        /// <summary></summary>
        public string sRsaDpBase64
        {
            get { return _TextConverter.BytesToBase64String(_RsaKey.DP); }
            set { _RsaKey.DP = _TextConverter.Base64StringToBytes(value); }
        }

        /// <summary></summary>
        public byte[] abRsaDq
        {
            get { return _RsaKey.DQ; }
            set { _RsaKey.DQ = value; }
        }

        /// <summary></summary>
        public string sRsaDqBase64
        {
            get { return _TextConverter.BytesToBase64String(_RsaKey.DQ); }
            set { _RsaKey.DQ = _TextConverter.Base64StringToBytes(value); }
        }

        /// <summary></summary>
        public byte[] abRsaExponent
        {
            get { return _RsaKey.Exponent; }
            set
            {
                _RsaKey.Exponent = value;
                CreateHashId();
            }
        }

        /// <summary></summary>
        public string sRsaExponentBase64
        {
            get { return _TextConverter.BytesToBase64String(_RsaKey.Exponent); }
            set { _RsaKey.Exponent = _TextConverter.Base64StringToBytes(value); }
        }

        /// <summary></summary>
        public byte[] abRsaInverseQ
        {
            get { return _RsaKey.InverseQ; }
            set { _RsaKey.InverseQ = value; }
        }

        /// <summary></summary>
        public string sRsaInverseQBase64
        {
            get { return _TextConverter.BytesToBase64String(_RsaKey.InverseQ); }
            set { _RsaKey.InverseQ = _TextConverter.Base64StringToBytes(value); }
        }

        public RSAParameters RsaKey
        {
            get { return _RsaKey; }
            set { _RsaKey = value; }
        }

        /// <summary></summary>
        public byte[] abRsaModulus
        {
            get { return _RsaKey.Modulus; }
            set
            {
                _RsaKey.Modulus = value;
                _iBytes = value == null ? 0 : value.Length;
                CreateHashId();
            }
        }

        /// <summary></summary>
        public string sRsaModulusBase64
        {
            get { return _TextConverter.BytesToBase64String(_RsaKey.Modulus); }
            set { _RsaKey.Modulus = _TextConverter.Base64StringToBytes(value); }
        }

        /// <summary></summary>
        public byte[] abRsaP
        {
            get { return _RsaKey.P; }
            set { _RsaKey.P = value; }
        }

        /// <summary></summary>
        public string sRsaPBase64
        {
            get { return _TextConverter.BytesToBase64String(_RsaKey.P); }
            set { _RsaKey.P = _TextConverter.Base64StringToBytes(value); }
        }

        /// <summary></summary>
        public byte[] abRsaQ
        {
            get { return _RsaKey.Q; }
            set { _RsaKey.Q = value; }
        }

        /// <summary></summary>
        public string sRsaQBase64
        {
            get { return _TextConverter.BytesToBase64String(_RsaKey.Q); }
            set { _RsaKey.Q = _TextConverter.Base64StringToBytes(value); }
        }

        /// <summary></summary>
        public Drive SavedOnDrive
        {
            get { return _SavedOnDrive; }
            set { _SavedOnDrive = value; }
        }

        /// <summary></summary>
        public byte[] abSignature
        {
            get { return _abSignature; }
            set { _abSignature = value; }
        }

        /// <summary></summary>
        public nKeyType eType
        {
            get { return _eType; }
        }

        /// <summary></summary>
        public byte[] abWrappedKey
        {
            get { return _abWrappedKey; }
            set { _abWrappedKey = value; }
        }

        #endregion

        #region methods

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public bool HashIdEquals(byte[] abOtherHashId)
        {
            bool isReturn = (_abHashId != null) && (abOtherHashId != null) && (_abHashId.Length == abOtherHashId.Length);

            if (isReturn)
            {
                for (int i = 0; i < _abHashId.Length; i++)
                    isReturn = isReturn && (_abHashId[i] == abOtherHashId[i]);
            }

            return isReturn;
        }

        /// <summary></summary>
        /// <param name=""></param>
        public bool Equals(CryptoKey Other)
        {
            return (Other != null) && (_abHashId != null) && (Other.abHashId != null) && (_eType == Other.eType) && (_iBytes == Other.iBytes) && HashIdEquals(Other.abHashId);
        }

        /// <summary></summary>
        /// <param name=""></param>
        public override bool Equals(object Other)
        {
            if (Other == null)
                return false;
            else
            {
                CryptoKey OtherKey = Other as CryptoKey;
                if (OtherKey == null)
                    return false;
                else
                    return Equals(OtherKey);
            }
        }

        /// <summary></summary>
        private void CreateHashId()
        {
            int i;

            if ((_eType == nKeyType.Symmetric) && (_abAesKey != null) && (_iBytes == CryptoServices.ciAesKeyBytesLength))
            {
                _abHashId = new byte[CryptoServices.ciAesKeyBytesLength];
                for (i = 0; i < CryptoServices.ciAesKeyBytesLength; i++)
                    _abHashId[i] = _abAesKey[i];
            }
            else if (((_eType == nKeyType.AsymmetricPrivate) || (_eType == nKeyType.AsymmetricPublic)) && (_RsaKey.Exponent != null) && (_RsaKey.Modulus != null))
            {
                _abHashId = new byte[_RsaKey.Exponent.Length + _RsaKey.Modulus.Length];
                for (i = 0; i < _RsaKey.Modulus.Length; i++)
                    _abHashId[i] = _RsaKey.Modulus[i];
                for (i = 0; i < _RsaKey.Exponent.Length; i++)
                    _abHashId[_RsaKey.Modulus.Length + i] = _RsaKey.Exponent[i];
            }
            else
            {
                _abHashId = new byte[1];
                _abHashId[0] = (byte)_eType;
            }
        }
    
        /// <summary></summary>
        public override int GetHashCode()
        {
            return _abHashId.GetHashCode();
        }


        public byte[] GetKeyParameter(nKeyParameter eKeyParameter)
        {
            byte[] abReturn = null;

            if (_eType != nKeyType.Invalid)
            {
                switch (eKeyParameter)
                {
                    case nKeyParameter.D: abReturn = _RsaKey.D; break;
                    case nKeyParameter.DP: abReturn = _RsaKey.DP; break;
                    case nKeyParameter.DQ: abReturn = _RsaKey.DQ; break;
                    case nKeyParameter.Exponent: abReturn = _RsaKey.Exponent; break;
                    case nKeyParameter.InverseQ: abReturn = _RsaKey.InverseQ; break;
                    case nKeyParameter.Modulus: abReturn = _RsaKey.Modulus; break;
                    case nKeyParameter.P: abReturn = _RsaKey.P; break;
                    case nKeyParameter.Q: abReturn = _RsaKey.Q; break;
                    case nKeyParameter.Symmetric: abReturn = _abAesKey; break;
                }
            }
            return abReturn;
        }

        public string GetKeyParameter(nKeyParameter eKeyParameter, int iBase)
        {
            byte[] abKeyParameter = GetKeyParameter(eKeyParameter);
            string sReturn = string.Empty;

            if (_eType != nKeyType.Invalid)
            {
                switch (eKeyParameter)
                {
                    case nKeyParameter.Email: sReturn = _sEmail; break;
                    case nKeyParameter.Owner: sReturn = _sOwner; break;
                    case nKeyParameter.Homepage: sReturn = _sHomepage; break;
                    default:
                        switch (iBase)
                        {
                            case 2: sReturn = _TextConverter.BytesToBinaryString(abKeyParameter); break;
                            case 10: sReturn = _TextConverter.BytesToDecimalString(abKeyParameter); break;
                            case 16: sReturn = _TextConverter.BytesToHexadecimalString(abKeyParameter); break;
                            case 64: sReturn = _TextConverter.BytesToBase64String(abKeyParameter); break;
                            default: throw new NotImplementedException("Cannot convert key parameter " + eKeyParameter.ToString() + " into base " + iBase.ToString() + " format.");
                        }; break;
                }
            }
            return sReturn;
        }

        /// <summary></summary>
        private string GetRsaFileContent(bool isPrivate)
        {
            StringBuilder KeyFileBuilder = new StringBuilder();

            if (_eType != nKeyType.Invalid)
            {
                KeyFileBuilder.AppendLine(csKeyFileEncoding);
                KeyFileBuilder.AppendLine(csKeyFileOpenTag);
                KeyFileBuilder.AppendLine("\t" + csKeyFileMetaOpenTag);
                KeyFileBuilder.AppendLine("\t\t" + csKeyFileVersionOpenTag + csKeyFileVersion + csKeyFileVersionCloseTag);
                KeyFileBuilder.AppendLine("\t\t" + csKeyFileDescriptionOpenTag + (isPrivate ? "private" : "public") + csKeyFileDescription + csKeyFileDescriptionCloseTag);
                KeyFileBuilder.AppendLine("\t\t" + csKeyFileOwnerOpenTag + _TextConverter.StringToBase64String(_sOwner) + csKeyFileOwnerCloseTag);
                KeyFileBuilder.AppendLine("\t\t" + csKeyFileEmailOpenTag + _TextConverter.StringToBase64String(_sEmail) + csKeyFileEmailCloseTag);
                KeyFileBuilder.AppendLine("\t\t" + csKeyFileHomepageOpenTag + _TextConverter.StringToBase64String(_sHomepage) + csKeyFileHomepageCloseTag);
                KeyFileBuilder.AppendLine("\t" + csKeyFileMetaCloseTag);
                KeyFileBuilder.AppendLine("\t" + csKeyFileKeyOpenTag);
                if (isPrivate)
                {
                    KeyFileBuilder.AppendLine("\t\t" + csKeyFileDOpenTag + sRsaDBase64 + csKeyFileDCloseTag);
                    KeyFileBuilder.AppendLine("\t\t" + csKeyFileDpOpenTag + sRsaDpBase64 + csKeyFileDpCloseTag);
                    KeyFileBuilder.AppendLine("\t\t" + csKeyFileDqOpenTag + sRsaDqBase64 + csKeyFileDqCloseTag);
                }
                KeyFileBuilder.AppendLine("\t\t" + csKeyFileExponentOpenTag + sRsaExponentBase64 + csKeyFileExponentCloseTag);
                if (isPrivate)
                {
                    KeyFileBuilder.AppendLine("\t\t" + csKeyFileInverseQOpenTag + sRsaInverseQBase64 + csKeyFileInverseQCloseTag);
                }
                KeyFileBuilder.AppendLine("\t\t" + csKeyFileModulusOpenTag + sRsaModulusBase64 + csKeyFileModulusCloseTag);
                if (isPrivate)
                {
                    KeyFileBuilder.AppendLine("\t\t" + csKeyFilePOpenTag + sRsaPBase64 + csKeyFilePCloseTag);
                    KeyFileBuilder.AppendLine("\t\t" + csKeyFileQOpenTag + sRsaQBase64 + csKeyFileQCloseTag);
                }
                KeyFileBuilder.AppendLine("\t" + csKeyFileKeyCloseTag);
                KeyFileBuilder.Append(csKeyFileCloseTag);
            }
            return KeyFileBuilder.ToString();
        }

        /// <summary></summary>
        /// <param name=""></param>
        public bool IsReadOnly(nKeyParameter eKeyParameter)
        {
            return (eKeyParameter != nKeyParameter.Email) && (eKeyParameter != nKeyParameter.Homepage) && (eKeyParameter != nKeyParameter.Owner) && (eKeyParameter != nKeyParameter.Symmetric);
        }

        /// <summary></summary>
        /// <param name=""></param>
        private nParserState ParseAsynchronousParameter(string sLine, bool isPrivate)
        {
            bool isOk = !string.IsNullOrEmpty(sLine);

            if (isOk)
            {
                if (_TextConverter.IsTag(sLine, csKeyFileExponentOpenTag, csKeyFileExponentCloseTag))
                    abRsaExponent = _TextConverter.ParseBase64Tag(sLine, csKeyFileExponentOpenTag, csKeyFileExponentCloseTag);
                else if (_TextConverter.IsTag(sLine, csKeyFileModulusOpenTag, csKeyFileModulusCloseTag))
                    abRsaModulus = _TextConverter.ParseBase64Tag(sLine, csKeyFileModulusOpenTag, csKeyFileModulusCloseTag);
                else if (isPrivate)
                {
                    if (_TextConverter.IsTag(sLine, csKeyFileDOpenTag, csKeyFileDCloseTag))
                        abRsaD = _TextConverter.ParseBase64Tag(sLine, csKeyFileDOpenTag, csKeyFileDCloseTag);
                    else if (_TextConverter.IsTag(sLine, csKeyFileDpOpenTag, csKeyFileDpCloseTag))
                        abRsaDp = _TextConverter.ParseBase64Tag(sLine, csKeyFileDpOpenTag, csKeyFileDpCloseTag);
                    else if (_TextConverter.IsTag(sLine, csKeyFileDqOpenTag, csKeyFileDqCloseTag))
                        abRsaDq = _TextConverter.ParseBase64Tag(sLine, csKeyFileDqOpenTag, csKeyFileDqCloseTag);
                    else if (_TextConverter.IsTag(sLine, csKeyFileInverseQOpenTag, csKeyFileInverseQCloseTag))
                        abRsaInverseQ = _TextConverter.ParseBase64Tag(sLine, csKeyFileInverseQOpenTag, csKeyFileInverseQCloseTag);
                    else if (_TextConverter.IsTag(sLine, csKeyFilePOpenTag, csKeyFilePCloseTag))
                        abRsaP = _TextConverter.ParseBase64Tag(sLine, csKeyFilePOpenTag, csKeyFilePCloseTag);
                    else if (_TextConverter.IsTag(sLine, csKeyFileQOpenTag, csKeyFileQCloseTag))
                        abRsaQ = _TextConverter.ParseBase64Tag(sLine, csKeyFileQOpenTag, csKeyFileQCloseTag);
                    else
                        isOk = false;
                }
                else
                    isOk = false;

            }
            return isOk ? nParserState.ParameterTags : nParserState.Error;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private nParserState ParseAsynchronousProperty(string sLine)
        {
            bool isOk = !string.IsNullOrEmpty(sLine);

            if (isOk)
            {
                if (_TextConverter.IsTag(sLine, csKeyFileOwnerOpenTag, csKeyFileOwnerCloseTag))
                    sOwner = _TextConverter.BytesToString(_TextConverter.ParseBase64Tag(sLine, csKeyFileOwnerOpenTag, csKeyFileOwnerCloseTag));
                else if (_TextConverter.IsTag(sLine, csKeyFileEmailOpenTag, csKeyFileEmailCloseTag))
                    sEmail = _TextConverter.BytesToString(_TextConverter.ParseBase64Tag(sLine, csKeyFileEmailOpenTag, csKeyFileEmailCloseTag));
                else if (_TextConverter.IsTag(sLine, csKeyFileHomepageOpenTag, csKeyFileHomepageCloseTag))
                    sHomepage = _TextConverter.BytesToString(_TextConverter.ParseBase64Tag(sLine, csKeyFileHomepageOpenTag, csKeyFileHomepageCloseTag));
                else
                    isOk = _TextConverter.IsTag(sLine, csKeyFileVersionOpenTag, csKeyFileVersionCloseTag) || _TextConverter.IsTag(sLine, csKeyFileDescriptionOpenTag, csKeyFileDescriptionCloseTag);
            }
            return isOk ? nParserState.PropertyTags : nParserState.Error;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private int ParseBinary(string sBinary)
        {
            bool isValid;
            int i, iReturn = 0;
            char c;

            if (string.IsNullOrEmpty(sBinary) || (sBinary.Length > 31))
            {
                isValid = false;
            }
            else
            {
                isValid = true;
                for (i = 0; i < sBinary.Length; i++)
                {
                    c = sBinary[i];
                    if (c == '0')
                        iReturn <<= 1;
                    else if (c == '1')
                        iReturn = (iReturn << 1) + 1;
                    else
                        isValid = false;
                }
            }
            return isValid ? iReturn : - 1;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private int ParseHexadecimal(string sHexadecimal)
        {
            bool isValid;
            int i, iReturn = 0;
            char c;

            if (string.IsNullOrEmpty(sHexadecimal) || (sHexadecimal.Length > 7))
            {
                isValid = false;
            }
            else
            {
                isValid = true;
                for (i = 0; i < sHexadecimal.Length; i++)
                {
                    c = sHexadecimal[i];
                    if ((c >= '0') && (c <= '9'))
                        iReturn = 16 * iReturn + (c - '0');
                    else if ((c >= 'A') && (c <= 'F'))
                        iReturn = 16 * iReturn + (c - '7');
                    else if ((c >= 'a') && (c <= 'f'))
                        iReturn = 16 * iReturn + (c - 'W');
                    else
                        isValid = false;
                }
            }
            return isValid ? iReturn : -1;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private nParserState ParseKeePassKeyBase64(string sLine)
        {
            nParserState eReturn;

            if (string.IsNullOrEmpty(sLine) || (sLine.Length != (csKeyFileDataOpenTag.Length + ciAesKeyBase64Length + csKeyFileDataCloseTag.Length)) || (sLine.Substring(0, csKeyFileDataOpenTag.Length) != csKeyFileDataOpenTag) || (sLine.Substring(sLine.Length - csKeyFileDataCloseTag.Length) != csKeyFileDataCloseTag))
            {
                _abAesKey = null;
                _eType = nKeyType.Invalid;
                eReturn = nParserState.Error;
            }
            else
            {
                _eType = nKeyType.Symmetric;
                sAesKeyBase64 = sLine.Substring(csKeyFileDataOpenTag.Length, ciAesKeyBase64Length);
                eReturn = (_eType == nKeyType.Symmetric) ? nParserState.ParameterTags : nParserState.Error;
            }
            return eReturn;
        }


        /// <summary></summary>
        /// <param name=""></param>
        private void ReadAsymmetricKey(string sFilePath, bool isPrivate)
        {
            nParserState eParserState = nParserState.Start;
            string sCurrentLine;
            string[] asLines = null;
            string[] asDelimiters = { "\r\n", "\r", "\n" };

            if (File.Exists(sFilePath))
            {
                using (StreamReader KeePassStreamReader = new StreamReader(sFilePath))
                    asLines = KeePassStreamReader.ReadToEnd().Split(asDelimiters, StringSplitOptions.RemoveEmptyEntries);
            }

            if ((asLines != null) && (asLines.Length > 9))
            {
                for (int i = 0; i < asLines.Length; i++)
                {
                    sCurrentLine = asLines[i].Trim();

                    switch (eParserState)
                    {
                        case nParserState.Start: if (sCurrentLine == csKeyFileEncoding) eParserState = nParserState.Header; else eParserState = nParserState.Error; break;
                        case nParserState.Header: if (sCurrentLine == csKeyFileOpenTag) eParserState = nParserState.FileOpenTag; else eParserState = nParserState.Error; break;
                        case nParserState.FileOpenTag: if (sCurrentLine == csKeyFileMetaOpenTag) eParserState = nParserState.MetaOpenTag; else eParserState = nParserState.Error; break;
                        case nParserState.MetaOpenTag:
                        case nParserState.PropertyTags: if (sCurrentLine == csKeyFileMetaCloseTag) eParserState = nParserState.MetaCloseTag; else eParserState = ParseAsynchronousProperty(sCurrentLine); break;
                        case nParserState.MetaCloseTag: if (sCurrentLine == csKeyFileKeyOpenTag) eParserState = nParserState.KeyOpenTag; else eParserState = nParserState.Error; break;
                        case nParserState.KeyOpenTag:
                        case nParserState.ParameterTags: if (sCurrentLine == csKeyFileKeyCloseTag) eParserState = nParserState.KeyCloseTag; else eParserState = ParseAsynchronousParameter(sCurrentLine, isPrivate); break;
                        case nParserState.KeyCloseTag: if (sCurrentLine == csKeyFileCloseTag) eParserState = nParserState.FileCloseTag; break;   // no else: allow for lines in between
                        case nParserState.FileCloseTag: eParserState = nParserState.Error; break;   // nothing should come after the close tag
                    }
                }
            }

            if (eParserState == nParserState.FileCloseTag)
            {
                _sFileName = _sName = Path.GetFileNameWithoutExtension(sFilePath);
                _isNotSaved = false;

                if (isPrivate)
                {
                    _eFormat = nKeyFormat.Private;
                    _eType = nKeyType.AsymmetricPrivate;
                }
                else
                {
                    _eFormat = nKeyFormat.Public;
                    _eType = nKeyType.AsymmetricPublic;
                }
            }
            else
            {
                ResetAsymmetricKey();
                _eType = nKeyType.Invalid;
            }
            CreateHashId();
        }

        /// <summary></summary>
        /// <param name=""></param>
        private void ReadBitLockerKey(string sFilePath)
        {
            byte[] abBitLockerKey, abFileBuffer;
            int i, iFileSize0, iFileSize1, iFileSize2, iVersion, iHeaderSize, iEntrySize, iEntryType, iValueType, iEntryVersion, iEntryStart;
            FileInfo BitLockerFileInfo;

            if (File.Exists(sFilePath))
            {
                BitLockerFileInfo = new FileInfo(sFilePath);
                iFileSize0 = (int)BitLockerFileInfo.Length;

                if (iFileSize0 >= ciMinimumBitLockerFileSize)
                {
                    abFileBuffer = new byte[iFileSize0];
                    using (FileStream BitLockerStream = new FileStream(sFilePath, FileMode.Open, FileAccess.Read))
                        BitLockerStream.Read(abFileBuffer, 0, iFileSize0);

                    iFileSize1 = BitConverter.ToInt32(abFileBuffer, 0);
                    iVersion = BitConverter.ToInt32(abFileBuffer, 4);
                    iHeaderSize = BitConverter.ToInt32(abFileBuffer, 8);
                    iFileSize2 = BitConverter.ToInt32(abFileBuffer, 12);
                    iEntrySize = BitConverter.ToInt16(abFileBuffer, iHeaderSize);
                    iEntryType = BitConverter.ToInt16(abFileBuffer, iHeaderSize + 2);
                    iValueType = BitConverter.ToInt16(abFileBuffer, iHeaderSize + 4);
                    iEntryVersion = BitConverter.ToInt16(abFileBuffer, iHeaderSize + 6);

                    if ((iFileSize0 == iFileSize1) && (iFileSize0 == iFileSize2) && (iVersion == 1) && (iEntrySize == (iFileSize0 - iHeaderSize)) && (iEntryType == 6) && (iValueType == 9) && (iEntryVersion == 1))
                    {
                        iEntryStart = iHeaderSize + ciBitLockerEntryHeaderSize;
                        do
                        {
                            iEntrySize = BitConverter.ToInt16(abFileBuffer, iEntryStart);
                            iEntryType = BitConverter.ToInt16(abFileBuffer, iEntryStart + 2);
                            iValueType = BitConverter.ToInt16(abFileBuffer, iEntryStart + 4);
                            iEntryVersion = BitConverter.ToInt16(abFileBuffer, iEntryStart + 6);

                            if ((iEntrySize == (ciBitLockerEntrySize + CryptoServices.ciAesKeyBytesLength)) && (iEntryType == 0) && (iValueType == 1) && (iEntryVersion == 1))
                            {
                                abBitLockerKey = new byte[CryptoServices.ciAesKeyBytesLength];
                                for (i = 0; i < CryptoServices.ciAesKeyBytesLength; i++)
                                    abBitLockerKey[i] = abFileBuffer[iEntryStart + ciBitLockerEntrySize + i];
                                _eFormat = nKeyFormat.BitLocker;
                                _eType = nKeyType.Symmetric;
                                _sFileName = _sName = Path.GetFileNameWithoutExtension(sFilePath);
                                abAesKey = abBitLockerKey;
                                _isNotSaved = false;
                            }
                            iEntryStart += iEntrySize;
                        } while (iEntryStart < (iFileSize0 - ciBitLockerEntrySize));
                    }
                }
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        private void ReadKeePassKey(string sFilePath)
        {
            nParserState eParserState = nParserState.Start;
            string sCurrentLine;
            string[] asLines = null;
            string[] asDelimiters = { "\r\n", "\r", "\n" };

            if (File.Exists(sFilePath))
            {
                using (StreamReader KeePassStreamReader = new StreamReader(sFilePath))
                    asLines = KeePassStreamReader.ReadToEnd().Split(asDelimiters, StringSplitOptions.RemoveEmptyEntries);
            }

            if ((asLines != null) && (asLines.Length > 5))
            {
                for (int i = 0; i < asLines.Length; i++)
                {
                    sCurrentLine = asLines[i].Trim();

                    switch (eParserState)
                    {
                        case nParserState.Start: if (sCurrentLine == csKeyFileEncoding) eParserState = nParserState.Header; else eParserState = nParserState.Error; break;
                        case nParserState.Header: if (sCurrentLine == csKeyFileOpenTag) eParserState = nParserState.FileOpenTag; else eParserState = nParserState.Error; break;
                        case nParserState.FileOpenTag: if (sCurrentLine == csKeyFileKeyOpenTag) eParserState = nParserState.KeyOpenTag; break;   // no else: allow for lines in between
                        case nParserState.KeyOpenTag: eParserState = ParseKeePassKeyBase64(sCurrentLine); break;
                        case nParserState.ParameterTags: if (sCurrentLine == csKeyFileKeyCloseTag) eParserState = nParserState.KeyCloseTag; else eParserState = nParserState.Error; break;
                        case nParserState.KeyCloseTag: if (sCurrentLine == csKeyFileCloseTag) eParserState = nParserState.FileCloseTag; break;   // no else: allow for lines in between
                        case nParserState.FileCloseTag: eParserState = nParserState.Error; break;   // nothing should come after the close tag
                    }
                }
            }

            if (eParserState == nParserState.FileCloseTag)
            {
                _sFileName = _sName = Path.GetFileNameWithoutExtension(sFilePath);
                _eFormat = nKeyFormat.KeePass;
                _eType = nKeyType.Symmetric;
                _isNotSaved = false;
            }
            else
            {
                abAesKey = null;
                _eType = nKeyType.Invalid;
            }
        }

        /// <summary></summary>
        private void ResetAsymmetricKey()
        {
            _RsaKey.D = null;
            _RsaKey.DP = null;
            _RsaKey.DQ = null;
            _RsaKey.Exponent = null;
            _RsaKey.InverseQ = null;
            _RsaKey.Modulus = null;
            _RsaKey.P = null;
            _RsaKey.Q = null;
            _sEmail = _sHomepage = _sOwner = string.Empty;
        }


        /// <summary></summary>
        /// <param name=""></param> 
        public void Save(Drive SaveToDrive)
        {
            string sKeyPathFrom, sKeyPathTo;

            if (_SavedOnDrive != null)
            {
                sKeyPathFrom = _SavedOnDrive.ConcatenatePath(_SavedOnDrive.sSettingsDirectory, _sFileName + sFileExtension);
                if (File.Exists(sKeyPathFrom))
                    File.Delete(sKeyPathFrom);   // TODO exception handling
            }

            if (!Directory.Exists(SaveToDrive.sSettingsDirectory))
                Directory.CreateDirectory(SaveToDrive.sSettingsDirectory);
            sKeyPathTo = SaveToDrive.ConcatenatePath(SaveToDrive.sSettingsDirectory, _sName + sFileExtension);

            switch (_eFormat)
            {
                case nKeyFormat.KeePass: SaveKeePassKey(sKeyPathTo); break;
                case nKeyFormat.Private: SavePrivateKey(sKeyPathTo); break;
                case nKeyFormat.Public: SavePublicKey(sKeyPathTo); break;
                default: throw new NotImplementedException("Cannot save key of format " + _eFormat.ToString() + ".");
            }
            _isNotSaved = false;
            _sFileName = _sName;
            _SavedOnDrive = SaveToDrive;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private void SaveKeePassKey(string sKeyPath)
        {
            using (StreamWriter KeePassStreamWriter = new StreamWriter(sKeyPath))
                KeePassStreamWriter.Write(sKeePassFileContent);
        }

        /// <summary></summary>
        /// <param name=""></param>
        private void SavePrivateKey(string sKeyPath)
        {
            using (StreamWriter PrivateStreamWriter = new StreamWriter(sKeyPath))
                PrivateStreamWriter.Write(sPrivateFileContent);
        }

        /// <summary></summary>
        /// <param name=""></param>
        private void SavePublicKey(string sKeyPath)
        {
            using (StreamWriter PublicStreamWriter = new StreamWriter(sKeyPath))
                PublicStreamWriter.Write(sPublicFileContent);
        }

        /// <summary></summary>
        public override string ToString()
        {
            return _sName;
        }
        #endregion
    }
}
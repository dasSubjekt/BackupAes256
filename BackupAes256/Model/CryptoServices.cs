namespace BackupAes256.Model
{
    using System;
    using System.IO;
    using System.Security.Cryptography;


    /// <summary>All the cryptography in one class.</summary>
    public class CryptoServices
    {
        public const int ciIvOrSaltBytesLength = 16;     // 16 * 8 bits per byte = 128-bit, the only block size allowed for AES
                                                         // the salt size could be different, but this gives the same file size with a key or a password for encryption
        public const int ciAesKeyBytesLength = 32;       // 32 * 8 bits per byte = 256-bit encryption

        private AesCng _AesServices;
        private HMACSHA256 _HmacServices;
        private RNGCryptoServiceProvider _Randomness;
        private RSACng _RsaServices;
        private SHA256Cng _HashServices;

        #region constructors

        public CryptoServices()
        {
            _AesServices = new AesCng
            {
                BlockSize = ciIvOrSaltBytesLength << 3,
                KeySize = ciAesKeyBytesLength << 3,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            _HmacServices = new HMACSHA256();
            _RsaServices = new RSACng();
            _HashServices = new SHA256Cng();
            _Randomness = new RNGCryptoServiceProvider();
        }

        #endregion

        #region properties

        #endregion

        #region methods

        public byte[] ComputeHmac(Stream InputStream, CryptoKey AuthenticationKey)
        {
            byte[] abReturn = null;

            if (AuthenticationKey.eType == CryptoKey.nKeyType.Symmetric)
            {
                _HmacServices.Key = AuthenticationKey.abAesKey;
                abReturn = _HmacServices.ComputeHash(InputStream);
            }
            return abReturn;
        }

        public byte[] ComputeSHA256(byte[] abData)
        {
            return _HashServices.ComputeHash(abData);
        }

        /// <summary></summary>
        public ICryptoTransform CreateAesDecryptor(byte[] abInitializationVector, CryptoKey DecryptionKey)
        {
            if (DecryptionKey.eType == CryptoKey.nKeyType.Symmetric)
                return _AesServices.CreateDecryptor(DecryptionKey.abAesKey, abInitializationVector);
            else
                return null;
        }

        /// <summary></summary>
        public ICryptoTransform CreateAesEncryptor(byte[] abInitializationVector, CryptoKey EncryptionKey)
        {
            if (EncryptionKey.eType == CryptoKey.nKeyType.Symmetric)
                return _AesServices.CreateEncryptor(EncryptionKey.abAesKey, abInitializationVector);
            else
                return null;
        }

        /// <summary></summary>
        public byte[] DecryptRsa(CryptoKey DecryptionKey)
        {
            byte[] abReturn = null;

            if ((DecryptionKey.eType == CryptoKey.nKeyType.AsymmetricPrivate) && (DecryptionKey.abWrappedKey != null))
            {
                _RsaServices.ImportParameters(DecryptionKey.RsaKey);
                abReturn = _RsaServices.Decrypt(DecryptionKey.abWrappedKey, RSAEncryptionPadding.Pkcs1);
            }
            return abReturn;
        }

        /// <summary></summary>
        public void Dispose()
        {
            if (_AesServices != null)
            {
                _AesServices.Clear();
                _AesServices = null;
            }
            if (_HmacServices != null)
            {
                _HmacServices.Clear();
                _HmacServices = null;
            }
            if (_Randomness != null)
            {
                _Randomness.Dispose();
                _Randomness = null;
            }
            if (_RsaServices != null)
            {
                // _RsaServices.PersistKeyInCsp = false;
                _RsaServices.Clear();
                _RsaServices = null;
            }
        }

        public byte[] EncryptRsa(byte[] abData, CryptoKey EncryptionKey)
        {
            byte[] abReturn = null;
            string sReturn = string.Empty;

            if ((EncryptionKey.eType == CryptoKey.nKeyType.AsymmetricPrivate) || (EncryptionKey.eType == CryptoKey.nKeyType.AsymmetricPublic))
            {
                _RsaServices.ImportParameters(EncryptionKey.RsaKey);
                abReturn = _RsaServices.Encrypt(abData, RSAEncryptionPadding.Pkcs1);
            }
            return abReturn;
        }

        public void FillKey(CryptoKey Key)
        {
            byte[] abNewKey;
            RSAParameters NewRsaParameters;

            if (Key != null)
            {
                if (Key.iBytes == ciAesKeyBytesLength)
                {
                    abNewKey = new byte[ciAesKeyBytesLength];
                    GetRandomBytes(abNewKey);
                    Key.abAesKey = abNewKey;
                }
                else
                {
                    using (RSACryptoServiceProvider NewRsaServices = new RSACryptoServiceProvider(Key.iBytes << 3))
                    {
                        NewRsaParameters = NewRsaServices.ExportParameters(true);
                        Key.abRsaD = NewRsaParameters.D;
                        Key.abRsaDp = NewRsaParameters.DP;
                        Key.abRsaDq = NewRsaParameters.DQ;
                        Key.abRsaExponent = NewRsaParameters.Exponent;
                        Key.abRsaInverseQ = NewRsaParameters.InverseQ;
                        Key.abRsaModulus = NewRsaParameters.Modulus;
                        Key.abRsaP = NewRsaParameters.P;
                        Key.abRsaQ = NewRsaParameters.Q;
                        NewRsaServices.PersistKeyInCsp = false;
                        NewRsaServices.Clear();
                    }
                }
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        public void GetRandomBytes(byte[] abBuffer)
        {
            _Randomness.GetBytes(abBuffer);
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public void GetRandomBytes(byte[] abBuffer, int iCount)
        {
            if ((iCount < 0) || (iCount > abBuffer.Length))
                throw new ArgumentException("iCount=" + iCount.ToString() + " is out of range in GetRandomBytes()");
            else if (iCount > 0)
                _Randomness.GetBytes(abBuffer, 0, iCount);
        }

        /// <summary></summary>
        public byte[] SignRsa(Stream AuthenticationStream, CryptoKey AuthenticationKey)
        {
            byte[] abReturn = null;
            string sReturn = string.Empty;

            if ((AuthenticationStream != null) && (AuthenticationKey != null) && (AuthenticationKey.eType == CryptoKey.nKeyType.AsymmetricPrivate))
            {
                _RsaServices.ImportParameters(AuthenticationKey.RsaKey);
                abReturn = _RsaServices.SignData(AuthenticationStream, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            return abReturn;
        }

        /// <summary></summary>
        public bool VerifyRsa(Stream AuthenticationStream, CryptoKey AuthenticationKey)
        {
            bool isReturn = false;
            string sReturn = string.Empty;

            if ((AuthenticationStream != null) && (AuthenticationKey != null) && (AuthenticationKey.abSignature != null)
                && ((AuthenticationKey.eType == CryptoKey.nKeyType.AsymmetricPrivate) || (AuthenticationKey.eType == CryptoKey.nKeyType.AsymmetricPublic)))
            {
                _RsaServices.ImportParameters(AuthenticationKey.RsaKey);
                isReturn = _RsaServices.VerifyData(AuthenticationStream, AuthenticationKey.abSignature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            return isReturn;
        }
        #endregion
    }
}

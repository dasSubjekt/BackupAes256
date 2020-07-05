namespace BackupAes256.Model
{
    using System;
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
            _HashServices = new SHA256Cng();
            _Randomness = new RNGCryptoServiceProvider();
        }

        #endregion

        #region properties

        #endregion

        #region methods

        /// <summary></summary>
        public void Dispose()
        {
            if (_AesServices != null)
            {
                _AesServices.Clear();
                _AesServices = null;
            }
            if (_HashServices != null)
            {
                _HashServices.Dispose();
                _HashServices = null;
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
        }

        #endregion
    }
}

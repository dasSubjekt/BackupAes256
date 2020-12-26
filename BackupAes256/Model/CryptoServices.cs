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


        #region constructors

        public CryptoServices()
        {

        }

        #endregion

        #region properties

        #endregion

        #region methods

        /// <summary></summary>
        public void Dispose()
        {

        }

        #endregion
    }
}

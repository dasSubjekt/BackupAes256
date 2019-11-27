namespace BackupAes256.Model
{
    using System;
    using System.Text;


    /// <summary></summary>
    public class TextConverter
    {
        public const double cdDecimalDigitsPerByte = 2.4082399653118495617099111577959;   // = Math.Log(256) / Math.Log(10)

        private readonly UTF8Encoding _TextEncoder;

        #region constructors

        public TextConverter()
        {
            _TextEncoder = new UTF8Encoding();
        }

        #endregion

        #region properties

        #endregion

        #region methods

        /// <summary></summary>
        /// <param name=""></param>
        public byte[] Base64StringToBytes(string sBase64)
        {
            bool isValid = !string.IsNullOrEmpty(sBase64) && (sBase64.Length > 3) && ((sBase64.Length & 3) == 0);
            char c;
            int i, iPaddingLength = 0;

            if (isValid)
            {
                if (sBase64[sBase64.Length - 1] == '=')
                {
                    if (sBase64[sBase64.Length - 2] == '=')
                    {
                        iPaddingLength = 2;
                        c = sBase64[sBase64.Length - 3];
                        isValid = ((c == 'g') || (c == 'w') || (c == 'A') || (c == 'Q'));
                    }
                    else
                    {
                        iPaddingLength = 1;
                        c = sBase64[sBase64.Length - 2];
                        isValid = ((c == '0') || (c == '4') || (c == '8') || (c == 'c') || (c == 'g') || (c == 'k') || (c == 'o') || (c == 's') || (c == 'w') || (c == 'A') || (c == 'E') || (c == 'I') || (c == 'M') || (c == 'Q') || (c == 'U') || (c == 'Y'));
                    }
                }
            }

            if (isValid)
            {
                for (i = 0; i < (sBase64.Length - iPaddingLength - 1); i++)
                {
                    c = sBase64[i];
                    isValid = isValid && (((c >= '0') && (c <= '9')) || ((c >= 'A') && (c <= 'Z')) || ((c >= 'a') && (c <= 'z')) || (c == '+') || (c == '/'));
                }
            }

            if (isValid)
                return Convert.FromBase64String(sBase64);
            else
                return null;
        }

        /// <summary></summary>
        /// <param name=""></param>
        public string BytesToBase64String(byte[] abValue)
        {
            if (abValue == null)
                return string.Empty;
            else
                return Convert.ToBase64String(abValue);
        }

        /// <summary></summary>
        /// <param name=""></param>
        public string BytesToBinaryString(byte[] abValue)
        {
            int i;
            string sHexadecimal, sHalfByte;
            StringBuilder HexadecimalStringBuilder = new StringBuilder();

            if (abValue != null)
            {
                sHexadecimal = BytesToHexadecimalString(abValue);

                for (i = 0; i < sHexadecimal.Length; i++)
                {
                    switch (sHexadecimal[i])
                    {
                        case '0': sHalfByte = "0000"; break;
                        case '1': sHalfByte = "0001"; break;
                        case '2': sHalfByte = "0010"; break;
                        case '3': sHalfByte = "0011"; break;
                        case '4': sHalfByte = "0100"; break;
                        case '5': sHalfByte = "0101"; break;
                        case '6': sHalfByte = "0110"; break;
                        case '7': sHalfByte = "0111"; break;
                        case '8': sHalfByte = "1000"; break;
                        case '9': sHalfByte = "1001"; break;
                        case 'a': sHalfByte = "1010"; break;
                        case 'b': sHalfByte = "1011"; break;
                        case 'c': sHalfByte = "1100"; break;
                        case 'd': sHalfByte = "1101"; break;
                        case 'e': sHalfByte = "1110"; break;
                        case 'f': sHalfByte = "1111"; break;
                        default: sHalfByte = string.Empty; break;
                    }
                    HexadecimalStringBuilder.Append(sHalfByte);
                }
            }
            return HexadecimalStringBuilder.ToString();
        }

        /// <summary></summary>
        /// <param name=""></param>
        public string BytesToDecimalString(byte[] abValue)
        {
            string sReturn = string.Empty;
            DecimalInt Decimal;

            if (abValue != null)
            {
                Decimal = new DecimalInt(abValue);
                sReturn = Decimal.ToString();
                while (sReturn.Length < ((int)(abValue.Length * cdDecimalDigitsPerByte) + 1))
                    sReturn = "0" + sReturn;
            }
            return sReturn;
        }

        /// <summary></summary>
        /// <param name=""></param>
        public string BytesToHexadecimalString(byte[] abValue)
        {
            StringBuilder HexadecimalStringBuilder = new StringBuilder();

            if (abValue != null)
            {
                for (int i = 0; i < abValue.Length; i++)
                    HexadecimalStringBuilder.AppendFormat("{0:x2}", abValue[i]);
            }
            return HexadecimalStringBuilder.ToString();
        }

        /// <summary></summary>
        /// <param name=""></param>
        public string BytesToString(byte[] abText)
        {
            if (abText == null)
                return string.Empty;
            else
                return _TextEncoder.GetString(abText);
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public bool IsTag(string sLine, string sOpenTag, string sCloseTag)
        {
            return (sLine.Length >= sOpenTag.Length + sCloseTag.Length) && (sLine.Substring(0, sOpenTag.Length) == sOpenTag) && (sLine.Substring(sLine.Length - sCloseTag.Length) == sCloseTag);
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public byte[] ParseBase64Tag(string sLine, string sOpenTag, string sCloseTag)
        {
            if (IsTag(sLine, sOpenTag, sCloseTag))
                return Base64StringToBytes(sLine.Substring(sOpenTag.Length, sLine.Length - sOpenTag.Length - sCloseTag.Length));
            else
                return null;
        }

        /// <summary></summary>
        /// <param name=""></param>
        public byte[] StringToBytes(string sText)
        {
            if (string.IsNullOrEmpty(sText))
                return null;
            else
                return _TextEncoder.GetBytes(sText);
        }

        /// <summary></summary>
        /// <param name=""></param>
        public string StringToBase64String(string sText)
        {
            if (string.IsNullOrEmpty(sText))
                return string.Empty;
            else
                return Convert.ToBase64String(_TextEncoder.GetBytes(sText));
        }

        #endregion
    }
}

﻿namespace BackupAes256.Model
{
    using System;


    /// <summary>A multi-purpose property for an entry in a combo box or a data validation message.</summary>
    public class Property : IEquatable<Property>
    {
        private int _iId, _iNumber;
        string _sName, _sText;

        #region constructors

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public Property(int iId, int iNumber, string sName, string sText)
        {
            _iId = iId;
            _iNumber = iNumber;
            _sName = sName;
            _sText = sText;
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public Property(int iId, int iNumber, string sText) : this(iId, iNumber, string.Empty, sText)
        {
            _iId = iId;
            _iNumber = iNumber;
            _sText = sText;
        }

        /// <summary></summary>
        /// <param name=""></param>
        public Property(int iId, int iNumber) : this(iId, iNumber, string.Empty, string.Empty)
        {
        }

        /// <summary>Constructs a user message with a time stamp.</summary>
        /// <param name="CurrentTime">Time stamp</param>
        /// <param name="sText">Message to the user</param>
        public Property(DateTime CurrentTime, string sText) : this(-1, 0, string.Empty, sText)
        {
            _iId = 10000 * CurrentTime.Year + 100 * CurrentTime.Month + CurrentTime.Day;
            _iNumber = 10000000 * CurrentTime.Hour + 100000 * CurrentTime.Minute + 1000 * CurrentTime.Second + CurrentTime.Millisecond;
        }
        #endregion

        #region operators

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public static bool operator ==(Property First, Property Second)
        {
            if (((object)First) == null || ((object)Second) == null)
                return Equals(First, Second);
            else
                return First.Equals(Second);
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public static bool operator !=(Property First, Property Second)
        {
            if (((object)First) == null || ((object)Second) == null)
                return !Equals(First, Second);
            else
                return !(First.Equals(Second));
        }
        #endregion

        #region properties

        /// <summary></summary>
        public int iId
        {
            get { return _iId; }
        }

        /// <summary></summary>
        public string sId
        {
            get { return _iId.ToString(); }
        }

        /// <summary></summary>
        public int iNumber
        {
            get { return _iNumber; }
            set { _iNumber = value; }
        }

        /// <summary></summary>
        public string sNumber
        {
            get { return _iNumber.ToString(); }
        }

        /// <summary></summary>
        public string sName
        {
            get { return _sName; }
            set { _sName = value; }
        }

        /// <summary></summary>
        public string sText
        {
            get { return _sText; }
            set { _sText = value; }
        }

        /// <summary></summary>
        public string sTime
        {
            get
            {
                string sDigits = _iNumber.ToString("d9");
                return sDigits.Substring(0, 2) + ":" + sDigits.Substring(2, 2) + ":" + sDigits.Substring(4, 2) + "  " + sDigits.Substring(6, 3);
            }
        }
        #endregion

        #region methods

        /// <summary></summary>
        /// <param name=""></param>
        public bool Equals(Property Other)
        {
            return (Other != null) && (iId == Other.iId) && (sName == Other.sName);
        }

        /// <summary></summary>
        /// <param name=""></param>
        public override bool Equals(object Other)
        {
            if (Other == null)
                return false;
            else
            {
                Property OtherProperty = Other as Property;
                if (OtherProperty == null)
                    return false;
                else
                    return Equals(OtherProperty);
            }
        }

        /// <summary></summary>
        public override int GetHashCode()
        {
            return 3 * _iId.GetHashCode() + 5 * _sName.GetHashCode();
        }

        /// <summary></summary>
        public override string ToString()
        {
            return _sText;
        }
        #endregion
    }
}

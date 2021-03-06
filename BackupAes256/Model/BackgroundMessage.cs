namespace BackupAes256.Model
{
    using System;


    /// <summary>A data structure for exchanging information with <c>BackgroundThread</c>.</summary>
    public class BackgroundMessage
    {
        public enum nReturnCode { AesAuthenticated, Empty, FileNotFound, FinishCompare, ParsingSuccessful, ProgrammingError, StartCompare, UnspecifiedError, WrongFileFormat };
        public enum nType { Cancelled, Compare, NewPair, ReportProgress, SetupProgress, Status, Stop, Synchronize, UserMessage };

        private readonly nReturnCode _eReturnCode;
        private nType _eType;
        private int _iProgressMaximum, _iValue;
        private string _sText;
        private readonly DateTime _TimeStamp;
        private readonly Drive _DriveProperty;
        private PairOfFiles _PairProperty;


        #region constructors

        /// <summary>A constructor to initialize a <c>new BackgroundMessage</c>.</summary>
        /// <param name=""></param>
        public BackgroundMessage(nType eType)
        {
            _eType = eType;
            _eReturnCode = nReturnCode.Empty;
            _iValue = _iProgressMaximum = 0;
            _sText = string.Empty;
            _TimeStamp = DateTime.MinValue;
            _DriveProperty = null;
            _PairProperty = null;
        }

        /// <summary>A constructor to initialize a <c>new BackgroundMessage</c>.</summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public BackgroundMessage(nType eType, PairOfFiles PairProperty) : this(eType)
        {
            _PairProperty = PairProperty;
        }

        /// <summary>A constructor to initialize a <c>new BackgroundMessage</c>.</summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public BackgroundMessage(nType eType, Drive DriveProperty, int iValue) : this(eType)
        {
            _DriveProperty = DriveProperty;
            _iValue = iValue;
        }

        /// <summary>A constructor to initialize a <c>new BackgroundMessage</c>.</summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public BackgroundMessage(nType eType, int iValue, int iProgressMaximum = 0) : this(eType)
        {
            _iValue = iValue;
            _iProgressMaximum = iProgressMaximum;
        }

        /// <summary>A constructor to initialize a <c>new BackgroundMessage</c>.</summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public BackgroundMessage(nType eType, string sText) : this(eType)
        {
            _sText = sText;
        }

        /// <summary>A constructor to initialize a <c>new BackgroundMessage</c>.</summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public BackgroundMessage(nType eType, int iValue, string sText) : this(eType)
        {
            _iValue = iValue;
            _sText = sText;
        }

        /// <summary>A constructor to initialize a <c>new BackgroundMessage</c>.</summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public BackgroundMessage(nType eType, nReturnCode eReturnCode) : this(eType)
        {
            _eReturnCode = eReturnCode;
            _TimeStamp = DateTime.Now;
        }

        /// <summary>A constructor to initialize a <c>new BackgroundMessage</c>.</summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public BackgroundMessage(nType eType, nReturnCode eReturnCode, string sText) : this(eType)
        {
            _eReturnCode = eReturnCode;
            _sText = sText;
            _TimeStamp = DateTime.Now;
        }

        #endregion

        #region properties

        /// <summary></summary>
        public Drive DriveProperty
        {
            get { return _DriveProperty; }
        }

        /// <summary></summary>
        public PairOfFiles PairProperty
        {
            get { return _PairProperty; }
        }

        /// <summary></summary>
        public int iProgressMaximum
        {
            get { return _iProgressMaximum; }
            set { _iProgressMaximum = value; }
        }

        /// <summary></summary>
        public nReturnCode eReturnCode
        {
            get { return _eReturnCode; }
        }

        /// <summary></summary>
        public string sText
        {
            get { return _sText; }
            set { _sText = value; }
        }

        /// <summary></summary>
        public DateTime TimeStamp
        {
            get { return _TimeStamp; }
        }

        /// <summary></summary>
        public nType eType
        {
            get { return _eType; }
            set { _eType = value; }
        }

        /// <summary></summary>
        public int iValue
        {
            get { return _iValue; }
            set { _iValue = value; }
        }
        #endregion
    }
}

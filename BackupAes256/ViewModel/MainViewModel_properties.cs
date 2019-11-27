namespace BackupAes256.ViewModel
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Reflection;
    using BackupAes256.Model;
    using System.Windows.Input;
    using System.ComponentModel;
    using System.Windows.Threading;
    using System.Collections.Generic;
    using System.Windows.Media.Imaging;


    public partial class MainViewModel : ViewModelBase
    {
        private const int ciDefaultRowHeight = 45;
        private const int ciProgrssBarDefaultMaximum = 1;

        /// <summary>Enumerated type of menu tabs. The predefined numbers are for <c>TabControl.SelectedIndex</c>.</summary>
        public enum nMenuTab { Task = 0, Progress = 1, Messages = 2, Keys = 3 };

        private bool _isDragOverTasks, _isKeyValueFocused, _isProgressBarIndeterminate;
        private int _iKeyNumeralIfCanHide, _iProgressBarValue, _iProgressBarMaximum, _iSelectedKeyNumeral, _iSelectedNewKeyBytes, _iWorkingMemoryLimit;
        private readonly int[] _aiPairsCount, _aiPairsOrder;
        private string _sApplicationDirectory, _sBackgroundStatus, _sDestinationFileOrDirectory, _sKeyValue, _sSourceFileOrDirectory, _sTaskName, _sTemporaryDirectory;
        private nMenuTab _eMenuTab;
        private PairOfFiles.nComparison _eCaseTab;
        private PairOfFiles.nSynchronizationMode _eSynchronizationMode;
        private CryptoKey.nKeyParameter _eSelectedKeyParameterAsymmetric;
        private BackgroundThread _BackgroundThread;
        private BackgroundMessage.nType _eBackgroundTask;
        private Drive _DestinationDrive, _SourceDrive, _SelectedDrive, _SelectedKeyDrive;
        private CryptoKey _SelectedKey, _SelectedKeyFileAesKey;
        private Property _SelectedTask;
        private readonly List<Drive> _ltDrives;
        private readonly List<CryptoKey> _ltKeys;
        private readonly List<PairOfFiles> _ltPairs;
        private readonly BindingList<Drive> _blDrives;
        private readonly BindingList<CryptoKey> _blAsymmetricKeys, _blKeys, _blPrivateKeys, _blSymmetricKeys;
        private readonly BindingList<PairOfFiles> _blPairs;
        private BindingList<Property> _blKeyNumerals;
        private readonly BindingList<Property> _blBlockSizes, _blDestinationOptions, _blFileSystemLevels, _blKeyNumeralsPrivate, _blKeyNumeralsPublic, _blKeyTextPublic, _blKeyParametersAsymmetric, _blMemoryLimits, _blMessages, _blNewKeyTypes, _blSourceOptions, _blTasks;
        private CryptoServices _Cryptography;

        #region properties

        private readonly DispatcherTimer _UserInterfaceTimer;
        private readonly ICollectionView _ViewSourceKeys;
        private BitmapSource _IconKeys, _IconMessages, _IconProgress, _IconTask;

        public ICommand dcCompare { get; }
        public ICommand dcExportKey { get; }
        public ICommand dcF5 { get; }
        public ICommand dcNewKey { get; }
        public ICommand dcReadDrivesAndKeys { get; }
        public ICommand dcSaveKey { get; }
        public ICommand dcSaveTask { get; }
        public ICommand dcSelectDestination { get; }
        public ICommand dcSelectSource { get; }
        public ICommand dcSelectTemporary { get; }
        public ICommand dcSwap { get; }
        public ICommand dcSynchronizeCancelOrRecompare { get; }

        public BitmapSource IconKeys { get => _IconKeys; }
        public BitmapSource IconMessages { get => _IconMessages; }
        public BitmapSource IconProgress { get => _IconProgress; }
        public BitmapSource IconTask { get => _IconTask; }

        public string sAsymmetric { get => Translate("Asymmetric"); }
        public string sAuthenticationAndDecryptionSuccessful { get => Translate("AuthenticationAndDecryptionSuccessful"); }
        public string sAuthenticationKey { get => Translate("AuthenticationKey"); }
        public string sBitsText { get => Translate("BitsText"); }
        public string sBlocks { get => Translate("Blocks"); }
        public string sBlockSize { get => Translate("BlockSize"); }
        public string sCancel { get => Translate("Cancel"); }
        public string sClose { get => Translate("Close"); }
        public string sComingSoon { get => Translate("ComingSoon"); }
        public string sCompare { get => Translate("Compare"); }
        public string sCompareAgain { get => Translate("CompareAgain"); }
        public string sCreate { get => Translate("Create"); }
        public string sDestinationDateText { get => Translate("DestinationDateText"); }
        public string sDestinationSizeText { get => Translate("DestinationSizeText"); }
        public string sDriveText { get => Translate("DriveText"); }
        public string sEncrypted { get => Translate("Encrypted"); }
        public string sFinishCompare { get => Translate("FinishCompare"); }
        public string sFinishDecryption { get => Translate("FinishDecryption"); }
        public string sFinishEncryption { get => Translate("FinishEncryption"); }
        public string sFinishFillKey { get => Translate("FinishFillKey"); }
        public string sErrorMessageText { get => Translate("ErrorMessageText"); }
        public string sErrorWorkingMemoryLimit { get => Translate("ErrorMessageText"); }
        public string sExport { get => Translate("Export"); }
        public string sFileNotFound { get => Translate("FileNotFound"); }
        public string sFormatText { get => Translate("FormatText"); }
        public string sFoundAuthenticationKey { get => Translate("FoundAuthenticationKey"); }
        public string sFoundPrivateKey { get => Translate("FoundPrivateKey"); }
        public string sFoundSymmetricKey { get => Translate("FoundSymmetricKey"); }
        public string sIterations { get => Translate("Iterations"); }
        public string sKeyNameText { get => Translate("KeyNameText"); }
        public string sKeys { get => Translate("Keys"); }
        public string sMaximumBlockCount { get => Translate("MaximumBlockCount"); }
        public string sMessages { get => Translate("Messages"); }
        public string sMessageText { get => Translate("MessageText"); }
        public string sModeNoDelete { get => Translate("ModeNoDelete"); }
        public string sModeTwoWay { get => Translate("ModeTwoWay"); }
        public string sModeWithDelete { get => Translate("ModeWithDelete"); }
        public string sNewKey { get => Translate("NewKey"); }
        public string sNoAsymmetricKey { get => Translate("NoAsymmetricKey"); }
        public string sNoAuthenticationKey { get => Translate("NoAuthenticationKey"); }
        public string sNoSymmetricKey { get => Translate("NoSymmetricKey"); }
        public string sOnDrive { get => Translate("OnDrive"); }
        public string sProgrammingError { get => Translate("ProgrammingError"); }
        public string sProgress { get => Translate("Progress"); }
        public string sReadDrives { get => Translate("ReadDrivesAndKeys"); }
        public string sRelativePathText { get => Translate("RelativePathText"); }
        public string sReserve { get => Translate("Reserve"); }
        public string sSave { get => Translate("Save"); }
        public string sSelect { get => Translate("Select"); }
        public string sSelectDestinationDirectory { get => Translate("SelectDestinationDirectory"); }
        public string sSelectDestinationFile { get => Translate("SelectDestinationFile"); }
        public string sSelectSourceDirectory { get => Translate("SelectSourceDirectory"); }
        public string sSelectSourceFile { get => Translate("SelectSourceFile"); }
        public string sSelectTemporaryDirectory { get => Translate("SelectTemporaryDirectory"); }
        public string sSize { get => Translate("Size"); }
        public string sSourceDateText { get => Translate("SourceDateText"); }
        public string sSourceSizeText { get => Translate("SourceSizeText"); }
        public string sStartCompare { get => Translate("StartCompare"); }
        public string sStartDecryption { get => Translate("StartDecryption"); }
        public string sStartEncryption { get => Translate("StartEncryption"); }
        public string sStartFillKey { get => Translate("StartFillKey"); }
        public string sSwap { get => Translate("Swap"); }
        public string sSymmetric { get => Translate("Symmetric"); }
        public string sSynchronize { get => Translate("Synchronize"); }
        public string sTask { get => Translate("Task"); }
        public string sTaskNameText { get => Translate("TaskNameText"); }
        public string sTemporaryDirectoryText { get => Translate("TemporaryDirectoryText"); }
        public string sTimeText { get => Translate("TimeText"); }
        public string sTypeText { get => Translate("TypeText"); }
        public string sUnspecifiedError { get => Translate("UnspecifiedError"); }
        public string sUseWorkingMemory { get => Translate("UseWorkingMemory"); }
        public string sWindowTitle { get => Translate("WindowTitle"); }
        public string sWrongFileFormat { get => Translate("WrongFileFormat"); }


        /// <summary></summary>
        public string[] asAllowedFileExtensions
        {
            get { return new string[2] { Drive.csSymmetricFileExtension, Drive.csAsymmetricFileExtension }; }
        }

        /// <summary></summary>
        public BindingList<CryptoKey> blAuthenticationKeys
        {
            get { return _DestinationDrive.eEncryptionType == Drive.nEncryptionType.FileAsymmetric ? _blPrivateKeys : _blSymmetricKeys; }
        }

        /// <summary></summary>
        public string sBackgroundStatus
        {
            get { return _sBackgroundStatus; }
            set
            {
                if (value != _sBackgroundStatus)
                {
                    _sBackgroundStatus = value;
                    NotifyPropertyChanged("sStatus");
                }
            }
        }

        /// <summary></summary>
        public BindingList<Property> blBlockSizes
        {
            get { return _blBlockSizes; }
        }

        /// <summary></summary>
        public string sCapacityInformation
        {
            get
            {
                double dCapacity = _DestinationDrive.uFileSystemMaxBlocks * _DestinationDrive.iFileSystemBlockSize * 100.0d / _DestinationDrive.kTotalSize;

                return string.Format(Translate("CapacityInformation"), dCapacity, _DestinationDrive.sName);
            }
        }

        /// <summary></summary>
        public PairOfFiles.nComparison eCaseTab
        {
            get { return _eCaseTab; }
            set
            {
                if (value != _eCaseTab)
                {
                    IEnumerable<PairOfFiles> qyFoundPairs = from p in _ltPairs where p.eComparison == value orderby p.sRelativePath select p;
                    _eCaseTab = value;

                    _blPairs.Clear();
                    foreach (PairOfFiles FoundPair in qyFoundPairs)
                        _blPairs.Add(FoundPair);

                    NotifyPropertyChanged("iCaseTab");
                    NotifyPropertyChanged("sModeInformation");
                    NotifyPropertyChanged("VisibleWhenTabErrorSelected");
                    NotifyPropertyChanged("VisibleWhenTabsIncludedSelected");
                }
            }
        }

        /// <summary></summary>
        public int iCaseTab
        {
            get { return (int)_eCaseTab; }
            set { eCaseTab = (PairOfFiles.nComparison)value; }
        }

        /// <summary></summary>
        public string sCreateOrCancel
        {
            get
            {
                if ((_BackgroundThread.eState != BackgroundThread.nState.Idle) && (_eBackgroundTask == BackgroundMessage.nType.FillKey))
                    return sCancel;
                else
                    return sCreate;
            }
        }

        /// <summary></summary>
        public bool isDestinationADirectory
        {
            get
            {
                return (_DestinationDrive.eEncryptionType == Drive.nEncryptionType.DirectoryUnencrypted) || (_DestinationDrive.eEncryptionType == Drive.nEncryptionType.DirectorySymmetric);
            }
        }

        /// <summary></summary>
        public bool isDestinationAFile
        {
            get
            {
                return (_DestinationDrive.eEncryptionType == Drive.nEncryptionType.FileAsymmetric) || (_DestinationDrive.eEncryptionType == Drive.nEncryptionType.FileSymmetric);
            }
        }

        /// <summary></summary>
        public bool isDestinationEncrypted
        {
            get { return (_DestinationDrive.eEncryptionType != Drive.nEncryptionType.DirectoryUnencrypted); }
        }

        /// <summary></summary>
        public string sDestinationFileOrDirectory
        {
            get { return _sDestinationFileOrDirectory; }
            set
            {
                if (value != _sDestinationFileOrDirectory)
                {
                    _sDestinationFileOrDirectory = value;

                    if (value != _DestinationDrive.sRootPathAndFile)
                        _DestinationDrive.sRootPathAndFile = value;

                    if (string.IsNullOrEmpty(_DestinationDrive.sEncryptedFileName))
                    {
                        // ValidateRaiseErrorsChanged(nValidationType.Single, "sDestinationFileOrDirectory", Directory.Exists(value), Translate("DestinationDirectoryMissing"));
                        ValidateRaiseErrorsChanged(nValidationType.Single, "sFileOrDirectory", IsSynchronizationAllowed(sSourceFileOrDirectory, _sDestinationFileOrDirectory), Translate("ErrorDirectoriesIdentical"));
                        // eSelectedDestinationEncryptionType = Drive.nEncryptionType.DirectoryUnencrypted;
                    }
                    else
                    {
                        // ValidateRaiseErrorsChanged(nValidationType.Single, "sDestinationFileOrDirectory", File.Exists(value), Translate("DestinationFileMissing"));
                        ValidateRaiseErrorsChanged(nValidationType.Single, "sFileOrDirectory", _sDestinationFileOrDirectory != _sSourceFileOrDirectory, Translate("ErrorFilesIdentical"));
                    }
                    NotifyPropertyChanged("sStatus");
                    NotifyPropertyChangedEncryptionType();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary></summary>
        public BindingList<Property> blDestinationOptions
        {
            get { return _blDestinationOptions; }
        }

        /// <summary></summary>
        public bool isDragOverTasks
        {
            get { return _isDragOverTasks; }
            set
            {
                if (value != _isDragOverTasks)
                {
                    _isDragOverTasks = value;

                    if (_isDragOverTasks && (eMenuTab != nMenuTab.Task))
                        eMenuTab = nMenuTab.Task;

                    NotifyPropertyChanged("isDragOverTasks");
                }
            }
        }

        /// <summary></summary>
        public BindingList<Drive> blDrives
        {
            get { return _blDrives; }
        }

        /// <summary></summary>
        public bool isEditKeys
        {
            get { return isDestinationAFile; }   // || _DestinationDrive.isCanSetupEncryptedDirectory
        }

        public string sEncryptionKey
        {
            get { return _DestinationDrive.eEncryptionType == Drive.nEncryptionType.FileAsymmetric ? Translate("RsaKey") : Translate("AesKey"); }
        }

        /// <summary></summary>
        public BindingList<CryptoKey> blEncryptionKeys
        {
            get { return _DestinationDrive.eEncryptionType == Drive.nEncryptionType.FileAsymmetric ? _blAsymmetricKeys : _blSymmetricKeys; }
        }

        /// <summary></summary>
        public bool isExecuteCancelSynchronize
        {
            get
            {
                return ((_eBackgroundTask == BackgroundMessage.nType.Compare) || (_eBackgroundTask == BackgroundMessage.nType.Synchronize)) && (_BackgroundThread.eState != BackgroundThread.nState.CancelRequested);
            }
        }

        /// <summary></summary>
        public bool isExecuteCompare
        {
            get
            {
                return (_BackgroundThread.eState == BackgroundThread.nState.Idle) && !HasErrors
                    && ((isSourceADirectory && Directory.Exists(_SourceDrive.sRootPath)) || File.Exists(_SourceDrive.sRootPathAndFile))
                    && ((isDestinationADirectory && Directory.Exists(_DestinationDrive.sRootPath)) || ((_DestinationDrive.SelectedAuthenticationKey != null)
                    && (_DestinationDrive.SelectedEncryptionKey != null) && (_DestinationDrive.SelectedAuthenticationKey != _DestinationDrive.SelectedEncryptionKey)));
            }
        }

        /// <summary></summary>
        public bool isExecuteExportKey
        {
            get
            {
                return (_SelectedKey != null) && (_SelectedKey.eFormat == CryptoKey.nKeyFormat.Private) && !string.IsNullOrEmpty(_SelectedKey.sName) && !_SelectedKeyDrive.isReadOnly && (_ltKeys.SingleOrDefault(key => (key.sName == _SelectedKey.sName) && (key.eType == CryptoKey.nKeyType.AsymmetricPublic)) == null);
            }
        }

        /// <summary></summary>
        public bool isExecuteF5
        {
            get
            {
                return (eMenuTab == nMenuTab.Task) || (eMenuTab == nMenuTab.Keys);
            }
        }

        /// <summary></summary>
        public bool isExecuteNewKey
        {
            get
            {
                return (_BackgroundThread.eState == BackgroundThread.nState.Idle) || (_eBackgroundTask == BackgroundMessage.nType.FillKey);
            }
        }

        /// <summary></summary>
        public bool isExecuteSaveKey
        {
            get
            {
                return (_SelectedKey != null) && ((_SelectedKey.eFormat == CryptoKey.nKeyFormat.KeePass) || (_SelectedKey.eFormat == CryptoKey.nKeyFormat.Private) || (_SelectedKey.eFormat == CryptoKey.nKeyFormat.Public)) && (_SelectedKey.isNotSaved || (_SelectedKeyDrive != _SelectedKey.SavedOnDrive)) && !string.IsNullOrEmpty(_SelectedKey.sName) && !_SelectedKeyDrive.isReadOnly;
            }
        }

        /// <summary></summary>
        public bool isExecuteSynchronize
        {
            get
            {
                return (_BackgroundThread.eState == BackgroundThread.nState.Idle) && ((_aiPairsCount[0] + _aiPairsCount[1] + _aiPairsCount[2] + _aiPairsCount[3]) > 0);
            }
        }

        /// <summary></summary>
        public bool isExecuteSynchronizeCancelOrRecompare
        {
            get
            {
                return isExecuteSynchronize || isExecuteCancelSynchronize || isExecuteCompare;
            }
        }

        /// <summary></summary>
        public bool isExecuteSwap
        {
            get
            {
                return _BackgroundThread.eState == BackgroundThread.nState.Idle;
            }
        }

        /// <summary></summary>
        public BindingList<Property> blFileSystemLevels
        {
            get { return _blFileSystemLevels; }
        }

        /// <summary></summary>
        public string sHeaderDestinationNewer
        {
            get { return string.Format(Translate("DestinationNewer") + "   ({0:d})", _aiPairsCount[3]); }
        }

        /// <summary></summary>
        public string sHeaderDestinationOnly
        {
            get { return string.Format(Translate("DestinationOnly") + "   ({0:d})", _aiPairsCount[1]); }
        }

        /// <summary></summary>
        public string sHeaderError
        {
            get { return string.Format(Translate("Failure") + "   ({0:d})", _aiPairsCount[5]); }
        }

        /// <summary></summary>
        public string sHeaderIdentical
        {
            get { return string.Format(Translate("Identical") + "   ({0:d})", _aiPairsCount[4]); }
        }

        /// <summary></summary>
        public string sHeaderSourceNewer
        {
            get { return string.Format(Translate("SourceNewer") + "   ({0:d})", _aiPairsCount[2]); }
        }

        /// <summary></summary>
        public string sHeaderSourceOnly
        {
            get { return string.Format(Translate("SourceOnly") + "   ({0:d})", _aiPairsCount[0]); }
        }

        /// <summary></summary>
        public bool isKeyDrivesEnabled
        {
            get { return (_SelectedKey != null) && ((_SelectedKey.eFormat == CryptoKey.nKeyFormat.KeePass) || (_SelectedKey.eFormat == CryptoKey.nKeyFormat.Private) || (_SelectedKey.eFormat == CryptoKey.nKeyFormat.Public)); }
        }

        /// <summary></summary>
        public bool isKeyAsymmetric
        {
            get
            {
                return (_SelectedKey != null) && ((_SelectedKey.eType == CryptoKey.nKeyType.AsymmetricPrivate) || (_SelectedKey.eType == CryptoKey.nKeyType.AsymmetricPublic));
            }
        }

        /// <summary></summary>
        public string sKeyFormat
        {
            get
            {
                string sReturn = string.Empty;

                if (_SelectedKey != null)
                {
                    switch (_SelectedKey.eFormat)
                    {
                        case CryptoKey.nKeyFormat.BitLocker: sReturn = "BitLocker"; break;
                        case CryptoKey.nKeyFormat.KeePass: sReturn = "KeePass"; break;
                        case CryptoKey.nKeyFormat.Password: sReturn = Translate("KeyFormatPassword"); break;
                        case CryptoKey.nKeyFormat.Private: sReturn = Translate("KeyFormatPrivate"); break;
                        case CryptoKey.nKeyFormat.Public: sReturn = Translate("KeyFormatPublic"); break;
                    }
                }
                return sReturn;
            }
        }

        /// <summary></summary>
        public string sKeyName
        {
            get
            {
                if (_SelectedKey == null)
                    return string.Empty;
                else
                    return _SelectedKey.sName;
            }

            set
            {
                if ((_SelectedKey != null) && (value != _SelectedKey.sName))
                {
                    _SelectedKey.sName = value;
                    NotifyPropertyChanged("sKeyName");
#if WINDOWS
                    _ViewSourceKeys.Refresh();
#endif
                }
            }
        }

        /// <summary></summary>
        public bool isKeyNameReadOnly
        {
            get { return (_SelectedKey == null) || (_SelectedKey.eFormat == CryptoKey.nKeyFormat.BitLocker) || (_SelectedKey.eFormat == CryptoKey.nKeyFormat.Password); }
        }

        /// <summary></summary>
        public BindingList<Property> blKeyNumerals
        {
            get { return _blKeyNumerals; }
            set
            {
                if ((value != _blKeyNumerals))
                {
                    _blKeyNumerals = value;
                    NotifyPropertyChanged("blKeyNumerals");
                }
            }
        }

        /// <summary></summary>
        public BindingList<Property> blKeyParametersAsymmetric
        {
            get { return _blKeyParametersAsymmetric; }
        }

        public bool isKeyParametersEnabled
        {
            get
            {
                return (_SelectedKey != null) && (_SelectedKey.eType != CryptoKey.nKeyType.Invalid) && (_SelectedKey.eType != CryptoKey.nKeyType.Symmetric) && !PropertyHasErrors("sKeyValue");
            }
        }

        // public int iKeyRowHeight
        // {
        //     get { return _SelectedKey == null || !_SelectedKey.isSavedEncrypted ? 0 : ciDefaultRowHeight; }
        // }

        /// <summary></summary>
        public BindingList<CryptoKey> blKeys
        {
            get { return _blKeys; }
        }

        /// <summary></summary>
        public string sKeyValue
        {
            get
            {
                CryptoKey.nKeyParameter eSelectedKeyParameter;

                if (_SelectedKey == null)
                    _sKeyValue = string.Empty;
                else if (!PropertyHasErrors("sKeyValue"))
                {
                    eSelectedKeyParameter = (_SelectedKey.eType == CryptoKey.nKeyType.Symmetric ? CryptoKey.nKeyParameter.Symmetric : _eSelectedKeyParameterAsymmetric);

                    switch (eSelectedKeyParameter)
                    {
                        case CryptoKey.nKeyParameter.Email: _sKeyValue = _SelectedKey.sEmail; break;
                        case CryptoKey.nKeyParameter.Homepage: _sKeyValue = _SelectedKey.sHomepage; break;
                        case CryptoKey.nKeyParameter.Owner: _sKeyValue = _SelectedKey.sOwner; break;
                        default:
                            switch (_iSelectedKeyNumeral)
                            {
                                case 0: _sKeyValue = Translate("HiddenKey"); break;
                                case 2: _sKeyValue = _SelectedKey.GetKeyParameter(eSelectedKeyParameter, 2); break;
                                case 10: _sKeyValue = _SelectedKey.GetKeyParameter(eSelectedKeyParameter, 10); break;
                                case 16: _sKeyValue = _SelectedKey.GetKeyParameter(eSelectedKeyParameter, 16); break;
                                case 64: _sKeyValue = _SelectedKey.GetKeyParameter(eSelectedKeyParameter, 64); break;
                            }; break;
                    }
                }
                return _sKeyValue;
            }

            set
            {
                int iValidKeyLength = 0;
                CryptoKey.nKeyParameter eSelectedKeyParameter;

                if ((_SelectedKey != null) && (value != _sKeyValue))
                {
                    _sKeyValue = value;
                    eSelectedKeyParameter = (_SelectedKey.eType == CryptoKey.nKeyType.Symmetric ? CryptoKey.nKeyParameter.Symmetric : _eSelectedKeyParameterAsymmetric);

                    switch (eSelectedKeyParameter)
                    {
                        case CryptoKey.nKeyParameter.Email: _SelectedKey.sEmail = value; break;
                        case CryptoKey.nKeyParameter.Homepage: _SelectedKey.sHomepage = value; break;
                        case CryptoKey.nKeyParameter.Owner: _SelectedKey.sOwner = value; break;
                        default:
                            switch (_iSelectedKeyNumeral)
                            {
                                case 2: iValidKeyLength = (CryptoServices.ciAesKeyBytesLength << 3); break;
                                case 10: iValidKeyLength = ((int)(CryptoServices.ciAesKeyBytesLength * TextConverter.cdDecimalDigitsPerByte) + 1); break;
                                case 16: iValidKeyLength = (CryptoServices.ciAesKeyBytesLength << 1); break;
                                case 64: iValidKeyLength = CryptoKey.ciAesKeyBase64Length; break;
                            }
                            ValidateRaiseErrorsChanged(nValidationType.First, "sKeyValue", _sKeyValue.Length >= iValidKeyLength, Translate("KeyTooShort"));
                            ValidateRaiseErrorsChanged(nValidationType.Last, "sKeyValue", _sKeyValue.Length <= iValidKeyLength, Translate("KeyTooLong"));

                            if (!PropertyHasErrors("sKeyValue"))
                            {
                                try
                                {
                                    switch (_iSelectedKeyNumeral)
                                    {
                                        case 2: _SelectedKey.sAesKeyBinary = value; break;
                                        case 10: _SelectedKey.sAesKeyDecimal = value; break;
                                        case 16: _SelectedKey.sAesKeyHexadecimal = value; break;
                                        case 64: _SelectedKey.sAesKeyBase64 = value; break;
                                    }
                                }
                                catch (FormatException)
                                {
                                    ValidateRaiseErrorsChanged(nValidationType.Single, "sKeyValue", false, Translate("KeyInvalid"));
                                }
                            }; break;
                    }
                    NotifyPropertyChanged("sStatus");
                    NotifyPropertyChanged("sKeyValue");
                    NotifyPropertyChanged("isSelectedKeyFormatEnabled");
                }
            }
        }

        /// <summary></summary>
        public bool isKeyValueFocused
        {
            get { return _isKeyValueFocused; }
            set
            {
                if (value != isKeyValueFocused)
                {
                    _isKeyValueFocused = value;
                    NotifyPropertyChanged("isKeyValueFocused");
                }
            }
        }

        /// <summary></summary>
        public bool isKeyValueReadOnly
        {
            get { return (_SelectedKey == null) || (_iSelectedKeyNumeral == 0) || (_SelectedKey.eFormat ==  CryptoKey.nKeyFormat.BitLocker) || (_SelectedKey.eType == CryptoKey.nKeyType.Invalid) || _SelectedKey.IsReadOnly(eSelectedKeyParameter); }
        }

        /// <summary></summary>
        public BindingList<Property> blMemoryLimits
        {
            get { return _blMemoryLimits; }
        }

        /// <summary></summary>
        public nMenuTab eMenuTab
        {
            get { return _eMenuTab; }
            set
            {
                if (value != _eMenuTab)
                {
                    _eMenuTab = value;

                    if (_eMenuTab != nMenuTab.Task)
                        isDragOverTasks = false;

                    NotifyPropertyChanged("iMenuTab");
                }
            }
        }

        /// <summary></summary>
        public int iMenuTab
        {
            get { return (int)_eMenuTab; }
            set { eMenuTab = (nMenuTab)value; }
        }

        /// <summary></summary>
        public BindingList<Property> blMessages
        {
            get { return _blMessages; }
        }

        /// <summary></summary>
        public string sModeInformation
        {
            get
            {
                string sReturn = string.Empty;

                switch (_eCaseTab)
                {
                    case PairOfFiles.nComparison.SourceOnly: sReturn = Translate("SourceOnly") + ":   " + Translate("WillBe") + Translate("CopiedToDestination"); break;
                    case PairOfFiles.nComparison.DestinationOnly: sReturn = Translate("DestinationOnly") + ":   " + Translate("WillBe") + ModeInformationPhrase(true); break;
                    case PairOfFiles.nComparison.SourceNewer: sReturn = Translate("SourceNewer") + ":   " + Translate("WillBe") + Translate("CopiedToDestination"); break;
                    case PairOfFiles.nComparison.DestinationNewer: sReturn = Translate("DestinationNewer") + ":   " + Translate("WillBe") + ModeInformationPhrase(false); break;
                    case PairOfFiles.nComparison.Identical: sReturn = Translate("Identical") + ":   " + Translate("WillBeSkipped"); break;
                    case PairOfFiles.nComparison.Error: sReturn = Translate("Failure") + ":   " + Translate("PleaseCheck"); break;
                }
                return sReturn;
            }
        }

        /// <summary></summary>
        public bool isModeNoDelete
        {
            get { return _eSynchronizationMode == PairOfFiles.nSynchronizationMode.NoDelete; }
            set
            {
                if (value)
                {
                    _eSynchronizationMode = PairOfFiles.nSynchronizationMode.NoDelete;
                    NotifyPropertyChangedSynchronizationMode();
                }
            }
        }

        /// <summary></summary>
        public bool isModeTwoWay
        {
            get { return _eSynchronizationMode == PairOfFiles.nSynchronizationMode.TwoWay; }
            set
            {
                if (value)
                {
                    _eSynchronizationMode = PairOfFiles.nSynchronizationMode.TwoWay;
                    NotifyPropertyChangedSynchronizationMode();
                }
            }
        }

        /// <summary></summary>
        public bool isModeWithDelete
        {
            get { return _eSynchronizationMode == PairOfFiles.nSynchronizationMode.WithDelete; }
            set
            {
                if (value)
                {
                    _eSynchronizationMode = PairOfFiles.nSynchronizationMode.WithDelete;
                    NotifyPropertyChangedSynchronizationMode();
                }
            }
        }

        /// <summary></summary>
        public BindingList<Property> blNewKeyTypes
        {
            get { return _blNewKeyTypes; }
        }

        /// <summary></summary>
        public BindingList<Property> blSourceOptions
        {
            get { return _blSourceOptions; }
        }

        /// <summary></summary>
        public BindingList<PairOfFiles> blPairs
        {
            get { return _blPairs; }
        }

        public string sProgramVersion
        {
            get
            {
                Version ProgramVersion = Assembly.GetExecutingAssembly().GetName().Version;

                return ProgramVersion.Major.ToString() + "." + ProgramVersion.Minor.ToString() + "." + ProgramVersion.Build.ToString();
            }
        }

        /// <summary></summary>
        public bool isProgressBarIndeterminate
        {
            get { return _isProgressBarIndeterminate; }
            set
            {
                if (value != _isProgressBarIndeterminate)
                {
                    _isProgressBarIndeterminate = value;
                    NotifyPropertyChanged("isProgressBarIndeterminate");
                }
            }
        }

        /// <summary></summary>
        public int iProgressBarValue
        {
            get { return _iProgressBarValue; }
            set
            {
                if (value != _iProgressBarValue)
                {
                    _iProgressBarValue = value;
                    NotifyPropertyChanged("iProgressBarValue");
                }
            }
        }

        /// <summary></summary>
        public int iProgressBarMaximum
        {
            get { return _iProgressBarMaximum; }
            set
            {
                if (value != _iProgressBarMaximum)
                {
                    _iProgressBarMaximum = value;
                    NotifyPropertyChanged("iProgressBarMaximum");
                }
            }
        }

        /// <summary></summary>
        public int iRowHeightEncrypted
        {
            get { return isSourceOrDestinationEncrypted ? ciDefaultRowHeight : 0; }
        }

        /// <summary></summary>
        public int iRowHeightEncryptedDirectory
        {
            get { return _DestinationDrive.eEncryptionType == Drive.nEncryptionType.DirectorySymmetric ? ciDefaultRowHeight : 0; }
        }

        /// <summary></summary>
        public int iRowHeightKeys
        {
            get { return _DestinationDrive.eEncryptionType == Drive.nEncryptionType.DirectoryUnencrypted ? 0 : ciDefaultRowHeight; }
        }

        /// <summary></summary>
        public CryptoKey SelectedAuthenticationKey
        {
            get { return _DestinationDrive.SelectedAuthenticationKey; }
            set
            {
                if (value != _DestinationDrive.SelectedAuthenticationKey)
                {
                    _DestinationDrive.SelectedAuthenticationKey = value;
                    NotifyPropertyChanged("SelectedAuthenticationKey");
                }
            }
        }

        /// <summary></summary>
        public Property SelectedBlockSize
        {
            get { return GetBindingListNumber(_blBlockSizes, _DestinationDrive.iFileSystemBlockSize); }
            set
            {
                if (value.iNumber != _DestinationDrive.iFileSystemBlockSize)
                {
                    _DestinationDrive.iFileSystemBlockSize = value.iNumber;
                    NotifyPropertyChanged("sCapacityInformation");

                }
            }
        }

        /// <summary></summary>
        public Drive.nEncryptionType eSelectedDestinationEncryptionType
        {
            get { return _DestinationDrive.eEncryptionType; }
            set
            {
                if (value != _DestinationDrive.eEncryptionType)
                {
                    _DestinationDrive.eEncryptionType = value;

                    if (blAuthenticationKeys.Count == 1)
                        SelectedAuthenticationKey = blAuthenticationKeys.First();
                    else
                        SelectedAuthenticationKey = null;

                    if (blEncryptionKeys.Count == 1)
                        SelectedEncryptionKey = blEncryptionKeys.First();
                    else
                        SelectedEncryptionKey = null;

                    NotifyPropertyChangedEncryptionType();
                }
            }
        }

        /// <summary></summary>
        public Property SelectedDestinationOption
        {
            get { return GetBindingListId(_blDestinationOptions, (int)_DestinationDrive.eEncryptionType); }
            set
            {
                if ((Drive.nEncryptionType)value.iId != _DestinationDrive.eEncryptionType)
                {

                    if (isDestinationAFile && !string.IsNullOrEmpty(_DestinationDrive.sEncryptedFileName))
                    {
                        if ((Drive.nEncryptionType)value.iId == Drive.nEncryptionType.FileSymmetric)
                            sDestinationFileOrDirectory = ReplaceExtension(_sDestinationFileOrDirectory, Drive.csAsymmetricFileExtension, Drive.csSymmetricFileExtension);
                        else
                            sDestinationFileOrDirectory = ReplaceExtension(_sDestinationFileOrDirectory, Drive.csSymmetricFileExtension, Drive.csAsymmetricFileExtension);
                    }
                    eSelectedDestinationEncryptionType = (Drive.nEncryptionType)value.iId;
                }
            }
        }

        /// <summary></summary>
        public Drive SelectedDrive
        {
            get { return _SelectedDrive; }
            set
            {
                if (value != _SelectedDrive)
                {
                    _SelectedDrive = value;
                    NotifyPropertyChanged("SelectedDrive");
                }
            }
        }

        /// <summary></summary>
        public CryptoKey SelectedEncryptionKey
        {
            get { return _DestinationDrive.SelectedEncryptionKey; }
            set
            {
                if (value != _DestinationDrive.SelectedEncryptionKey)
                {
                    _DestinationDrive.SelectedEncryptionKey = value;
                    NotifyPropertyChanged("SelectedEncryptionKey");
                }
            }
        }

        /// <summary></summary>
        public Property SelectedFileSystemLevel
        {
            get { return GetBindingListId(_blFileSystemLevels, _DestinationDrive.iFileSystemLevel); }
            set
            {
                if (value.iId != _DestinationDrive.iFileSystemLevel)
                {
                    _DestinationDrive.iFileSystemLevel = value.iId;
                    NotifyPropertyChanged("sCapacityInformation");
                    NotifyPropertyChanged("SelectedFileSystemLevel");
                }
            }
        }

        /// <summary></summary>
        public CryptoKey SelectedKey
        {
            get { return _SelectedKey; }
            set
            {
                if (value != _SelectedKey)
                {
                    _SelectedKey = value;
                    SetSelectedKeyNumeral();

                    if ((_SelectedKey == null) || (_SelectedKey.SavedOnDrive == null))
                        _SelectedKeyDrive = _blDrives[0];
                    else
                        _SelectedKeyDrive = _SelectedKey.SavedOnDrive;

                    ClearErrors("sKeyValue");
                    NotifyPropertyChanged("sKeyName");
                    NotifyPropertyChanged("sKeyValue");
                    NotifyPropertyChanged("sKeyFormat");
                    NotifyPropertyChanged("SelectedKey");
                    NotifyPropertyChanged("blKeyParameters");
                    NotifyPropertyChanged("SelectedKeyDrive");
                    NotifyPropertyChanged("isKeyNameReadOnly");
                    NotifyPropertyChanged("isKeyValueReadOnly");
                    NotifyPropertyChanged("isKeyDrivesEnabled");
                    NotifyPropertyChanged("isKeyParametersEnabled");
                    NotifyPropertyChanged("VisibleWhenKeyAsymmetric");
                    NotifyPropertyChanged("isSelectedKeyFormatEnabled");
                    NotifyPropertyChanged("SelectedKeyParameterAsymmetric");
                }
            }
        }

        /// <summary></summary>
        public Drive SelectedKeyDrive
        {
            get { return _SelectedKeyDrive; }
            set
            {
                if (value != _SelectedKeyDrive)
                {
                    _SelectedKeyDrive = value;
                    NotifyPropertyChanged("SelectedKeyDrive");
                }
            }
        }

        /// <summary></summary>
        public CryptoKey SelectedKeyFileAesKey
        {
            get { return _SelectedKeyFileAesKey; }
            set
            {
                if (value != _SelectedKeyFileAesKey)
                {
                    _SelectedKeyFileAesKey = value;
                    NotifyPropertyChanged("SelectedKeyFileAesKey");
                }
            }
        }

        /// <summary></summary>
        public bool isSelectedKeyFormatEnabled
        {
            get { return !PropertyHasErrors("sKeyValue"); }
        }

        /// <summary></summary>
        public Property SelectedKeyNumeral
        {
            get { return GetBindingListId(blKeyNumerals, _iSelectedKeyNumeral); }
            set
            {
                if ((value != null) && (value.iId != _iSelectedKeyNumeral))
                {
                    _iSelectedKeyNumeral = value.iId;

                    if ((_SelectedKey != null) && (_iSelectedKeyNumeral != 1) && (_eSelectedKeyParameterAsymmetric != CryptoKey.nKeyParameter.Exponent) || (_eSelectedKeyParameterAsymmetric != CryptoKey.nKeyParameter.Modulus))
                        _iKeyNumeralIfCanHide = _iSelectedKeyNumeral;

                    FocusKeyValue();
                    NotifyPropertyChanged("sKeyValue");
                    NotifyPropertyChanged("SelectedKeyNumeral");
                    NotifyPropertyChanged("isKeyValueReadOnly");
                }
            }
        }

        /// <summary></summary>
        public CryptoKey.nKeyParameter eSelectedKeyParameter
        {
            get
            {
                if ((_SelectedKey != null) && (_SelectedKey.eType != CryptoKey.nKeyType.Symmetric))
                    return _eSelectedKeyParameterAsymmetric;
                else
                    return CryptoKey.nKeyParameter.Symmetric;
            }
        }

        /// <summary></summary>
        public Property SelectedKeyParameterAsymmetric
        {
            get

            {
                if ((_SelectedKey != null) && (_SelectedKey.eType != CryptoKey.nKeyType.Symmetric))
                    return GetBindingListId(_blKeyParametersAsymmetric, (int)_eSelectedKeyParameterAsymmetric);
                else
                    return null;
            }
            set
            {
                if ((value != null) && (value.iId != (int)_eSelectedKeyParameterAsymmetric))
                {
                    _eSelectedKeyParameterAsymmetric = (CryptoKey.nKeyParameter)value.iId;
                    SetSelectedKeyNumeral();
                    FocusKeyValue();
                    NotifyPropertyChanged("sKeyValue");
                    NotifyPropertyChanged("isKeyValueReadOnly");
                    NotifyPropertyChanged("SelectedKeyParameter");
                }
            }
        }

        /// <summary></summary>
        public Property SelectedNewKeyType
        {
            get { return GetBindingListId(_blNewKeyTypes, _iSelectedNewKeyBytes); }
            set
            {
                if (value.iId != _iSelectedNewKeyBytes)
                {
                    _iSelectedNewKeyBytes = value.iId;
                    NotifyPropertyChanged("SelectedNewKeyFormat");
                }
            }
        }

        /// <summary></summary>
        public Drive.nEncryptionType eSelectedSourceEncryptionType
        {
            get { return _SourceDrive.eEncryptionType; }
            set
            {
                if (value != _SourceDrive.eEncryptionType)
                {
                    _SourceDrive.eEncryptionType = value;
                    NotifyPropertyChanged("SelectedSourceOption");
                }
            }
        }

        /// <summary></summary>
        public Property SelectedSourceOption
        {
            get { return GetBindingListId(_blSourceOptions, (int)_SourceDrive.eEncryptionType); }
            set
            {
                if ((Drive.nEncryptionType)value.iId != _SourceDrive.eEncryptionType)
                {
                    if (isSourceAFile && !string.IsNullOrEmpty(_SourceDrive.sEncryptedFileName))
                    {
                        if ((Drive.nEncryptionType)value.iId == Drive.nEncryptionType.FileSymmetric)
                            sSourceFileOrDirectory = ReplaceExtension(_sSourceFileOrDirectory, Drive.csAsymmetricFileExtension, Drive.csSymmetricFileExtension);
                        else
                            sSourceFileOrDirectory = ReplaceExtension(_sSourceFileOrDirectory, Drive.csSymmetricFileExtension, Drive.csAsymmetricFileExtension);

                        if (_eSynchronizationMode == PairOfFiles.nSynchronizationMode.TwoWay)
                            isModeWithDelete = true;
                    }
                    eSelectedSourceEncryptionType = (Drive.nEncryptionType)value.iId;

                    NotifyPropertyChanged("isSourceADirectory");
                    NotifyPropertyChanged("iRowHeightEncrypted");
                    NotifyPropertyChanged("SelectedSourceOption");
                    NotifyPropertyChanged("VisibleWhenEncrypted");
                }
            }
        }

        /// <summary></summary>
        public Property SelectedTask
        {
            get { return _SelectedTask; }
            set
            {
                if (value != _SelectedTask)
                {
                    _SelectedTask = value;
                    NotifyPropertyChanged("SelectedTask");
                }
            }
        }

        /// <summary></summary>
        public Property SelectedWorkingMemoryLimit
        {
            get { return GetBindingListNumber(_blMemoryLimits, _iWorkingMemoryLimit); }
            set
            {
                if (value.iNumber != _iWorkingMemoryLimit)
                {
                    _iWorkingMemoryLimit = _SourceDrive.iWorkingMemoryLimit = _DestinationDrive.iWorkingMemoryLimit = value.iNumber;
                    NotifyPropertyChanged("SelectedWorkingMemoryLimit");
                }
            }
        }

        /// <summary></summary>
        public bool isSourceADirectory
        {
            get
            {
                return (_SourceDrive.eEncryptionType == Drive.nEncryptionType.DirectoryUnencrypted) || (_SourceDrive.eEncryptionType == Drive.nEncryptionType.DirectorySymmetric);
            }
        }

        /// <summary></summary>
        public bool isSourceAFile
        {
            get
            {
                return (_SourceDrive.eEncryptionType == Drive.nEncryptionType.FileAsymmetric) || (_SourceDrive.eEncryptionType == Drive.nEncryptionType.FileSymmetric);
            }
        }

        /// <summary></summary>
        public bool isSourceEncrypted
        {
            get { return (_SourceDrive.eEncryptionType != Drive.nEncryptionType.DirectoryUnencrypted); }
            // set
            // {
            //     if (value != _SourceDrive.isEncrypted)
            //     {
            //         _SourceDrive.isEncrypted = value;
            //         NotifyPropertyChanged("VisibleWhenEncrypted");
            //         NotifyPropertyChanged("isSourceEncrypted");
            //         NotifyPropertyChanged("iSourceRowHeight");
            //         NotifyPropertyChanged("VisibleWhenSourceEncrypted");
            //     }
            // }
        }

        /// <summary></summary>
        public string sSourceFileOrDirectory
        {
            get { return _sSourceFileOrDirectory; }
            set
            {
                if (value != _sSourceFileOrDirectory)
                {
                    _sSourceFileOrDirectory = value;

                    if (value != _SourceDrive.sRootPathAndFile)
                        _SourceDrive.sRootPathAndFile = value;

                    if (string.IsNullOrEmpty(_SourceDrive.sEncryptedFileName))
                    {
                        ValidateRaiseErrorsChanged(nValidationType.Single, "sSourceFileOrDirectory", Directory.Exists(_sSourceFileOrDirectory), Translate("SourceDirectoryMissing"));
                        ValidateRaiseErrorsChanged(nValidationType.Single, "sFileOrDirectory", IsSynchronizationAllowed(_sSourceFileOrDirectory, sDestinationFileOrDirectory), Translate("ErrorDirectoriesIdentical"));
                        // eSelectedSourceEncryptionType = Drive.nEncryptionType.DirectoryUnencrypted;
                    }
                    else
                    {
                        ValidateRaiseErrorsChanged(nValidationType.Single, "sSourceFileOrDirectory", File.Exists(_sSourceFileOrDirectory), Translate("SourceFileMissing"));
                        ValidateRaiseErrorsChanged(nValidationType.Single, "sFileOrDirectory", _sSourceFileOrDirectory != sDestinationFileOrDirectory, Translate("ErrorFilesIdentical"));

                        if (_eSynchronizationMode == PairOfFiles.nSynchronizationMode.TwoWay)
                            isModeWithDelete = true;
                    }
                    NotifyPropertyChanged("sStatus");
                    NotifyPropertyChanged("isSourceADirectory");
                    NotifyPropertyChanged("SelectedSourceOption");
                    NotifyPropertyChanged("sSourceFileOrDirectory");
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary></summary>
        public bool isSourceOrDestinationEncrypted
        {
            get { return isSourceEncrypted || isDestinationEncrypted; }
        }

        /// <summary></summary>
        public string sStatus
        {
            get { return string.IsNullOrEmpty(sErrorMessage) ? _sBackgroundStatus : sErrorMessage; }
        }

        /// <summary></summary>
        public string sSynchronizeCancelOrRecompare
        {
            get
            {
                if (isExecuteCancelSynchronize)
                    return sCancel;
                else if (isExecuteSynchronize)
                    return sSynchronize;
                else if (isExecuteCompare)
                    return sCompareAgain;
                else
                    return string.Empty;
            }
        }

        /// <summary></summary>
        public bool isTabErrorSelected
        {
            get { return (_eCaseTab == PairOfFiles.nComparison.Error); }
        }

        /// <summary></summary>
        public string sTaskName
        {
            get { return _sTaskName; }
            set
            {
                if (value != _sTaskName)
                {
                    _sTaskName = value;
                    NotifyPropertyChanged("sTaskName");
                }
            }
        }

        /// <summary></summary>
        public BindingList<Property> blTasks
        {
            get { return _blTasks; }
        }

        /// <summary></summary>
        public string sTemporaryDirectory
        {
            get { return _sTemporaryDirectory; }
            set
            {
                if (value != _sTemporaryDirectory)
                {
                    _sTemporaryDirectory = _SourceDrive.sTemporaryDirectory = _DestinationDrive.sTemporaryDirectory = value;
                    NotifyPropertyChanged("sTemporaryDirectory");
                }
            }
        }

        /// <summary></summary>
        public Visibility VisibleWhenDestinationIsDirectory
        {
            get { return isDestinationAFile ? Visibility.Collapsed : Visibility.Visible; }
        }

        /// <summary></summary>
        public Visibility VisibleWhenDestinationDirectoryIsSymmetric
        {
            get { return _DestinationDrive.eEncryptionType == Drive.nEncryptionType.DirectorySymmetric ? Visibility.Visible : Visibility.Collapsed; }
        }

        /// <summary></summary>
        public Visibility VisibleWhenEncrypted
        {
            get { return isSourceOrDestinationEncrypted ? Visibility.Visible : Visibility.Collapsed; }
        }

        /// <summary></summary>
        public Visibility VisibleWhenKeyAsymmetric
        {
            get { return isKeyAsymmetric ? Visibility.Visible : Visibility.Collapsed; }
        }

        /// <summary></summary>
        public Visibility VisibleWhenTabsIncludedSelected
        {
            get { return isTabErrorSelected ? Visibility.Collapsed : Visibility.Visible; }
        }

        /// <summary></summary>
        public Visibility VisibleWhenTabErrorSelected
        {
            get { return isTabErrorSelected ? Visibility.Visible : Visibility.Collapsed; }
        }

        #endregion
    }
}

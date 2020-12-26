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


    public partial class MainViewModel : ViewModelBase
    {
        private const int ciDefaultRowHeight = 60;
        private const int ciProgrssBarDefaultMaximum = 1;

        /// <summary>Enumerated type of menu tabs. The predefined numbers are for <c>TabControl.SelectedIndex</c>.</summary>
        public enum nMenuTab { Task = 0, Progress = 1, Messages = 2, Keys = 3 };

        private bool _isDragOverTasks, _isProgressBarIndeterminate;
        private int _iProgressBarValue, _iProgressBarMaximum;
        private readonly int[] _aiPairsCount, _aiPairsOrder;
        private string _sBackgroundStatus, _sDestinationDirectory, _sSourceDirectory, _sTaskName, _sTemporaryDirectory;
        private nMenuTab _eMenuTab;
        private PairOfFiles.nComparison _eCaseTab;
        private PairOfFiles.nSynchronizationMode _eSynchronizationMode;
        private BackgroundThread _BackgroundThread;
        private BackgroundMessage.nType _eBackgroundTask;
        private Drive _DestinationDrive, _SourceDrive;
        private Property _SelectedTask;
        private readonly List<Drive> _ltDrives;
        private readonly List<PairOfFiles> _ltPairs;
        private readonly BindingList<PairOfFiles> _blPairs;
        private readonly BindingList<Property> _blMessages;
        private CryptoServices _Cryptography;

        #region properties

        private readonly DispatcherTimer _UserInterfaceTimer;

        public ICommand dcCompare { get; }
        public ICommand dcF5 { get; }
        public ICommand dcSelectDestination { get; }
        public ICommand dcSelectSource { get; }
        public ICommand dcSwap { get; }
        public ICommand dcSynchronizeCancelOrRecompare { get; }

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
        public string sDestinationDirectoryText { get => Translate("DestinationDirectoryText"); }
        public string sDestinationSizeText { get => Translate("DestinationSizeText"); }
        public string sDriveText { get => Translate("DriveText"); }
        public string sFinishCompare { get => Translate("FinishCompare"); }
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
        public string sSourceDirectoryText { get => Translate("SourceDirectoryText"); }
        public string sSourceSizeText { get => Translate("SourceSizeText"); }
        public string sStartCompare { get => Translate("StartCompare"); }
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
                if (_BackgroundThread.eState != BackgroundThread.nState.Idle)
                    return sCancel;
                else
                    return sCreate;
            }
        }

        /// <summary></summary>
        public string sDestinationDirectory
        {
            get { return _sDestinationDirectory; }
            set
            {
                if (value != _sDestinationDirectory)
                {
                    string sAdaptedPath;

                    _sDestinationDirectory = value;
                    _DestinationDrive.sRootPath = value;

                    foreach (Drive DriveToTry in _ltDrives)
                    {
                        if (_DestinationDrive.sName != DriveToTry.sName)
                        {
                            sAdaptedPath = DriveToTry.AdaptPath(value);

                            if (Directory.Exists(sAdaptedPath))
                                sSourceDirectory = sAdaptedPath;
                        }
                    }

                    ValidateRaiseErrorsChanged(nValidationType.Single, "sDirectory", _sDestinationDirectory != _sSourceDirectory, Translate("ErrorDirectoriesIdentical"));

                    NotifyPropertyChanged("sStatus");
                    NotifyPropertyChanged("sDestinationDirectory");
                    CommandManager.InvalidateRequerySuggested();
                }
            }
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
                return (_BackgroundThread.eState == BackgroundThread.nState.Idle) && !HasErrors && (Directory.Exists(_sSourceDirectory)) && (Directory.Exists(_sDestinationDirectory));
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
                return (_BackgroundThread.eState == BackgroundThread.nState.Idle);
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
            get { return 0; }
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
        public bool isSelectedKeyFormatEnabled
        {
            get { return !PropertyHasErrors("sKeyValue"); }
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
        public string sSourceDirectory
        {
            get { return _sSourceDirectory; }
            set
            {
                if (value != _sSourceDirectory)
                {
                    string sAdaptedPath;

                    _sSourceDirectory = value;
                    _SourceDrive.sRootPath = value;

                    foreach (Drive DriveToTry in _ltDrives)
                    {
                        if (_SourceDrive.sName != DriveToTry.sName)
                        {
                            sAdaptedPath = DriveToTry.AdaptPath(value);

                            if (Directory.Exists(sAdaptedPath))
                                sDestinationDirectory = sAdaptedPath;
                        }
                    }

                    ValidateRaiseErrorsChanged(nValidationType.Single, "sSourceDirectory", Directory.Exists(_sSourceDirectory), Translate("SourceDirectoryMissing"));
                    ValidateRaiseErrorsChanged(nValidationType.Single, "sDirectory", _sSourceDirectory != _sDestinationDirectory, Translate("ErrorDirectoriesIdentical"));

                    NotifyPropertyChanged("sStatus");
                    NotifyPropertyChanged("sSourceDirectory");
                    CommandManager.InvalidateRequerySuggested();
                }
            }
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

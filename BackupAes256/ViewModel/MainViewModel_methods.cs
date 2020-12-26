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
        #region constructors

        /// <summary>Initializes a new instance of the MainViewModel class.</summary>
        public MainViewModel() : base()
        {
#if ENGLISH
            _dyTranslations.Add("AesKey", "256-bit AES key");
            _dyTranslations.Add("AuthenticationKey", "authentication key");
            _dyTranslations.Add("BitsText", "bits");
            _dyTranslations.Add("Blocks", "blocks");
            _dyTranslations.Add("BlockSize", "block size");
            _dyTranslations.Add("Cancel", "cancel");
            _dyTranslations.Add("CancelCreation", "cancel creation");
            _dyTranslations.Add("CapacityInformation", "capacity up to {0:f1} % of the size of drive {1:s}");
            _dyTranslations.Add("Close", "close");
            _dyTranslations.Add("ComingSoon", "coming soon");
            _dyTranslations.Add("Compare", "compare");
            _dyTranslations.Add("CompareAgain", "compare again");
            _dyTranslations.Add("CopiedToDestination", "copied to the destination directory");
            _dyTranslations.Add("CopiedToSource", "copied to the source directory");
            _dyTranslations.Add("Create", "create");
            _dyTranslations.Add("CreatingNewKey", "Creating new key...");
            _dyTranslations.Add("Deleted", "deleted");
            _dyTranslations.Add("DestinationDateText", "destination date");
            _dyTranslations.Add("DestinationDirectoryMissing", "The destination directory does not exist.");
            _dyTranslations.Add("DestinationDirectoryText", "destination directory");
            _dyTranslations.Add("DestinationFile", "destination file");
            _dyTranslations.Add("DestinationNewer", "destination newer");
            _dyTranslations.Add("DestinationOnly", "destination only");
            _dyTranslations.Add("DestinationSizeText", "destination size");
            _dyTranslations.Add("DriveText", "drive");
            _dyTranslations.Add("EmptyDriveName", "not saved");
            _dyTranslations.Add("ErrorDirectoriesIdentical", "Identical or nested directories cannot be synchronized.");
            _dyTranslations.Add("ErrorFilesIdentical", "Identical files cannot be synchronized.");
            _dyTranslations.Add("ErrorMessageText", "error message");
            _dyTranslations.Add("Export", "export");
            _dyTranslations.Add("Failure", "failure");
            _dyTranslations.Add("FileNotFound", "File »{0:s}« was not found.");
            _dyTranslations.Add("FinishCompare", "Comparison of the directories was finished.");
            _dyTranslations.Add("FinishFillKey", "Creation of a key was finished.");
            _dyTranslations.Add("FormatText", "format");
            _dyTranslations.Add("FoundAuthenticationKey", "The name of the authentication key is »{0:s}«.");
            _dyTranslations.Add("FoundPrivateKey", "The name of the private key is »{0:s}«.");
            _dyTranslations.Add("FoundSymmetricKey", "The name of the AES key is »{0:s}«.");
            _dyTranslations.Add("Identical", "identical");
            _dyTranslations.Add("Iterations", "iterations");
            _dyTranslations.Add("KeyFormatPassword", "password");
            _dyTranslations.Add("KeyFormatPrivate", "private");
            _dyTranslations.Add("KeyFormatPublic", "public");
            _dyTranslations.Add("KeyNumerals0", "value (hidden)");
            _dyTranslations.Add("KeyNumerals2", "value (binary)");
            _dyTranslations.Add("KeyNumerals10", "value (decimal)");
            _dyTranslations.Add("KeyNumerals16", "value (hexadecimal)");
            _dyTranslations.Add("KeyNumerals64", "value (Base64)");
            _dyTranslations.Add("KeyInvalid", "The value for the 256-bit key is invalid.");
            _dyTranslations.Add("KeyNameText", "key name");
            _dyTranslations.Add("KeyParameterSymmetric", "symmetric key");
            _dyTranslations.Add("Keys", "keys");
            _dyTranslations.Add("KeyTextPublic", "public");
            _dyTranslations.Add("KeyTooLong", "The value for the 256-bit key is too long.");
            _dyTranslations.Add("KeyTooShort", "The value for the 256-bit key is too short.");
            _dyTranslations.Add("KeyTypeInvalid", "invalid");
            _dyTranslations.Add("KeyTypeSymmetric", "symmetric");
            _dyTranslations.Add("KeyTypeSymmetric256Bit", "symmetric, 256 bits");
            _dyTranslations.Add("MaximumBlockCount", "max. block count  (levels)");
            _dyTranslations.Add("Messages", "messages");
            _dyTranslations.Add("MessageText", "message");
            _dyTranslations.Add("ModeNoDelete", "no delete");
            _dyTranslations.Add("ModeTwoWay", "two-way");
            _dyTranslations.Add("ModeWithDelete", "with delete");
            _dyTranslations.Add("NewKey", "new key");
            _dyTranslations.Add("NoAuthenticationKey", "No authentication key was found to match these data.");
            _dyTranslations.Add("NoSymmetricKey", "No symmetric key was found to match these data.");
            _dyTranslations.Add("OnDrive", "save on drive");
            _dyTranslations.Add("PleaseCheck", "please check");
            _dyTranslations.Add("ProgrammingError", "A programming error occurred. I am sorry, this should not have happened.");
            _dyTranslations.Add("ProgramVersion", "Program version " + sProgramVersion + " of 26/12/2020 is ready.");
            _dyTranslations.Add("Progress", "progress");
            _dyTranslations.Add("ReadDrivesAndKeys", "re-read drives");
            _dyTranslations.Add("RelativePathText", "relative path");
            _dyTranslations.Add("ReplacedFromSource", "replaced with the older version from the source directory");
            _dyTranslations.Add("Reserve", "reserve");
            _dyTranslations.Add("Save", "save");
            _dyTranslations.Add("Select", "select");
            _dyTranslations.Add("SelectDestinationDirectory", "select destination directory");
            _dyTranslations.Add("SelectDestinationFile", "select destination file");
            _dyTranslations.Add("SelectSourceDirectory", "select source directory");
            _dyTranslations.Add("SelectSourceFile", "select source file");
            _dyTranslations.Add("SelectTemporaryDirectory", "select temporary directory");
            _dyTranslations.Add("Size", "size");
            _dyTranslations.Add("Skipped", "skipped");
            _dyTranslations.Add("SourceDateText", "source date");
            _dyTranslations.Add("SourceDirectoryMissing", "The source directory does not exist.");
            _dyTranslations.Add("SourceDirectoryText", "source directory");
            _dyTranslations.Add("SourceFile", "source file");
            _dyTranslations.Add("SourceFileMissing", "The source file does not exist.");
            _dyTranslations.Add("SourceSizeText", "source size");
            _dyTranslations.Add("SourceNewer", "source newer");
            _dyTranslations.Add("SourceOnly", "source only");
            _dyTranslations.Add("StartCompare", "Comparison of the directories was started.");
            _dyTranslations.Add("StartFillKey", "Creation of a key was started.");
            _dyTranslations.Add("Swap", "swap");
            _dyTranslations.Add("Synchronize", "synchronize");
            _dyTranslations.Add("Task", "task");
            _dyTranslations.Add("TaskNameText", "task name");
            _dyTranslations.Add("TemporaryDirectoryText", "temporary directory for larger files");
            _dyTranslations.Add("TimeText", "time");
            _dyTranslations.Add("TypeText", "type");
            _dyTranslations.Add("UnspecifiedError", "A relatively rare error occurred. This is why there is no more information.");
            _dyTranslations.Add("UseWorkingMemory", "use working memory up to");
            _dyTranslations.Add("WillBe", "will be ");
            _dyTranslations.Add("WillBeSkipped", "will be skipped");
            _dyTranslations.Add("WindowTitle", "File synchronization and mirrored backups");
            _dyTranslations.Add("WrongFileFormat", "File »{0:s}« has a wrong format and could not be read.");

#elif DEUTSCH
            _dyTranslations.Add("AesKey", "256-Bit AES-Schlüssel");
            _dyTranslations.Add("AuthenticationKey", "Authentifizierungs-Schlüssel");
            _dyTranslations.Add("BitsText", "Bit");
            _dyTranslations.Add("Blocks", "Blöcke");
            _dyTranslations.Add("BlockSize", "Blockgröße");
            _dyTranslations.Add("Cancel", "abbrechen");
            _dyTranslations.Add("CancelCreation", "Erzeugung abbrechen");
            _dyTranslations.Add("CapacityInformation", "Kapazität bis zu {0:f1} % der Größe von Laufwerk {1:s}");
            _dyTranslations.Add("Close", "schließen");
            _dyTranslations.Add("ComingSoon", "folgt demnächst");
            _dyTranslations.Add("Compare", "vergleichen");
            _dyTranslations.Add("CompareAgain", "neu vergleichen");
            _dyTranslations.Add("CopiedToDestination", "in das Zielverzeichnis kopiert");
            _dyTranslations.Add("CopiedToSource", "in das Quellverzeichnis kopiert");
            _dyTranslations.Add("Create", "erzeugen");
            _dyTranslations.Add("CreatingNewKey", "Erzeuge neuen Schlüssel...");
            _dyTranslations.Add("Deleted", "gelöscht");
            _dyTranslations.Add("DestinationDateText", "Datum Ziel");
            _dyTranslations.Add("DestinationDirectoryMissing", "Das Zielvereichnis existiert nicht.");
            _dyTranslations.Add("DestinationDirectoryText", "Zielverzeichnis");
            _dyTranslations.Add("DestinationFile", "Zieldatei");
            _dyTranslations.Add("DestinationNewer", "neueres Ziel");
            _dyTranslations.Add("DestinationOnly", "nur Ziel");
            _dyTranslations.Add("DestinationSizeText", "Größe Ziel");
            _dyTranslations.Add("DriveText", "Laufwerk");
            _dyTranslations.Add("EmptyDriveName", "ungespeichert");
            _dyTranslations.Add("ErrorDirectoriesIdentical", "Identische oder in einander enthaltene Verzeichnisse können nicht synchronisiert werden.");
            _dyTranslations.Add("ErrorFilesIdentical", "Identische Dateien können nicht synchronisiert werden.");
            _dyTranslations.Add("ErrorMessageText", "Fehlernachricht");
            _dyTranslations.Add("ErrorWorkingMemoryLimit", "Das Verschlüsseln mehrerer Gigabyte an Daten ist (noch) nicht möglich.");
            _dyTranslations.Add("Export", "exportieren");
            _dyTranslations.Add("Failure", "fehlerhaft");
            _dyTranslations.Add("FileNotFound", "Die Datei »{0:s}« wurde nicht gefunden.");
            _dyTranslations.Add("FinishCompare", "Der Vergleich der Verzeichnisse wurde beendet.");
            _dyTranslations.Add("FinishFillKey", "Das Erzeugen eines Schlüssels wurde beendet.");
            _dyTranslations.Add("FormatText", "Format");
            _dyTranslations.Add("FoundAuthenticationKey", "Der Name des Authentifizierungs-Schlüssels ist »{0:s}«.");
            _dyTranslations.Add("FoundPrivateKey", "Der Name des privaten Schlüssels ist »{0:s}«.");
            _dyTranslations.Add("FoundSymmetricKey", "Der Name des AES-Schlüssels ist »{0:s}«.");
            _dyTranslations.Add("Identical", "identisch");
            _dyTranslations.Add("Iterations", "Durchläufe");
            _dyTranslations.Add("KeyFormatPassword", "Passwort");
            _dyTranslations.Add("KeyFormatPrivate", "privat");
            _dyTranslations.Add("KeyFormatPublic", "öffentlich");
            _dyTranslations.Add("KeyNumerals0", "Wert (verborgen)");
            _dyTranslations.Add("KeyNumerals2", "Wert (binär)");
            _dyTranslations.Add("KeyNumerals10", "Wert (dezimal)");
            _dyTranslations.Add("KeyNumerals16", "Wert (hexadezimal)");
            _dyTranslations.Add("KeyNumerals64", "Wert (Base64)");
            _dyTranslations.Add("KeyInvalid", "Der Wert für den 256-Bit-Schlüssel ist ungültig.");
            _dyTranslations.Add("KeyNameText", "Schlüsselname");
            _dyTranslations.Add("KeyParameterSymmetric", "symmetrischer Schlüssel");
            _dyTranslations.Add("Keys", "Schlüssel");
            _dyTranslations.Add("KeyTextPublic", "öffentlich");
            _dyTranslations.Add("KeyTooLong", "Der Wert für den 256-Bit-Schlüssel ist zu lang.");
            _dyTranslations.Add("KeyTooShort", "Der Wert für den 256-Bit-Schlüssel ist zu kurz.");
            _dyTranslations.Add("KeyTypeInvalid", "ungültig");
            _dyTranslations.Add("KeyTypeSymmetric", "symmetrisch");
            _dyTranslations.Add("KeyTypeSymmetric256Bit", "symmetrisch, 256 Bit");
            _dyTranslations.Add("MaximumBlockCount", "max. Blockzahl  (Ebenen)");
            _dyTranslations.Add("Messages", "Nachrichten");
            _dyTranslations.Add("MessageText", "Nachricht");
            _dyTranslations.Add("ModeNoDelete", "kein Löschen");
            _dyTranslations.Add("ModeTwoWay", "gegenseitig");
            _dyTranslations.Add("ModeWithDelete", "mit Löschen");
            _dyTranslations.Add("NewKey", "neuen Schlüssel");
            _dyTranslations.Add("NoAuthenticationKey", "Zu diesen Daten wurde kein Authentifizierungs-Schlüssel gefunden.");
            _dyTranslations.Add("NoSymmetricKey", "Zu diesen Daten wurde kein symmetrischer Schlüssel gefunden.");
            _dyTranslations.Add("OnDrive", "auf Laufwerk");
            _dyTranslations.Add("PleaseCheck", "bitte überprüfen");
            _dyTranslations.Add("ProgrammingError", "Ein Programmierfehler ist aufgetreten. Entschuldigung, das hätte nicht passieren dürfen.");
            _dyTranslations.Add("ProgramVersion", "Die Programmversion " + sProgramVersion + " vom 26.12.2020 ist bereit.");
            _dyTranslations.Add("Progress", "Fortschritt");
            _dyTranslations.Add("ReadDrivesAndKeys", "Laufwerke neu lesen");
            _dyTranslations.Add("RelativePathText", "Relativer Pfad");
            _dyTranslations.Add("ReplacedFromSource", "durch die ältere Version aus dem Quellverzeichnis ersetzt");
            _dyTranslations.Add("Reserve", "reserviere");
            _dyTranslations.Add("Save", "speichern");
            _dyTranslations.Add("Select", "auswählen");
            _dyTranslations.Add("SelectDestinationDirectory", "Zielverzeichnis wählen");
            _dyTranslations.Add("SelectDestinationFile", "Zieldatei wählen");
            _dyTranslations.Add("SelectSourceDirectory", "Quellverzeichnis wählen");
            _dyTranslations.Add("SelectSourceFile", "Quelldatei wählen");
            _dyTranslations.Add("SelectTemporaryDirectory", "Temporäres Verzeichnis wählen");
            _dyTranslations.Add("Size", "Größe");
            _dyTranslations.Add("Skipped", "übersprungen");
            _dyTranslations.Add("SourceDateText", "Datum Quelle");
            _dyTranslations.Add("SourceDirectoryMissing", "Das Quellvereichnis existiert nicht.");
            _dyTranslations.Add("SourceDirectoryText", "Quellverzeichnis");
            _dyTranslations.Add("SourceFile", "Quelldatei");
            _dyTranslations.Add("SourceFileMissing", "Die Quelldatei existiert nicht.");
            _dyTranslations.Add("SourceSizeText", "Größe Quelle");
            _dyTranslations.Add("SourceNewer", "neuere Quelle");
            _dyTranslations.Add("SourceOnly", "nur Quelle");
            _dyTranslations.Add("StartCompare", "Der Vergleich der Verzeichnisse wurde gestartet.");
            _dyTranslations.Add("StartFillKey", "Das Erzeugen eines Schlüssels wurde gestartet.");
            _dyTranslations.Add("Swap", "tauschen");
            _dyTranslations.Add("Synchronize", "synchronisieren");
            _dyTranslations.Add("Task", "Aufgabe");
            _dyTranslations.Add("TaskNameText", "Aufgabenname");
            _dyTranslations.Add("TemporaryDirectoryText", "temporäres Verzeichnis für größere Dateien");
            _dyTranslations.Add("TimeText", "Zeit");
            _dyTranslations.Add("TypeText", "Typ");
            _dyTranslations.Add("UnspecifiedError", "Ein relativ seltener Fehler ist aufgeteten. Genaueres ist deshalb nicht bekannt.");
            _dyTranslations.Add("UseWorkingMemory", "Arbeitsspeicher nutzen bis");
            _dyTranslations.Add("WillBe", "werden ");
            _dyTranslations.Add("WillBeSkipped", "werden übersprungen");
            _dyTranslations.Add("WindowTitle", "Dateisynchronisierung und gespiegelte Sicherungskopien");
            _dyTranslations.Add("WrongFileFormat", "Die Datei »{0:s}« hat ein falsches Format und kann nicht gelesen werden.");
#endif
            _dyTranslations.Add("HiddenKey", "●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●");


            _sBackgroundStatus = _sDestinationDirectory = _sSourceDirectory = _sTaskName = string.Empty;
            _eBackgroundTask = BackgroundMessage.nType.Stop;
            _eSynchronizationMode = PairOfFiles.nSynchronizationMode.WithDelete;
            _ltDrives = new List<Drive>();
            _ltPairs = new List<PairOfFiles>();
            _blPairs = new BindingList<PairOfFiles>();
            _Cryptography = null;
            _DestinationDrive = new Drive(_Cryptography, false);
            _SourceDrive = new Drive(_Cryptography, true);
            _BackgroundThread = new BackgroundThread(_Cryptography);
            _eMenuTab = nMenuTab.Task;
            _eCaseTab = PairOfFiles.nComparison.SourceOnly;

            _aiPairsCount = new int[(int)PairOfFiles.nComparison.Error + 1];
            for (int i = 0; i < _aiPairsCount.Length; i++)
                _aiPairsCount[i] = 0;

            _aiPairsOrder = new int[(int)PairOfFiles.nComparison.Error + 1];
            _aiPairsOrder[0] = 3;
            _aiPairsOrder[1] = 5;
            _aiPairsOrder[2] = 4;
            _aiPairsOrder[3] = 2;
            _aiPairsOrder[4] = 1;
            _aiPairsOrder[5] = 0;

            _blMessages = new BindingList<Property>
            {
                new Property(DateTime.Now, Translate("ProgramVersion"))
            };

            _SelectedTask = null;
            _isDragOverTasks = _isProgressBarIndeterminate = false;
            _iProgressBarValue = 0;
            _iProgressBarMaximum = ciProgrssBarDefaultMaximum;
            _UserInterfaceTimer = new DispatcherTimer();
            _UserInterfaceTimer.Tick += new EventHandler(UserInterfaceTimerTick);
            _UserInterfaceTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);

            sTemporaryDirectory = Path.GetTempPath();
            ExecuteReadDrivesAndKeys();

            dcCompare = new DelegateCommand(ExecuteCompare, CanExecuteCompare);
            dcF5 = new DelegateCommand(ExecuteF5, CanExecuteF5);
            dcSelectDestination = new DelegateCommand(ExecuteSelectDestination);
            dcSelectSource = new DelegateCommand(ExecuteSelectSource);
            dcSwap = new DelegateCommand(ExecuteSwap, CanExecuteSwap);
            dcSynchronizeCancelOrRecompare = new DelegateCommand(ExecuteSynchronizeCancelOrRecompare, CanExecuteSynchronizeCancelOrRecompare);
        }
        #endregion

        #region commands and methods

        private Property BackgroundMessageToProperty(BackgroundMessage Message)
        {
            string sMessageText = string.Empty;

            switch (Message.eReturnCode)
            {
                case BackgroundMessage.nReturnCode.FileNotFound: sMessageText = string.Format(sFileNotFound, Message.sText); break;
                case BackgroundMessage.nReturnCode.FinishCompare: sMessageText = sFinishCompare; break;
                case BackgroundMessage.nReturnCode.ProgrammingError: sMessageText = sProgrammingError; break;
                case BackgroundMessage.nReturnCode.StartCompare: sMessageText = sStartCompare; break;
                case BackgroundMessage.nReturnCode.UnspecifiedError: sMessageText = sUnspecifiedError; break;
                case BackgroundMessage.nReturnCode.WrongFileFormat: sMessageText = string.Format(sWrongFileFormat, Message.sText); break;
            }
            return new Property(Message.TimeStamp, sMessageText);
        }

        /// <summary></summary>
        private bool CanExecuteCompare()
        {
            return isExecuteCompare;
        }

        /// <summary></summary>
        protected bool CanExecuteF5()
        {
            return isExecuteF5;
        }

        /// <summary></summary>
        protected bool CanExecuteNewKey()
        {
            return isExecuteNewKey;
        }

        /// <summary></summary>
        private bool CanExecuteSwap()
        {
            return isExecuteSwap;
        }

        /// <summary></summary>
        private bool CanExecuteSynchronizeCancelOrRecompare()
        {
            return isExecuteSynchronizeCancelOrRecompare;
        }

        /// <summary>Delegate method invoked by dcCompare.</summary>
        private void ExecuteCompare()
        {
            if (CanExecuteCompare())
            {
                _ltPairs.Clear();   // delete the results of the previous comparison
                RequeryDisplayedPairs(false);
                _BackgroundThread.Enqueue(new BackgroundMessage(BackgroundMessage.nType.Compare, new PairOfFiles(_SourceDrive, _DestinationDrive)));

                if (_BackgroundThread.Start())
                {
                    isProgressBarIndeterminate = true;   // until we know how many files we are processing, show at least that there is some activity
                    _eBackgroundTask = BackgroundMessage.nType.Compare;
                    eMenuTab = nMenuTab.Progress;
                    _UserInterfaceTimer.Start();
                    NotifyPropertyChanged("sSynchronizeCancelOrRecompare");
                }
            }
        }

        /// <summary>Delegate method invoked by dcF5.</summary>
        private void ExecuteF5()
        {
            // if (isExecuteF5)
            // {
            //     ExecuteReadDrivesAndKeys();
            // }
        }

        /// <summary>Delegate method invoked by dcReadDrivesAndKeys.</summary>
        private void ExecuteReadDrivesAndKeys()
        {
            ReadDrivesAndKeys();
            // RequeryDisplayedDrives();
            // RequeryDisplayedKeys();
        }

        /// <summary>Delegate method invoked by dcSelectDestination.</summary>
        private void ExecuteSelectDestination()
        {
            ExecuteFolderBrowserDialog(false);
        }

        /// <summary>Delegate method invoked by dcSelectSource.</summary>
        private void ExecuteSelectSource()
        {
            ExecuteFolderBrowserDialog(true);
        }

        /// <summary></summary>
        private void ExecuteFolderBrowserDialog(bool isSourceFolder)
        {
            using (System.Windows.Forms.FolderBrowserDialog FolderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = isSourceFolder ? sSelectSourceDirectory : sSelectDestinationDirectory,
                SelectedPath = isSourceFolder ? sSourceDirectory : sDestinationDirectory,
                ShowNewFolderButton = !isSourceFolder
            })
            {
                if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (isSourceFolder)
                        sSourceDirectory = FolderDialog.SelectedPath;
                    else
                        sDestinationDirectory = FolderDialog.SelectedPath;
                }
            }
        }

        /// <summary>Delegate method invoked by dcSelectTemporary</summary>
        private void ExecuteSelectTemporary()
        {
            using (System.Windows.Forms.FolderBrowserDialog FolderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = sSelectTemporaryDirectory,
                SelectedPath = sTemporaryDirectory,
                ShowNewFolderButton = true
            })
            {
                if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (Directory.Exists(FolderDialog.SelectedPath))
                        sTemporaryDirectory = FolderDialog.SelectedPath;
                }
            }
        }

        /// <summary>Delegate method invoked by dcSwap.</summary>
        private void ExecuteSwap()
        {
            if (CanExecuteSwap())
            {
                Drive SwapDrive = _SourceDrive;
                _SourceDrive = _DestinationDrive;
                _DestinationDrive = SwapDrive;

                string sSwapDirectory = _sSourceDirectory;
                _sSourceDirectory = _sDestinationDirectory;
                _sDestinationDirectory = sSwapDirectory;

                _SourceDrive.isSource = true;
                _DestinationDrive.isSource = false;

                foreach (PairOfFiles SwapPair in _ltPairs)
                    SwapPair.SwapSourceAndDestination();

                RequeryDisplayedPairs(true);
                NotifyPropertyChanged("sSourceDirectory");
                NotifyPropertyChanged("sDestinationDirectory");
            }
        }

        /// <summary>Delegate method invoked by dcSynchronizeCancelOrRecompare.</summary>
        private void ExecuteSynchronizeCancelOrRecompare()
        {
            int iNewProgressBarMaximum = 0;
            long kTotalBytesToEncrypt = 0;
            IEnumerable<PairOfFiles> qySynchronizeFirst, qyDeleteLast;

            if (isExecuteSynchronizeCancelOrRecompare)
            {
                if (_eBackgroundTask == BackgroundMessage.nType.Stop)   // if there are no background tasks running, start synchronization
                {
                    if (isExecuteSynchronize)
                    {
                        if (_eSynchronizationMode == PairOfFiles.nSynchronizationMode.WithDelete)
                        {
                            // overwrite with newer data, directories first
                            qySynchronizeFirst = from p in _ltPairs where (p.eComparison == PairOfFiles.nComparison.SourceOnly) || (p.eComparison == PairOfFiles.nComparison.SourceNewer) orderby !p.isDirectory, p.sRelativePath select p;
                            // delete or overwrite with older data, directories last, so older data has a better chance of being preserved if the user cancels the synchronization
                            qyDeleteLast = from p in _ltPairs where (p.eComparison == PairOfFiles.nComparison.DestinationOnly) || (p.eComparison == PairOfFiles.nComparison.DestinationNewer) orderby p.isDirectory, p.sRelativePath.Length descending select p;
                        }
                        else
                        {
                            qySynchronizeFirst = from p in _ltPairs where (p.eComparison < PairOfFiles.nComparison.Identical) orderby !p.isDirectory, p.sRelativePath select p;   // directories first
                            qyDeleteLast = null;
                        }

                        foreach (PairOfFiles UnsynchronizedPair in qySynchronizeFirst)
                        {
                            UnsynchronizedPair.eSynchronizationMode = _eSynchronizationMode;
                            iNewProgressBarMaximum += UnsynchronizedPair.iMaximumProgress;
                            _BackgroundThread.Enqueue(new BackgroundMessage(BackgroundMessage.nType.Synchronize, UnsynchronizedPair));
                        }                     

                        if (qyDeleteLast != null)
                        {
                            foreach (PairOfFiles UnsynchronizedPair in qyDeleteLast)
                            {
                                UnsynchronizedPair.eSynchronizationMode = _eSynchronizationMode;
                                iNewProgressBarMaximum += UnsynchronizedPair.iMaximumProgress;
                                _BackgroundThread.Enqueue(new BackgroundMessage(BackgroundMessage.nType.Synchronize, UnsynchronizedPair));
                            }
                        }

                        if (kTotalBytesToEncrypt > Drive.ciFileSizeLimitForTesting)
                            MessageBox.Show(sErrorWorkingMemoryLimit);
                        else
                        {
                            if (_BackgroundThread.Start())
                            {
                                iProgressBarMaximum = iNewProgressBarMaximum;
                                _eBackgroundTask = BackgroundMessage.nType.Synchronize;
                                NotifyPropertyChanged("sSynchronizeCancelOrRecompare");
                                _UserInterfaceTimer.Start();
                            }
                        }
                    }
                    else
                        ExecuteCompare();
                }
                else if ((_eBackgroundTask == BackgroundMessage.nType.Synchronize) || (_eBackgroundTask == BackgroundMessage.nType.Compare))   // if synchronization is running, request it to cancel
                {
                    _eBackgroundTask = BackgroundMessage.nType.Cancelled;
                    _BackgroundThread.RequestCancel();
                }
            }
        }

        /// <summary>Delegate method invoked by dcIsClosing.</summary>
        protected override void ExecuteIsClosing()
        {
            if (_BackgroundThread != null)
            {
                _BackgroundThread.Dispose();
                _BackgroundThread = null;
            }
            if (_Cryptography != null)
            {
                _Cryptography.Dispose();
                _Cryptography = null;
            }
            if (_DestinationDrive != null)
            {
                _DestinationDrive.Dispose();
                _DestinationDrive = null;
            }
            if (_SourceDrive != null)
            {
                _SourceDrive.Dispose();
                _SourceDrive = null;
            }
        }

        /// <summary></summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private bool IsSynchronizationAllowed(string sFirstDirectory, string sSecondDirectory)
        {
            bool isReturn;
            int iFirstLength, iSecondLength;

            if (string.IsNullOrEmpty(sFirstDirectory) || string.IsNullOrEmpty(sSecondDirectory))
                isReturn = true;
            else
            {
                iFirstLength = sFirstDirectory.Length;
                iSecondLength = sSecondDirectory.Length;

                if (iFirstLength < iSecondLength)
                    isReturn = sFirstDirectory != sSecondDirectory.Substring(0, iFirstLength);
                else
                    isReturn = sFirstDirectory.Substring(0, iSecondLength) != sSecondDirectory;
            }
            return isReturn;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private string ModeInformationPhrase(bool isDelete)
        {
            string sReturn = string.Empty;

            switch (_eSynchronizationMode)
            {
                case PairOfFiles.nSynchronizationMode.TwoWay: sReturn = Translate("CopiedToSource"); break;
                case PairOfFiles.nSynchronizationMode.WithDelete: sReturn = isDelete ? Translate("Deleted") : Translate("ReplacedFromSource"); break;
                case PairOfFiles.nSynchronizationMode.NoDelete: sReturn = Translate("Skipped"); break;
            }
            return sReturn;
        }

        /// <summary></summary>
        private void NotifyPropertyChangedSynchronizationMode()
        {
            NotifyPropertyChanged("isModeWithDelete");
            NotifyPropertyChanged("isModeNoDelete");
            NotifyPropertyChanged("isModeTwoWay");
            NotifyPropertyChanged("sModeInformation");
        }

        /// <summary></summary>
        private void ReadDrivesAndKeys()
        {
            Drive NewDrive;
            DriveInfo[] _aDriveInfo = DriveInfo.GetDrives();

            _ltDrives.Clear();
            foreach (DriveInfo Info in _aDriveInfo)
            {
                NewDrive = new Drive(Info);
                _ltDrives.Add(NewDrive);
            }
        }

        private string ReplaceExtension(string sFilePath, string sOldExtension, string sNewExtension)
        {
            if (string.IsNullOrEmpty(sFilePath) || (sFilePath.Length < sOldExtension.Length) || (sFilePath.Length < sNewExtension.Length) || (sFilePath.Substring(sFilePath.Length - sOldExtension.Length) != sOldExtension))
                return sFilePath;
            else
                return sFilePath.Substring(0, sFilePath.Length - sOldExtension.Length) + sNewExtension;
        }

        /// <summary></summary>
        /// <param name=""></param>
        private void RequeryDisplayedPairs(bool isChangeTab)
        {
            int iIndex;
            IEnumerable<PairOfFiles> qyFoundPairs;

            _blPairs.Clear();
            for (int i = 0; i < _aiPairsCount.Length; i++)
            {
                iIndex = _aiPairsOrder[i];
                qyFoundPairs = from p in _ltPairs where p.eComparison == (PairOfFiles.nComparison)iIndex orderby p.sRelativePath select p;
                _aiPairsCount[iIndex] = qyFoundPairs.Count();

                if (isChangeTab && (_aiPairsCount[iIndex] > 0))
                {
                    iCaseTab = iIndex;
                    _blPairs.Clear();
                }

                if (iCaseTab == iIndex)
                {
                    foreach (PairOfFiles FoundPair in qyFoundPairs)
                        _blPairs.Add(FoundPair);
                }
            }

            NotifyPropertyChanged("sHeaderSourceOnly");
            NotifyPropertyChanged("sHeaderDestinationOnly");
            NotifyPropertyChanged("sHeaderSourceNewer");
            NotifyPropertyChanged("sHeaderDestinationNewer");
            NotifyPropertyChanged("sHeaderIdentical");
            NotifyPropertyChanged("sHeaderError");
        }

        /// <summary>Timer event handler that updates the user interface in regular intervals.</summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void UserInterfaceTimerTick(object sender, EventArgs e)
        {
            bool isListOfPairsChanged = false;
            bool isProgressChanged = false;

            while (!_BackgroundThread.quReturn.IsEmpty)
            {
                _BackgroundThread.quReturn.TryDequeue(out BackgroundMessage BackgroundMessage);

                if (BackgroundMessage != null)
                {
                    switch (BackgroundMessage.eType)
                    {
                        case BackgroundMessage.nType.NewPair:
                            if (BackgroundMessage.PairProperty != null)
                            {
                                _ltPairs.Add(BackgroundMessage.PairProperty);
                                _iProgressBarValue += 1;
                                _sBackgroundStatus = BackgroundMessage.PairProperty.sRelativePath;
                                isListOfPairsChanged = true;
                                isProgressChanged = true;
                            }
                            break;

                        case BackgroundMessage.nType.ReportProgress:
                            if (BackgroundMessage.PairProperty == null)
                            {
                                _iProgressBarValue += BackgroundMessage.iValue;
                                isProgressChanged = true;
                            }
                            else if (_ltPairs.Contains(BackgroundMessage.PairProperty))
                            {
                                _ltPairs.Remove(BackgroundMessage.PairProperty);
                                _ltPairs.Add(BackgroundMessage.PairProperty);
                            }
                            isListOfPairsChanged = true;
                            break;

                        case BackgroundMessage.nType.SetupProgress:
                            if ((BackgroundMessage.iValue <= BackgroundMessage.iProgressMaximum) && (BackgroundMessage.iProgressMaximum > 0))
                            {
                                isProgressBarIndeterminate = false;
                                _iProgressBarValue = BackgroundMessage.iValue;
                                iProgressBarMaximum = BackgroundMessage.iProgressMaximum;
                            }
                            else
                            {
                                isProgressBarIndeterminate = true;
                                _iProgressBarValue = 0;
                                iProgressBarMaximum = ciProgrssBarDefaultMaximum;
                            }
                            isProgressChanged = true;
                            break;

                        case BackgroundMessage.nType.Status:
                            if (BackgroundMessage.PairProperty != null)
                                _sBackgroundStatus = BackgroundMessage.PairProperty.sRelativePath;
                            else
                                _sBackgroundStatus = BackgroundMessage.sText;
                            isListOfPairsChanged = false;
                            isProgressChanged = true;
                            break;

                        case BackgroundMessage.nType.Stop:
                            _ltPairs.RemoveAll(Item => (Item.eComparison == PairOfFiles.nComparison.Deleted));
                            isProgressBarIndeterminate = false;
                            _iProgressBarValue = 0;
                            iProgressBarMaximum = ciProgrssBarDefaultMaximum;
                            _sBackgroundStatus = string.Empty;
                            isProgressChanged = true;
                            if (_UserInterfaceTimer.IsEnabled)
                                _UserInterfaceTimer.Stop();

                            if (_eBackgroundTask == BackgroundMessage.nType.Cancelled)
                                _ltPairs.Clear();   // delete the results of the comparison

                            _eBackgroundTask = BackgroundMessage.nType.Stop;
                            RequeryDisplayedPairs(true);
                            isListOfPairsChanged = false;
                            NotifyPropertyChanged("isEditKeys");
                            NotifyPropertyChanged("sCreateOrCancel");
                            NotifyPropertyChanged("sSynchronizeCancelOrRecompare");
                            break;

                        case BackgroundMessage.nType.UserMessage:
                            Property NewProperty = BackgroundMessageToProperty(BackgroundMessage);
                            _blMessages.Add(NewProperty);
                            if (BackgroundMessage.iValue > 0)
                                eMenuTab = nMenuTab.Messages;
                            break;
                    }
                }
            }

            if (isListOfPairsChanged)
                RequeryDisplayedPairs(false);

            if (isProgressChanged)
            {
                NotifyPropertyChanged("iProgressBarValue");
                NotifyPropertyChanged("sStatus");
                CommandManager.InvalidateRequerySuggested();
            }
        }
        #endregion
    }
}
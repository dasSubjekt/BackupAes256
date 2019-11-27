namespace BackupAes256.ViewModel
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Reflection;
    using BackupAes256.Model;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.ComponentModel;
    using System.Windows.Threading;
    using System.Collections.Generic;
    using System.Windows.Media.Imaging;


    public partial class MainViewModel : ViewModelBase
    {
        #region constructors

        /// <summary>Initializes a new instance of the MainViewModel class.</summary>
        public MainViewModel() : base()
        {
#if ENGLISH
            _dyTranslations.Add("AesKey", "256-bit AES key");
            _dyTranslations.Add("AuthenticationAndDecryptionSuccessful", "File »{0:s}« was decrypted successfully.");
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
            _dyTranslations.Add("DestinationDirectorySymmetric", "destination directory  (" + Drive.csSymmetricFileExtension + ")");
            _dyTranslations.Add("DestinationDirectoryUnencrypted", "destination directory");
            _dyTranslations.Add("DestinationFile", "destination file");
            _dyTranslations.Add("DestinationFileAsymmetric", "destination file  (" + Drive.csAsymmetricFileExtension + ")");
            _dyTranslations.Add("DestinationFileSymmetric", "destination file  (" + Drive.csSymmetricFileExtension + ")");
            _dyTranslations.Add("DestinationNewer", "destination newer");
            _dyTranslations.Add("DestinationOnly", "destination only");
            _dyTranslations.Add("DestinationSizeText", "destination size");
            _dyTranslations.Add("DriveText", "drive");
            _dyTranslations.Add("DummyDriveName", "not saved");
            _dyTranslations.Add("Encrypted", "encrypted");
            _dyTranslations.Add("ErrorDirectoriesIdentical", "Identical or nested directories cannot be synchronized.");
            _dyTranslations.Add("ErrorFilesIdentical", "Identical files cannot be synchronized.");
            _dyTranslations.Add("ErrorMessageText", "error message");
            _dyTranslations.Add("ErrorWorkingMemoryLimit", "Encrypting several gigabyte of data is not (yet) possible.");
            _dyTranslations.Add("Export", "export");
            _dyTranslations.Add("Failure", "failure");
            _dyTranslations.Add("FileNotFound", "File »{0:s}« was not found.");
            _dyTranslations.Add("FinishCompare", "Comparison of the directories was finished.");
            _dyTranslations.Add("FinishDecryption", "Decryption of the file was finished.");
            _dyTranslations.Add("FinishEncryption", "Encryption of the file was finished.");
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
            _dyTranslations.Add("KeyParameterD", "decryption exponent d");
            _dyTranslations.Add("KeyParameterDP", "dp = d mod (p - 1)");
            _dyTranslations.Add("KeyParameterDQ", "dq = d mod (q - 1)");
            _dyTranslations.Add("KeyParameterEmail", "Email address");
            _dyTranslations.Add("KeyParameterExponent", "public exponent e");
            _dyTranslations.Add("KeyParameterHomePage", "home page");
            _dyTranslations.Add("KeyParameterInverseQ", "invq = (1 / q) mod p");
            _dyTranslations.Add("KeyParameterModulus", "modulus = p * q");
            _dyTranslations.Add("KeyParameterOwner", "owner");
            _dyTranslations.Add("KeyParameterP", "prime factor p");
            _dyTranslations.Add("KeyParameterQ", "prime factor q");
            _dyTranslations.Add("KeyParameterSymmetric", "symmetric key");
            _dyTranslations.Add("Keys", "keys");
            _dyTranslations.Add("KeyTextPublic", "public");
            _dyTranslations.Add("KeyTooLong", "The value for the 256-bit key is too long.");
            _dyTranslations.Add("KeyTooShort", "The value for the 256-bit key is too short.");
            _dyTranslations.Add("KeyTypeAsymmetric4096Bit", "asymmetric, 4096 bits");
            _dyTranslations.Add("KeyTypeAsymmetric7680Bit", "asymmetric, 7680 bits (~AES-192)");
            _dyTranslations.Add("KeyTypeAsymmetric8192Bit", "asymmetric, 8192 bits");
            _dyTranslations.Add("KeyTypeAsymmetric15360Bit", "asymmetric, 15360 bits (~AES-256)");
            _dyTranslations.Add("KeyTypeAsymmetric16384Bit", "asymmetric, 16384 bits");
            _dyTranslations.Add("KeyTypeAsymmetricPrivate", "asymmetric");
            _dyTranslations.Add("KeyTypeAsymmetricPublic", "asymmetric");
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
            _dyTranslations.Add("NoAsymmetricKey", "No private RSA key was found to match these data.");
            _dyTranslations.Add("NoAuthenticationKey", "No authentication key was found to match these data.");
            _dyTranslations.Add("NoSymmetricKey", "No symmetric key was found to match these data.");
            _dyTranslations.Add("OnDrive", "save on drive");
            _dyTranslations.Add("PleaseCheck", "please check");
            _dyTranslations.Add("ProgrammingError", "A programming error occurred. I am sorry, this should not have happened.");
            _dyTranslations.Add("ProgramVersion", "Program version " + sProgramVersion + " of 26/11/2019 is ready.");
            _dyTranslations.Add("Progress", "progress");
            _dyTranslations.Add("ReadDrivesAndKeys", "re-read drives");
            _dyTranslations.Add("RelativePathText", "relative path");
            _dyTranslations.Add("ReplacedFromSource", "replaced with the older version from the source directory");
            _dyTranslations.Add("Reserve", "reserve");
            _dyTranslations.Add("RsaKey", "public RSA key");
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
            _dyTranslations.Add("SourceDirectorySymmetric", "source directory  (" + Drive.csSymmetricFileExtension + ")");
            _dyTranslations.Add("SourceDirectoryUnencrypted", "source directory");
            _dyTranslations.Add("SourceFile", "source file");
            _dyTranslations.Add("SourceFileAsymmetric", "source file  (" + Drive.csAsymmetricFileExtension + ")");
            _dyTranslations.Add("SourceFileMissing", "The source file does not exist.");
            _dyTranslations.Add("SourceFileSymmetric", "source file  (" + Drive.csSymmetricFileExtension + ")");
            _dyTranslations.Add("SourceSizeText", "source size");
            _dyTranslations.Add("SourceNewer", "source newer");
            _dyTranslations.Add("SourceOnly", "source only");
            _dyTranslations.Add("StartCompare", "Comparison of the directories was started.");
            _dyTranslations.Add("StartDecryption", "Decryption of the file was started.");
            _dyTranslations.Add("StartEncryption", "Encryption of the file was started.");
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
            _dyTranslations.Add("WindowTitle", "Backup and file synchronization with symmetric and hybrid encryption");
            _dyTranslations.Add("WrongFileFormat", "File »{0:s}« has a wrong format and could not be read.");

#elif DEUTSCH
            _dyTranslations.Add("AesKey", "256-Bit AES-Schlüssel");
            _dyTranslations.Add("AuthenticationAndDecryptionSuccessful", "Die Datei »{0:s}« wurde erfolgreich entschlüsselt.");
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
            _dyTranslations.Add("DestinationDirectorySymmetric", "Zielverzeichnis  (" + Drive.csSymmetricFileExtension + ")");
            _dyTranslations.Add("DestinationDirectoryUnencrypted", "Zielverzeichnis");
            _dyTranslations.Add("DestinationFile", "Zieldatei");
            _dyTranslations.Add("DestinationFileAsymmetric", "Zieldatei  (" + Drive.csAsymmetricFileExtension + ")");
            _dyTranslations.Add("DestinationFileSymmetric", "Zieldatei  (" + Drive.csSymmetricFileExtension + ")");
            _dyTranslations.Add("DestinationNewer", "neueres Ziel");
            _dyTranslations.Add("DestinationOnly", "nur Ziel");
            _dyTranslations.Add("DestinationSizeText", "Größe Ziel");
            _dyTranslations.Add("DriveText", "Laufwerk");
            _dyTranslations.Add("DummyDriveName", "ungespeichert");
            _dyTranslations.Add("Encrypted", "verschlüsselt");
            _dyTranslations.Add("ErrorDirectoriesIdentical", "Identische oder in einander enthaltene Verzeichnisse können nicht synchronisiert werden.");
            _dyTranslations.Add("ErrorFilesIdentical", "Identische Dateien können nicht synchronisiert werden.");
            _dyTranslations.Add("ErrorMessageText", "Fehlernachricht");
            _dyTranslations.Add("ErrorWorkingMemoryLimit", "Das Verschlüsseln mehrerer Gigabyte an Daten ist (noch) nicht möglich.");
            _dyTranslations.Add("Export", "exportieren");
            _dyTranslations.Add("Failure", "fehlerhaft");
            _dyTranslations.Add("FileNotFound", "Die Datei »{0:s}« wurde nicht gefunden.");
            _dyTranslations.Add("FinishCompare", "Der Vergleich der Verzeichnisse wurde beendet.");
            _dyTranslations.Add("FinishDecryption", "Das Entschlüsseln der Datei wurde beendet.");
            _dyTranslations.Add("FinishEncryption", "Das Verschlüsseln der Datei wurde beendet.");
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
            _dyTranslations.Add("KeyParameterD", "Entschlüsselungsexp. d");
            _dyTranslations.Add("KeyParameterDP", "dp = d mod (p - 1)");
            _dyTranslations.Add("KeyParameterDQ", "dq = d mod (q - 1)");
            _dyTranslations.Add("KeyParameterEmail", "E-Mail-Adresse");
            _dyTranslations.Add("KeyParameterExponent", "öffentlicher Exponent e");
            _dyTranslations.Add("KeyParameterHomePage", "Homepage");
            _dyTranslations.Add("KeyParameterInverseQ", "invq = (1 / q) mod p");
            _dyTranslations.Add("KeyParameterModulus", "Modul = p * q");
            _dyTranslations.Add("KeyParameterOwner", "Besitzer");
            _dyTranslations.Add("KeyParameterP", "Primfaktor p");
            _dyTranslations.Add("KeyParameterQ", "Primfaktor q");
            _dyTranslations.Add("KeyParameterSymmetric", "symmetrischer Schlüssel");
            _dyTranslations.Add("Keys", "Schlüssel");
            _dyTranslations.Add("KeyTextPublic", "öffentlich");
            _dyTranslations.Add("KeyTooLong", "Der Wert für den 256-Bit-Schlüssel ist zu lang.");
            _dyTranslations.Add("KeyTooShort", "Der Wert für den 256-Bit-Schlüssel ist zu kurz.");
            _dyTranslations.Add("KeyTypeAsymmetric4096Bit", "asymmetrisch, 4096 Bit");
            _dyTranslations.Add("KeyTypeAsymmetric7680Bit", "asymmetrisch, 7680 Bit (~AES-192)");
            _dyTranslations.Add("KeyTypeAsymmetric8192Bit", "asymmetrisch, 8192 Bit");
            _dyTranslations.Add("KeyTypeAsymmetric15360Bit", "asymmetrisch, 15360 Bit (~AES-256)");
            _dyTranslations.Add("KeyTypeAsymmetric16384Bit", "asymmetrisch, 16384 Bit");
            _dyTranslations.Add("KeyTypeAsymmetricPrivate", "asymmetrisch");
            _dyTranslations.Add("KeyTypeAsymmetricPublic", "asymmetrisch");
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
            _dyTranslations.Add("NoAsymmetricKey", "Zu diesen Daten wurde kein privater RSA-Schlüssel gefunden.");
            _dyTranslations.Add("NoAuthenticationKey", "Zu diesen Daten wurde kein Authentifizierungs-Schlüssel gefunden.");
            _dyTranslations.Add("NoSymmetricKey", "Zu diesen Daten wurde kein symmetrischer Schlüssel gefunden.");
            _dyTranslations.Add("OnDrive", "auf Laufwerk");
            _dyTranslations.Add("PleaseCheck", "bitte überprüfen");
            _dyTranslations.Add("ProgrammingError", "Ein Programmierfehler ist aufgetreten. Entschuldigung, das hätte nicht passieren dürfen.");
            _dyTranslations.Add("ProgramVersion", "Die Programmversion " + sProgramVersion + " vom 26.11.2019 ist bereit.");
            _dyTranslations.Add("Progress", "Fortschritt");
            _dyTranslations.Add("ReadDrivesAndKeys", "Laufwerke neu lesen");
            _dyTranslations.Add("RelativePathText", "Relativer Pfad");
            _dyTranslations.Add("ReplacedFromSource", "durch die ältere Version aus dem Quellverzeichnis ersetzt");
            _dyTranslations.Add("Reserve", "reserviere");
            _dyTranslations.Add("RsaKey", "öffentlicher RSA-Schlüssel");
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
            _dyTranslations.Add("SourceDirectorySymmetric", "Quellverzeichnis  (" + Drive.csSymmetricFileExtension + ")");
            _dyTranslations.Add("SourceDirectoryUnencrypted", "Quellverzeichnis");
            _dyTranslations.Add("SourceFile", "Quelldatei");
            _dyTranslations.Add("SourceFileAsymmetric", "Quelldatei  (" + Drive.csAsymmetricFileExtension + ")");
            _dyTranslations.Add("SourceFileMissing", "Die Quelldatei existiert nicht.");
            _dyTranslations.Add("SourceFileSymmetric", "Quelldatei  (" + Drive.csSymmetricFileExtension + ")");
            _dyTranslations.Add("SourceSizeText", "Größe Quelle");
            _dyTranslations.Add("SourceNewer", "neuere Quelle");
            _dyTranslations.Add("SourceOnly", "nur Quelle");
            _dyTranslations.Add("StartCompare", "Der Vergleich der Verzeichnisse wurde gestartet.");
            _dyTranslations.Add("StartDecryption", "Das Entschlüsseln der Datei wurde gestartet.");
            _dyTranslations.Add("StartEncryption", "Das Verschlüsseln der Datei wurde gestartet.");
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
            _dyTranslations.Add("WindowTitle", "Sicherungskopien und Dateisynchronisierung mit symmetrischer und hybrider Verschlüsselung");
            _dyTranslations.Add("WrongFileFormat", "Die Datei »{0:s}« hat ein falsches Format und kann nicht gelesen werden.");
#endif
            _dyTranslations.Add("HiddenKey", "●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●");


            _sBackgroundStatus = _sDestinationFileOrDirectory = _sKeyValue = _sSourceFileOrDirectory = _sTaskName = string.Empty;
            _eBackgroundTask = BackgroundMessage.nType.Stop;
            _eSynchronizationMode = PairOfFiles.nSynchronizationMode.WithDelete;
            _ltDrives = new List<Drive>();
            _ltKeys = new List<CryptoKey>();
            _ltPairs = new List<PairOfFiles>();
            _blAsymmetricKeys = new BindingList<CryptoKey>();
            _blKeys = new BindingList<CryptoKey>();
            _blPrivateKeys = new BindingList<CryptoKey>();
            _blSymmetricKeys = new BindingList<CryptoKey>();
            _blPairs = new BindingList<PairOfFiles>();
            _Cryptography = new CryptoServices();
            _DestinationDrive = new Drive(_Cryptography, _ltKeys, false);
            _SourceDrive = new Drive(_Cryptography, _ltKeys, true);
            _BackgroundThread = new BackgroundThread(_Cryptography);
            _eMenuTab = nMenuTab.Task;
            _eCaseTab = PairOfFiles.nComparison.SourceOnly;
            _SelectedKey = _SelectedKeyFileAesKey = null;

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

            _blDrives = new BindingList<Drive>();
            _SelectedKeyDrive = null;

            _blBlockSizes = new BindingList<Property>
            {
                new Property(12, 0x001000, "4 KB  "),
                new Property(13, 0x002000, "8 KB  "),
                new Property(14, 0x004000, "16 KB  "),
                new Property(15, 0x008000, "32 KB  "),
                new Property(16, 0x010000, "64 KB  "),
                new Property(17, 0x020000, "128 KB  "),
                new Property(18, 0x040000, "256 KB  "),
                new Property(19, 0x080000, "512 KB  "),
                new Property(20, 0x100000, "1 MB  "),
                new Property(21, 0x200000, "2 MB  "),
                new Property(22, 0x400000, "4 MB  ")
            };

            _blDestinationOptions = new BindingList<Property>
            {
                new Property((int)Drive.nEncryptionType.FileAsymmetric, 0, Translate("DestinationFileAsymmetric")),
                new Property((int)Drive.nEncryptionType.FileSymmetric, 0, Translate("DestinationFileSymmetric")),
                // new Property((int)Drive.nEncryptionType.DirectorySymmetric, 0, Translate("DestinationDirectorySymmetric")),
                new Property((int)Drive.nEncryptionType.DirectoryUnencrypted, 0, Translate("DestinationDirectoryUnencrypted"))
            };

            _blFileSystemLevels = new BindingList<Property>
            {
                new Property(3, 3, "46 656  (3)  "),
                new Property(4, 4, "1 679 616  (4)  "),
                new Property(5, 5, "60 466 176  (5)  "),
                new Property(6, 6, "2 176 782 336  (6)  ")
            };

            _blKeyNumeralsPrivate = new BindingList<Property>
            {
                new Property(0, 0, Translate("KeyNumerals0")),
                new Property(2, 2, Translate("KeyNumerals2")),
                new Property(10, 10, Translate("KeyNumerals10")),
                new Property(16, 16, Translate("KeyNumerals16")),
                new Property(64, 64, Translate("KeyNumerals64")),
            };

            _blKeyNumeralsPublic = new BindingList<Property>
            {
                new Property(2, 2, Translate("KeyNumerals2")),
                new Property(10, 10, Translate("KeyNumerals10")),
                new Property(16, 16, Translate("KeyNumerals16")),
                new Property(64, 64, Translate("KeyNumerals64")),
            };

            _blKeyTextPublic = new BindingList<Property>
            {
                new Property(1, 1, Translate("KeyTextPublic")),
            };
            _blKeyNumerals = null;
            _iSelectedKeyNumeral = 1;

            _blKeyParametersAsymmetric = new BindingList<Property>
            {
                new Property((int)CryptoKey.nKeyParameter.D, 0, Translate("KeyParameterD")),
                new Property((int)CryptoKey.nKeyParameter.DP, 0, Translate("KeyParameterDP")),
                new Property((int)CryptoKey.nKeyParameter.DQ, 0, Translate("KeyParameterDQ")),
                new Property((int)CryptoKey.nKeyParameter.Exponent, 0, Translate("KeyParameterExponent")),
                new Property((int)CryptoKey.nKeyParameter.InverseQ, 0, Translate("KeyParameterInverseQ")),
                new Property((int)CryptoKey.nKeyParameter.Modulus, 0, Translate("KeyParameterModulus")),
                new Property((int)CryptoKey.nKeyParameter.P, 0, Translate("KeyParameterP")),
                new Property((int)CryptoKey.nKeyParameter.Q, 0, Translate("KeyParameterQ")),
                new Property((int)CryptoKey.nKeyParameter.Owner, 0, Translate("KeyParameterOwner")),
                new Property((int)CryptoKey.nKeyParameter.Email, 0, Translate("KeyParameterEmail")),
                new Property((int)CryptoKey.nKeyParameter.Homepage, 0, Translate("KeyParameterHomePage"))
            };
            _eSelectedKeyParameterAsymmetric = CryptoKey.nKeyParameter.Modulus;
            _iKeyNumeralIfCanHide = 0;

            _blMemoryLimits = new BindingList<Property>
            {
                new Property(1, 1048576, "1 MB  "),
                new Property(5, 5242880, "5 MB  "),
                new Property(10, 10485760, "10 MB  "),
                new Property(50, 52428800, "50 MB  "),
                new Property(100, 104857600, "100 MB  "),
                new Property(500, 524288000, "500 MB  "),
                new Property(1000, 1048576000, "1000 MB  "),
                new Property(1500, 1572864000, "1500 MB  "),
                new Property(2000, 2097152000, "2000 MB  ")
            };

            _blMessages = new BindingList<Property>
            {
                new Property(DateTime.Now, Translate("ProgramVersion"))
            };

            _blNewKeyTypes = new BindingList<Property>
            {
                new Property(32, (int)CryptoKey.nKeyType.Symmetric, Translate("KeyTypeSymmetric256Bit")),
                new Property(512, (int)CryptoKey.nKeyType.AsymmetricPrivate, Translate("KeyTypeAsymmetric4096Bit")),
                new Property(960, (int)CryptoKey.nKeyType.AsymmetricPrivate, Translate("KeyTypeAsymmetric7680Bit")),
                new Property(1024, (int)CryptoKey.nKeyType.AsymmetricPrivate, Translate("KeyTypeAsymmetric8192Bit")),
                new Property(1920, (int)CryptoKey.nKeyType.AsymmetricPrivate, Translate("KeyTypeAsymmetric15360Bit")),
                new Property(2048, (int)CryptoKey.nKeyType.AsymmetricPrivate, Translate("KeyTypeAsymmetric16384Bit"))
            };
            _iSelectedNewKeyBytes = 32;

            _blSourceOptions = new BindingList<Property>
            {
                new Property((int)Drive.nEncryptionType.FileAsymmetric, 0, Translate("SourceFileAsymmetric")),
                new Property((int)Drive.nEncryptionType.FileSymmetric, 0, Translate("SourceFileSymmetric")),
                // new Property((int)Drive.nEncryptionType.DirectorySymmetric, 0, Translate("SourceDirectorySymmetric")),
                new Property((int)Drive.nEncryptionType.DirectoryUnencrypted, 0, Translate("SourceDirectoryUnencrypted"))
            };

            _blTasks = new BindingList<Property>();
            _SelectedTask = null;
            _isDragOverTasks = _isKeyValueFocused = _isProgressBarIndeterminate = false;
            _iProgressBarValue = 0;
            _iProgressBarMaximum = ciProgrssBarDefaultMaximum;
            _iWorkingMemoryLimit = Drive.ciDefaultWorkingMemoryLimit;
            _UserInterfaceTimer = new DispatcherTimer();
            _UserInterfaceTimer.Tick += new EventHandler(UserInterfaceTimerTick);
            _UserInterfaceTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);

            _sApplicationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            sTemporaryDirectory = Path.GetTempPath();
            _IconKeys = LoadBitmap("Letter.png");
            _IconMessages = LoadBitmap("News.png");
            _IconProgress = LoadBitmap("Ship.png");
            _IconTask = LoadBitmap("Bottle.png");

            ExecuteReadDrivesAndKeys();
            _ViewSourceKeys = CollectionViewSource.GetDefaultView(_blKeys);

            dcCompare = new DelegateCommand(ExecuteCompare, CanExecuteCompare);
            dcExportKey = new DelegateCommand(ExecuteExportKey, CanExecuteExportKey);
            dcF5 = new DelegateCommand(ExecuteF5, CanExecuteF5);
            dcNewKey = new DelegateCommand(ExecuteNewKey, CanExecuteNewKey);
            dcReadDrivesAndKeys = new DelegateCommand(ExecuteReadDrivesAndKeys);
            dcSaveKey = new DelegateCommand(ExecuteSaveKey, CanExecuteSaveKey);
            dcSaveTask = new DelegateCommand(ExecuteSaveTask);
            dcSelectDestination = new DelegateCommand(ExecuteSelectDestination);
            dcSelectSource = new DelegateCommand(ExecuteSelectSource);
            dcSelectTemporary = new DelegateCommand(ExecuteSelectTemporary);
            dcSwap = new DelegateCommand(ExecuteSwap, CanExecuteSwap);
            dcSynchronizeCancelOrRecompare = new DelegateCommand(ExecuteSynchronizeCancelOrRecompare, CanExecuteSynchronizeCancelOrRecompare);
        }
        #endregion

        #region commands and methods

        private void AddKey(CryptoKey Key)
        {
            // if ((Key.eType == CryptoKey.nKeyType.AsymmetricPrivate) && (_ltKeys.Contains(Key)))
            //     _ltKeys.Remove(Key);

            if (!_ltKeys.Contains(Key))
                _ltKeys.Add(Key);
        }

        private Property BackgroundMessageToProperty(BackgroundMessage Message)
        {
            string sMessageText = string.Empty;

            switch (Message.eReturnCode)
            {
                case BackgroundMessage.nReturnCode.AuthenticationAndDecryptionSuccessful: sMessageText = string.Format(sAuthenticationAndDecryptionSuccessful, Message.sText); break;
                case BackgroundMessage.nReturnCode.FileNotFound: sMessageText = string.Format(sFileNotFound, Message.sText); break;
                case BackgroundMessage.nReturnCode.FinishCompare: sMessageText = sFinishCompare; break;
                case BackgroundMessage.nReturnCode.FinishDecryption: sMessageText = sFinishDecryption; break;
                case BackgroundMessage.nReturnCode.FinishEncryption: sMessageText = sFinishEncryption; break;
                case BackgroundMessage.nReturnCode.FinishFillKey: sMessageText = sFinishFillKey; break;
                case BackgroundMessage.nReturnCode.FoundAuthenticationKey: sMessageText = string.Format(sFoundAuthenticationKey, Message.sText); break;
                case BackgroundMessage.nReturnCode.FoundPrivateKey: sMessageText = string.Format(sFoundPrivateKey, Message.sText); break;
                case BackgroundMessage.nReturnCode.FoundSymmetricKey: sMessageText = string.Format(sFoundSymmetricKey, Message.sText); break;
                case BackgroundMessage.nReturnCode.NoAsymmetricKey: sMessageText = sNoAsymmetricKey; break;
                case BackgroundMessage.nReturnCode.NoAuthenticationKey: sMessageText = sNoAuthenticationKey; break;
                case BackgroundMessage.nReturnCode.NoSymmetricKey: sMessageText = sNoSymmetricKey; break;
                case BackgroundMessage.nReturnCode.ProgrammingError: sMessageText = sProgrammingError; break;
                case BackgroundMessage.nReturnCode.StartCompare: sMessageText = sStartCompare; break;
                case BackgroundMessage.nReturnCode.StartDecryption: sMessageText = sStartDecryption; break;
                case BackgroundMessage.nReturnCode.StartEncryption: sMessageText = sStartEncryption; break;
                case BackgroundMessage.nReturnCode.StartFillKey: sMessageText = sStartFillKey; break;
                case BackgroundMessage.nReturnCode.UnspecifiedError: sMessageText = sUnspecifiedError; break;
                case BackgroundMessage.nReturnCode.WrongFileFormat: sMessageText = sWrongFileFormat; break;
            }
            return new Property(Message.TimeStamp, sMessageText);
        }

        /// <summary></summary>
        private bool CanExecuteCompare()
        {
            return isExecuteCompare;
        }

        /// <summary></summary>
        protected bool CanExecuteExportKey()
        {
            return isExecuteExportKey;
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
                    // eCaseTab = PairOfFiles.nComparison.Identical;
                    eMenuTab = nMenuTab.Progress;
                    _UserInterfaceTimer.Start();
                    NotifyPropertyChanged("sSynchronizeCancelOrRecompare");
                }
            }
        }

        /// <summary>Delegate method invoked by dcExportKey.</summary>
        private void ExecuteExportKey()
        {
            if (isExecuteExportKey)
            {
                CryptoKey KeyToSave = new CryptoKey(_SelectedKey.sName, CryptoKey.nKeyFormat.Public, CryptoKey.nKeyType.AsymmetricPublic, _SelectedKey.iBytes)
                {
                    sOwner = _SelectedKey.sOwner,
                    sEmail = _SelectedKey.sEmail,
                    sHomepage = _SelectedKey.sHomepage,
                    abRsaExponent = _SelectedKey.abRsaExponent,   // Passing these by reference would be a problem if the user could change asymmetric key values.
                    abRsaModulus = _SelectedKey.abRsaModulus      // Editing one key would then simultaneously and illogically change the other key.
                };
                KeyToSave.Save(_SelectedKeyDrive);
                AddKey(KeyToSave);
                SelectedKey = null;
                RequeryDisplayedKeys();
                SelectedKey = KeyToSave;
            }
        }

        /// <summary>Delegate method invoked by dcF5.</summary>
        private void ExecuteF5()
        {
            if (isExecuteF5)
            {
                ExecuteReadDrivesAndKeys();
            }
        }

        /// <summary>Delegate method invoked by dcNewKey.</summary>
        private void ExecuteNewKey()
        {
            CryptoKey NewKey;
            int iNameCounter = 1;
            string sNewKeyName;

            if (isExecuteNewKey)
            {
                if (_eBackgroundTask == BackgroundMessage.nType.Stop)
                {
                    if (_iSelectedNewKeyBytes == CryptoServices.ciAesKeyBytesLength)
                    {
                        sNewKeyName = Translate("KeyTypeSymmetric");
                        while (_ltKeys.SingleOrDefault(key => key.sName == sNewKeyName) != null)
                        {
                            sNewKeyName = Translate("KeyTypeSymmetric") + " " + iNameCounter.ToString();
                            iNameCounter++;
                        }
                        NewKey = new CryptoKey(sNewKeyName, CryptoKey.nKeyFormat.KeePass, CryptoKey.nKeyType.Symmetric, _iSelectedNewKeyBytes);
                    }
                    else
                    {
                        sNewKeyName = Translate("KeyTypeAsymmetricPrivate");
                        while (_ltKeys.SingleOrDefault(key => key.sName == sNewKeyName) != null)
                        {
                            sNewKeyName = Translate("KeyTypeAsymmetricPrivate") + " " + iNameCounter.ToString();
                            iNameCounter++;
                        }
                        NewKey = new CryptoKey(sNewKeyName, CryptoKey.nKeyFormat.Private, CryptoKey.nKeyType.AsymmetricPrivate, _iSelectedNewKeyBytes);
                    }

                    _BackgroundThread.Enqueue(new BackgroundMessage(BackgroundMessage.nType.FillKey, NewKey));
                    if (_BackgroundThread.Start())
                    {
                        _eBackgroundTask = BackgroundMessage.nType.FillKey;
                        sBackgroundStatus = Translate("CreatingNewKey");
                        isProgressBarIndeterminate = true;
                        _UserInterfaceTimer.Start();
                    }
                }
                else if (_eBackgroundTask == BackgroundMessage.nType.FillKey)   // the user clicked cancel
                {
                    if (_UserInterfaceTimer.IsEnabled)
                        _UserInterfaceTimer.Stop();
                    _eBackgroundTask = BackgroundMessage.nType.Stop;           // this is messy but I could not figure out something better:
                    _BackgroundThread = new BackgroundThread(_Cryptography);   // the new RSACryptoServiceProvider() does not react to a ThreadAbortException
                    isProgressBarIndeterminate = false;                        // so we let it run and continue with a new BackgroundThread() instead
                    sBackgroundStatus = string.Empty;
                }
                NotifyPropertyChanged("sCreateOrCancel");
            }
        }

        /// <summary>Delegate method invoked by dcReadDrivesAndKeys.</summary>
        private void ExecuteReadDrivesAndKeys()
        {
            ReadDrivesAndKeys();
            RequeryDisplayedDrives();
            RequeryDisplayedKeys();
        }

        /// <summary>Delegate method invoked by dcSaveKey.</summary>
        private void ExecuteSaveKey()
        {
            if (isExecuteSaveKey)
            {
                CryptoKey KeyToSave = _SelectedKey;
                KeyToSave.Save(_SelectedKeyDrive);
                SelectedKey = null;
                RequeryDisplayedKeys();
                SelectedKey = KeyToSave;
            }
        }

        /// <summary></summary>
        protected bool CanExecuteSaveKey()
        {
            return isExecuteSaveKey;
        }

        /// <summary>Delegate method invoked by dcSaveTask.</summary>
        private void ExecuteSaveTask()
        {

        }

        /// <summary>Delegate method invoked by dcSelectDestination.</summary>
        private void ExecuteSelectDestination()
        {
            if (isDestinationAFile)
                ExecuteOpenFileDialog(false, _DestinationDrive.eEncryptionType == Drive.nEncryptionType.FileAsymmetric ? Drive.csAsymmetricFileExtension : Drive.csSymmetricFileExtension);
            else
                ExecuteFolderBrowserDialog(false);
        }

        /// <summary>Delegate method invoked by dcSelectSource.</summary>
        private void ExecuteSelectSource()
        {
            if (isSourceAFile)
                ExecuteOpenFileDialog(true, _SourceDrive.eEncryptionType == Drive.nEncryptionType.FileAsymmetric ? Drive.csAsymmetricFileExtension : Drive.csSymmetricFileExtension);
            else
                ExecuteFolderBrowserDialog(true);
        }

        /// <summary></summary>
        private void ExecuteFolderBrowserDialog(bool isSourceFolder)
        {
            using (System.Windows.Forms.FolderBrowserDialog FolderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = isSourceFolder ? sSelectSourceDirectory : sSelectDestinationDirectory,
                SelectedPath = isSourceFolder ? sSourceFileOrDirectory : sDestinationFileOrDirectory,
                ShowNewFolderButton = !isSourceFolder
            })
            {
                if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (isSourceFolder)
                        sSourceFileOrDirectory = FolderDialog.SelectedPath;
                    else
                        sDestinationFileOrDirectory = FolderDialog.SelectedPath;
                }
            }
        }

        /// <summary></summary>
        private void ExecuteOpenFileDialog(bool isSourceFile, string sDefaultExtension)
        {
            using (System.Windows.Forms.OpenFileDialog FileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Title = isSourceFile ? sSelectSourceFile : sSelectDestinationFile,
                InitialDirectory = isSourceFile ? sSourceFileOrDirectory : sDestinationFileOrDirectory,
                DefaultExt = sDefaultExtension,
                FilterIndex = 1,
                Filter = Translate(isSourceFile ? "SourceFile" : "DestinationFile") + " (*" + Drive.csAsymmetricFileExtension + ";*" + Drive.csSymmetricFileExtension + ")|*" + Drive.csAsymmetricFileExtension + ";*" + Drive.csSymmetricFileExtension,
                CheckFileExists = isSourceFile,
                Multiselect = false
            })
            {
                if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (isSourceFile)
                        sSourceFileOrDirectory = FileDialog.FileName;
                    else
                        sDestinationFileOrDirectory = FileDialog.FileName;
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

                string sSwapFileOrDirectory = _sSourceFileOrDirectory;
                _sSourceFileOrDirectory = _sDestinationFileOrDirectory;
                _sDestinationFileOrDirectory = sSwapFileOrDirectory;

                _SourceDrive.isSource = true;
                _DestinationDrive.isSource = false;

                foreach (PairOfFiles SwapPair in _ltPairs)
                    SwapPair.SwapSourceAndDestination();

                RequeryDisplayedPairs(true);
                NotifyPropertyChanged("iRowHeightKeys");
                NotifyPropertyChanged("blEncryptionKeys");
                NotifyPropertyChanged("SelectedBlockSize");
                NotifyPropertyChanged("blAuthenticationKeys");
                NotifyPropertyChanged("SelectedSourceOption");
                NotifyPropertyChanged("isEditKeys");
                NotifyPropertyChanged("sCapacityInformation");
                NotifyPropertyChanged("VisibleWhenEncrypted");
                NotifyPropertyChanged("SelectedEncryptionKey");
                NotifyPropertyChanged("sSourceFileOrDirectory");
                NotifyPropertyChanged("SelectedFileSystemLevel");
                NotifyPropertyChanged("SelectedAuthenticationKey");
                NotifyPropertyChanged("SelectedDestinationOption");
                NotifyPropertyChanged("sDestinationFileOrDirectory");
                NotifyPropertyChanged("iRowHeightEncryptedDirectory");
            }
        }

        /// <summary>Delegate method invoked by dcSynchronizeCancelOrRecompare.</summary>
        private void ExecuteSynchronizeCancelOrRecompare()
        {
            int iNewProgressBarMaximum = 0;
            long kTotalBytesToEncrypt = 0;
            IEnumerable<PairOfFiles> qySynchronizeFirst, qyDeleteLast;
            Queue<BackgroundMessage> quEncrypt;

            if (isExecuteSynchronizeCancelOrRecompare)
            {
                if (_eBackgroundTask == BackgroundMessage.nType.Stop)   // if there are no background tasks running, start synchronization
                {
                    if (isExecuteSynchronize)
                    {
                        if ((_eSynchronizationMode == PairOfFiles.nSynchronizationMode.WithDelete) && isDestinationADirectory)
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

                        if ((isSourceADirectory) && (isDestinationADirectory))
                        {
                            foreach (PairOfFiles UnsynchronizedPair in qySynchronizeFirst)
                            {
                                UnsynchronizedPair.eSynchronizationMode = _eSynchronizationMode;
                                iNewProgressBarMaximum += UnsynchronizedPair.iMaximumProgress;
                                _BackgroundThread.Enqueue(new BackgroundMessage(BackgroundMessage.nType.Synchronize, UnsynchronizedPair));
                            }
                        }
                        else if (isSourceAFile)
                        {
                            _BackgroundThread.Enqueue(new BackgroundMessage(BackgroundMessage.nType.DecryptFile, new PairOfFiles(_SourceDrive, _DestinationDrive)));
                            // TODO set up progress bar
                            // TODO PairOfFiles.nSynchronizationMode.WithDelete
                        }
                        else
                        {
                            quEncrypt = new Queue<BackgroundMessage>();

                            foreach (PairOfFiles UnsynchronizedPair in qySynchronizeFirst)
                                kTotalBytesToEncrypt += UnsynchronizedPair.kBytesToEncryptSource;

                            _DestinationDrive.aAsymmetricEncryptionKeys = new CryptoKey[2];
                            _DestinationDrive.aAsymmetricEncryptionKeys[0] = _DestinationDrive.SelectedEncryptionKey;
                            _DestinationDrive.aAsymmetricEncryptionKeys[1] = _DestinationDrive.SelectedAuthenticationKey;

                            _BackgroundThread.Enqueue(new BackgroundMessage(BackgroundMessage.nType.StartEncryption, _DestinationDrive, kTotalBytesToEncrypt > int.MaxValue ? int.MaxValue : (int)kTotalBytesToEncrypt));
                            _BackgroundThread.Enqueue(new BackgroundMessage(BackgroundMessage.nType.EncryptionIndexCount, _DestinationDrive, qySynchronizeFirst.Count()));

                            foreach (PairOfFiles Pair in qySynchronizeFirst)
                            {
                                Pair.eSynchronizationMode = _eSynchronizationMode;
                                iNewProgressBarMaximum += Pair.iMaximumProgress;
                                _BackgroundThread.Enqueue(new BackgroundMessage(BackgroundMessage.nType.EncryptAttributes, Pair));
                                quEncrypt.Enqueue(new BackgroundMessage(BackgroundMessage.nType.EncryptFile, Pair));
                            }

                            while (quEncrypt.Count > 0)
                                _BackgroundThread.Enqueue(quEncrypt.Dequeue());

                            _BackgroundThread.Enqueue(new BackgroundMessage(BackgroundMessage.nType.FinishEncryption, new PairOfFiles(null, _DestinationDrive)));
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
        private void FocusKeyValue()
        {
            if (!isKeyValueReadOnly)
            {
                isKeyValueFocused = false;
                isKeyValueFocused = true;
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
        private BitmapSource LoadBitmap(string sFileName)
        {
            // string sPathToAsset = SearchForAsset(sFileName);
            // 
            // if (!string.IsNullOrEmpty(sPathToAsset))
            //     return new BitmapSource(sPathToAsset);
            // else
            return null;
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
        private void NotifyPropertyChangedEncryptionType()
        {
            NotifyPropertyChanged("isEditKeys");
            NotifyPropertyChanged("sEncryptionKey");
            NotifyPropertyChanged("iRowHeightKeys");
            NotifyPropertyChanged("blEncryptionKeys");
            NotifyPropertyChanged("iRowHeightEncrypted");
            NotifyPropertyChanged("sCapacityInformation");
            NotifyPropertyChanged("VisibleWhenEncrypted");
            NotifyPropertyChanged("blAuthenticationKeys");
            NotifyPropertyChanged("SelectedDestinationOption");
            NotifyPropertyChanged("sDestinationFileOrDirectory");
            NotifyPropertyChanged("iRowHeightEncryptedDirectory");
            NotifyPropertyChanged("VisibleWhenDestinationIsDirectory");
            NotifyPropertyChanged("VisibleWhenDestinationDirectoryIsSymmetric");
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
            _ltKeys.Clear();
            foreach (DriveInfo Info in _aDriveInfo)
            {
                NewDrive = new Drive(Info);
                _ltDrives.Add(NewDrive);

                if ((NewDrive.ltKeysStored != null) && (NewDrive.ltKeysStored.Count > 0))
                {
                    foreach (CryptoKey Key in NewDrive.ltKeysStored)
                        AddKey(Key);
                }
            }
        }

        /// <summary></summary>
        private void RemoveKey(CryptoKey Key)
        {
            if (_ltKeys.Contains(Key))
                _ltKeys.Remove(Key);
        }

        private string ReplaceExtension(string sFilePath, string sOldExtension, string sNewExtension)
        {
            if (string.IsNullOrEmpty(sFilePath) || (sFilePath.Length < sOldExtension.Length) || (sFilePath.Length < sNewExtension.Length) || (sFilePath.Substring(sFilePath.Length - sOldExtension.Length) != sOldExtension))
                return sFilePath;
            else
                return sFilePath.Substring(0, sFilePath.Length - sOldExtension.Length) + sNewExtension;
        }

        /// <summary></summary>
        private void RequeryDisplayedDrives()
        {
            Drive DummyDrive = new Drive(Translate("DummyDriveName"));
            IEnumerable<Drive> qyDrives;

            _blDrives.Clear();
            _blDrives.Add(DummyDrive);

            qyDrives = from d in _ltDrives where d.isReady orderby d.sName select d;
            foreach (Drive NewDrive in qyDrives)
                _blDrives.Add(NewDrive);

            if (_blDrives.Count == 0)
                SelectedDrive = null;
            else
                SelectedDrive = _blDrives[0];
        }


        /// <summary></summary>
        private void RequeryDisplayedKeys()
        {
            IEnumerable<CryptoKey> qyKeys;

            _blKeys.Clear();
            _blAsymmetricKeys.Clear();
            _blPrivateKeys.Clear();
            _blSymmetricKeys.Clear();
            qyKeys = from k in _ltKeys where k.eType != CryptoKey.nKeyType.Invalid orderby k.sName, k.sDrive select k;
            foreach (CryptoKey NewKey in qyKeys)
            {
                _blKeys.Add(NewKey);

                switch(NewKey.eType)
                {
                    case CryptoKey.nKeyType.AsymmetricPrivate:  if (_blAsymmetricKeys.SingleOrDefault(key => (key.sName == NewKey.sName)) == null)
                                                                    _blAsymmetricKeys.Add(NewKey);
                                                                if (_blPrivateKeys.SingleOrDefault(key => (key.sName == NewKey.sName)) == null)
                                                                    _blPrivateKeys.Add(NewKey);
                                                                break;
                    case CryptoKey.nKeyType.AsymmetricPublic:   if (_blAsymmetricKeys.SingleOrDefault(key => (key.sName == NewKey.sName)) == null)
                                                                    _blAsymmetricKeys.Add(NewKey);
                                                                break;
                    case CryptoKey.nKeyType.Symmetric:          if (_blSymmetricKeys.SingleOrDefault(key => (key.sName == NewKey.sName)) == null)
                                                                    _blSymmetricKeys.Add(NewKey);
                                                                break;
                }
            }
            SelectedKey = null;
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

        /// <summary></summary>
        /// <param name=""></param>
        private string SearchForAsset(string sFileName)
        {
            const string csAssestsDirectory = "/Assets/";

            string sPathToAsset;

            if (File.Exists(_sApplicationDirectory + csAssestsDirectory + sFileName))
                sPathToAsset = _sApplicationDirectory + csAssestsDirectory + sFileName;
            else if (File.Exists(_sApplicationDirectory + "/" + sFileName))
                sPathToAsset = _sApplicationDirectory + "/" + sFileName;
            else
                sPathToAsset = string.Empty;

            return sPathToAsset;
        }

        /// <summary></summary>
        private void SetSelectedKeyNumeral()
        {
            if (_SelectedKey == null || _SelectedKey.eType == CryptoKey.nKeyType.Invalid)
            {
                blKeyNumerals = null;
                _iSelectedKeyNumeral = 0;
            }
            else if (_SelectedKey.eType == CryptoKey.nKeyType.Symmetric)
            {
                blKeyNumerals = _blKeyNumeralsPrivate;
                _iSelectedKeyNumeral = _iKeyNumeralIfCanHide;
            }
            else if ((_eSelectedKeyParameterAsymmetric == CryptoKey.nKeyParameter.Email) || (_eSelectedKeyParameterAsymmetric == CryptoKey.nKeyParameter.Homepage) || (_eSelectedKeyParameterAsymmetric == CryptoKey.nKeyParameter.Owner))
            {
                blKeyNumerals = _blKeyTextPublic;
                _iSelectedKeyNumeral = 1;
            }
            else if ((_eSelectedKeyParameterAsymmetric == CryptoKey.nKeyParameter.Exponent) || (_eSelectedKeyParameterAsymmetric == CryptoKey.nKeyParameter.Modulus))
            {
                blKeyNumerals = _blKeyNumeralsPublic;
                if (_iSelectedKeyNumeral < 2)
                    _iSelectedKeyNumeral = 16;
            }
            else
            {
                blKeyNumerals = _blKeyNumeralsPrivate;
                _iSelectedKeyNumeral = _iKeyNumeralIfCanHide;
            }
            NotifyPropertyChanged("SelectedKeyNumeral");
        }

        /// <summary>Timer event handler that updates the user interface in regular intervals.</summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void UserInterfaceTimerTick(object sender, EventArgs e)
        {
            bool isListOfPairsChanged = false;
            bool isProgressChanged = false;

            while (!_BackgroundThread.ReturnQueue.IsEmpty)
            {
                _BackgroundThread.ReturnQueue.TryDequeue(out BackgroundMessage BackgroundMessage);

                if (BackgroundMessage != null)
                {
                    switch (BackgroundMessage.eType)
                    {
                        case BackgroundMessage.nType.DecryptIndex:
                            if (BackgroundMessage.PairProperty != null)
                            {
                                if (BackgroundMessage.PairProperty.DestinationDrive != null)
                                {
                                    NotifyPropertyChanged("SelectedAuthenticationKey");
                                    NotifyPropertyChanged("SelectedEncryptionKey");
                                }
                            }
                            break;

                        case BackgroundMessage.nType.FillKey:
                            if (BackgroundMessage.KeyProperty != null)
                            {
                                AddKey(BackgroundMessage.KeyProperty);
                                RequeryDisplayedKeys();
                                SelectedKey = BackgroundMessage.KeyProperty;
                            }
                            break;

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
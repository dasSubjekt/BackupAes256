namespace BackupAes256.ViewModel
{
    using System;
    using System.Windows;
    using BackupAes256.Model;
    using System.Windows.Data;
    using System.Globalization;


    /// <summary></summary>
    public class ValueTranslator : IValueConverter
    {
        private ViewModelBase _ViewModelBase;


        /// <summary></summary>
        public ValueTranslator()
        {
            _ViewModelBase = (ViewModelBase)Application.Current.MainWindow.DataContext;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string sReturn = string.Empty;

            if (targetType != typeof(string))
            {
                throw new ArgumentException("ValueTranslator can only convert into type string.");
            }
            else if (value is CryptoKey.nKeyFormat)
            {
                switch (value)
                {
                    case CryptoKey.nKeyFormat.BitLocker: sReturn = "BitLocker"; break;
                    case CryptoKey.nKeyFormat.KeePass: sReturn = "KeePass"; break;
                    case CryptoKey.nKeyFormat.Password: sReturn = _ViewModelBase.Translate("KeyFormatPassword"); break;
                    case CryptoKey.nKeyFormat.Private: sReturn = _ViewModelBase.Translate("KeyFormatPrivate"); break;
                    case CryptoKey.nKeyFormat.Public: sReturn = _ViewModelBase.Translate("KeyFormatPublic"); break;
                }
            }
            else if (value is CryptoKey.nKeyType)
            {
                switch (value)
                {
                    case CryptoKey.nKeyType.Invalid: sReturn = _ViewModelBase.Translate("KeyTypeInvalid"); break;
                    case CryptoKey.nKeyType.AsymmetricPrivate: sReturn = _ViewModelBase.Translate("KeyTypeAsymmetricPrivate"); break;
                    case CryptoKey.nKeyType.AsymmetricPublic: sReturn = _ViewModelBase.Translate("KeyTypeAsymmetricPublic"); break;
                    case CryptoKey.nKeyType.Symmetric: sReturn = _ViewModelBase.Translate("KeyTypeSymmetric"); break;
                }
            }
            else
            {
                throw new ArgumentException("ValueTranslator cannot convert from type " + value.GetType().ToString() + ".");
            }
            return sReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

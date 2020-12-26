namespace BackupAes256.ViewModel
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;


    public class DragDropTextBox : TextBox
    {

        #region constructors

        public DragDropTextBox() : base()
        {
            AllowDrop = true;
        }

        #endregion

        #region properties

        #endregion

        #region methods

        private DragDropEffects VerifyDrop(DragEventArgs Arguments)
        {
            string[] asFileNames;
            DragDropEffects Return = DragDropEffects.None;

            if (Arguments.Data.GetDataPresent(DataFormats.FileDrop))
            {
                asFileNames = (string[])Arguments.Data.GetData(DataFormats.FileDrop);
                if ((asFileNames != null) && (asFileNames.Length == 1))
                {
                    if (Directory.Exists(asFileNames[0]))
                        Return = DragDropEffects.Copy;
                }
            }
            return Return;
        }

        protected override void OnDragEnter(DragEventArgs Arguments)
        {
            Arguments.Effects = VerifyDrop(Arguments);
            Arguments.Handled = true;
        }

        protected override void OnDragOver(DragEventArgs Arguments)
        {
            Arguments.Effects = VerifyDrop(Arguments);
            Arguments.Handled = true;
        }

        protected override void OnDrop(DragEventArgs Arguments)
        {
            string[] asFileNames = null;

            if (VerifyDrop(Arguments) == DragDropEffects.Copy)
            {
                asFileNames = (string[])Arguments.Data.GetData(DataFormats.FileDrop);
                Text = asFileNames[0];
            }
            Arguments.Handled = true;
        }
        #endregion
    }
}

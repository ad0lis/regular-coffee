namespace Regular_Coffee
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Windows;
    using System.Xml;

    [Serializable]
    public class Settings
    {
        private double _width;
        private double _height;
        private double _top;
        private double _left;
        private bool _isMaximized;
        private string _regexInput;
        private string _textInput;
        private int _regexOptions;
        private bool _highlightEnabled;
        private bool _wrappingEnabled;
        private double _leftColumnWidth;
        private double _rightColumnWidth;
        private double _topRowHeight;
        private double _bottomRowHeight;

        internal Settings()
        {
        }

        internal Settings(MainWindow window)
        {
            this._width = window.Width;
            this._height = window.Height;
            this._top = window.Top;
            this._left = window.Left;
            this._isMaximized = window.WindowState == WindowState.Maximized;
            this._regexInput = MainWindow.RegexUserInput.Text.Compress() ?? string.Empty;
            this._textInput = MainWindow.TextUserInput.Text.Compress() ?? string.Empty;
            this._regexOptions = (int)HelperMethods.MainRegexOptions;
            this._highlightEnabled = MainWindow.MainViewModel.HighlightEnabled;
            this._wrappingEnabled = MainWindow.MainViewModel.WrappingEnabled;
            this._leftColumnWidth = MainWindow.MainViewModel.LeftColumnWidth.Value;
            this._rightColumnWidth = MainWindow.MainViewModel.RightColumnWidth.Value;
            this._topRowHeight = MainWindow.MainViewModel.TopRowHeight.Value;
            this._bottomRowHeight = MainWindow.MainViewModel.BottomRowHeight.Value;
    }

        public double Width => this._width;

        public double Height => this._height;

        public double Top => this._top;

        public double Left => this._left;

        public bool IsMaximized => this._isMaximized;

        public string RegexInput => this._regexInput;

        public string TextInput => this._textInput;

        public int RegexOptions => this._regexOptions;

        public bool HighlightEnabled => this._highlightEnabled;

        public bool WrappingEnabled => this._wrappingEnabled;

        public double LeftColumnWidth => this._leftColumnWidth;

        public double RightColumnWidth => this._rightColumnWidth;

        public double TopRowHeight => this._topRowHeight;

        public double BottomRowHeight => this._bottomRowHeight;

        internal static void Load(MainWindow window)
        {
            if (!TryLoadSettings(out Settings settings))
            {
                return;
            }

            settings.FitToScreen();
            settings.MoveToScreen();

            window.Width = settings.Width;
            window.Height = settings.Height;
            window.Top = settings.Top;
            window.Left = settings.Left;

            MainWindow.RegexUserInput.Text = settings.RegexInput.Decompress();
            MainWindow.TextUserInput.Text = settings.TextInput.Decompress();
            HelperMethods.MainRegexOptions = (System.Text.RegularExpressions.RegexOptions)settings.RegexOptions;

            if (settings._isMaximized)
            {
                window.WindowState = WindowState.Maximized;
            }

            MainWindow.MainViewModel.HighlightEnabled = settings.HighlightEnabled;
            MainWindow.MainViewModel.WrappingEnabled = settings.WrappingEnabled;
            MainWindow.MainViewModel.LeftColumnWidth = new GridLength(settings.LeftColumnWidth, GridUnitType.Star);
            MainWindow.MainViewModel.RightColumnWidth = new GridLength(settings.RightColumnWidth, GridUnitType.Star);
            MainWindow.MainViewModel.TopRowHeight = new GridLength(settings.TopRowHeight, GridUnitType.Star);
            MainWindow.MainViewModel.BottomRowHeight = new GridLength(settings.BottomRowHeight, GridUnitType.Star);
        }

        internal void Save()
        {
            using (Stream stream = File.Open("Settings.ini", FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, this);
            }
        }

        internal void FitToScreen()
        {
            if (this._height > SystemParameters.VirtualScreenHeight)
            {
                this._height = SystemParameters.VirtualScreenHeight;
            }

            if (this._width > SystemParameters.VirtualScreenWidth)
            {
                this._width = SystemParameters.VirtualScreenWidth;
            }
        }

        internal void MoveToScreen()
        {
            if (this._top + (this._height / 2) > SystemParameters.VirtualScreenHeight)
            {
                this._top = SystemParameters.VirtualScreenHeight - this._height;
            }

            if (this._left + (this._width / 2) > SystemParameters.VirtualScreenWidth)
            {
                this._left = SystemParameters.VirtualScreenWidth - this._width;
            }

            if (this._top < 0)
            {
                this._top = 0;
            }

            if (this._left < 0)
            {
                this._left = 0;
            }
        }

        private static bool TryLoadSettings(out Settings settings)
        {
            if (!File.Exists("Settings.ini"))
            {
                settings = new Settings();
                return false;
            }

            try
            {
                using (Stream stream = File.Open("Settings.ini", FileMode.Open))
                {
                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                    settings = (Settings)binaryFormatter.Deserialize(stream);
                    return true;
                }
            }
            catch
            {
                settings = new Settings();
                return false;
            }
        }
    }
}

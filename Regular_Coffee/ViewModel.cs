namespace Regular_Coffee
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    public class ViewModel : INotifyPropertyChanged
    {
        private string _errorText = string.Empty;
        private string _diagnosticsText = string.Empty;
        private string _matchesShowingText = string.Empty;
        private ObservableCollection<IMainMatch> _mainMatches = new ObservableCollection<IMainMatch>();
        private Visibility _bottomButtonsVisibility = Visibility.Hidden;
        private Visibility _exportRectangleVisibility = Visibility.Hidden;
        private int _matchesLeftUnshown = 1337;
        private bool _matchButtonsEnabled = true;
        private bool _highlightEnabled = true;
        private bool _wrappingEnabled = true;
        private GridLength _leftColumnWidth = new GridLength(1, GridUnitType.Star);
        private GridLength _rightColumnWidth = new GridLength(1, GridUnitType.Star);
        private GridLength _topRowHeight = new GridLength(1, GridUnitType.Star);
        private GridLength _bottomRowHeight = new GridLength(1, GridUnitType.Star);

        public event PropertyChangedEventHandler PropertyChanged;

        public GridLength BottomRowHeight
        {
            get
            {
                return this._bottomRowHeight;
            }

            set
            {
                this._bottomRowHeight = value;
                this.NotifyPropertyChanged("BottomRowHeight");
            }
        }

        public GridLength TopRowHeight
        {
            get
            {
                return this._topRowHeight;
            }

            set
            {
                this._topRowHeight = value;
                this.NotifyPropertyChanged("TopRowHeight");
            }
        }

        public GridLength RightColumnWidth
        {
            get
            {
                return this._rightColumnWidth;
            }

            set
            {
                this._rightColumnWidth = value;
                this.NotifyPropertyChanged("RightColumnWidth");
            }
        }

        public GridLength LeftColumnWidth
        {
            get
            {
                return this._leftColumnWidth;
            }

            set
            {
                this._leftColumnWidth = value;
                this.NotifyPropertyChanged("LeftColumnWidth");
            }
        }

        public Visibility ExportRectangleVisibility
        {
            get
            {
                return this._exportRectangleVisibility;
            }

            set
            {
                this._exportRectangleVisibility = value;
                this.NotifyPropertyChanged("ExportRectangleVisibility");
            }
        }

        public bool HighlightEnabled
        {
            get
            {
                return this._highlightEnabled;
            }

            set
            {
                this._highlightEnabled = value;
                this.NotifyPropertyChanged("HighlightEnabled");
            }
        }

        public bool WrappingEnabled
        {
            get
            {
                return this._wrappingEnabled;
            }

            set
            {
                this._wrappingEnabled = value;
                this.NotifyPropertyChanged("WrappingEnabled");
            }
        }

        public bool MatchButtonsEnabled
        {
            get
            {
                return this._matchButtonsEnabled;
            }

            set
            {
                this._matchButtonsEnabled = value;
                this.NotifyPropertyChanged("MatchButtonsEnabled");
            }
        }

        public int MatchesLeftUnshown
        {
            get
            {
                return this._matchesLeftUnshown;
            }

            set
            {
                this._matchesLeftUnshown = value;
                this.NotifyPropertyChanged("MatchesLeftUnshown");
            }
        }

        public int OutsetOfTreeView
        {
            get
            {
                return HelperMethods.OutsetOfTreeView;
            }

            set
            {
                HelperMethods.OutsetOfTreeView = value;
                this.NotifyPropertyChanged("OutsetOfTreeView");
            }
        }

        public Visibility BottomButtonsVisibility
        {
            get
            {
                return this._bottomButtonsVisibility;
            }

            set
            {
                this._bottomButtonsVisibility = value;
                this.NotifyPropertyChanged("BottomButtonsVisibility");
            }
        }

        public string MatchesShowingText
        {
            get
            {
                return this._matchesShowingText;
            }

            set
            {
                this._matchesShowingText = value;
                this.NotifyPropertyChanged("MatchesShowingText");
            }
        }

        public string ErrorText
        {
            get
            {
                return this._errorText;
            }

            set
            {
                this._errorText = value;
                this.NotifyPropertyChanged("ErrorText");
            }
        }

        public string DiagnosticsText
        {
            get
            {
                return this._diagnosticsText;
            }

            set
            {
                this._diagnosticsText = value;
                this.NotifyPropertyChanged("DiagnosticsText");
            }
        }

        public ObservableCollection<IMainMatch> MainMatches
        {
            get
            {
                return this._mainMatches;
            }

            set
            {
                this._mainMatches = value;
                this.NotifyPropertyChanged("MainMatches");
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class IsGreaterThanZero : MarkupExtension, IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (int)value > 0;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return null;
            }

            public override object ProvideValue(IServiceProvider serviceProvider)
            {
                return this;
            }
        }
    }
}

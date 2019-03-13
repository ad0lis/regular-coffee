namespace Regular_Coffee
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using ScintillaNET;
    using ScintillaNET_FindReplaceDialog;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static double _treeViewHeight;
        private static Scintilla _regexScintilla = new Scintilla();
        private static Scintilla _textScintilla = new Scintilla();
        private static ScintillaMethods _regexSciMethods;
        private static ScintillaMethods _textSciMethods;
        private static FindReplace _textFindReplace;
        private static ViewModel _mainViewModel = new ViewModel();
        private static int _prefHeight = 130;
        private bool _restoreBeforeMoving = false;

        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = _mainViewModel;
            this.SetRegexPreferencesCheckboxes();
            _textFindReplace = new FindReplace(_textScintilla);
            _textFindReplace.KeyPressed += this.TextFindReplace_KeyPressed;

            Settings.Load(this);
        }

        public static Scintilla RegexUserInput => _regexScintilla;

        public static Scintilla TextUserInput => _textScintilla;

        public static ScintillaMethods RegexScintillaMethods => _regexSciMethods;

        public static ScintillaMethods TextUserInputScintillaMethods => _textSciMethods;

        public static ViewModel MainViewModel => _mainViewModel;

        public static double TreeViewHeight
        {
            get
            {
                return _treeViewHeight;
            }

            set
            {
                _treeViewHeight = value;
            }
        }

        private void InputText_Loaded(object sender, RoutedEventArgs e)
        {
            ScintillaMethods.SetupBasicScintilla(ref _textScintilla, this.inputRegex);

            inputText.Child = _textScintilla;
            _textSciMethods = new ScintillaMethods(_textScintilla);

            _textScintilla.KeyDown += this.GenericScintilla_KeyDown;
            _textScintilla.GotFocus += this.Scintilla_GotFocus;
            _textScintilla.LostFocus += this.Scintilla_LostFocus;
        }

        private void InputRegex_Loaded(object sender, RoutedEventArgs e)
        {
            ScintillaMethods.SetupBasicScintilla(ref _regexScintilla, this.inputRegex);
            
            inputRegex.Child = _regexScintilla;
            _regexSciMethods = new ScintillaMethods(_regexScintilla);
            _regexSciMethods.ConfigureBraceMatching(_regexScintilla);

            _regexScintilla.KeyDown += this.GenericScintilla_KeyDown;
            _regexScintilla.GotFocus += this.Scintilla_GotFocus;
            _regexScintilla.LostFocus += this.Scintilla_LostFocus;
        }

        private void SplitterDragStarted_Handler(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            inputText.Visibility = Visibility.Hidden;
            inputRegex.Visibility = Visibility.Hidden;
        }

        private void SplitterDragEnded_Handler(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            inputText.Visibility = Visibility.Visible;
            inputRegex.Visibility = Visibility.Visible;
        }

        private async void FullMatch_ButtonClick(object sender, RoutedEventArgs e)
        {
            await HelperMethods.MainMatchMethod(RegexUserInput.Text);
        }

        private async void PartialMatch_ButtonClick(object sender, RoutedEventArgs e)
        {
            await HelperMethods.MainMatchMethod(RegexUserInput.SelectedText);
        }

        private void CancelRegex_ButtonClick(object sender, RoutedEventArgs e)
        {
            HelperMethods.RegexCancelTokenSource.Cancel();
        }

        private void TreeViewSizeChanged_Handler(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                TreeViewHeight = matchesTreeView.RenderSize.Height - 1;
                HelperMethods.HandleTreeViewOnResize();
            }
        }

        private void PreviousMatches_Button_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.OutsetOfTreeView -= HelperMethods.CountToPutToTreeView;
            if (MainViewModel.OutsetOfTreeView < 0)
            {
                MainViewModel.OutsetOfTreeView = 0;
            }

            MainViewModel.MatchesLeftUnshown = HelperMethods.MatchesCount - MainViewModel.OutsetOfTreeView - HelperMethods.CountToPutToTreeView;
            HelperMethods.UpdateTreeView();
        }

        private void NextMatches_Button_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.OutsetOfTreeView += HelperMethods.CountToPutToTreeView;
            if (MainViewModel.OutsetOfTreeView > HelperMethods.MatchesCount - HelperMethods.CountToPutToTreeView)
            {
                MainViewModel.OutsetOfTreeView = HelperMethods.MatchesCount - HelperMethods.CountToPutToTreeView;
            }

            MainViewModel.MatchesLeftUnshown = HelperMethods.MatchesCount - MainViewModel.OutsetOfTreeView - HelperMethods.CountToPutToTreeView;

            HelperMethods.UpdateTreeView();
        }

        private void MatchesTreeView_SelectedItemChanged_Handler(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            IHighlightableMatch selectedItem = (IHighlightableMatch)((System.Windows.Controls.TreeView)sender).SelectedItem;

            if (selectedItem != null)
            {
                _textScintilla.FirstVisibleLine = _textScintilla.Lines[_textScintilla.LineFromPosition(selectedItem.Index)].DisplayIndex;

                _textScintilla.ScrollRange(selectedItem.Index, selectedItem.Index + selectedItem.Length);
                _textScintilla.SelectionStart = selectedItem.Index;
                _textScintilla.SelectionEnd = selectedItem.Index + selectedItem.Length;

                if (MainViewModel.HighlightEnabled)
                {
                    _textSciMethods.ClearHighlights();
                    _textSciMethods.Highlight(10, selectedItem.Index, selectedItem.Length);
                    if (selectedItem is MainMatch)
                    {
                        MainMatch mainMatch = (MainMatch)selectedItem;
                        bool evenNumber = true;
                        foreach (IHighlightableMatch groupMatch in mainMatch.GroupMembers)
                        {
                            _textSciMethods.HighlightGroups(evenNumber, groupMatch.Index, groupMatch.Length);
                            evenNumber = !evenNumber;
                        }
                    }
                }

                e.Handled = true;
            }
        }

        private void Window_Move(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                _restoreBeforeMoving = false;

                if (this.WindowState == WindowState.Maximized)
                {
                    this.RestoreWindow();
                }
                else
                {
                    this.MaximizeWindow();
                }
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                _restoreBeforeMoving = this.WindowState == WindowState.Maximized;
                this.DragMove();
            }
        }

        private void RestoreWindow()
        {
            mainBorder.BorderThickness = new Thickness(5, 5, 5, 5);
            _textScintilla.Margins[1].Width = 2;
            _regexScintilla.Margins[1].Width = 2;
            WindowState = WindowState.Normal;
        }

        private void MaximizeWindow()
        {
            mainBorder.BorderThickness = new Thickness(10, 10, 10, 10);
            _textScintilla.Margins[1].Width = 5;
            _regexScintilla.Margins[1].Width = 5;
            if (this.WindowState != WindowState.Maximized)
            {
                SystemCommands.MaximizeWindow(this);
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.RestoreWindow();
            }
            else
            {
                this.MaximizeWindow();
            }
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void Save_Preferences_Click(object sender, RoutedEventArgs e)
        {
            this.GetRegexPreferences(regexPrefs.Children.OfType<CheckBox>().ToList());
            this.Hide_Preferences_Click(sender, e);
        }

        private void GetRegexPreferences(List<CheckBox> checkboxList)
        {
            int power = 0;
            double regexOptions = 0;
            for (int i = 0; i < checkboxList.Count; i++, power++)
            {
                // Skip compiled and unused
                if (i == 2 || i == 7)
                {
                    power++;
                }

                if (checkboxList[i].IsChecked.Value)
                {
                    regexOptions += Math.Pow(2, power);
                }
            }

            HelperMethods.MainRegexOptions = (RegexOptions)regexOptions;
        }

        private void SetRegexPreferencesCheckboxes(RegexOptions? regexOptions = null)
        {
            List<CheckBox> checkboxList = regexPrefs.Children.OfType<CheckBox>().ToList();
            int regexOptionsInt = regexOptions == null ? (int)HelperMethods.MainRegexOptions : (int)regexOptions;
            for (int i = 0; i < checkboxList.Count; i++)
            {
                // Skip compiled and unused
                if (i == 2 || i == 7) 
                {
                    regexOptionsInt = regexOptionsInt >> 1;
                }

                checkboxList[i].IsChecked = Convert.ToBoolean(regexOptionsInt % 2);

                regexOptionsInt = regexOptionsInt >> 1;
            }
        }

        private void Hide_Preferences_Click(object sender, RoutedEventArgs e)
        {
            this.wholePrefGrid.Height = new GridLength(0);
        }

        private void RegExPreferences_Open(object sender, RoutedEventArgs e)
        {
            this.wholePrefGrid.Height = new GridLength(_prefHeight);

            if (this.Height < this.MinHeight + _prefHeight)
            {
                this.Height = this.MinHeight + _prefHeight;
            }

            this.SetRegexPreferencesCheckboxes();
        }

        private void WrapMenu_Checked(object sender, RoutedEventArgs e)
        {
            if (_regexScintilla == null || _textScintilla == null)
            {
                return;
            }

            MenuItem item = (MenuItem)sender;
            if (item.IsChecked)
            {
                _regexScintilla.WrapMode = WrapMode.Char;
                _textScintilla.WrapMode = WrapMode.Char;
            }
            else
            {
                _regexScintilla.WrapMode = WrapMode.None;
                _textScintilla.WrapMode = WrapMode.None;
            }
        }

        private void DCTClassics_Click(object sender, RoutedEventArgs e)
        {
            this.SetRegexPreferencesCheckboxes(RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
        }

        private void MainMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            if (item.IsSubmenuOpen)
            {
                item.IsSubmenuOpen = false;
            }
            else
            {
                item.IsSubmenuOpen = true;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TreeViewHeight = matchesTreeView.RenderSize.Height - 1;
        }

        // Find replace button combos
        private async void GenericScintilla_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == System.Windows.Forms.Keys.F)
            {
                _textFindReplace.ShowFind();
                e.SuppressKeyPress = true;
            }
            else if (e.Shift && e.KeyCode == System.Windows.Forms.Keys.F3)
            {
                _textFindReplace.Window.FindPrevious();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.F3)
            {
                _textFindReplace.Window.FindNext();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.H)
            {
                _textFindReplace.ShowReplace();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.I)
            {
                _textFindReplace.ShowIncrementalSearch();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.G)
            {
                GoTo myGoTo = new GoTo((Scintilla)sender);
                myGoTo.ShowGoToDialog();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.P)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                await HelperMethods.MainMatchMethod(RegexUserInput.SelectedText);
            }
            else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.M)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                await HelperMethods.MainMatchMethod(RegexUserInput.Text);
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.F5)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                await HelperMethods.MainMatchMethod(RegexUserInput.Text);
            }
        }

        private void TextFindReplace_KeyPressed(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            this.GenericScintilla_KeyDown(sender, e);
        }

        private void Scintilla_GotFocus(object sender, EventArgs e)
        {
            Scintilla scintilla = (Scintilla)sender;
            _textFindReplace.Scintilla = scintilla;

            scintilla.SetSelectionBackColor(true, ProjectColors.DarkTeal);
            scintilla.SetSelectionForeColor(true, Color.White);
        }

        private void Scintilla_LostFocus(object sender, EventArgs e)
        {
            Scintilla scintilla = (Scintilla)sender;
            scintilla.SetSelectionBackColor(true, Color.DarkGray);
            scintilla.SetSelectionForeColor(true, Color.White);
        }

        private void TreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                IHighlightableMatch selectedTVI = (IHighlightableMatch)((TreeView)sender).SelectedItem;
                if (selectedTVI != null)
                {
                    Clipboard.SetText(_textScintilla.GetTextRange(selectedTVI.Index, selectedTVI.Length));
                }

                e.Handled = true;
            }

            if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
            {
                MainViewModel.ExportRectangleVisibility = Visibility.Visible;
                exportRectangle.Focus();
            }
        }

        private void Copy_TreeViewItem(object sender, RoutedEventArgs e)
        {
            object dataContext = ((MenuItem)e.OriginalSource).DataContext;
            if (dataContext != null)
            {
                IHighlightableMatch selectedTVI = (IHighlightableMatch)dataContext;
                if (selectedTVI != null)
                {
                    Clipboard.SetText(_textScintilla.GetTextRange(selectedTVI.Index, selectedTVI.Length));
                }
            }
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = SearchForTVI(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
                e.Handled = true;
            }

            TreeViewItem SearchForTVI(DependencyObject source)
            {
                while (source != null && !(source is TreeViewItem))
                {
                    source = System.Windows.Media.VisualTreeHelper.GetParent(source);
                }

                return source as TreeViewItem;
            }
        }

        private async void MainWindow_KeyDownEvents(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.P)
                {
                    await HelperMethods.MainMatchMethod(RegexUserInput.SelectedText);
                    e.Handled = true;
                }

                if (e.Key == Key.M)
                {
                    await HelperMethods.MainMatchMethod(RegexUserInput.Text);
                    e.Handled = true;
                }

                if (e.Key == Key.C && MainViewModel.ExportRectangleVisibility == Visibility.Visible)
                {
                    matchesTreeView.Focus();
                    HelperMethods.ExportAllMatches();
                    e.Handled = true;
                }
            }

            if (e.Key == Key.F5)
            {
                await HelperMethods.MainMatchMethod(RegexUserInput.Text);
                e.Handled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings userSettings = new Settings(this);
            userSettings.Save();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            this.FinalizeWindowLoad();
        }

        private void FinalizeWindowLoad()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.MaximizeWindow();
            }

            this.SetRegexPreferencesCheckboxes();
        }

        private void TopBar_LeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _restoreBeforeMoving = false;
        }

        private void TopBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (_restoreBeforeMoving)
            {
                _restoreBeforeMoving = false;

                System.Windows.Point point = PointToScreen(e.MouseDevice.GetPosition(this));

                this.Left = point.X - (this.RestoreBounds.Width * 0.5);
                this.Top = point.Y;

                RestoreWindow();
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            }
        }

        private void ExportRectangle_LostFocus(object sender, RoutedEventArgs e)
        {
            MainViewModel.ExportRectangleVisibility = Visibility.Hidden;
        }
    }
}

namespace Regular_Coffee
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    public static class HelperMethods
    {
        private static MatchCollection _regexMatches;
        private static int _regexTimeoutSeconds = 10;
        private static int _countToPutToTreeView = 0;
        private static int _outsetOfTreeView = 0;
        private static CancellationTokenSource _regexCancelTokenSource = new CancellationTokenSource();
        private static RegexOptions _mainRegexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline;

        private static Regex whitespace = new Regex("\r\n?|\n", RegexOptions.Compiled);

        public static RegexOptions MainRegexOptions
        {
            get => _mainRegexOptions;
            set => _mainRegexOptions = value;
        }

        public static CancellationTokenSource RegexCancelTokenSource
        {
            get => _regexCancelTokenSource;
            set => _regexCancelTokenSource = value;
        }

        public static int OutsetOfTreeView
        {
            get => _outsetOfTreeView;
            set => _outsetOfTreeView = value;
        }

        public static int CountToPutToTreeView
        {
            get => _countToPutToTreeView;
            set => _countToPutToTreeView = value;
        }

        public static int MatchesCount
        {
            get => _regexMatches.Count;
        }

        public static void UpdateTreeView()
        {
            _countToPutToTreeView = Convert.ToInt32(Math.Floor(Math.Min(MainWindow.TreeViewHeight / 16, _regexMatches.Count)));

            ObservableCollection<IMainMatch> mainMatches = GetMainMatches();

            MainWindow.MainViewModel.MainMatches = mainMatches;

            if (_countToPutToTreeView >= _regexMatches.Count)
            {
                MainWindow.MainViewModel.BottomButtonsVisibility = Visibility.Hidden;
            }
            else
            {
                MainWindow.MainViewModel.MatchesShowingText = $"Showing {_outsetOfTreeView + 1}-{_outsetOfTreeView + _countToPutToTreeView} / {_regexMatches.Count} matches";
                MainWindow.MainViewModel.BottomButtonsVisibility = Visibility.Visible;
            }

            MainWindow.MainViewModel.MatchesLeftUnshown = _regexMatches.Count - _outsetOfTreeView - _countToPutToTreeView;
        }

        public static bool VerifyRegEx(string testPattern)
        {
            bool isValid = true;

            if ((testPattern != null) && (testPattern.Trim().Length > 0))
            {
                try
                {
                    Regex testRegex = new Regex(testPattern);
                    testRegex.Match(string.Empty);
                }
                catch
                {
                    isValid = false;
                }
            }
            else
            {
                isValid = false;
            }

            return isValid;
        }

        public static async Task MainMatchMethod(string userInput)
        {
            MainWindow.RegexScintillaMethods.ClearHighlights();
            MainWindow.TextUserInputScintillaMethods.ClearHighlights();
            MainWindow.MainViewModel.MainMatches = new ObservableCollection<IMainMatch>();
            MainWindow.MainViewModel.ErrorText = string.Empty;
            MainWindow.MainViewModel.DiagnosticsText = string.Empty;
            MainWindow.MainViewModel.BottomButtonsVisibility = Visibility.Hidden;

            RegexCancelTokenSource = new CancellationTokenSource();

            if (TryExtractRegex(userInput, out Regex extractedRegex))
            {
                await MatchExtractedRegex(extractedRegex);
            }
        }

        public static bool IsBrace(int c)
        {
            switch (c)
            {
                case '(':
                case ')':
                case '[':
                case ']':
                case '{':
                case '}':
                    return true;
            }

            return false;
        }

        public static bool IsOpeningBrace(int c)
        {
            switch (c)
            {
                case '(':
                case '[':
                case '{':
                    return true;
            }

            return false;
        }

        public static bool TryExtractRegex(string userInputRegex, out Regex extractedRegex)
        {
            if (!VerifyRegEx(userInputRegex))
            {
                List<int> unmatchedBraces = FindUnmatchedBraces();
                MainWindow.RegexScintillaMethods.HighlightError(unmatchedBraces);
                string errorText = "Incorrect RegEx";
                if (string.IsNullOrEmpty(userInputRegex))
                {
                    errorText = $"{errorText}, input is empty";
                }

                if (unmatchedBraces.Any())
                {
                    bool isMatchesPlural = unmatchedBraces.Count % 10 != 1 || unmatchedBraces.Count == 11;
                    errorText = $"{errorText}, {unmatchedBraces.Count} unbalanced brace{(isMatchesPlural ? "s" : string.Empty)} {(isMatchesPlural ? "were" : "was")} found";
                }

                TextMethods.WriteError(errorText);

                extractedRegex = null;
                return false;
            }

            extractedRegex = new Regex(userInputRegex, MainRegexOptions, new TimeSpan(0, 0, _regexTimeoutSeconds));
            return true;
        }

        public static async Task MatchExtractedRegex(Regex extractedRegex)
        {
            Stopwatch stopwatch = new Stopwatch();

            Task.Run(() => CheckStopwatchForButtonDisabling(stopwatch));

            try
            {
                CancellationToken regexCancelToken = _regexCancelTokenSource.Token;
                string input = MainWindow.TextUserInput.Text;
                _regexMatches = await Task.Run(() => MatchTheRegex(extractedRegex, input, regexCancelToken, stopwatch));
            }
            catch (TaskCanceledException)
            {
                TextMethods.WriteError("RegEx matching was cancelled");
                MainWindow.MainViewModel.MatchButtonsEnabled = true;
                return;
            }
            catch (RegexMatchTimeoutException)
            {
                TextMethods.WriteError($"RegEx matching was terminated after a timeout ({_regexTimeoutSeconds} s)");
                MainWindow.MainViewModel.MatchButtonsEnabled = true;
                return;
            }

            int regexCount = _regexMatches.Count;
            MainWindow.MainViewModel.MatchButtonsEnabled = true;
            TextMethods.FillDiagnostics(stopwatch.Elapsed, _regexMatches.Count);
            ResetConstants();
            UpdateTreeView();
        }

        public static void HandleTreeViewOnResize()
        {
            if (_regexMatches == null)
            {
                return;
            }

            int countToPutToTreeView = Convert.ToInt32(Math.Floor(Math.Min(MainWindow.TreeViewHeight / 16, _regexMatches.Count)));
            if (countToPutToTreeView == _countToPutToTreeView)
            {
                return;
            }

            _countToPutToTreeView = countToPutToTreeView;
            AddOrRemoveItemsFromTreeView();
        }

        public static void ExportAllMatches()
        {
            if (_regexMatches == null || _regexMatches.Count == 0)
            {
                return;
            }

            List<string> firstLineMembers = new List<string>
            {
                "Whole match"
            };

            firstLineMembers.AddRange(Enumerable.Range(0, _regexMatches[0].Groups.Count - 1).Select(i => $"Group {++i}"));

            string firstLine = string.Join("\t", firstLineMembers);

            List<string> lines = _regexMatches.Cast<Match>().Select(match => string.Join("\t", Enumerable.Range(0, match.Groups.Count).Select(i => match.Groups[i]))).ToList();
            lines.Insert(0, firstLine);
            Clipboard.SetText(string.Join("\r\n", lines));
        }

        private static List<int> FindUnmatchedBraces()
        {
            List<int> unmatchedBraces = new List<int>();
            string regexText = MainWindow.RegexUserInput.Text;
            Stack<int> type1Braces = new Stack<int>();
            Stack<int> type2Braces = new Stack<int>();
            Stack<int> type3Braces = new Stack<int>();

            for (int i = 0; i < regexText.Length; i++)
            {
                int charToCheck = regexText[i];
                if (IsBrace(regexText[i]))
                {
                    if (IsOpeningBrace(charToCheck))
                    {
                        PutToStack();
                    }
                    else
                    {
                        TryToPop();
                    }
                }

                void PutToStack()
                {
                    switch (charToCheck)
                    {
                        case '(':
                            type1Braces.Push(i);
                            break;
                        case '[':
                            type2Braces.Push(i);
                            break;
                        case '{':
                            type3Braces.Push(i);
                            break;
                    }
                }

                void TryToPop()
                {
                    switch (charToCheck)
                    {
                        case ')':
                            if (type1Braces.Count > 0)
                            {
                                type1Braces.Pop();
                            }
                            else
                            {
                                unmatchedBraces.Add(i);
                            }

                            break;
                        case ']':
                            if (type2Braces.Count > 0)
                            {
                                type2Braces.Pop();
                            }
                            else
                            {
                                unmatchedBraces.Add(i);
                            }

                            break;
                        case '}':
                            if (type3Braces.Count > 0)
                            {
                                type3Braces.Pop();
                            }
                            else
                            {
                                unmatchedBraces.Add(i);
                            }

                            break;
                    }
                }
            }

            unmatchedBraces.AddRange(type1Braces);
            unmatchedBraces.AddRange(type2Braces);
            unmatchedBraces.AddRange(type3Braces);

            return unmatchedBraces.OrderBy(x => x).ToList();
        }

        private static void CheckStopwatchForButtonDisabling(Stopwatch stopwatch)
        {
            int checks = 0;
            while (stopwatch.IsRunning || checks++ < 20)
            {
                if (stopwatch.ElapsedMilliseconds > 100)
                {
                    MainWindow.MainViewModel.MatchButtonsEnabled = false;
                    return;
                }

                Thread.Sleep(10);
            }
        }

        private static async Task<MatchCollection> MatchTheRegex(Regex extractedRegex, string input, CancellationToken cancellationToken, Stopwatch stopwatch)
        {
            TaskCompletionSource<MatchCollection> taskCompletionSource = new TaskCompletionSource<MatchCollection>();

            using (cancellationToken.Register(() => { taskCompletionSource.SetCanceled(); }))
            {
                var task = Task.Run(() =>
                {
                    MatchCollection results = extractedRegex.Matches(input);
                    int count = results.Count;
                    return results;
                });

                stopwatch.Start();
                var completedTask = await Task.WhenAny(task, taskCompletionSource.Task);
                stopwatch.Stop();

                return await completedTask;
            }
        }

        private static void AddOrRemoveItemsFromTreeView()
        {
            ObservableCollection<IMainMatch> mainMatchCollection = MainWindow.MainViewModel.MainMatches;
            if (_countToPutToTreeView > mainMatchCollection.Count)
            {
                int howManyToAdd = _countToPutToTreeView - mainMatchCollection.Count;
                int howManyToAddToFront = 0;

                for (int i = 0; i < howManyToAdd; i++)
                {
                    int elementToAddIndex = _outsetOfTreeView + mainMatchCollection.Count;
                    if (elementToAddIndex >= _regexMatches.Count)
                    {
                        howManyToAddToFront = howManyToAdd - i;
                        break;
                    }

                    mainMatchCollection.Add(GetMainMatchFromMatch(_regexMatches[elementToAddIndex]));
                }

                for (int i = 0; i < howManyToAddToFront; i++)
                {
                    int elementToAddIndex = _regexMatches.Count - mainMatchCollection.Count - 1;
                    if (elementToAddIndex < 0)
                    {
                        break;
                    }

                    mainMatchCollection.Insert(0, GetMainMatchFromMatch(_regexMatches[elementToAddIndex]));
                }
            }
            else 
            {
                int howManyToRemove = mainMatchCollection.Count - _countToPutToTreeView;
                for (int i = 0; i < howManyToRemove; i++)
                {
                    if (_outsetOfTreeView + mainMatchCollection.Count > _regexMatches.Count)
                    {
                        mainMatchCollection.RemoveAt(0);
                    }
                    else
                    {
                        mainMatchCollection.RemoveAt(mainMatchCollection.Count - 1);
                    }
                }
            }

            int countAboveAvailableMatches = _regexMatches.Count - _countToPutToTreeView - _outsetOfTreeView;
            int localOutset = _outsetOfTreeView;
            if (countAboveAvailableMatches < 0)
            {
                localOutset += countAboveAvailableMatches;
            }

            if (_countToPutToTreeView >= _regexMatches.Count)
            {
                MainWindow.MainViewModel.BottomButtonsVisibility = Visibility.Hidden;
            }
            else
            {
                MainWindow.MainViewModel.MatchesShowingText = $"Showing {localOutset + 1}-{localOutset + _countToPutToTreeView} / {_regexMatches.Count} matches";
                MainWindow.MainViewModel.BottomButtonsVisibility = Visibility.Visible;
            }

            MainWindow.MainViewModel.MatchesLeftUnshown = _regexMatches.Count - localOutset - _countToPutToTreeView;
        }

        private static IMainMatch GetMainMatchFromMatch(Match matchInQuestion)
        {
            int j = 0;

            List<IGroupMatch> groupMatches = new List<IGroupMatch>();

            matchInQuestion.Groups.Cast<Group>().Skip(1).ToList().ForEach(x => groupMatches.Add(
                new GroupMatch
                {
                    Number = ++j,
                    MatchText = whitespace.Replace(x.Value, " ").Truncate(2000),
                    Index = x.Index,
                    Length = x.Length
                }));

            return new MainMatch
                    {
                        MatchText = whitespace.Replace(matchInQuestion.Value, " ").Truncate(2000),
                        GroupMembers = groupMatches,
                        Index = matchInQuestion.Index,
                        Length = matchInQuestion.Length
                    };
        }

        private static void ResetConstants()
        {
            MainWindow.MainViewModel.MatchesLeftUnshown = 1337;
            MainWindow.MainViewModel.OutsetOfTreeView = 0;
        }

        private static ObservableCollection<IMainMatch> GetMainMatches()
        {
            ObservableCollection<IMainMatch> mainMatches = new ObservableCollection<IMainMatch>();

            for (int i = 0; i < _countToPutToTreeView; i++)
            {
                Match matchInQuestion = _regexMatches[_outsetOfTreeView + i];

                mainMatches.Add(GetMainMatchFromMatch(matchInQuestion));
            }

            return mainMatches;
        }
    }
}

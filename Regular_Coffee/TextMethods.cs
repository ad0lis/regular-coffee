namespace Regular_Coffee
{
    using System;

    public static class TextMethods
    {
        public static void FillDiagnostics(TimeSpan elapsed, int count)
        {
            string time = elapsed.TotalMilliseconds > 999 ? $"{elapsed.TotalSeconds.ToString("0.00")} s" : $"{elapsed.TotalMilliseconds.ToString("0.00")} ms";
            string diagnostics = $"Time elapsed: {time}, matches found: {count}";
            MainWindow.MainViewModel.DiagnosticsText = diagnostics;
        }

        public static void WriteError(string input)
        {
            MainWindow.MainViewModel.ErrorText = input;
        }
    }
}

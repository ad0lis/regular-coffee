namespace Regular_Coffee
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms.Integration;
    using ScintillaNET;

    public class ScintillaMethods
    {
        private Scintilla _scintilla;
        private int _lastCaretPos = 0;

        public ScintillaMethods(Scintilla scintilla)
        {
            this._scintilla = scintilla;
        }

        public static void SetupBasicScintilla(ref Scintilla scintilla, WindowsFormsHost hostObject)
        {
            scintilla.Lexer = Lexer.Null;
            scintilla.Margins[1].Width = 2;
            scintilla.BorderStyle = System.Windows.Forms.BorderStyle.None;
            scintilla.ScrollWidth = Convert.ToInt32(hostObject.ActualWidth - 1);
            scintilla.WrapMode = WrapMode.Char;

            scintilla.SetSelectionBackColor(true, ProjectColors.DarkTeal);
            scintilla.SetSelectionForeColor(true, Color.White);

            scintilla.Indicators[9].Style = IndicatorStyle.TextFore;
            scintilla.Indicators[9].ForeColor = Color.Red;
            scintilla.Indicators[9].Alpha = 100;
            scintilla.Indicators[9].OutlineAlpha = 100;

            scintilla.Indicators[10].Style = IndicatorStyle.FullBox;
            scintilla.Indicators[10].ForeColor = ProjectColors.Teal;
            scintilla.Indicators[10].Alpha = 100;
            scintilla.Indicators[10].OutlineAlpha = 100;

            scintilla.Indicators[11].Style = IndicatorStyle.TextFore;
            scintilla.Indicators[11].ForeColor = ProjectColors.Orange;
            scintilla.Indicators[11].Alpha = 100;
            scintilla.Indicators[11].OutlineAlpha = 100;

            scintilla.Indicators[12].Style = IndicatorStyle.TextFore;
            scintilla.Indicators[12].ForeColor = ProjectColors.Blue;
            scintilla.Indicators[12].Alpha = 100;
            scintilla.Indicators[12].OutlineAlpha = 100;

            scintilla.Indicators[13].Style = IndicatorStyle.CompositionThick;
            scintilla.Indicators[13].ForeColor = ProjectColors.Orange;
            scintilla.Indicators[13].Alpha = 100;
            scintilla.Indicators[13].OutlineAlpha = 100;

            scintilla.Indicators[14].Style = IndicatorStyle.CompositionThick;
            scintilla.Indicators[14].ForeColor = ProjectColors.Blue;
            scintilla.Indicators[14].Alpha = 100;
            scintilla.Indicators[14].OutlineAlpha = 100;
        }

        public void ConfigureBraceMatching(Scintilla scintilla)
        {
            scintilla.Styles[Style.BraceLight].BackColor = Color.GreenYellow;
            scintilla.Styles[Style.BraceLight].ForeColor = Color.Black;
            scintilla.Styles[Style.BraceBad].ForeColor = Color.Red;
            scintilla.UpdateUI += this.Scintilla_UpdateUI;
        }

        public void HighlightGroups(bool evenNumber, int index, int length)
        {
            this.Highlight(evenNumber ? 11 : 12, index, length);
            this.Highlight(evenNumber ? 13 : 14, index, length);
        }

        public void HighlightError(List<int> symbolPositions)
        {
            this._scintilla.IndicatorCurrent = 9;

            foreach (int pos in symbolPositions)
            {
                this._scintilla.IndicatorFillRange(pos, 1);
            }
        }

        public void Highlight(int indicator, int pos, int length)
        {
            this._scintilla.IndicatorCurrent = indicator;
            this._scintilla.IndicatorFillRange(pos, length);
        }

        public void ClearHighlights()
        {
            List<int> usedIndicators = new List<int>
            {
                9,
                10,
                11,
                12,
                13,
                14
            };

            foreach (int indicator in usedIndicators)
            {
                this._scintilla.IndicatorCurrent = indicator;
                this._scintilla.IndicatorClearRange(0, this._scintilla.Text.Length);
            }
        }

        private void Scintilla_UpdateUI(object sender, UpdateUIEventArgs e)
        {
            Scintilla scintilla = (Scintilla)sender;

            int caretPos = scintilla.CurrentPosition;
            if (this._lastCaretPos != caretPos)
            {
                this._lastCaretPos = caretPos;
                int bracePos1 = -1;
                int bracePos2 = -1;

                // Check char near caret is brace
                if (caretPos > 0 && HelperMethods.IsBrace(scintilla.GetCharAt(caretPos - 1)))
                {
                    bracePos1 = caretPos - 1;
                }
                else if (HelperMethods.IsBrace(scintilla.GetCharAt(caretPos)))
                {
                    bracePos1 = caretPos;
                }

                // Check if not escaped
                if (bracePos1 > 0 && CheckIfEscaped(bracePos1))
                {
                    bracePos1 = -1;
                }

                if (bracePos1 >= 0)
                {
                    bracePos2 = FindNextMatchingBrace(scintilla.GetCharAt(bracePos1), bracePos1);

                    if (bracePos2 != Scintilla.InvalidPosition)
                    {
                        scintilla.BraceHighlight(bracePos1, bracePos2);
                    }
                    else
                    {
                        scintilla.BraceBadLight(bracePos1);
                    }
                }
                else
                {
                    scintilla.BraceHighlight(Scintilla.InvalidPosition, Scintilla.InvalidPosition);
                    scintilla.HighlightGuide = 0;
                }
            }

            bool CheckIfEscaped(int bracePos)
            {
                int posBeforeBrace = bracePos - 1;
                bool escaped = false;
                while (posBeforeBrace >= 0 && scintilla.GetCharAt(posBeforeBrace) == '\\')
                {
                    escaped = !escaped;
                    posBeforeBrace--;
                }

                return escaped;
            }

            int FindNextMatchingBrace(int sameBrace, int startFrom)
            {
                bool isOpeningBrace = HelperMethods.IsOpeningBrace(sameBrace);
                int braceToFind = this.FindMatchingBraceType(sameBrace);
                if (braceToFind < 0)
                {
                    return -1;
                }

                int index = startFrom;
                IncreaseOrDecreaseIndex();

                Stack<int> sameBraces = new Stack<int>();

                while (index < scintilla.TextLength && index >= 0)
                {
                    int charToCheck = scintilla.GetCharAt(index);
                    if (charToCheck == sameBrace && !CheckIfEscaped(index))
                    {
                        sameBraces.Push(index);
                    }
                    else if (charToCheck == braceToFind && !CheckIfEscaped(index))
                    {
                        if (sameBraces.Count != 0)
                        {
                            sameBraces.Pop();
                        }
                        else
                        {
                            return index;
                        }
                    }

                    IncreaseOrDecreaseIndex();
                }

                return -1;

                void IncreaseOrDecreaseIndex()
                {
                    if (isOpeningBrace)
                    {
                        index++;
                    }
                    else
                    {
                        index--;
                    }
                }
            }
        }

        private int FindMatchingBraceType(int brace)
        {
            switch (brace)
            {
                case '[':
                    return ']';
                case '{':
                    return '}';
                case '(':
                    return ')';
                case ']':
                    return '[';
                case '}':
                    return '{';
                case ')':
                    return '(';
                default:
                    return -1;
            }
        }
    }
}

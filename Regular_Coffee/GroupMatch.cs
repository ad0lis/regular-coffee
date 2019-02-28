namespace Regular_Coffee
{
    public interface IGroupMatch : IMatchText
    {
        int Number { get; set; }
    }

    public class GroupMatch : IGroupMatch, IHighlightableMatch
    {
        private string _matchText;
        private int _number;
        private int _index;
        private int _length;

        public string MatchText
        {
            get => this._matchText;
            set => this._matchText = value;
        }

        public int Number
        {
            get => this._number;
            set => this._number = value;
        }

        public int Index
        {
            get => this._index;
            set => this._index = value;
        }

        public int Length
        {
            get => this._length;
            set => this._length = value;
        }
    }
}

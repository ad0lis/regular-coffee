namespace Regular_Coffee
{
    using System.Collections.Generic;

    public interface IMatchText
    {
        string MatchText { get; set; }
    }

    public interface IMainMatch : IMatchText
    {
        List<IGroupMatch> GroupMembers { get; set; }
    }

    public interface IHighlightableMatch
    {
        int Index { get; set; }

        int Length { get; set; }
    }

    public class MainMatch : IMainMatch, IHighlightableMatch
    {
        private string _matchText;
        private List<IGroupMatch> _groupMembers;
        private int _index;
        private int _length;

        public MainMatch()
        {
            this._groupMembers = new List<IGroupMatch>();
        }

        public string MatchText
        {
            get => this._matchText;
            set => this._matchText = value;
        }

        public List<IGroupMatch> GroupMembers
        {
            get => this._groupMembers;
            set => this._groupMembers = value;
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

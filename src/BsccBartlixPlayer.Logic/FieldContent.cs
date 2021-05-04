using NBattleshipCodingContest.Logic;

namespace BsccBartlixPlayer
{
    public class FieldContent
    {
        public FieldContent(BoardIndex index, SquareContent content)
        {
            Index = index;
            Content = content;
        }

        public BoardIndex Index { get; }

        public SquareContent Content { get; }
    }
}

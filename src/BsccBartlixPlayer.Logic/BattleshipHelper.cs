using System.Collections.Generic;
using System.Linq;
using NBattleshipCodingContest.Logic;

namespace BsccBartlixPlayer
{
    public class BattleshipHelper
    {
        private readonly List<int> _allShips = new(new int[] { 2, 3, 3, 4, 5 });
        private readonly EvaluationBoard _evaluationBoard;

        private List<(BoardIndex[], Direction)> _ships;

        internal BattleshipHelper(BoardContent board)
        {
            Board = board;
            _evaluationBoard = new EvaluationBoard(Fields.ToList());

            InitializeBattlefield();
        }

        public BoardContent Board { get; }

        public List<BoardIndex> AvailableUnkownFields { get; private set; }

        public List<int> MissingShips
        {
            get
            {
                var all = _allShips.ToList();

                foreach (var s in _ships)
                {
                    all.Remove(s.Item1.Length);
                }

                return all;
            }
        }

        public IEnumerable<FieldContent> Fields => Board.Select((d, i) => new FieldContent(new BoardIndex(i), d)).ToList();

        public IEnumerable<FieldContent> HitFields => Fields.SelectAllHits().ToList();

        public IEnumerable<BoardIndex> AllUnkownFields { get; private set; }

        public static BoardIndex GetNextShot(BoardContent board)
        {
            var helper = new BattleshipHelper(board);

            return helper.GetNextShot();
        }

        private void InitializeBattlefield()
        {
            SetMissingShips();
            SetAvailableUnkownFields();
            _evaluationBoard.Initialize(MissingShips.ToArray(), AllUnkownFields.ToList());
        }

        private void SetMissingShips()
        {
            _ships = new List<(BoardIndex[], Direction)>();

            var sunkenPos = Fields.SelectAllSunkenShips().ToList();

            while (sunkenPos.Any())
            {
                var s = sunkenPos.First();

                var shipDirectionHorizontal = false;

                if (s.Index.TryNext(Direction.Horizontal, out var rightPos) && Fields.Any(x => x.Index == rightPos && x.Content == SquareContent.SunkenShip))
                {
                    shipDirectionHorizontal = true;
                }

                IEnumerable<FieldContent> qry = sunkenPos;

                if (shipDirectionHorizontal)
                {
                    qry = qry.Where(x => x.Index.Row == s.Index.Row && x.Index.Column >= s.Index.Column);

                    var xxx = Fields.Where(x => x.Index.Row == s.Index.Row && x.Index.Column > s.Index.Column && x.Content != SquareContent.SunkenShip).FirstOrDefault();

                    if (xxx != null)
                    {
                        qry = qry.Where(x => x.Index.Column < xxx.Index.Column).OrderBy(x => x.Index.Column);
                    }
                }
                else
                {
                    qry = qry.Where(x => x.Index.Column == s.Index.Column && x.Index.Row >= s.Index.Row);

                    var yyy = Fields.Where(x => x.Index.Column == s.Index.Column && x.Index.Row > s.Index.Row && x.Content != SquareContent.SunkenShip).FirstOrDefault();

                    if (yyy != null)
                    {
                        qry = qry.Where(x => x.Index.Row < yyy.Index.Row).OrderBy(x => x.Index.Row);
                    }
                }

                var bla = qry.ToList();

                _ships.Add((bla.Select(x => x.Index).ToArray(), shipDirectionHorizontal ? Direction.Horizontal : Direction.Vertical));

                foreach (var b in bla)
                {
                    sunkenPos.Remove(b);
                }
            }
        }

        private void SetAvailableUnkownFields()
        {
            var unknown = Fields.SelectAllUnkownFields().ToList();

            foreach (var s in Fields.SelectAllSunkenShips())
            {
                if (s.Index.TryPrevious(Direction.Horizontal, out var left))
                {
                    var removeItem = unknown.FirstOrDefault(x => x.Index == left);
                    if (removeItem != null)
                    {
                        unknown.Remove(removeItem);
                    }
                }

                if (s.Index.TryNext(Direction.Horizontal, out var right))
                {
                    var removeItem = unknown.FirstOrDefault(x => x.Index == right);
                    if (removeItem != null)
                    {
                        unknown.Remove(removeItem);
                    }
                }

                if (s.Index.TryPrevious(Direction.Vertical, out var top))
                {
                    var removeItem = unknown.FirstOrDefault(x => x.Index == top);
                    if (removeItem != null)
                    {
                        unknown.Remove(removeItem);
                    }
                }

                if (s.Index.TryNext(Direction.Vertical, out var bottom))
                {
                    var removeItem = unknown.FirstOrDefault(x => x.Index == bottom);
                    if (removeItem != null)
                    {
                        unknown.Remove(removeItem);
                    }
                }
            }

            foreach (var s in _ships)
            {
                if (s.Item2 == Direction.Horizontal)
                {
                    if (s.Item1.First().TryPrevious(Direction.Horizontal, out var leftPos))
                    {
                        if (leftPos.TryPrevious(Direction.Vertical, out var leftTop) && unknown.Any(x => x.Index == leftTop && x.Content == SquareContent.Unknown))
                        {
                            var removeItem = unknown.FirstOrDefault(x => x.Index == leftTop);
                            unknown.Remove(removeItem);
                        }

                        if (leftPos.TryNext(Direction.Vertical, out var leftBottom) && unknown.Any(x => x.Index == leftBottom && x.Content == SquareContent.Unknown))
                        {
                            var removeItem = unknown.FirstOrDefault(x => x.Index == leftBottom);
                            unknown.Remove(removeItem);
                        }
                    }

                    if (s.Item1.Last().TryNext(Direction.Horizontal, out var rightPos))
                    {
                        if (rightPos.TryPrevious(Direction.Vertical, out var rightTop) && unknown.Any(x => x.Index == rightTop && x.Content == SquareContent.Unknown))
                        {
                            var removeItem = unknown.FirstOrDefault(x => x.Index == rightTop);
                            unknown.Remove(removeItem);
                        }

                        if (rightPos.TryNext(Direction.Vertical, out var rightBottom) && unknown.Any(x => x.Index == rightBottom && x.Content == SquareContent.Unknown))
                        {
                            var removeItem = unknown.FirstOrDefault(x => x.Index == rightBottom);
                            unknown.Remove(removeItem);
                        }
                    }
                }

                if (s.Item2 == Direction.Vertical)
                {
                    if (s.Item1.First().TryPrevious(Direction.Vertical, out var topPos))
                    {
                        if (topPos.TryPrevious(Direction.Horizontal, out var topLeft) && unknown.Any(x => x.Index == topLeft && x.Content == SquareContent.Unknown))
                        {
                            var removeItem = unknown.FirstOrDefault(x => x.Index == topLeft);
                            unknown.Remove(removeItem);
                        }

                        if (topPos.TryNext(Direction.Horizontal, out var topRight) && unknown.Any(x => x.Index == topRight && x.Content == SquareContent.Unknown))
                        {
                            var removeItem = unknown.FirstOrDefault(x => x.Index == topRight);
                            unknown.Remove(removeItem);
                        }
                    }

                    if (s.Item1.Last().TryNext(Direction.Vertical, out var bottomPos))
                    {
                        if (bottomPos.TryPrevious(Direction.Horizontal, out var bottomLeft) && unknown.Any(x => x.Index == bottomLeft && x.Content == SquareContent.Unknown))
                        {
                            var removeItem = unknown.FirstOrDefault(x => x.Index == bottomLeft);
                            unknown.Remove(removeItem);
                        }

                        if (bottomPos.TryNext(Direction.Horizontal, out var bottomRight) && unknown.Any(x => x.Index == bottomRight && x.Content == SquareContent.Unknown))
                        {
                            var removeItem = unknown.FirstOrDefault(x => x.Index == bottomRight);
                            unknown.Remove(removeItem);
                        }
                    }
                }
            }

            //// remove Unknown fields where no place for ships.

            AllUnkownFields = unknown.Select(x => x.Index).ToList();

            RemoveKnownUnkownFields(unknown);

            AvailableUnkownFields = unknown.Select(x => x.Index).ToList();
        }

        private BoardIndex GetNextShot()
        {
            if (Fields.Any(x => x.Content == SquareContent.HitShip))
            {
                return FindNextTryOnHits();
            }
            else
            {
                return FindNextTry();
            }
        }

        private BoardIndex FindNextTryOnHits()
        {
            var evaluationBoard = new EvaluationBoard(Fields.ToList());
            return evaluationBoard.GetBestShotField(HitFields.ToArray(), MissingShips.ToArray(), AllUnkownFields.ToArray());
        }

        private BoardIndex FindNextTry()
        {
            return _evaluationBoard.GetBestField();
        }

        private void RemoveKnownUnkownFields(List<FieldContent> unknownFields)
        {
            var maybeIndex = new List<BoardIndex>();

            var length = MissingShips.Min();

            foreach (var u in unknownFields)
            {
                var fields = unknownFields.Where(x => x.Index.Row == u.Index.Row &&
                                                x.Index.Column >= u.Index.Column &&
                                                x.Index.Column < u.Index.Column + length).ToList();

                if (fields.Count == length)
                {
                    fields.ForEach(x => maybeIndex.Add(x.Index));
                }

                fields.Clear();

                fields = unknownFields.Where(x => x.Index.Column == u.Index.Column &&
                                                x.Index.Row >= u.Index.Row &&
                                                x.Index.Row < u.Index.Row + length).ToList();

                if (fields.Count == length)
                {
                    fields.ForEach(x => maybeIndex.Add(x.Index));
                }
            }

            foreach (var u in unknownFields.ToList())
            {
                if (!maybeIndex.Any(x => x == u.Index))
                {
                    unknownFields.Remove(u);
                }
            }
        }
    }
}

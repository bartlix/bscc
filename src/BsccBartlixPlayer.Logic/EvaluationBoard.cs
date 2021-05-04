using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NBattleshipCodingContest.Logic;

namespace BsccBartlixPlayer
{
    public class EvaluationBoard
    {
        private readonly List<FieldContent> _fields;
        private readonly ConcurrentDictionary<BoardIndex, int> _items;

        public EvaluationBoard(IEnumerable<FieldContent> fields)
        {
            _fields = fields.ToList();
            _items = new ConcurrentDictionary<BoardIndex, int>(fields.ToDictionary(x => x.Index, (x) => 0));
        }

        public BoardIndex GetBestField()
        {
            var first = _items.OrderByDescending(x => x.Value).First();

#if DEBUG
            ////Console.WriteLine("BestFields");
            ////foreach (var i in _items.Where(x => x.Value == first.Value))
            ////{
            ////    Console.WriteLine($"Value:{i.Value} Column:{i.Key.Column}, Row:{i.Key.Row}");
            ////}
#endif
            return first.Key;
        }

        public int GetScore(BoardIndex index)
        {
            return _items[index];
        }

        public void Initialize(int[] missingShips, List<BoardIndex> unkownFields)
        {
            SetScoreHorizontal(missingShips, unkownFields);
            SetScoreVertical(missingShips, unkownFields);
        }

        public BoardIndex GetBestShotField(FieldContent[] hitFields, int[] missingShips, BoardIndex[] unknownFields)
        {
            var hitsPerShip = new List<List<BoardIndex>>();

            // Find missing hit between hitFields
            if (hitFields.Length > 1 && hitFields.All(x => x.Index.Row == hitFields[0].Index.Row))
            {
                var list = hitFields.Where(x => x.Index.Row == hitFields[0].Index.Row).OrderBy(x => x.Index.Column).ToList();

                foreach (var pos in list)
                {
                    if (pos != list.Last() && pos.Index.TryNext(Direction.Horizontal, out var blub) && _fields[blub].Content == SquareContent.Unknown)
                    {
                        if (blub.TryNext(Direction.Horizontal, out var nexhit) && hitFields.Any(x => x.Index == nexhit))
                        {
                            return blub;
                        }
                    }
                }
            }

            if (hitFields.Length > 1 && hitFields.All(x => x.Index.Column == hitFields[0].Index.Column))
            {
                var list = hitFields.Where(x => x.Index.Column == hitFields[0].Index.Column).OrderBy(x => x.Index.Row).ToList();

                foreach (var pos in list)
                {
                    if (pos != list.Last() && pos.Index.TryNext(Direction.Vertical, out var blub) && _fields[blub].Content == SquareContent.Unknown)
                    {
                        if (blub.TryNext(Direction.Vertical, out var nexhit) && hitFields.Any(x => x.Index == nexhit))
                        {
                            return blub;
                        }
                    }
                }
            }

            foreach (var hit in hitFields)
            {
                if (!hitsPerShip.Any())
                {
                    hitsPerShip.Add(new List<BoardIndex> { hit.Index });
                    continue;
                }

                var x = hitsPerShip.Where(x => (x.All(y => y.Column == hit.Index.Column) && x.Any(y => Math.Abs(y.Row - hit.Index.Row) == 1)) ||
                                               (x.All(y => y.Row == hit.Index.Row) && x.Any(y => Math.Abs(y.Column - hit.Index.Column) == 1))).FirstOrDefault();
                if (x != null)
                {
                    x.Add(hit.Index);
                }
                else
                {
                    hitsPerShip.Add(new List<BoardIndex> { hit.Index });
                }
            }

            List<int> searchShipList = missingShips.ToList();

            foreach (var h in hitsPerShip)
            {
                searchShipList = searchShipList.Where(x => x > h.Count).ToList();
            }

            Parallel.ForEach(searchShipList, s =>
            ////foreach (var s in searchShipList)
            {
                Direction? shipDirection = null;

                if (hitFields.Length > 1)
                {
                    if (hitFields.All(x => x.Index.Row == hitFields[0].Index.Row))
                    {
                        var col = hitFields[0].Index.Column;
                        foreach (var h in hitFields)
                        {
                            if (h.Index.Column == col)
                            {
                                col++;
                            }
                            else
                            {
                                shipDirection = null;
                                break;
                            }

                            shipDirection = Direction.Horizontal;
                        }
                    }
                    else if (hitFields.All(x => x.Index.Column == hitFields[0].Index.Column))
                    {
                        var row = hitFields[0].Index.Row;
                        foreach (var h in hitFields)
                        {
                            if (h.Index.Row == row)
                            {
                                row++;
                            }
                            else
                            {
                                shipDirection = null;
                                break;
                            }

                            shipDirection = Direction.Vertical;
                        }
                    }
                }

                List<BoardIndex> ship = new List<BoardIndex>();

                if (shipDirection == null || shipDirection == Direction.Horizontal)
                {
                    var row = hitFields[0].Index.Row;

                    for (var i = 0; i < s; i++)
                    {
                        ship.Add(new BoardIndex(i, row));
                    }

                    while (ship != null)
                    {
                        var ok = false;

                        if (hitFields.Any(x => ship.Contains(x.Index)))
                        {
                            var tryingFields = ship.Where(x => !hitFields.Any(y => y.Index == x)).ToList();
                            ok = true;
                            foreach (var t in tryingFields)
                            {
                                ok &= unknownFields.Any(x => x == t);
                            }

                            if (ok)
                            {
                                var result = hitsPerShip.Where(x => x.All(x => ship.Contains(x))).FirstOrDefault();

                                var x = result == null ? 1 : result.Count;

                                if (x > 1)
                                {
                                    x *= 100;
                                }

                                foreach (var p in ship.Where(x => unknownFields.Contains(x)).ToList())
                                {
                                    _items[p] += 1 * x;
                                }
                            }
                        }

                        var newShip = new List<BoardIndex>();
                        foreach (var p in ship)
                        {
                            if (p.TryNext(Direction.Horizontal, out var newPos))
                            {
                                newShip.Add(newPos);
                            }
                        }

                        if (newShip.Count == ship.Count)
                        {
                            ship = newShip;
                        }
                        else
                        {
                            ship = null;
                        }
                    }
                }

                ship = new List<BoardIndex>();

                if (shipDirection == null || shipDirection == Direction.Vertical)
                {
                    var column = hitFields[0].Index.Column;

                    for (var i = 0; i < s; i++)
                    {
                        ship.Add(new BoardIndex(column, i));
                    }

                    while (ship != null)
                    {
                        var ok = false;

                        if (hitFields.Any(x => ship.Contains(x.Index)))
                        {
                            var tryingFields = ship.Where(x => !hitFields.Any(y => y.Index == x)).ToList();
                            ok = true;
                            foreach (var t in tryingFields)
                            {
                                ok &= unknownFields.Any(x => x == t);
                            }

                            if (ok)
                            {
                                var result = hitsPerShip.Where(x => x.All(x => ship.Contains(x))).FirstOrDefault();

                                var x = result == null ? 1 : result.Count;

                                if (x > 1)
                                {
                                    x *= 100;
                                }

                                foreach (var p in ship.Where(x => unknownFields.Contains(x)).ToList())
                                {
                                    _items[p] += 1 * x;
                                }
                            }
                        }

                        var newShip = new List<BoardIndex>();
                        foreach (var p in ship)
                        {
                            if (p.TryNext(Direction.Vertical, out var newPos))
                            {
                                newShip.Add(newPos);
                            }
                        }

                        if (newShip.Count == ship.Count)
                        {
                            ship = newShip;
                        }
                        else
                        {
                            ship = null;
                        }
                    }
                }
            });

            var first = _items.OrderByDescending(x => x.Value).First();

            if (first.Key.Row == 0 && first.Key.Column == 0 && first.Value == 0)
            {
            }

#if DEBUG
            Console.WriteLine("BestShotFields");
            foreach (var i in _items.Where(x => x.Value == first.Value))
            {
                Console.WriteLine($"Value:{i.Value} Column:{i.Key.Column}, Row:{i.Key.Row}");
            }
#endif

            return first.Key;
        }

        private void SetScoreHorizontal(int[] missingShips, List<BoardIndex> unknownFields)
        {
            Parallel.ForEach(missingShips, s =>
            {
                Parallel.For(0, 10, row =>
                {
                    List<BoardIndex> ship = new List<BoardIndex>();

                    for (var i = 0; i < s; i++)
                    {
                        ship.Add(new BoardIndex(i, row));
                    }

                    while (ship != null)
                    {
                        var ok = true;
                        foreach (var p in ship)
                        {
                            ok &= unknownFields.Any(x => x == p);
                        }

                        if (ok)
                        {
                            foreach (var p in ship)
                            {
                                _items[p]++;
                            }
                        }

                        var newShip = new List<BoardIndex>();
                        foreach (var p in ship)
                        {
                            if (p.TryNext(Direction.Horizontal, out var newPos))
                            {
                                newShip.Add(newPos);
                            }
                        }

                        if (newShip.Count == ship.Count)
                        {
                            ship = newShip;
                        }
                        else
                        {
                            ship = null;
                        }
                    }
                });
            });
        }

        private void SetScoreVertical(int[] missingShips, List<BoardIndex> unknownFields)
        {
            Parallel.ForEach(missingShips, s =>
            {
                Parallel.For(0, 10, col =>
                {
                    List<BoardIndex> ship = new List<BoardIndex>();

                    for (var i = 0; i < s; i++)
                    {
                        ship.Add(new BoardIndex(col, i));
                    }

                    while (ship != null)
                    {
                        var ok = true;
                        foreach (var p in ship)
                        {
                            ok &= unknownFields.Any(x => x == p);
                        }

                        if (ok)
                        {
                            foreach (var p in ship)
                            {
                                _items[p]++;
                            }
                        }

                        var newShip = new List<BoardIndex>();
                        foreach (var p in ship)
                        {
                            if (p.TryNext(Direction.Vertical, out var newPos))
                            {
                                newShip.Add(newPos);
                            }
                        }

                        if (newShip.Count == ship.Count)
                        {
                            ship = newShip;
                        }
                        else
                        {
                            ship = null;
                        }
                    }
                });
            });
        }
    }
}
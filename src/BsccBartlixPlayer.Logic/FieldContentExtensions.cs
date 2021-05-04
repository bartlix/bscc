using System.Collections.Generic;
using System.Linq;
using NBattleshipCodingContest.Logic;

namespace BsccBartlixPlayer
{
    public static class FieldContentExtensions
    {
        public static IEnumerable<FieldContent> SelectAllUnkownFields(this IEnumerable<FieldContent> source)
        {
            return source.Where(x => x.Content == SquareContent.Unknown);
        }

        public static IEnumerable<FieldContent> SelectAllSunkenShips(this IEnumerable<FieldContent> source)
        {
            return source.Where(x => x.Content == SquareContent.SunkenShip);
        }

        public static IEnumerable<FieldContent> SelectAllHits(this IEnumerable<FieldContent> source)
        {
            return source.Where(x => x.Content == SquareContent.HitShip);
        }
    }
}

using System;
using NBattleshipCodingContest.Logic;

namespace BsccBartlixPlayer
{
    public record ShotRequest(Guid GameId, BoardIndex? LastShot, string Board);
}

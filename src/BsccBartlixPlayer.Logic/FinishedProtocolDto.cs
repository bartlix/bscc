using System;
using NBattleshipCodingContest.Logic;

namespace BsccBartlixPlayer.Controllers
{
    public record FinishedProtocolDto(Guid GameId, BoardContent Board, int NumberOfShots);
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NBattleshipCodingContest.Logic;

namespace BsccBartlixPlayer.Controllers
{
    [Route("api/v3/bartlix")]
    [ApiController]
    public class BartlixController : ControllerBase
    {
        [HttpGet("getReady")]
        public ActionResult GetReady() => Ok();

        [HttpPost("finished")]
        public ActionResult Finished([FromBody] FinishedProtocolDto[] x)
        {
            foreach (var y in x.OrderBy(x => x.NumberOfShots))
            {
                Console.WriteLine(y.Board);
                Console.WriteLine($"Board:{y.Board.ToShortString()}");
                Console.WriteLine($"GameId: {y.GameId}");
                Console.WriteLine($"Number of shots: {y.NumberOfShots}");

                var items = y.Board.Select((d, i) => new FieldContent(new BoardIndex(i), d));

                var sunkenPos = items.Where(x => x.Content == SquareContent.SunkenShip).ToList();
            }

            return Ok();
        }

        [HttpPost("getShots")]
        public ActionResult<BoardIndex[]> GetShots([FromBody] ShotRequest[] shotRequests)
        {
            // Create a helper variable that will receive our calculated
            // shots for each shot request.
            var shots = new BoardIndex[shotRequests.Length];

            Parallel.For(0, shotRequests.Length, index =>
            {
                var board = new BoardContent(shotRequests[index].Board.Select(d => (byte)BoardContentJsonConverter.CharToSquareContent(d)));
#if DEBUG
                Console.WriteLine(board);
                Console.WriteLine(board.ToShortString());
#endif
                shots[index] = BattleshipHelper.GetNextShot(board);
#if DEBUG
                Console.WriteLine($"Next shot:{(string)shots[index]}");
                ////Thread.Sleep(500);
#endif
            });

            return shots;
        }
    }
}

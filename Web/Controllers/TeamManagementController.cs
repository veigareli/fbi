using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeamManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add-player")]
        public async Task<IActionResult> AddPlayer([FromBody] AddPlayerRequest request)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not logged in");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Check if player exists
            var player = await _context.Players
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == request.PlayerId);

            if (player == null)
            {
                return NotFound("Player not found");
            }

            // Get current round
            var currentRoundRecord = await _context.CurrentRound.FirstOrDefaultAsync();
            var currentRound = currentRoundRecord?.RoundNumber ?? 1;
            
            // Use current round instead of request round
            var roundToUse = currentRound;
            
            // Check if player is already in team
            var existingFantasyPlayer = await _context.FantasyTeams
                .FirstOrDefaultAsync(ft => ft.UserId == user.Id && ft.PlayerId == request.PlayerId && ft.Round == roundToUse);

            if (existingFantasyPlayer != null)
            {
                return BadRequest("Player is already in your team");
            }

            // Get current team for this round
            var currentTeam = await _context.FantasyTeams
                .Include(ft => ft.Player)
                .Where(ft => ft.UserId == user.Id && ft.Round == roundToUse && ft.IsActive)
                .ToListAsync();

            // Check team size limit (10 players)
            if (currentTeam.Count >= 10)
            {
                return BadRequest("Team is full. Maximum 10 players allowed.");
            }

            // Check position limit (max 2 players per position)
            var playersAtPosition = currentTeam.Count(ft => ft.Player.Position == player.Position);
            if (playersAtPosition >= 2)
            {
                return BadRequest($"You already have 2 players at {player.Position} position. Maximum 2 players per position allowed.");
            }

            // Check budget constraint
            var currentUsedBudget = currentTeam.Sum(ft => ft.Player.Cost);
            if (currentUsedBudget + player.Cost > 100)
            {
                return BadRequest($"Adding this player would exceed your budget. Current: {currentUsedBudget}, Player cost: {player.Cost}, Budget limit: 100");
            }

            // Determine if player should be on court or bench
            // If there's already a player of this position on court, new player goes to bench
            var existingCourtPlayer = currentTeam.FirstOrDefault(ft => ft.Player.Position == player.Position && ft.IsOnCourt);
            var isOnCourt = existingCourtPlayer == null; // true if no court player exists, false if bench

            // Add player to fantasy team
            var fantasyPlayer = new FantasyTeam
            {
                UserId = user.Id,
                PlayerId = player.Id,
                Round = roundToUse,
                IsActive = true,
                IsOnCourt = isOnCourt
            };

            _context.FantasyTeams.Add(fantasyPlayer);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Player added successfully",
                player = new {
                    id = player.Id,
                    name = player.Name,
                    position = player.Position,
                    team = player.Team?.Name,
                    cost = player.Cost,
                    totalPoints = player.TotalPoints
                }
            });
        }

        [HttpPost("remove-player")]
        public async Task<IActionResult> RemovePlayer([FromBody] RemovePlayerRequest request)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not logged in");
            }

            // Get current round
            var currentRoundRecord = await _context.CurrentRound.FirstOrDefaultAsync();
            var currentRound = currentRoundRecord?.RoundNumber ?? 1;
            
            var fantasyPlayer = await _context.FantasyTeams
                .Include(ft => ft.Player)
                .FirstOrDefaultAsync(ft => ft.UserId == int.Parse(userId) && 
                                         ft.PlayerId == request.PlayerId && 
                                         ft.Round == currentRound);

            if (fantasyPlayer == null)
            {
                return NotFound("Player not found in your team");
            }

            _context.FantasyTeams.Remove(fantasyPlayer);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Player removed successfully",
                player = new {
                    id = fantasyPlayer.Player.Id,
                    name = fantasyPlayer.Player.Name,
                    position = fantasyPlayer.Player.Position,
                    team = fantasyPlayer.Player.Team?.Name,
                    cost = fantasyPlayer.Player.Cost
                }
            });
        }

        [HttpPost("clear-team")]
        public async Task<IActionResult> ClearTeam([FromBody] ClearTeamRequest request)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not logged in");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Get current round
            var currentRoundRecord = await _context.CurrentRound.FirstOrDefaultAsync();
            var currentRound = currentRoundRecord?.RoundNumber ?? 1;
            
            // Get all fantasy team entries for this user and round
            var fantasyTeamEntries = await _context.FantasyTeams
                .Where(ft => ft.UserId == user.Id && ft.Round == currentRound)
                .ToListAsync();

            // Remove all entries
            _context.FantasyTeams.RemoveRange(fantasyTeamEntries);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Team cleared successfully",
                removedCount = fantasyTeamEntries.Count
            });
        }

        [HttpGet("team-status")]
        public async Task<IActionResult> GetTeamStatus(int round = 1)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not logged in");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Get current round if no round specified
            var currentRoundRecord = await _context.CurrentRound.FirstOrDefaultAsync();
            var currentRound = currentRoundRecord?.RoundNumber ?? 1;
            var roundToUse = round == 1 ? currentRound : round;

            var currentTeam = await _context.FantasyTeams
                .Include(ft => ft.Player)
                    .ThenInclude(p => p.Team)
                .Where(ft => ft.UserId == user.Id && ft.Round == roundToUse && ft.IsActive)
                .ToListAsync();
            var usedBudget = currentTeam.Sum(ft => ft.Player.Cost);
            var totalBudget = 100;
            var remainingBudget = totalBudget - usedBudget;

            return Ok(new {
                currentRound = round,
                totalBudget = totalBudget,
                usedBudget = usedBudget,
                remainingBudget = remainingBudget,
                selectedPlayersCount = currentTeam.Count,
                maxPlayers = 10,
                players = currentTeam.Select(ft => new {
                    id = ft.Player.Id,
                    name = ft.Player.Name,
                    position = ft.Player.Position,
                    team = ft.Player.Team?.Name,
                    cost = ft.Player.Cost,
                    totalPoints = ft.Player.TotalPoints,
                    isOnCourt = ft.IsOnCourt
                }).ToList()
            });
        }

        [HttpPost("swap-players")]
        public async Task<IActionResult> SwapPlayers([FromBody] SwapPlayersRequest request)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not logged in");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Get the players to swap using IsOnCourt field
            var fromPlayer = await _context.FantasyTeams
                .Include(ft => ft.Player)
                .FirstOrDefaultAsync(ft => ft.UserId == user.Id && 
                                         ft.Round == request.Round && 
                                         ft.IsActive &&
                                         ft.Player.Position == request.FromPosition &&
                                         ft.IsOnCourt == !request.FromIsBench);

            var toPlayer = await _context.FantasyTeams
                .Include(ft => ft.Player)
                .FirstOrDefaultAsync(ft => ft.UserId == user.Id && 
                                         ft.Round == request.Round && 
                                         ft.IsActive &&
                                         ft.Player.Position == request.ToPosition &&
                                         ft.IsOnCourt == !request.ToIsBench);

            // Validate the swap
            if (fromPlayer == null)
            {
                return BadRequest($"No player found at {request.FromPosition} position on {(request.FromIsBench ? "bench" : "court")}");
            }

            // If swapping with an empty position, just move the player
            if (toPlayer == null)
            {
                // Check if there's already a player in the target position
                var existingPlayer = await _context.FantasyTeams
                    .FirstOrDefaultAsync(ft => ft.UserId == user.Id && 
                                             ft.Round == request.Round && 
                                             ft.IsActive &&
                                             ft.Player.Position == request.ToPosition &&
                                             ft.IsOnCourt == !request.ToIsBench);

                if (existingPlayer != null)
                {
                    return BadRequest($"Cannot move to {(request.ToIsBench ? "bench" : "court")} {request.ToPosition} - position is already occupied");
                }

                // Move the player to the new position
                fromPlayer.IsOnCourt = !request.ToIsBench;
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = $"Moved {fromPlayer.Player.Name} to {request.ToPosition} position on {(request.ToIsBench ? "bench" : "court")}",
                    action = "move"
                });
            }

            // If both positions have players, swap their IsOnCourt values
            var fromIsOnCourt = fromPlayer.IsOnCourt;
            var toIsOnCourt = toPlayer.IsOnCourt;

            fromPlayer.IsOnCourt = toIsOnCourt;
            toPlayer.IsOnCourt = fromIsOnCourt;

            await _context.SaveChangesAsync();

            return Ok(new { 
                message = $"Swapped {fromPlayer.Player.Name} and {toPlayer.Player.Name}",
                action = "swap",
                fromPlayer = new {
                    id = fromPlayer.Player.Id,
                    name = fromPlayer.Player.Name,
                    position = fromPlayer.Player.Position
                },
                toPlayer = new {
                    id = toPlayer.Player.Id,
                    name = toPlayer.Player.Name,
                    position = toPlayer.Player.Position
                }
            });
        }
    }

    public class AddPlayerRequest
    {
        public int PlayerId { get; set; }
        public int Round { get; set; } = 1;
    }

    public class RemovePlayerRequest
    {
        public int PlayerId { get; set; }
        public int Round { get; set; } = 1;
    }

    public class ClearTeamRequest
    {
        public int Round { get; set; } = 1;
    }

    public class SwapPlayersRequest
    {
        public string FromPosition { get; set; } = string.Empty;
        public bool FromIsBench { get; set; }
        public string ToPosition { get; set; } = string.Empty;
        public bool ToIsBench { get; set; }
        public int Round { get; set; } = 1;
    }
}

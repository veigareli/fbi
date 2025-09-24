using Web.Models;

namespace Web.Services
{
    public class ScoringService
    {
        /// <summary>
        /// Calculates fantasy points for a player based on their statistics
        /// </summary>
        /// <param name="points">Points scored by the player</param>
        /// <param name="rebounds">Rebounds by the player</param>
        /// <param name="assists">Assists by the player</param>
        /// <param name="steals">Steals by the player</param>
        /// <param name="blocks">Blocks by the player</param>
        /// <param name="turnovers">Turnovers by the player</param>
        /// <param name="teamWin">Whether the player's team won</param>
        /// <returns>Total fantasy points</returns>
        public static int CalculateFantasyPoints(int points, int rebounds, int assists, int steals, int blocks, int turnovers, bool teamWin)
        {
            int fantasyPoints = 0;
            
            // +1 point for each point scored
            fantasyPoints += points;
            
            // +1 rebound for each rebound
            fantasyPoints += rebounds;
            
            // +1 assist for each assist
            fantasyPoints += assists;
            
            // +2 steals for each steal
            fantasyPoints += steals * 2;
            
            // +2 blocks for each block
            fantasyPoints += blocks * 2;
            
            // -1 turnover for each turnover
            fantasyPoints -= turnovers;
            
            // +5 team win if your player's team wins, -3 if team loses
            if (teamWin)
            {
                fantasyPoints += 5;
            }
            else
            {
                fantasyPoints -= 3;
            }
            
            return fantasyPoints;
        }
        
        /// <summary>
        /// Calculates fantasy points for a PlayerRoundPoints record
        /// </summary>
        /// <param name="playerRoundPoints">The player's round statistics</param>
        /// <returns>Total fantasy points</returns>
        public static int CalculateFantasyPoints(PlayerRoundPoints playerRoundPoints)
        {
            return CalculateFantasyPoints(
                playerRoundPoints.Points,
                playerRoundPoints.Rebounds,
                playerRoundPoints.Assists,
                playerRoundPoints.Steals,
                playerRoundPoints.Blocks,
                playerRoundPoints.Turnovers,
                playerRoundPoints.TeamWin
            );
        }
        
        /// <summary>
        /// Updates the TotalPoints field for a PlayerRoundPoints record
        /// </summary>
        /// <param name="playerRoundPoints">The player's round statistics</param>
        public static void UpdateTotalPoints(PlayerRoundPoints playerRoundPoints)
        {
            playerRoundPoints.TotalPoints = CalculateFantasyPoints(playerRoundPoints);
        }
        
        /// <summary>
        /// Calculates total team score for a round
        /// All 5 starters count + top 3 bench players
        /// </summary>
        /// <param name="teamPlayers">List of team players with their fantasy points</param>
        /// <returns>Total team score</returns>
        public static int CalculateTeamScore(List<(int FantasyPoints, bool IsOnCourt)> teamPlayers)
        {
            // Get all starters (5 players on court)
            var starters = teamPlayers.Where(tp => tp.IsOnCourt).ToList();
            
            // Get all bench players
            var benchPlayers = teamPlayers.Where(tp => !tp.IsOnCourt).ToList();
            
            // All starters count
            int totalScore = starters.Sum(s => s.FantasyPoints);
            
            // Top 3 bench players count
            var top3Bench = benchPlayers
                .OrderByDescending(bp => bp.FantasyPoints)
                .Take(3)
                .ToList();
            
            totalScore += top3Bench.Sum(bp => bp.FantasyPoints);
            
            return totalScore;
        }
    }
}

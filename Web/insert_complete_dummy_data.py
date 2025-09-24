#!/usr/bin/env python3
"""
Complete Fantasy Basketball Dummy Data Script
Creates comprehensive test data for 5 users across 22 rounds
"""

import sqlite3
import hashlib
import random
from datetime import datetime

# Database file path
DB_PATH = "/app/data/app.db"

# Team names (12 teams)
TEAMS = [
    "Breidablik", "Fjölnir", "Fylkir", "Hamar", "Haukar", "Hottur",
    "KV", "Selfoss", "Sindri", "Skallagrimur", "Snæfell", "Þorak"
]

# Player names for each team (12 players per team)
PLAYERS_BY_TEAM = {
    "Breidablik": [
        "Jóhann Freyrsson", "Einar Ævarsson", "Valdimar Kristjánsson", "Árni Jónsson",
        "Guðmundur Þórsson", "Ólafur Sigurðsson", "Björn Einarsson", "Magnús Jónsson",
        "Stefán Guðmundsson", "Hrafn Ólafsson", "Gunnar Björnsson", "Þorsteinn Magnússon"
    ],
    "Fjölnir": [
        "Níels Helgason", "Dagfinnur Þórsson", "Ívar Gunnarsson", "Ragnar Stefánsson",
        "Eiríkur Hrafnsson", "Baldur Gunnarsson", "Vilhjálmur Þorsteinsson", "Sigurður Ólafsson",
        "Geir Magnússon", "Hjalti Björnsson", "Ásgeir Einarsson", "Jón Guðmundsson"
    ],
    "Fylkir": [
        "Sævar Hrafnsson", "Fannar Helgason", "Ísak Þórsson", "Óskar Dagfinnursson",
        "Kristján Ívarsson", "Hjörleifur Ragnarsson", "Þórir Eiríkursson", "Baldvin Baldursson",
        "Gísli Vilhjálmursson", "Haukur Sigurðursson", "Snorri Geirsson", "Árni Hjaltisson"
    ],
    "Hamar": [
        "Gunnar Ásgeirsson", "Jón Jónsson", "Ólafur Sævarsson", "Björn Fannarsson",
        "Magnús Ísaksson", "Stefán Óskarsson", "Hrafn Kristjánsson", "Guðmundur Hjörleifursson",
        "Þorsteinn Þórirsson", "Einar Baldvinsson", "Árni Gíslisson", "Valdimar Haukursson"
    ],
    "Haukar": [
        "Snorri Haukarsson", "Árni Haukarsson", "Gunnar Haukarsson", "Jón Haukarsson",
        "Ólafur Haukarsson", "Björn Haukarsson", "Magnús Haukarsson", "Stefán Haukarsson",
        "Hrafn Haukarsson", "Guðmundur Haukarsson", "Þorsteinn Haukarsson", "Einar Haukarsson"
    ],
    "Hottur": [
        "Valdimar Valdimarsson", "Jóhann Jóhannsson", "Einar Einarsson", "Níels Níelsson",
        "Dagfinnur Dagfinnursson", "Sævar Sævarsson", "Fannar Fannarsson", "Ísak Ísaksson",
        "Óskar Óskarsson", "Kristján Kristjánsson", "Hjörleifur Hjörleifursson", "Þórir Þórirsson"
    ],
    "KV": [
        "Baldvin KVsson", "Gísli KVsson", "Haukur KVsson", "Snorri KVsson",
        "Árni KVsson", "Gunnar KVsson", "Jón KVsson", "Ólafur KVsson",
        "Björn KVsson", "Magnús KVsson", "Stefán KVsson", "Hrafn KVsson"
    ],
    "Selfoss": [
        "Guðmundur Selfossson", "Þorsteinn Selfossson", "Einar Selfossson", "Valdimar Selfossson",
        "Jóhann Selfossson", "Níels Selfossson", "Dagfinnur Selfossson", "Sævar Selfossson",
        "Fannar Selfossson", "Ísak Selfossson", "Óskar Selfossson", "Kristján Selfossson"
    ],
    "Sindri": [
        "Hjörleifur Sindrisson", "Þórir Sindrisson", "Baldvin Sindrisson", "Gísli Sindrisson",
        "Haukur Sindrisson", "Snorri Sindrisson", "Árni Sindrisson", "Gunnar Sindrisson",
        "Jón Sindrisson", "Ólafur Sindrisson", "Björn Sindrisson", "Magnús Sindrisson"
    ],
    "Skallagrimur": [
        "Stefán Skallagrimsson", "Hrafn Skallagrimsson", "Guðmundur Skallagrimsson", "Þorsteinn Skallagrimsson",
        "Einar Skallagrimsson", "Valdimar Skallagrimsson", "Jóhann Skallagrimsson", "Níels Skallagrimsson",
        "Dagfinnur Skallagrimsson", "Sævar Skallagrimsson", "Fannar Skallagrimsson", "Ísak Skallagrimsson"
    ],
    "Snæfell": [
        "Óskar Snæfellsson", "Kristján Snæfellsson", "Hjörleifur Snæfellsson", "Þórir Snæfellsson",
        "Baldvin Snæfellsson", "Gísli Snæfellsson", "Haukur Snæfellsson", "Snorri Snæfellsson",
        "Árni Snæfellsson", "Gunnar Snæfellsson", "Jón Snæfellsson", "Ólafur Snæfellsson"
    ],
    "Þorak": [
        "Björn Þoraksson", "Magnús Þoraksson", "Stefán Þoraksson", "Hrafn Þoraksson",
        "Guðmundur Þoraksson", "Þorsteinn Þoraksson", "Einar Þoraksson", "Valdimar Þoraksson",
        "Jóhann Þoraksson", "Níels Þoraksson", "Dagfinnur Þoraksson", "Sævar Þoraksson"
    ]
}

# Generate 100 users
USERS = []
for i in range(1, 101):
    USERS.append({
        "name": f"User {i}",
        "email": f"user{i}@fantasy.com", 
        "password": "user123"
    })

# Position distribution (2 players per position per team)
POSITIONS = ["PG", "SG", "SF", "PF", "C"]

def hash_password(password):
    """Hash password using SHA-256"""
    return hashlib.sha256(password.encode()).hexdigest()

def clear_database():
    """Clear all existing data"""
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    # Clear tables in reverse dependency order
    tables = [
        "CurrentRound", "UserRoundTeams", "UserRoundPoints", "PlayerRoundPoints", 
        "FantasyTeams", "Players", "Teams", "Users"
    ]
    
    for table in tables:
        try:
            cursor.execute(f"DELETE FROM {table}")
            print(f"Cleared {table}")
        except sqlite3.OperationalError:
            print(f"Skipped {table} (table doesn't exist)")
    
    conn.commit()
    conn.close()

def wait_for_tables():
    """Wait for tables to be created by the .NET application"""
    import time
    max_attempts = 30
    attempt = 0
    
    while attempt < max_attempts:
        try:
            conn = sqlite3.connect(DB_PATH)
            cursor = conn.cursor()
            cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='Users'")
            result = cursor.fetchone()
            conn.close()
            
            if result:
                print("Database tables found!")
                return True
        except sqlite3.OperationalError:
            pass
        
        attempt += 1
        print(f"Waiting for database tables... (attempt {attempt}/{max_attempts})")
        time.sleep(1)
    
    print("Warning: Database tables not found after waiting. Proceeding anyway...")
    return False

def insert_teams():
    """Insert all teams"""
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    for team_name in TEAMS:
        cursor.execute("INSERT INTO Teams (Name) VALUES (?)", (team_name,))
    
    conn.commit()
    conn.close()
    print(f"Inserted {len(TEAMS)} teams")

def insert_players():
    """Insert all players with positions and costs"""
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    # Get team IDs
    cursor.execute("SELECT Id, Name FROM Teams")
    teams = {name: team_id for team_id, name in cursor.fetchall()}
    
    player_count = 0
    for team_name, player_names in PLAYERS_BY_TEAM.items():
        team_id = teams[team_name]
        
        # Assign 2 players per position (10 players total per team)
        for i, player_name in enumerate(player_names[:10]):  # Only use first 10 players
            position = POSITIONS[i // 2]  # 2 players per position
            cost = random.randint(5, 25)  # Random cost between 5-25
            
            cursor.execute("""
                INSERT INTO Players (TeamId, Name, Position, Cost, TotalPoints)
                VALUES (?, ?, ?, ?, 0)
            """, (team_id, player_name, position, cost))
            
            player_count += 1
    
    conn.commit()
    conn.close()
    print(f"Inserted {player_count} players")

def insert_users():
    """Insert all users"""
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    for user in USERS:
        password_hash = hash_password(user["password"])
        cursor.execute("""
            INSERT INTO Users (Name, Email, PasswordHash, TotalPoints)
            VALUES (?, ?, ?, 0)
        """, (user["name"], user["email"], password_hash))
    
    conn.commit()
    conn.close()
    print(f"Inserted {len(USERS)} users")

def insert_user_round_teams():
    """Insert UserRoundTeam records for all users and rounds"""
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    # Get user IDs
    cursor.execute("SELECT Id FROM Users")
    user_ids = [row[0] for row in cursor.fetchall()]
    
    for user_id in user_ids:
        for round_num in range(1, 21):  # 20 rounds
            # More realistic budget usage (80-100) to ensure we can select 10 players
            used_budget = random.randint(80, 100)
            is_locked = random.choice([True, False])  # Random lock status
            
            cursor.execute("""
                INSERT INTO UserRoundTeams (UserId, Round, TotalBudget, UsedBudget, IsLocked)
                VALUES (?, ?, 100, ?, ?)
            """, (user_id, round_num, used_budget, is_locked))
    
    conn.commit()
    conn.close()
    print("Inserted UserRoundTeam records for all users and rounds")

def insert_fantasy_teams():
    """Insert fantasy team selections for all users and rounds with budget validation"""
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    # Get user IDs
    cursor.execute("SELECT Id FROM Users")
    user_ids = [row[0] for row in cursor.fetchall()]
    
    # Get players with their costs and positions
    cursor.execute("SELECT Id, Position, Cost FROM Players")
    players_data = cursor.fetchall()
    
    # Organize players by position
    players_by_position = {}
    for player_id, position, cost in players_data:
        if position not in players_by_position:
            players_by_position[position] = []
        players_by_position[position].append((player_id, cost))
    
    for user_id in user_ids:
        for round_num in range(1, 21):  # 20 rounds
            # Get the budget for this user/round
            cursor.execute("""
                SELECT UsedBudget FROM UserRoundTeams 
                WHERE UserId = ? AND Round = ?
            """, (user_id, round_num))
            result = cursor.fetchone()
            if not result:
                print(f"Warning: No budget found for user {user_id}, round {round_num}")
                continue
            budget = result[0]
            
            # Select players within budget
            selected_players = select_players_within_budget(players_by_position, budget)
            
            if len(selected_players) != 10:
                print(f"Warning: Only selected {len(selected_players)} players for user {user_id}, round {round_num}")
            
            # Assign players to court/bench with proper position distribution
            # Group players by position
            players_by_position_selected = {}
            for player_id, cost in selected_players:
                # Get player position
                cursor.execute("SELECT Position FROM Players WHERE Id = ?", (player_id,))
                position = cursor.fetchone()[0]
                
                if position not in players_by_position_selected:
                    players_by_position_selected[position] = []
                players_by_position_selected[position].append((player_id, cost))
            
            # Assign first player of each position to court, second to bench
            for position in POSITIONS:
                if position in players_by_position_selected:
                    players = players_by_position_selected[position]
                    # First player goes to court
                    cursor.execute("""
                        INSERT INTO FantasyTeams (UserId, PlayerId, Round, IsActive, IsOnCourt)
                        VALUES (?, ?, ?, 1, 1)
                    """, (user_id, players[0][0], round_num))
                    
                    # Second player goes to bench
                    if len(players) > 1:
                        cursor.execute("""
                            INSERT INTO FantasyTeams (UserId, PlayerId, Round, IsActive, IsOnCourt)
                            VALUES (?, ?, ?, 1, 0)
                        """, (user_id, players[1][0], round_num))
    
    conn.commit()
    conn.close()
    print("Inserted fantasy team selections for all users and rounds")

def select_players_within_budget(players_by_position, budget):
    """Select 2 players per position (10 total) within the given budget using greedy algorithm"""
    # First, calculate the minimum possible cost
    min_cost = 0
    for position in POSITIONS:
        available_players = players_by_position[position].copy()
        cheapest_players = sorted(available_players, key=lambda x: x[1])[:2]
        min_cost += sum(cost for _, cost in cheapest_players)
    
    # If budget is too small, adjust it to minimum cost
    if budget < min_cost:
        budget = min_cost
    
    # Start with the cheapest possible team
    selected_players = []
    for position in POSITIONS:
        available_players = players_by_position[position].copy()
        cheapest_players = sorted(available_players, key=lambda x: x[1])[:2]
        selected_players.extend(cheapest_players)
    
    total_cost = sum(cost for _, cost in selected_players)
    
    # Now try to upgrade players while staying within budget
    for position in POSITIONS:
        available_players = players_by_position[position].copy()
        random.shuffle(available_players)
        
        # Find current players for this position
        current_position_players = [p for p in selected_players if any(p[0] == ap[0] for ap in available_players)]
        
        # Try to upgrade each player in this position
        for i, current_player in enumerate(current_position_players):
            if i >= 2:  # Only 2 players per position
                break
                
            # Find a better player that fits in budget
            for new_player in available_players:
                if new_player[0] == current_player[0]:  # Skip same player
                    continue
                
                # Check if this player is already selected
                if any(p[0] == new_player[0] for p in selected_players):
                    continue
                
                # Calculate new total cost if we replace this player
                new_total_cost = total_cost - current_player[1] + new_player[1]
                
                if new_total_cost <= budget:
                    # Replace the player
                    selected_players = [p if p[0] != current_player[0] else new_player for p in selected_players]
                    total_cost = new_total_cost
                    break
    
    return selected_players

def insert_player_round_points():
    """Insert player points for all rounds with individual statistics"""
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    # Get all players
    cursor.execute("SELECT Id FROM Players")
    player_ids = [row[0] for row in cursor.fetchall()]
    
    for player_id in player_ids:
        total_fantasy_points = 0
        for round_num in range(1, 21):  # 20 rounds
            # Generate realistic basketball statistics
            points = random.randint(0, 35)  # Points scored
            rebounds = random.randint(0, 15)  # Rebounds
            assists = random.randint(0, 12)  # Assists
            steals = random.randint(0, 4)  # Steals
            blocks = random.randint(0, 5)  # Blocks
            turnovers = random.randint(0, 6)  # Turnovers
            team_win = random.choice([True, False])  # Team win/loss
            
            # Calculate fantasy points using the new scoring system
            fantasy_points = (points + rebounds + assists + 
                           steals * 2 + blocks * 2 - turnovers + 
                           (5 if team_win else -3))
            
            total_fantasy_points += fantasy_points
            
            # Set score as W or L
            score = "W" if team_win else "L"
            
            cursor.execute("""
                INSERT INTO PlayerRoundPoints (PlayerId, Round, Points, Rebounds, Assists, 
                                            Steals, Blocks, Turnovers, TeamWin, FantasyPoints, 
                                            Score, TotalPoints)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """, (player_id, round_num, points, rebounds, assists, 
                  steals, blocks, turnovers, 1 if team_win else 0, fantasy_points, 
                  score, fantasy_points))
        
        # Update player's total points (using fantasy points)
        cursor.execute("UPDATE Players SET TotalPoints = ? WHERE Id = ?", (total_fantasy_points, player_id))
    
    conn.commit()
    conn.close()
    print("Inserted player round points with individual statistics for all rounds")

def insert_user_round_points():
    """Insert user points for all rounds using new scoring system (starters + top 3 bench)"""
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    # Get user IDs
    cursor.execute("SELECT Id FROM Users")
    user_ids = [row[0] for row in cursor.fetchall()]
    
    for user_id in user_ids:
        total_points = 0
        for round_num in range(1, 21):  # 20 rounds
            # Get all players in the user's fantasy team for this round with their fantasy points
            cursor.execute("""
                SELECT ft.IsOnCourt, prp.FantasyPoints
                FROM FantasyTeams ft
                JOIN PlayerRoundPoints prp ON ft.PlayerId = prp.PlayerId
                WHERE ft.UserId = ? AND ft.Round = ? AND ft.IsActive = 1 AND prp.Round = ?
            """, (user_id, round_num, round_num))
            
            team_players = cursor.fetchall()
            
            if len(team_players) == 10:  # Ensure we have a complete team
                # Calculate score: all starters + top 3 bench players
                starters = [fp for is_on_court, fp in team_players if is_on_court]
                bench_players = [fp for is_on_court, fp in team_players if not is_on_court]
                
                # All starters count
                round_points = sum(starters)
                
                # Top 3 bench players count
                if len(bench_players) >= 3:
                    top_3_bench = sorted(bench_players, reverse=True)[:3]
                    round_points += sum(top_3_bench)
                else:
                    round_points += sum(bench_players)  # If less than 3 bench players
            else:
                round_points = 0
            
            total_points += round_points
            
            cursor.execute("""
                INSERT INTO UserRoundPoints (UserId, Round, Points)
                VALUES (?, ?, ?)
            """, (user_id, round_num, round_points))
        
        # Update user's total points
        cursor.execute("UPDATE Users SET TotalPoints = ? WHERE Id = ?", (total_points, user_id))
    
    conn.commit()
    conn.close()
    print("Inserted user round points using new scoring system (starters + top 3 bench)")

def create_current_round_table():
    """Create CurrentRound table and set current round to 21"""
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    # Create CurrentRound table if it doesn't exist
    cursor.execute("""
        CREATE TABLE IF NOT EXISTS CurrentRound (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            RoundNumber INTEGER NOT NULL
        )
    """)
    
    # Clear any existing data and insert current round
    cursor.execute("DELETE FROM CurrentRound")
    cursor.execute("INSERT INTO CurrentRound (RoundNumber) VALUES (21)")
    
    conn.commit()
    conn.close()
    print("Created CurrentRound table and set current round to 21")

def validate_fantasy_teams():
    """Validate that all fantasy teams have valid players and don't exceed budget"""
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    print("\n🔍 Validating fantasy teams...")
    
    # Get all fantasy teams with their costs
    cursor.execute("""
        SELECT ft.UserId, ft.Round, ft.PlayerId, p.Cost, p.Name, p.Position
        FROM FantasyTeams ft
        JOIN Players p ON ft.PlayerId = p.Id
        WHERE ft.IsActive = 1
        ORDER BY ft.UserId, ft.Round, ft.PlayerId
    """)
    
    fantasy_teams = cursor.fetchall()
    
    # Group by user and round
    user_round_teams = {}
    for user_id, round_num, player_id, cost, name, position in fantasy_teams:
        key = (user_id, round_num)
        if key not in user_round_teams:
            user_round_teams[key] = []
        user_round_teams[key].append((player_id, cost, name, position))
    
    # Validate each user/round combination
    validation_errors = []
    for (user_id, round_num), players in user_round_teams.items():
        # Check if we have exactly 10 players
        if len(players) != 10:
            validation_errors.append(f"User {user_id}, Round {round_num}: Expected 10 players, got {len(players)}")
        
        # Check position distribution (2 per position)
        position_counts = {}
        for player_id, cost, name, position in players:
            position_counts[position] = position_counts.get(position, 0) + 1
        
        for position in POSITIONS:
            if position_counts.get(position, 0) != 2:
                validation_errors.append(f"User {user_id}, Round {round_num}: Expected 2 {position} players, got {position_counts.get(position, 0)}")
        
        # Check total cost vs budget
        total_cost = sum(cost for player_id, cost, name, position in players)
        cursor.execute("""
            SELECT UsedBudget FROM UserRoundTeams 
            WHERE UserId = ? AND Round = ?
        """, (user_id, round_num))
        result = cursor.fetchone()
        if result:
            budget = result[0]
            if total_cost > budget:
                validation_errors.append(f"User {user_id}, Round {round_num}: Total cost {total_cost} exceeds budget {budget}")
    
    conn.close()
    
    if validation_errors:
        print("❌ Validation errors found:")
        for error in validation_errors:
            print(f"  • {error}")
        return False
    else:
        print("✅ All fantasy teams are valid!")
        print(f"  • {len(user_round_teams)} user/round combinations validated")
        print(f"  • All teams have exactly 10 players")
        print(f"  • All teams have correct position distribution (2 per position)")
        print(f"  • All teams are within budget")
        return True

def main():
    """Main function to populate database"""
    print("Starting comprehensive dummy data insertion...")
    print(f"Database: {DB_PATH}")
    print(f"Users: {len(USERS)}")
    print(f"Teams: {len(TEAMS)}")
    print(f"Total Players: {sum(len(players) for players in PLAYERS_BY_TEAM.values())}")
    print(f"Rounds: 20 (with current round set to 21)")
    print("-" * 50)
    
    # Wait for tables to be created by the .NET application
    wait_for_tables()
    
    # Clear existing data
    clear_database()
    
    # Insert data in dependency order
    insert_teams()
    insert_players()
    insert_users()
    insert_user_round_teams()
    insert_fantasy_teams()
    insert_player_round_points()
    insert_user_round_points()
    create_current_round_table()
    
    # Validate the data
    validation_success = validate_fantasy_teams()
    
    print("-" * 50)
    if validation_success:
        print("✅ Complete dummy data insertion finished!")
    else:
        print("⚠️ Dummy data insertion completed with validation warnings!")
    print("\n📊 Summary:")
    print(f"• {len(USERS)} users created")
    print(f"• {len(TEAMS)} teams created")
    print(f"• {sum(len(players) for players in PLAYERS_BY_TEAM.values())} players created")
    print(f"• 20 rounds of historical data created")
    print(f"• Current round set to 21")
    print(f"• Fantasy teams, points, and budgets populated")
    print("\n🔑 Login Credentials:")
    for user in USERS:
        print(f"• {user['email']} / {user['password']}")

if __name__ == "__main__":
    main()

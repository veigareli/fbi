#!/usr/bin/env python3
"""
Complete Fantasy Basketball Dummy Data Script - Docker Version
Creates comprehensive test data for 5 users across 22 rounds
"""

import sqlite3
import hashlib
import random
from datetime import datetime

# Database file path for Docker
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

# Generate 100 users + admin
USERS = []
# Add admin user first
USERS.append({
    "name": "Admin",
    "email": "admin@gmail.com", 
    "password": "admin123"
})
# Add regular users
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

def create_current_round_table():
    """Create CurrentRound table and set current round to 1"""
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
    cursor.execute("INSERT INTO CurrentRound (RoundNumber) VALUES (1)")
    
    conn.commit()
    conn.close()
    print("Created CurrentRound table and set current round to 1")

def main():
    """Main function to populate database"""
    print("Starting comprehensive dummy data insertion...")
    print(f"Database: {DB_PATH}")
    print(f"Users: {len(USERS)}")
    print(f"Teams: {len(TEAMS)}")
    print(f"Total Players: {sum(len(players) for players in PLAYERS_BY_TEAM.values())}")
    print(f"Rounds: 1 (current round set to 1)")
    print("-" * 50)
    
    # Wait for tables to be created by the .NET application
    wait_for_tables()
    
    # Clear existing data
    clear_database()
    
    # Insert data in dependency order
    insert_teams()
    insert_players()
    insert_users()
    create_current_round_table()
    
    print("-" * 50)
    print("✅ Complete dummy data insertion finished!")
    print("\n📊 Summary:")
    print(f"• {len(USERS)} users created")
    print(f"• {len(TEAMS)} teams created")
    print(f"• {sum(len(players) for players in PLAYERS_BY_TEAM.values())} players created")
    print(f"• Current round set to 1")
    print("\n🔑 Login Credentials:")
    print("• ADMIN: admin@gmail.com / admin123")
    print("• Regular users: user1@fantasy.com through user100@fantasy.com / user123")

if __name__ == "__main__":
    main()

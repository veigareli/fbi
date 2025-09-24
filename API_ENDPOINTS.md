# Fantasy Basketball Iceland - API Endpoints

## Base URL
`http://localhost:5062/api`

---

## 📊 **Users API** (`/api/users`)

### Basic CRUD Operations
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

### Additional Endpoints
- `GET /api/users/{id}/fantasy-teams` - Get user's fantasy teams
- `GET /api/users/{id}/round-points` - Get user's round points

---

## 🏀 **Teams API** (`/api/teams`)

### Basic CRUD Operations
- `GET /api/teams` - Get all teams (with players)
- `GET /api/teams/{id}` - Get team by ID (with players)
- `POST /api/teams` - Create new team
- `PUT /api/teams/{id}` - Update team
- `DELETE /api/teams/{id}` - Delete team

### Additional Endpoints
- `GET /api/teams/{id}/players` - Get all players for a team

---

## 👤 **Players API** (`/api/players`)

### Basic CRUD Operations
- `GET /api/players` - Get all players (with team info)
- `GET /api/players/{id}` - Get player by ID (with team info)
- `POST /api/players` - Create new player
- `PUT /api/players/{id}` - Update player
- `DELETE /api/players/{id}` - Delete player

### Additional Endpoints
- `GET /api/players/{id}/round-points` - Get player's round points
- `GET /api/players/by-position/{position}` - Get players by position
- `GET /api/players/by-team/{teamId}` - Get players by team

---

## 🎯 **Fantasy Teams API** (`/api/fantasyteams`)

### Basic CRUD Operations
- `GET /api/fantasyteams` - Get all fantasy teams
- `GET /api/fantasyteams/{id}` - Get fantasy team by ID
- `POST /api/fantasyteams` - Create new fantasy team entry
- `PUT /api/fantasyteams/{id}` - Update fantasy team entry
- `DELETE /api/fantasyteams/{id}` - Delete fantasy team entry

### Additional Endpoints
- `GET /api/fantasyteams/user/{userId}` - Get fantasy teams by user
- `GET /api/fantasyteams/user/{userId}/round/{round}` - Get user's team for specific round
- `GET /api/fantasyteams/round/{round}` - Get all fantasy teams for a round
- `POST /api/fantasyteams/bulk` - Create multiple fantasy team entries

---

## 📈 **Player Round Points API** (`/api/playerroundpoints`)

### Basic CRUD Operations
- `GET /api/playerroundpoints` - Get all player round points
- `GET /api/playerroundpoints/{id}` - Get player round point by ID
- `POST /api/playerroundpoints` - Create new player round point
- `PUT /api/playerroundpoints/{id}` - Update player round point
- `DELETE /api/playerroundpoints/{id}` - Delete player round point

### Additional Endpoints
- `GET /api/playerroundpoints/player/{playerId}` - Get points for specific player
- `GET /api/playerroundpoints/round/{round}` - Get all points for specific round
- `POST /api/playerroundpoints/bulk` - Create multiple player round points

---

## 🏆 **User Round Points API** (`/api/userroundpoints`)

### Basic CRUD Operations
- `GET /api/userroundpoints` - Get all user round points
- `GET /api/userroundpoints/{id}` - Get user round point by ID
- `POST /api/userroundpoints` - Create new user round point
- `PUT /api/userroundpoints/{id}` - Update user round point
- `DELETE /api/userroundpoints/{id}` - Delete user round point

### Additional Endpoints
- `GET /api/userroundpoints/user/{userId}` - Get points for specific user
- `GET /api/userroundpoints/round/{round}` - Get all points for specific round
- `POST /api/userroundpoints/bulk` - Create multiple user round points

---

## 📊 **Statistics API** (`/api/statistics`)

### Analytics Endpoints
- `GET /api/statistics/leaderboard` - Get overall leaderboard
- `GET /api/statistics/leaderboard/round/{round}` - Get leaderboard for specific round
- `GET /api/statistics/top-players` - Get top 20 players by total points
- `GET /api/statistics/player-stats/{playerId}` - Get detailed player statistics
- `GET /api/statistics/team-stats/{teamId}` - Get team statistics
- `GET /api/statistics/round-summary/{round}` - Get comprehensive round summary

---

## 🎮 **Fantasy Team Management API** (`/api/fantasyteammanagement`)

### Advanced Operations
- `POST /api/fantasyteammanagement/create-team` - Create complete fantasy team for user
- `POST /api/fantasyteammanagement/calculate-round-points` - Calculate and update round points
- `GET /api/fantasyteammanagement/user/{userId}/team-summary` - Get user's complete team summary
- `DELETE /api/fantasyteammanagement/user/{userId}/round/{round}` - Delete user's team for specific round

---

## 📝 **Example API Calls**

### Create a User
```bash
POST /api/users
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "passwordHash": "hashedpassword123",
  "totalPoints": 0
}
```

### Create a Team
```bash
POST /api/teams
Content-Type: application/json

{
  "name": "Los Angeles Lakers"
}
```

### Create a Player
```bash
POST /api/players
Content-Type: application/json

{
  "teamId": 1,
  "name": "LeBron James",
  "position": "SF",
  "cost": 100,
  "totalPoints": 0
}
```

### Create Fantasy Team
```bash
POST /api/fantasyteammanagement/create-team
Content-Type: application/json

{
  "userId": 1,
  "round": 1,
  "playerIds": [1, 2, 3, 4, 5]
}
```

### Add Player Round Points
```bash
POST /api/playerroundpoints
Content-Type: application/json

{
  "playerId": 1,
  "round": 1,
  "points": 25
}
```

---

## 🔧 **System Analysis - Additional Endpoints Added**

Based on the fantasy basketball system analysis, I've added these additional useful endpoints:

1. **Bulk Operations**: For creating multiple records at once
2. **Filtering & Search**: By position, team, user, round
3. **Statistics & Analytics**: Leaderboards, player stats, team stats
4. **Fantasy Team Management**: Complete team creation and point calculation
5. **Round Management**: Round-specific operations and summaries

These endpoints provide a complete API for managing a fantasy basketball league with all the necessary functionality for:
- User management
- Team and player management
- Fantasy team creation and management
- Point tracking and calculation
- Statistics and leaderboards
- Round management

Your API is now ready for frontend development! 🚀

using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace TaikoGameServer
{
public class GameHub : Hub
{
private static Dictionary<string, GameRoom> _rooms = new Dictionary<string, GameRoom>();

public async Task<bool> CreateRoom(string roomId, string playerName)
{
if (_rooms.ContainsKey(roomId)) return false;

_rooms[roomId] = new GameRoom { RoomId = roomId };
_rooms[roomId].Player1 = new PlayerInfo
{
Name = playerName,
ConnectionId = Context.ConnectionId,
PlayerNumber = 1
};

await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
await Clients.Caller.SendAsync("AssignedPlayerNumber", 1);
return true;
}

public async Task<(bool success, string message)> JoinRoom(string roomId, string playerName)
{
if (!_rooms.ContainsKey(roomId)) return (false, "房間不存在");

var room = _rooms[roomId];
if (room.Player2 != null) return (false, "房間已滿");
if (room.Player1 == null) return (false, "房主已斷線");

room.Player2 = new PlayerInfo
{
Name = playerName,
ConnectionId = Context.ConnectionId,
PlayerNumber = 2
};

await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
await Clients.Caller.SendAsync("AssignedPlayerNumber", 2);
await Clients.Group(roomId).SendAsync("PlayerJoined", playerName, 2);

if (room.Player1 != null)
await Clients.Client(room.Player1.ConnectionId).SendAsync("RemotePlayerJoined");

return (true, "成功加入");
}

public async Task<bool> StartGame(string roomId)
{
if (!_rooms.ContainsKey(roomId)) return false;
await Clients.Group(roomId).SendAsync("BeginSongSelection");
return true;
}

public async Task<bool> StartGameplay(string roomId, string songId, string p1Diff, string p2Diff, double p1Speed, double p2Speed)
{
if (!_rooms.ContainsKey(roomId)) return false;

await Clients.Group(roomId).SendAsync("StartGameplay",
songId, p1Diff, p2Diff, p1Speed, p2Speed);

return true;
}

public async Task AllowClientControl(string roomId, bool allow)
{
if (_rooms.ContainsKey(roomId))
await Clients.Group(roomId).SendAsync("AllowClientControl", allow);
}

public async Task SendScore(string roomId, int playerNumber, int score, int combo)
{
if (!_rooms.ContainsKey(roomId)) return;

var room = _rooms[roomId];
if (playerNumber == 1)
{
room.Player1Score = score;
room.Player1Combo = combo;
}
else if (playerNumber == 2)
{
room.Player2Score = score;
room.Player2Combo = combo;
}

await Clients.Group(roomId).SendAsync("ScoreUpdated",
room.Player1Score, room.Player2Score,
room.Player1Combo, room.Player2Combo);
}

public async Task UpdateScore(string roomId, int score, int combo, int perfect, int good, int bad, int miss)
{
int playerNum = GetPlayerNumber(Context.ConnectionId, roomId);
if (playerNum > 0)
{
await Clients.GroupExcept(roomId, Context.ConnectionId)
.SendAsync("OpponentStatsUpdated", playerNum, score, combo, perfect, good, bad, miss);
}
}
public async Task BroadcastJudgment(string roomId, int playerNumber, string judgment, string sound, bool isRed)
{
if (_rooms.ContainsKey(roomId))
{
await Clients.Group(roomId).SendAsync("JudgmentOccurred",
playerNumber, judgment, sound, isRed);
}
}

public async Task TogglePause(string roomId, bool isPaused)
{
if (_rooms.ContainsKey(roomId))
await Clients.Group(roomId).SendAsync("PauseStateChanged", isPaused);
}

public async Task NotifyGameEnded(string roomId, int winnerPlayerNumber)
{
if (_rooms.ContainsKey(roomId))
await Clients.Group(roomId).SendAsync("GameEnded", winnerPlayerNumber);
}

public async Task ResetRoom(string roomId)
{
if (!_rooms.ContainsKey(roomId)) return;

var room = _rooms[roomId];
room.Player1Score = 0;
room.Player2Score = 0;
room.Player1Combo = 0;
room.Player2Combo = 0;

await Clients.Group(roomId).SendAsync("RoomReset");
}

public async Task SendOpponentStats(string roomId, int misses, int goods, int perfects, int bads)
{
if (!_rooms.ContainsKey(roomId)) return;

await Clients.Group(roomId).SendAsync("OpponentStatsUpdated",
misses, goods, perfects, bads);
}

public override async Task OnDisconnectedAsync(Exception? exception)
{
foreach (var room in _rooms.Values)
{
if (room.Player1?.ConnectionId == Context.ConnectionId)
{
room.Player1 = null;
await Clients.Group(room.RoomId).SendAsync("PlayerLeft", 1);
}
if (room.Player2?.ConnectionId == Context.ConnectionId)
{
room.Player2 = null;
await Clients.Group(room.RoomId).SendAsync("PlayerLeft", 2);
}
}

await base.OnDisconnectedAsync(exception);
}

private int GetPlayerNumber(string connectionId, string roomId)
{
if (!_rooms.ContainsKey(roomId)) return 0;
var room = _rooms[roomId];

if (room.Player1?.ConnectionId == connectionId) return 1;
if (room.Player2?.ConnectionId == connectionId) return 2;
return 0;
}

public class GameRoom
{
public string RoomId { get; set; } = "";
public PlayerInfo? Player1 { get; set; }
public PlayerInfo? Player2 { get; set; }
public int Player1Score { get; set; }
public int Player2Score { get; set; }
public int Player1Combo { get; set; }
public int Player2Combo { get; set; }
}

public class PlayerInfo
{
public string Name { get; set; } = "";
public string ConnectionId { get; set; } = "";
public int PlayerNumber { get; set; }
}
}
}
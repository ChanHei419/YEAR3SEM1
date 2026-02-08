using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace TaikoGame
{
public class NetworkManager
{
private HubConnection? _connection;
private string _currentRoomId = "";
private int _assignedPlayerNumber = 0;

public event Action<int, string, string, bool>? OnJudgmentOccurred;
public event Action<bool, string>? OnJoinRoomResult;
public event Action<string>? OnPlayerJoined;
public event Action? OnRemotePlayerJoined;
public event Action<int, int, int, int>? OnScoreUpdated;
public event Action<int>? OnPlayerLeft;
public event Action<string>? OnConnectionStatusChanged;
public event Action? OnBeginSongSelection;
public event Action<string, string, string, double, double>? OnStartGameplay;
public event Action<bool>? OnAllowClientControl;
public event Action<int>? OnAssignedPlayerNumber;
public event Action<bool>? OnCreateRoomResult;
public event Action<double, bool, int>? OnNoteSpawned;
public event Action<int>? OnGameEnded;
public event Action? OnRoomReset;
public event Action<bool>? OnPauseStateChanged;
public event Action<int, int, int, int, int, int, int>? OnOpponentStatsUpdated;

public NetworkManager() { }

public async Task ConnectAsync(string serverUrl = "http://localhost:5000/gameHub")
{
try
{
_connection = new HubConnectionBuilder()
.WithUrl(serverUrl)
.WithAutomaticReconnect()
.Build();

SetupEventHandlers();

await _connection.StartAsync();
OnConnectionStatusChanged?.Invoke("✓ Connected to server");
Console.WriteLine("[Network] Connected to server");
}
catch (Exception ex)
{
OnConnectionStatusChanged?.Invoke($"✗ Connection failed: {ex.Message}");
Console.WriteLine($"[Network] Connection failed: {ex.Message}");
}
}

private void SetupEventHandlers()
{
if (_connection == null) return;

_connection.On<string, int>("PlayerJoined", (playerName, playerNumber) =>
{
Console.WriteLine($"[Network] Player {playerNumber} joined: {playerName}");
OnPlayerJoined?.Invoke(playerName);
});

_connection.On<int, int, int, int>("ScoreUpdated", (p1Score, p2Score, p1Combo, p2Combo) =>
{
OnScoreUpdated?.Invoke(p1Score, p2Score, p1Combo, p2Combo);
});

_connection.On<int>("PlayerLeft", (playerNumber) =>
{
Console.WriteLine($"[Network] Player {playerNumber} left");
OnPlayerLeft?.Invoke(playerNumber);
});

_connection.On("BeginSongSelection", () =>
{
Console.WriteLine("[Network] BeginSongSelection");
OnBeginSongSelection?.Invoke();
});

_connection.On<string, string, string, double, double>("StartGameplay", (songId, p1Diff, p2Diff, p1Speed, p2Speed) =>
{
Console.WriteLine($"[Network] StartGameplay: {songId}, P1:{p1Diff}, P2:{p2Diff}, P1Speed:{p1Speed}, P2Speed:{p2Speed}");
OnStartGameplay?.Invoke(songId, p1Diff, p2Diff, p1Speed, p2Speed);
});

_connection.On<bool>("AllowClientControl", (allow) =>
{
OnAllowClientControl?.Invoke(allow);
});

_connection.On<int>("AssignedPlayerNumber", (num) =>
{
_assignedPlayerNumber = num;
Console.WriteLine($"[Network] Assigned as Player {num}");
OnAssignedPlayerNumber?.Invoke(num);
});

_connection.On<bool>("CreateRoomResult", (success) =>
{
Console.WriteLine($"[Network] CreateRoomResult: {success}");
OnCreateRoomResult?.Invoke(success);
});

_connection.On("RemotePlayerJoined", () =>
{
Console.WriteLine("[Network] Remote player joined");
OnRemotePlayerJoined?.Invoke();
});

_connection.On<double, bool>("NoteSpawnedFromHost", (time, isRed) => OnNoteSpawned?.Invoke(time, isRed, 1));
_connection.On<double, bool>("NoteSpawnedFromClient", (time, isRed) => OnNoteSpawned?.Invoke(time, isRed, 2));

_connection.On<int>("GameEnded", (winner) => OnGameEnded?.Invoke(winner));
_connection.On("RoomReset", () => OnRoomReset?.Invoke());

_connection.On<int, string, string, bool>("JudgmentOccurred", (playerNumber, judgment, sound, isRed) =>
{
Console.WriteLine($"[Network] Judgment received: P{playerNumber} {judgment}");
OnJudgmentOccurred?.Invoke(playerNumber, judgment, sound, isRed);
});

_connection.On<bool>("PauseStateChanged", (isPaused) =>
{
Console.WriteLine($"[Network] Pause State Changed: {isPaused}");
OnPauseStateChanged?.Invoke(isPaused);
});

_connection.On<int, int, int, int, int, int, int>("OpponentStatsUpdated", (playerNum, score, combo, perfect, good, bad, miss) =>
{
Console.WriteLine($"[Network] Opponent stats: P{playerNum}, Score:{score}, Combo:{combo}, P:{perfect}, G:{good}, B:{bad}, M:{miss}");
OnOpponentStatsUpdated?.Invoke(playerNum, score, combo, perfect, good, bad, miss);
});

		_connection.Reconnecting += error =>
		{
			Console.WriteLine("[Network] Reconnecting...");
			OnConnectionStatusChanged?.Invoke("...Reconnecting to server...");
			return Task.CompletedTask;
		};

		_connection.Closed += async error =>
		{
			Console.WriteLine("[Network] Connection Closed");
			OnConnectionStatusChanged?.Invoke("✗ Disconnected from server");
			await Task.CompletedTask;
		};
}

public async Task<bool> CreateRoomAsync(string roomId, string playerName)
{
var conn = _connection;
if (conn == null || conn.State != HubConnectionState.Connected) return false;
_currentRoomId = roomId;
return await conn.InvokeAsync<bool>("CreateRoom", roomId, playerName);
}

public async Task JoinRoomAsync(string roomId, string playerName)
{
var conn = _connection;
if (conn == null || conn.State != HubConnectionState.Connected) return;
_currentRoomId = roomId;
var result = await conn.InvokeAsync<(bool success, string message)>("JoinRoom", roomId, playerName);
OnJoinRoomResult?.Invoke(result.success, result.message);
}

public async Task SendScoreAsync(int pNum, int score, int combo)
{
var conn = _connection;
if (conn == null || conn.State != HubConnectionState.Connected) return;
await conn.InvokeAsync("SendScore", _currentRoomId, pNum, score, combo);
}

public async Task SendScoreUpdateAsync(int score, int combo, int perfect, int good, int bad, int miss)
{
if (_connection != null && _connection.State == HubConnectionState.Connected)
{
await _connection.InvokeAsync("UpdateScore", _currentRoomId, score, combo, perfect, good, bad, miss);
}
}

public async Task SendJudgmentAsync(int playerNumber, string judgment, string sound, bool isRed)
{
var conn = _connection;
if (conn == null || conn.State != HubConnectionState.Connected) return;
Console.WriteLine($"[Network] Sending Judgment: P{playerNumber} {judgment}");
await conn.InvokeAsync("BroadcastJudgment", _currentRoomId, playerNumber, judgment, sound, isRed);
}

public async Task SendPauseStateAsync(bool isPaused)
{
var conn = _connection;
if (conn == null || conn.State != HubConnectionState.Connected) return;
Console.WriteLine($"[Network] Sending Pause: {isPaused}");
await conn.InvokeAsync("TogglePause", _currentRoomId, isPaused);
}

public async Task SendOpponentStatsAsync(int misses, int goods, int perfects, int bads)
{
var conn = _connection;
if (conn == null || conn.State != HubConnectionState.Connected) return;
Console.WriteLine($"[Network] Sending opponent stats: Misses={misses}, Goods={goods}, Perfects={perfects}, Bads={bads}");
await conn.InvokeAsync("SendOpponentStats", _currentRoomId, misses, goods, perfects, bads);
}

public async Task RequestRoomResetAsync()
{
try
{
var conn = _connection;
if (conn != null)
await conn.InvokeAsync("ResetRoom", _currentRoomId);
}
catch { }
}

public async Task<bool> RequestStartGameAsync()
{
try
{
var conn = _connection;
if (conn == null) return false;
return await conn.InvokeAsync<bool>("StartGame", _currentRoomId);
}
catch { return false; }
}

public async Task<bool> RequestStartGameplayAsync(string songId, string p1Difficulty, string p2Difficulty, double p1Speed, double p2Speed)
{
try
{
var conn = _connection;
if (conn == null) return false;
return await conn.InvokeAsync<bool>("StartGameplay", _currentRoomId, songId, p1Difficulty, p2Difficulty, p1Speed, p2Speed);
}
catch { return false; }
}

public async Task AllowClientControlAsync(bool allow)
{
try
{
var conn = _connection;
if (conn != null)
await conn.InvokeAsync("AllowClientControl", _currentRoomId, allow);
}
catch { }
}

public async Task DisconnectAsync()
{
if (_connection != null)
await _connection.StopAsync();
}

public string CurrentRoomId => _currentRoomId;
public int AssignedPlayerNumber => _assignedPlayerNumber;
public bool IsConnected => _connection?.State == HubConnectionState.Connected;
}
}
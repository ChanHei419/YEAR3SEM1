using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Effects;

namespace TaikoGame
{
public class PlayerSettings
{
public int PlayerNumber { get; set; } = 1;

public Key RedLeft { get; set; } = Key.F;
public Key RedRight { get; set; } = Key.J;
public Key BlueLeft { get; set; } = Key.D;
public Key BlueRight { get; set; } = Key.K;


public double ScrollSpeedMultiplier { get; set; } = 1.0;
public string Difficulty { get; set; } = "Normal";

public int Score { get; set; }
public int Combo { get; set; }
public int PerfectCount { get; set; }
public int GoodCount { get; set; }
public int BadCount { get; set; }
public int MissCount { get; set; }
public int TotalNotes { get; set; }
}
public class Note
{
public required UIElement UIElement { get; set; }
public bool IsRed { get; set; }
public bool IsHit { get; set; } = false;
public int PlayerNumber { get; set; } = 1;
}
public partial class MainWindow : Window
{
private enum GameState
{
Title,
SongSelection,
DifficultySelection,
PlayerSettings,
Playing,
Paused,
GameOver
}
private GameState _currentState;
private NetworkManager? _networkManager;
private bool _isOnlineMode = false;
private bool _isHost = false;
private bool _clientControlAllowed = false;
private System.Windows.Controls.Button? _hostStartButton;
private string _currentRoomId = "";
private const double BaseNoteSpeed = 6.0;
private bool _isTwoPlayerMode = false;
private PlayerSettings _player1 = new PlayerSettings { PlayerNumber = 1 };
private PlayerSettings _player2 = new PlayerSettings { PlayerNumber = 2 };
private int _currentSettingPlayerNumber = 1;
private double _player1TargetX = 70;
private double _player1TargetY = 100;
private double _player2TargetX = 70;
private double _player2TargetY = 100;
private List<Note> _player1Notes = new List<Note>();
private List<Note> _player2Notes = new List<Note>();
private bool _isGameRunning = false;
private DispatcherTimer? _gameTimer;
private MediaPlayer? _musicPlayer;
private MediaPlayer? _sfxPlayer;
private DispatcherTimer? _drumAnimationTimer;
private BackgroundParticleSystem? _particleSystem;
private FloatingImageParticleSystem? _floatingImageSystem;
private TextBlock? _p1StatusText;
private TextBlock? _p2StatusText;
private TextBlock? _tutorialP1Text;
private TextBlock? _tutorialP2Text;
private TextBlock TutorialP1 => _tutorialP1Text!;
private TextBlock TutorialP2 => _tutorialP2Text!;
private DispatcherTimer? _tutorialGlowTimer;
private double _songStartTime = 0;
private double _gameDuration = 30.0;
private double _currentGameTime = 0;
private double _pausedTime = 0;
private Ellipse? _backgroundCircle;
private double _circlePosX = 0;
private double _circleSpeed = 2.0;
private int _currentBackgroundColor = 0;
private int _currentPhotoIndex = 0; private System.Windows.Shapes.Path? _drumLeftRedD;
private System.Windows.Shapes.Path? _drumLeftRedF;
private System.Windows.Shapes.Path? _drumRightBlueK;
private System.Windows.Shapes.Path? _drumRightBlueD;
private System.Windows.Shapes.Path? _drum2LeftRedD;
private System.Windows.Shapes.Path? _drum2LeftRedF;
private System.Windows.Shapes.Path? _drum2RightBlueK;
private System.Windows.Shapes.Path? _drum2RightBlueD;
private List<Color> _bgColors = new List<Color>
{
Color.FromArgb(30, 255, 100, 100),
Color.FromArgb(30, 100, 150, 255),
Color.FromArgb(30, 100, 255, 150),
Color.FromArgb(30, 255, 200, 100),
Color.FromArgb(30, 200, 100, 255)
};

private Color _currentBgColor;
private Color _targetBgColor;
private double _bgColorTransition = 0;
private const double BgColorTransitionSpeed = 0.015;
private Rectangle? _backgroundRect;
private Image? _backgroundImage;
private Rectangle? _dividerLine;
private Random _random = new Random();
private double _currentScrollSpeed = 1.0;
private string _currentSongId = "song1";
private List<(double time, bool isRed)> _songNotes = new List<(double, bool)>();
private int _nextNoteIndex = 0; private List<(double time, bool isRed)> _songNotesP1 = new List<(double, bool)>();
private List<(double time, bool isRed)> _songNotesP2 = new List<(double, bool)>();
private int _nextNoteIndexP1 = 0;
private int _nextNoteIndexP2 = 0;
private Dictionary<string, (string filename, double duration, string title, string artist)> _songs =
new Dictionary<string, (string, double, string, string)>
{
{ "song1", ("1.wav", 173.15, "阿修羅ちゃん", "Ado") },
{ "song2", ("2.wav", 96.71, "千本桜", "初音ミク") }
};
private Dictionary<string, List<(double time, bool isRed)>> _easyNotes = new Dictionary<string, List<(double time, bool isRed)>>();
private Dictionary<string, List<(double time, bool isRed)>> _normalNotes = new Dictionary<string, List<(double time, bool isRed)>>();
private Dictionary<string, List<(double time, bool isRed)>> _hardNotes = new Dictionary<string, List<(double time, bool isRed)>>();

public MainWindow()
{
InitializeComponent();
this.KeyDown += Window_KeyDown;
InitializeGame();

this.Loaded += (s, e) =>
{
this.Focus();
Keyboard.Focus(this);
};

this.Closing += async (s, e) =>
{
if (_networkManager != null)
{
await _networkManager.DisconnectAsync();
}
};
}
private void InitializeGame()
{
_currentRoomId = GenerateRoomId();

_gameTimer = new DispatcherTimer();
_gameTimer.Interval = TimeSpan.FromMilliseconds(16);
_gameTimer.Tick += GameLoop;

_drumAnimationTimer = new DispatcherTimer();
_drumAnimationTimer.Interval = TimeSpan.FromMilliseconds(16);
_drumAnimationTimer.Tick += DrumAnimationLoop;

_musicPlayer = new MediaPlayer();
_sfxPlayer = new MediaPlayer();

InitializeDifficultyNotes();
InitializeNetwork();

this.Loaded += (s, e) =>
{
ShowTitleScreen();
DisplayRoomId();
};
}
private void InitializeNetwork()
{
_networkManager = new NetworkManager();
_networkManager.OnJoinRoomResult += (success, message) =>
{
Dispatcher.Invoke(() =>
{
if (success)
{
Console.WriteLine($"[UI] Join Success: {message}");
_isTwoPlayerMode = true;
ShowSongSelection();
}
else
{
MessageBox.Show($"✗ {message}");
}
});
};
_networkManager.OnPlayerJoined += (playerName) =>
Dispatcher.Invoke(() => Console.WriteLine($"[UI] Player Joined: {playerName}"));

_networkManager.OnRemotePlayerJoined += () =>
{
Dispatcher.Invoke(() =>
{
if (_hostStartButton != null)
{
_hostStartButton.IsEnabled = true;
_hostStartButton.Background = Brushes.LimeGreen;
}
Console.WriteLine("[UI] Remote Player Joined - Ready");
});
};
_networkManager.OnBeginSongSelection += () =>
Dispatcher.Invoke(() => ShowSongSelection());



_networkManager.OnStartGameplay += (songId, p1Diff, p2Diff, p1Speed, p2Speed) =>
{
Dispatcher.Invoke(() =>
{

_player1.Difficulty = p1Diff;
_player2.Difficulty = p2Diff;




_player1.ScrollSpeedMultiplier = p1Speed;
_player2.ScrollSpeedMultiplier = p2Speed;


double mySpeed = (_networkManager.AssignedPlayerNumber == 1) ? p1Speed : p2Speed;
_currentScrollSpeed = mySpeed;


StartGame(songId);
});
}; _networkManager.OnAllowClientControl += (allow) =>
Dispatcher.Invoke(() => _clientControlAllowed = allow);
_networkManager.OnAssignedPlayerNumber += (num) =>
Dispatcher.Invoke(() =>
{
Console.WriteLine($"[UI] I am Player {num}");
_isHost = (num == 1);
});
_networkManager.OnScoreUpdated += (p1Score, p2Score, p1Combo, p2Combo) =>
{
Dispatcher.Invoke(() =>
{
_player1.Score = p1Score;
_player2.Score = p2Score;
_player1.Combo = p1Combo;
_player2.Combo = p2Combo;
UpdateUI();
});
};

_networkManager.OnOpponentStatsUpdated += (playerNum, score, combo, perfect, good, bad, miss) =>
{
Dispatcher.Invoke(() =>
{

var targetPlayer = (playerNum == 1) ? _player1 : _player2;

targetPlayer.Score = score;
targetPlayer.Combo = combo;
targetPlayer.PerfectCount = perfect;
targetPlayer.GoodCount = good;
targetPlayer.BadCount = bad;
targetPlayer.MissCount = miss;


UpdateScoreDisplay();
});
};
_networkManager.OnJudgmentOccurred += (playerNum, judgment, sound, isRed) =>
{
Dispatcher.Invoke(() =>
{
Console.WriteLine($"[UI] Judgment Received: P{playerNum} {judgment}");


if (playerNum == _networkManager.AssignedPlayerNumber)
return;


ShowJudgment(judgment, GetJudgmentColor(judgment), playerNum);
ForceRemoveNote(playerNum, isRed);


if (!string.IsNullOrEmpty(sound))
PlaySound(sound, false);
});
};


_networkManager.OnPauseStateChanged += (isPaused) =>
{
Dispatcher.Invoke(() =>
{
Console.WriteLine($"[UI] Network Pause Signal: {isPaused}");
if (isPaused)
ShowPauseMenu(false);
else
ResumePause(false);
});
};



_networkManager.OnGameEnded += (winner) =>
Dispatcher.Invoke(() =>
{
_isGameRunning = false;
ShowGameOverScreen();
});


_networkManager.OnRoomReset += () =>
Dispatcher.Invoke(() =>
{
ResetGameData();
ShowSongSelection();
});
_networkManager.OnPlayerLeft += (playerNumber) =>
Dispatcher.Invoke(() =>
{
Console.WriteLine($"[UI] Player {playerNumber} left");
MessageBox.Show($"Player {playerNumber} left the room. The match will end.");
if (_isGameRunning)
{
_isGameRunning = false;
ShowGameOverScreen();
}
});

_networkManager.OnConnectionStatusChanged += (msg) =>
Dispatcher.Invoke(() =>
{
Console.WriteLine($"[UI] Connection status: {msg}");

if (!string.IsNullOrEmpty(msg))
{
MessageBox.Show(msg);
}
if (msg.Contains("Disconnected") && _isGameRunning)
{
_isGameRunning = false;
ShowGameOverScreen();
}
});
}

private string GenerateRoomId()
{
int randomNum = _random.Next(10000, 99999);
string roomId = $"ROOM-{randomNum}";
_currentRoomId = roomId;
return roomId;
}

private void DisplayRoomId()
{
if (GameCanvas.FindName("RoomIdText") is TextBlock roomIdText)
{
roomIdText.Text = $"Room ID: {_currentRoomId}";
}
}

private void OnHitJudgment(PlayerSettings playerSettings, int playerNumber)
{

if (_isOnlineMode && _networkManager != null && _networkManager.IsConnected)
{
_ = _networkManager.SendScoreUpdateAsync(
playerSettings.Score,
playerSettings.Combo,
playerSettings.PerfectCount,
playerSettings.GoodCount,
playerSettings.BadCount,
playerSettings.MissCount
);
}
}


private void InitializeDifficultyNotes()
{

_easyNotes["song1"] = new List<(double, bool)>
{
(1.342, false), (3.429, false), (5.255, false), (7.081, false),
(8.8, true), (10.056, false), (11.311, false), (12.567, true),
(13.823, true), (15.08, false), (16.336, false), (17.591, true),
(19.137, false), (20.507, false), (20.963, true), (22.789, true),
(24.615, true), (26.441, true), (29.523, false), (31.235, true),
(33.404, true), (35.23, false), (37.399, true), (39.225, true),
(46.302, false), (48.014, true), (50.183, true), (52.009, true),
(54.178, true), (56.69, true), (58.403, true), (60.915, false),
(63.427, true), (65.596, false), (68.908, true), (70.165, false),
(72.677, true), (74.846, false), (77.359, false), (79.414, false),
(81.926, true), (84.095, false), (86.151, true), (88.663, false),
(91.175, true), (93.344, false), (95.857, false), (98.369, true),
(100.881, true), (103.393, false), (106.362, false), (108.874, false),
(111.386, true), (113.898, true), (116.41, false), (119.379, true),
(122.348, true), (124.86, false), (127.372, false), (129.884, true),
(132.396, true), (134.908, false), (137.877, true), (140.389, false),
(142.901, true), (145.413, false), (148.382, true), (150.894, true),
(153.406, false), (156.375, true), (159.344, true), (162.655, false),
(165.624, true), (168.593, true), (171.105, false), (173.15, false)
};

_normalNotes["song1"] = GetSong1NormalNotes();
_hardNotes["song1"] = GetSong1HardNotes();

_easyNotes["song2"] = new List<(double, bool)>
{
(0.39, true), (1.17, false), (1.95, true), (2.73, false),
(3.51, true), (4.29, false), (5.07, false), (6.24, false),
(7.02, true), (8.19, true), (10.14, false), (12.09, true),
(14.04, false), (15.60, true), (17.16, true), (18.33, false),
(20.28, true), (22.23, true), (24.18, false), (26.13, false),
(28.08, true), (30.42, false), (32.37, false), (34.32, true),
(37.44, false), (40.16, true), (42.50, false), (45.23, true),
(47.96, true), (50.69, false), (53.42, true), (56.15, false),
(58.88, true), (61.22, true), (64.34, false), (67.07, true),
(70.19, true), (72.92, false), (75.65, true), (78.38, false),
(80.72, true), (83.45, false), (86.18, false), (88.91, false),
(91.64, true), (94.37, true), (96.71, true)
};

_normalNotes["song2"] = GetSong2NormalNotes();
_hardNotes["song2"] = GetSong2HardNotes();
}

private List<(double time, bool isRed)> GetSong1NormalNotes()
{
return new List<(double, bool)>
{
(1.342, false), (3.429, false), (3.886, false), (4.342, false), (4.799, false),
(5.255, false), (5.712, false), (6.168, false), (6.625, false),
(7.081, false), (7.538, false), (8.8, true), (9.257, false),
(10.056, false), (10.512, false), (11.311, false), (11.768, false),
(12.567, true), (13.024, false), (13.823, true), (14.28, false),
(15.08, false), (15.536, false), (16.336, false), (16.792, false),
(17.591, true), (18.048, false),
(19.137, false), (19.594, false), (20.05, false),
(20.507, false), (20.963, true), (21.42, false), (21.876, false), (22.333, false),
(22.789, true), (23.246, false), (23.702, true), (24.159, false),
(24.615, true), (25.072, false), (25.528, true), (25.985, false),
(26.441, true), (26.898, false),
(27.697, false), (28.153, false), (28.61, true), (29.066, true), (29.523, false), (29.979, true),
(30.436, false), (31.235, true), (31.692, true), (32.148, false), (32.605, false),
(33.404, true), (33.861, false), (34.317, true), (34.774, true), (35.23, false), (35.687, true),
(36.143, false), (36.6, false), (37.399, true), (37.856, false), (38.312, true), (38.769, true),
(39.225, true), (39.682, false), (40.138, false), (40.595, false),
(44.476, true), (44.932, false), (45.389, true), (45.845, true), (46.302, false), (46.758, true),
(47.215, false), (48.014, true), (48.471, true), (48.927, false), (49.384, false),
(50.183, true), (50.64, false), (51.096, true), (51.553, true), (52.009, true),
(52.466, false), (52.922, false), (53.379, false), (54.178, true), (54.635, false),
(55.434, true), (55.891, false), (56.69, true), (57.147, false), (57.603, true),
(58.403, true), (58.86, false), (59.659, true), (60.116, false), (60.915, false),
(61.372, true), (62.171, false), (62.628, false), (63.427, true), (63.884, true),
(64.683, true), (65.14, true), (65.596, false), (66.396, false), (66.853, true),
(67.652, true), (68.109, false), (68.908, true), (69.365, false), (70.165, false),
(70.622, true), (71.421, false), (71.878, false), (72.677, true), (73.134, true),
(73.933, true), (74.39, true), (74.846, false), (75.646, false), (76.103, true),
(76.902, true), (77.359, false), (78.158, true), (78.615, false), (79.414, false),
(79.871, true), (80.67, false), (81.127, false), (81.926, true), (82.383, true),
(83.182, true), (83.639, true), (84.095, false), (84.895, false), (85.352, true),
(86.151, true), (86.608, false), (87.407, true), (87.864, false), (88.663, false),
(89.12, true), (89.919, false), (90.376, false), (91.175, true), (91.632, true),
(92.431, true), (92.888, true), (93.344, false), (94.144, false), (94.601, true),
(95.4, true), (95.857, false), (96.656, true), (97.113, false), (97.912, false),
(98.369, true), (99.168, false), (99.625, false), (100.424, true), (100.881, true),
(101.68, true), (102.137, true), (102.593, false), (103.393, false), (103.85, true),
(104.649, true), (105.106, false), (105.905, true), (106.362, false), (107.161, false),
(107.618, true), (108.417, false), (108.874, false), (109.673, true), (110.13, true),
(110.929, true), (111.386, true), (111.842, false), (112.642, false), (113.099, true),
(113.898, true), (114.355, false), (115.154, true), (115.611, false), (116.41, false),
(116.867, true), (117.666, false), (118.123, false), (118.922, true), (119.379, true),
(120.178, true), (120.635, true), (121.091, false), (121.891, false), (122.348, true),
(123.147, true), (123.604, false), (124.403, true), (124.86, false), (125.659, false),
(126.116, true), (126.915, false), (127.372, false), (128.171, true), (128.628, true),
(129.427, true), (129.884, true), (130.34, false), (131.14, false), (131.597, true),
(132.396, true), (132.853, false), (133.652, true), (134.109, false), (134.908, false),
(135.365, true), (136.164, false), (136.621, false), (137.42, true), (137.877, true),
(138.676, true), (139.133, true), (139.589, false), (140.389, false), (140.846, true),
(141.645, true), (142.102, false), (142.901, true), (143.358, false), (144.157, false),
(144.614, true), (145.413, false), (145.87, false), (146.669, true), (147.126, true),
(147.925, true), (148.382, true), (148.838, false), (149.638, false), (150.095, true),
(150.894, true), (151.351, false), (152.15, true), (152.607, false), (153.406, false),
(153.863, true), (154.662, false), (155.119, false), (155.918, true), (156.375, true),
(157.174, true), (157.631, true), (158.087, false), (158.887, false), (159.344, true),
(160.143, true), (160.6, false), (161.399, true), (161.856, false), (162.655, false),
(163.112, true), (163.911, false), (164.368, false), (165.167, true), (165.624, true),
(166.423, true), (166.88, true), (167.336, false), (168.136, false), (168.593, true),
(169.392, true), (169.849, false), (170.648, true), (171.105, false), (171.904, false),
(172.361, true), (173.15, false)
};
}

private List<(double time, bool isRed)> GetSong1HardNotes()
{
var normalNotes = GetSong1NormalNotes();
var hardNotes = new List<(double, bool)>(normalNotes);
hardNotes.Add((2.0, true));
hardNotes.Add((6.9, false));
hardNotes.Add((9.5, true));
hardNotes.Sort((a, b) => a.Item1.CompareTo(b.Item1));
return hardNotes;
}

private List<(double time, bool isRed)> GetSong2NormalNotes()
{
return new List<(double, bool)>
{
(0.39, true), (0.78, true), (1.17, false), (1.56, true), (1.95, true), (2.34, true), (2.73, false), (3.12, true),
(3.51, true), (3.90, true), (4.29, false), (4.68, true), (5.07, false), (5.46, false), (5.85, true), (6.24, false),
(6.63, false), (7.02, true), (7.41, false), (7.80, false), (8.19, true), (8.58, false), (8.97, true), (9.36, true),
(9.75, true), (10.14, false), (10.53, false), (10.92, true), (11.31, false), (11.70, false), (12.09, true), (12.48, false),
(12.87, true), (13.26, true), (13.65, true), (14.04, false), (14.43, false), (14.82, true), (15.21, false), (15.60, true),
(15.99, true), (16.38, false), (16.77, false), (17.16, true), (17.55, false), (17.94, true), (18.33, false), (18.72, true),
(19.11, false), (19.50, false), (19.89, true), (20.28, true), (20.67, true), (21.06, false), (21.45, true), (21.84, false),
(22.23, true), (22.62, true), (23.01, true), (23.40, true), (23.79, true), (24.18, false), (24.57, false), (24.96, true),
(25.35, false), (25.74, true), (26.13, false), (26.52, true), (26.91, false), (27.30, true), (27.69, false), (28.08, true),
(28.47, true), (28.86, true), (29.25, false), (29.64, true), (30.03, false), (30.42, false), (30.81, true), (31.20, false),
(31.59, false), (31.98, true), (32.37, false), (32.76, false), (33.15, true), (33.54, false), (33.93, true), (34.32, true),
(34.71, false), (35.10, false), (35.49, true), (35.88, false), (36.27, true), (36.66, false), (37.05, true), (37.44, false),
(37.83, false), (38.22, true), (38.61, true), (38.99, true), (39.38, false), (39.77, true), (40.16, false), (40.55, true),
(40.94, true), (41.33, true), (41.72, false), (42.11, true), (42.50, false), (42.89, true), (43.28, false), (43.67, true),
(44.06, false), (44.45, false), (44.84, true), (45.23, true), (45.62, true), (46.01, false), (46.40, true), (46.79, false),
(47.18, true), (47.57, true), (47.96, true), (48.35, true), (48.74, false), (49.13, false), (49.52, true), (49.91, false),
(50.30, true), (50.69, false), (51.08, true), (51.47, false), (51.86, false), (52.25, true), (52.64, false), (53.03, false),
(53.42, true), (53.81, false), (54.20, true), (54.59, true), (54.98, false), (55.37, false), (55.76, true), (56.15, false),
(56.54, true), (56.93, false), (57.32, true), (57.71, false), (58.10, false), (58.49, true), (58.88, true), (59.27, true),
(59.66, false), (60.05, true), (60.44, false), (60.83, true), (61.22, true), (61.61, false), (62.00, false), (62.39, true),
(62.78, true), (63.17, true), (63.56, false), (63.95, true), (64.34, false), (64.73, true), (65.12, false), (65.51, true),
(65.90, false), (66.29, false), (66.68, true), (67.07, true), (67.46, true), (67.85, false), (68.24, true), (68.63, false),
(69.02, true), (69.41, true), (69.80, true), (70.19, true), (70.58, true), (70.97, false), (71.36, false), (71.75, true),
(72.14, false), (72.53, true), (72.92, false), (73.31, true), (73.70, false), (74.09, false), (74.48, true), (74.87, false),
(75.26, false), (75.65, true), (76.04, false), (76.43, true), (76.82, true), (77.21, false), (77.60, false), (77.99, true),
(78.38, false), (78.77, true), (79.16, false), (79.55, true), (79.94, false), (80.33, false), (80.72, true), (81.11, true),
(81.50, true), (81.89, false), (82.28, true), (82.67, false), (83.06, true), (83.45, true), (83.84, true), (84.23, true),
(84.62, false), (85.01, false), (85.40, true), (85.79, false), (86.18, true), (86.57, false), (86.96, true), (87.35, false),
(87.74, false), (88.13, true), (88.52, false), (88.91, false), (89.30, true), (89.69, false), (90.08, true), (90.47, true),
(90.86, false), (91.25, false), (91.64, true), (92.03, false), (92.42, true), (92.81, false), (93.20, true), (93.59, false),
(93.98, false), (94.37, true), (94.76, true), (95.15, true), (95.54, false), (95.93, true), (96.32, false), (96.71, true)
};
}

private List<(double time, bool isRed)> GetSong2HardNotes()
{
var normalNotes = GetSong2NormalNotes();
var hardNotes = new List<(double, bool)>(normalNotes);
hardNotes.Add((0.5, false));
hardNotes.Add((2.0, false));
hardNotes.Add((4.0, true));
hardNotes.Sort((a, b) => a.Item1.CompareTo(b.Item1));
return hardNotes;
}




private void ShowTitleScreen()
{
_currentState = GameState.Title;


_currentRoomId = GenerateRoomId();

if (_gameTimer != null) _gameTimer.Stop();
if (_musicPlayer != null) _musicPlayer.Stop();
if (_drumAnimationTimer != null) _drumAnimationTimer.Stop();
if (_particleSystem != null) _particleSystem.Stop();

GameCanvas.Children.Clear();
if (TopRightPanel != null) TopRightPanel.Visibility = Visibility.Collapsed;
JudgmentText.Visibility = Visibility.Collapsed;
ProgressBar.Visibility = Visibility.Collapsed;
ProgressText.Visibility = Visibility.Collapsed;
if (_tutorialP1Text != null) _tutorialP1Text.Visibility = Visibility.Collapsed;
if (_tutorialP2Text != null) _tutorialP2Text.Visibility = Visibility.Collapsed;

StackPanel titlePanel = new StackPanel
{
HorizontalAlignment = HorizontalAlignment.Center,
VerticalAlignment = VerticalAlignment.Center,
Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0))
};

TextBlock titleText = new TextBlock
{
Text = "TAIKO GAME",
FontSize = 72,
FontWeight = FontWeights.Bold,
Foreground = Brushes.Gold,
TextAlignment = TextAlignment.Center,
Margin = new Thickness(20)
};

TextBlock modeText = new TextBlock
{
Text = "Press Space for 1P Mode\nPress 2 for 2P Local Mode\nPress 3 for Online Mode",
FontSize = 32,
Foreground = Brushes.White,
TextAlignment = TextAlignment.Center,
Margin = new Thickness(20)
};

titlePanel.Children.Add(titleText);
titlePanel.Children.Add(modeText);

Canvas.SetLeft(titlePanel, GameCanvas.ActualWidth / 2 - 300);
Canvas.SetTop(titlePanel, GameCanvas.ActualHeight / 2 - 200);
GameCanvas.Children.Add(titlePanel);


StackPanel buttonPanel = new StackPanel
{
Orientation = Orientation.Horizontal,
HorizontalAlignment = HorizontalAlignment.Center,
VerticalAlignment = VerticalAlignment.Bottom,
Margin = new Thickness(0, 0, 0, 50)
};

Button onePlayerButton = new Button
{
Content = "1 Player Mode",
Width = 150,
Height = 60,
FontSize = 18,
Margin = new Thickness(10),
Background = Brushes.Green,
Foreground = Brushes.White,
FontWeight = FontWeights.Bold
};
onePlayerButton.Click += (s, e) =>
{
PlaySound("click.wav", false);
_isTwoPlayerMode = false;
_isOnlineMode = false;
_currentSettingPlayerNumber = 1;
ShowSongSelection();
};

Button twoPlayerButton = new Button
{
Content = "2 Player Local",
Width = 150,
Height = 60,
FontSize = 18,
Margin = new Thickness(10),
Background = Brushes.Blue,
Foreground = Brushes.White,
FontWeight = FontWeights.Bold
};
twoPlayerButton.Click += (s, e) =>
{
PlaySound("click.wav", false);
_isTwoPlayerMode = true;
_isOnlineMode = false;
_currentSettingPlayerNumber = 1;
ShowSongSelection();
};

Button onlineButton = new Button
{
Content = "Online Mode",
Width = 150,
Height = 60,
FontSize = 18,
Margin = new Thickness(10),
Background = Brushes.Orange,
Foreground = Brushes.White,
FontWeight = FontWeights.Bold
};
onlineButton.Click += (s, e) =>
{
PlaySound("click.wav", false);
ShowOnlineConnectionScreen();
};

buttonPanel.Children.Add(onePlayerButton);
buttonPanel.Children.Add(twoPlayerButton);
buttonPanel.Children.Add(onlineButton);
GameCanvas.Children.Add(buttonPanel);
}


private void ShowOnlineConnectionScreen()
{
_currentState = GameState.Title;
GameCanvas.Children.Clear();

StackPanel connectionPanel = new StackPanel
{
HorizontalAlignment = HorizontalAlignment.Center,
VerticalAlignment = VerticalAlignment.Center,
Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
Margin = new Thickness(40)
};

TextBlock titleBlock = new TextBlock
{
Text = "Online Mode",
FontSize = 48,
FontWeight = FontWeights.Bold,
Foreground = Brushes.Gold,
TextAlignment = TextAlignment.Center,
Margin = new Thickness(0, 0, 0, 20)
};

TextBlock hostWarning = new TextBlock
{
Text = "⚠ After clicking 'Host', complete this window before entering password.",
FontSize = 13,
Foreground = Brushes.Yellow,
TextAlignment = TextAlignment.Center,
Margin = new Thickness(0, 0, 0, 20),
TextWrapping = TextWrapping.Wrap,
MaxWidth = 450
};

Button hostButton = new Button
{
Content = "Host Game",
Width = 300,
Height = 60,
FontSize = 24,
FontWeight = FontWeights.Bold,
Background = Brushes.Green,
Foreground = Brushes.White,
Margin = new Thickness(0, 10, 0, 10)
};
hostButton.Click += async (s, e) =>
{
if (_networkManager != null)
{
await _networkManager.ConnectAsync();

int retries = 0;
while (!_networkManager.IsConnected && retries < 10)
{
await Task.Delay(100);
retries++;
}

if (!_networkManager.IsConnected)
{
MessageBox.Show("✗ 無法連接到服務器");
return;
}

_isOnlineMode = true;
_isTwoPlayerMode = true;

var createSuccess = await _networkManager.CreateRoomAsync(_currentRoomId, "Host");

if (createSuccess)
{
_isHost = true;
MessageBox.Show($"✓ Room Created: {_currentRoomId}\nWaiting for player to join.", "Host");
ShowWaitingForPlayerScreen();
}
else
{
MessageBox.Show("✗ Failed to create room (Room ID may be in use).", "Error");
}
}
};

StackPanel joinPanel = new StackPanel
{
Orientation = Orientation.Horizontal,
Margin = new Thickness(0, 20, 0, 0)
};

TextBlock sessionLabel = new TextBlock
{
Text = "Room ID:",
FontSize = 16,
Foreground = Brushes.White,
VerticalAlignment = VerticalAlignment.Center,
Margin = new Thickness(0, 0, 10, 0)
};

TextBox sessionInput = new TextBox
{
Width = 150,
Height = 40,
FontSize = 16,
Padding = new Thickness(5),
Text = "ROOM-"
};

TextBlock inputHint = new TextBlock
{
Text = "If no response, try entering again.",
FontSize = 11,
Foreground = Brushes.Yellow,
Margin = new Thickness(0, 5, 0, 0)
};

Button joinButton = new Button
{
Content = "Join Game",
Width = 100,
Height = 40,
FontSize = 16,
Background = Brushes.Blue,
Foreground = Brushes.White,
Margin = new Thickness(10, 0, 0, 0)
};
joinButton.Click += async (s, e) =>
{
string userInput = sessionInput.Text.Trim();
if (string.IsNullOrEmpty(userInput) || userInput == "ROOM-")
{
inputHint.Text = "Please enter a room ID.";
inputHint.Foreground = Brushes.Red;
return;
}


var joinRoomId = userInput.StartsWith("ROOM-") ? userInput : $"ROOM-{userInput}";

if (_networkManager != null)
{
await _networkManager.ConnectAsync();

int retries = 0;
while (!_networkManager.IsConnected && retries < 10)
{
await Task.Delay(100);
retries++;
}

if (!_networkManager.IsConnected)
{
inputHint.Text = "Unable to connect to server.";
inputHint.Foreground = Brushes.Red;
return;
}

inputHint.Text = "Status: Input submitted...";
inputHint.Foreground = Brushes.LimeGreen;

_isOnlineMode = true;
_isTwoPlayerMode = true;
_currentRoomId = joinRoomId;
_isHost = false;
_clientControlAllowed = false;

await _networkManager.JoinRoomAsync(joinRoomId, "Guest");
}
};

joinPanel.Children.Add(sessionLabel);
joinPanel.Children.Add(sessionInput);
joinPanel.Children.Add(joinButton);

Button backButton = new Button
{
Content = "Back",
Width = 100,
Height = 40,
FontSize = 16,
Background = Brushes.Red,
Foreground = Brushes.White,
Margin = new Thickness(0, 20, 0, 0)
};
backButton.Click += (s, e) => ShowTitleScreen();

connectionPanel.Children.Add(titleBlock);
connectionPanel.Children.Add(hostWarning);
connectionPanel.Children.Add(hostButton);
connectionPanel.Children.Add(joinPanel);
connectionPanel.Children.Add(inputHint);
connectionPanel.Children.Add(backButton);

Canvas.SetLeft(connectionPanel, GameCanvas.ActualWidth / 2 - 300);
Canvas.SetTop(connectionPanel, GameCanvas.ActualHeight / 2 - 250);
GameCanvas.Children.Add(connectionPanel);
}


private void ShowSongSelection()
{
_currentState = GameState.SongSelection;
if (_isOnlineMode && !_isHost && !_clientControlAllowed)
{
GameCanvas.Children.Clear();
TextBlock wait = new TextBlock
{
Text = "Waiting for host to allow song selection...",
FontSize = 28,
Foreground = Brushes.White,
TextAlignment = TextAlignment.Center
};
Canvas.SetLeft(wait, GameCanvas.ActualWidth / 2 - 250);
Canvas.SetTop(wait, GameCanvas.ActualHeight / 2 - 20);
GameCanvas.Children.Add(wait);
return;
}

if (_gameTimer != null) _gameTimer.Stop();
if (_musicPlayer != null) _musicPlayer.Stop();

GameCanvas.Children.Clear();
if (TopRightPanel != null) TopRightPanel.Visibility = Visibility.Collapsed;
JudgmentText.Visibility = Visibility.Collapsed;
ProgressBar.Visibility = Visibility.Collapsed;
ProgressText.Visibility = Visibility.Collapsed;
if (_tutorialP1Text != null) _tutorialP1Text.Visibility = Visibility.Collapsed;
if (_tutorialP2Text != null) _tutorialP2Text.Visibility = Visibility.Collapsed;

TextBlock songText = new TextBlock
{
Text = "SELECT SONG\n\n" +
"1. Asura Chan (2:53)\n" +
"2. Senbon Zakura (1:37)\n\n" +
"Press 1 or 2 to Select\nPress ESC to Back",
FontSize = 36,
Foreground = Brushes.White,
TextAlignment = TextAlignment.Center
};
Canvas.SetLeft(songText, GameCanvas.ActualWidth / 2 - 300);
Canvas.SetTop(songText, GameCanvas.ActualHeight / 2 - 150);
GameCanvas.Children.Add(songText);

StackPanel songButtonPanel = new StackPanel
{
Orientation = Orientation.Vertical,
HorizontalAlignment = HorizontalAlignment.Center,
VerticalAlignment = VerticalAlignment.Center
};

Button song1Button = new Button
{
Content = "1. Asura Chan",
Width = 400,
Height = 50,
FontSize = 18,
Margin = new Thickness(10),
Background = Brushes.DarkBlue,
Foreground = Brushes.White
};
song1Button.Click += (s, e) =>
{
PlaySound("click.wav", false);
_currentSettingPlayerNumber = 1;
_currentSongId = "song1";
ShowDifficultySelection();
};

Button song2Button = new Button
{
Content = "2. Senbon Zakura",
Width = 400,
Height = 50,
FontSize = 18,
Margin = new Thickness(10),
Background = Brushes.DarkBlue,
Foreground = Brushes.White
};
song2Button.Click += (s, e) =>
{
PlaySound("click.wav", false);
_currentSettingPlayerNumber = 1;
_currentSongId = "song2";
ShowDifficultySelection();
};

songButtonPanel.Children.Add(song1Button);
songButtonPanel.Children.Add(song2Button);

Canvas.SetLeft(songButtonPanel, GameCanvas.ActualWidth / 2 - 200);
Canvas.SetTop(songButtonPanel, GameCanvas.ActualHeight / 2 + 150);
GameCanvas.Children.Add(songButtonPanel);
}


private void ShowWaitingForPlayerScreen()
{
_currentState = GameState.Title;
GameCanvas.Children.Clear();

StackPanel waitPanel = new StackPanel
{
HorizontalAlignment = HorizontalAlignment.Center,
VerticalAlignment = VerticalAlignment.Center,
Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
Margin = new Thickness(40)
};

TextBlock roomIdText = new TextBlock
{
Text = $"Room ID: {_currentRoomId}",
FontSize = 32,
FontWeight = FontWeights.Bold,
Foreground = Brushes.Gold,
TextAlignment = TextAlignment.Center,
Margin = new Thickness(0, 0, 0, 20)
};

TextBlock waitingText = new TextBlock
{
Text = "Waiting for player to join...",
FontSize = 24,
Foreground = Brushes.White,
TextAlignment = TextAlignment.Center,
Margin = new Thickness(0, 0, 0, 30)
};

TextBlock hintText = new TextBlock
{
Text = "If unable to start, press ESC to restart.",
FontSize = 13,
Foreground = Brushes.LimeGreen,
TextAlignment = TextAlignment.Center,
Margin = new Thickness(0, 0, 0, 20)
};

_hostStartButton = new Button
{
Content = "Start Game",
Width = 200,
Height = 60,
FontSize = 20,
IsEnabled = false,
Background = Brushes.Green,
Foreground = Brushes.White,
Margin = new Thickness(0, 10, 0, 0)
};
_hostStartButton.Click += async (s, e) =>
{
if (_networkManager != null)
{
var ok = await _networkManager.RequestStartGameAsync();
if (!ok) MessageBox.Show("Unable to start game: not enough players.", "Error");
}
};

waitPanel.Children.Add(roomIdText);
waitPanel.Children.Add(waitingText);
waitPanel.Children.Add(hintText);
waitPanel.Children.Add(_hostStartButton);

Canvas.SetLeft(waitPanel, GameCanvas.ActualWidth / 2 - 300);
Canvas.SetTop(waitPanel, GameCanvas.ActualHeight / 2 - 200);
GameCanvas.Children.Add(waitPanel);
}


private void ShowDifficultySelection()
{
_currentState = GameState.DifficultySelection;


if (_isOnlineMode && !_isHost && !_clientControlAllowed)
{
GameCanvas.Children.Clear();
TextBlock wait = new TextBlock
{
Text = $"等待房主選擇難度...\n你是 P2 (Client)\n房主是 P1",
FontSize = 32,
Foreground = Brushes.White,
TextAlignment = TextAlignment.Center,
HorizontalAlignment = HorizontalAlignment.Center,
VerticalAlignment = VerticalAlignment.Center
};
Canvas.SetLeft(wait, GameCanvas.ActualWidth / 2 - 250);
Canvas.SetTop(wait, GameCanvas.ActualHeight / 2 - 60);
GameCanvas.Children.Add(wait);
return;
}

GameCanvas.Children.Clear();

string modeLabel = _isTwoPlayerMode ? $"Player {_currentSettingPlayerNumber}" : "Mode";
TextBlock diffText = new TextBlock
{
Text = $"SELECT DIFFICULTY - {modeLabel}\n\n" +
"1. Easy (Slower, Sparse)\n" +
"2. Normal (Standard)\n" +
"3. Hard (Faster, Dense)\n\n" +
"Press 1, 2, or 3 to Select",
FontSize = 32,
Foreground = Brushes.White,
TextAlignment = TextAlignment.Center
};
Canvas.SetLeft(diffText, GameCanvas.ActualWidth / 2 - 300);
Canvas.SetTop(diffText, GameCanvas.ActualHeight / 2 - 150);
GameCanvas.Children.Add(diffText);

StackPanel diffButtonPanel = new StackPanel
{
Orientation = Orientation.Vertical,
HorizontalAlignment = HorizontalAlignment.Center,
VerticalAlignment = VerticalAlignment.Center
};

Button easyButton = new Button
{
Content = "1. Easy",
Width = 400,
Height = 50,
FontSize = 18,
Margin = new Thickness(10),
Background = Brushes.LimeGreen,
Foreground = Brushes.Black,
FontWeight = FontWeights.Bold
};
easyButton.Click += (s, e) =>
{
PlaySound("click.wav", false);
var player = (_currentSettingPlayerNumber == 1) ? _player1 : _player2;
player.Difficulty = "Easy";

if (_isTwoPlayerMode && _currentSettingPlayerNumber == 1)
{
_currentSettingPlayerNumber = 2;
ShowDifficultySelection();
}
else
{
_currentSettingPlayerNumber = 1;
ShowPlayerSettings();
}
};

Button normalButton = new Button
{
Content = "2. Normal",
Width = 400,
Height = 50,
FontSize = 18,
Margin = new Thickness(10),
Background = Brushes.SkyBlue,
Foreground = Brushes.Black,
FontWeight = FontWeights.Bold
};
normalButton.Click += (s, e) =>
{
PlaySound("click.wav", false);
var player = (_currentSettingPlayerNumber == 1) ? _player1 : _player2;
player.Difficulty = "Normal";

if (_isTwoPlayerMode && _currentSettingPlayerNumber == 1)
{
_currentSettingPlayerNumber = 2;
ShowDifficultySelection();
}
else
{
_currentSettingPlayerNumber = 1;
ShowPlayerSettings();
}
};

Button hardButton = new Button
{
Content = "3. Hard",
Width = 400,
Height = 50,
FontSize = 18,
Margin = new Thickness(10),
Background = Brushes.Crimson,
Foreground = Brushes.White,
FontWeight = FontWeights.Bold
};
hardButton.Click += (s, e) =>
{
PlaySound("click.wav", false);
var player = (_currentSettingPlayerNumber == 1) ? _player1 : _player2;
player.Difficulty = "Hard";

if (_isTwoPlayerMode && _currentSettingPlayerNumber == 1)
{
_currentSettingPlayerNumber = 2;
ShowDifficultySelection();
}
else
{
_currentSettingPlayerNumber = 1;
ShowPlayerSettings();
}
};

diffButtonPanel.Children.Add(easyButton);
diffButtonPanel.Children.Add(normalButton);
diffButtonPanel.Children.Add(hardButton);

Canvas.SetLeft(diffButtonPanel, GameCanvas.ActualWidth / 2 - 200);
Canvas.SetTop(diffButtonPanel, GameCanvas.ActualHeight / 2 + 150);
GameCanvas.Children.Add(diffButtonPanel);
}


private void ShowPlayerSettings()
{
_currentState = GameState.PlayerSettings;
if (_isOnlineMode && !_isHost && !_clientControlAllowed)
{
GameCanvas.Children.Clear();
TextBlock wait = new TextBlock
{
Text = "Waiting for host to allow settings...",
FontSize = 28,
Foreground = Brushes.White,
TextAlignment = TextAlignment.Center
};
Canvas.SetLeft(wait, GameCanvas.ActualWidth / 2 - 250);
Canvas.SetTop(wait, GameCanvas.ActualHeight / 2 - 20);
GameCanvas.Children.Add(wait);
return;
}
GameCanvas.Children.Clear();

var player = (_currentSettingPlayerNumber == 1) ? _player1 : _player2;

StackPanel settingsPanel = new StackPanel
{
HorizontalAlignment = HorizontalAlignment.Center,
VerticalAlignment = VerticalAlignment.Center,
Background = new SolidColorBrush(Color.FromArgb(200, 30, 30, 30)),
Margin = new Thickness(40)
};

TextBlock titleBlock = new TextBlock
{
Text = $"Player {_currentSettingPlayerNumber} Settings",
FontSize = 40,
FontWeight = FontWeights.Bold,
Foreground = Brushes.Cyan,
TextAlignment = TextAlignment.Center,
Margin = new Thickness(20)
};


string keyGuide = "";
if (_isTwoPlayerMode)
{
if (_currentSettingPlayerNumber == 1)
{
keyGuide = $"P1 Keys:\nF, J = DON (Red)\nD, K = KA (Blue)\n\n";
}
else
{
keyGuide = $"P2 Keys:\n2, 4 = DON (Red)\n1, 5 = KA (Blue)\n\n";
}
}
else
{
keyGuide = $"Keys:\nF, J = DON (Red)\nD, K = KA (Blue)\n\n";
}

Brush speedColor = GetSpeedColor(player.ScrollSpeedMultiplier);
Brush diffColor = GetDifficultyColor(player.Difficulty);

TextBlock speedLabel = new TextBlock
{
Text = $"Speed: ",
FontSize = 24,
FontWeight = FontWeights.Bold,
Foreground = Brushes.White,
TextAlignment = TextAlignment.Left
};

TextBlock speedValue = new TextBlock
{
Text = $"{player.ScrollSpeedMultiplier}x",
FontSize = 28,
FontWeight = FontWeights.Bold,
Foreground = speedColor,
TextAlignment = TextAlignment.Left
};

StackPanel speedPanel = new StackPanel
{
Orientation = Orientation.Horizontal,
Margin = new Thickness(20, 10, 20, 10)
};
speedPanel.Children.Add(speedLabel);
speedPanel.Children.Add(speedValue);

TextBlock diffLabel = new TextBlock
{
Text = $"Difficulty: ",
FontSize = 24,
FontWeight = FontWeights.Bold,
Foreground = Brushes.White,
TextAlignment = TextAlignment.Left
};

TextBlock diffValue = new TextBlock
{
Text = $"{player.Difficulty}",
FontSize = 28,
FontWeight = FontWeights.Bold,
Foreground = diffColor,
TextAlignment = TextAlignment.Left
};

StackPanel diffPanel = new StackPanel
{
Orientation = Orientation.Horizontal,
Margin = new Thickness(20, 10, 20, 10)
};
diffPanel.Children.Add(diffLabel);
diffPanel.Children.Add(diffValue);

if (player.ScrollSpeedMultiplier == 4.0 && player.Difficulty == "Hard")
{
speedValue.FontSize = 32;
diffValue.FontSize = 32;
speedValue.Effect = new DropShadowEffect
{
Color = Colors.Red,
BlurRadius = 10,
ShadowDepth = 2
};
diffValue.Effect = new DropShadowEffect
{
Color = Colors.Red,
BlurRadius = 10,
ShadowDepth = 2
};

StartUltimateFlashing(speedValue, diffValue);
}

TextBlock keysBlock = new TextBlock
{
Text = keyGuide +
$"UP/DOWN or W/S: Adjust Speed (1x-4x)\n" +
$"LEFT/RIGHT or A/D: Change Difficulty\n" +
$"R: Reset to Default\n" +
$"SPACE: Continue",
FontSize = 16,
Foreground = Brushes.White,
TextAlignment = TextAlignment.Left,
Margin = new Thickness(20)
};

settingsPanel.Children.Add(titleBlock);
settingsPanel.Children.Add(speedPanel);
settingsPanel.Children.Add(diffPanel);
settingsPanel.Children.Add(keysBlock);

StackPanel buttonPanel = new StackPanel
{
Orientation = Orientation.Horizontal,
HorizontalAlignment = HorizontalAlignment.Center,
Margin = new Thickness(0, 20, 0, 0)
};

Button upButton = new Button
{
Content = "▲",
Width = 50,
Height = 40,
FontSize = 20,
Margin = new Thickness(5),
Background = Brushes.DodgerBlue
};
upButton.Click += (s, e) =>
{
player.ScrollSpeedMultiplier = Math.Min(player.ScrollSpeedMultiplier + 0.5, 4.0);
ShowPlayerSettings();
};

Button downButton = new Button
{
Content = "▼",
Width = 50,
Height = 40,
FontSize = 20,
Margin = new Thickness(5),
Background = Brushes.DodgerBlue
};
downButton.Click += (s, e) =>
{
player.ScrollSpeedMultiplier = Math.Max(player.ScrollSpeedMultiplier - 0.5, 1.0);
ShowPlayerSettings();
};

Button leftButton = new Button
{
Content = "◄",
Width = 50,
Height = 40,
FontSize = 20,
Margin = new Thickness(5),
Background = Brushes.OrangeRed
};
leftButton.Click += (s, e) =>
{
if (player.Difficulty == "Hard") player.Difficulty = "Normal";
else if (player.Difficulty == "Normal") player.Difficulty = "Easy";
ShowPlayerSettings();
};

Button rightButton = new Button
{
Content = "►",
Width = 50,
Height = 40,
FontSize = 20,
Margin = new Thickness(5),
Background = Brushes.OrangeRed
};
rightButton.Click += (s, e) =>
{
if (player.Difficulty == "Easy") player.Difficulty = "Normal";
else if (player.Difficulty == "Normal") player.Difficulty = "Hard";
ShowPlayerSettings();
};

Button selectButton = new Button
{
Content = "SELECT",
Width = 120,
Height = 40,
FontSize = 16,
Margin = new Thickness(10, 0, 0, 0),
Background = Brushes.LimeGreen,
Foreground = Brushes.Black,
FontWeight = FontWeights.Bold
};
selectButton.Click += async (s, e) =>
{
if (_isTwoPlayerMode && _currentSettingPlayerNumber == 1)
{
_currentSettingPlayerNumber = 2;
ShowPlayerSettings();
}
else
{
if (_isOnlineMode && _networkManager != null)
{
if (_isHost || _clientControlAllowed)
{
var ok = await _networkManager.RequestStartGameplayAsync(
_currentSongId,
_player1.Difficulty,
_player2.Difficulty,
_player1.ScrollSpeedMultiplier,
_player2.ScrollSpeedMultiplier);
if (!ok) MessageBox.Show("無法開始遊戲：請稍候或聯繫房主。");
}
else
{
MessageBox.Show("請等待房主或已允許控制的玩家開始遊戲。");
}
}
else
{
StartGame(_currentSongId);
}
}
};

buttonPanel.Children.Add(upButton);
buttonPanel.Children.Add(downButton);
buttonPanel.Children.Add(leftButton);
buttonPanel.Children.Add(rightButton);
buttonPanel.Children.Add(selectButton);

settingsPanel.Children.Add(buttonPanel);

Canvas.SetLeft(settingsPanel, GameCanvas.ActualWidth / 2 - 450);
Canvas.SetTop(settingsPanel, GameCanvas.ActualHeight / 2 - 300);
GameCanvas.Children.Add(settingsPanel);
}

private Brush GetSpeedColor(double speed)
{
return speed switch
{
1.0 => Brushes.LimeGreen,
1.5 => Brushes.Yellow,
2.0 => Brushes.Orange,
2.5 => Brushes.OrangeRed,
3.0 => Brushes.Red,
3.5 => Brushes.Crimson,
4.0 => Brushes.Magenta,
_ => Brushes.White
};
}

private Brush GetDifficultyColor(string difficulty)
{
return difficulty switch
{
"Easy" => Brushes.LimeGreen,
"Normal" => Brushes.SkyBlue,
"Hard" => Brushes.Crimson,
_ => Brushes.White
};
}

private void StartUltimateFlashing(TextBlock speedText, TextBlock diffText)
{
DispatcherTimer flashTimer = new DispatcherTimer();
int flashCount = 0;
Color[] rainbowColors = new Color[]
{
Colors.Red, Colors.Orange, Colors.Yellow, Colors.Lime,
Colors.Cyan, Colors.Blue, Colors.Magenta
};

flashTimer.Interval = TimeSpan.FromMilliseconds(200);
flashTimer.Tick += (s, e) =>
{
flashCount++;
if (flashCount >= rainbowColors.Length * 4)
{
flashTimer.Stop();
return;
}

int colorIndex = (flashCount / 4) % rainbowColors.Length;
speedText.Foreground = new SolidColorBrush(rainbowColors[colorIndex]);
diffText.Foreground = new SolidColorBrush(rainbowColors[colorIndex]);
};
flashTimer.Start();
}


private async void StartGame(string songId)
{
_currentState = GameState.Playing;
_currentSongId = songId;


if (_songs.ContainsKey(songId))
{
_gameDuration = _songs[songId].duration;
}


ResetGameData();



if (_isTwoPlayerMode)
{

var diff1 = _player1.Difficulty;
if (diff1 == "Easy") _songNotesP1 = new List<(double, bool)>(_easyNotes[songId]);
else if (diff1 == "Hard") _songNotesP1 = new List<(double, bool)>(_hardNotes[songId]);
else _songNotesP1 = new List<(double, bool)>(_normalNotes[songId]);


var diff2 = _player2.Difficulty;
if (diff2 == "Easy") _songNotesP2 = new List<(double, bool)>(_easyNotes[songId]);
else if (diff2 == "Hard") _songNotesP2 = new List<(double, bool)>(_hardNotes[songId]);
else _songNotesP2 = new List<(double, bool)>(_normalNotes[songId]);

_nextNoteIndexP1 = 0;
_nextNoteIndexP2 = 0;
}
else
{
string diff = _player1.Difficulty;
if (diff == "Easy") _songNotes = _easyNotes[songId];
else if (diff == "Hard") _songNotes = _hardNotes[songId];
else _songNotes = _normalNotes[songId];

_nextNoteIndex = 0;
}


if (_isOnlineMode && _networkManager?.IsConnected == true)
{
await _networkManager.SendScoreAsync(1, _player1.Score, _player1.Combo);
if (_isTwoPlayerMode)
{
await _networkManager.SendScoreAsync(2, _player2.Score, _player2.Combo);
}
}
GameCanvas.HorizontalAlignment = HorizontalAlignment.Stretch;
GameCanvas.VerticalAlignment = VerticalAlignment.Stretch;
Panel.SetZIndex(GameCanvas, 0);

GameCanvas.Children.Clear();

_backgroundRect = new Rectangle
{
Width = GameCanvas.ActualWidth,
Height = GameCanvas.ActualHeight,
IsHitTestVisible = false
};
GameCanvas.Children.Add(_backgroundRect);
Panel.SetZIndex(_backgroundRect, 0);

if (_isTwoPlayerMode)
{
_currentBgColor = Color.FromArgb(100, 255, 150, 80);
_targetBgColor = Color.FromArgb(100, 255, 100, 100);
}
else
{
_currentBgColor = Color.FromArgb(100, 100, 180, 255);
_targetBgColor = Color.FromArgb(100, 150, 100, 255);
}
_bgColorTransition = 0;
UpdateBackgroundColor();

_currentPhotoIndex = _random.Next(1, 11);
AddBackgroundImage();

_currentBackgroundColor = _random.Next(_bgColors.Count);
CreateBackgroundCircle();

DrawSemicircledrums();
DrawTargetRings();


CreatePlayerStatusDisplay();

if (_isTwoPlayerMode)
{
DrawDividerLine();

if (_isOnlineMode)
{
UpdatePlayerLabels();
if (_isHost)
HideDrum2Components();
else
HideDrum1Components();
}
}

DisplaySongInfo();

_particleSystem = new BackgroundParticleSystem(GameCanvas);
_particleSystem.Start();

_floatingImageSystem = new FloatingImageParticleSystem(GameCanvas);
_floatingImageSystem.Start();

_isGameRunning = true;
_nextNoteIndex = 0;
_currentGameTime = 0;
JudgmentText.Text = "";
JudgmentText.Visibility = _isTwoPlayerMode ? Visibility.Collapsed : Visibility.Visible;
if (TopRightPanel != null) TopRightPanel.Visibility = Visibility.Visible;
ProgressBar.Visibility = Visibility.Visible;
ProgressText.Visibility = Visibility.Visible;UpdateTutorialDisplays();

UpdateUI();

PlaySound(_songs[songId].filename, true);

_songStartTime = DateTime.Now.TimeOfDay.TotalSeconds;

if (_gameTimer != null) _gameTimer.Start();
if (_drumAnimationTimer != null) _drumAnimationTimer.Start();
}


private void UpdatePlayerLabels()
{
double midY = GameCanvas.ActualHeight / 2;

TextBlock p1Label = new TextBlock
{
Text = _isHost && _isOnlineMode ? "P1 (you)" : "P1 (Host)",
FontSize = 20,
FontWeight = FontWeights.Bold,
Foreground = Brushes.White,
Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0))
};
Canvas.SetLeft(p1Label, GameCanvas.ActualWidth / 2 - 50);
Canvas.SetTop(p1Label, midY - 30);
GameCanvas.Children.Add(p1Label);
Panel.SetZIndex(p1Label, 70);

TextBlock p2Label = new TextBlock
{
Text = _isHost && _isOnlineMode ? "P2 (Client)" : "P2 (you)",
FontSize = 20,
FontWeight = FontWeights.Bold,
Foreground = Brushes.Cyan,
Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0))
};
Canvas.SetLeft(p2Label, GameCanvas.ActualWidth / 2 - 50);
Canvas.SetTop(p2Label, midY + 10);
GameCanvas.Children.Add(p2Label);
Panel.SetZIndex(p2Label, 70);
}



private void AddBackgroundImage()
{
try
{
string[] searchPaths = new string[]
{
System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "photos"),
System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "photos"),
System.IO.Path.Combine(System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) ?? "", "photos")
};

string imagePath = "";
foreach (var dir in searchPaths)
{
string testPath = System.IO.Path.Combine(dir, $"{_currentPhotoIndex}.png");
if (File.Exists(testPath))
{
imagePath = testPath;
break;
}
}

if (!string.IsNullOrEmpty(imagePath))
{
_backgroundImage = new Image
{
Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute)),
Opacity = 0.3,
IsHitTestVisible = false,
Stretch = Stretch.UniformToFill
};
Canvas.SetLeft(_backgroundImage, 0);
Canvas.SetTop(_backgroundImage, 0);
GameCanvas.Children.Add(_backgroundImage);
Panel.SetZIndex(_backgroundImage, 3);
}
else
{
Console.WriteLine($"[Image] Photo {_currentPhotoIndex}.png not found in any search path.");
}
}
catch (Exception ex) { Debug.WriteLine($"[Image] Error: {ex.Message}"); }
}


private void DrawDividerLine()
{
double midY = GameCanvas.ActualHeight / 2;

_dividerLine = new Rectangle
{
Width = GameCanvas.ActualWidth,
Height = 4,
Fill = Brushes.White,
IsHitTestVisible = false
};

Canvas.SetLeft(_dividerLine, 0);
Canvas.SetTop(_dividerLine, midY);
GameCanvas.Children.Add(_dividerLine);
Panel.SetZIndex(_dividerLine, 65);

TextBlock p1Label = new TextBlock
{
Text = _isOnlineMode ? "" : "P1",
FontSize = 20,
FontWeight = FontWeights.Bold,
Foreground = Brushes.White,
Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0))
};
Canvas.SetLeft(p1Label, GameCanvas.ActualWidth / 2 - 30);
Canvas.SetTop(p1Label, midY - 30);
GameCanvas.Children.Add(p1Label);
Panel.SetZIndex(p1Label, 66);

TextBlock p2Label = new TextBlock
{
Text = _isOnlineMode ? "" : "P2",
FontSize = 20,
FontWeight = FontWeights.Bold,
Foreground = Brushes.Cyan,
Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0))
};
Canvas.SetLeft(p2Label, GameCanvas.ActualWidth / 2 - 30);
Canvas.SetTop(p2Label, midY + 20);
GameCanvas.Children.Add(p2Label);
Panel.SetZIndex(p2Label, 66);
}


private void DisplaySongInfo()
{
var songInfo = _songs[_currentSongId];
TextBlock songNameBlock = new TextBlock
{
Text = $"{songInfo.title} - {songInfo.artist}",
FontSize = 20,
FontWeight = FontWeights.Bold,
Foreground = Brushes.White,
TextAlignment = TextAlignment.Center
};
Canvas.SetLeft(songNameBlock, GameCanvas.ActualWidth / 2 - 200);
Canvas.SetTop(songNameBlock, 10);
GameCanvas.Children.Add(songNameBlock);
Panel.SetZIndex(songNameBlock, 100);
}


private void CreateBackgroundCircle()
{
_backgroundCircle = new Ellipse
{
Width = 200,
Height = 200,
Fill = new SolidColorBrush(_bgColors[_currentBackgroundColor]),
IsHitTestVisible = false
};

_circlePosX = -200;
Canvas.SetLeft(_backgroundCircle, _circlePosX);
Canvas.SetTop(_backgroundCircle, GameCanvas.ActualHeight - 250);
GameCanvas.Children.Add(_backgroundCircle);
Panel.SetZIndex(_backgroundCircle, 4);
}


private void DrawSemicircledrums()
{
if (!_isTwoPlayerMode)
{
double drumCenterX = 40;
double drumCenterY = GameCanvas.ActualHeight / 2;
double redRadius = 25;
double blueRadius = 45;

_drumRightBlueD = CreateSemicircle(drumCenterX, drumCenterY, blueRadius, true, false, Brushes.LightBlue, Brushes.DarkBlue);
GameCanvas.Children.Add(_drumRightBlueD);
Panel.SetZIndex(_drumRightBlueD, 30);

_drumRightBlueK = CreateSemicircle(drumCenterX, drumCenterY, blueRadius, true, true, Brushes.LightBlue, Brushes.DarkBlue);
GameCanvas.Children.Add(_drumRightBlueK);
Panel.SetZIndex(_drumRightBlueK, 30);

_drumLeftRedF = CreateSemicircle(drumCenterX, drumCenterY, redRadius, false, false, Brushes.IndianRed, Brushes.DarkRed);
GameCanvas.Children.Add(_drumLeftRedF);
Panel.SetZIndex(_drumLeftRedF, 31);

_drumLeftRedD = CreateSemicircle(drumCenterX, drumCenterY, redRadius, false, true, Brushes.IndianRed, Brushes.DarkRed);
GameCanvas.Children.Add(_drumLeftRedD);
Panel.SetZIndex(_drumLeftRedD, 31);

_player1TargetY = GameCanvas.ActualHeight / 2;
}
else
{

double drum1CenterX = 40;
double drum1CenterY = GameCanvas.ActualHeight / 4 - 80;
double redRadius = 25;
double blueRadius = 45;

_drumRightBlueD = CreateSemicircle(drum1CenterX, drum1CenterY, blueRadius, true, false, Brushes.LightBlue, Brushes.DarkBlue);
GameCanvas.Children.Add(_drumRightBlueD);
Panel.SetZIndex(_drumRightBlueD, 30);

_drumRightBlueK = CreateSemicircle(drum1CenterX, drum1CenterY, blueRadius, true, true, Brushes.LightBlue, Brushes.DarkBlue);
GameCanvas.Children.Add(_drumRightBlueK);
Panel.SetZIndex(_drumRightBlueK, 30);

_drumLeftRedF = CreateSemicircle(drum1CenterX, drum1CenterY, redRadius, false, false, Brushes.IndianRed, Brushes.DarkRed);
GameCanvas.Children.Add(_drumLeftRedF);
Panel.SetZIndex(_drumLeftRedF, 31);

_drumLeftRedD = CreateSemicircle(drum1CenterX, drum1CenterY, redRadius, false, true, Brushes.IndianRed, Brushes.DarkRed);
GameCanvas.Children.Add(_drumLeftRedD);
Panel.SetZIndex(_drumLeftRedD, 31);


double drum2CenterX = 40;
double drum2CenterY = GameCanvas.ActualHeight * 3 / 4 + 80;

_drum2RightBlueD = CreateSemicircle(drum2CenterX, drum2CenterY, blueRadius, true, false, Brushes.LightCyan, Brushes.DarkCyan);
GameCanvas.Children.Add(_drum2RightBlueD);
Panel.SetZIndex(_drum2RightBlueD, 30);

_drum2RightBlueK = CreateSemicircle(drum2CenterX, drum2CenterY, blueRadius, true, true, Brushes.LightCyan, Brushes.DarkCyan);
GameCanvas.Children.Add(_drum2RightBlueK);
Panel.SetZIndex(_drum2RightBlueK, 30);

_drum2LeftRedF = CreateSemicircle(drum2CenterX, drum2CenterY, redRadius, false, false, Brushes.LightSalmon, Brushes.Firebrick);
GameCanvas.Children.Add(_drum2LeftRedF);
Panel.SetZIndex(_drum2LeftRedF, 31);

_drum2LeftRedD = CreateSemicircle(drum2CenterX, drum2CenterY, redRadius, false, true, Brushes.LightSalmon, Brushes.Firebrick);
GameCanvas.Children.Add(_drum2LeftRedD);
Panel.SetZIndex(_drum2LeftRedD, 31);

_player1TargetY = GameCanvas.ActualHeight / 4;
_player2TargetY = GameCanvas.ActualHeight * 3 / 4;
}
}

private System.Windows.Shapes.Path CreateSemicircle(double centerX, double centerY, double radius, bool isBlue, bool isLeft, Brush fill, Brush stroke)
{
PathGeometry pathGeometry = new PathGeometry();
PathFigure pathFigure = new PathFigure();
pathFigure.StartPoint = new Point(centerX, centerY);

if (isLeft)
{
pathFigure.Segments.Add(new LineSegment(new Point(centerX, centerY - radius), true));
pathFigure.Segments.Add(new ArcSegment(
new Point(centerX, centerY + radius),
new Size(radius, radius),
0,
false,
SweepDirection.Counterclockwise,
true
));
}
else
{
pathFigure.Segments.Add(new LineSegment(new Point(centerX, centerY - radius), true));
pathFigure.Segments.Add(new ArcSegment(
new Point(centerX, centerY + radius),
new Size(radius, radius),
0,
false,
SweepDirection.Clockwise,
true
));
}

pathFigure.IsClosed = true;
pathGeometry.Figures.Add(pathFigure);

System.Windows.Shapes.Path path = new System.Windows.Shapes.Path
{
Data = pathGeometry,
Fill = fill,
Stroke = stroke,
StrokeThickness = 3,
IsHitTestVisible = false
};

return path;
}

private void DrawTargetRings()
{
double ringSize = 60;

Ellipse targetRing1 = new Ellipse
{
Width = ringSize,
Height = ringSize,
Stroke = Brushes.White,
StrokeThickness = 4,
Fill = Brushes.Transparent,
IsHitTestVisible = false
};
Canvas.SetLeft(targetRing1, _player1TargetX - (ringSize / 2));
Canvas.SetTop(targetRing1, _player1TargetY - (ringSize / 2));
GameCanvas.Children.Add(targetRing1);
Panel.SetZIndex(targetRing1, 50);

if (_isTwoPlayerMode)
{
Ellipse targetRing2 = new Ellipse
{
Width = ringSize,
Height = ringSize,
Stroke = Brushes.Cyan,
StrokeThickness = 4,
Fill = Brushes.Transparent,
IsHitTestVisible = false
};
Canvas.SetLeft(targetRing2, _player2TargetX - (ringSize / 2));
Canvas.SetTop(targetRing2, _player2TargetY - (ringSize / 2));
GameCanvas.Children.Add(targetRing2);
Panel.SetZIndex(targetRing2, 50);
}
}

private void DrawTargetRingForPlayer(double centerY, int playerNumber)
{
Ellipse ring = new Ellipse
{
Width = 140,
Height = 140,
Stroke = Brushes.White,
StrokeThickness = 2,
IsHitTestVisible = false
};

Canvas.SetLeft(ring, _player1TargetX - 70);
Canvas.SetTop(ring, centerY - 70);
GameCanvas.Children.Add(ring);
Panel.SetZIndex(ring, 20);
}



private void Window_KeyDown(object sender, KeyEventArgs e)
{
if (Keyboard.FocusedElement is TextBox) return;


if (e.Key == Key.Escape)
{
if (_currentState == GameState.Playing)
{
ShowPauseMenu();
e.Handled = true;
}
else if (_currentState == GameState.DifficultySelection || _currentState == GameState.SongSelection)
{
ShowTitleScreen();
e.Handled = true;
}
else if (_currentState == GameState.Title && _isOnlineMode && _isHost)
{

if (_networkManager != null) _ = _networkManager.DisconnectAsync();
_currentRoomId = GenerateRoomId();
ShowOnlineConnectionScreen();
e.Handled = true;
}
}


if (e.Key == Key.Space && _currentState == GameState.Title)
{
PlaySound("click.wav", false);
_isTwoPlayerMode = false;
_currentSettingPlayerNumber = 1;
_player1.RedLeft = Key.F;
_player1.RedRight = Key.J;
_player1.BlueLeft = Key.D;
_player1.BlueRight = Key.K;
_player2.RedLeft = Key.D1;
_player2.RedRight = Key.D2;
_player2.BlueLeft = Key.D3;
_player2.BlueRight = Key.D4;
ShowSongSelection();
e.Handled = true;
return;
}

if (e.Key == Key.D2 && _currentState == GameState.Title)
{
PlaySound("click.wav", false);
_isTwoPlayerMode = true;
_currentSettingPlayerNumber = 1;
_player1.RedLeft = Key.F;
_player1.RedRight = Key.J;
_player1.BlueLeft = Key.D;
_player1.BlueRight = Key.K;
_player2.RedLeft = Key.D1;
_player2.RedRight = Key.D2;
_player2.BlueLeft = Key.D3;
_player2.BlueRight = Key.D4;
ShowSongSelection();
e.Handled = true;
return;
}


if (e.Key == Key.Space && _currentState == GameState.GameOver)
{
PlaySound("click.wav", false);
ShowSongSelection();
e.Handled = true;
return;
}


if (!_isGameRunning)
{
return;
}


if (_isOnlineMode)
{
int myPlayerNum = _isHost ? 1 : 2;
PlayerSettings mySettings = (myPlayerNum == 1) ? _player1 : _player2;


bool isRedLeft = (e.Key == _player1.RedLeft);
bool isRedRight = (e.Key == _player1.RedRight);
bool isBlueLeft = (e.Key == _player1.BlueLeft);
bool isBlueRight = (e.Key == _player1.BlueRight);

if (isRedLeft || isRedRight || isBlueLeft || isBlueRight)
{
bool isRed = (isRedLeft || isRedRight);
bool isLeft = (isRedLeft || isBlueLeft);

if (isRed) PlaySound("drum_hit.wav", false); else PlayRandomBlueSound();
AnimateDrumSemicircle(isRed, isLeft, myPlayerNum);

double targetX = (myPlayerNum == 1) ? _player1TargetX : _player2TargetX;
CheckHit(mySettings, targetX, isRed, myPlayerNum);

e.Handled = true;
}
else if (e.Key == Key.Down)
{

if (myPlayerNum == 1)
_player1TargetY = _isTwoPlayerMode ? GameCanvas.ActualHeight / 4 : GameCanvas.ActualHeight / 2;
else
_player2TargetY = GameCanvas.ActualHeight * 3 / 4;

e.Handled = true;
}
}
else
{

bool p1Hit = false;
bool p1Red = false;
bool p1Left = false;

if (e.Key == Key.D) { p1Hit = true; p1Red = false; p1Left = true; }
else if (e.Key == Key.F) { p1Hit = true; p1Red = true; p1Left = true; }
else if (e.Key == Key.J) { p1Hit = true; p1Red = true; p1Left = false; }
else if (e.Key == Key.K) { p1Hit = true; p1Red = false; p1Left = false; }

if (p1Hit)
{
if (p1Red) PlaySound("drum_hit.wav", false); else PlayRandomBlueSound();
AnimateDrumSemicircle(p1Red, p1Left, 1);
CheckHit(_player1, _player1TargetX, p1Red, 1);
e.Handled = true;
}


bool p2Hit = false;
bool p2Red = false;
bool p2Left = false;

if (e.Key == Key.D1) { p2Hit = true; p2Red = false; p2Left = true; }
else if (e.Key == Key.D2) { p2Hit = true; p2Red = true; p2Left = true; }
else if (e.Key == Key.D4) { p2Hit = true; p2Red = true; p2Left = false; }
else if (e.Key == Key.D5) { p2Hit = true; p2Red = false; p2Left = false; }

if (p2Hit)
{
if (p2Red) PlaySound("drum_hit.wav", false); else PlayRandomBlueSound();
AnimateDrumSemicircle(p2Red, p2Left, 2);
CheckHit(_player2, _player2TargetX, p2Red, 2);
e.Handled = true;
}


if (e.Key == Key.Down && !p1Hit && !p2Hit)
{
_player1TargetY = _isTwoPlayerMode ? GameCanvas.ActualHeight / 4 : GameCanvas.ActualHeight / 2;
_player2TargetY = GameCanvas.ActualHeight * 3 / 4;
e.Handled = true;
}
}
}

private void ShowPauseMenu(bool sendToNetwork = true)
{
if (_currentState == GameState.Paused) return;

_currentState = GameState.Paused;
if (_gameTimer != null) _gameTimer.Stop();
if (_drumAnimationTimer != null) _drumAnimationTimer.Stop();
if (_musicPlayer != null) _musicPlayer.Pause();

_pausedTime = _currentGameTime;


if (_isOnlineMode && sendToNetwork && _networkManager != null)
{
_ = _networkManager.SendPauseStateAsync(true);
}

GameCanvas.Children.Clear();

Rectangle overlay = new Rectangle
{
Width = GameCanvas.ActualWidth,
Height = GameCanvas.ActualHeight,
Fill = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0))
};
GameCanvas.Children.Add(overlay);

StackPanel pausePanel = new StackPanel
{
HorizontalAlignment = HorizontalAlignment.Center,
VerticalAlignment = VerticalAlignment.Center,
Background = new SolidColorBrush(Color.FromArgb(220, 30, 30, 30)),
Margin = new Thickness(40)
};

TextBlock pauseText = new TextBlock
{
Text = _isOnlineMode ? "OPPONENT PAUSED" : "PAUSED",
FontSize = 60,
FontWeight = FontWeights.Bold,
Foreground = Brushes.Yellow,
TextAlignment = TextAlignment.Center,
Margin = new Thickness(0, 0, 0, 30)
};

Button resumeButton = new Button
{
Content = "Resume Game (R)",
Width = 300,
Height = 60,
FontSize = 24,
FontWeight = FontWeights.Bold,
Background = Brushes.Green,
Foreground = Brushes.White,
Margin = new Thickness(0, 10, 0, 10)
};
resumeButton.Click += (s, e) => ResumePause(true);

Button mainMenuButton = new Button
{
Content = "Return to Menu (M)",
Width = 300,
Height = 60,
FontSize = 24,
FontWeight = FontWeights.Bold,
Background = Brushes.Red,
Foreground = Brushes.White,
Margin = new Thickness(0, 10, 0, 10)
};
mainMenuButton.Click += async (s, e) =>
{
if (_networkManager != null) await _networkManager.DisconnectAsync();
ShowTitleScreen();
};

pausePanel.Children.Add(pauseText);
pausePanel.Children.Add(resumeButton);
pausePanel.Children.Add(mainMenuButton);

Canvas.SetLeft(pausePanel, GameCanvas.ActualWidth / 2 - 200);
Canvas.SetTop(pausePanel, GameCanvas.ActualHeight / 2 - 200);
GameCanvas.Children.Add(pausePanel);
}


private void ResumePause(bool sendToNetwork = true)
{
if (_currentState == GameState.Playing) return;


if (_isOnlineMode && sendToNetwork && _networkManager != null)
{
_ = _networkManager.SendPauseStateAsync(false);
}

_currentState = GameState.Playing;
GameCanvas.Children.Clear();


_backgroundRect = new Rectangle
{
Width = GameCanvas.ActualWidth,
Height = GameCanvas.ActualHeight,
IsHitTestVisible = false
};
GameCanvas.Children.Add(_backgroundRect);
Panel.SetZIndex(_backgroundRect, 0);
UpdateBackgroundColor();
AddBackgroundImage();
CreateBackgroundCircle();
DrawSemicircledrums();
DrawTargetRings();

if (_isTwoPlayerMode)
{
DrawDividerLine();
CreatePlayerStatusDisplay();
if (_isOnlineMode)
{
UpdatePlayerLabels();
if (_isHost) HideDrum2Components(); else HideDrum1Components();
}
}

DisplaySongInfo();
UpdateUI();

if (_musicPlayer != null) _musicPlayer.Play();
if (_gameTimer != null) _gameTimer.Start();
if (_drumAnimationTimer != null) _drumAnimationTimer.Start();
}


private void ShowGameOverScreen()
{
_currentState = GameState.GameOver;
_isGameRunning = false;

if (_gameTimer != null) _gameTimer.Stop();
if (_drumAnimationTimer != null) _drumAnimationTimer.Stop();
if (_particleSystem != null) _particleSystem.Stop();
if (_floatingImageSystem != null) _floatingImageSystem.Stop();
if (_musicPlayer != null) _musicPlayer.Stop();

GameCanvas.Children.Clear();

Rectangle overlay = new Rectangle
{
Width = GameCanvas.ActualWidth,
Height = GameCanvas.ActualHeight,
Fill = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0))
};
GameCanvas.Children.Add(overlay);

StackPanel gameOverPanel = new StackPanel
{
HorizontalAlignment = HorizontalAlignment.Center,
VerticalAlignment = VerticalAlignment.Center,
Background = new SolidColorBrush(Color.FromArgb(220, 30, 30, 30)),
Margin = new Thickness(40)
};

TextBlock gameOverText = new TextBlock
{
Text = "GAME OVER",
FontSize = 60,
FontWeight = FontWeights.Bold,
Foreground = Brushes.Red,
TextAlignment = TextAlignment.Center,
Margin = new Thickness(0, 0, 0, 30)
};
gameOverPanel.Children.Add(gameOverText);

if (_isTwoPlayerMode)
{
TextBlock p1StatsBlock = new TextBlock
{
Text = $"Player 1: {_player1.Score} pts | {_player1.PerfectCount} Perfect | {_player1.GoodCount} Good | {_player1.BadCount} Bad | {_player1.MissCount} Miss",
FontSize = 18,
Foreground = Brushes.White,
TextAlignment = TextAlignment.Center,
Margin = new Thickness(20)
};
gameOverPanel.Children.Add(p1StatsBlock);

TextBlock p2StatsBlock = new TextBlock
{
Text = $"Player 2: {_player2.Score} pts | {_player2.PerfectCount} Perfect | {_player2.GoodCount} Good | {_player2.BadCount} Bad | {_player2.MissCount} Miss",
FontSize = 18,
Foreground = Brushes.Cyan,
TextAlignment = TextAlignment.Center,
Margin = new Thickness(20)
};
gameOverPanel.Children.Add(p2StatsBlock);
}
else
{
TextBlock p1StatsBlock = new TextBlock
{
Text = $"Score: {_player1.Score} | {_player1.PerfectCount} Perfect | {_player1.GoodCount} Good | {_player1.BadCount} Bad | {_player1.MissCount} Miss",
FontSize = 18,
Foreground = Brushes.White,
TextAlignment = TextAlignment.Center,
Margin = new Thickness(20)
};
gameOverPanel.Children.Add(p1StatsBlock);
}

Button restartButton = new Button
{
Content = "Back to Menu",
Width = 200,
Height = 50,
FontSize = 18,
Background = Brushes.Green,
Foreground = Brushes.White,
Margin = new Thickness(0, 20, 0, 0)
};
restartButton.Click += (s, e) =>
{
PlaySound("click.wav", false);
ShowTitleScreen();
};

gameOverPanel.Children.Add(restartButton);

Canvas.SetLeft(gameOverPanel, GameCanvas.ActualWidth / 2 - 400);
Canvas.SetTop(gameOverPanel, GameCanvas.ActualHeight / 2 - 250);
GameCanvas.Children.Add(gameOverPanel);
}


private void PlaySound(string filename, bool loop)
{
try
{

string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sounds", filename);
if (!File.Exists(path))
{
path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) ?? "", "sounds", filename);
}
if (!File.Exists(path))
{
path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", filename);
}
if (!File.Exists(path))
{
Debug.WriteLine($"[PlaySound] File not found: {filename} (tried multiple paths)");
return;
}

Debug.WriteLine($"[PlaySound] Playing {filename} (loop={loop}) from path: {path}");

if (loop)
{

_musicPlayer?.Stop();
_musicPlayer?.Open(new Uri(path, UriKind.Absolute));
_musicPlayer?.Play();
}
else
{

_sfxPlayer?.Stop();
_sfxPlayer?.Open(new Uri(path, UriKind.Absolute));
_sfxPlayer?.Play();
}
}
catch (Exception ex)
{
Debug.WriteLine($"[PlaySound] Error: {ex.Message}");
}
}


private void PlayRandomBlueSound()
{
int randomIndex = _random.Next(1, 4);
string filename = $"KA{randomIndex}.wav";
PlaySound(filename, false);
}

private void AnimateDrumSemicircle(bool isRed, bool isLeft, int playerNumber)
{
System.Windows.Shapes.Path? targetPath = null;

if (playerNumber == 1)
{
if (isRed)
{
targetPath = isLeft ? _drumLeftRedD : _drumLeftRedF;
}
else
{
targetPath = isLeft ? _drumRightBlueK : _drumRightBlueD;
}
}
else if (playerNumber == 2 && _isTwoPlayerMode)
{
if (isRed)
{
targetPath = isLeft ? _drum2LeftRedD : _drum2LeftRedF;
}
else
{
targetPath = isLeft ? _drum2RightBlueK : _drum2RightBlueD;
}
}

if (targetPath != null)
{
Brush originalFill = targetPath.Fill;
targetPath.Fill = isRed ? Brushes.OrangeRed : Brushes.DeepSkyBlue;
targetPath.StrokeThickness = 5;

DispatcherTimer resetTimer = new DispatcherTimer();
resetTimer.Interval = TimeSpan.FromMilliseconds(150);
resetTimer.Tick += (s, e) =>
{
targetPath.Fill = originalFill;
targetPath.StrokeThickness = 3;
resetTimer.Stop();
};
resetTimer.Start();
}
}


private void CheckHit(PlayerSettings player, double targetX, bool isRedInput, int playerNumber)
{

var noteList = (playerNumber == 1) ? _player1Notes : _player2Notes;

var targetNote = noteList.Where(n => !n.IsHit)
.OrderBy(n => Math.Abs((Canvas.GetLeft(n.UIElement) + 25) - targetX))
.FirstOrDefault();

if (targetNote == null)
{
Debug.WriteLine($"[CheckHit] No note found for player {playerNumber} at X={targetX}");
return;
}

double noteX = Canvas.GetLeft(targetNote.UIElement) + 25;
double distance = Math.Abs(noteX - targetX);
string judgment = "";
string soundFile = "";

if (distance < 60)
{
bool colorMatch = targetNote.IsRed == isRedInput;

if (distance < 40 && colorMatch)
{
if (distance < 15)
{
judgment = "PERFECT";
soundFile = "perfect.wav";
player.Score += 100;
player.PerfectCount++;
}
else
{
judgment = "GOOD";
soundFile = "good.wav";
player.Score += 50;
player.GoodCount++;
}
player.Combo++;
}
else
{
judgment = "MISS";
soundFile = "miss.wav";
player.MissCount++;
player.Combo = 0;
}

targetNote.IsHit = true;
GameCanvas.Children.Remove(targetNote.UIElement);
noteList.Remove(targetNote);


ShowJudgment(judgment, GetJudgmentColor(judgment), playerNumber);

Debug.WriteLine($"[CheckHit] Player {playerNumber}: {judgment} at distance {distance:F1}");


if (_isOnlineMode && _networkManager != null)
{
Debug.WriteLine($"[CheckHit] Sending to server: Room={_currentRoomId}, Player={playerNumber}, Judgment={judgment}, Sound={soundFile}");

_ = _networkManager.SendScoreUpdateAsync(
player.Score,
player.Combo,
player.PerfectCount,
player.GoodCount,
player.BadCount,
player.MissCount
);


_ = _networkManager.SendJudgmentAsync(playerNumber, judgment, soundFile, isRedInput);
}
else
{
Debug.WriteLine($"[CheckHit] Not in online mode or no network manager");
}
}
UpdateUI();
}


private void HandleMiss(PlayerSettings player, int playerNumber)
{
player.MissCount++;
player.Combo = 0;
ShowJudgment("MISS", Brushes.DarkRed, playerNumber);
UpdateUI();


if (_isOnlineMode && _networkManager != null)
{

bool isMe = (_isHost && playerNumber == 1) || (!_isHost && playerNumber == 2);
if (isMe)
{
_ = _networkManager.SendScoreUpdateAsync(
player.Score,
player.Combo,
player.PerfectCount,
player.GoodCount,
player.BadCount,
player.MissCount
);
}
}
}

private void ShowJudgment(string judgment, Brush color, int playerNumber)
{
TextBlock judgmentBlock = new TextBlock
{
Text = judgment,
FontSize = 48,
FontWeight = FontWeights.Bold,
Foreground = color,
TextAlignment = TextAlignment.Center,
Opacity = 1.0
};

if (_isTwoPlayerMode)
{
double posY = (playerNumber == 1) ? GameCanvas.ActualHeight / 4 : GameCanvas.ActualHeight * 3 / 4;
Canvas.SetLeft(judgmentBlock, GameCanvas.ActualWidth / 2 - 50);
Canvas.SetTop(judgmentBlock, posY - 30);
}
else
{
Canvas.SetLeft(judgmentBlock, GameCanvas.ActualWidth / 2 - 50);
Canvas.SetTop(judgmentBlock, GameCanvas.ActualHeight / 2 - 30);
}

GameCanvas.Children.Add(judgmentBlock);
Panel.SetZIndex(judgmentBlock, 100);

DispatcherTimer fadeTimer = new DispatcherTimer();
double fadeCounter = 0;
fadeTimer.Interval = TimeSpan.FromMilliseconds(20);
fadeTimer.Tick += (s, e) =>
{
fadeCounter += 0.05;
judgmentBlock.Opacity = Math.Max(0, 1.0 - fadeCounter);

if (fadeCounter >= 1.0)
{
GameCanvas.Children.Remove(judgmentBlock);
fadeTimer.Stop();
}
};
fadeTimer.Start();
}


private Brush GetJudgmentColor(string judgment)
{
return judgment switch
{
"PERFECT" => Brushes.Gold,
"GOOD" => Brushes.White,
"BAD" => Brushes.Orange,
"MISS" => Brushes.DarkRed,
_ => Brushes.White
};
}

private void UpdateUI()
{
if (_isTwoPlayerMode)
{
if (_p1StatusText != null)
{
_p1StatusText.Text = $"P1: {_player1.Score} | Combo: {_player1.Combo}";
}
if (_p2StatusText != null)
{
_p2StatusText.Text = $"P2: {_player2.Score} | Combo: {_player2.Combo}";
}
}
else
{

if (_p1StatusText != null)
{
_p1StatusText.Text = $"P1: {_player1.Score} | Combo: {_player1.Combo}";
}

if (_p2StatusText != null)
{
_p2StatusText.Visibility = Visibility.Collapsed;
}
}
}

private void UpdateOpponentUI(int playerNum, int score, int combo, int perfect, int good, int bad, int miss)
{


string text = $"P{playerNum}: {score} | Combo: {combo} | P:{perfect} G:{good} B:{bad} M:{miss}";
if (playerNum == 1)
{
if (_p1StatusText != null)
{
_p1StatusText.Text = text;
}
}
else if (playerNum == 2)
{
if (_p2StatusText != null)
{
_p2StatusText.Text = text;
}
}
}

private void UpdateScoreDisplay()
{

UpdateUI();


if (_p1StatusText != null)
{
_p1StatusText.Text = $"P1: {_player1.Score} | Combo: {_player1.Combo} | P:{_player1.PerfectCount} G:{_player1.GoodCount} B:{_player1.BadCount} M:{_player1.MissCount}";
}
if (_p2StatusText != null)
{
_p2StatusText.Text = $"P2: {_player2.Score} | Combo: {_player2.Combo} | P:{_player2.PerfectCount} G:{_player2.GoodCount} B:{_player2.BadCount} M:{_player2.MissCount}";
}
}


private void CreatePlayerStatusDisplay()
{
if (_p1StatusText == null)
{
_p1StatusText = new TextBlock
{
Text = $"P1: {_player1.Score} | Combo: {_player1.Combo}",
FontSize = 20,
FontWeight = FontWeights.Bold,
Foreground = Brushes.White,
Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
Margin = new Thickness(0, 5, 0, 0)
};
TopRightPanel.Children.Add(_p1StatusText);
}

if (_p2StatusText == null)
{
_p2StatusText = new TextBlock
{
Text = $"P2: {_player2.Score} | Combo: {_player2.Combo}",
FontSize = 20,
FontWeight = FontWeights.Bold,
Foreground = Brushes.Cyan,
Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
Margin = new Thickness(0, 5, 0, 0)
};
TopRightPanel.Children.Add(_p2StatusText);
}

_p2StatusText.Visibility = _isTwoPlayerMode ? Visibility.Visible : Visibility.Collapsed;
if (_tutorialP1Text == null)
{
_tutorialP1Text = new TextBlock
{
Text = "DF(RED) JK(BLUE)",
FontSize = 20,
FontWeight = FontWeights.Bold,
Foreground = Brushes.White,
IsHitTestVisible = false
};
Canvas.SetLeft(_tutorialP1Text, 10);
Canvas.SetTop(_tutorialP1Text, 10);
GameCanvas.Children.Add(_tutorialP1Text);
Panel.SetZIndex(_tutorialP1Text, 200);
}

if (_tutorialP2Text == null)
{
_tutorialP2Text = new TextBlock
{
Text = "P2: 12(RED) 45(BLUE)",
FontSize = 20,
FontWeight = FontWeights.Bold,
Foreground = Brushes.White,
IsHitTestVisible = false
};
Canvas.SetLeft(_tutorialP2Text, 10);
Canvas.SetTop(_tutorialP2Text, Math.Max(10, GameCanvas.ActualHeight - 80));
GameCanvas.Children.Add(_tutorialP2Text);
Panel.SetZIndex(_tutorialP2Text, 200);
}

UpdateTutorialDisplays();

if (_tutorialGlowTimer == null)
{
_tutorialGlowTimer = new DispatcherTimer();
_tutorialGlowTimer.Interval = TimeSpan.FromMilliseconds(600);
bool bright = false;
_tutorialGlowTimer.Tick += (s, e) =>
{
bright = !bright;
double opacity = bright ? 1.0 : 0.65;
if (_tutorialP1Text != null) _tutorialP1Text.Opacity = opacity;
if (_tutorialP2Text != null) _tutorialP2Text.Opacity = opacity;
};
_tutorialGlowTimer.Start();
}
}


private void UpdateBackgroundColor()
{
if (_backgroundRect != null)
{
int r = (int)(_currentBgColor.R + (_targetBgColor.R - _currentBgColor.R) * _bgColorTransition);
int g = (int)(_currentBgColor.G + (_targetBgColor.G - _currentBgColor.G) * _bgColorTransition);
int b = (int)(_currentBgColor.B + (_targetBgColor.B - _currentBgColor.B) * _bgColorTransition);
int a = (int)(_currentBgColor.A + (_targetBgColor.A - _currentBgColor.A) * _bgColorTransition);

Color newColor = Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
_backgroundRect.Fill = new SolidColorBrush(newColor);
}
}
private void UpdateNetworkScore()
{

var s = (_networkManager.AssignedPlayerNumber == 1) ? _player1 : _player2;


_networkManager.SendScoreUpdateAsync(
s.Score,
s.Combo,
s.PerfectCount,
s.GoodCount,
s.BadCount,
s.MissCount
);
}
private void UpdateProgressBar(double currentTime)
{
double progress = Math.Min(currentTime / _gameDuration, 1.0) * 100;
ProgressBar.Value = progress;
int minutes = (int)_gameDuration / 60;
int seconds = (int)_gameDuration % 60;
ProgressText.Text = $"{progress:F0}% / {minutes}:{seconds:D2}";
}

private void UpdateNotePositions()
{
double noteSpeed1 = BaseNoteSpeed * _player1.ScrollSpeedMultiplier;
double noteSpeed2 = BaseNoteSpeed * _player2.ScrollSpeedMultiplier;

List<Note> missingNotes = new List<Note>();

foreach (var note in _player1Notes)
{
double currentLeft = Canvas.GetLeft(note.UIElement);
Canvas.SetLeft(note.UIElement, currentLeft - noteSpeed1);

if (currentLeft < -50)
{
missingNotes.Add(note);
if (!note.IsHit)
{
HandleMiss(_player1, 1);
}
}
}

foreach (var note in _player2Notes)
{
double currentLeft = Canvas.GetLeft(note.UIElement);
Canvas.SetLeft(note.UIElement, currentLeft - noteSpeed2);

if (currentLeft < -50)
{
missingNotes.Add(note);
if (!note.IsHit)
{
HandleMiss(_player2, 2);
}
}
}

foreach (var note in missingNotes)
{
GameCanvas.Children.Remove(note.UIElement);
if (note.PlayerNumber == 1)
_player1Notes.Remove(note);
else
_player2Notes.Remove(note);
}
}


private void SpawnNoteAtTime((double time, bool isRed) noteData)
{
bool isRed = noteData.isRed;


SpawnSingleNote(noteData.time, isRed, 1);


if (_isTwoPlayerMode)
{
SpawnSingleNote(noteData.time, isRed, 2);
}
}


private void SpawnSingleNote(double time, bool isRed, int pNum)
{
Ellipse noteShape = new Ellipse
{
Width = 50,
Height = 50,
Fill = isRed ? Brushes.Red : (pNum == 1 ? Brushes.Blue : Brushes.Cyan),
Stroke = Brushes.White,
StrokeThickness = 2
};

double startY = (pNum == 1) ? _player1TargetY - 25 : _player2TargetY - 25;
Canvas.SetLeft(noteShape, GameCanvas.ActualWidth + 50);
Canvas.SetTop(noteShape, startY);
GameCanvas.Children.Add(noteShape);
Panel.SetZIndex(noteShape, 40);

var note = new Note { UIElement = noteShape, IsRed = isRed, PlayerNumber = pNum };
if (pNum == 1)
_player1Notes.Add(note);
else
_player2Notes.Add(note);
}


private void DrumAnimationLoop(object? sender, EventArgs e)
{
if (_isGameRunning && _bgColorTransition < 1.0)
{
_bgColorTransition += BgColorTransitionSpeed;
if (_bgColorTransition >= 1.0)
{
_bgColorTransition = 0;
_currentBgColor = _targetBgColor;
int colorIndex = _random.Next(_bgColors.Count);
_targetBgColor = _bgColors[colorIndex];
}
UpdateBackgroundColor();
}

if (_backgroundCircle != null && GameCanvas != null)
{
_circlePosX += _circleSpeed;
if (_circlePosX > GameCanvas.ActualWidth)
{
_circlePosX = -200;
_currentBackgroundColor = _random.Next(_bgColors.Count);
_backgroundCircle.Fill = new SolidColorBrush(_bgColors[_currentBackgroundColor]);
}
Canvas.SetLeft(_backgroundCircle, _circlePosX);
}
}


private void GameLoop(object? sender, EventArgs e)
{
if (!_isGameRunning) return;

_currentGameTime = DateTime.Now.TimeOfDay.TotalSeconds - _songStartTime;

UpdateProgressBar(_currentGameTime);

if (_isTwoPlayerMode)
{

while (_nextNoteIndexP1 < _songNotesP1.Count && _songNotesP1[_nextNoteIndexP1].Item1 <= _currentGameTime)
{
var n = _songNotesP1[_nextNoteIndexP1];
SpawnSingleNote(n.time, n.isRed, 1);
_nextNoteIndexP1++;
}


while (_nextNoteIndexP2 < _songNotesP2.Count && _songNotesP2[_nextNoteIndexP2].Item1 <= _currentGameTime)
{
var n = _songNotesP2[_nextNoteIndexP2];
SpawnSingleNote(n.time, n.isRed, 2);
_nextNoteIndexP2++;
}
}
else
{
while (_nextNoteIndex < _songNotes.Count && _songNotes[_nextNoteIndex].Item1 <= _currentGameTime)
{
SpawnNoteAtTime(_songNotes[_nextNoteIndex]);
_nextNoteIndex++;
}
}

if (_currentGameTime >= _gameDuration)
{
var notesToRemove1 = _player1Notes.Where(n => !n.IsHit).ToList();
foreach (var note in notesToRemove1)
{
GameCanvas.Children.Remove(note.UIElement);
_player1Notes.Remove(note);
HandleMiss(_player1, 1);
}

if (_isTwoPlayerMode)
{
var notesToRemove2 = _player2Notes.Where(n => !n.IsHit).ToList();
foreach (var note in notesToRemove2)
{
GameCanvas.Children.Remove(note.UIElement);
_player2Notes.Remove(note);
HandleMiss(_player2, 2);
}
}

if (_player1Notes.Count == 0 && (!_isTwoPlayerMode || _player2Notes.Count == 0))
{
ShowGameOverScreen();
return;
}
}

UpdateNotePositions();
}


private void ResetGameData()
{
_player1.Score = 0;
_player1.Combo = 0;
_player1.PerfectCount = 0;
_player1.GoodCount = 0;
_player1.BadCount = 0;
_player1.MissCount = 0;
_player1.TotalNotes = 0;

_player2.Score = 0;
_player2.Combo = 0;
_player2.PerfectCount = 0;
_player2.GoodCount = 0;
_player2.BadCount = 0;
_player2.MissCount = 0;
_player2.TotalNotes = 0;

_player1Notes.Clear();
_player2Notes.Clear();
_nextNoteIndex = 0;
_songNotesP1.Clear();
_songNotesP2.Clear();
_nextNoteIndexP1 = 0;
_nextNoteIndexP2 = 0;
_currentGameTime = 0;
}

private void HideDrum1Components()
{
if (_drumLeftRedD != null) _drumLeftRedD.Visibility = Visibility.Hidden;
if (_drumLeftRedF != null) _drumLeftRedF.Visibility = Visibility.Hidden;
if (_drumRightBlueK != null) _drumRightBlueK.Visibility = Visibility.Hidden;
if (_drumRightBlueD != null) _drumRightBlueD.Visibility = Visibility.Hidden;
}

private void HideDrum2Components()
{
if (_drum2LeftRedD != null) _drum2LeftRedD.Visibility = Visibility.Hidden;
if (_drum2LeftRedF != null) _drum2LeftRedF.Visibility = Visibility.Hidden;
if (_drum2RightBlueK != null) _drum2RightBlueK.Visibility = Visibility.Hidden;
if (_drum2RightBlueD != null) _drum2RightBlueD.Visibility = Visibility.Hidden;
}


private void ForceRemoveNote(int playerNumber, bool isRed)
{
var noteList = (playerNumber == 1) ? _player1Notes : _player2Notes;


var noteToRemove = noteList.FirstOrDefault(n => !n.IsHit && n.IsRed == isRed);

if (noteToRemove != null)
{
noteToRemove.IsHit = true;
GameCanvas.Children.Remove(noteToRemove.UIElement);
noteList.Remove(noteToRemove);
Debug.WriteLine($"[ForceRemoveNote] Removed note for player {playerNumber}, isRed={isRed}");
}
else
{
Debug.WriteLine($"[ForceRemoveNote] No note found for player {playerNumber}, isRed={isRed}");
}
}

private void UpdateTutorialDisplays()
{
if (_tutorialP1Text == null || _tutorialP2Text == null) return;
if (!_isTwoPlayerMode || _isOnlineMode)
{
_tutorialP1Text.Inlines.Clear();
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run("DF("));
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run("red") { Foreground = Brushes.Red });
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run(") "));
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run("JK("));
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run("Blue") { Foreground = Brushes.LightBlue });
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run(")"));

_tutorialP1Text.Visibility = Visibility.Visible;
_tutorialP2Text.Visibility = Visibility.Collapsed;
}

else if (_isOnlineMode)
{
_tutorialP1Text.Inlines.Clear();
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run("DF("));
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run("red") { Foreground = Brushes.Red });
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run(") "));
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run("JK("));
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run("Blue") { Foreground = Brushes.LightBlue });
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run(")"));

_tutorialP1Text.Visibility = Visibility.Visible;
_tutorialP2Text.Visibility = Visibility.Collapsed;
}
if (_isTwoPlayerMode)
{
_tutorialP1Text.Inlines.Clear();
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run("P1: "));
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run("DF(") );
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run("red") { Foreground = Brushes.Red });
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run(") "));
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run("JK("));
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run("Blue") { Foreground = Brushes.LightBlue });
_tutorialP1Text.Inlines.Add(new System.Windows.Documents.Run(")"));

_tutorialP2Text.Inlines.Clear();
_tutorialP2Text.Inlines.Add(new System.Windows.Documents.Run("P2: "));
_tutorialP2Text.Inlines.Add(new System.Windows.Documents.Run("12(") );
_tutorialP2Text.Inlines.Add(new System.Windows.Documents.Run("red") { Foreground = Brushes.Red });
_tutorialP2Text.Inlines.Add(new System.Windows.Documents.Run(") "));
_tutorialP2Text.Inlines.Add(new System.Windows.Documents.Run("45("));
_tutorialP2Text.Inlines.Add(new System.Windows.Documents.Run("Blue") { Foreground = Brushes.LightBlue });
_tutorialP2Text.Inlines.Add(new System.Windows.Documents.Run(")"));

_tutorialP1Text.Visibility = Visibility.Visible;
_tutorialP2Text.Visibility = Visibility.Visible;
}
}

}
}
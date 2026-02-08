using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Diagnostics;

namespace TaikoGame.UI
{
    public interface IGameScreen
    {
        void Show(Canvas canvas);
        void Hide(Canvas canvas);
    }

    public class TitleScreen : IGameScreen
    {
        private Canvas? _canvas;
        private Button? _startButton;
        private TextBlock? _titleText;

        public event Action? OnStartGame;

        public void Show(Canvas canvas)
        {
            _canvas = canvas;
            _canvas.Children.Clear();
            _canvas.Background = new SolidColorBrush(Color.FromRgb(25, 25, 112));

            // Grid
            Grid mainGrid = new Grid
            {
                Width = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 1000,
                Height = _canvas.ActualHeight > 0 ? _canvas.ActualHeight : 600
            };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            StackPanel leftPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };

            _titleText = new TextBlock
            {
                Text = "太鼓の達人",
                FontSize = 56,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 8,
                    ShadowDepth = 4,
                    Opacity = 0.9
                },
                Margin = new Thickness(0, 0, 0, 20)
            };

            TextBlock subtitleText = new TextBlock
            {
                Text = "A Rhythm Game",
                FontSize = 28,
                Foreground = new SolidColorBrush(Colors.LightYellow),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 5,
                    ShadowDepth = 3,
                    Opacity = 0.8
                }
            };

            leftPanel.Children.Add(_titleText);
            leftPanel.Children.Add(subtitleText);

            Grid.SetColumn(leftPanel, 0);
            mainGrid.Children.Add(leftPanel);
            _startButton = new Button
            {
                Content = "START\nGAME",
                Width = 180,
                Height = 180,
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Colors.DarkRed),
                Foreground = Brushes.White,
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(3),
                BorderBrush = new SolidColorBrush(Colors.Gold),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            _startButton.Click += (s, e) => 
            {
                Debug.WriteLine("START GAME button clicked!");
                OnStartGame?.Invoke();
            };
            _startButton.MouseEnter += (s, e) => 
            {
                _startButton.Background = new SolidColorBrush(Colors.OrangeRed);
                _startButton.Foreground = Brushes.Yellow;
            };
            _startButton.MouseLeave += (s, e) => 
            {
                _startButton.Background = new SolidColorBrush(Colors.DarkRed);
                _startButton.Foreground = Brushes.White;
            };

            Grid.SetColumn(_startButton, 1);
            mainGrid.Children.Add(_startButton);
            _canvas.Children.Add(mainGrid);

            Debug.WriteLine("TitleScreen displayed");
        }

        public void Hide(Canvas canvas)
        {
            canvas.Children.Clear();
        }
    }

    public class SongSelectionScreen : IGameScreen
    {
        private Canvas? _canvas;
        private List<Button> _songButtons = new List<Button>();

        public event Action<string>? OnSongSelected;
        public event Action? OnBackToTitle;

        public void Show(Canvas canvas)
        {
            _canvas = canvas;
            _canvas.Children.Clear();

            Debug.WriteLine("SongSelectionScreen displayed");

            LinearGradientBrush bgBrush = new LinearGradientBrush();
            bgBrush.StartPoint = new Point(0, 0);
            bgBrush.EndPoint = new Point(0, 1);
            bgBrush.GradientStops.Add(new GradientStop(Colors.SteelBlue, 0.0));
            bgBrush.GradientStops.Add(new GradientStop(Colors.CornflowerBlue, 1.0));
            _canvas.Background = bgBrush;
            Grid mainGrid = new Grid
            {
                Width = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 1000,
                Height = _canvas.ActualHeight > 0 ? _canvas.ActualHeight : 600
            };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            TextBlock titleText = new TextBlock
            {
                Text = "SELECT YOUR\nSONG",
                FontSize = 44,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 8,
                    ShadowDepth = 4,
                    Opacity = 0.9
                }
            };

            Grid.SetColumn(titleText, 0);
            mainGrid.Children.Add(titleText);
            StackPanel songListPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(20)
            };

            var songs = new List<(string id, string displayName)>
            {
                ("song1", "🎵 Song 1 - Rhythm Starter"),
                ("song2", "🎵 Song 2 - Rhythm Master")
            };

            _songButtons.Clear();

            foreach (var song in songs)
            {
                Button songBtn = new Button
                {
                    Content = song.displayName,
                    Width = 300,
                    Height = 65,
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Background = new SolidColorBrush(Colors.DarkGreen),
                    Foreground = Brushes.White,
                    Cursor = Cursors.Hand,
                    BorderThickness = new Thickness(2),
                    BorderBrush = new SolidColorBrush(Colors.LimeGreen),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 10, 0, 10)
                };

                string songId = song.id;
                songBtn.Click += (s, e) => 
                {
                    Debug.WriteLine($"Song selected: {songId}");
                    OnSongSelected?.Invoke(songId);
                };
                songBtn.MouseEnter += (s, e) => 
                {
                    songBtn.Background = new SolidColorBrush(Colors.ForestGreen);
                    songBtn.Foreground = Brushes.Yellow;
                };
                songBtn.MouseLeave += (s, e) => 
                {
                    songBtn.Background = new SolidColorBrush(Colors.DarkGreen);
                    songBtn.Foreground = Brushes.White;
                };

                songListPanel.Children.Add(songBtn);
                _songButtons.Add(songBtn);
            }

            Button backButton = new Button
            {
                Content = "← BACK TO TITLE",
                Width = 300,
                Height = 55,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Colors.Gray),
                Foreground = Brushes.White,
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 10, 0, 0),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Colors.DarkGray)
            };

            backButton.Click += (s, e) => 
            {
                Debug.WriteLine("Back to title button clicked");
                OnBackToTitle?.Invoke();
            };
            backButton.MouseEnter += (s, e) => 
            {
                backButton.Background = new SolidColorBrush(Colors.DarkGray);
                backButton.Foreground = Brushes.Yellow;
            };
            backButton.MouseLeave += (s, e) => 
            {
                backButton.Background = new SolidColorBrush(Colors.Gray);
                backButton.Foreground = Brushes.White;
            };

            songListPanel.Children.Add(backButton);

            Grid.SetColumn(songListPanel, 1);
            mainGrid.Children.Add(songListPanel);

            // 添加到 Canvas
            _canvas.Children.Add(mainGrid);

            Debug.WriteLine("SongSelectionScreen displayed");
        }

        public void Hide(Canvas canvas)
        {
            canvas.Children.Clear();
        }
    }

    public class GameOverScreen : IGameScreen
    {
        private Canvas? _canvas;

        public int Score { get; set; }
        public int PerfectCount { get; set; }
        public int GoodCount { get; set; }
        public int BadCount { get; set; }
        public int MissCount { get; set; }
        public double Accuracy { get; set; }

        public event Action? OnReturnToSongSelection;
        public event Action? OnReturnToTitle;

        public void Show(Canvas canvas)
        {
            _canvas = canvas;
            _canvas.Children.Clear();

            Debug.WriteLine("GameOverScreen displayed");

            LinearGradientBrush bgBrush = new LinearGradientBrush();
            bgBrush.StartPoint = new Point(0, 0);
            bgBrush.EndPoint = new Point(0, 1);
            bgBrush.GradientStops.Add(new GradientStop(Colors.Indigo, 0.0));
            bgBrush.GradientStops.Add(new GradientStop(Colors.Purple, 1.0));
            _canvas.Background = bgBrush;

            ScrollViewer scrollViewer = new ScrollViewer
            {
                Width = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 1000,
                Height = _canvas.ActualHeight > 0 ? _canvas.ActualHeight : 600,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            StackPanel mainPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(20)
            };

            TextBlock gameOverTitle = new TextBlock
            {
                Text = "GAME OVER",
                FontSize = 64,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Gold),
                TextAlignment = TextAlignment.Center,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 5,
                    ShadowDepth = 3,
                    Opacity = 0.8
                },
                Margin = new Thickness(0, 0, 0, 30)
            };

            mainPanel.Children.Add(gameOverTitle);

            Border resultBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(47, 79, 79)),
                BorderThickness = new Thickness(3),
                BorderBrush = new SolidColorBrush(Colors.Gold),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(40, 30, 40, 30),
                Width = 500
            };

            StackPanel resultPanel = new StackPanel();

            resultPanel.Children.Add(CreateScoreRow("FINAL SCORE", Score.ToString(), Brushes.Gold, 40));
            resultPanel.Children.Add(CreateScoreRow("ACCURACY", $"{Accuracy:F1}%", Brushes.LimeGreen, 28));

            Rectangle separatorLine = new Rectangle
            {
                Width = 400,
                Height = 2,
                Fill = new SolidColorBrush(Colors.Gray),
                Margin = new Thickness(0, 10, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            resultPanel.Children.Add(separatorLine);

            resultPanel.Children.Add(CreateScoreRow("PERFECT", PerfectCount.ToString(), Brushes.Gold, 22));
            resultPanel.Children.Add(CreateScoreRow("GOOD", GoodCount.ToString(), Brushes.LimeGreen, 22));
            resultPanel.Children.Add(CreateScoreRow("BAD", BadCount.ToString(), Brushes.Orange, 22));
            resultPanel.Children.Add(CreateScoreRow("MISS", MissCount.ToString(), Brushes.Red, 22));

            resultBorder.Child = resultPanel;
            mainPanel.Children.Add(resultBorder);

            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            };

            Button nextSongButton = new Button
            {
                Content = "NEXT SONG",
                Width = 200,
                Height = 60,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Colors.DarkGreen),
                Foreground = Brushes.White,
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 30, 0)
            };

            nextSongButton.Click += (s, e) => OnReturnToSongSelection?.Invoke();
            nextSongButton.MouseEnter += (s, e) => nextSongButton.Background = new SolidColorBrush(Colors.ForestGreen);
            nextSongButton.MouseLeave += (s, e) => nextSongButton.Background = new SolidColorBrush(Colors.DarkGreen);

            Button mainMenuButton = new Button
            {
                Content = "MAIN MENU",
                Width = 200,
                Height = 60,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Colors.DarkRed),
                Foreground = Brushes.White,
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(0)
            };

            mainMenuButton.Click += (s, e) => OnReturnToTitle?.Invoke();
            mainMenuButton.MouseEnter += (s, e) => mainMenuButton.Background = new SolidColorBrush(Colors.Crimson);
            mainMenuButton.MouseLeave += (s, e) => mainMenuButton.Background = new SolidColorBrush(Colors.DarkRed);

            buttonPanel.Children.Add(nextSongButton);
            buttonPanel.Children.Add(mainMenuButton);

            mainPanel.Children.Add(buttonPanel);

            scrollViewer.Content = mainPanel;
            _canvas.Children.Add(scrollViewer);
        }

        public void Hide(Canvas canvas)
        {
            canvas.Children.Clear();
        }

        private StackPanel CreateScoreRow(string label, string value, Brush valueBrush, int fontSize)
        {
            StackPanel row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 8, 0, 8)
            };

            TextBlock labelText = new TextBlock
            {
                Text = label,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                MinWidth = 150
            };

            TextBlock valueText = new TextBlock
            {
                Text = value,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                Foreground = valueBrush,
                MinWidth = 100,
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(20, 0, 0, 0)
            };

            row.Children.Add(labelText);
            row.Children.Add(valueText);

            return row;
        }
    }
}
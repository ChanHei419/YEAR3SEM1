using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace TaikoGame
{
public class FloatingImage
{
public Image? Element { get; set; }
public double VelocityX { get; set; }
public double VelocityY { get; set; }
public double X { get; set; }
public double Y { get; set; }
public double Life { get; set; } // 0-1
public double MaxLife { get; set; }
public double Rotation { get; set; }
public double RotationSpeed { get; set; }
}

public class FloatingImageParticleSystem
{
private Canvas _canvas;
private DispatcherTimer? _particleTimer;
private List<FloatingImage> _floatingImages = new List<FloatingImage>();
private Random _random = new Random();
private const int MaxFloatingImages = 8;
private List<string> _imagePaths = new List<string>();

public FloatingImageParticleSystem(Canvas canvas)
{
_canvas = canvas;
LoadImagePaths();
InitializeParticleTimer();
}

private void LoadImagePaths()
{
try
{
string basePath = AppDomain.CurrentDomain.BaseDirectory;
string photosDir = Path.Combine(basePath, "photos");

if (!Directory.Exists(photosDir))
{
basePath = Path.GetDirectoryName(basePath) ?? basePath;
photosDir = Path.Combine(basePath, "photos");
}

if (!Directory.Exists(photosDir))
{
basePath = Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)) ?? AppDomain.CurrentDomain.BaseDirectory;
photosDir = Path.Combine(basePath, "photos");
}

for (int i = 1; i <= 10; i++)
{
string imagePath = Path.Combine(photosDir, $"{i}.png");
if (File.Exists(imagePath))
{
_imagePaths.Add(imagePath);
}
}
}
catch (Exception ex)
{
System.Diagnostics.Debug.WriteLine($"[FloatingImageParticleSystem] Error loading images: {ex.Message}");
}
}

private void InitializeParticleTimer()
{
_particleTimer = new DispatcherTimer();
_particleTimer.Interval = TimeSpan.FromMilliseconds(100);
_particleTimer.Tick += ParticleTimerTick;
}

public void Start()
{
_particleTimer?.Start();
}

public void Stop()
{
_particleTimer?.Stop();
}

public void Clear()
{
foreach (var floatingImage in _floatingImages)
{
if (floatingImage.Element != null && _canvas.Children.Contains(floatingImage.Element))
{
_canvas.Children.Remove(floatingImage.Element);
}
}
_floatingImages.Clear();
}

private void ParticleTimerTick(object? sender, EventArgs e)
{
if (_floatingImages.Count < MaxFloatingImages && _imagePaths.Count > 0 && _random.NextDouble() > 0.6)
{
SpawnFloatingImage();
}
var deadImages = new List<FloatingImage>();
foreach (var floatingImage in _floatingImages)
{
floatingImage.X += floatingImage.VelocityX;
floatingImage.Y += floatingImage.VelocityY;
floatingImage.Life -= 1.0 / 60.0;
floatingImage.Rotation += floatingImage.RotationSpeed;
if (floatingImage.Element != null)
{Canvas.SetLeft(floatingImage.Element, floatingImage.X);
Canvas.SetTop(floatingImage.Element, floatingImage.Y);
floatingImage.Element.Opacity = Math.Max(0, floatingImage.Life / floatingImage.MaxLife);
var rotateTransform = new System.Windows.Media.RotateTransform(
floatingImage.Rotation,
floatingImage.Element.ActualWidth / 2,
floatingImage.Element.ActualHeight / 2
);
floatingImage.Element.RenderTransform = rotateTransform;
}
if (floatingImage.Life <= 0 || floatingImage.Y < -150 || floatingImage.Y > _canvas.ActualHeight + 150)
{
deadImages.Add(floatingImage);
}
}
foreach (var deadImage in deadImages)
{
if (_canvas.Children.Contains(deadImage.Element))
{
_canvas.Children.Remove(deadImage.Element);
}
_floatingImages.Remove(deadImage);
}
}
private void SpawnFloatingImage()
{
if (_imagePaths.Count == 0) return;

try
{
string imagePath = _imagePaths[_random.Next(_imagePaths.Count)];
double startX = _random.NextDouble() * _canvas.ActualWidth;
double startY = -100;

BitmapImage bitmapImage = new BitmapImage();
bitmapImage.BeginInit();
bitmapImage.UriSource = new Uri(imagePath, UriKind.Absolute);
bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
bitmapImage.EndInit();

Image imageElement = new Image
{
Source = bitmapImage,
Width = _random.Next(60, 120),
Height = _random.Next(60, 120),
IsHitTestVisible = false,
Opacity = 0.5
};

Canvas.SetLeft(imageElement, startX);
Canvas.SetTop(imageElement, startY);
_canvas.Children.Add(imageElement);
Panel.SetZIndex(imageElement, 2);

double velocityX = (_random.NextDouble() - 0.5) * 1.5;
double velocityY = _random.NextDouble() * 0.8 + 0.5;

FloatingImage floatingImage = new FloatingImage
{
Element = imageElement,
X = startX,
Y = startY,
VelocityX = velocityX,
VelocityY = velocityY,
Life = 1.0,
MaxLife = _random.Next(8, 15),
Rotation = 0,
RotationSpeed = (_random.NextDouble() - 0.5) * 5
};

_floatingImages.Add(floatingImage);
}
catch (Exception ex)
{
System.Diagnostics.Debug.WriteLine($"[FloatingImageParticleSystem] Error spawning image: {ex.Message}");
}
}
}
}
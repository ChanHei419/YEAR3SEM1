using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TaikoGame
{
public class Particle
{
public UIElement? Element { get; set; }
public double VelocityX { get; set; }
public double VelocityY { get; set; }
public double X { get; set; }
public double Y { get; set; }
public double Life { get; set; }
public double MaxLife { get; set; }
}

public enum ParticleType
{
Star,
Heart,
Square
}

public class BackgroundParticleSystem
{
private Canvas _canvas;
private System.Windows.Threading.DispatcherTimer? _particleTimer;
private System.Collections.Generic.List<Particle> _particles = new System.Collections.Generic.List<Particle>();
private Random _random = new Random();
private const int MaxParticles = 30;

public BackgroundParticleSystem(Canvas canvas)
{
_canvas = canvas;
InitializeParticleTimer();
}

private void InitializeParticleTimer()
{
_particleTimer = new System.Windows.Threading.DispatcherTimer();
_particleTimer.Interval = TimeSpan.FromMilliseconds(50);
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
foreach (var particle in _particles)
{
if (particle.Element != null && _canvas.Children.Contains(particle.Element))
{
_canvas.Children.Remove(particle.Element);
}
}
_particles.Clear();
}

private void ParticleTimerTick(object? sender, EventArgs e)
{
if (_particles.Count < MaxParticles && _random.NextDouble() > 0.4)
{
SpawnParticle();
}
var deadParticles = new System.Collections.Generic.List<Particle>();
foreach (var particle in _particles)
{
particle.X += particle.VelocityX;
particle.Y += particle.VelocityY;
particle.Life -= 1.0 / 60.0; 
if (particle.Element is Shape shape)
{Canvas.SetLeft(particle.Element, particle.X);
Canvas.SetTop(particle.Element, particle.Y);
Brush brush = shape.Fill;
if (brush is SolidColorBrush solidBrush)
{var color = solidBrush.Color;
brush = new SolidColorBrush(Color.FromArgb(
(byte)(255 * Math.Max(0, particle.Life / particle.MaxLife)),
color.R,
color.G,
color.B
));
shape.Fill = brush;
}
}
if (particle.Life <= 0 || particle.Y < -100 || particle.Element == null || particle.Y > _canvas.ActualHeight + 100)
{deadParticles.Add(particle);
}
}
foreach (var deadParticle in deadParticles)
{
if (deadParticle.Element != null && _canvas.Children.Contains(deadParticle.Element))
{
_canvas.Children.Remove(deadParticle.Element);
}
_particles.Remove(deadParticle);
}
}
private void SpawnParticle()
{
ParticleType type = (ParticleType)_random.Next(0, 3);
double startX = _random.NextDouble() * _canvas.ActualWidth;
double startY = _random.NextDouble() * _canvas.ActualHeight;

UIElement? element = null;
double size = _random.Next(10, 30);

switch (type)
{
case ParticleType.Star:
element = CreateStar(size);
break;
case ParticleType.Heart:
element = CreateHeart(size);
break;
case ParticleType.Square:
element = CreateSquare(size);
break;
}

if (element != null)
{
Canvas.SetLeft(element, startX);
Canvas.SetTop(element, startY);
_canvas.Children.Add(element);
Panel.SetZIndex(element, 5);

Particle particle = new Particle
{
Element = element,
X = startX,
Y = startY,
VelocityX = (_random.NextDouble() - 0.5) * 2,
VelocityY = _random.NextDouble() * 1 - 0.5,
Life = 1.0,
MaxLife = _random.Next(3, 8)
};

_particles.Add(particle);
}
}

private UIElement CreateStar(double size)
{
PathGeometry starGeometry = new PathGeometry();
PathFigure starFigure = new PathFigure();
double angle = -Math.PI / 2;
double points = 5;
double outerRadius = size / 2;
double innerRadius = size / 4;
Point[] starPoints = new Point[10];
for (int i = 0; i < 10; i++)
{
double radius = (i % 2 == 0) ? outerRadius : innerRadius;
double currentAngle = angle + (i * Math.PI / points);
starPoints[i] = new Point(
size / 2 + radius * Math.Cos(currentAngle),
size / 2 + radius * Math.Sin(currentAngle)
);
}
starFigure.StartPoint = starPoints[0];
for (int i = 1; i < 10; i++)
{
starFigure.Segments.Add(new LineSegment(starPoints[i], true));
}
starFigure.IsClosed = true;

starGeometry.Figures.Add(starFigure);

Path star = new Path
{
Data = starGeometry,
Fill = new SolidColorBrush(GetRandomColor()),
Stroke = Brushes.White,
StrokeThickness = 1,
IsHitTestVisible = false
};
return star;
}

private UIElement CreateHeart(double size)
{
PathGeometry heartGeometry = new PathGeometry();
PathFigure heartFigure = new PathFigure();
double x = size / 2;
double y = size / 2;

heartFigure.StartPoint = new Point(x, y + size * 0.3);
heartFigure.Segments.Add(new ArcSegment(
new Point(x - size * 0.3, y),
new Size(size * 0.3, size * 0.3),
0, false, SweepDirection.Clockwise, true
));
heartFigure.Segments.Add(new ArcSegment(
new Point(x + size * 0.3, y),
new Size(size * 0.3, size * 0.3),
0, false, SweepDirection.Clockwise, true
));
heartFigure.Segments.Add(new LineSegment(new Point(x, y + size * 0.5), true));
heartFigure.IsClosed = true;
heartGeometry.Figures.Add(heartFigure);
Path heart = new Path
{
Data = heartGeometry,
Fill = new SolidColorBrush(GetRandomColor()),
Stroke = Brushes.White,
StrokeThickness = 1,
IsHitTestVisible = false
};

return heart;
}
private UIElement CreateSquare(double size)
{
Rectangle square = new Rectangle
{
Width = size,
Height = size,
Fill = new SolidColorBrush(GetRandomColor()),
Stroke = Brushes.White,
StrokeThickness = 1,
IsHitTestVisible = false
};
return square;
}
private Color GetRandomColor()
{
Color[] colors = new Color[]
{
Colors.Red,
Colors.Yellow,
Colors.Cyan,
Colors.Lime,
Colors.Magenta,
Colors.Gold,
Colors.DeepSkyBlue,
Colors.LightCoral,
Colors.LightGreen
};

return colors[_random.Next(colors.Length)];
}
}
}
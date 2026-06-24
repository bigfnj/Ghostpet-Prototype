using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace GhostPet.Controls;

public partial class GhostPanel : UserControl
{
    private readonly BitmapImage _standImage =
        new(new Uri("pack://application:,,,/Resources/images/stand.png"));

    public event EventHandler? GhostMouseMoved;
    public event EventHandler? TestTalkRequested;
    public event EventHandler? QuitRequested;
    public event EventHandler? SnapToCornerRequested;

    public GhostPanel()
    {
        InitializeComponent();
    }

    public void ShowImage(string imageName)
    {
        try
        {
            CharImage.Source = new BitmapImage(
                new Uri($"pack://application:,,,/Resources/images/{imageName}"));
        }
        catch
        {
            CharImage.Source = _standImage;
        }
    }

    public void Reset() => CharImage.Source = _standImage;

    private void CharImage_MouseMove(object sender, MouseEventArgs e) =>
        GhostMouseMoved?.Invoke(this, EventArgs.Empty);

    private void CharImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
        Window.GetWindow(this)?.DragMove();

    private void MenuTestTalk_Click(object sender, RoutedEventArgs e) =>
        TestTalkRequested?.Invoke(this, EventArgs.Empty);

    private void MenuSnapToCorner_Click(object sender, RoutedEventArgs e) =>
        SnapToCornerRequested?.Invoke(this, EventArgs.Empty);

    private void MenuQuit_Click(object sender, RoutedEventArgs e) =>
        QuitRequested?.Invoke(this, EventArgs.Empty);
}

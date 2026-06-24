using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace GhostPet.Controls;

public sealed record ChoiceItem(string Label, string? BehaviorName);

public partial class SpeakPanel : UserControl
{
    public event EventHandler<string?>? ChoiceSelected;

    public SpeakPanel()
    {
        InitializeComponent();
    }

    public void ShowSpeech(string text, string ghostName, string userName)
    {
        SpeakText.Text = Expand(text, ghostName, userName);
        ChoiceList.ItemsSource = null;
        Visibility = Visibility.Visible;
    }

    public void ShowAsk(string text, string[] options, string[]? behaviorNames,
                        string ghostName, string userName)
    {
        SpeakText.Text = Expand(text, ghostName, userName);

        ChoiceList.ItemsSource = options
            .Select((opt, i) => new ChoiceItem(
                "* " + Expand(opt, ghostName, userName),
                i < (behaviorNames?.Length ?? 0) ? behaviorNames![i] : null))
            .ToList();

        Visibility = Visibility.Visible;
    }

    public void Reset()
    {
        ChoiceList.ItemsSource = null;
        Visibility = Visibility.Collapsed;
    }

    private static string Expand(string text, string ghostName, string userName) =>
        text.Replace("$NAME$", userName).Replace("$GNAME$", ghostName);

    private void Choice_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBlock { Tag: string name })
            ChoiceSelected?.Invoke(this, name);
    }
}

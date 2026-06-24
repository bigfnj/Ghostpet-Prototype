using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using GhostPet.Behaviors;
using GhostPet.Models;
using WinForms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace GhostPet;

public partial class MainWindow : Window, IBehaviorContext
{
    private GhostConfig _config = new();
    private List<BehaviorBase> _behaviors = [];
    private DispatcherTimer _timer = new();
    private DispatcherTimer _idleTimer = new();
    private WinForms.NotifyIcon _trayIcon = null!;

    public string GhostName => _config.GhostName;
    public string UserName => _config.UserName;
    public GhostState State { get; set; } = GhostState.Passive;

    public MainWindow()
    {
        InitializeComponent();
        LoadConfig();
        LoadBehaviors();
        InitTrayIcon();

        SpeakPanel.ChoiceSelected += OnChoiceSelected;
        GhostPanel.GhostMouseMoved += OnGhostMouseMoved;
        GhostPanel.TestTalkRequested += OnTestTalk;
        GhostPanel.SnapToCornerRequested += (_, _) => SnapToRightCorner();
        GhostPanel.QuitRequested += (_, _) => Quit();

        Loaded += (_, _) =>
        {
            SnapToRightCorner();
            _trayIcon.ShowBalloonTip(3000, "GhostPet",
                $"{GhostName} is here! Drag to move. Right-click tray or sprite for options.",
                WinForms.ToolTipIcon.Info);
            ScheduleNextIdle(initialDelay: true);
        };
    }

    // ── Idle loop ────────────────────────────────────────────────────────────

    // Reactive behaviors are excluded from the idle pool; they fire on triggers
    private static readonly HashSet<string> _reactiveNames =
        ["PetBehavior", "GiveUpBehavior"];

    private void ScheduleNextIdle(bool initialDelay = false)
    {
        _idleTimer.Stop();
        _idleTimer = new DispatcherTimer
        {
            // First trigger: 15-30s so the pet settles in before talking
            // Subsequent: 60-120s for a natural, occasional cadence
            Interval = initialDelay
                ? TimeSpan.FromSeconds(Random.Shared.Next(15, 30))
                : TimeSpan.FromSeconds(Random.Shared.Next(60, 120))
        };
        _idleTimer.Tick += OnIdleTick;
        _idleTimer.Start();
    }

    private void OnIdleTick(object? sender, EventArgs e)
    {
        _idleTimer.Stop();

        if (State == GhostState.Passive)
        {
            var pool = _behaviors
                .Where(b => !_reactiveNames.Contains(BehaviorName(b) ?? ""))
                .ToList();

            if (pool.Count > 0)
                Say(pool[Random.Shared.Next(pool.Count)]);
        }

        ScheduleNextIdle();
    }

    private static string? BehaviorName(BehaviorBase b) =>
        b.GetType().GetCustomAttribute<BehaviorNameAttribute>()?.Name;

    // ── Tray icon ────────────────────────────────────────────────────────────

    private void InitTrayIcon()
    {
        Drawing.Icon icon;
        try
        {
            var uri = new Uri("pack://application:,,,/Resources/images/stand.png");
            var info = Application.GetResourceStream(uri)!;
            using var bmp = new Drawing.Bitmap(info.Stream);
            using var resized = new Drawing.Bitmap(bmp, 32, 32);
            icon = Drawing.Icon.FromHandle(resized.GetHicon());
        }
        catch
        {
            icon = Drawing.SystemIcons.Application;
        }

        var menu = new WinForms.ContextMenuStrip();
        menu.Items.Add("Test Talk", null, (_, _) => OnTestTalk(null, EventArgs.Empty));
        menu.Items.Add("Move Ghost", null, (_, _) => SnapToRightCorner());
        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add("Quit", null, (_, _) => Quit());

        _trayIcon = new WinForms.NotifyIcon
        {
            Icon = icon,
            Text = $"GhostPet — {GhostName}",
            ContextMenuStrip = menu,
            Visible = true
        };
    }

    private void Quit()
    {
        _idleTimer.Stop();
        _timer.Stop();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Current.Shutdown();
    }

    protected override void OnClosed(EventArgs e)
    {
        _idleTimer.Stop();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        base.OnClosed(e);
    }

    // ── Setup ────────────────────────────────────────────────────────────────

    private void LoadConfig()
    {
        try
        {
            var uri = new Uri("pack://application:,,,/Resources/behaviors/GhostConfig.json");
            var info = Application.GetResourceStream(uri);
            if (info is null) return;

            _config = JsonSerializer.Deserialize<GhostConfig>(info.Stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new GhostConfig();
        }
        catch { }
    }

    private void LoadBehaviors() => _behaviors = BehaviorRegistry.CreateAll(this);

    private void SnapToRightCorner()
    {
        var screen = WinForms.Screen.AllScreens
            .OrderByDescending(s => s.WorkingArea.Right)
            .First();

        var source = PresentationSource.FromVisual(this);
        double scaleX = source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
        double scaleY = source?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;

        Left = screen.WorkingArea.Right / scaleX - Width;
        Top  = screen.WorkingArea.Bottom / scaleY - Height;
    }

    // ── IBehaviorContext ─────────────────────────────────────────────────────

    public void Say(BehaviorBase behavior)
    {
        _timer.Stop();
        _timer = new DispatcherTimer { IsEnabled = false };
        State = behavior.EntersState;

        switch (behavior)
        {
            case BehaviorAskBase ask:
                SpeakPanel.ShowAsk(
                    ask.GetRandomStatement(),
                    ask.ActStatements ?? [],
                    ask.BehaviorList,
                    GhostName, UserName);
                if (ask.ImageUsed is not null) GhostPanel.ShowImage(ask.ImageUsed);

                int askTimeout = ask.ActTime > 0 ? ask.ActTime : 180;
                _timer.Interval = TimeSpan.FromSeconds(askTimeout);
                _timer.Tick += (_, _) =>
                {
                    _timer.Stop();
                    Reset();
                    GetBehavior("GiveUpBehavior")?.Execute();
                };
                _timer.Start();
                break;

            case BehaviorLongTalkBase lt when lt.StmtChains is not null:
                int chain = lt.ChainUsed;
                int idx = lt.ChainIndex;

                if (idx < lt.StmtChains[chain].Length)
                {
                    SpeakPanel.ShowSpeech(lt.StmtChains[chain][idx], GhostName, UserName);
                    GhostPanel.ShowImage(lt.ResolveImage(chain, idx));

                    int delay = lt.TimeChains?[chain][idx] ?? 3;
                    lt.ChainIndex++;

                    _timer.Interval = TimeSpan.FromSeconds(delay);
                    _timer.Tick += (_, _) =>
                    {
                        _timer.Stop();
                        Reset();
                        Say(behavior);
                    };
                    _timer.Start();
                }
                else
                {
                    lt.PickNextChain();
                }
                break;

            default:
                SpeakPanel.ShowSpeech(behavior.GetRandomStatement(), GhostName, UserName);
                if (behavior.ImageUsed is not null) GhostPanel.ShowImage(behavior.ImageUsed);

                int duration = behavior.ActTime > 0 ? behavior.ActTime : 3;
                _timer.Interval = TimeSpan.FromSeconds(duration);
                _timer.Tick += (_, _) =>
                {
                    _timer.Stop();
                    Reset();
                };
                _timer.Start();
                break;
        }
    }

    public BehaviorBase? GetBehavior(string name) =>
        _behaviors.FirstOrDefault(b => BehaviorName(b) == name);

    public void Reset()
    {
        SpeakPanel.Reset();
        GhostPanel.Reset();
        State = GhostState.Passive;
    }

    // ── Event handlers ───────────────────────────────────────────────────────

    private void OnChoiceSelected(object? sender, string? name)
    {
        Reset();
        if (name is not null) GetBehavior(name)?.Execute();
    }

    private void OnGhostMouseMoved(object? sender, EventArgs e) =>
        GetBehavior("PetBehavior")?.Execute();

    private void OnTestTalk(object? sender, EventArgs e)
    {
        var b = GetBehavior("GenLongTalkBehavior");
        if (b is not null) Say(b);
    }
}

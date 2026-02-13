using System.Text.Json;

namespace WalkUpCounter;

public class MainForm : Form
{
    private Label _countLabel;
    private Label _dateLabel;
    private Label _titleLabel;
    private Label _requestsLabel;
    private Label _hintLabel;
    private Label _historyLabel;
    private Button _addButton;
    private Button _undoButton;
    private Button _resetButton;
    private ListBox _historyList;
    private Panel _countPanel;
    private NotifyIcon _trayIcon;
    private ContextMenuStrip _trayMenu;
    
    private CounterData _data;
    private readonly string _dataFilePath;
    
    // Colors
    private readonly Color _bgDark = Color.FromArgb(13, 13, 13);
    private readonly Color _surface = Color.FromArgb(26, 26, 26);
    private readonly Color _border = Color.FromArgb(42, 42, 42);
    private readonly Color _textPrimary = Color.FromArgb(232, 232, 232);
    private readonly Color _textSecondary = Color.FromArgb(136, 136, 136);
    private readonly Color _accent = Color.FromArgb(0, 212, 170);
    
    public MainForm()
    {
        // Data file in same directory as exe
        _dataFilePath = Path.Combine(AppContext.BaseDirectory, "walkup_data.json");
        _data = LoadData();
        
        InitializeComponent();
        SetupTrayIcon();
        UpdateDisplay();
        
        // Check for day change every minute
        var timer = new System.Windows.Forms.Timer { Interval = 60000 };
        timer.Tick += (s, e) => UpdateDisplay();
        timer.Start();
    }
    
    private void InitializeComponent()
    {
        // Form settings
        Text = "Walk-Up Counter";
        Size = new Size(300, 420);
        MinimumSize = new Size(250, 350);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = _bgDark;
        TopMost = true;
        ShowInTaskbar = true;
        KeyPreview = true;
        
        // Date label
        _dateLabel = new Label
        {
            Text = DateTime.Now.ToString("ddd, MMM d"),
            ForeColor = _textSecondary,
            Font = new Font("Segoe UI", 9f),
            Location = new Point(20, 15),
            AutoSize = true
        };
        Controls.Add(_dateLabel);
        
        // Title label
        _titleLabel = new Label
        {
            Text = "WALK-UP COUNTER",
            ForeColor = _textSecondary,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        Controls.Add(_titleLabel);
        
        // Count display panel
        _countPanel = new Panel
        {
            BackColor = _surface,
            Location = new Point(20, 45),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        Controls.Add(_countPanel);
        
        // Count label
        _countLabel = new Label
        {
            Text = "0",
            ForeColor = _textPrimary,
            Font = new Font("Consolas", 56f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };
        _countPanel.Controls.Add(_countLabel);
        
        // "requests today" label
        _requestsLabel = new Label
        {
            Text = "requests today",
            ForeColor = _textSecondary,
            Font = new Font("Segoe UI", 10f),
            AutoSize = true,
            Anchor = AnchorStyles.Top
        };
        Controls.Add(_requestsLabel);
        
        // Add button
        _addButton = new Button
        {
            Text = "+ Add Request",
            ForeColor = _bgDark,
            BackColor = _accent,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        _addButton.FlatAppearance.BorderSize = 0;
        _addButton.Click += (s, e) => AddCount();
        Controls.Add(_addButton);
        
        // Undo button
        _undoButton = new Button
        {
            Text = "Undo",
            ForeColor = _textSecondary,
            BackColor = _surface,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        _undoButton.FlatAppearance.BorderColor = _border;
        _undoButton.Click += (s, e) => UndoCount();
        Controls.Add(_undoButton);
        
        // Reset button
        _resetButton = new Button
        {
            Text = "Reset Day",
            ForeColor = _textSecondary,
            BackColor = _surface,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _resetButton.FlatAppearance.BorderColor = _border;
        _resetButton.Click += (s, e) => ResetDay();
        Controls.Add(_resetButton);
        
        // History label
        _historyLabel = new Label
        {
            Text = "RECENT DAYS",
            ForeColor = _textSecondary,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            AutoSize = true,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        Controls.Add(_historyLabel);
        
        // History list
        _historyList = new ListBox
        {
            BackColor = _surface,
            ForeColor = _textSecondary,
            Font = new Font("Consolas", 9f),
            BorderStyle = BorderStyle.None,
            SelectionMode = SelectionMode.None,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        Controls.Add(_historyList);
        
        // Shortcut hint
        _hintLabel = new Label
        {
            Text = "Press Space or + to add",
            ForeColor = Color.FromArgb(68, 68, 68),
            Font = new Font("Segoe UI", 8f),
            AutoSize = true,
            Anchor = AnchorStyles.Bottom
        };
        Controls.Add(_hintLabel);
        
        // Keyboard handler
        KeyDown += MainForm_KeyDown;
        
        // Handle resize
        Resize += (s, e) => LayoutControls();
        LayoutControls();
    }
    
    private void LayoutControls()
    {
        int padding = 20;
        int w = ClientSize.Width;
        int h = ClientSize.Height;
        
        // Title label (top right)
        _titleLabel.Location = new Point(w - _titleLabel.Width - padding, 17);
        
        // Count panel
        int panelHeight = Math.Max(80, (int)(h * 0.28));
        _countPanel.Location = new Point(padding, 45);
        _countPanel.Size = new Size(w - padding * 2, panelHeight);
        
        // Adjust font size based on panel height
        float fontSize = Math.Max(24, Math.Min(72, panelHeight * 0.55f));
        if (Math.Abs(_countLabel.Font.Size - fontSize) > 2)
        {
            _countLabel.Font = new Font("Consolas", fontSize, FontStyle.Bold);
        }
        
        // Requests label (centered below count panel)
        _requestsLabel.Location = new Point((w - _requestsLabel.Width) / 2, _countPanel.Bottom + 8);
        
        // Add button
        int buttonTop = _requestsLabel.Bottom + 15;
        _addButton.Location = new Point(padding, buttonTop);
        _addButton.Size = new Size(w - padding * 2, 45);
        
        // Undo and Reset buttons
        int smallBtnTop = _addButton.Bottom + 10;
        int smallBtnWidth = (w - padding * 2 - 10) / 2;
        _undoButton.Location = new Point(padding, smallBtnTop);
        _undoButton.Size = new Size(smallBtnWidth, 35);
        _resetButton.Location = new Point(padding + smallBtnWidth + 10, smallBtnTop);
        _resetButton.Size = new Size(smallBtnWidth, 35);
        
        // History section (anchored to bottom)
        int historyHeight = 55;
        _historyList.Location = new Point(padding, h - padding - historyHeight - 25);
        _historyList.Size = new Size(w - padding * 2, historyHeight);
        
        _historyLabel.Location = new Point(padding, _historyList.Top - 20);
        
        // Hint label (centered at bottom)
        _hintLabel.Location = new Point((w - _hintLabel.Width) / 2, h - 20);
    }
    
    private void SetupTrayIcon()
    {
        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("Show", null, (s, e) => { Show(); WindowState = FormWindowState.Normal; });
        _trayMenu.Items.Add("Add +1", null, (s, e) => AddCount());
        _trayMenu.Items.Add("-");
        _trayMenu.Items.Add("Exit", null, (s, e) => { _trayIcon.Visible = false; Application.Exit(); });
        
        _trayIcon = new NotifyIcon
        {
            Text = "Walk-Up Counter",
            Icon = SystemIcons.Application,
            ContextMenuStrip = _trayMenu,
            Visible = true
        };
        
        _trayIcon.DoubleClick += (s, e) => { Show(); WindowState = FormWindowState.Normal; };
        
        UpdateTrayTooltip();
    }
    
    private void UpdateTrayTooltip()
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        var count = _data.Days.GetValueOrDefault(today, 0);
        _trayIcon.Text = $"Walk-Ups Today: {count}";
    }
    
    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            AddCount();
        }
        else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            UndoCount();
        }
    }
    
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            _trayIcon.ShowBalloonTip(1000, "Walk-Up Counter", "Running in background. Double-click tray icon to open.", ToolTipIcon.Info);
        }
        else
        {
            _trayIcon.Visible = false;
        }
        base.OnFormClosing(e);
    }
    
    private void AddCount()
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        _data.Days.TryGetValue(today, out int current);
        _data.Days[today] = current + 1;
        SaveData();
        UpdateDisplay();
        
        _countLabel.ForeColor = _accent;
        Task.Delay(100).ContinueWith(_ => 
            Invoke(() => _countLabel.ForeColor = _textPrimary));
    }
    
    private void UndoCount()
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        if (_data.Days.TryGetValue(today, out int current) && current > 0)
        {
            _data.Days[today] = current - 1;
            SaveData();
            UpdateDisplay();
        }
    }
    
    private void ResetDay()
    {
        var result = MessageBox.Show("Reset today's count to 0?", "Confirm Reset", 
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            _data.Days[today] = 0;
            SaveData();
            UpdateDisplay();
        }
    }
    
    private void UpdateDisplay()
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        var count = _data.Days.GetValueOrDefault(today, 0);
        
        _countLabel.Text = count.ToString();
        _dateLabel.Text = DateTime.Now.ToString("ddd, MMM d");
        
        _historyList.Items.Clear();
        var history = _data.Days
            .Where(kvp => kvp.Key != today)
            .OrderByDescending(kvp => kvp.Key)
            .Take(3);
            
        foreach (var day in history)
        {
            var date = DateTime.Parse(day.Key);
            var label = date.Date == DateTime.Today.AddDays(-1) ? "Yesterday" : date.ToString("MMM d");
            _historyList.Items.Add($"  {label,-15} {day.Value,5}");
        }
        
        if (!_historyList.Items.Cast<string>().Any())
        {
            _historyList.Items.Add("  No history yet");
        }
        
        UpdateTrayTooltip();
    }
    
    private CounterData LoadData()
    {
        try
        {
            if (File.Exists(_dataFilePath))
            {
                var json = File.ReadAllText(_dataFilePath);
                return JsonSerializer.Deserialize<CounterData>(json) ?? new CounterData();
            }
        }
        catch { }
        return new CounterData();
    }
    
    private void SaveData()
    {
        try
        {
            var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_dataFilePath, json);
        }
        catch { }
    }
}

public class CounterData
{
    public Dictionary<string, int> Days { get; set; } = new();
}

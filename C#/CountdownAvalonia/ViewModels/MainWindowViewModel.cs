using CountdownAvalonia.Models;
using System;
using System.Threading;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace CountdownAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // 输入界面绑定属性
    public string InputName { get; set; } = string.Empty;
    public string InputSpan { get; set; } = string.Empty;
    public bool IsInputVisible { get; set; } = true;
    public bool IsCountdownVisible { get; set; } = false;

    // 倒计时控制界面绑定属性
    private CountdownAvaloniaClass? _cd;
    public string CountdownName { get; set; } = string.Empty;
    public string RemainTimeStr { get; set; } = "00:00:00";
    public double Progress { get; set; } = 0.0;
    private int _totalSeconds = 1;
    private Thread? _timerThread;
    private bool _running = false;

    // 命令
    public ICommand StartCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand ResumeCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand ExitCommand { get; }

    public MainWindowViewModel()
    {
        StartCommand = new RelayCommand(StartCountdown);
        PauseCommand = new RelayCommand(Pause);
        ResumeCommand = new RelayCommand(Resume);
        ResetCommand = new RelayCommand(Reset);
        ExitCommand = new RelayCommand(Exit);
    }

    private void StartCountdown()
    {
        if (string.IsNullOrWhiteSpace(InputName) || string.IsNullOrWhiteSpace(InputSpan))
            return;
        if (!TryParseSpan(InputSpan, out var span))
        {
            RemainTimeStr = "时长格式错误";
            OnPropertyChanged(nameof(RemainTimeStr));
            return;
        }
        _cd = CountdownAvaloniaClass.Create(InputName, DateTime.Now.Add(span));
        CountdownName = InputName;
        _totalSeconds = (int)span.TotalSeconds;
        if (_totalSeconds <= 0) _totalSeconds = 1;
        IsInputVisible = false;
        IsCountdownVisible = true;
        OnPropertyChanged(nameof(IsInputVisible));
        OnPropertyChanged(nameof(IsCountdownVisible));
        StartTimerThread();
    }

    private void StartTimerThread()
    {
        _running = true;
        _timerThread = new Thread(() =>
        {
            while (_running && _cd != null)
            {
                int remain = CountdownAvaloniaClass.GetRemainSeconds(_cd);
                if (remain < 0) remain = 0;
                RemainTimeStr = FormatTime(remain);
                Progress = 1.0 - (double)remain / _totalSeconds;
                if (Progress < 0) Progress = 0;
                if (Progress > 1) Progress = 1;
                OnPropertyChanged(nameof(RemainTimeStr));
                OnPropertyChanged(nameof(Progress));
                if (remain == 0 && _cd.Status == true)
                {
                    _cd.Status = false;
                    // Avalonia 无法直接弹窗，这里可扩展
                }
                Thread.Sleep(1000);
            }
        });
        _cd.Status = true;
        _timerThread.IsBackground = true;
        _timerThread.Start();
    }

    private void Pause()
    {
        if (_cd != null)
        {
            CountdownAvaloniaClass.Pause(_cd);
            _cd.Status = false;
        }
    }

    private void Resume()
    {
        if (_cd != null && CountdownAvaloniaClass.GetRemainSeconds(_cd) > 0)
        {
            CountdownAvaloniaClass.Start(_cd);
            _cd.Status = true;
        }
    }

    private void Reset()
    {
        if (_cd != null)
        {
            CountdownAvaloniaClass.Reset(_cd);
            _cd.Status = true;
            _cd.TargetDate = DateTime.Now.AddSeconds(_totalSeconds);
        }
    }

    private void Exit()
    {
        _running = false;
        Environment.Exit(0);
    }

    private static string FormatTime(int sec)
    {
        int h = sec / 3600;
        int m = (sec % 3600) / 60;
        int s = sec % 60;
        if (h > 0)
            return $"{h:D2}:{m:D2}:{s:D2}";
        else
            return $"{m:D2}:{s:D2}";
    }

    // 支持 1h30m/90s/1d2h 格式解析
    private static bool TryParseSpan(string raw, out TimeSpan span)
    {
        try
        {
            raw = raw.Trim().ToLower();
            int total = 0;
            int weeks = 0, days = 0, hours = 0, minutes = 0, seconds = 0;
            var pattern = new System.Text.RegularExpressions.Regex(@"(\d+)([dhmsw])");
            var matches = pattern.Matches(raw);
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                int val = int.Parse(m.Groups[1].Value);
                switch (m.Groups[2].Value)
                {
                    case "w": weeks += val; break;
                    case "d": days += val; break;
                    case "h": hours += val; break;
                    case "m": minutes += val; break;
                    case "s": seconds += val; break;
                }
            }
            span = new TimeSpan(weeks * 7 + days, hours, minutes, seconds);
            return span.TotalSeconds > 0;
        }
        catch { span = TimeSpan.Zero; return false; }
    }
}

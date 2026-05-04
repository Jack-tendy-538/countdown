using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace CountdownShell;

class CountDownClass
{
    public string? Name { get; set; }
    public DateTime TargetDate { get; set; }
    public bool? Status { get; set; } // true=运行 false=暂停 null=未开始/结束
    public DateTime RemainDate { get; set; }

    public static CountDownClass Create(string name, DateTime targetDate)
    {
        return new CountDownClass { Name = name, TargetDate = targetDate, Status = null, RemainDate = targetDate };
    }
    public static int GetRemainSeconds(CountDownClass cd)
    {
        if (cd.Status == null) return 0;
        if (cd.Status == true) return (int)(cd.TargetDate - DateTime.Now).TotalSeconds;
        return (int)(cd.RemainDate - DateTime.Now).TotalSeconds;
    }
    public static void Start(CountDownClass cd)
    {
        if (cd.Status == null) { cd.Status = true; cd.RemainDate = cd.TargetDate; }
        else if (cd.Status == false) { cd.Status = true; cd.TargetDate = DateTime.Now.AddSeconds(GetRemainSeconds(cd)); }
    }
    public static void Pause(CountDownClass cd)
    {
        if (cd.Status == true) { cd.Status = false; cd.RemainDate = DateTime.Now.AddSeconds(GetRemainSeconds(cd)); }
    }
    public static void Reset(CountDownClass cd)
    {
        cd.Status = null; cd.RemainDate = cd.TargetDate;
    }
    public static bool TryParseSpan(string raw, out TimeSpan span)
    {
        try
        {
            raw = raw.Trim().ToLower();
            int weeks = 0, days = 0, hours = 0, minutes = 0, seconds = 0;
            var pattern = new Regex(@"(\d+)([dhmsw])");
            var matches = pattern.Matches(raw);
            foreach (Match m in matches)
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
    public double Percentage()
    {
        var total = (TargetDate - DateTime.Now).TotalSeconds;
        var remain = CountDownClass.GetRemainSeconds(this);
        if (total <= 0) return 1.0;
        return Math.Round(1.0 - remain / total, 3);
    }
}

class Program
{
    static void Main()
    {
        var (name, span) = GetArgs();
        Console.WriteLine($"倒计时 [{name}] 开始，总时长: {span}");
        Countdown(name, span);
    }

    static (string, TimeSpan) GetArgs()
    {
        Console.Write("请输入倒计时名称: ");
        string name = Console.ReadLine()?.Trim() ?? "";
        Console.Write("请输入倒计时时长 (例如 1h30m / 2m30s / 1d): ");
        string raw = Console.ReadLine()?.Trim() ?? "";
        while (true)
        {
            if (CountDownClass.TryParseSpan(raw, out var span))
                return (name, span);
            Console.WriteLine("时长格式错误，请重新输入");
            Console.Write("请输入倒计时时长: ");
            raw = Console.ReadLine()?.Trim() ?? "";
        }
    }

    static void ShowProgress(CountDownClass cd, int totalSec, int barLength = 30)
    {
        double percent = 1.0 - (double)CountDownClass.GetRemainSeconds(cd) / totalSec;
        if (percent < 0) percent = 0;
        if (percent > 1) percent = 1;
        int filled = (int)(barLength * percent);
        string bar = new string('■', filled) + new string('□', barLength - filled);
        int remainSec = CountDownClass.GetRemainSeconds(cd);
        string remainStr = remainSec <= 0 ? "0秒" : FormatTime(remainSec);
        Console.Write($"\r进度: [{bar}] {percent * 100,5:0.0}%  剩余: {remainStr}");
    }

    static string FormatTime(int sec)
    {
        int h = sec / 3600;
        int m = (sec % 3600) / 60;
        int s = sec % 60;
        return h > 0 ? $"{h:D2}:{m:D2}:{s:D2}" : $"{m:D2}:{s:D2}";
    }

    static void Countdown(string name, TimeSpan span)
    {
        var cd = CountDownClass.Create(name, DateTime.Now.Add(span));
        int totalSec = (int)span.TotalSeconds;
        CountDownClass.Start(cd);
        while (true)
        {
            try
            {
                ShowProgress(cd, totalSec);
                if (CountDownClass.GetRemainSeconds(cd) <= 0)
                {
                    Console.WriteLine("\n倒计时结束！");
                    break;
                }
                Thread.Sleep(1000);
            }
            catch (ThreadInterruptedException) { break; }
            catch (Exception) { break; }
        }
        // 支持暂停/重置/继续/退出
        while (true)
        {
            Console.Write("命令 (c=继续, r=重置并继续, q=退出): ");
            string cmd = Console.ReadLine()?.Trim().ToLower() ?? "";
            if (cmd == "c")
            {
                CountDownClass.Start(cd);
                Console.WriteLine("继续...");
                Countdown(name, span);
                return;
            }
            else if (cmd == "r")
            {
                CountDownClass.Reset(cd);
                CountDownClass.Start(cd);
                Console.WriteLine("已重置并开始");
                Countdown(name, span);
                return;
            }
            else if (cmd == "q")
            {
                Console.WriteLine("退出倒计时");
                return;
            }
            else
            {
                Console.WriteLine("无效命令，请输入 c / r / q");
            }
        }
    }
}
using System;

namespace CountdownAvalonia.Models;

class CountdownAvaloniaClass
{
    public string? Name { get; set; }
    public DateTime TargetDate { get; set; }
    public bool? Status { get; set; } // true表示正在倒计时，false表示暂停，null表示未开始或已结束
    public DateTime RemainDate { get; set; } // 指的是上一次status从true变为false的时间

    static public CountdownAvaloniaClass Create(string name, DateTime targetDate)
    {
        return new CountdownAvaloniaClass
        {
            Name = name,
            TargetDate = targetDate,
            Status = null,
            RemainDate = targetDate
        };
    }

    static public int GetRemainSeconds(CountdownAvaloniaClass countDown)
    {
        if (countDown.Status == null) // 未开始或已结束
        {
            return 0;
        }
        else if (countDown.Status == true) // 正在倒计时
        {
            return (int)(countDown.TargetDate - DateTime.Now).TotalSeconds;
        }
        else // 暂停
        {
            return (int)(countDown.RemainDate - DateTime.Now).TotalSeconds;
        }
    }

    static public void Start(CountdownAvaloniaClass countDown)
    {
        if (countDown.Status == null) // 未开始
        {
            countDown.Status = true;
            countDown.RemainDate = countDown.TargetDate;
        }
        else if (countDown.Status == false) // 暂停
        {
            countDown.Status = true;
            countDown.TargetDate = DateTime.Now.AddSeconds(GetRemainSeconds(countDown));
        }
    }

    static public void Pause(CountdownAvaloniaClass countDown)
    {
        if (countDown.Status == true) // 正在倒计时
        {
            countDown.Status = false;
            countDown.RemainDate = DateTime.Now.AddSeconds(GetRemainSeconds(countDown));
        }
    }

    static public void Reset(CountdownAvaloniaClass countDown)
    {
        countDown.Status = null;
        countDown.RemainDate = countDown.TargetDate;
    }

    static public bool IsFinished(CountdownAvaloniaClass countDown)
    {
        return GetRemainSeconds(countDown) <= 0;
    }
}
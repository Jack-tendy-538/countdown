using CountDown.Models;
using System;
using System.Threading;

namespace CountDown.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // public string Greeting { get; } = "Welcome to Avalonia!";

    static CountDownClass countDown = CountDownClass.Create("Test", DateTime.Now.AddSeconds(10));
    public string name { get; set; } = CountDownClass.GetRemainSeconds(countDown).ToString();

    public MainWindowViewModel()
    {
        // 模拟倒计时
        new Thread(() =>
        {
            while (true)
            {
                Thread.Sleep(1000);
                name = CountDownClass.GetRemainSeconds(countDown).ToString();
                OnPropertyChanged(nameof(name));
            }
        }).Start();
    }
}

#include <iostream>
#include <iomanip>
#include <thread>
#include <chrono>
#include <atomic>
#include <csignal>
#include "countdown_base.hpp"

std::atomic<bool> pause_requested(false);

void signal_handler(int) {
    pause_requested = true;
}

// 格式化剩余秒数为 hh:mm:ss 或 mm:ss
std::string format_duration(std::chrono::seconds sec) {
    long long total = sec.count();
    if (total < 0) total = 0;
    long long hours = total / 3600;
    long long minutes = (total % 3600) / 60;
    long long seconds = total % 60;
    std::ostringstream oss;
    if (hours > 0)
        oss << std::setfill('0') << std::setw(2) << hours << ":"
            << std::setw(2) << minutes << ":" << std::setw(2) << seconds;
    else
        oss << std::setfill('0') << std::setw(2) << minutes << ":"
            << std::setw(2) << seconds;
    return oss.str();
}

// 打印进度条和剩余时间
void print_progress(const CountDown& cd, int bar_width = 30) {
    double percent = cd.percentage();
    int filled = static_cast<int>(bar_width * percent);
    std::cout << "\r[";
    for (int i = 0; i < bar_width; ++i) {
        std::cout << (i < filled ? '#' : '-');
    }
    std::cout << "] " << std::fixed << std::setprecision(1) << percent * 100
              << "% 剩余: " << format_duration(cd.get_remain()) << "   ";
    std::cout << std::flush;
}

// 获取用户输入 (名称和时长)
void get_args(std::string& name, std::chrono::seconds& span) {
    std::cout << "请输入倒计时名称: ";
    std::getline(std::cin, name);
    std::string raw_span;
    while (true) {
        std::cout << "请输入倒计时时长 (例如 1h30m / 2m30s / 1d): ";
        std::getline(std::cin, raw_span);
        try {
            span = parse_span(raw_span);
            break;
        } catch (const std::exception& e) {
            std::cerr << "错误: " << e.what() << std::endl;
        }
    }
}

// 倒计时主循环，返回 true 表示正常结束，false 表示被用户中断（需进入菜单）
bool run_countdown(CountDown& cd) {
    cd.start();
    pause_requested = false;
    while (cd.get_remain().count() > 0) {
        if (pause_requested) {
            cd.pause();
            return false;   // 用户中断
        }
        print_progress(cd);
        std::this_thread::sleep_for(std::chrono::milliseconds(200)); // 稍微频繁刷新，但不影响体验
        // 实际上每 200ms 刷新一次显示，但倒计时每秒变化一次即可，这里简单保持每秒更新
        // 为了精确每秒更新，可以改用 sleep_until，但为了简化，这里每 200ms 打印一次
        // 但为了不闪烁，我们仅每 200ms 刷新，但实际剩余时间每秒变化一次没问题
        // 更好的：记录上次打印的剩余秒数，这里为了简单直接每 200ms 刷新
    }
    // 倒计时结束
    print_progress(cd);
    std::cout << std::endl << "🎉 倒计时结束！" << std::endl;
    return true;
}

// 暂停菜单
void pause_menu(CountDown& cd) {
    std::cout << "\n[已暂停] 命令: (c)继续, (r)重置并继续, (q)退出: ";
    std::string cmd;
    std::getline(std::cin, cmd);
    if (cmd == "c") {
        cd.start();
    } else if (cmd == "r") {
        cd.reset();
        cd.start();
    } else if (cmd == "q") {
        std::cout << "退出倒计时。" << std::endl;
        exit(0);
    } else {
        std::cout << "无效命令，按 Enter 继续倒计时（相当于继续）" << std::endl;
        cd.start();
    }
}

int main() {
    // 设置 Ctrl+C 信号处理
    std::signal(SIGINT, signal_handler);

    std::string name;
    std::chrono::seconds span;
    get_args(name, span);

    CountDown cd(name, span);
    std::cout << "倒计时 [" << name << "] 开始，总时长: " << format_duration(span) << std::endl;

    while (true) {
        bool finished = run_countdown(cd);
        if (finished) {
            break;  // 倒计时自然结束，退出程序
        } else {
            pause_menu(cd);      // 处理暂停后的选择，然后继续循环
        }
    }
    return 0;
}

#ifndef COUNT_BASE_HPP
#define COUNT_BASE_HPP

#include <chrono>
#include <regex>
#include <string>
#include <stdexcept>

// 解析时间字符串，如 "1h30m20s", "2d5h", "90s" 等，返回秒数
std::chrono::seconds parse_span(const std::string& raw) {
    std::string s = std::regex_replace(raw, std::regex("\\s+"), "");
    std::regex re(R"((\d+)([dhms]))");
    std::smatch match;
    std::string::const_iterator searchStart(s.cbegin());
    long long total_seconds = 0;

    while (std::regex_search(searchStart, s.cend(), match, re)) {
        int quantity = std::stoi(match[1]);
        char unit = match[2].str()[0];
        switch (unit) {
            case 'd': total_seconds += quantity * 86400; break;
            case 'h': total_seconds += quantity * 3600; break;
            case 'm': total_seconds += quantity * 60; break;
            case 's': total_seconds += quantity; break;
            default: throw std::invalid_argument("未知单位: " + std::string(1, unit));
        }
        searchStart = match.suffix().first;
    }
    if (total_seconds == 0 && !raw.empty()) {
        throw std::invalid_argument("无效的时间格式: " + raw);
    }
    return std::chrono::seconds(total_seconds);
}

class CountDown {
public:
    CountDown(const std::string& name, std::chrono::seconds span)
        : name_(name), total_(span), remaining_(span), running_(false) {}

    void start() {
        if (!running_ && remaining_.count() > 0) {
            running_ = true;
            last_start_ = std::chrono::steady_clock::now();
        }
    }

    void pause() {
        if (running_) {
            // 更新剩余时间
            auto now = std::chrono::steady_clock::now();
            auto elapsed = std::chrono::duration_cast<std::chrono::seconds>(now - last_start_);
            remaining_ -= elapsed;
            if (remaining_.count() < 0) remaining_ = std::chrono::seconds(0);
            running_ = false;
        }
    }

    void reset() {
        running_ = false;
        remaining_ = total_;
    }

    double percentage() const {
        if (total_.count() == 0) return 1.0;
        return 1.0 - static_cast<double>(remaining_.count()) / total_.count();
    }

    std::chrono::seconds get_remain() const {
        if (!running_) return remaining_;
        auto now = std::chrono::steady_clock::now();
        auto elapsed = std::chrono::duration_cast<std::chrono::seconds>(now - last_start_);
        auto remain = remaining_ - elapsed;
        if (remain.count() < 0) remain = std::chrono::seconds(0);
        return remain;
    }

    bool is_running() const { return running_; }
    const std::string& name() const { return name_; }

private:
    std::string name_;
    std::chrono::seconds total_;
    std::chrono::seconds remaining_;
    bool running_;
    std::chrono::time_point<std::chrono::steady_clock> last_start_;
};

#endif // COUNT_BASE_HPP
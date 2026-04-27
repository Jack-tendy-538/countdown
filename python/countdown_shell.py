import time
import sys
from countdown_base import parse_span, CountDown, cd_holder

def get_args():
    name = input("请输入倒计时名称: ").strip()
    raw = input("请输入倒计时时长 (例如 1h30m / 2m30s / 1d): ").strip()
    while True:
        try:
            span = parse_span(raw)
            break
        except ValueError as e:
            print(f"错误: {e}")
            raw = input("请重新输入时长: ").strip()
    return name, span

def show_progress(cd: CountDown, bar_length=30):
    """打印进度条 + 剩余时间"""
    percent = cd.percentage()
    filled = int(bar_length * percent)
    bar = "■" * filled + "□" * (bar_length - filled)
    remain_sec = cd.remaining
    if remain_sec <= 0:
        remain_str = "0秒"
    else:
        h = int(remain_sec // 3600)
        m = int((remain_sec % 3600) // 60)
        s = int(remain_sec % 60)
        remain_str = f"{h:02d}:{m:02d}:{s:02d}" if h else f"{m:02d}:{s:02d}"
    sys.stdout.write(f"\r进度: [{bar}] {percent*100:5.1f}%  剩余: {remain_str}")
    sys.stdout.flush()

def countdown(name, span):
    cd = CountDown(name, span)
    holder = cd_holder(cd)
    cd.start()

    try:
        while True:
            status, remain_sec = next(holder)
            show_progress(cd)
            if remain_sec <= 0:
                print("\n倒计时结束！")
                break
            time.sleep(1)
    except KeyboardInterrupt:
        cd.pause()
        print("\n[已暂停]")
        while True:
            cmd = input("命令 (c=继续, r=重置并继续, q=退出): ").strip().lower()
            if cmd == 'c':
                cd.start()
                print("继续...")
                break
            elif cmd == 'r':
                cd.reset()
                cd.start()
                print("已重置并开始")
                break
            elif cmd == 'q':
                print("退出倒计时")
                return
            else:
                print("无效命令，请输入 c / r / q")
        # 继续倒计时循环
        countdown(name, span)  # 递归重启显示循环（简单方式）

def main():
    name, span = get_args()
    print(f"倒计时 [{name}] 开始，总时长: {span}")
    countdown(name, span)

if __name__ == '__main__':
    main()
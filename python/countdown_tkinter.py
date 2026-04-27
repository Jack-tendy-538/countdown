import tkinter as tk
from tkinter import ttk, messagebox
from countdown_base import parse_span, CountDown

class CountdownApp:
    def __init__(self):
        self.root = tk.Tk()
        self.root.title("倒计时器")
        self.root.geometry("400x300")
        self.cd = None
        self.update_id = None
        self._build_input_ui()

    def _build_input_ui(self):
        """初始输入界面"""
        tk.Label(self.root, text="倒计时名称:", font=("Arial", 12)).pack(pady=10)
        self.name_var = tk.StringVar()
        tk.Entry(self.root, textvariable=self.name_var, font=("Arial", 12)).pack(pady=5)

        tk.Label(self.root, text="时长 (例如: 1h30m / 90s / 1d2h):", font=("Arial", 12)).pack(pady=10)
        self.span_var = tk.StringVar()
        tk.Entry(self.root, textvariable=self.span_var, font=("Arial", 12)).pack(pady=5)

        btn = tk.Button(self.root, text="开始倒计时", command=self.start_countdown, font=("Arial", 12))
        btn.pack(pady=20)

    def start_countdown(self):
        name = self.name_var.get().strip()
        raw_span = self.span_var.get().strip()
        if not name:
            messagebox.showerror("错误", "请输入名称")
            return
        try:
            span = parse_span(raw_span)
        except ValueError as e:
            messagebox.showerror("错误", f"时长格式错误: {e}")
            return

        # 清除输入界面，构建控制界面
        for widget in self.root.winfo_children():
            widget.destroy()

        self.cd = CountDown(name, span)
        self.cd.start()

        # 显示信息
        tk.Label(self.root, text=f"倒计时：{name}", font=("Arial", 14, "bold")).pack(pady=10)
        self.remain_label = tk.Label(self.root, text="", font=("Arial", 20))
        self.remain_label.pack(pady=10)

        self.progress = ttk.Progressbar(self.root, length=300, mode='determinate')
        self.progress.pack(pady=10)

        btn_frame = tk.Frame(self.root)
        btn_frame.pack(pady=20)
        tk.Button(btn_frame, text="暂停", command=self.pause, width=8).pack(side=tk.LEFT, padx=5)
        tk.Button(btn_frame, text="继续", command=self.resume, width=8).pack(side=tk.LEFT, padx=5)
        tk.Button(btn_frame, text="重置", command=self.reset, width=8).pack(side=tk.LEFT, padx=5)
        tk.Button(btn_frame, text="退出", command=self.quit_app, width=8).pack(side=tk.LEFT, padx=5)

        self.update_display()

    def update_display(self):
        """每秒刷新显示"""
        if self.cd is None:
            return
        remain_sec = self.cd.remaining
        total_sec = self.cd.span.total_seconds()
        # 更新剩余时间文字
        if remain_sec <= 0:
            self.remain_label.config(text="00:00:00")
            self.progress['value'] = 100
            if self.cd.status:  # 刚结束
                self.cd.status = False
                messagebox.showinfo("完成", f"倒计时 [{self.cd.name}] 结束！")
                self.quit_app()
            return
        h = int(remain_sec // 3600)
        m = int((remain_sec % 3600) // 60)
        s = int(remain_sec % 60)
        time_str = f"{h:02d}:{m:02d}:{s:02d}" if h else f"{m:02d}:{s:02d}"
        self.remain_label.config(text=time_str)
        # 更新进度条
        percent = (total_sec - remain_sec) / total_sec * 100
        self.progress['value'] = percent
        # 继续循环
        self.update_id = self.root.after(1000, self.update_display)

    def pause(self):
        if self.cd:
            self.cd.pause()

    def resume(self):
        if self.cd and self.cd.remaining > 0:
            self.cd.start()

    def reset(self):
        if self.cd:
            self.cd.reset()
            self.cd.start()

    def quit_app(self):
        if self.update_id:
            self.root.after_cancel(self.update_id)
        self.root.destroy()

    def run(self):
        self.root.mainloop()

def main():
    app = CountdownApp()
    app.run()

if __name__ == '__main__':
    main()
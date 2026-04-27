import re
import datetime

def parse_span(raw: str) -> datetime.timedelta:
    """
    解析形如 "2h30m", "1d2h15m", "90s" 的时间字符串。
    支持单位：d(天), h(小时), m(分钟), s(秒), w(周)
    示例：parse_span("1d2h30m") → timedelta(days=1, hours=2, minutes=30)
    """
    formatted = re.sub(r"\s+", "", raw).strip().lower()
    pattern = re.compile(r"(\d+)([dhmsw])")
    parts = pattern.findall(formatted)
    if not parts:
        raise ValueError(f"无效的时间格式: {raw}，示例：2h30m / 1d2h / 90s")

    unit_map = {
        'd': 'days',
        'h': 'hours',
        'm': 'minutes',
        's': 'seconds',
        'w': 'weeks'
    }
    kwargs = {}
    for quantity, unit in parts:
        key = unit_map.get(unit)
        if key:
            kwargs[key] = kwargs.get(key, 0) + int(quantity)
        else:
            raise ValueError(f"未知单位: {unit}")
    return datetime.timedelta(**kwargs)


class CountDown:
    def __init__(self, name: str, span: datetime.timedelta):
        self.name = name
        self.span = span          # 总时长
        self.status = False       # False=暂停, True=运行中
        self.last_start_time = None
        self._remaining = span    # 当前剩余时间（暂停时保存）

    @property
    def remaining(self) -> float:
        """返回剩余秒数（float）"""
        if not self.status:
            return self._remaining.total_seconds()
        if self.last_start_time is None:
            return self._remaining.total_seconds()
        elapsed = datetime.datetime.now() - self.last_start_time
        remaining = self._remaining - elapsed
        return max(remaining.total_seconds(), 0.0)

    def get_remain(self) -> datetime.timedelta:
        """返回剩余时间（timedelta）"""
        return datetime.timedelta(seconds=self.remaining)

    def start(self):
        if not self.status:
            self.status = True
            self.last_start_time = datetime.datetime.now()

    def pause(self):
        if self.status:
            self._remaining = self.get_remain()
            self.status = False
            self.last_start_time = None

    def reset(self):
        self.status = False
        self._remaining = self.span
        self.last_start_time = None

    def percentage(self) -> float:
        """返回完成进度 0~1，总时长为0时返回1"""
        total = self.span.total_seconds()
        if total <= 0:
            return 1.0
        remain = self.remaining
        return round(1.0 - remain / total, 3)


def cd_holder(cd: CountDown):
    """倒计时状态生成器，每秒产生一次 (是否运行中, 剩余秒数)"""
    while True:
        remain_sec = cd.remaining
        if remain_sec <= 0 and cd.status:
            cd.status = False   # 自动停止
        yield cd.status, remain_sec
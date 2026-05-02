// countdown.base.js
// 倒计时核心类，仿照 C# 的 CountDownClass

class CountDown {
    constructor(name, targetDate) {
        this.name = name;               // 倒计时名称
        this.targetDate = targetDate;   // 目标日期 (Date 对象)
        this.status = null;             // null: 未开始, true: 运行中, false: 暂停
        this.remainDate = targetDate;   // 暂停时存储的剩余截止时间点
    }

    // 静态工厂方法
    static create(name, targetDate) {
        return new CountDown(name, targetDate);
    }

    // 获取剩余秒数（逻辑与 C# 完全一致）
    getRemainSeconds() {
        const now = new Date();
        if (this.status === null) {
            return 0;
        } else if (this.status === true) {
            return Math.max(0, Math.floor((this.targetDate - now) / 1000));
        } else { // status === false （暂停）
            return Math.max(0, Math.floor((this.remainDate - now) / 1000));
        }
    }

    // 开始 / 恢复倒计时
    start() {
        if (this.status === null) {
            // 从未开始启动
            this.status = true;
            this.remainDate = this.targetDate;
        } else if (this.status === false) {
            // 从暂停恢复：重新计算目标日期
            const remainingSecs = this.getRemainSeconds();
            this.status = true;
            this.targetDate = new Date(Date.now() + remainingSecs * 1000);
        }
    }

    // 暂停倒计时
    pause() {
        if (this.status === true) {
            const remainingSecs = this.getRemainSeconds();
            this.status = false;
            this.remainDate = new Date(Date.now() + remainingSecs * 1000);
        }
    }

    // 重置倒计时（未开始状态，保留原始目标日期）
    reset() {
        this.status = null;
        this.remainDate = this.targetDate;
    }

    // 判断倒计时是否已结束
    isFinished() {
        return this.getRemainSeconds() <= 0;
    }
}

// 导出到全局
window.CountDown = CountDown;
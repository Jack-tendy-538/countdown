// countdown.js
// UI 控制逻辑，使用 prompt 对话框输入名称和时长

let currentCd = null;         // 当前倒计时对象
let timerInterval = null;     // 定时器ID
let totalSeconds = 0;         // 初始总秒数（用于进度计算）
let currentName = "";         // 当前倒计时名称

// DOM 元素
const $countdownArea = $("#countdownArea");
const $countdownName = $("#countdownName");
const $remainTime = $("#remainTime");
const $progressFill = $("#progressFill");
const $progressPercent = $("#progressPercent");
const $noTimerMsg = $("#noTimerMsg");
const $errorMsg = $("#errorMsg");

// 按钮
const $newBtn = $("#newBtn");
const $pauseBtn = $("#pauseBtn");
const $resumeBtn = $("#resumeBtn");
const $resetBtn = $("#resetBtn");
const $exitBtn = $("#exitBtn");

// 辅助函数：格式化剩余秒数为 mm:ss 或 hh:mm:ss
function formatTime(seconds) {
    if (seconds < 0) seconds = 0;
    const h = Math.floor(seconds / 3600);
    const m = Math.floor((seconds % 3600) / 60);
    const s = seconds % 60;
    if (h > 0) {
        return `${h.toString().padStart(2, "0")}:${m.toString().padStart(2, "0")}:${s.toString().padStart(2, "0")}`;
    } else {
        return `${m.toString().padStart(2, "0")}:${s.toString().padStart(2, "0")}`;
    }
}

// 更新界面显示（剩余时间、进度条）
function updateDisplay() {
    if (!currentCd) {
        // 无倒计时时显示占位内容
        $remainTime.text("00:00:00");
        $progressFill.css("width", "0%");
        $progressPercent.text("0%");
        return;
    }

    const remainSec = currentCd.getRemainSeconds();
    const remainStr = formatTime(remainSec);
    $remainTime.text(remainStr);

    // 进度条 (1 - 剩余秒数/总秒数)
    let progress = 0;
    if (totalSeconds > 0) {
        progress = 1 - remainSec / totalSeconds;
        if (progress < 0) progress = 0;
        if (progress > 1) progress = 1;
    }
    const percent = Math.floor(progress * 100);
    $progressFill.css("width", percent + "%");
    $progressPercent.text(percent + "%");

    // 倒计时结束处理
    if (remainSec <= 0 && currentCd.status === true) {
        currentCd.status = false;
        if (timerInterval) {
            clearInterval(timerInterval);
            timerInterval = null;
        }
        $errorMsg.text("⏰ 倒计时结束！").fadeIn(200);
        setTimeout(() => $errorMsg.fadeOut(1500), 3000);
    }
}

// 启动计时器（每秒刷新）
function startTimer() {
    if (timerInterval) clearInterval(timerInterval);
    updateDisplay();
    timerInterval = setInterval(() => {
        if (currentCd && currentCd.status === true) {
            updateDisplay();
            if (currentCd.isFinished() && currentCd.status !== true) {
                if (timerInterval) clearInterval(timerInterval);
                timerInterval = null;
            }
        } else if (currentCd && currentCd.status === false) {
            updateDisplay();
        } else if (!currentCd) {
            // 没有倒计时，清除定时器
            if (timerInterval) clearInterval(timerInterval);
            timerInterval = null;
        }
    }, 1000);
}

// 停止计时器
function stopTimer() {
    if (timerInterval) {
        clearInterval(timerInterval);
        timerInterval = null;
    }
}

// 解析时长字符串（支持 w,d,h,m,s，示例：1d2h，90s，1h30m，2w）
function parseSpan(raw) {
    try {
        let str = raw.trim().toLowerCase();
        let weeks = 0, days = 0, hours = 0, minutes = 0, seconds = 0;
        const regex = /(\d+)([wdhms])/g;
        let match;
        while ((match = regex.exec(str)) !== null) {
            const val = parseInt(match[1], 10);
            const unit = match[2];
            switch (unit) {
                case 'w': weeks += val; break;
                case 'd': days += val; break;
                case 'h': hours += val; break;
                case 'm': minutes += val; break;
                case 's': seconds += val; break;
            }
        }
        let totalSec = (weeks * 7 + days) * 86400 + hours * 3600 + minutes * 60 + seconds;
        return totalSec > 0 ? totalSec : null;
    } catch (e) {
        return null;
    }
}

// 显示错误提示（短暂消息）
function showError(msg) {
    $errorMsg.text(msg).fadeIn(200);
    setTimeout(() => $errorMsg.fadeOut(1500), 3000);
}

// 创建并启动新倒计时（通过 prompt 输入）
function createNewCountdown() {
    // 输入名称
    let name = prompt("请输入倒计时名称：", "我的倒计时");
    if (name === null) return; // 用户取消
    name = name.trim();
    if (!name) {
        showError("名称不能为空！");
        return;
    }

    // 输入时长
    let spanRaw = prompt("请输入时长（支持格式：1d2h / 90s / 1h30m / 2w）：", "1h");
    if (spanRaw === null) return;
    spanRaw = spanRaw.trim();
    if (!spanRaw) {
        showError("时长不能为空！");
        return;
    }

    const secs = parseSpan(spanRaw);
    if (!secs || secs <= 0) {
        showError("时长格式错误，请使用例如: 1d2h, 90s, 1h30m, 2w");
        return;
    }

    // 停止旧定时器
    stopTimer();

    totalSeconds = secs;
    currentName = name;
    const targetDate = new Date(Date.now() + secs * 1000);
    currentCd = CountDown.create(name, targetDate);
    currentCd.start();  // 开始运行

    // 更新界面
    $countdownName.text(name);
    $countdownArea.show();
    $noTimerMsg.hide();

    // 启动计时器
    startTimer();
}

// 暂停
function pauseCountdown() {
    if (currentCd && currentCd.status === true) {
        currentCd.pause();
        updateDisplay();
    } else if (!currentCd) {
        showError("没有正在进行的倒计时，请先新建一个。");
    } else if (currentCd.status === false) {
        showError("倒计时已经暂停，请按“恢复”按钮。");
    } else {
        showError("倒计时尚未开始。");
    }
}

// 恢复
function resumeCountdown() {
    if (currentCd && currentCd.status === false) {
        const remainBefore = currentCd.getRemainSeconds();
        if (remainBefore > 0) {
            currentCd.start();
            updateDisplay();
            if (!timerInterval) startTimer();
        } else {
            showError("倒计时已结束，请使用“重置”或新建倒计时。");
        }
    } else if (!currentCd) {
        showError("没有倒计时，请先新建一个。");
    } else if (currentCd.status === true) {
        showError("倒计时正在运行中，无需恢复。");
    } else {
        showError("倒计时尚未开始，请先启动。");
    }
}

// 重置：重新开始相同名称和总时长
function resetCountdown() {
    if (!currentCd) {
        showError("没有倒计时可重置，请先新建一个。");
        return;
    }
    stopTimer();
    const newTarget = new Date(Date.now() + totalSeconds * 1000);
    currentCd = CountDown.create(currentName, newTarget);
    currentCd.start();
    updateDisplay();
    startTimer();
}

// 退出（清除当前倒计时，回到无倒计时状态）
function exitCountdown() {
    stopTimer();
    currentCd = null;
    totalSeconds = 0;
    currentName = "";
    $countdownArea.hide();
    $noTimerMsg.show();
    $errorMsg.hide();
}

// 页面初始化绑定事件
$(document).ready(function () {
    $newBtn.on("click", createNewCountdown);
    $pauseBtn.on("click", pauseCountdown);
    $resumeBtn.on("click", resumeCountdown);
    $resetBtn.on("click", resetCountdown);
    $exitBtn.on("click", exitCountdown);

    // 初始状态：无倒计时，显示提示信息
    $countdownArea.hide();
    $noTimerMsg.show();
    $errorMsg.hide();
});
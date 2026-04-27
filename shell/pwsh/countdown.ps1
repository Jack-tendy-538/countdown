function now {
    param (
        [string]$OptionalParameters
    )

    Get-Date
}

function parseSpan {
    param (
        $raw
    )
    # 解析输入的时间字符串，支持格式如 "1h30m"、"45s"、"90" 等
    $span = 0
    if ($raw -match '^(\d+)$') {
        return [int]$matches[1]
    }
    if ($raw -match '(\d+)h') {
        $span += [int]$matches[1] * 3600
    }
    if ($raw -match '(\d+)m') {
        $span += [int]$matches[1] * 60
    }
    if ($raw -match '(\d+)s') {
        $span += [int]$matches[1]
    }
    return $span
}

function printBrick {
    param (
        $past, $remain, $total
    )
    Clear-Host
    $blocks = 25
    $filled = [math]::Round(($past / $total) * $blocks)
    if ($filled -lt 0) { $filled = 0 }
    if ($filled -gt $blocks) { $filled = $blocks }
    $bar = ('#' * $filled) + ('-' * ($blocks - $filled))
    Write-Output "|$bar|"
    Write-Output "名称: $name"
    Write-Output "总时长: $total 秒"
    Write-Output "已过: $past 秒  剩余: $remain 秒"
    Write-Output "截止时间: $((Get-Date).AddSeconds($remain).ToString('yyyy-MM-dd HH:mm:ss'))"
}

$name = Read-Host "请输入倒计时名称"

$span = 0
while ($span -le 0) {
    $raw = Read-Host "请输入倒计时时长 (例如 1h30m, 45s, 90)"
    $span = parseSpan $raw
    if ($span -le 0) {
        Write-Host "无效的时间格式，请重新输入。"
    }
}

$total = $span
$start = Get-Date
$pause = $false

while ($span -gt 0) {
    try {
        if ($pause) {
            Start-Sleep -Milliseconds 200
            continue
        }

        $past = $total - $span
        printBrick $past $span $total
        Start-Sleep -Seconds 1
        $span -= 1
    } catch [System.Management.Automation.PipelineStoppedException] {
        $pause = -not $pause
        if ($pause) {
            Write-Host "`n已暂停。再次按 Ctrl+C 继续。" -ForegroundColor Yellow
        } else {
            Write-Host "`n已继续。" -ForegroundColor Green
        }
        continue
    }
}
printBrick $total 0 $total
Write-Host "倒计时已结束！"
[console]::beep(750, 300)

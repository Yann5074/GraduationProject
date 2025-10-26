export function formatDateTime(datetimeString) {
    if (!datetimeString)
        return '';
    const cleanString = datetimeString.trim().replace(/\s+/g, ' ')
    const match = cleanString.match(
        /^(\d{1,2})\s+(\d{1,2})\s+(\d{4})\s+(\d{1,2}):(\d{2})(AM|PM)$/i
    )

    if (!match) {
        // 若不是這種格式，就原樣返回
        return datetimeString
    }

    let [, month, day, year, hour, minute, meridian] = match
    month = month.padStart(2, '0')
    day = day.padStart(2, '0')
    hour = parseInt(hour, 10)
    if (meridian.toUpperCase() === 'PM' && hour < 12) hour += 12
    if (meridian.toUpperCase() === 'AM' && hour === 12) hour = 0

    const yyyy = year
    const mm = month
    const dd = day
    const hh = String(hour).padStart(2, '0')
    const mi = minute

    return `${yyyy}/${mm}/${dd} ${hh}:${mi}`
}

export function formatCurrency(amount) {
    if ((amount == null) || isNaN(amount))
        return '0'
    return new Intl.NumberFormat('zh-TW', {
        minimumFractionDigits: 0
    }).format(amount)
}

export function formatImageUrl(path) {
    if (!path)
        return ''
    // 若網址正確就不處理
    if (/^https?:\/\//i.test(path))
        return path
    const baseUrl = import.meta.env.VITE_BASE_URL
    // 移除可能的重複斜線
    return `${baseUrl.replace(/\/$/, '')}/${path.replace(/^\/+/, '')}`
}
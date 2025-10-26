export function formatDateTime(datetimeString){
    if (!datetimeString)
        return '';
    const date = new Date(datetimeString)
    if (isNaN(date))
        return datetimeString;
    const yyyy = date.getFullYear()
    const mm = String(date.getMonth() + 1).padStart(2, '0')
    const dd = String(date.getDate()).padStart(2, '0')
    const hh = String(date.getHours()).padStart(2, '0')
    const mi = String(date.getMinutes()).padStart(2, '0')
    return `${yyyy}/${mm}/${dd} ${hh}:${mi}`
}

export function formatCurrency(amount){
    if ((amount == null) || isNaN(amount))
        return '0'
    return new Intl.NumberFormat('zh-TW', {
        minimumFractionDigits: 0
    }).format(amount)
}
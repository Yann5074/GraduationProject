// 透過統一工具進行錯誤發生時的資訊捕捉，減少呼叫API層的程式碼數量
export function handleApiResult(result){
    if (!result.ok){
        const msg = result.message;
        throw new Error(msg)
    }
    return result;
}
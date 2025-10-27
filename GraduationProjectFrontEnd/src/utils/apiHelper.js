// 透過統一工具進行錯誤發生時的資訊捕捉，減少呼叫API層的程式碼數量
export function handleApiResult(result){
    if (result === null && typeof result === 'undefined'){
        return{
            ok: true,
            code: 204,
            message: '操作成功 (204)',
            data: null
        }
    }
    
    if (!result.ok){
        const error = new Error(result.message)
        error.apiData = result
        throw error
    }
    return result
}
export function getOrCreateGuestSessionId(){
    const key = 'fm_sessoin_id'
    let sid = localStorage.getItem(key)
    if(!sid){
        sid = crypto.randomUUID()
        localStorage.setItem(key, sid)
    }
    return sid
}
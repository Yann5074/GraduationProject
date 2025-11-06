import http from "./axios";
import { getOrCreateGuestSessionId } from "@/utils/session";
import { useAuthStore } from "@/stores/auth";

export function trackEvent({productId = null, productVariantId = null, eventType = 1, dwellSec = null}){
    const auth = useAuthStore()
    const payload = {
        sessionId: getOrCreateGuestSessionId(),
        productId,
        productVariantId,
        eventType, // 1=view, 2=add_to_cart, 3=purchase
        dwellSec,
        userId: auth.isLoggedIn ? auth.user.memberId: null
    }
    const API = import.meta.env.VITE_API_URL;
    const url = `${API}/events/track`;
    console.log('Track Event Payload:', payload);
    //使用sendBeacon攔截 #TODO
    try{
        const blob = new Blob([JSON.stringify(payload)], {type: 'application/json'})
        const ok = navigator.sendBeacon(url, blob)
        if (!ok){
            return http.post('/events/track', payload)
        }
    }catch{
        return http.post('/events/track', payload)
    }
}
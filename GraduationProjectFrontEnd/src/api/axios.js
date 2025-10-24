import axios from 'axios'
import { useAuthStore } from '../stores/auth'
import router from '@/router'

const apiBaseUrl = import.meta.env.VITE_API_URL;

const http = axios.create({
    baseURL: apiBaseUrl, // 後端網址
    withCredentials: true, // 確保請求帶有 cookie
    timeout: 10000, // 等待上限10秒
})

http.interceptors.response.use(
    response => response,
    error => {
        const auth = useAuthStore();
        const status = error.response?.status;
        const result = error.response?.data;

        switch(status){
            case 401:
                auth.logout();
                router.push('/signin');
                break;
            case 500:
                console.error('API錯誤: ', error);
                // showGlobalNotification('伺服器發生問題，請稍後再試', 'error'); #TODO 未來若要顯示通知要額外npm 裝套件 Element Plus 或 Vant
                break;
        }
        return Promise.reject(result);
    }
)

export default http
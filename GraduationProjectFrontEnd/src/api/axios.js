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

        if (error.response.status === 401) {
            auth.logout();
            router.push('/signin');
        }
        console.error('API出錯: ', error)
        return Promise.reject(error)
    }
)

export default http
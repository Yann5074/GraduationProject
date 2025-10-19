import axios from 'axios'

const apiBaseUrl = import.meta.env.VITE_API_URL;

const api = axios.create({
    baseURL: apiBaseUrl, // 後端網址
    timeout: 10000, // 等待上限10秒
})

api.interceptors.response.use(
    response => response,
    error => {
        console.error('API出錯: ', error)
        return Promise.reject(error)
    }
)

export default api
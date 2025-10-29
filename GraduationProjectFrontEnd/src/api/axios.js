import axios from 'axios'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'

const apiBaseUrl = import.meta.env.VITE_API_URL || 'http://localhost:7131'

const http = axios.create({
  baseURL: apiBaseUrl, // 後端網址
  timeout: 10000, // 等待上限10秒
  withCredentials: true, // ⬅️ 關鍵！讓 Cookie / Session 一起送出
})

http.interceptors.response.use(
    response =>{
      console.log(response)
        return response
    },
    error => {
        const auth = useAuthStore();
        const status = error.response?.status;
        const result = error.response?.data;
        let message = result?.message;
        let code = result?.code

        switch(status){
            case 401:
                auth.logout();
                import('@/router').then(({default: router}) =>{
                  router.push('/signin');
                })
                code = '401'
                message = '登入逾時，請重新登入'
                break;
            case 500:
                console.error('API錯誤: ', error);
                code = '500'
                message = '伺服器發生問題，請稍後再試'
                break;
        }
        return Promise.reject({
            ok: false,
            code,
            message,
            data: result
        });
      })

export default http

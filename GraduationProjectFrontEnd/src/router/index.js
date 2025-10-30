import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import ProductDetailPage from '@/views/ProductDetailPage.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/home'
    },
    {
      path: '/home',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/shop',
      name: 'shop',
      component: () => import('../views/Shop.vue'),
    },
    {
      path: '/about',
      name: 'about',
      component: () => import('../views/AboutView.vue'),
    },
    {
      path: '/contact',
      name: 'contact',
      component: () => import('../views/Contact.vue'),
    },
    {
      path: '/services',
      name: 'services',
      component: () => import('../views/Services.vue'),
    },
    {
      path: '/design',
      name: 'design',
      component: () => import('../views/Design.vue'),
    },
    {
      path: '/cart',
      name: 'cart',
      component: () => import('../views/Cart.vue'),
    },
    {
      path: '/checkout',
      name: 'checkout',
      component: () => import('../views/CheckOut.vue'),
    },
    {
      path: '/thanks',
      name: 'thanks',
      component: () => import('../views/Thanks.vue'),
    },
    {
      path: '/signin',
      name: 'signin',
      component: () => import('../views/SignIn.vue'),
    },
    {
      path: '/Order',
      name: 'Order',
      component: () => import('../views/Order.vue'),
    },
    {
      path: '/signup',
      name: 'signup',
      component: () => import('@/views/SignUp.vue'),
    },
    {
      path: '/products',
      name: 'ProductList',
      component: () => import('../views/ProductListPage.vue'),
      meta: {
        title: '產品列表',
      },
    },
    {
      path: '/products/:id',
      name: 'ProductDetail',
      component: ProductDetailPage,
      meta: {
        title: '產品詳情',
      },
    },
    {
      path: '/search',
      name: 'ProductSearch',
      component: () => import('../views/ProductListPage.vue'),
      meta: {
        title: '搜尋結果',
      },
    },
    {
      path: '/account',
      component: () => import('@/views/account/AccountLayout.vue'),
      children: [
        { path: '', redirect: '/account/profile' },
        { path: 'profile', component: () => import('@/views/account/AccountProfile.vue') },
        { path: 'security', component: () => import('@/views/account/AccountSecurity.vue') },
      ],
      meta: { requiresAuth: true },
    },
    {
      path: '/forgot-password',
      name: 'forgot-password',
      component: () => import('@/views/ForgotPassword.vue'),
    },
  ],

  scrollBehavior(to, from, savedPosition) {

    if (savedPosition) {
      return savedPosition
    }


    if (from.name === 'ProductDetail' && to.name === 'ProductList') {
      return false
    }


    return { left: 0, top: 0 }
  }
})
// // ✅ 全域守衛：未登入導回 /signin（可先保留，之後接 Pinia 再強化）
// router.beforeEach(async (to) => {
//   if (to.meta.requiresAuth) {
//     const auth = useAuthStore?.() // 若你尚未建立 auth store，先註解掉這段判斷
//     if (auth && !auth.isLoggedIn) {
//       return { path: '/signin', query: { redirect: to.fullPath } }
//     }
//   }
// })

export default router

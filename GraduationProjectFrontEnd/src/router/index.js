import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
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
        title: '產品列表'
      }
    },
    // {
    //   path: '/products/:id',
    //   name: 'ProductDetail',
    //   component: () => import('../views/ProductDetailPage.vue'),
    //   meta: {
    //     title: '產品詳情'
    //   }
    // },
    {
      path: '/search',
      name: 'ProductSearch',
      component: () => import('../views/ProductListPage.vue'),
      meta: {
        title: '搜尋結果'
      }
    }

  ],
})

export default router

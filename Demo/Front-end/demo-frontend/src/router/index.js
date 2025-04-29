import { createRouter, createWebHistory } from 'vue-router';
import store from '../stores';
import Home from '../views/Home.vue';
import ProductDetail from '../views/ProductDetail.vue';
import Categories from '../views/Categories.vue';
import Cart from '../views/Cart.vue';
import Login from '../views/Login.vue';
import Register from '../views/Register.vue';
import AdminPanel from '../views/AdminPanel.vue';
import CategoryProduct from '@/views/CategoryProduct.vue';
import Shop from '../views/Shop.vue';
import Checkout from '../views/Checkout.vue';
import OrderConfirmation from '../views/OrderConfirmation.vue';
import Wishlist from '../views/Wishlist.vue';

const routes = [
  { path: '/', component: Home },
  { path: '/products/:id', component: ProductDetail },
  { path: '/categories', component: Categories },
  { path: '/cart', component: Cart },
  { path: '/login', component: Login },
  { path: '/register', component: Register },
  { 
    path: '/admin', 
    component: AdminPanel,
    beforeEnter: (to, from, next) => {
      if (store.getters['user/isAuthenticated'] && store.getters['user/isAdmin']) next();
      else next('/');
    },
  },
  {
    path: '/category/:id',
    name: 'CategoryProduct',
    component: CategoryProduct,
    props: true,
  },
  {
    path: '/shop',
    name: 'Shop',
    component: Shop
  },
  {
    path: '/checkout',
    name: 'checkout',
    component: Checkout,
    meta: { requiresAuth: true }
  },
  {
    path: '/order-confirmation',
    name: 'order-confirmation',
    component: OrderConfirmation
  },
  {
    path: '/wishlist',
    name: 'Wishlist',
    component: Wishlist,
    meta: { requiresAuth: true }
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
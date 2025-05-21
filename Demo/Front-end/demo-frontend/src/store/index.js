import { createStore } from 'vuex';
import user from './modules/user';
import cart from './modules/cart';
import notification from './modules/notification';
import auth from './modules/auth';
import orders from './modules/orders';

export default createStore({
  modules: {
    user,
    cart,
    notification,
    auth,
    orders
  },
  state: {
    // Global state if needed
  },
  mutations: {
    // Global mutations if needed
  },
  actions: {
    // Global actions if needed
  },
  getters: {
    // Global getters if needed
  }
});
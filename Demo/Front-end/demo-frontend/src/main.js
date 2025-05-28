import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import store from './store';
import './assets/tailwind.css';
import axios from 'axios';
import SignalRPlugin from './plugins/signalr';

const token = localStorage.getItem('token');

// Configure SignalR plugin
const signalRConfig = {
  baseUrl: 'http://localhost:5285', // Adjust this to match your API URL
  autoConnect: true,
  reconnectOnAuth: true
};

if (token) {
  axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
  // Chỉ mount app sau khi fetchUser xong (hoặc logout)
  store.dispatch('auth/fetchUser')
    .catch(() => store.dispatch('auth/logout'))
    .finally(() => {
      const app = createApp(App);
      app.use(router);
      app.use(store);
      app.use(SignalRPlugin, signalRConfig);
      app.mount('#app');
    });
} else {
  const app = createApp(App);
  app.use(router);
  app.use(store);
  app.use(SignalRPlugin, signalRConfig);
  app.mount('#app');
}
import axios from 'axios';
import store from '../stores';

const api = axios.create({
  baseURL: 'http://localhost:5285/api', // Thay bằng URL API của bạn
});

api.interceptors.request.use(config => {
  const token = store.state.user.token;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
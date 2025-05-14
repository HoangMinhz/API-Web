import axios from 'axios';
import store from '../stores';
import router from '../router';

const api = axios.create({
  baseURL: 'http://localhost:5285/api', // Thay bằng URL API của bạn
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  },
  withCredentials: true
});

// Request interceptor
api.interceptors.request.use(
  config => {
    // List of public endpoints that don't require authentication
    const publicEndpoints = [
      '/Auth/login',
      '/Auth/register',
      '/Auth/confirm-email',
      '/Product',
      '/Category'
    ];

    // Check if the current request is to a public endpoint
    const isPublicEndpoint = publicEndpoints.some(endpoint => config.url.includes(endpoint));

    if (!isPublicEndpoint) {
      const token = store.state.user.token;
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      } else {
        // If no token and trying to access protected route, redirect to login
        router.push('/login');
        return Promise.reject('No authentication token found');
      }
    }
    return config;
  },
  error => {
    console.error('Request error:', error);
    return Promise.reject(error);
  }
);

// Response interceptor
api.interceptors.response.use(
  response => {
    // For email confirmation, check if the response indicates success
    if (response.config.url.includes('/Auth/confirm-email')) {
      if (response.status === 200 && response.data && response.data.success) {
        return response;
      } else {
        return Promise.reject({
          response: {
            status: 400,
            data: { message: 'Xác thực email không thành công. Vui lòng thử lại.' }
          }
        });
      }
    }
    return response;
  },
  error => {
    // Log detailed error information
    console.error('API Error Details:', {
      message: error.message,
      response: {
        data: error.response?.data,
        status: error.response?.status,
        headers: error.response?.headers
      },
      request: {
        url: error.config?.url,
        method: error.config?.method,
        data: error.config?.data,
        headers: error.config?.headers
      }
    });
    
    if (error.response) {
      const responseData = error.response.data;
      let errorMessage;

      // Try to get error message from response
      if (typeof responseData === 'string') {
        errorMessage = responseData;
      } else if (responseData?.message) {
        errorMessage = responseData.message;
      } else if (responseData?.error) {
        errorMessage = responseData.error;
      } else if (responseData?.errors) {
        // Handle validation errors
        const errors = Object.values(responseData.errors).flat();
        errorMessage = errors.join(', ');
      }

      // Handle specific status codes
      switch (error.response.status) {
        case 400:
          if (error.config.url.includes('/Auth/confirm-email')) {
            return Promise.reject('Link xác thực không hợp lệ hoặc đã hết hạn');
          }
          return Promise.reject(errorMessage || 'Dữ liệu không hợp lệ. Vui lòng kiểm tra lại thông tin.');
        
        case 401:
          if (!error.config.url.includes('/Auth/login') && 
              !error.config.url.includes('/Auth/register') && 
              !error.config.url.includes('/Auth/confirm-email')) {
            store.dispatch('user/logout');
            router.push('/login');
          }
          return Promise.reject(errorMessage || 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
        
        case 403:
          return Promise.reject(errorMessage || 'Bạn không có quyền thực hiện hành động này.');
        
        case 404:
          return Promise.reject(errorMessage || 'Không tìm thấy tài nguyên yêu cầu.');
        
        case 500:
          return Promise.reject(errorMessage || 'Lỗi máy chủ. Vui lòng thử lại sau.');
        
        default:
          return Promise.reject(errorMessage || 'Đã xảy ra lỗi không mong muốn');
      }
    }
    
    // Handle network errors
    if (error.message === 'Network Error') {
      return Promise.reject('Lỗi kết nối mạng. Vui lòng kiểm tra kết nối internet của bạn.');
    }
    
    // Handle timeout
    if (error.code === 'ECONNABORTED') {
      return Promise.reject('Kết nối đến máy chủ bị timeout. Vui lòng thử lại sau.');
    }
    
    return Promise.reject(error.message || 'Đã xảy ra lỗi không mong muốn');
  }
);

export default api;
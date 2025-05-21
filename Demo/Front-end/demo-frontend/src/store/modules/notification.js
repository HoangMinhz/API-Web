export default {
  namespaced: true,
  state: () => ({
    notifications: []
  }),
  mutations: {
    addNotification(state, notification) {
      state.notifications.push({
        ...notification,
        id: Date.now()
      });
    },
    removeNotification(state, id) {
      state.notifications = state.notifications.filter(n => n.id !== id);
    }
  },
  actions: {
    showNotification({ commit }, { type = 'info', message, duration = 3000 }) {
      const id = Date.now();
      commit('addNotification', { type, message, id });
      
      setTimeout(() => {
        commit('removeNotification', id);
      }, duration);
    }
  }
}; 
<template>
  <div class="min-h-screen bg-gray-100">
    <NavBar />
    <main class="container mx-auto px-4 py-8">
      <router-view />
    </main>
    <NotificationToast />
  </div>
</template>

<script>
import NavBar from './components/NavBar.vue';
import NotificationToast from './components/Notification.vue';

export default {
  name: 'App',
  components: {
    NavBar,
    NotificationToast
  }
};
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}
</style>
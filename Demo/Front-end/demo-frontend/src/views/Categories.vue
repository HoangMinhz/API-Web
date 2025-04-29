<template>
  <div>
    <h1 class="text-3xl font-bold text-gray-800 mb-6">Categories</h1>
    <ul class="space-y-2">
      <li v-for="cat in categories" :key="cat.id" class="bg-white rounded-lg shadow-md p-4 hover:bg-gray-50 transition-colors">
        <router-link :to="'/products?categoryId=' + cat.id" class="text-blue-600 hover:underline">{{ cat.name }}</router-link>
      </li>
    </ul>
  </div>
</template>

<script>
import api from '../services/api';

export default {
  data() {
    return { categories: [] };
  },
  async created() {
    const response = await api.get('/categories');
    this.categories = response.data;
  },
};
</script>
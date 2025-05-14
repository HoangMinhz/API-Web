<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <div class="bg-white shadow-sm">
      <div class="container mx-auto px-4 py-6 mt-16">
        <h1 class="text-3xl font-bold text-gray-800">Admin Dashboard</h1>
        <p class="text-gray-600 mt-2">Manage your store's products and categories</p>
      </div>
    </div>

    <!-- Main Content -->
    <div class="container mx-auto px-4 py-8">
      <!-- Stats Overview -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <div class="bg-white rounded-lg shadow-sm p-6">
          <h3 class="text-lg font-semibold text-gray-700">Total Products</h3>
          <p class="text-3xl font-bold text-blue-600 mt-2">{{ products.length }}</p>
        </div>
        <div class="bg-white rounded-lg shadow-sm p-6">
          <h3 class="text-lg font-semibold text-gray-700">Total Categories</h3>
          <p class="text-3xl font-bold text-green-600 mt-2">{{ categories.length }}</p>
        </div>
        <div class="bg-white rounded-lg shadow-sm p-6">
          <h3 class="text-lg font-semibold text-gray-700">Low Stock Items</h3>
          <p class="text-3xl font-bold text-red-600 mt-2">{{ lowStockCount }}</p>
        </div>
      </div>

      <!-- Tabs Navigation -->
      <div class="bg-white rounded-lg shadow-sm mb-8">
        <div class="border-b border-gray-200">
          <nav class="flex -mb-px">
            <button
              v-for="tab in tabs"
              :key="tab.id"
              @click="activeTab = tab.id"
              :class="[
                'px-4 py-3 text-sm font-medium',
                activeTab === tab.id
                  ? 'border-b-2 border-blue-500 text-blue-600'
                  : 'text-gray-500 hover:text-gray-700 hover:border-gray-300'
              ]"
            >
              {{ tab.name }}
            </button>
          </nav>
        </div>
      </div>

      <!-- Products Tab -->
      <div v-if="activeTab === 'products'" class="space-y-8">
        <!-- Add Product Form -->
        <div class="bg-white rounded-lg shadow-sm p-6">
          <h2 class="text-xl font-semibold text-gray-800 mb-4">Add New Product</h2>
          <form @submit.prevent="addProduct" class="space-y-4">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Name</label>
                <input
                  v-model="newProduct.name"
                  type="text"
                  required
                  class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Category</label>
                <select
                  v-model="newProduct.categoryId"
                  required
                  class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">Select Category</option>
                  <option v-for="cat in categories" :key="cat.id" :value="cat.id">
                    {{ cat.name }}
                  </option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Price</label>
                <input
                  v-model.number="newProduct.price"
                  type="number"
                  step="0.01"
                  required
                  class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Stock</label>
                <input
                  v-model.number="newProduct.stock"
                  type="number"
                  required
                  class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div class="md:col-span-2">
                <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
                <textarea
                  v-model="newProduct.description"
                  required
                  rows="3"
                  class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                ></textarea>
              </div>
              <div class="md:col-span-2">
                <label class="block text-sm font-medium text-gray-700 mb-1">Image URL</label>
                <input
                  v-model="newProduct.imageUrl"
                  type="url"
                  required
                  class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>
            <div class="flex justify-end">
              <button
                type="submit"
                class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
              >
                Add Product
              </button>
            </div>
          </form>
        </div>

        <!-- Products Table -->
        <div class="bg-white rounded-lg shadow-sm overflow-hidden">
          <div class="p-4 border-b border-gray-200">
            <h2 class="text-xl font-semibold text-gray-800">Products</h2>
          </div>
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Product</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Category</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Price</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Stock</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr v-for="product in products" :key="product.id">
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="flex items-center">
                      <div class="flex-shrink-0 h-10 w-10">
                        <img :src="product.imageUrl" :alt="product.name" class="h-10 w-10 rounded-full object-cover" />
                      </div>
                      <div class="ml-4">
                        <div class="text-sm font-medium text-gray-900">{{ product.name }}</div>
                        <div class="text-sm text-gray-500">{{ product.description.substring(0, 50) }}...</div>
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm text-gray-900">{{ product.category?.name }}</div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm text-gray-900">{{ formatPrice(product.price) }}</div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span
                      :class="[
                        'px-2 inline-flex text-xs leading-5 font-semibold rounded-full',
                        product.stock > 10 ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                      ]"
                    >
                      {{ product.stock }}
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <button
                      @click="editProduct(product)"
                      class="text-blue-600 hover:text-blue-900 mr-4"
                    >
                      Edit
                    </button>
                    <button
                      @click="deleteProduct(product.id)"
                      class="text-red-600 hover:text-red-900"
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Categories Tab -->
      <div v-if="activeTab === 'categories'" class="space-y-8">
        <!-- Add Category Form -->
        <div class="bg-white rounded-lg shadow-sm p-6">
          <h2 class="text-xl font-semibold text-gray-800 mb-4">Add New Category</h2>
          <form @submit.prevent="addCategory" class="flex space-x-4">
            <div class="flex-grow">
              <input
                v-model="newCategory.name"
                type="text"
                required
                placeholder="Category name"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
            <button
              type="submit"
              class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
            >
              Add Category
            </button>
          </form>
        </div>

        <!-- Categories List -->
        <div class="bg-white rounded-lg shadow-sm overflow-hidden">
          <div class="p-4 border-b border-gray-200">
            <h2 class="text-xl font-semibold text-gray-800">Categories</h2>
          </div>
          <ul class="divide-y divide-gray-200">
            <li
              v-for="category in categories"
              :key="category.id"
              class="px-6 py-4 flex items-center justify-between hover:bg-gray-50"
            >
              <div class="flex items-center">
                <span class="text-sm font-medium text-gray-900">{{ category.name }}</span>
                <span class="ml-2 px-2 py-1 text-xs font-medium bg-gray-100 text-gray-600 rounded-full">
                  {{ getProductCount(category.id) }} products
                </span>
              </div>
              <button
                @click="deleteCategory(category.id)"
                class="text-red-600 hover:text-red-900 text-sm font-medium"
              >
                Delete
              </button>
            </li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../services/api';
import formatPrice from '../utils/formatPrice';

export default {
  name: 'AdminPanel',
  data() {
    return {
      activeTab: 'products',
      tabs: [
        { id: 'products', name: 'Products' },
        { id: 'categories', name: 'Categories' }
      ],
      products: [],
      categories: [],
      newProduct: {
        name: '',
        description: '',
        price: 0,
        imageUrl: '',
        stock: 0,
        categoryId: ''
      },
      newCategory: {
        name: ''
      }
    };
  },
  computed: {
    lowStockCount() {
      return this.products.filter(p => p.stock <= 10).length;
    }
  },
  async created() {
    await Promise.all([this.fetchProducts(), this.fetchCategories()]);
  },
  methods: {
    formatPrice,
    async fetchProducts() {
      try {
        const response = await api.get('/Product');
        this.products = response.data;
      } catch (error) {
        console.error('Failed to fetch products:', error);
        this.$store.dispatch('notification/showNotification', {
          type: 'error',
          message: 'Failed to load products'
        }, { root: true });
      }
    },
    async fetchCategories() {
      try {
        const response = await api.get('/Category');
        this.categories = response.data;
      } catch (error) {
        console.error('Failed to fetch categories:', error);
        this.$store.dispatch('notification/showNotification', {
          type: 'error',
          message: 'Failed to load categories'
        }, { root: true });
      }
    },
    async addProduct() {
      try {
        await api.post('/Product', this.newProduct);
        await this.fetchProducts();
        this.newProduct = {
          name: '',
          description: '',
          price: 0,
          imageUrl: '',
          stock: 0,
          categoryId: ''
        };
        this.$store.dispatch('notification/showNotification', {
          type: 'success',
          message: 'Product added successfully'
        }, { root: true });
      } catch (error) {
        console.error('Failed to add product:', error);
        this.$store.dispatch('notification/showNotification', {
          type: 'error',
          message: 'Failed to add product'
        }, { root: true });
      }
    },
    async deleteProduct(id) {
      if (!confirm('Are you sure you want to delete this product?')) return;
      
      try {
        await api.delete(`/Product/${id}`);
        await this.fetchProducts();
        this.$store.dispatch('notification/showNotification', {
          type: 'success',
          message: 'Product deleted successfully'
        }, { root: true });
      } catch (error) {
        console.error('Failed to delete product:', error);
        this.$store.dispatch('notification/showNotification', {
          type: 'error',
          message: 'Failed to delete product'
        }, { root: true });
      }
    },
    async addCategory() {
      try {
        await api.post('/Category', this.newCategory);
        await this.fetchCategories();
        this.newCategory = { name: '' };
        this.$store.dispatch('notification/showNotification', {
          type: 'success',
          message: 'Category added successfully'
        }, { root: true });
      } catch (error) {
        console.error('Failed to add category:', error);
        this.$store.dispatch('notification/showNotification', {
          type: 'error',
          message: 'Failed to add category'
        }, { root: true });
      }
    },
    async deleteCategory(id) {
      if (!confirm('Are you sure you want to delete this category?')) return;
      
      try {
        await api.delete(`/Category/${id}`);
        await this.fetchCategories();
        this.$store.dispatch('notification/showNotification', {
          type: 'success',
          message: 'Category deleted successfully'
        }, { root: true });
      } catch (error) {
        console.error('Failed to delete category:', error);
        this.$store.dispatch('notification/showNotification', {
          type: 'error',
          message: 'Failed to delete category'
        }, { root: true });
      }
    },
    getProductCount(categoryId) {
      return this.products.filter(p => p.categoryId === categoryId).length;
    },
    editProduct(product) {
      // TODO: Implement edit functionality
      console.log('Edit product:', product);
    }
  }
};
</script>
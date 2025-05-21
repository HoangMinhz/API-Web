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
        <!-- Products Table -->
        <div class="bg-white rounded-lg shadow-sm overflow-hidden">
          <div class="p-4 border-b border-gray-200">
            <h2 class="text-xl font-semibold text-gray-800">Products</h2>
          </div>
          <div class="p-4 border-b border-gray-200">
            <button
              @click="openAddProductModal"
              class="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"            >
              <svg class="-ml-1 mr-2 h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 5a1 1 0 011 1v3h3a1 1 0 110 2h-3v3a1 1 0 11-2 0v-3H6a1 1 0 110-2h3V6a1 1 0 011-1z" clip-rule="evenodd" />
              </svg>
              Add New Product
            </button>
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

  <!-- Add Product Modal -->
  <div v-if="showAddModal" class="fixed inset-0 overflow-y-auto z-50">
    <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
      <!-- Background overlay -->
      <div class="fixed inset-0 transition-opacity" aria-hidden="true">
        <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
      </div>

      <!-- Modal panel -->
      <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
        <form @submit.prevent="addProduct">
          <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
            <div class="mb-4">
              <label class="block text-sm font-medium text-gray-700 mb-2">
                Product Name *
              </label>
              <input
                type="text"
                v-model="newProduct.name"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              />
            </div>

            <div class="mb-4">
              <label class="block text-sm font-medium text-gray-700 mb-2">
                Description *
              </label>
              <textarea
                v-model="newProduct.description"
                required
                rows="3"
                class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              ></textarea>
            </div>

            <div class="grid grid-cols-2 gap-4 mb-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">
                  Price *
                </label>
                <input
                  type="number"
                  v-model="newProduct.price"
                  required
                  min="0"
                  step="0.01"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                />
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">
                  Stock *
                </label>
                <input
                  type="number"
                  v-model="newProduct.stock"
                  required
                  min="0"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                />
              </div>
            </div>

            <div class="mb-4">
              <label class="block text-sm font-medium text-gray-700 mb-2">
                Category *
              </label>
              <select
                v-model.number="newProduct.categoryId"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              >
                <option disabled value="">Select a category</option>
                <option v-for="category in categories" :key="category.id" :value="category.id">
                  {{ category.name }}
                </option>
              </select>
            </div>

            <div class="mb-4">
              <label class="block text-sm font-medium text-gray-700 mb-2">
                Image URL *
              </label>
              <input
                type="text"
                v-model="newProduct.imageUrl"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              />
              <p class="mt-1 text-sm text-gray-500">Enter a valid URL for the product image</p>
            </div>
          </div>

          <div class="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
            <button
              type="submit"
              class="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm"
            >
              Add Product
            </button>
            <button
              type="button"
              @click="closeAddModal"
              class="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../services/api';
import formatPrice from '../utils/formatPrice';
import axios from 'axios';

export default {
  name: 'AdminPanel',
  data() {
    return {
      activeTab: 'products',
      showAddModal: false,
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
        categoryId: null
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
        const response = await api.get('/Product/list');
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
        const response = await api.get('/Category/list');
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
        // Validate form data
        if (!this.newProduct.name || !this.newProduct.description || 
            !this.newProduct.price || !this.newProduct.stock || 
            !this.newProduct.imageUrl || !this.newProduct.categoryId) {
          this.$store.dispatch('notification/showNotification', {
            type: 'error',
            message: 'Please fill in all product information'
          }, { root: true });
          return;
        }

        // Validate numeric fields
        const price = parseFloat(this.newProduct.price);
        const stock = parseInt(this.newProduct.stock);
        const categoryId = parseInt(this.newProduct.categoryId);

        if (isNaN(price) || price <= 0) {
          this.$store.dispatch('notification/showNotification', {
            type: 'error',
            message: 'Invalid product price'
          }, { root: true });
          return;
        }

        if (isNaN(stock) || stock < 0) {
          this.$store.dispatch('notification/showNotification', {
            type: 'error',
            message: 'Invalid stock quantity'
          }, { root: true });
          return;
        }

        if (isNaN(categoryId) || categoryId <= 0) {
          this.$store.dispatch('notification/showNotification', {
            type: 'error',
            message: 'Please select a product category'
          }, { root: true });
          return;
        }

        // Use a direct axios call to avoid any interceptor issues
        const token = this.$store.state.user.token;
        if (!token) {
          this.$store.dispatch('notification/showNotification', {
            type: 'error',
            message: 'You must be logged in to add products'
          }, { root: true });
          return;
        }

        // Prepare product data with capital first letter properties to match C# model
        const productData = {
          Name: this.newProduct.name.trim(),
          Description: this.newProduct.description.trim(),
          Price: price,
          Stock: stock,
          ImageUrl: this.newProduct.imageUrl.trim(),
          CategoryId: categoryId
        };

        console.log('Sending product data to API:', JSON.stringify(productData, null, 2));
        
        // Direct axios request with proper headers
        const response = await axios.post('http://localhost:5285/api/Product', productData, {
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          }
        });
        
        console.log('API Response:', response.data);
        
        // Add new product to list
        this.products.push(response.data);
        
        // Show success message
        this.$store.dispatch('notification/showNotification', {
          type: 'success',
          message: 'Product added successfully'
        }, { root: true });

        // Close modal and reset form
        this.closeAddModal();
        
        // Refresh product list to get latest data
        await this.fetchProducts();
      } catch (error) {
        console.error('Failed to add product:', error);
        
        // Detailed error logging
        if (error.response) {
          console.error('Error response data:', error.response.data);
          console.error('Error response status:', error.response.status);
          
          // Log model state errors if present
          if (error.response.data && error.response.data.errors) {
            console.error('Validation errors:', error.response.data.errors);
          }
        }
        
        // Better error message extraction
        let errorMessage = 'Unable to add product';
        
        if (error.response && error.response.data) {
          if (typeof error.response.data === 'string') {
            errorMessage = error.response.data;
          } else if (error.response.data.message) {
            errorMessage = error.response.data.message;
          } else if (error.response.data.errors) {
            // Try to get specific validation errors
            const errors = error.response.data.errors;
            if (errors.CategoryId) {
              errorMessage = errors.CategoryId[0];
            } else {
              // Combine all error messages
              const allErrors = Object.values(errors).flat();
              if (allErrors.length > 0) {
                errorMessage = allErrors.join(', ');
              }
            }
          }
        }
        
        this.$store.dispatch('notification/showNotification', {
          type: 'error',
          message: errorMessage
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
    },
    openAddProductModal() {
      this.showAddModal = true;
      // Reset form
      this.newProduct = {
        name: '',
        description: '',
        price: 0,
        imageUrl: '',
        stock: 0,
        categoryId: null
      };
    },
    closeAddModal() {
      this.showAddModal = false;
    }
  }
};
</script>
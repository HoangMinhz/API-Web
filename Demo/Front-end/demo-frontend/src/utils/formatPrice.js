/**
 * Formats a price value into a currency string
 * @param {number} price - The price value to format
 * @returns {string} - Formatted price string with currency symbol
 */
export default function formatPrice(price) {
  if (typeof price !== 'number') {
    return '0';
  }
  
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0
  }).format(price);
} 
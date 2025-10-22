import axios from 'axios';


const Product = {
    /**
     * 取得所有商品（支援分頁、篩選、排序）
     * @param {Object} filter - 篩選條件
     * @returns {Promise}
     */
    getAllProducts(filter) {
        return axios.get(`${API_BASE_URL}/product`, {
            params: {
                pageNumber: filter.pageNumber || 1,
                pageSize: filter.pageSize || 12,
                keyword: filter.keyword || undefined,
                categoryId: filter.categoryId || undefined,
                minPrice: filter.minPrice || undefined,
                maxPrice: filter.maxPrice || undefined,
                inStockOnly: filter.inStockOnly || undefined,
                customizableOnly: filter.customizableOnly || undefined,
                sortBy: filter.sortBy || 'newest'
            }
        });
    },

    /**
     * 取得所有分類
     * @returns {Promise}
     */
    getAllCategories() {
        return axios.get(`${API_BASE_URL}/category`);
    }
};

export default Product;
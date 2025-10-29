// src/stores/cart.js
import { defineStore } from 'pinia'

export const useCartStore = defineStore('cart', {
  state: () => ({
    items: [], // { productVariantId, qty }
  }),

  actions: {
    addItem(productVariantId, qty, productName, imageUrl, unitPrice) {
      const existing = this.items.find(i => i.productVariantId === productVariantId)
      if (existing) {
        existing.qty += qty
      } else {
        this.items.push({ productVariantId, qty, productName, imageUrl, unitPrice })
      }
      this.persistCart()
    },

    removeItem(productVariantId) {
      this.items = this.items.filter(i => i.productVariantId !== productVariantId)
      this.persistCart()
    },

    clearCart() {
      this.items = []
      localStorage.removeItem('guest_cart')
    },

    persistCart() {
      localStorage.setItem('guest_cart', JSON.stringify(this.items))
    },

    hydrateCart() {
      try {
        const raw = localStorage.getItem('guest_cart')
        if (raw) this.items = JSON.parse(raw)
      } catch {
        this.items = []
      }
    }
  }
})

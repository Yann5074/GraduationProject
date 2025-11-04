// src/stores/cart.js
import { defineStore } from 'pinia'

export const useCartStore = defineStore('cart', {
  state: () => ({
    items: [], // { productVariantId, qty }
  }),

  actions: {
    addItem(productVariantId, qty, productName, imageUrl, unitPrice, extra={}) {
      const existing = this.items.find(i => i.productVariantId === productVariantId)
      if (existing) {
        existing.qty += qty
        for (const k in extra){
          if (existing[k]==null) existing[k] = extra[k]
        }
      } else {
        this.items.push({ productVariantId, qty, productName, imageUrl, unitPrice, ...extra})
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
      const raw = localStorage.getItem('guest_cart')
      try {
        this.items = raw ? JSON.parse(raw) : []
      } catch {
        this.items = []
      }
    }
  }
})

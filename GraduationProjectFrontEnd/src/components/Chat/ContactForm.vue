<template>
  <div class="form-container">
     <div>
    <ContactForm v-if="!formSubmitted" @formSubmitted="formSubmitted = true" />
    <ChatRoom v-else />
  </div>
    <div class="welcome-message">
      👋您好，歡迎使用智能客服聊天室，
      <br />
      為了維護您的權益，請先填寫以下的表單後使用～
    </div>

    <form @submit.prevent="submitForm">
      <input v-model="form.name" type="text" placeholder="姓名" required />
      <input v-model="form.company" type="text" placeholder="公司" required />
      <input v-model="form.phone" type="tel" placeholder="電話" required />
      <input v-model="form.email" type="email" placeholder="Email" required />

      <button type="submit">提交</button>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'
import ContactForm from './ContactForm.vue'
import ChatRoom from './ChatRoom.vue'

const formSubmitted = ref(false)
const emit = defineEmits(['formSubmitted'])

const form = ref({
  name: '',
  company: '',
  phone: '',
  email: ''
})

async function submitForm() {
  await axios.post('https://localhost:7131/api/ContactForm', {
    fContactName: form.value.name,
    fCompanyName: form.value.company,
    fPhone: form.value.phone,
    fEmail: form.value.email,
    fCreatedAt: new Date()
    // 若有 chatRoomId, memberId 可一併傳送
  })

  emit('formSubmitted') // 通知父層：表單已提交
}
</script>

<style scoped>
.form-container {
  background: #fff;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
}
.welcome-message {
  margin-bottom: 16px;
  font-size: 15px;
  line-height: 1.5;
  color: #333;
  background: #f9f9f9;
  padding: 12px;
  border-radius: 8px;
  font-weight: 500;
}
form {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
input {
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 6px;
}
button {
  padding: 10px;
  background-color: #000;
  color: #fff;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}
</style>

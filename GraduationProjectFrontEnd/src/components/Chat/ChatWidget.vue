<template>
<div>
<div class="floating-button" @click="toggleChat">💬</div>


<div v-if="showChat" class="chat-popup">
<div v-if="status === 'init'">
<div class="welcome">歡迎使用客服，請問您是會員嗎？</div>
<button @click="handleYes">是</button>
<button @click="status = 'form'">否</button>
</div>


<ContactForm v-if="status === 'form'" @formCompleted="onFormDone" />
<ChatRoom v-if="status === 'chat'" :chatRoomId="chatRoomId" />
</div>
</div>
</template>


<script setup>
import { ref } from 'vue'
import ChatRoom from './ChatRoom.vue'
import ContactForm from './ContactForm.vue'
import { useAuthStore } from '@/stores/auth'


const showChat = ref(false)
const status = ref('init') // init | form | chat
const chatRoomId = ref(null)
const auth = useAuthStore()


function toggleChat() {
showChat.value = !showChat.value
}


function handleYes() {
if (!auth.isLoggedIn) {
window.location.href = '/signin'
} else {
// 若已登入，呼叫會員聊天室 API（Index）載入資料
status.value = 'chat'
}
}


function onFormDone(roomId) {
chatRoomId.value = roomId
status.value = 'chat'
}
</script>
<script setup>
import ChatRoom from "@/components/chat/ChatRoom.vue";
import { ref } from "vue";

const showChat = ref(false);
</script>

<template>
  <button @click="showChat = !showChat">💬 開啟聊天室</button>

  <div v-if="showChat" class="chat-popup">
    <ChatRoom />
  </div>
</template>

<style scoped>
.chat-popup {
  position: fixed;
  right: 20px;
  bottom: 20px;
  width: 400px;
  height: 500px;
  background: white;
  border: 1px solid #ddd;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
  border-radius: 12px;
  overflow: hidden;
  z-index: 9999;
}
</style>

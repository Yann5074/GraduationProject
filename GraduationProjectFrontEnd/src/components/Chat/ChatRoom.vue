<template>
  <div class="chat-wrap">
    <!-- 左側：聊天室清單 -->
    <aside class="rooms">
      <div class="search">
        <input v-model="q" @keyup.enter="loadIndex()" placeholder="搜尋名稱或ID..." />
        <button @click="loadIndex()">搜尋</button>
      </div>

      <ul class="room-list">
        <li
          v-for="r in rooms"
          :key="r.fChatRoomId"
          :class="{ active: selectedId === r.fChatRoomId }"
          @click="openRoom(r.fChatRoomId)"
        >
          <img :src="r.image" alt="" />
          <div class="meta">
            <div class="name">{{ r.fName }}</div>
            <div class="last">{{ r.fLastMessage }}</div>
          </div>
          <div class="time">{{ formatTime(r.fLastMessageTime) }}</div>
        </li>
      </ul>
    </aside>

    <!-- 右側：訊息區 -->
    <main class="messages">
      <div class="head">
        <div>ChatRoom #{{ selectedId ?? "-" }}</div>
        <button @click="reloadRoom()" :disabled="!selectedId">重新整理</button>
      </div>

      <div class="msg-list">
        <div class="msg" v-for="m in messages" :key="m.messageId">
          <div class="bubble">
            <div class="content">{{ m.content }}</div>
            <div class="ts">{{ formatTime(m.createdAt) }}</div>
          </div>
        </div>
      </div>

      <div class="send-bar">
        <input
          v-model="content"
          :disabled="!selectedId"
          @keyup.enter="send()"
          placeholder="輸入訊息..."
        />
        <button @click="send()" :disabled="!selectedId || !content.trim()">送出</button>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import axios from "axios";

// 後端基底位址：請依您的實際埠號調整
const api = axios.create({
  baseURL: "https://localhost:7131/api/ChatRoom",
  withCredentials: false,
});

const q = ref<string>("");
const rooms = ref<any[]>([]);
const selectedId = ref<number | null>(null);
const messages = ref<any[]>([]);
const content = ref<string>("");

// 讀取 Index（支援搜尋與指定聊天室）
async function loadIndex(chatRoomId?: number | null) {
  const params: any = {};
  if (q.value?.trim()) params.q = q.value.trim();
  if (chatRoomId != null) params.chatRoomId = chatRoomId;

  const { data } = await api.get("/Index", { params });
  // 依 ASP.NET Core 預設 camelCase，這裡用 data.rooms / data.messages / data.selectedId
  rooms.value = data.rooms ?? [];
  messages.value = data.messages ?? [];
  selectedId.value = data.selectedId ?? null;
}

function openRoom(id: number) {
  loadIndex(id);
}

function reloadRoom() {
  if (selectedId.value != null) loadIndex(selectedId.value);
}

async function send() {
  if (!selectedId.value || !content.value.trim()) return;
  // API 定義：POST /SendMessage?chatRoomId=&content=
  await api.post("/SendMessage", null, {
    params: { chatRoomId: selectedId.value, content: content.value.trim() },
  });
  content.value = "";
  // 送出後再載入目前房間
  await loadIndex(selectedId.value);
}

function formatTime(ts?: string | null) {
  if (!ts) return "";
  try {
    const d = new Date(ts);
    if (isNaN(d.getTime())) return ts;
    return d.toLocaleString();
  } catch {
    return String(ts);
  }
}

onMounted(() => {
  // 初始載入（不帶 chatRoomId）
  loadIndex();
});
</script>

<style scoped>
.chat-wrap {
  display: grid;
  grid-template-columns: 320px 1fr;
  height: 80vh;
  gap: 12px;
  padding: 12px;
  box-sizing: border-box;
}
.rooms {
  border: 1px solid #ddd;
  border-radius: 12px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}
.search {
  display: flex;
  gap: 8px;
  padding: 10px;
  border-bottom: 1px solid #eee;
}
.search input {
  flex: 1;
  padding: 8px 10px;
}
.room-list {
  list-style: none;
  margin: 0;
  padding: 0;
  overflow: auto;
}
.room-list li {
  display: grid;
  grid-template-columns: 44px 1fr auto;
  align-items: center;
  gap: 10px;
  padding: 10px;
  cursor: pointer;
  border-bottom: 1px solid #f2f2f2;
}
.room-list li.active {
  background: #f7f9ff;
}
.room-list img {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  object-fit: cover;
}
.room-list .meta {
  overflow: hidden;
}
.room-list .name {
  font-weight: 600;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.room-list .last {
  font-size: 12px;
  color: #666;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.room-list .time {
  font-size: 12px;
  color: #888;
  margin-left: 6px;
}

.messages {
  border: 1px solid #ddd;
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.head {
  display: flex;
  justify-content: space-between;
  padding: 10px 12px;
  border-bottom: 1px solid #eee;
}
.msg-list {
  flex: 1;
  padding: 12px;
  overflow: auto;
  background: #fafafa;
}
.msg {
  margin-bottom: 10px;
}
.bubble {
  background: white;
  border: 1px solid #eee;
  border-radius: 10px;
  padding: 8px 10px;
  display: inline-block;
  max-width: 70%;
}
.bubble .content {
  white-space: pre-wrap;
  word-break: break-word;
}
.bubble .ts {
  text-align: right;
  font-size: 11px;
  color: #888;
  margin-top: 4px;
}
.send-bar {
  display: flex;
  gap: 8px;
  padding: 10px;
  border-top: 1px solid #eee;
  background: #fff;
}
.send-bar input {
  flex: 1;
  padding: 10px;
}
</style>

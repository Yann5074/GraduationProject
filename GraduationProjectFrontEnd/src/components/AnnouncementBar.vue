<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue';
import { getActiveAnnouncements } from '@/api/Announcement';

const announcements = ref([]);
const etag = ref(null);
const loading = ref(true);

// carousel state
const currentIndex = ref(0);
const isPaused = ref(false);
const intervalMs = ref(4000); // change if you want a different speed
let timer = null;

const currentAnnouncement = computed(() =>
  announcements.value.length ? announcements.value[currentIndex.value] : null
);

const hasMultiple = computed(() => announcements.value.length > 1);
const goTo = (i) => { if (hasMultiple.value) currentIndex.value = i; };

const loadData = async () => {
  try {
    const res = await getActiveAnnouncements(etag.value);
    if (!res.notModified && res.items) {
      announcements.value = res.items;
      // reset index if new list is shorter or items changed
      if (currentIndex.value >= announcements.value.length) currentIndex.value = 0;
    }
    if (res.etag) etag.value = res.etag;
  } catch (e) {
    console.error('公告載入失敗:', e);
  } finally {
    loading.value = false;
  }
};

function next() {
  if (!announcements.value.length) return;
  currentIndex.value = (currentIndex.value + 1) % announcements.value.length;
}

function startTimer() {
  stopTimer();
  if (announcements.value.length > 1 && !isPaused.value) {
    timer = setInterval(next, intervalMs.value);
  }
}

function stopTimer() {
  if (timer) {
    clearInterval(timer);
    timer = null;
  }
}

function handleMouseEnter() {
  isPaused.value = true;
  stopTimer();
}

function handleMouseLeave() {
  isPaused.value = false;
  startTimer();
}

onMounted(async () => {
  await loadData();
  startTimer();
});

onUnmounted(stopTimer);

// restart timer when announcements or interval change
watch([announcements, intervalMs], () => {
  if (currentIndex.value >= announcements.value.length) currentIndex.value = 0;
  startTimer();
}, { deep: true });
</script>

<template>
  <section
    v-if="!loading && announcements.length"
    class="announcement"
    @mouseenter="handleMouseEnter"
    @mouseleave="handleMouseLeave"
  >
    <div class="annouce-card" role="region" aria-label="最新公告">
      <transition name="fade" mode="out-in">
        <div
          v-if="currentAnnouncement"
          :key="currentAnnouncement.id"
          class="announce-line"
        >
          <span class="icon" aria-hidden="true">📣</span>
          <span class="title">{{ currentAnnouncement.title }}</span>
          <span class="dot">•</span>
          <span class="msg">{{ currentAnnouncement.message }}</span>

          <!-- 可選連結（有 url 才顯示） -->
          <a
            v-if="currentAnnouncement.url"
            class="cta"
            :href="currentAnnouncement.url"
            target="_blank"
            rel="noopener noreferrer"
          >了解更多</a>
        </div>
      </transition>

      <!-- 指示點（可點） -->
      <div v-if="hasMultiple" class="dots" role="tablist" aria-label="切換公告">
        <button
          v-for="(a, i) in announcements"
          :key="a.id ?? i"
          class="dot-btn"
          :class="{ active: i === currentIndex }"
          :aria-label="`切換至第 ${i+1} 則公告`"
          :aria-selected="i === currentIndex"
          @click="goTo(i)"
        />
      </div>
    </div>
  </section>
</template>


<style scoped>
/* 整體區塊留白 */
.announcement {
  padding: 0.5rem 1rem;
}

/* 膠囊卡片：玻璃霧面、漸層、陰影 */
.annouce-card {
  max-width: 1080px;
  margin: 0 auto;
  padding: 0.6rem 1rem;
  border-radius: 999px;
  background: linear-gradient(135deg, rgba(255,255,255,0.75), rgba(255,255,255,0.55));
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255,255,255,0.6);
  box-shadow: 0 6px 24px rgba(0,0,0,0.08);
}

/* 文字行：水平置中、可截斷 */
.announce-line {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: .5rem;
  min-height: 1.75rem;
  color: #1f2937; /* slate-800 */
  font-size: 0.95rem;
  line-height: 1.5;
  text-align: center;
  padding: .2rem .5rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* 小圖示 */
.icon {
  font-size: 1.05rem;
}

/* 標題加粗、訊息正常 */
.title {
  font-weight: 700;
}
.msg {
  font-weight: 400;
  opacity: .95;
}
.dot {
  opacity: .6;
}

/* 連結樣式：細膩下劃線，hover 提示色 */
.cta {
  margin-left: .5rem;
  font-weight: 600;
  text-decoration: none;
  position: relative;
}
.cta::after {
  content: "";
  position: absolute;
  left: 0; right: 0; bottom: -2px;
  height: 1px;
  background: currentColor;
  opacity: .35;
  transition: opacity .2s ease, transform .2s ease;
  transform: scaleX(.6);
}
.cta:hover::after { opacity: .7; transform: scaleX(1); }

/* 指示點（可點） */
.dots {
  display: flex;
  justify-content: center;
  gap: .4rem;
  margin-top: .35rem;
}
.dot-btn {
  width: 7px; height: 7px;
  border-radius: 999px;
  background: rgba(31,41,55,.25);
  border: 0;
  padding: 0;
  cursor: pointer;
  transition: transform .2s ease, background .2s ease;
}
.dot-btn:hover { transform: scale(1.15); }
.dot-btn.active { background: rgba(31,41,55,.9); }

/* 動畫 */
.fade-enter-active, .fade-leave-active {
  transition: opacity .45s ease;
}
.fade-enter-from, .fade-leave-to { opacity: 0; }

/* RWD：小螢幕自動換行 */
@media (max-width: 480px) {
  .annouce-card { border-radius: 16px; }
  .announce-line {
    white-space: normal;
    text-align: left;
    justify-content: flex-start;
  }
}

/* 無動畫偏好：遵守使用者設定 */
@media (prefers-reduced-motion: reduce) {
  .fade-enter-active, .fade-leave-active { transition: none; }
  .dot-btn { transition: none; }
}
</style>


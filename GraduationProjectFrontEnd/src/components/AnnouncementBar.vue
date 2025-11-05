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
    class="announcement py-3 bg-transparent text-dark text-center"
    @mouseenter="handleMouseEnter"
    @mouseleave="handleMouseLeave"
  >
    <div class="container">
      <transition name="fade" mode="out-in">
        <div
          v-if="currentAnnouncement"
          :key="currentAnnouncement.id"
          class="fs-6 mb-1"
        >
          🎉<strong>{{ currentAnnouncement.title }}</strong>🎉 {{ currentAnnouncement.message }}
        </div>
      </transition>
    </div>
  </section>
</template>

<style scoped>
.announcement {
  background-color: rgba(255,255,255,0.8);
}
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.6s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
</style>

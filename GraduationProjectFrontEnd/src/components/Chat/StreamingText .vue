<template>
  <span class="streaming-text" @click="skipAll" :title="enabled ? '點我顯示完整' : ''">
    {{ display }}
    <span v-if="enabled && !finished" class="caret">▋</span>
  </span>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onBeforeUnmount } from 'vue'

const props = defineProps<{
  text: string
  enabled?: boolean
  speed?: number
}>()

const emit = defineEmits<{ (e: 'done'): void }>()

const display = ref('')
const finished = ref(false)
let stop = false
let timer: number | null = null

const PUNCT_PAUSE: Record<string, number> = {
  ',': 180, '.': 260, '?': 280, '!': 280,
  '，': 220, '。': 320, '、': 160, '！': 300, '？': 300
}

function clearTimer() {
  if (timer !== null) {
    clearTimeout(timer)
    timer = null
  }
}

function skipAll() {
  if (!props.enabled || finished.value) return
  stop = true
  clearTimer()
  display.value = props.text
  finished.value = true
  emit('done')
}

function play() {
  clearTimer()
  display.value = ''
  finished.value = false
  stop = false

  if (!props.enabled) {
    display.value = props.text
    finished.value = true
    return
  }

  let i = 0
  const base = Math.max(8, props.speed ?? 28)
  const tick = () => {
    if (stop) return
    const ch = props.text[i++]
    display.value += ch ?? ''
    if (i >= props.text.length) {
      finished.value = true
      emit('done')
      return
    }
    const extra = PUNCT_PAUSE[ch as keyof typeof PUNCT_PAUSE] ?? 0
    const jitter = Math.random() * 20
    timer = window.setTimeout(tick, base + extra + jitter)
  }
  timer = window.setTimeout(tick, base)
}

watch(() => props.text, play, { immediate: true })
watch(() => props.enabled, play)
onMounted(play)
onBeforeUnmount(() => { stop = true; clearTimer() })
</script>

<style scoped>
.streaming-text { white-space: pre-wrap; word-break: break-word; }
.caret {
  display: inline-block;
  animation: blink 1s steps(1, end) infinite;
}
@keyframes blink { 50% { opacity: 0; } }
</style>

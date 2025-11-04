<template>
    <Teleport to="body">
        <transition name="fade">
            <div v-if="visible" class="position-fixed top-0 start-0 w-100 h-100 bg-dark bg-opacity-25" style="z-index: 2000;">
                <div class="d-flex align-items-center justify-content-center h-100">
                    <div class="bg-body rounded-3 shadow p-4 d-flex flex-column align-items-center justify-content-center text-center" role="status" style="min-width: 220px; min-height: 220px ;padding:1.25rem;" aria-live="polite" aria-busy="true">
                        <div class="spinner-border" role="status" style="width: 3rem;height: 3rem; border-width: .35rem;" aria-hidden="true"></div>
                        <br/>
                        <div v-if="message" class="mt-2 small fs-5">
                            <span v-for="(char, index) in message.split('')" :key="index" class="wavy-char" :style="{'animation-delay': (index * 0.1) + 's'}">
                                {{ char === ' '? '&nbsp' : char }}
                            </span>
                            <!-- {{ message }} -->
                        </div>
                    </div>
                </div>
            </div>
        </transition>
    </Teleport>
</template>


<style scoped>
.fade-enter-active, .fade-leave-active{
    transition: opacity .15s linear;
}
.fade-enter-form, .fade-leave-to{
    opacity: 0;
}
/* 定義波浪起伏特效 */
@keyframes wave-up-down{
    0%, 100%{
        transform: translateY(0);
    }
    50%{
        transform: translateY(-8px);
    }
}
/* 把特效應用到每個字元 */
.wavy-char{
    display: inline-block;
    animation: wave-up-down 1.5s infinite ease-in-out;
}
</style>


<script setup>
import { useLoadingStore } from '@/stores/loading';
import {ref, computed, watch} from 'vue';

const props = defineProps({
    scope: String,
    message: String,
    force: {type: Boolean, default: false},
    delay: {type: Number, default: 120}, // 顯示前延遲
    minDuration: {type: Number, default: 300} // 最小顯示時間
});

const store = useLoadingStore();
const activeRaw = computed(() =>
props.force ? true :
    (props.scope ? store.isActive(props.scope) : store.isActive())
);

const visible = ref(false);
let shownAt = 0;
let timer;

watch(activeRaw, (now) =>{
    clearTimeout(timer);
    if (now){
        timer = setTimeout(() =>{
            visible.value = true;
            shownAt = Date.now();
        }, props.delay);
    }else{
        const elapsed = Date.now() - shownAt;
        const remain = Math.max(0, props.minDuration - elapsed);
        timer = setTimeout(() =>{
            visible.value = false;
        }, remain);
    }
}, {immediate: true});
</script>
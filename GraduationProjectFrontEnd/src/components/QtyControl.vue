<script setup>
const props = defineProps({
  modelValue: { type: Number, default: 1 },    //預設1
  min: { type: Number, default: 1 },           //最小值1
  max: { type: Number, default: 99 },          //最大值99
  name: { type: String, default: 'quantity' }, //送出表單時需要name
})
const emit = defineEmits(['update:modelValue'])//父層自動更新狀態

//限制範圍
const clamp = (v) => Math.min(props.max, Math.max(props.min, Number(v) || props.min))

const dec = () => emit('update:modelValue', clamp(props.modelValue - 1))
const inc = () => emit('update:modelValue', clamp(props.modelValue + 1))
const onInput = (e) => emit('update:modelValue', clamp(e.target.value))
</script>

<template>
  <div class="input-group" style="max-width: 120px;">
    <button type="button" class="btn btn-outline-black" @click="dec">
      <i class="bi bi-dash-square"></i>
    </button>
    <input
      type="text"
      class="form-control text-center"
      :value="modelValue"
      :min="min"
      :max="max"
      step="1"
      inputmode="numeric"
      @input="onInput"
      @blur="onInput"
    />
    <button type="button" class="btn btn-outline-black" @click="inc">
      <i class="bi bi-plus-square"></i>
    </button>
  </div>
</template>

<style scoped>
.input-group {
  display: inline-flex;           /* inline-flex 讓它像文字一樣排列 */
  align-items: center;            /* 垂直置中 */
  justify-content: center;
  vertical-align: middle;         /* 與文字中線對齊 */
}
</style>
<!-- src/components/Furniture3DViewer_PBR.vue (fixed) -->
<template>
  <div class="three-wrap">
    <div ref="canvasHost" class="three-canvas"></div>

    <!-- 簡單的 loading/錯誤提示 -->
    <div v-if="loading" class="overlay">Loading 3D…</div>
    <div v-else-if="error" class="overlay error">{{ error }}</div>
  </div>
</template>

<script setup>
import { onMounted, onBeforeUnmount, ref, watch } from 'vue'
import * as THREE from 'three'

import { RGBELoader } from 'three/examples/jsm/loaders/RGBELoader.js'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js'
import ProductAPI from '@/api/Product' // default export OK
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js'



// -------------------- Props --------------------
const props = defineProps({
  productId: { type: Number, required: true },
  variantId: { type: Number, default: null },   // 切色時改這個
  autoRotate: { type: Boolean, default: false },
  exposure: { type: Number, default: 1.0 },
  rotateSpeed: { type: Number, default: 0.6 },
  modelScale: { type: Number, default: 1.0 },   // 視模型調整
})

//-------------------------------------------------------


// -------------------- Refs & state --------------------
const canvasHost = ref(null)
const loading = ref(false)
const error = ref('')

let renderer, scene, camera, controls
let currentModel = null
let pmremGen = null

// 建立一次 GLTFLoader，避免重複宣告／重覆載入
const gltfLoader = new GLTFLoader()
gltfLoader.setCrossOrigin('anonymous')

// 用於重用的貼圖快取，避免重複下載
const texCache = new Map()

function getTexture(url, isSRGB) {
  if (!url) return null
  if (texCache.has(url)) return texCache.get(url)

  const loader = new THREE.TextureLoader()
  loader.setCrossOrigin('anonymous')   // ✅ 讓圖片也能跨域載入

  const tex = loader.load(url)
  tex.flipY = false
  tex.colorSpace = isSRGB ? THREE.SRGBColorSpace : THREE.LinearSRGBColorSpace
  tex.anisotropy = 8
  tex.wrapS = tex.wrapT = THREE.RepeatWrapping

  texCache.set(url, tex)
  return tex
}


async function getPBRDto() {
  const res = await ProductAPI.getPBR(props.productId, props.variantId)
  console.log('🎨 Viewer 收到 PBR 資料:', res)

  if (!res || !res.success) throw new Error('PBR API 回傳失敗')
  if (!res.data || !Array.isArray(res.data) || res.data.length === 0) throw new Error('沒有 PBR 資料')

  const dto = res.data[0]
  console.log('✅ Viewer 使用的 PBR 資料:', dto)
  return dto
}

async function setupEnvMap(envUrl) {
  // 先清掉舊的環境
  if (scene.environment && scene.environment.isTexture) {
    scene.environment.dispose?.()
  }
  if (!envUrl) {
    scene.environment = null
    return
  }
  const hdr = await new Promise((resolve, reject) => {
    new RGBELoader().load(envUrl, resolve, undefined, reject)
  })
  const envMap = pmremGen.fromEquirectangular(hdr).texture
  hdr.dispose()
  scene.environment = envMap
  scene.background = null // 如果你想要 HDR 當背景可以設 envMap
}

function applyPBRToMesh(mesh, dto) {
  let mat = mesh.material
  if (!(mat && (mat.isMeshStandardMaterial || mat.isMeshPhysicalMaterial))) {
    mat = new THREE.MeshStandardMaterial({ color: 0xffffff })
  }

  const hasEnv  = !!scene.environment
  const base    = dto.baseColorUrl ? getTexture(dto.baseColorUrl, true)  : null
  const met     = dto.metallicUrl  ? getTexture(dto.metallicUrl,  false) : null
  const rough   = dto.roughnessUrl ? getTexture(dto.roughnessUrl, false) : null
  const normal  = dto.normalUrl    ? getTexture(dto.normalUrl,    false) : null
  const ao      = dto.aoUrl        ? getTexture(dto.aoUrl,        false) : null
  const emi     = dto.emissiveUrl  ? getTexture(dto.emissiveUrl,  false) : null

  // ✅ 沒有 HDR 時，不要硬設金屬 = 1
  mat.metalness = met && hasEnv ? 1.0 : 0.05
  mat.roughness = rough ? 1.0 : 0.6
  mat.envMapIntensity = hasEnv ? 1.0 : 0.0

  mat.map           = base
  mat.metalnessMap  = met  || null
  mat.roughnessMap  = rough|| null
  mat.normalMap     = normal||null
  mat.aoMap         = ao   || null

  if (emi) {
    mat.emissiveMap = emi
    mat.emissiveIntensity = 0.6
  } else {
    mat.emissiveMap = null
    mat.emissiveIntensity = 0.0
  }

  // ✅ 很多模型沒有 uv2，aoMap 會沒效果；補一份 uv 給 uv2（可見度更穩）
  if (ao && mesh.geometry && !mesh.geometry.getAttribute('uv2') && mesh.geometry.getAttribute('uv')) {
    mesh.geometry.setAttribute('uv2', mesh.geometry.getAttribute('uv'))
  }

  mat.needsUpdate = true
  mesh.material = mat
  mesh.castShadow = true
  mesh.receiveShadow = true
}


function traverseMeshes(root, fn) {
  root.traverse((obj) => {
    if (obj.isMesh) fn(obj)
  })
}

async function loadModel(dto) {
  if (currentModel) {
    scene.remove(currentModel)
    currentModel.traverse((o) => {
      if (o.isMesh) {
        o.geometry?.dispose?.()
        if (o.material?.map) o.material.map.dispose?.()
        if (o.material) o.material.dispose?.()
      }
    })
    currentModel = null
  }

  const gltf = await new Promise((resolve, reject) => {
    gltfLoader.load(dto.modelUrl, resolve, undefined, reject)
  })
  currentModel = gltf.scene
  currentModel.scale.setScalar(props.modelScale)

  // 套用 PBR
  traverseMeshes(currentModel, (mesh) => applyPBRToMesh(mesh, dto))
  scene.add(currentModel)

  // 自動調整鏡頭框選
  fitCameraToObject(currentModel)
}

function fitCameraToObject(object3D) {
  const box = new THREE.Box3().setFromObject(object3D)
  const size = new THREE.Vector3()
  const center = new THREE.Vector3()
  box.getSize(size)
  box.getCenter(center)

  const maxDim = Math.max(size.x, size.y, size.z)
  const fov = camera.fov * (Math.PI / 180)

  // ✅ 用 tan，而不是 sin
  let cameraZ = (maxDim * 0.5) / Math.tan(fov / 2)
  cameraZ *= 1.6 // 留安全邊界

  camera.position.set(center.x + cameraZ, center.y + cameraZ * 0.35, center.z + cameraZ)
  camera.near = Math.max(0.01, cameraZ / 100)
  camera.far = cameraZ * 200
  camera.updateProjectionMatrix()
  controls.target.copy(center)
  controls.update()
}


async function boot() {
  loading.value = true
  error.value = ''
  try {
    // Renderer
    renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true })
    renderer.outputColorSpace = THREE.SRGBColorSpace
    renderer.toneMapping = THREE.ACESFilmicToneMapping
    renderer.toneMappingExposure = Math.max(1.0, props.exposure) // ✅ 至少 1.0
    renderer.shadowMap.enabled = true 
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2.5))
    renderer.setSize(canvasHost.value.clientWidth, canvasHost.value.clientHeight)
    canvasHost.value.appendChild(renderer.domElement)

     if ('physicallyCorrectLights' in renderer) renderer.physicallyCorrectLights = true
     renderer.shadowMap.enabled = true
     renderer.shadowMap.type = THREE.PCFSoftShadowMap

    // Scene/Camera
    scene = new THREE.Scene()
    camera = new THREE.PerspectiveCamera(50, canvasHost.value.clientWidth / canvasHost.value.clientHeight, 0.01, 2000)
    camera.position.set(0, 1, 3)

    // Controls
    controls = new OrbitControls(camera, renderer.domElement)
    controls.enableDamping = false
    controls.autoRotate = false  
    controls.autoRotateSpeed = props.rotateSpeed

     // ✅ Light（可搭配 envMap）
    renderer.shadowMap.enabled = true
    const hemi = new THREE.HemisphereLight(0xffffff, 0x404040, 0.9) // 提高強度
    scene.add(hemi)

    const amb = new THREE.AmbientLight(0xffffff, 1.0)               // 提高強度
    scene.add(amb)

    const key = new THREE.DirectionalLight(0xffffff, 3.0)           // 提高強度
    key.position.set(5, 10, 7)
    key.castShadow = true
    scene.add(key)

    // （可選）地面陰影
    const plane = new THREE.Mesh(
      new THREE.PlaneGeometry(20, 20),
      new THREE.ShadowMaterial({ opacity: 0.25 })
    )
    plane.rotation.x = -Math.PI / 2
    plane.receiveShadow = true
    scene.add(plane)

    // PMREM
    pmremGen = new THREE.PMREMGenerator(renderer)
    pmremGen.compileEquirectangularShader()

    // 拉後端資料
    const dto = await getPBRDto()
    if (!dto?.modelUrl) throw new Error('找不到 3D 模型 URL')
    await setupEnvMap(dto.envMap || null)
    await loadModel(dto)

    // 監聽尺寸
    window.addEventListener('resize', onResize)

    // 進入渲染
    tick()
  } catch (e) {
    console.error(e)
    error.value = e?.message || '3D 載入失敗'
  } finally {
    loading.value = false
  }
}



function onResize() {
  if (!renderer || !camera || !canvasHost.value) return
  const w = canvasHost.value.clientWidth
  const h = canvasHost.value.clientHeight
  renderer.setSize(w, h)
  camera.aspect = w / h
  camera.updateProjectionMatrix()
}

function tick() {
  requestAnimationFrame(tick)
  controls?.update?.()
  renderer?.render?.(scene, camera)
}

// 切換 variant 只更新 baseColor，避免重載模型
watch(() => props.variantId, async () => {
  if (!currentModel) return
  try {
    const dto = await getPBRDto()
    traverseMeshes(currentModel, (mesh) => {
      const mat = mesh.material
      mat.map = getTexture(dto.baseColorUrl, true) // ← 以 *Url 結尾
      mat.needsUpdate = true
    })
  } catch (e) {
    console.error(e)
    error.value = e?.message || '切換顏色失敗'
  }
})

onMounted(boot)

onBeforeUnmount(() => {
  window.removeEventListener('resize', onResize)
  controls?.dispose?.()
  pmremGen?.dispose?.()
  if (scene) {
    scene.traverse((o) => {
      if (o.isMesh) {
        o.geometry?.dispose?.()
        if (o.material?.map) o.material.map.dispose?.()
        if (o.material) o.material.dispose?.()
      }
    })
  }
  renderer?.dispose?.()
  texCache.forEach((t) => t.dispose?.())
  texCache.clear()
})
</script>

<style scoped>
.three-wrap {
  position: relative;
  width: 100%;
  height: 520px; /* 可依頁面調整 */
  border-radius: 12px;
  overflow: hidden;
  background: #0a0a0a10;
}
.three-canvas {
  width: 100%;
  height: 100%;
}
.overlay {
  position: absolute; inset: 0;
  display: grid; place-items: center;
  font-weight: 600;
  backdrop-filter: blur(2px);
}
.overlay.error { color: #b00020; }
</style>

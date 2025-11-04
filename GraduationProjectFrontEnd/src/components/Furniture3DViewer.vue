<!-- src/components/Furniture3DViewer_PBR.vue (match example) -->
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
import { HDRLoader } from 'three/addons/loaders/HDRLoader.js'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js'
import ProductAPI from '@/api/Product'
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js'
import { RoomEnvironment } from 'three/examples/jsm/environments/RoomEnvironment.js'

let pmremGenerator = null
let pmremEnvRT = null





// -------------------- Props --------------------
const props = defineProps({
  productId: { type: Number, required: true },
  variantId: { type: Number, default: null },   // 切色時改這個
  autoRotate: { type: Boolean, default: false },
  exposure: { type: Number, default: 1.0 },     // ← 與示例一致的預設曝光
  rotateSpeed: { type: Number, default: 0.6 },
  modelScale: { type: Number, default: 1.0 },




  envHdrUrl: { type: String, default: '' },          // HDR 檔案 URL
  showHdrAsBackground: { type: Boolean, default: true },
  envIntensity: { type: Number, default: 1.0 },
})


// -------------------- Refs & state --------------------
const canvasHost = ref(null)
const loading = ref(false)
const error = ref('')

let renderer, scene, camera, controls
let currentModel = null

const gltfLoader = new GLTFLoader()
gltfLoader.setCrossOrigin('anonymous')

// 用於重用的貼圖快取
const texCache = new Map()

function getTexture(url, kind) {
  if (!url) return null
  const key = `${kind}:${url}`
  if (texCache.has(key)) return texCache.get(key)

  const loader = new THREE.TextureLoader()
  loader.setCrossOrigin('anonymous')

  const tex = loader.load(url)
  tex.flipY = false

  // sRGB / Linear
  if (kind === 'base' || kind === 'emissive') {
    tex.colorSpace = THREE.SRGBColorSpace
  } else {
    tex.colorSpace = THREE.NoColorSpace
  }

  const maxAniso = renderer?.capabilities?.getMaxAnisotropy?.() ?? 1
  tex.anisotropy = Math.min(maxAniso, 8)
  tex.minFilter = THREE.LinearMipmapLinearFilter
  tex.magFilter = THREE.LinearFilter
  tex.wrapS = tex.wrapT = THREE.RepeatWrapping

  texCache.set(key, tex)
  return tex
}

async function getPBRDto() {
  const res = await ProductAPI.getPBR(props.productId, props.variantId)
  if (!res || !res.success) throw new Error('PBR API 回傳失敗')
  if (!res.data || !Array.isArray(res.data) || res.data.length === 0) throw new Error('沒有 PBR 資料')

  //依 variantId 選對那一筆
  if (props.variantId) {
    const vid = Number(props.variantId)
    const hit = res.data.find(d =>
      Number(d.productVariantId ?? d.ProductVariantId) === vid
    )
    if (hit) return hit
    console.warn('[PBR] 指定 variantId 找不到對應資料，退回第一筆', { vid, list: res.data })
  }
  return res.data[0]
}

async function setupEnvMap(envUrl) {
  // 先清舊環境
  if (scene.environment && scene.environment.isTexture) {
    scene.environment.dispose?.()
  }
  if (!envUrl) {
    // 沒提供就保持 RoomEnvironment 作為環境光，不設背景圖
    return
  }

  // 用 HDR 當環境
  const hdr = await new HDRLoader().loadAsync(envUrl)
  hdr.mapping = THREE.EquirectangularReflectionMapping
  const envMap = pmremGenerator.fromEquirectangular(hdr).texture
  hdr.dispose()

  scene.environment = envMap

  //  用 IBL 同時當背景，並讓背景「模糊」
  scene.background = envMap
  // 0~1（甚至可 >1），數值越大越模糊
  scene.backgroundBlurriness = 0.6
  // 背景亮度（不影響材質反射強度）
  scene.backgroundIntensity = 1.0
}

function applyPBRToMesh(mesh, dto) {
  let mat = mesh.material
  if (!(mat && (mat.isMeshStandardMaterial || mat.isMeshPhysicalMaterial))) {
    mat = new THREE.MeshStandardMaterial({ color: 0xffffff })
  }

  const hasEnv = !!scene.environment

  mat.map           = dto.baseColorUrl ? getTexture(dto.baseColorUrl, 'base') : null
  mat.metalnessMap  = dto.metallicUrl  ? getTexture(dto.metallicUrl,  'metallic') : null
  mat.roughnessMap  = dto.roughnessUrl ? getTexture(dto.roughnessUrl, 'roughness') : null
  mat.normalMap     = dto.normalUrl    ? getTexture(dto.normalUrl,    'normal') : null
  mat.aoMap         = dto.aoUrl        ? getTexture(dto.aoUrl,        'ao') : null
  mat.emissiveMap   = dto.emissiveUrl  ? getTexture(dto.emissiveUrl,  'emissive') : null

  // 保守基線（不產生陰影、由 IBL 主導）
  mat.metalness = (mat.metalnessMap && hasEnv) ? 1.0 : 0.0
  mat.roughness = mat.roughnessMap ? 1.0 : 0.5
  mat.envMapIntensity = hasEnv ? 1.0 : 0.0 // [MATCH-EXAMPLE] IBL 主導

  if (mat.normalMap) {
    mat.normalScale = new THREE.Vector2(1.2, -1.2)
  }
  if (mat.emissiveMap) {
    mat.emissiveIntensity = 0.6
  }

  if (mat.aoMap && mesh.geometry && !mesh.geometry.getAttribute('uv2') && mesh.geometry.getAttribute('uv')) {
    mesh.geometry.setAttribute('uv2', mesh.geometry.getAttribute('uv'))
  }

  mat.needsUpdate = true
  mesh.material = mat

  // [MATCH-EXAMPLE] 不使用陰影
  mesh.castShadow = false
  mesh.receiveShadow = false
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

  traverseMeshes(currentModel, (mesh) => applyPBRToMesh(mesh, dto))
  scene.add(currentModel)

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
  let cameraZ = (maxDim * 0.5) / Math.tan(fov / 2)
  cameraZ *= 1

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
    renderer.toneMapping = THREE.ACESFilmicToneMapping                  // [MATCH-EXAMPLE]
    renderer.toneMappingExposure = (props.exposure ?? 1.8)              // [MATCH-EXAMPLE]
    renderer.shadowMap.enabled = false                                  // [MATCH-EXAMPLE] 不啟用陰影
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2.5))
    renderer.setSize(canvasHost.value.clientWidth, canvasHost.value.clientHeight)
    canvasHost.value.appendChild(renderer.domElement)

    if ('physicallyCorrectLights' in renderer) renderer.physicallyCorrectLights = true

    // Scene / Camera
    scene = new THREE.Scene()
    camera = new THREE.PerspectiveCamera(45, canvasHost.value.clientWidth / canvasHost.value.clientHeight, 0.1, 20) // [MATCH-EXAMPLE] 45°/0.1~20
    camera.position.set(-0.75, 0.7, 1.25) // [MATCH-EXAMPLE] 與範例相同初始位

 

    // Environment / IBL（RoomEnvironment + PMREM）
    const environment = new RoomEnvironment()                            // [MATCH-EXAMPLE]
    pmremGenerator = new THREE.PMREMGenerator(renderer)                  // [MATCH-EXAMPLE]
    pmremEnvRT = pmremGenerator.fromScene(environment)                   // [MATCH-EXAMPLE]
    scene.background = new THREE.Color(0xbbbbbb)                         // [MATCH-EXAMPLE]
    scene.environment = pmremEnvRT.texture                               // [MATCH-EXAMPLE]

     const hemi = new THREE.HemisphereLight(
      new THREE.Color('#bcd6ff'), // 天空偏冷藍
      new THREE.Color('#ffe9cc'), // 地面偏暖
      0.9
    )
    scene.add(hemi)

    const amb = new THREE.AmbientLight(new THREE.Color('#ffffff'), 0.35)
    scene.add(amb)

   const key = new THREE.DirectionalLight(new THREE.Color('#ffd7a3'), 1.1) // 帶點暖色
    key.position.set(5, 10, 7)
    scene.add(key)
    
    // Controls（對齊示例）
    controls = new OrbitControls(camera, renderer.domElement)
    controls.enableDamping = true                                        // [MATCH-EXAMPLE]
    controls.minDistance = 1                                             // [MATCH-EXAMPLE]
    controls.maxDistance = 10                                            // [MATCH-EXAMPLE]
    controls.target.set(0, 0.35, 0)                                      // [MATCH-EXAMPLE]
    controls.update()

    // 不加入任何燈光（保持與示例一致）

    // 後端資料
    const dto = await getPBRDto()
    if (!dto?.modelUrl) throw new Error('找不到 3D 模型 URL')

    // 若 API 提供 envMap，就覆蓋 IBL；否則維持 RoomEnvironment
    await setupEnvMap(dto.envMap || null)

    await loadModel(dto)

    window.addEventListener('resize', onResize)
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

// 切換 variant 只更新 baseColor（保留你的流程）
watch(() => props.variantId, async () => {
  if (!currentModel) return
  try {
    // 從後端拿這個變體的 PBR（重點是新的 baseColorUrl）
    const dto = await getPBRDto()
    if (!dto?.baseColorUrl) return

    // 加一個 cache-buster，避免拿到舊貼圖
    const freshBase = `${dto.baseColorUrl}${dto.baseColorUrl.includes('?') ? '&' : '?'}v=${Date.now()}`

    traverseMeshes(currentModel, (mesh) => {
      const mats = Array.isArray(mesh.material) ? mesh.material : [mesh.material]
      mats.forEach((m) => {
        if (!m) return
        // 釋放舊貼圖（如有）
        if (m.map) {
          m.map.dispose?.()
        }
        // 只換 baseColor，不動 normal/metallic/roughness
        m.map = getTexture(freshBase, 'base')
        m.needsUpdate = true
      })
    })

    // 立即重繪一幀
    renderer?.render?.(scene, camera)
  } catch (e) {
    console.error(e)
    error.value = e?.message || '切換顏色失敗'
  }
})


onMounted(boot)

onBeforeUnmount(() => {
  window.removeEventListener('resize', onResize)
  controls?.dispose?.()

  if (pmremEnvRT) { pmremEnvRT.dispose(); pmremEnvRT = null }
  if (pmremGenerator) { pmremGenerator.dispose(); pmremGenerator = null }

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
  height: 520px;
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

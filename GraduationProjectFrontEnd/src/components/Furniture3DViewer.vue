<!-- 3D 家具展示組件 - 支援 PBR 材質 -->
<template>
  <div class="viewer-3d-container" ref="containerRef">
    <!-- 載入中 -->
    <div v-if="loading" class="loading-overlay">
      <div class="spinner-border text-light" role="status"></div>
      <p class="text-light mt-3">載入 3D 模型中...</p>
      <div class="progress" style="width: 200px">
        <div 
          class="progress-bar progress-bar-striped progress-bar-animated" 
          :style="{ width: loadingProgress + '%' }"
        ></div>
      </div>
      <small class="text-light">{{ loadingProgress }}%</small>
    </div>

    <!-- 錯誤訊息 -->
    <div v-if="error" class="error-overlay">
      <div class="alert alert-danger">
        <i class="bi bi-exclamation-triangle me-2"></i>
        {{ error }}
      </div>
    </div>

    <!-- Canvas -->
    <canvas ref="canvasRef" class="viewer-canvas"></canvas>

    <!-- 控制面板 -->
    <div class="controls-panel">
      <!-- 視角控制 -->
      <div class="btn-group mb-2" role="group">
        <button 
          class="btn btn-sm btn-light" 
          title="正面視角"
          @click="setView('front')"
        >
          <i class="bi bi-box-arrow-up-right"></i>
        </button>
        <button 
          class="btn btn-sm btn-light" 
          title="側面視角"
          @click="setView('side')"
        >
          <i class="bi bi-box-arrow-right"></i>
        </button>
        <button 
          class="btn btn-sm btn-light" 
          title="上方視角"
          @click="setView('top')"
        >
          <i class="bi bi-box-arrow-up"></i>
        </button>
        <button 
          class="btn btn-sm btn-light" 
          title="重置視角"
          @click="resetView"
        >
          <i class="bi bi-arrow-counterclockwise"></i>
        </button>
      </div>

      <!-- 環境光調整 -->
      <div class="mb-2">
        <label class="form-label small">環境光強度</label>
        <input 
          v-model.number="ambientIntensity" 
          type="range" 
          class="form-range form-range-sm" 
          min="0" 
          max="3" 
          step="0.1"
          @input="updateLighting"
        />
      </div>

      <!-- 自動旋轉 -->
      <div class="form-check form-switch mb-2">
        <input 
          v-model="autoRotate" 
          class="form-check-input" 
          type="checkbox" 
          id="autoRotateSwitch"
          @change="toggleAutoRotate"
        />
        <label class="form-check-label small" for="autoRotateSwitch">
          自動旋轉
        </label>
      </div>

      <!-- 顯示網格 -->
      <div class="form-check form-switch">
        <input 
          v-model="showGrid" 
          class="form-check-input" 
          type="checkbox" 
          id="gridSwitch"
          @change="toggleGrid"
        />
        <label class="form-check-label small" for="gridSwitch">
          顯示網格
        </label>
      </div>
    </div>

    <!-- 操作提示 -->
    <div class="hint-panel">
      <small class="text-muted">
        <i class="bi bi-mouse me-1"></i>左鍵旋轉 | 
        <i class="bi bi-mouse2 me-1"></i>右鍵平移 | 
        <i class="bi bi-mouse3 me-1"></i>滾輪縮放
      </small>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import * as THREE from 'three'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls'
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader'
import { RGBELoader } from 'three/examples/jsm/loaders/RGBELoader'

const props = defineProps({
  // 模型 URL
  f3dModelPath: {
    type: String,
    required: true
  },
  // PBR 貼圖
  textures: {
    type: Object,
    default: () => ({
      baseColor: null,    // 基礎顏色
      normal: null,       // 法線貼圖
      roughness: null,    // 粗糙度
      metalness: null,    // 金屬度
      ao: null           // 環境光遮蔽
    })
  },
  // HDR 環境貼圖
  envMapUrl: {
    type: String,
    default: null
  },
  // 自動旋轉速度
  autoRotateSpeed: {
    type: Number,
    default: 2.0
  }
})

const emit = defineEmits(['loaded', 'error'])

// Refs
const containerRef = ref(null)
const canvasRef = ref(null)

// Three.js 對象
let scene, camera, renderer, controls
let model = null
let grid = null
let ambientLight, directionalLight, hemisphereLight

// 狀態
const loading = ref(true)
const loadingProgress = ref(0)
const error = ref(null)
const ambientIntensity = ref(1.5)
const autoRotate = ref(false)
const showGrid = ref(true)

// 初始化 Three.js 場景
function initScene() {
  if (!containerRef.value || !canvasRef.value) return

  // 場景
  scene = new THREE.Scene()
  scene.background = new THREE.Color(0xf0f0f0)
  scene.fog = new THREE.Fog(0xf0f0f0, 10, 50)

  // 相機
  const width = containerRef.value.clientWidth
  const height = containerRef.value.clientHeight
  camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 1000)
  camera.position.set(4, 2, 4)

  // 渲染器
  renderer = new THREE.WebGLRenderer({
    canvas: canvasRef.value,
    antialias: true,
    alpha: true
  })
  renderer.setSize(width, height)
  renderer.setPixelRatio(window.devicePixelRatio)
  renderer.shadowMap.enabled = true
  renderer.shadowMap.type = THREE.PCFSoftShadowMap
  renderer.outputEncoding = THREE.sRGBEncoding
  renderer.toneMapping = THREE.ACESFilmicToneMapping
  renderer.toneMappingExposure = 1.0

  // 控制器
  controls = new OrbitControls(camera, renderer.domElement)
  controls.enableDamping = true
  controls.dampingFactor = 0.05
  controls.minDistance = 2
  controls.maxDistance = 10
  controls.maxPolarAngle = Math.PI / 2
  controls.autoRotate = autoRotate.value
  controls.autoRotateSpeed = props.autoRotateSpeed

  // 燈光
  setupLights()

  // 網格
  setupGrid()

  // 動畫循環
  animate()
}

// 設定燈光
function setupLights() {
  // 環境光
  ambientLight = new THREE.AmbientLight(0xffffff, ambientIntensity.value)
  scene.add(ambientLight)

  // 半球光（模擬天空和地面反射）
  hemisphereLight = new THREE.HemisphereLight(0xffffff, 0x444444, 0.6)
  hemisphereLight.position.set(0, 20, 0)
  scene.add(hemisphereLight)

  // 主光源
  directionalLight = new THREE.DirectionalLight(0xffffff, 1.0)
  directionalLight.position.set(5, 10, 7.5)
  directionalLight.castShadow = true
  directionalLight.shadow.camera.left = -10
  directionalLight.shadow.camera.right = 10
  directionalLight.shadow.camera.top = 10
  directionalLight.shadow.camera.bottom = -10
  directionalLight.shadow.mapSize.width = 2048
  directionalLight.shadow.mapSize.height = 2048
  scene.add(directionalLight)

  // 補光
  const fillLight = new THREE.DirectionalLight(0xffffff, 0.3)
  fillLight.position.set(-5, 5, -5)
  scene.add(fillLight)
}

// 設定網格
function setupGrid() {
  grid = new THREE.GridHelper(10, 10, 0x888888, 0xcccccc)
  grid.visible = showGrid.value
  scene.add(grid)
}

// 載入 HDR 環境貼圖
async function loadEnvironmentMap() {
  if (!props.envMapUrl) return

  try {
    const rgbeLoader = new RGBELoader()
    const texture = await rgbeLoader.loadAsync(props.envMapUrl)
    texture.mapping = THREE.EquirectangularReflectionMapping
    scene.environment = texture
    scene.background = texture
  } catch (err) {
    console.warn('⚠️ 載入 HDR 環境貼圖失敗:', err)
  }
}

// 載入 3D 模型
async function loadModel() {
  loading.value = true
  loadingProgress.value = 0
  error.value = null

  try {
    // 載入環境貼圖
    await loadEnvironmentMap()

    // 載入模型
    const loader = new GLTFLoader()
    
    const gltf = await new Promise((resolve, reject) => {
      loader.load(
        props.f3dModelPath,
        (gltf) => resolve(gltf),
        (xhr) => {
          loadingProgress.value = Math.round((xhr.loaded / xhr.total) * 100)
        },
        (err) => reject(err)
      )
    })

    model = gltf.scene

    // 應用 PBR 材質
    if (props.textures && Object.keys(props.textures).length > 0) {
      await applyPBRTextures(model)
    }

    // 啟用陰影
    model.traverse((child) => {
      if (child.isMesh) {
        child.castShadow = true
        child.receiveShadow = true
      }
    })

    // 居中模型
    const box = new THREE.Box3().setFromObject(model)
    const center = box.getCenter(new THREE.Vector3())
    model.position.sub(center)

    // 調整模型大小
    const size = box.getSize(new THREE.Vector3())
    const maxDim = Math.max(size.x, size.y, size.z)
    const scale = 2 / maxDim
    model.scale.multiplyScalar(scale)

    scene.add(model)

    loading.value = false
    emit('loaded', model)

  } catch (err) {
    console.error('❌ 載入模型失敗:', err)
    error.value = '無法載入 3D 模型，請稍後再試'
    loading.value = false
    emit('error', err)
  }
}

// 應用 PBR 貼圖
async function applyPBRTextures(model) {
  const textureLoader = new THREE.TextureLoader()
  const textures = {}

  // 載入所有貼圖
  const loadPromises = []
  
  if (props.textures.baseColor) {
    loadPromises.push(
      textureLoader.loadAsync(props.textures.baseColor)
        .then(tex => { textures.map = tex })
    )
  }
  
  if (props.textures.normal) {
    loadPromises.push(
      textureLoader.loadAsync(props.textures.normal)
        .then(tex => { textures.normalMap = tex })
    )
  }
  
  if (props.textures.roughness) {
    loadPromises.push(
      textureLoader.loadAsync(props.textures.roughness)
        .then(tex => { textures.roughnessMap = tex })
    )
  }
  
  if (props.textures.metalness) {
    loadPromises.push(
      textureLoader.loadAsync(props.textures.metalness)
        .then(tex => { textures.metalnessMap = tex })
    )
  }
  
  if (props.textures.ao) {
    loadPromises.push(
      textureLoader.loadAsync(props.textures.ao)
        .then(tex => { textures.aoMap = tex })
    )
  }

  await Promise.all(loadPromises)

  // 應用到所有材質
  model.traverse((child) => {
    if (child.isMesh) {
      const material = new THREE.MeshStandardMaterial({
        map: textures.map || null,
        normalMap: textures.normalMap || null,
        roughnessMap: textures.roughnessMap || null,
        metalnessMap: textures.metalnessMap || null,
        aoMap: textures.aoMap || null,
        roughness: 0.7,
        metalness: 0.3,
        envMapIntensity: 1.0
      })

      // 如果有 UV2，用於 AO 貼圖
      if (textures.aoMap && child.geometry.attributes.uv2) {
        material.aoMapIntensity = 1.0
      }

      child.material = material
    }
  })
}

// 動畫循環
function animate() {
  requestAnimationFrame(animate)
  controls.update()
  renderer.render(scene, camera)
}

// 設定視角
function setView(view) {
  if (!model) return

  const box = new THREE.Box3().setFromObject(model)
  const center = box.getCenter(new THREE.Vector3())
  const size = box.getSize(new THREE.Vector3())
  const maxDim = Math.max(size.x, size.y, size.z)
  const distance = maxDim * 2

  switch (view) {
    case 'front':
      camera.position.set(center.x, center.y, center.z + distance)
      break
    case 'side':
      camera.position.set(center.x + distance, center.y, center.z)
      break
    case 'top':
      camera.position.set(center.x, center.y + distance, center.z)
      break
  }

  controls.target.copy(center)
  controls.update()
}

// 重置視角
function resetView() {
  camera.position.set(4, 2, 4)
  controls.target.set(0, 0, 0)
  controls.update()
}

// 更新燈光
function updateLighting() {
  if (ambientLight) {
    ambientLight.intensity = ambientIntensity.value
  }
}

// 切換自動旋轉
function toggleAutoRotate() {
  if (controls) {
    controls.autoRotate = autoRotate.value
  }
}

// 切換網格
function toggleGrid() {
  if (grid) {
    grid.visible = showGrid.value
  }
}

// 處理視窗大小變化
function handleResize() {
  if (!containerRef.value || !camera || !renderer) return

  const width = containerRef.value.clientWidth
  const height = containerRef.value.clientHeight

  camera.aspect = width / height
  camera.updateProjectionMatrix()
  renderer.setSize(width, height)
}

// 清理
function dispose() {
  if (renderer) {
    renderer.dispose()
  }
  if (controls) {
    controls.dispose()
  }
  if (model) {
    scene.remove(model)
  }
}

// Watch 模型 URL 變化
watch(() => props.modelUrl, () => {
  if (model) {
    scene.remove(model)
    model = null
  }
  loadModel()
})

// 生命週期
onMounted(() => {
  initScene()
  loadModel()
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
  dispose()
})
</script>

<style scoped>
.viewer-3d-container {
  position: relative;
  width: 100%;
  height: 100%;
  min-height: 500px;
  background: linear-gradient(180deg, #e8e8e8 0%, #f5f5f5 100%);
  border-radius: 8px;
  overflow: hidden;
}

.viewer-canvas {
  display: block;
  width: 100%;
  height: 100%;
}

.loading-overlay,
.error-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: rgba(0, 0, 0, 0.7);
  z-index: 10;
}

.controls-panel {
  position: absolute;
  top: 20px;
  right: 20px;
  background: rgba(255, 255, 255, 0.95);
  padding: 15px;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  min-width: 200px;
}

.controls-panel .btn-group {
  width: 100%;
}

.controls-panel .btn {
  flex: 1;
}

.hint-panel {
  position: absolute;
  bottom: 20px;
  left: 50%;
  transform: translateX(-50%);
  background: rgba(255, 255, 255, 0.95);
  padding: 8px 16px;
  border-radius: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

@media (max-width: 768px) {
  .controls-panel {
    top: 10px;
    right: 10px;
    padding: 10px;
    min-width: 150px;
  }

  .hint-panel {
    display: none;
  }
}
</style>
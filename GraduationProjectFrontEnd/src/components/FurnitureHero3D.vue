<template>
  <div ref="wrap" class="three-wrap">
    <div ref="host" class="three-canvas"></div>
    <div v-if="loading" class="overlay">Loading 3D…</div>
    <div v-else-if="error" class="overlay error">{{ error }}</div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import * as THREE from 'three'
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js'
import { RoomEnvironment } from 'three/examples/jsm/environments/RoomEnvironment.js'

const props = defineProps({
  /** GLB 模型路徑：例如 /3D/shabbychic.glb */
  model: { type: String, required: true },
  /** 貼圖資料夾（預設 /3D/） */
  texDir: { type: String, default: '/3D/' },
  /** 是否自動旋轉 */
  autoRotate: { type: Boolean, default: true }
})

const host = ref(null)
const loading = ref(true)
const error = ref('')

let renderer, scene, camera, controls, mixer
let pmremGen, envRT, gltfScene, raf

function onResize() {
  if (!host.value) return
  const { clientWidth, clientHeight } = host.value
  camera.aspect = clientWidth / clientHeight
  camera.updateProjectionMatrix()
  renderer.setSize(clientWidth, clientHeight, false)
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2))
}

function fitCameraToObject(obj, offset = 1.4) {
  const box = new THREE.Box3().setFromObject(obj)
  const size = new THREE.Vector3()
  const center = new THREE.Vector3()
  box.getSize(size); box.getCenter(center)

  const maxDim = Math.max(size.x, size.y, size.z)
  const fov = camera.fov * (Math.PI / 180)
  let dist = Math.abs(maxDim / (2 * Math.tan(fov / 2))) * offset

  camera.near = dist / 100
  camera.far  = dist * 100
  camera.position.set(center.x + dist, center.y + dist * 0.25, center.z + dist)
  camera.updateProjectionMatrix()

  controls.target.copy(center)
  controls.update()
  return dist
}

function animate() {
  raf = requestAnimationFrame(animate)
  controls.update()
  if (mixer) mixer.update(1 / 60)
  renderer.render(scene, camera)
}

onMounted(async () => {
  try {
    // === Renderer ===
    renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true })
    renderer.outputColorSpace = THREE.SRGBColorSpace
    renderer.toneMapping = THREE.ACESFilmicToneMapping
    renderer.toneMappingExposure = 1.0
    renderer.physicallyCorrectLights = true
    host.value.appendChild(renderer.domElement)

    // === Scene & Camera ===
    scene = new THREE.Scene()
    camera = new THREE.PerspectiveCamera(50, 1, 0.01, 2000)
    scene.background = null // 透明背景

    // === Environment (IBL) ===
    pmremGen = new THREE.PMREMGenerator(renderer)
    envRT = pmremGen.fromScene(new RoomEnvironment(), 0.04)
    scene.environment = envRT.texture

    // === Light ===
    const hemi = new THREE.HemisphereLight(0xffffff, 0x444444, 0.6)
    scene.add(hemi)
    const dir = new THREE.DirectionalLight(0xffffff, 1.1)
    dir.position.set(3, 5, 2)
    scene.add(dir)

    // === Controls ===
    controls = new OrbitControls(camera, renderer.domElement)
    controls.enableDamping = true
    controls.dampingFactor = 0.06
    controls.enablePan = false
    controls.autoRotate = props.autoRotate
    controls.autoRotateSpeed = 5
    controls.enableZoom = false // ✅ 禁止放大縮小

    // 防止滑鼠滾輪造成頁面滾動
    renderer.domElement.addEventListener('wheel', e => e.preventDefault(), { passive: false })

    // === Load GLB ===
    const gltf = await new GLTFLoader().loadAsync(props.model)
    gltfScene = gltf.scene
    scene.add(gltfScene)

    // === Load external PBR textures ===
    const tl = new THREE.TextureLoader()
    const baseColor = tl.load(props.texDir + 'shabbychic_BaseColor.png')
    baseColor.colorSpace = THREE.SRGBColorSpace
    baseColor.flipY = false

    const normalMap = tl.load(props.texDir + 'shabbychic_Normal.png')
    normalMap.flipY = false

    const metalnessMap = tl.load(props.texDir + 'shabbychic_Metallic.png')
    metalnessMap.flipY = false

    const roughnessMap = tl.load(props.texDir + 'shabbychic_Roughness.png')
    roughnessMap.flipY = false

    const aoMap = tl.load(props.texDir + 'shabbychic_ao.png')
    aoMap.flipY = false

    // 可選高度貼圖（模型細分不足可略）
    const heightMap = tl.load(props.texDir + 'shabbychic_Height.png')
    heightMap.flipY = false

    const mat = new THREE.MeshStandardMaterial({
      map: baseColor,
      normalMap,
      metalnessMap,
      roughnessMap,
      aoMap,
      metalness: 1.0,
      roughness: 1.0,
      // 若模型細分足夠可開啟位移貼圖
      // displacementMap: heightMap,
      // displacementScale: 0.002,
    })

    gltfScene.traverse((o) => {
      if (o.isMesh) {
        const geom = o.geometry
        // AO 要求 uv2，若沒有就複製 uv
        if (geom && !geom.attributes.uv2 && geom.attributes.uv) {
          geom.setAttribute('uv2', new THREE.BufferAttribute(geom.attributes.uv.array, 2))
        }
        o.material = mat
        o.castShadow = false
        o.receiveShadow = false
      }
    })

    // 若 GLB 有動畫
    if (gltf.animations?.length) {
      mixer = new THREE.AnimationMixer(gltfScene)
      mixer.clipAction(gltf.animations[0]).play()
    }

    // === Camera Fit ===
    const dist = fitCameraToObject(gltfScene, 1.2)
    controls.minDistance = dist // ✅ 固定距離
    controls.maxDistance = dist // ✅ 固定距離
    controls.enableZoom = false // ✅ 禁止縮放

    onResize()
    window.addEventListener('resize', onResize)

    loading.value = false
    animate()
  } catch (e) {
    console.error(e)
    error.value = '載入 3D 或貼圖失敗，請檢查路徑與檔名'
    loading.value = false
  }
})

onBeforeUnmount(() => {
  cancelAnimationFrame(raf)
  window.removeEventListener('resize', onResize)
  controls?.dispose()
  if (renderer) { renderer.dispose(); renderer.forceContextLoss?.() }
  pmremGen?.dispose(); envRT?.dispose()

  if (gltfScene) {
    gltfScene.traverse((o) => {
      if (o.isMesh) {
        o.geometry?.dispose?.()
        const mats = Array.isArray(o.material) ? o.material : [o.material]
        mats.forEach((m) => {
          if (!m) return
          ;['map','normalMap','metalnessMap','roughnessMap','aoMap','displacementMap','emissiveMap']
            .forEach(k => m[k]?.dispose?.())
          m.dispose?.()
        })
      }
    })
  }
})
</script>

<style scoped>
.three-wrap { position: relative; width: 100%; height: 100%; }
.three-canvas { width: 100%; height: 100%; }
.overlay {
  position: absolute; inset: 0; display:grid; place-items:center;
  font-weight:600; background: rgba(255,255,255,0.8);
}
.overlay.error { color:#b00020; background: rgba(255,230,230,0.9); }
</style>

<template>
  <div ref="rootEl" class="position-relative overflow-hidden bg-white" style="min-height:320px;">
    <canvas ref="canvasEl" class="d-block w-100 h-100"></canvas>
    <div
      v-if="showOverlay"
      class="position-absolute start-0 top-0 m-2 px-2 py-1 small rounded text-white"
      style="background: rgba(0,0,0,.5); pointer-events: none;"
    >
      {{ overlayText }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref, watch } from 'vue'
import * as THREE from 'three'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js'
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js'
import { RGBELoader } from 'three/examples/jsm/loaders/RGBELoader.js'

interface TextureURLs {
  baseColor?: string | null
  normal?: string | null
  roughness?: string | null
  metalness?: string | null
}

const props = defineProps<{
  modelUrl?: string | null
  textures?: TextureURLs
  envmapUrl?: string | null
  exposure?: number
  metalness?: number
  roughness?: number
}>()

const rootEl = ref<HTMLDivElement | null>(null)
const canvasEl = ref<HTMLCanvasElement | null>(null)

let renderer: THREE.WebGLRenderer | null = null
let scene: THREE.Scene | null = null
let camera: THREE.PerspectiveCamera | null = null
let controls: OrbitControls | null = null
let frameId: number | null = null
let modelRoot: THREE.Object3D | null = null
let pmremGen: THREE.PMREMGenerator | null = null

const showOverlay = ref(true)
const overlayText = ref('')
const dpr = Math.min(2, window.devicePixelRatio || 1)

function setOverlay(text: string) {
  overlayText.value = text
  showOverlay.value = true
  window.clearTimeout((setOverlay as any)._t)
  ;(setOverlay as any)._t = window.setTimeout(() => (showOverlay.value = false), 1600)
}

function disposeObject(obj: THREE.Object3D) {
  obj.traverse(child => {
    const mesh = child as THREE.Mesh
    if ((mesh as any).isMesh) {
      mesh.geometry?.dispose()
      const mats = Array.isArray(mesh.material) ? mesh.material : [mesh.material]
      mats.forEach((m: any) => {
        if (!m) return
        Object.keys(m).forEach(k => {
          const tex = m[k]
          if (tex && tex.isTexture) tex.dispose()
        })
        m.dispose?.()
      })
    }
  })
}

function fitCameraToObject(obj: THREE.Object3D, cam: THREE.PerspectiveCamera, margin = 1.2) {
  const box = new THREE.Box3().setFromObject(obj)
  const size = new THREE.Vector3()
  const center = new THREE.Vector3()
  box.getSize(size); box.getCenter(center)
  const maxDim = Math.max(size.x, size.y, size.z)
  const fov = (cam.fov * Math.PI) / 180
  const dist = (maxDim / (2 * Math.tan(fov / 2))) * margin
  cam.position.copy(center.clone().add(new THREE.Vector3(dist, dist * 0.6, dist)))
  cam.near = Math.max(0.01, dist / 100); cam.far = dist * 100; cam.updateProjectionMatrix()
  controls?.target.copy(center); controls?.update()
}

async function loadEnvMap(url: string) {
  if (!renderer) return null
  pmremGen = new THREE.PMREMGenerator(renderer)
  pmremGen.compileEquirectangularShader()
  const hdr = await new RGBELoader().loadAsync(url)
  hdr.mapping = THREE.EquirectangularReflectionMapping
  const envRT = pmremGen.fromEquirectangular(hdr)
  hdr.dispose()
  return envRT.texture
}

async function applyPBRTextures(root: THREE.Object3D, urls: TextureURLs) {
  const loader = new THREE.TextureLoader()
  const load = (u?: string | null) => (u ? loader.load(u) : null)
  const tBase = load(urls.baseColor)
  const tNorm = load(urls.normal)
  const tRough = load(urls.roughness)
  const tMetal = load(urls.metalness)

  ;[tBase, tNorm, tRough, tMetal].forEach(t => {
    if (!t) return
    t.wrapS = t.wrapT = THREE.RepeatWrapping
    t.anisotropy = Math.min(8, renderer?.capabilities.getMaxAnisotropy?.() || 1)
    t.needsUpdate = true
  })

  root.traverse(child => {
    const mesh = child as THREE.Mesh
    if ((mesh as any).isMesh) {
      const matOld = mesh.material
      const mats = (Array.isArray(matOld) ? matOld : [matOld]).map((m: any) => {
        const std = new THREE.MeshStandardMaterial()
        std.color = new THREE.Color('#ffffff')
        std.map = tBase || m?.map || null
        std.normalMap = tNorm || m?.normalMap || null
        std.roughnessMap = tRough || m?.roughnessMap || null
        std.metalnessMap = tMetal || m?.metalnessMap || null
        std.roughness = props.roughness ?? m?.roughness ?? 1.0
        std.metalness = props.metalness ?? m?.metalness ?? 0.0
        std.envMapIntensity = 1.0
        std.side = THREE.FrontSide
        std.needsUpdate = true
        return std
      })
      mesh.material = mats.length === 1 ? mats[0] : mats
    }
  })
}

async function loadModel(url?: string | null) {
  if (!scene || !url) return null
  const gltf = await new GLTFLoader().loadAsync(url)
  return gltf.scene
}

function animate() {
  if (!renderer || !scene || !camera) return
  frameId = requestAnimationFrame(animate)
  controls?.update()
  renderer.render(scene, camera)
}

function resize() {
  if (!renderer || !camera || !rootEl.value) return
  const w = rootEl.value.clientWidth
  const h = rootEl.value.clientHeight || 1
  camera.aspect = Math.max(0.0001, w / h)
  camera.updateProjectionMatrix()
  renderer.setSize(w, h, false)
  renderer.setPixelRatio(dpr)
}

async function init() {
  if (!rootEl.value || !canvasEl.value) return
  scene = new THREE.Scene()
  scene.background = new THREE.Color('#f8fafc')

  camera = new THREE.PerspectiveCamera(50, 1, 0.01, 2000)
  camera.position.set(2.5, 1.5, 2.5)

  renderer = new THREE.WebGLRenderer({ canvas: canvasEl.value, antialias: true, alpha: false })
  renderer.outputColorSpace = THREE.SRGBColorSpace
  renderer.toneMapping = THREE.ACESFilmicToneMapping
  renderer.toneMappingExposure = props.exposure ?? 1.0

  const hemi = new THREE.HemisphereLight(0xffffff, 0x444444, 0.6)
  hemi.position.set(0, 1, 0); scene.add(hemi)
  const dir = new THREE.DirectionalLight(0xffffff, 1)
  dir.position.set(3, 5, 2); scene.add(dir)

  controls = new OrbitControls(camera, renderer.domElement)
  controls.enableDamping = true
  controls.dampingFactor = 0.07
  controls.rotateSpeed = 0.9
  controls.zoomSpeed = 0.9

  const grid = new THREE.GridHelper(10, 10, 0x999999, 0xdddddd)
  ;(grid.material as any).transparent = true
  ;(grid.material as any).opacity = 0.25
  grid.position.y = -0.001
  scene.add(grid)

  if (props.envmapUrl) {
    try {
      setOverlay('載入環境貼圖…')
      const env = await loadEnvMap(props.envmapUrl)
      if (env) { scene.environment = env; scene.background = null }
    } catch (e) { console.warn('HDR load failed', e) }
  }

  if (props.modelUrl) {
    try {
      setOverlay('載入模型…')
      modelRoot = await loadModel(props.modelUrl)
      if (modelRoot) { scene.add(modelRoot); fitCameraToObject(modelRoot, camera); setOverlay('模型已載入') }
    } catch (e) { console.error('Model load failed', e); setOverlay('模型載入失敗') }
  }

  if (modelRoot && props.textures) {
    try { setOverlay('套用材質…'); await applyPBRTextures(modelRoot, props.textures); setOverlay('材質已套用') }
    catch (e) { console.warn('Apply textures failed', e) }
  }

  window.addEventListener('resize', resize)
  renderer.domElement.addEventListener('dblclick', () => {
    if (modelRoot && camera) fitCameraToObject(modelRoot, camera, 1.15)
  })

  resize(); animate()
}

onMounted(init)

onBeforeUnmount(() => {
  if (frameId) cancelAnimationFrame(frameId)
  window.removeEventListener('resize', resize)
  controls?.dispose()
  if (modelRoot) disposeObject(modelRoot)
  if (pmremGen) pmremGen.dispose()
  renderer?.dispose()
  scene = null; camera = null; controls = null; renderer = null
})

watch(() => props.textures, async (t) => { if (modelRoot && t) await applyPBRTextures(modelRoot, t) }, { deep: true })
watch(() => props.envmapUrl, async (u) => {
  if (!scene || !renderer) return
  if (!u) { scene.environment = null; return }
  try { const env = await loadEnvMap(u); if (env) scene.environment = env } catch {}
})
watch(() => props.exposure, (v) => { if (renderer && typeof v === 'number') renderer.toneMappingExposure = v })
</script>

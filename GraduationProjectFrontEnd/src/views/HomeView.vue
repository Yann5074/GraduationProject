<script setup>
import { onMounted, onUnmounted, ref, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import AnnouncementBar from '@/components/AnnouncementBar.vue';
import FurnitureHero3D_ExternalPBR from '@/components/FurnitureHero3D.vue'
import AnalyticsArea from '@/components/AnalyticsArea.vue';

const router = useRouter()
const goShop = () => router.push({ name: 'ProductList' })

// === 3D下方圖片lightbox ===
const images = [
  { src: '../../asset/images/plate1.png', alt: '復古托盤展示圖-復古餐桌' },
  { src: '../../asset/images/plate2.png', alt: '復古托盤展示圖-明亮餐桌' },
  { src: '../../asset/images/plate3.png', alt: '復古托盤展示圖-Buffet' }
]

const showLightbox = ref(false)
const current = ref(0)

const openLightbox = (idx) => {
  current.value = idx
  showLightbox.value = true
}

const closeLightbox = () => { showLightbox.value = false }
const prev = () => { current.value = (current.value - 1 + images.length) % images.length }
const next = () => { current.value = (current.value + 1) % images.length }

const onKey = (e) => {
  if (!showLightbox.value) return
  if (e.key === 'Escape') closeLightbox()
  if (e.key === 'ArrowLeft') prev()
  if (e.key === 'ArrowRight') next()
}

// === DOM refs ===
const sectionEl = ref(null) // 影片外層 <section>
const wrapEl = ref(null)    // 影片容器（做 scale / sticky）
const videoEl = ref(null)   // <video>
const textEl = ref(null)    // ← 新增：文字容器（上下移動的目標）

// === 平滑參數（可調）===
const SMOOTH = 0.18  // 0.06~0.18：愈小愈柔順、愈大反應愈跟手
let currentY = 0     // 畫面上真正套用的位移
let targetY = 0      // 計算得到的目標位移
let animRAF = null
let animating = false

// === 播放控制 ===
let observer

// === 滾動放大（scroll → scale）===
let ticking = false
let distance = 500 // 從開始位置到「撐滿」的滾動距離；會在 onResize 依視窗高重算

// === 文字上下停靠（scroll → translateY）===
let startY = 0 // 上方停止位移
let endY = 0   // 下方停止位移

function clamp(n, min, max) { return Math.max(min, Math.min(n, max)) }
function lerp(a, b, t) { return a + (b - a) * t }

function step() {
  // 慣性插值
  currentY += (targetY - currentY) * SMOOTH
  if (textEl.value?.style) {
    textEl.value.style.top = `${currentY}px`;       // ← 直接定位 top
    textEl.value.style.transform = 'translateX(-50%)';
  }
  if (Math.abs(targetY - currentY) > 0.5) {
    animating = true;
    animRAF = requestAnimationFrame(step);
  } else {
    animating = false;
  }
}

function updateScale() {
  ticking = false
  if (!sectionEl.value || !wrapEl.value) return

  const rect = sectionEl.value.getBoundingClientRect()
  const vh = window.innerHeight || document.documentElement.clientHeight

  // ==== A) Scale（你原本的放大效果）====
  const trigger = vh * 0.33
  const current = trigger - rect.top
  const progress = clamp(current / distance, 0, 1)
  const scale = 0.75 + 0.25 * progress
  wrapEl.value.style.transform = `scale(${scale})`
  wrapEl.value.style.transformOrigin = 'center top'

  // ==== B) Caption 依影片上/下緣移動 ====
if (textEl.value && wrapEl.value) {
  const secRect  = sectionEl.value.getBoundingClientRect();
  const wrapRect = wrapEl.value.getBoundingClientRect();
  const vh = window.innerHeight || document.documentElement.clientHeight;

  // 區塊內的可捲動量
  const sectionHeight = sectionEl.value.offsetHeight;
  const maxScroll = Math.max(1, sectionHeight - vh);
  const t = clamp(-secRect.top, 0, maxScroll);
  const ratio = t / maxScroll; // 0 → 1

  // 把 wrap 的 top/bottom 轉換成「以 section 頂端為 0」的座標
  const wrapTopInSection    = wrapRect.top    - secRect.top;
  const wrapBottomInSection = wrapRect.bottom - secRect.top;

  const capH = textEl.value.offsetHeight || 0;
  const GAP  = 12; // 與影片的間距

  // 文字「上停靠」= 影片頂端之上（預留 caption 高度 + 間距）
  const topStop    = wrapTopInSection - capH - GAP;
  // 文字「下停靠」= 影片底端之下（留間距）
  const bottomStop = wrapBottomInSection + GAP;

  // 在兩個停靠點之間插值
  const y = lerp(topStop, bottomStop, ratio);

  // 慣性：把 y 當作目標 top
  targetY = y;
  if (!animating) {
    animating = true;
    cancelAnimationFrame(animRAF);
    animRAF = requestAnimationFrame(step);
  }
}
}

function onScroll() {
  if (!ticking) {
    ticking = true
    requestAnimationFrame(updateScale)
  }
}

async function onResize() {
  // 依視窗高度調整放大距離，視覺更自然
  const vh = window.innerHeight || 800
  distance = Math.round(vh * 0.75)

  // 需要文字尺寸，保險起見等一次排版
  await nextTick()

  if (textEl.value) {
    const textH = textEl.value.offsetHeight || 0
	  // 建議位移量（桌機可再放大點、手機可再縮小）
    const offsetUp   = vh * 0.40;   // 往上 40% 視窗高
    const offsetDown = vh * 0.12;   // 往下 12% 視窗高

    // 🎯 直接用 offset 當上下界（以中心為 0）
    startY = -offsetUp;
    endY   =  offsetDown;

    // 依位移量設定 section 高度，剛好容納滾動空間
    if (sectionEl.value) {
      const extra = Math.max(0, endY - startY); // 需要的位移量
      sectionEl.value.style.minHeight = `${Math.round(vh + extra)}px`;
      sectionEl.value.style.paddingBottom = '0';
    }
  }

  updateScale()
}

onMounted(async () => {
  // IntersectionObserver：可見時播放，不可見暫停
  observer = new IntersectionObserver(
    (entries) => {
      const entry = entries[0]
      if (!videoEl.value) return
      if (entry.isIntersecting) {
        videoEl.value.play().catch(() => {})
      } else {
        videoEl.value.pause()
      }
    },
    { threshold: 0.35 }
  )
  if (videoEl.value) observer.observe(videoEl.value)

  await onResize()
  window.addEventListener('scroll', onScroll, { passive: true })
  window.addEventListener('resize', onResize)
  window.addEventListener('keydown', onKey)
})

onUnmounted(() => {
  if (observer && videoEl.value) observer.unobserve(videoEl.value)
  window.removeEventListener('scroll', onScroll)
  window.removeEventListener('resize', onResize)
  window.removeEventListener('keydown', onKey)
})
</script>

<template>
  <!-- Start Hero Section -->
	<section class="hero-neo">
 		<!-- 用影片；若沒有影片就改成背景圖 -->
    	<video class="hero-video" autoplay muted loop playsinline preload="auto" >
      		<source src="/asset/videos/hero2.mp4" type="video/mp4" />
    	</video>

    	<div class="hero-overlay"></div>

    	<div class="container d-flex flex-column justify-content-center align-items-start h-100">
      		<h1 class="display-3 fw-bold text-white lh-1 mb-3 ">Make Interiors <br><span class="grad">Feel Alive</span>
      		</h1>
      		<p class="lead text-white-50 mb-4">即時渲染展示你的家居靈感
      		</p>
      		<div class="d-flex gap-3">
        	<button class="btn btn-neo btn-lg px-4" @click="goShop">開始選購</button>
        	<RouterLink to="/design" class="btn btn-outline-light btn-lg px-4">看看靈感</RouterLink>
      		</div>
    	</div>

    		<!-- 漂浮光暈 -->
    		<div class="glow glow-a"></div>
    		<div class="glow glow-b"></div>
  </section>
	<!-- End Hero Section -->

  <!-- Start Announcement Section -->
  <section class="announcement py-3 bg-transparent text-dark text-center">
    <div class="container">
	  <AnnouncementBar />
    </div>
  </section>
  <!-- End Announcement Section -->

  <!-- Start 3D Section -->
  <section class="hero-3d">
  <div class="hero-inner container">
    <!-- 左邊文字 -->
    <div class="hero-copy">
      <h3 class="hero-title">
        Shabby Chic<br/>復古雙層餐盤座
      </h3>
      <p class="hero-desc">
		  細緻的仿舊工藝，讓時間在金屬表面留下溫柔的痕跡。無論是午後茶點、香氛蠟燭，或是乾燥花飾，這份優雅的層次感，都能為空間增添柔和的浪漫氣息。
      </p>

	    <!-- 之後放換顏色區 -->
      <div class="hero-cta">
        <!-- <RouterLink to="/products" class="btn btn-light btn-lg me-2">逛逛商品</RouterLink>
        <RouterLink to="/design" class="btn btn-light btn-lg me-2">看佈置靈感</RouterLink> -->
      </div>

      <!-- 小亮點/賣點 -->
      <ul class="hero-bullets">
        <li>真實 PBR 材質</li>
        <li>支援拖曳旋轉預覽</li>
        <li>高對比易讀設計</li>
      </ul>

      <!-- 圖片展示區（可點擊開啟 Lightbox） -->
      <div class="hero-gallery" role="list">
          <button
            v-for="(img, i) in images"
            :key="img.src"
            class="thumb"
            @click="openLightbox(i)"
            :aria-label="`開啟 ${img.alt}`"
          >
            <img :src="img.src" :alt="img.alt" />
          </button>
      </div>
    </div>

    <!-- 右邊 3D -->
    <div class="hero-visual">
      <FurnitureHero3D_ExternalPBR
        model="/3D/shabbychic.glb"
        tex-dir="/3D/"
        :auto-rotate="true"
      />
    </div>
  </div>

    <!-- Lightbox Overlay -->
    <div
      v-if="showLightbox"
      class="lightbox"
      role="dialog"
      aria-modal="true"
      @click.self="closeLightbox"
    >
      <button class="lb-close" @click="closeLightbox" aria-label="關閉預覽">✕</button>

      <button class="lb-nav lb-prev" @click.stop="prev" aria-label="上一張">‹</button>

      <figure class="lb-figure">
        <img :src="images[current].src" :alt="images[current].alt" />
        <figcaption class="lb-caption">{{ images[current].alt }}（{{ current + 1 }} / {{ images.length }}）</figcaption>
      </figure>

      <button class="lb-nav lb-next" @click.stop="next" aria-label="下一張">›</button>
    </div>
  </section>
  <!-- End 3D Section -->

  <!-- Start video Section -->
   <section ref="sectionEl" class="video-section">
    <!-- 會隨滾動上下移動，並在上下方各自停止 -->
    <div ref="textEl" class="caption">
      <h4 class="cap-title">感受每個角度的細節</h4>
      <p class="cap-desc">
        在 3D 空間中自由旋轉、放大與探索，體驗設計線條與材質紋理的完美結合。
      </p>
    </div>

    <div ref="wrapEl" class="video-wrap">
      <video
        ref="videoEl"
        class="showcase-video"
        src="/asset/videos/home2.mp4"
        autoplay
        muted
        playsinline
        loop
        preload="metadata"
      ></video>
    </div>
  </section>
	<!-- End video Section -->

  <!-- Start products Section -->
	<section class="products-section">
   <div class="container">
     <div class="products-header">
      <h3 class="products-title">年度熱銷商品排行 🔥</h3>
      <p class="products-sub">探索最受歡迎的設計作品，靈感不容錯過。</p>
     </div>

     <div class="products-area">
      <AnalyticsArea />
     </div>
   </div>
  </section>
	<!-- End products Section -->

  <!-- Start Blog Section -->
		<div class="blog-section">
			<div class="container">
				<div class="row mb-5">
					<div class="col-md-6">
						<h2 class="section-title">布置靈感</h2>
					</div>
					<div class="col-md-6 text-start text-md-end">
						<a href="#" class="more">觀看全部</a>
					</div>
				</div>

				<div class="row">

					<div class="col-12 col-sm-6 col-md-4 mb-4 mb-md-0">
						<div class="post-entry">
							<a href="#" class="post-thumbnail"><img src="../assets/images/post-1.jpg" alt="Image" class="img-fluid"></a>
							<div class="post-content-entry">
								<h3><a href="#">開始布置專屬於自己的空間</a></h3>
								<div class="meta">
									<span>從簡約開始<br/>讓空間保留呼吸的餘地，家的溫度就從這份純粹誕生</span>
								</div>
							</div>
						</div>
					</div>

					<div class="col-12 col-sm-6 col-md-4 mb-4 mb-md-0">
						<div class="post-entry">
							<a href="#" class="post-thumbnail"><img src="../assets/images/post-2.jpg" alt="Image" class="img-fluid"></a>
							<div class="post-content-entry">
								<h3><a href="#">光影裡的午後寧靜</a></h3>
								<div class="meta">
									<span>一束自然光<br/>時間在柔軟的節奏裡慢慢延伸</span>
								</div>
							</div>
						</div>
					</div>

					<div class="col-12 col-sm-6 col-md-4 mb-4 mb-md-0">
						<div class="post-entry">
							<a href="#" class="post-thumbnail"><img src="../assets/images/post-3.jpg" alt="Image" class="img-fluid"></a>
							<div class="post-content-entry">
								<h3><a href="#">小空間的設計魔法</a></h3>
								<div class="meta">
									<span>善用比例與線條<br/>創造出俐落又舒心的生活氛圍</span>
								</div>
							</div>
						</div>
					</div>

				</div>
			</div>
		</div>
	<!-- End Blog Section -->
</template>

<style scoped>
/* ======= HERO ======= */
.hero-neo{
  position: relative; height: 92vh; min-height: 560px; overflow: hidden;
  background: #0f1113;
}
.hero-video{
  position:absolute; inset:0; width:100%; height:100%; object-fit:cover; filter: saturate(1.1) contrast(1.05);
}
.hero-overlay{
  position:absolute; inset:0; background: radial-gradient(1200px 600px at 20% 20%, rgba(59,93,80,.50), rgba(15,17,19,.55) 50%, rgba(15,17,19,.85) 100%);
}
.hero-neo .container{ position:relative; z-index:2; }
.grad{ background: linear-gradient(90deg,#8ef7c2,#74d6ff,#b7a6ff); -webkit-background-clip:text; background-clip:text; }

/* 漂浮光暈 */
.glow{ position:absolute; filter: blur(40px); opacity:.45; z-index:1; }
.glow-a{ width:360px; height:360px; left: -80px; top: 10%; background: radial-gradient(circle,#7fe2c9,transparent 60%); }
.glow-b{ width:420px; height:420px; right: -120px; bottom: -60px; background: radial-gradient(circle,#8aa3ff,transparent 60%); }

/* ======= 按鈕 ======= */
.btn-neo{
  --c:#48d2a0;
  background: linear-gradient(180deg, var(--c), #2ab785);
  color:#0b0d0e; border: none; border-radius: .75rem;
  box-shadow: 0 10px 24px rgba(72,210,160,.35);
}
.btn-neo:hover{ filter: brightness(1.05); transform: translateY(-1px); }
.btn-outline-light{ border-radius: .75rem; }

/* ======= 3D ======= */
.hero-3d {
  /* 背景色可自行微調：深色＋淡漸層玻璃感 */
  --ring: rgba(255,255,255,.08);

  background:
    radial-gradient(120% 140% at 85% 10%, var(--bg2) 0%, var(--bg1) 60%),
    linear-gradient(180deg, rgba(255,255,255,.02), rgba(255,255,255,0));
  color: #000000;
}

.hero-inner {
  display: grid;
  grid-template-columns: 1.05fr 1.35fr;
  /* align-items: center; */
  gap: clamp(16px, 3vw, 40px);
  padding: clamp(28px, 5vw, 80px) 0;
  min-height: clamp(520px, 72vh, 860px);
}

.hero-copy {
  max-width: 620px;
}

.hero-title {
  font-size: clamp(28px, 3.2vw, 52px);
  line-height: 1.1;
  letter-spacing: .2px;
  margin: 0 0 .9rem;
  font-weight: 800;
}

.hero-desc {
  font-size: clamp(15px, 1.4vw, 18px);
  color: rgba(95, 68, 68, 0.85);
  margin: 0 0 1.25rem;
}

.hero-cta .btn {
  border-radius: 16px;
  padding: .7rem 1.1rem;
  font-weight: 700;
  box-shadow: 0 10px 30px rgba(0,0,0,.15);
}

.hero-cta .btn-outline-light:hover {
  color: #1b1e23;
}

.hero-bullets {
  display: flex;
  gap: 14px;
  flex-wrap: wrap;
  margin: 1rem 0 0;
  padding: 0;
  list-style: none;
}

.hero-bullets li {
  border: 1px solid var(--ring);
  padding: .45rem .7rem;
  border-radius: 999px;
  font-size: 14px;
  color: #8B4513;
  backdrop-filter: blur(6px);
  background: #fff2de;
  box-shadow: 0 0 6px 1px rgba(0, 0, 0, 0.1);
}

/* Lightbox */
.hero-gallery {
  display: flex;
  gap: 1rem;
  margin-top: 1.5rem;
  flex-wrap: wrap;
}
.thumb {
  padding: 0;
  border: none;
  background: transparent;
  cursor: zoom-in;
  border-radius: 12px;
  box-shadow: 0 2px 6px rgba(0,0,0,.15);
  overflow: hidden;
  transition: transform .25s ease, box-shadow .25s ease;
}
.thumb img {
  width: 110px;
  height: 110px;
  object-fit: cover;
  display: block;
}
.thumb:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0,0,0,.2);
}

.lightbox {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,.7);
  display: grid;
  grid-template-columns: auto 1fr auto;
  grid-template-rows: auto 1fr auto;
  align-items: center;
  justify-items: center;
  z-index: 9999;
  padding: 2rem;
  backdrop-filter: blur(2px);
}
.lb-figure {
  max-width: min(90vw, 1100px);
  max-height: 80vh;
  display: grid;
  gap: .75rem;
  justify-items: center;
}
.lb-figure img {
  max-width: 100%;
  max-height: 70vh;
  object-fit: contain;
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(0,0,0,.35);
}
.lb-caption {
  color: #f2f2f2;
  font-size: .95rem;
  text-align: center;
  opacity: .9;
}
.lb-close {
  position: absolute;
  top: 14px;
  right: 16px;
  width: 40px;
  height: 40px;
  border: none;
  border-radius: 999px;
  background: rgba(0,0,0,.55);
  color: #fff;
  font-size: 20px;
  line-height: 40px;
  cursor: pointer;
}
.lb-nav {
  width: 48px;
  height: 48px;
  border: none;
  border-radius: 999px;
  background: rgba(0,0,0,.5);
  color: #fff;
  font-size: 28px;
  cursor: pointer;
  display: grid;
  place-items: center;
  transition: background .2s ease;
}
.lb-prev { justify-self: start; margin-right: 1rem; }
.lb-next { justify-self: end; margin-left: 1rem; }
.lb-nav:hover, .lb-close:hover { background: rgba(0,0,0,.7); }

/* 3D 區塊容器：有圓角、淡邊框與玻璃感 */
.hero-visual {
  position: relative;
  width: 100%;
  height: min(72vh, 680px);
  overflow: hidden;
}

/* 讓你的 3D 組件撐滿右側容器 */
.hero-visual .three-wrap,
.hero-visual .three-canvas {
  width: 100%;
  height: 100%;
}

/* 響應式：窄螢幕改成上下堆疊，文字在上、3D 在下 */
@media (max-width: 992px) {
  .hero-inner {
    grid-template-columns: 1fr;
    gap: 20px;
    padding: clamp(20px, 6vw, 36px) 0;
  }

  .hero-visual {
    height: min(56vh, 520px);
  }

  .hero-cta .btn {
    width: 100%;
    margin-bottom: .5rem;
  }
}
@media (max-width: 768px) {
  .thumb img { width: 90px; height: 90px; }
  .lightbox { padding: 1rem; }
}

/* 字體 */
.shadows-into-light-regular {
  font-family: "Shadows Into Light", cursive;
  font-weight: 400;
  font-style: normal;
}

/* 影片 */
.video-section {
  padding: 7rem 0 3rem;
  position: relative;
  margin-bottom: 0;
  /*isolation: isolate;*/
  /* 讓 sticky/滾動有空間：可依版面微調 200~260vh */
  height: auto;
  overflow: clip;
}

/* 外框容器：用來維持比例，但背景改成透明 */
.video-wrap {
  /* 使用容器維持 16:9，影片再 cover 住 */
  aspect-ratio: 16 / 9;
  position: relative;
  width: 100%;
  overflow: hidden;
  z-index: 1;
  background: transparent !important;
  transform-origin: center top;
}

.video-wrap.scroll-scale {
  overflow: hidden;
  position: relative;
}

/* 讓外層容器吃到 scale，避免影響排版流 */
.scroll-scale {
  transform-origin: center top; /* 從上緣長大，看起來更順 */
  will-change: transform;
  transition: border-radius 0.2s linear; /* 圓角小幅過渡 */
}

/* 影片本體：不加外框、不留邊 */
.showcase-video {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
  /* z-index: 0; */
  position: relative;
  transform: translateY(0.5px);
}

/* 影片下方字體 */
.caption {
  position: absolute;
  left: 50%;
  transform: translate(-50%); /* JS 會覆寫 Y 偏移 */
  /* z-index: 2; */
  text-align: center;
  color: #000000;
  padding: 0 1rem;
  max-width: 960px;
  will-change: transform;
  transition: transform 0.08s linear;
  text-shadow: 0 2px 12px rgba(0,0,0,.45); /* 提升可讀性 */
}
.cap-title { position: relative; overflow: clip; font-size: clamp(20px, 3.2vw, 36px); margin: 0 0 .5rem; }
.cap-desc  { position: relative; z-index: 1; font-size: clamp(14px, 2vw, 18px); line-height: 1.6; margin: 0; text-shadow: 0 2px 12px rgba(0,0,0,.45); }

/* 尊重使用者「降低動態效果」偏好 */
@media (prefers-reduced-motion: reduce) {
  .scroll-scale { transform: none !important; transition: none !important; }
}

/* ===== Product 區塊 ===== */
.products-section {
  position: relative;
  background: #fafafa;
  padding: 5rem 1rem 6rem;
  margin-top: -2px; /* 緊密銜接 video 區塊 */
  border-top: 1px solid rgba(0,0,0,0.05);
  overflow: hidden;
}

.products-section::before {
  content: "";
  position: absolute;
  top: 0;
  left: 50%;
  transform: translateX(-50%);
  width: 120px;
  height: 4px;
  background: linear-gradient(90deg, #ff8a00, #ff4d4d);
  border-radius: 4px;
}

.products-header {
  text-align: center;
  margin-bottom: 3rem;
  animation: fadeInUp 0.8s ease forwards;
}

.products-title {
  font-size: 2rem;
  font-weight: 700;
  color: #222;
  margin-bottom: 0.5rem;
}

.products-sub {
  color: #666;
  font-size: 1rem;
  line-height: 1.6;
}

.products-area {
  max-width: 1100px;
  margin: 0 auto;
  animation: fadeIn 1s ease forwards;
}

/* 動畫 */
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(30px); }
  to { opacity: 1; transform: translateY(0); }
}

/* 手機版 */
@media (max-width: 768px) {
  .products-section {
    padding: 3rem 1rem 4rem;
  }
  .products-title {
    font-size: 1.6rem;
  }
}
</style>
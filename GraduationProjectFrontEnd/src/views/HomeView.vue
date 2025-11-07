<script setup>
import { onMounted, onUnmounted, ref, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import AnnouncementBar from '@/components/AnnouncementBar.vue';
import FurnitureHero3D_ExternalPBR from '@/components/FurnitureHero3D.vue'

const router = useRouter()
const goShop = () => router.push({ name: 'ProductList' })

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
  textEl.value?.style && (textEl.value.style.transform =
    `translate(-50%, calc(-50% + ${currentY}px))`)

  // 距離仍大就持續下一幀
  if (Math.abs(targetY - currentY) > 0.5) {
    animating = true
    animRAF = requestAnimationFrame(step)
  } else {
    animating = false
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

  // ==== B) Caption TranslateY（平滑移動）====
  if (textEl.value) {
    // 以整個 section 的可滾動量來算進度：
    const sectionHeight = sectionEl.value.offsetHeight
    const maxScroll = Math.max(1, sectionHeight - vh)
    const t = clamp(-rect.top, 0, maxScroll)
    const ratio = t / maxScroll

    // 在 onResize 中會依視窗與文字高度算好 startY / endY
    const y = lerp(startY, endY, ratio)

    // 🎯 改成：設定目標位移，讓動畫函式慢慢追上
    targetY = y
    if (!animating) {
      animating = true
      cancelAnimationFrame(animRAF)
      animRAF = requestAnimationFrame(step)
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
    // 停靠距離：可調 0.30～0.45 之間，數值小→更靠近上下邊
    const k = 0.35
	const offsetUp = vh * 1   // 往上移動距離
	const offsetDown = vh * 0.35 // 往下移動距離
    startY = -(offsetUp + textH * 0.5) // 上方停靠
	endY   =  (offsetDown + textH * 0.5) // 下方停靠
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
})

onUnmounted(() => {
  if (observer && videoEl.value) observer.unobserve(videoEl.value)
  window.removeEventListener('scroll', onScroll)
  window.removeEventListener('resize', onResize)
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
</section>

  <!-- End 3D Section -->

  <!-- Start video Section -->
   <section ref="sectionEl" class="video-section">
	<!-- <h3 class="text-center mb-5">EAGO 椅凳 — 現代優雅的完美平衡</h3> -->
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

  <!-- Start We Help Section -->
		<div class="we-help-section">
			<div class="container">
				<div class="row justify-content-between">
					<div class="col-lg-7 mb-5 mb-lg-0">
						<div class="imgs-grid">
							<div class="grid grid-1"><img src="../assets/images/img-grid-1.jpg" alt="Untree.co"></div>
							<div class="grid grid-2"><img src="../assets/images/img-grid-2.jpg" alt="Untree.co"></div>
							<div class="grid grid-3"><img src="../assets/images/img-grid-3.jpg" alt="Untree.co"></div>
						</div>
					</div>
					<div class="col-lg-5 ps-lg-5">
						<h2 class="section-title mb-4">We Help You Make Modern Interior Design</h2>
						<p>Donec facilisis quam ut purus rutrum lobortis. Donec vitae odio quis nisl dapibus malesuada. Nullam ac aliquet velit. Aliquam vulputate velit imperdiet dolor tempor tristique. Pellentesque habitant morbi tristique senectus et netus et malesuada</p>

						<ul class="list-unstyled custom-list my-4">
							<li>Donec vitae odio quis nisl dapibus malesuada</li>
							<li>Donec vitae odio quis nisl dapibus malesuada</li>
							<li>Donec vitae odio quis nisl dapibus malesuada</li>
							<li>Donec vitae odio quis nisl dapibus malesuada</li>
						</ul>
						<p><a herf="#" class="btn">Explore</a></p>
					</div>
				</div>
			</div>
		</div>
	<!-- End We Help Section -->

  <!-- Start Popular Product -->
		<div class="popular-product">
			<div class="container">
				<div class="row">

					<div class="col-12 col-md-6 col-lg-4 mb-4 mb-lg-0">
						<div class="product-item-sm d-flex">
							<div class="thumbnail">
								<img src="../assets/images/product-1.png" alt="Image" class="img-fluid">
							</div>
							<div class="pt-3">
								<h3>Nordic Chair</h3>
								<p>Donec facilisis quam ut purus rutrum lobortis. Donec vitae odio </p>
								<p><a href="#">Read More</a></p>
							</div>
						</div>
					</div>

					<div class="col-12 col-md-6 col-lg-4 mb-4 mb-lg-0">
						<div class="product-item-sm d-flex">
							<div class="thumbnail">
								<img src="../assets/images/product-2.png" alt="Image" class="img-fluid">
							</div>
							<div class="pt-3">
								<h3>Kruzo Aero Chair</h3>
								<p>Donec facilisis quam ut purus rutrum lobortis. Donec vitae odio </p>
								<p><a href="#">Read More</a></p>
							</div>
						</div>
					</div>

					<div class="col-12 col-md-6 col-lg-4 mb-4 mb-lg-0">
						<div class="product-item-sm d-flex">
							<div class="thumbnail">
								<img src="../assets/images/product-3.png" alt="Image" class="img-fluid">
							</div>
							<div class="pt-3">
								<h3>Ergonomic Chair</h3>
								<p>Donec facilisis quam ut purus rutrum lobortis. Donec vitae odio </p>
								<p><a href="#">Read More</a></p>
							</div>
						</div>
					</div>

				</div>
			</div>
		</div>
	<!-- End Popular Product -->

  <!-- Start Testimonial Slider -->
		<div class="testimonial-section">
			<div class="container">
				<div class="row">
					<div class="col-lg-7 mx-auto text-center">
						<h2 class="section-title">Testimonials</h2>
					</div>
				</div>

				<div class="row justify-content-center">
					<div class="col-lg-12">
						<div class="testimonial-slider-wrap text-center">

							<div id="testimonial-nav">
								<span class="prev" data-controls="prev"><span class="fa fa-chevron-left"></span></span>
								<span class="next" data-controls="next"><span class="fa fa-chevron-right"></span></span>
							</div>

							<div class="testimonial-slider">
								
								<div class="item">
									<div class="row justify-content-center">
										<div class="col-lg-8 mx-auto">

											<div class="testimonial-block text-center">
												<blockquote class="mb-5">
													<p>&ldquo;Donec facilisis quam ut purus rutrum lobortis. Donec vitae odio quis nisl dapibus malesuada. Nullam ac aliquet velit. Aliquam vulputate velit imperdiet dolor tempor tristique. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Integer convallis volutpat dui quis scelerisque.&rdquo;</p>
												</blockquote>

												<div class="author-info">
													<div class="author-pic">
														<img src="../assets/images/person-1.png" alt="Maria Jones" class="img-fluid">
													</div>
													<h3 class="font-weight-bold">Maria Jones</h3>
													<span class="position d-block mb-3">CEO, Co-Founder, XYZ Inc.</span>
												</div>
											</div>

										</div>
									</div>
								</div> 
								<!-- END item -->

								<div class="item">
									<div class="row justify-content-center">
										<div class="col-lg-8 mx-auto">

											<div class="testimonial-block text-center">
												<blockquote class="mb-5">
													<p>&ldquo;Donec facilisis quam ut purus rutrum lobortis. Donec vitae odio quis nisl dapibus malesuada. Nullam ac aliquet velit. Aliquam vulputate velit imperdiet dolor tempor tristique. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Integer convallis volutpat dui quis scelerisque.&rdquo;</p>
												</blockquote>

												<div class="author-info">
													<div class="author-pic">
														<img src="../assets/images/person-1.png" alt="Maria Jones" class="img-fluid">
													</div>
													<h3 class="font-weight-bold">Maria Jones</h3>
													<span class="position d-block mb-3">CEO, Co-Founder, XYZ Inc.</span>
												</div>
											</div>

										</div>
									</div>
								</div> 
								<!-- END item -->

								<div class="item">
									<div class="row justify-content-center">
										<div class="col-lg-8 mx-auto">

											<div class="testimonial-block text-center">
												<blockquote class="mb-5">
													<p>&ldquo;Donec facilisis quam ut purus rutrum lobortis. Donec vitae odio quis nisl dapibus malesuada. Nullam ac aliquet velit. Aliquam vulputate velit imperdiet dolor tempor tristique. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Integer convallis volutpat dui quis scelerisque.&rdquo;</p>
												</blockquote>

												<div class="author-info">
													<div class="author-pic">
														<img src="../public/asset/images/person-1.png" alt="Maria Jones" class="img-fluid">
													</div>
													<h3 class="font-weight-bold">Maria Jones</h3>
													<span class="position d-block mb-3">CEO, Co-Founder, XYZ Inc.</span>
												</div>
											</div>

										</div>
									</div>
								</div> 
								<!-- END item -->

							</div>

						</div>
					</div>
				</div>
			</div>
		</div>
	<!-- End Testimonial Slider -->

  <!-- Start Blog Section -->
		<div class="blog-section">
			<div class="container">
				<div class="row mb-5">
					<div class="col-md-6">
						<h2 class="section-title">Recent Blog</h2>
					</div>
					<div class="col-md-6 text-start text-md-end">
						<a href="#" class="more">View All Posts</a>
					</div>
				</div>

				<div class="row">

					<div class="col-12 col-sm-6 col-md-4 mb-4 mb-md-0">
						<div class="post-entry">
							<a href="#" class="post-thumbnail"><img src="../assets/images/post-1.jpg" alt="Image" class="img-fluid"></a>
							<div class="post-content-entry">
								<h3><a href="#">First Time Home Owner Ideas</a></h3>
								<div class="meta">
									<span>by <a href="#">Kristin Watson</a></span> <span>on <a href="#">Dec 19, 2021</a></span>
								</div>
							</div>
						</div>
					</div>

					<div class="col-12 col-sm-6 col-md-4 mb-4 mb-md-0">
						<div class="post-entry">
							<a href="#" class="post-thumbnail"><img src="../assets/images/post-2.jpg" alt="Image" class="img-fluid"></a>
							<div class="post-content-entry">
								<h3><a href="#">How To Keep Your Furniture Clean</a></h3>
								<div class="meta">
									<span>by <a href="#">Robert Fox</a></span> <span>on <a href="#">Dec 15, 2021</a></span>
								</div>
							</div>
						</div>
					</div>

					<div class="col-12 col-sm-6 col-md-4 mb-4 mb-md-0">
						<div class="post-entry">
							<a href="#" class="post-thumbnail"><img src="../assets/images/post-3.jpg" alt="Image" class="img-fluid"></a>
							<div class="post-content-entry">
								<h3><a href="#">Small Space Furniture Apartment Ideas</a></h3>
								<div class="meta">
									<span>by <a href="#">Kristin Watson</a></span> <span>on <a href="#">Dec 12, 2021</a></span>
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

/* ======= 賣點卡 ======= */
.feature-card{
  background: rgba(255,255,255,.9);
  border-radius: 1rem; padding: 1.25rem;
  box-shadow: 0 10px 30px rgba(0,0,0,.06);
  transition: transform .2s ease, box-shadow .2s ease;
}
.feature-card:hover{ transform: translateY(-4px); box-shadow: 0 16px 40px rgba(0,0,0,.12); }
.feature-card .icon-wrap{
  width:44px; height:44px; border-radius: 10px;
  display:grid; place-items:center; margin-bottom:.5rem;
  background: #e9f7f1;
}

/* ======= 展示牆 ======= */
.shot-main, .shot-side img{ box-shadow: 0 20px 50px rgba(0,0,0,.18); }

/* ======= CTA ======= */
.cta-neo{
  position: relative; background: linear-gradient(180deg,#131619,#0f1113);
  padding: 72px 0; overflow: hidden;
}
.glow-c{ position:absolute; left:50%; top:-60px; transform:translateX(-50%);
  width:800px; height:260px; filter: blur(60px); opacity:.4;
  background: radial-gradient(ellipse at center, #5be7b5, transparent 60%);
}

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
  isolation: isolate;
  /* 讓 sticky/滾動有空間：可依版面微調 200~260vh */
  height: 220vh;
  overflow: clip;
}

/* 外框容器：用來維持比例，但背景改成透明 */
.video-wrap {
  /* 使用容器維持 16:9，影片再 cover 住 */
  aspect-ratio: 16 / 9;
  position: sticky;
  width: 100%;
  overflow: hidden;
  z-index: 0;
  background: transparent !important;
  /* transform-origin: center top; */
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
  z-index: 0;
  position: relative;
  transform: translateY(0.5px);
}

/* 影片下方字體 */
.caption {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%); /* JS 會覆寫 Y 偏移 */
  z-index: 3;             /* 文字層在上 */
  text-align: center;
  color: #000000;
  padding: 0 1rem;
  max-width: 960px;
  will-change: transform;
  transition: transform 0.08s linear;
  text-shadow: 0 2px 12px rgba(0,0,0,.45); /* 提升可讀性 */
}
.cap-title { font-size: clamp(20px, 3.2vw, 36px); margin: 0 0 .5rem; }
.cap-desc  { font-size: clamp(14px, 2vw, 18px); line-height: 1.6; margin: 0; text-shadow: 0 2px 12px rgba(0,0,0,.45); }

/* 尊重使用者「降低動態效果」偏好 */
@media (prefers-reduced-motion: reduce) {
  .scroll-scale { transform: none !important; transition: none !important; }
}
</style>
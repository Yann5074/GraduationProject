<script setup>
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
const router = useRouter()
const goShop = () => router.push({ name: 'ProductList' })

onMounted(() => { 
  // 若有影片，避免 iOS 自動全屏
  const v = document.querySelector('.hero-video')
  if (v) v.play().catch(()=>{})
})

</script>

<template>
  <!-- Start Hero Section -->
	<section class="hero-neo">
 		<!-- 用影片；若沒有影片就改成背景圖 -->
    	<video class="hero-video" autoplay muted loop playsinline preload="auto" poster="/asset/images/hero-poster.jpg">
      		<source src="/asset/videos/hero2.mp4" type="video/mp4" />
    	</video>

    	<div class="hero-overlay"></div>

    	<div class="container d-flex flex-column justify-content-center align-items-start h-100">
      		<h1 class="display-3 fw-bold text-white lh-1 mb-3">Make Interiors <br><span class="grad">Feel Alive</span>
      		</h1>
      		<p class="lead text-white-50 mb-4">即時渲染展示你的家居靈感
      		</p>
      		<div class="d-flex gap-3">
        	<button class="btn btn-neo btn-lg px-4" @click="goShop">開始選購</button>
        	<RouterLink to="/design" class="btn btn-outline-light btn-lg px-4">看看靈感</RouterLink>
      		</div>

      		<!-- 信任徽章 -->
      		<!-- <div class="trust mt-4">
        		<img src="/asset/images/bolt.avif" alt="Trusted" />
        		<span class="text-white-50 ms-2">超過 2,000 位設計師選用</span>
      		</div> -->
    	</div>

    		<!-- 漂浮光暈 -->
    		<div class="glow glow-a"></div>
    		<div class="glow glow-b"></div>
  </section>
	<!-- End Hero Section -->

  <!-- Start Announcement Section -->
  <section class="announcement py-3 bg-transparent text-dark text-center">
    <div class="container">
      <p class="mb-0 fs-5">
        🎉 全館限時優惠中！滿 NT$2000 免運費，活動至 11/15 截止
      </p>
    </div>
  </section>
  <!-- End Announcement Section -->

  <!-- Start Product Section -->
		<div class="product-section">
			<div class="container">
				<div class="row">

					<!-- Start Column 1 -->
					<div class="col-md-12 col-lg-3 mb-5 mb-lg-0">
						<h2 class="mb-4 section-title">Crafted with excellent material.</h2>
						<p class="mb-4">Donec vitae odio quis nisl dapibus malesuada. Nullam ac aliquet velit. Aliquam vulputate velit imperdiet dolor tempor tristique. </p>
						<p><RouterLink class="btn btn-sm btn-outline-black" to="/shop">看更多</RouterLink></p>
					</div> 
					<!-- End Column 1 -->

					<!-- Start Column 2 -->
					<div class="col-12 col-md-4 col-lg-3 mb-5 mb-md-0">
						<a class="product-item" href="cart.html">
							<img src="../assets/images/product-1.png" class="img-fluid product-thumbnail">
							<h3 class="product-title">Nordic Chair</h3>
							<strong class="product-price">$50.00</strong>

							<span class="icon-cross">
								<img src="../assets/images/cross.svg" class="img-fluid">
							</span>
						</a>
					</div> 
					<!-- End Column 2 -->

					<!-- Start Column 3 -->
					<div class="col-12 col-md-4 col-lg-3 mb-5 mb-md-0">
						<a class="product-item" href="cart.html">
							<img src="../assets/images/product-2.png" class="img-fluid product-thumbnail">
							<h3 class="product-title">Kruzo Aero Chair</h3>
							<strong class="product-price">$78.00</strong>

							<span class="icon-cross">
								<img src="../assets/images/cross.svg" class="img-fluid">
							</span>
						</a>
					</div>
					<!-- End Column 3 -->

					<!-- Start Column 4 -->
					<div class="col-12 col-md-4 col-lg-3 mb-5 mb-md-0">
						<a class="product-item" href="cart.html">
							<img src="../assets/images/product-3.png" class="img-fluid product-thumbnail">
							<h3 class="product-title">Ergonomic Chair</h3>
							<strong class="product-price">$43.00</strong>

							<span class="icon-cross">
								<img src="../assets/images/cross.svg" class="img-fluid">
							</span>
						</a>
					</div>
					<!-- End Column 4 -->

				</div>
			</div>
		</div>
	<!-- End Product Section -->

  <!-- Start Why Choose Us Section -->
		<div class="why-choose-section">
			<div class="container">
				<div class="row justify-content-between">
					<div class="col-lg-6">
						<h2 class="section-title">Why Choose Us</h2>
						<p>Donec vitae odio quis nisl dapibus malesuada. Nullam ac aliquet velit. Aliquam vulputate velit imperdiet dolor tempor tristique.</p>

						<div class="row my-5">
							<div class="col-6 col-md-6">
								<div class="feature">
									<div class="icon">
										<img src="../assets/images/truck.svg" alt="Image" class="imf-fluid">
									</div>
									<h3>Fast &amp; Free Shipping</h3>
									<p>Donec vitae odio quis nisl dapibus malesuada. Nullam ac aliquet velit. Aliquam vulputate.</p>
								</div>
							</div>

							<div class="col-6 col-md-6">
								<div class="feature">
									<div class="icon">
										<img src="../assets/images/bag.svg" alt="Image" class="imf-fluid">
									</div>
									<h3>Easy to Shop</h3>
									<p>Donec vitae odio quis nisl dapibus malesuada. Nullam ac aliquet velit. Aliquam vulputate.</p>
								</div>
							</div>

							<div class="col-6 col-md-6">
								<div class="feature">
									<div class="icon">
										<img src="../assets/images/support.svg" alt="Image" class="imf-fluid">
									</div>
									<h3>24/7 Support</h3>
									<p>Donec vitae odio quis nisl dapibus malesuada. Nullam ac aliquet velit. Aliquam vulputate.</p>
								</div>
							</div>

							<div class="col-6 col-md-6">
								<div class="feature">
									<div class="icon">
										<img src="../assets/images/return.svg" alt="Image" class="imf-fluid">
									</div>
									<h3>Hassle Free Returns</h3>
									<p>Donec vitae odio quis nisl dapibus malesuada. Nullam ac aliquet velit. Aliquam vulputate.</p>
								</div>
							</div>

						</div>
					</div>

					<div class="col-lg-5">
						<div class="img-wrap">
							<img src="../assets/images/why-choose-us-img.jpg" alt="Image" class="img-fluid">
						</div>
					</div>

				</div>
			</div>
		</div>
	<!-- End Why Choose Us Section -->

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
.grad{ background: linear-gradient(90deg,#8ef7c2,#74d6ff,#b7a6ff); -webkit-background-clip:text; background-clip:text; color:transparent; }

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
</style>
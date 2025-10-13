// =======================
// script.js (完整版本)
// =======================

(function initSideMenuActive() {
    const allSideMenu = document.querySelectorAll("#sidebar .side-menu.top li a");
    allSideMenu.forEach((item) => {
        const li = item.parentElement;
        item.addEventListener("click", function () {
            allSideMenu.forEach((i) => i.parentElement.classList.remove("active"));
            li.classList.add("active");
        });
    });
})();

(function initSidebarToggle() {
    const menuBar = document.querySelector("#content nav .bx.bx-menu");
    const sidebar = document.getElementById("sidebar");

    function adjustSidebar() {
        if (!sidebar) return;
        if (window.innerWidth <= 576) {
            sidebar.classList.add("hide");
            sidebar.classList.remove("show");
        } else {
            sidebar.classList.remove("hide");
            sidebar.classList.add("show");
        }
    }

    if (menuBar && sidebar) {
        menuBar.addEventListener("click", function () {
            sidebar.classList.toggle("hide");
        });
    }

    window.addEventListener("load", adjustSidebar);
    window.addEventListener("resize", adjustSidebar);
})();

(function initTheme() {
    const THEME_KEY = "theme";
    const switchMode = document.getElementById("switch-mode");

    function applyTheme(theme) {
        const root = document.documentElement;
        const body = document.body;
        if (theme === "dark") {
            root.classList.add("dark");
            body.classList.add("dark");
            if (switchMode) switchMode.checked = true;
        } else {
            root.classList.remove("dark");
            body.classList.remove("dark");
            if (switchMode) switchMode.checked = false;
        }
    }

    // 初次載入：讀 localStorage；若無 → 預設淺色
    document.addEventListener("DOMContentLoaded", () => {
        try {
            let theme = localStorage.getItem(THEME_KEY);
            if (!theme) {
                theme = "light"; // 預設淺色
            }
            applyTheme(theme);
        } catch (e) {
            console.warn("Theme init failed", e);
        }
    });

    if (switchMode) {
        switchMode.addEventListener("change", function () {
            const theme = this.checked ? "dark" : "light";
            try {
                localStorage.setItem(THEME_KEY, theme);
            } catch (e) {
                console.warn("Theme save failed", e);
            }
            applyTheme(theme);
        });
    }
})();

(function initDropdowns() {
    const notiBtn = document.querySelector(".notification");
    const notiMenu = document.querySelector(".notification-menu");
    const profileBtn = document.querySelector(".profile");
    const profileMenu = document.querySelector(".profile-menu");

    if (notiBtn && notiMenu) {
        notiBtn.addEventListener("click", function (e) {
            e.stopPropagation();
            notiMenu.classList.toggle("show");
            if (profileMenu) profileMenu.classList.remove("show");
        });
    }
    if (profileBtn && profileMenu) {
        profileBtn.addEventListener("click", function (e) {
            e.stopPropagation();
            profileMenu.classList.toggle("show");
            if (notiMenu) notiMenu.classList.remove("show");
        });
    }

    window.addEventListener("click", function (e) {
        if (notiMenu && !e.target.closest(".notification")) {
            notiMenu.classList.remove("show");
        }
        if (profileMenu && !e.target.closest(".profile")) {
            profileMenu.classList.remove("show");
        }
    });
})();

(function initInlineMenus() {
    function hideAllMenus() {
        document.querySelectorAll(".menu").forEach((m) => {
            m.style.display = "none";
        });
    }

    document.addEventListener("DOMContentLoaded", hideAllMenus);

    window.toggleMenu = function (menuId) {
        const menu = document.getElementById(menuId);
        if (!menu) return;
        document.querySelectorAll(".menu").forEach((m) => {
            if (m !== menu) m.style.display = "none";
        });
        menu.style.display =
            menu.style.display === "none" || menu.style.display === ""
                ? "block"
                : "none";
    };
})();

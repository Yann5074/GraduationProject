// src/utils/google.js
export function loadGoogleSdk() {
  return new Promise((resolve, reject) => {
    if (window.google?.accounts?.id) return resolve()

    const script = document.createElement('script')
    script.src = 'https://accounts.google.com/gsi/client'
    script.async = true
    script.defer = true
    script.onload = () => resolve()
    script.onerror = () => reject(new Error('Google SDK 載入失敗'))
    document.head.appendChild(script)
  })
}

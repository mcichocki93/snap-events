import { ref, computed } from 'vue'

/**
 * Consent for analytics, and the only thing that starts Google Analytics.
 *
 * Analytics cookies need consent before they are set, so the tag cannot sit in
 * index.html loading on every visit - which is exactly what it used to do, while
 * the privacy policy claimed analytics ran "only with consent". Nothing here
 * loads until someone says yes, and saying no has to be as easy as saying yes.
 *
 * The answer lives in localStorage rather than a cookie: a cookie asking for
 * permission to set cookies is a bad look, and this one never leaves the browser.
 */

const STORAGE_KEY = 'snapevents.cookieConsent'
const GA_MEASUREMENT_ID = 'G-M1RDRFSMNZ'

export type ConsentChoice = 'granted' | 'denied' | null

/** Shared across every caller, so the banner and the privacy page agree. */
const choice = ref<ConsentChoice>(read())

let analyticsStarted = false

function read(): ConsentChoice {
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    return stored === 'granted' || stored === 'denied' ? stored : null
  } catch {
    // Private mode, or storage blocked entirely. No stored answer means we ask
    // again and run nothing in the meantime, which is the safe direction.
    return null
  }
}

function write(value: ConsentChoice): void {
  try {
    if (value === null) localStorage.removeItem(STORAGE_KEY)
    else localStorage.setItem(STORAGE_KEY, value)
  } catch {
    // The choice still holds for this visit; it just will not be remembered.
  }
}

/**
 * Injects the Google tag. Deliberately not idempotent-by-accident: a second call
 * would add a second script and double every pageview.
 */
function startAnalytics(): void {
  if (analyticsStarted || typeof document === 'undefined') return
  analyticsStarted = true

  const tag = document.createElement('script')
  tag.async = true
  tag.src = `https://www.googletagmanager.com/gtag/js?id=${GA_MEASUREMENT_ID}`
  document.head.appendChild(tag)

  const w = window as unknown as { dataLayer?: unknown[] }
  w.dataLayer = w.dataLayer || []
  function gtag(...args: unknown[]): void {
    w.dataLayer!.push(args)
  }

  gtag('js', new Date())
  gtag('config', GA_MEASUREMENT_ID)
}

/**
 * Clears what Google's tag leaves behind, as far as a page can: the _ga cookies
 * are readable from JavaScript, so they can be expired here. Anything already
 * sent cannot be recalled, which is the whole reason for asking first.
 */
function clearAnalyticsCookies(): void {
  if (typeof document === 'undefined') return

  const host = window.location.hostname
  const domains = [host, `.${host}`, `.${host.split('.').slice(-2).join('.')}`]

  for (const entry of document.cookie.split(';')) {
    const name = entry.split('=')[0]?.trim()
    if (!name || !name.startsWith('_ga')) continue

    for (const domain of domains) {
      document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; domain=${domain}`
    }
    document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/`
  }
}

export function useCookieConsent() {
  const accept = (): void => {
    choice.value = 'granted'
    write('granted')
    startAnalytics()
  }

  const decline = (): void => {
    const wasGranted = choice.value === 'granted'
    choice.value = 'denied'
    write('denied')
    clearAnalyticsCookies()

    // Withdrawing has to actually stop the tag, and a tag already running cannot
    // be unloaded. Reloading is the honest way to make "no" take effect now.
    if (wasGranted && analyticsStarted) window.location.reload()
  }

  /** Called once on boot: starts analytics only for a yes given earlier. */
  const applyStoredChoice = (): void => {
    if (choice.value === 'granted') startAnalytics()
  }

  return {
    choice: computed(() => choice.value),
    needsAnswer: computed(() => choice.value === null),
    accept,
    decline,
    applyStoredChoice
  }
}

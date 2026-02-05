// Font utilities for safe font family handling

// Common fonts available via Google Fonts CDN (loaded in index.html)
const AVAILABLE_FONTS: string[] = [
  'Poppins',
  'Playfair Display',
  'Roboto',
  'Montserrat',
  'Lato',
  'Open Sans',
  'Inter'
]

// System font stack as fallback
const SYSTEM_FONT_STACK = '-apple-system, BlinkMacSystemFont, "Segoe UI", "Helvetica Neue", Arial, sans-serif'

/**
 * Get safe font family string with fallback to system fonts
 * @param fontFamily - Font family from client settings
 * @returns Font family string with fallback
 */
export function getSafeFontFamily(fontFamily: string | null | undefined): string {
  if (!fontFamily || typeof fontFamily !== 'string') {
    return SYSTEM_FONT_STACK
  }

  const cleanFont = fontFamily.trim()

  // If empty after trim, use system fonts
  if (!cleanFont) {
    return SYSTEM_FONT_STACK
  }

  // Check if font needs quotes (has spaces)
  const needsQuotes = cleanFont.includes(' ')
  const fontWithQuotes = needsQuotes ? `"${cleanFont}"` : cleanFont

  // Return font with system fallback
  return `${fontWithQuotes}, ${SYSTEM_FONT_STACK}`
}

/**
 * Check if font is available in our CDN
 * @param fontFamily - Font family to check
 * @returns true if font is available
 */
export function isFontAvailable(fontFamily: string | null | undefined): boolean {
  if (!fontFamily) return false
  return AVAILABLE_FONTS.includes(fontFamily.trim())
}

/**
 * Get list of available fonts for UI
 * @returns Array of available font names
 */
export function getAvailableFonts(): string[] {
  return [...AVAILABLE_FONTS]
}
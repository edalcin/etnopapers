/**
 * Development environment detection
 * Check if running from source (src/ dir) or from compiled dist/
 */

export const isDev = __dirname.includes('src/main') || __dirname.includes('src\\main');

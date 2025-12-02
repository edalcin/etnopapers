/**
 * Bootstrap script - loads React app without ES modules
 * This solves the CORS/module loading issue in Electron
 */

(async function bootstrap() {
  console.log('[Bootstrap] Starting...');

  // Wait for DOM to be ready
  if (document.readyState === 'loading') {
    await new Promise(resolve => document.addEventListener('DOMContentLoaded', resolve));
  }

  console.log('[Bootstrap] DOM ready, loading app module...');

  try {
    // Import React app dynamically
    const { default: App } = await import('./App.tsx');
    const React = await import('react');
    const ReactDOM = await import('react-dom/client');

    console.log('[Bootstrap] Modules loaded, mounting app...');

    const root = document.getElementById('root');
    if (!root) throw new Error('Root element not found');

    ReactDOM.default.createRoot(root).render(
      React.default.createElement(
        React.default.StrictMode,
        null,
        React.default.createElement(App)
      )
    );

    console.log('[Bootstrap] App mounted successfully');
  } catch (error) {
    console.error('[Bootstrap] Failed to load app:', error);
    const root = document.getElementById('root');
    if (root) {
      root.innerHTML = '<div style="padding: 20px; color: red;"><h1>Failed to load application</h1><p>' +
        (error instanceof Error ? error.message : String(error)) +
        '</p></div>';
    }
  }
})();

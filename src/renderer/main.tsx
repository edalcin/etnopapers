/**
 * React app entry point
 * Renders the main App component with error boundary
 */

import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './index.css';

// Global error handler
window.addEventListener('error', (event) => {
  console.error('[Global Error]', event.error);
});

window.addEventListener('unhandledrejection', (event) => {
  console.error('[Unhandled Promise Rejection]', event.reason);
});

console.log('[React] Starting app initialization');
console.log('[React] Document ready state:', document.readyState);

const root = document.getElementById('root');
console.log('[React] Root element:', root);

if (!root) {
  console.error('[React] Root element not found!');
  throw new Error('Root element not found');
}

console.log('[React] Creating React root');
try {
  ReactDOM.createRoot(root).render(
    <React.StrictMode>
      <App />
    </React.StrictMode>
  );
  console.log('[React] App rendered');
} catch (error) {
  console.error('[React] Render error:', error);
  throw error;
}

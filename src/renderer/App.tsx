/**
 * Root App Component
 * Handles routing and main layout structure
 */

import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Layout from './layouts/Layout';
import HomePage from './pages/HomePage';
import UploadPage from './pages/UploadPage';
import RecordsPage from './pages/RecordsPage';
import SettingsPage from './pages/SettingsPage';
import AboutPage from './pages/AboutPage';

const App: React.FC = () => {
  return (
    <Router>
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/upload" element={<UploadPage />} />
          <Route path="/records" element={<RecordsPage />} />
          <Route path="/settings" element={<SettingsPage />} />
          <Route path="/about" element={<AboutPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
    </Router>
  );
};

export default App;

/**
 * Main layout component with sidebar, header, and status bar
 */

import React, { useState } from 'react';
import { Outlet } from 'react-router-dom';
import Sidebar from '../components/Sidebar';
import Header from '../components/Header';
import StatusBar from '../components/StatusBar';

const Layout: React.FC = () => {
  const [sidebarOpen, setSidebarOpen] = useState(true);

  return (
    <div className="flex h-screen bg-gray-100">
      {/* Sidebar */}
      <Sidebar open={sidebarOpen} onToggle={() => setSidebarOpen(!sidebarOpen)} />

      {/* Main content */}
      <div className="flex flex-col flex-1 overflow-hidden">
        {/* Header */}
        <Header onMenuClick={() => setSidebarOpen(!sidebarOpen)} />

        {/* Content area */}
        <main className="flex-1 overflow-y-auto">
          <Outlet />
        </main>

        {/* Status bar */}
        <StatusBar />
      </div>
    </div>
  );
};

export default Layout;

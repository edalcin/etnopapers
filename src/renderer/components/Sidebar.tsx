import React from 'react';
import { Link } from 'react-router-dom';

interface SidebarProps {
  open: boolean;
  onToggle: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({ open }) => {
  return (
    <aside className={`${open ? 'w-64' : 'w-20'} bg-gray-900 text-white transition-all duration-300 flex flex-col`}>
      <div className="p-4 text-center">
        <h1 className={`font-bold ${open ? 'text-lg' : 'text-xs'}`}>EP</h1>
      </div>
      <nav className="flex-1 px-2 py-4 space-y-2">
        <Link to="/" className="block px-4 py-2 rounded hover:bg-gray-800 text-sm">
          {open && 'Home'}
        </Link>
        <Link to="/upload" className="block px-4 py-2 rounded hover:bg-gray-800 text-sm">
          {open && 'Upload'}
        </Link>
        <Link to="/records" className="block px-4 py-2 rounded hover:bg-gray-800 text-sm">
          {open && 'Records'}
        </Link>
        <Link to="/settings" className="block px-4 py-2 rounded hover:bg-gray-800 text-sm">
          {open && 'Settings'}
        </Link>
        <Link to="/about" className="block px-4 py-2 rounded hover:bg-gray-800 text-sm">
          {open && 'About'}
        </Link>
      </nav>
    </aside>
  );
};

export default Sidebar;

import React from 'react';

interface HeaderProps {
  onMenuClick: () => void;
}

const Header: React.FC<HeaderProps> = ({ onMenuClick }) => {
  return (
    <header className="bg-white border-b border-gray-200 px-6 py-4 flex items-center">
      <button onClick={onMenuClick} className="text-gray-600 hover:text-gray-900">☰</button>
      <h1 className="ml-4 text-xl font-bold text-gray-900">EtnoPapers</h1>
    </header>
  );
};

export default Header;

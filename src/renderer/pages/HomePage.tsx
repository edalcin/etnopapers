import React from 'react';
import { Link } from 'react-router-dom';

const HomePage: React.FC = () => {
  return (
    <div className="p-8">
      <h1 className="text-3xl font-bold mb-6">Welcome to EtnoPapers</h1>
      <p className="text-gray-600 mb-8">Extract and catalog ethnobotanical metadata from scientific papers.</p>

      <div className="grid grid-cols-2 gap-4">
        <Link to="/upload" className="p-4 bg-blue-600 text-white rounded hover:bg-blue-700">
          Upload PDF
        </Link>
        <Link to="/records" className="p-4 bg-green-600 text-white rounded hover:bg-green-700">
          View Records
        </Link>
      </div>
    </div>
  );
};

export default HomePage;

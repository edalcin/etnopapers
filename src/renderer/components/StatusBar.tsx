import React, { useEffect, useState } from 'react';

const StatusBar: React.FC = () => {
  const [recordCount, setRecordCount] = useState(0);

  useEffect(() => {
    window.etnopapers.storage.count().then(setRecordCount).catch(console.error);
  }, []);

  return (
    <footer className="bg-gray-800 text-white px-6 py-2 text-sm flex justify-between">
      <span>Records: {recordCount}</span>
      <span>Ready</span>
    </footer>
  );
};

export default StatusBar;

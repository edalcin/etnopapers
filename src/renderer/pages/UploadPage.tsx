import React, { useState } from 'react';

const UploadPage: React.FC = () => {
  const [isExtracting, setIsExtracting] = useState(false);
  const [progress, setProgress] = useState(0);

  const handleDrop = async (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    const files = e.dataTransfer.files;
    if (files.length === 0) return;

    const file = files[0];
    if (!file.name.endsWith('.pdf')) {
      alert('Please drop a PDF file');
      return;
    }

    setIsExtracting(true);
    setProgress(0);

    try {
      // Unsubscribe function would be returned here
      window.etnopapers.extraction.onProgress((status) => {
        setProgress(status.progress);
      });

      const result = await window.etnopapers.extraction.start(file.path);
      alert('Extraction successful!');
      console.log('Extracted record:', result);
    } catch (error) {
      alert(`Extraction failed: ${error}`);
    } finally {
      setIsExtracting(false);
    }
  };

  return (
    <div className="p-8">
      <h1 className="text-3xl font-bold mb-6">Upload PDF</h1>

      <div
        onDrop={handleDrop}
        onDragOver={(e) => e.preventDefault()}
        className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center hover:border-blue-400"
      >
        <p className="text-gray-600">Drag and drop your PDF here</p>
      </div>

      {isExtracting && (
        <div className="mt-6">
          <div className="flex items-center mb-2">
            <span className="text-gray-600">Extracting: {progress}%</span>
          </div>
          <div className="w-full bg-gray-200 rounded h-2">
            <div className="bg-blue-600 h-2 rounded" style={{ width: `${progress}%` }}></div>
          </div>
        </div>
      )}
    </div>
  );
};

export default UploadPage;

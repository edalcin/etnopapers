import React from 'react';

const AboutPage: React.FC = () => {
  return (
    <div className="p-8 max-w-2xl">
      <h1 className="text-3xl font-bold mb-6">About EtnoPapers</h1>

      <div className="space-y-4 text-gray-700">
        <p>
          <strong>EtnoPapers</strong> is a Windows desktop application designed for ethnobotanists and
          researchers working with traditional plant knowledge.
        </p>

        <h2 className="text-xl font-semibold mt-6 mb-3">Features</h2>
        <ul className="list-disc list-inside space-y-2">
          <li>Upload and process scientific papers in PDF format</li>
          <li>Automatic extraction of ethnobotanical metadata using AI</li>
          <li>Local storage with cloud backup capability</li>
          <li>Full record management with CRUD interface</li>
          <li>Support for multiple languages</li>
        </ul>

        <h2 className="text-xl font-semibold mt-6 mb-3">Technology</h2>
        <ul className="list-disc list-inside space-y-2">
          <li>Built with Electron + React</li>
          <li>Uses OLLAMA for local AI processing</li>
          <li>MongoDB integration for cloud sync</li>
          <li>TypeScript for type safety</li>
        </ul>

        <h2 className="text-xl font-semibold mt-6 mb-3">Version</h2>
        <p>v1.0.0</p>
      </div>
    </div>
  );
};

export default AboutPage;

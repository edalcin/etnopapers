import React, { useState, useEffect } from 'react';
import { ArticleRecord } from '@shared/types/article';

const RecordsPage: React.FC = () => {
  const [records, setRecords] = useState<ArticleRecord[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadRecords();
  }, []);

  const loadRecords = async () => {
    try {
      const allRecords = await window.etnopapers.storage.getAll();
      setRecords(allRecords);
    } catch (error) {
      console.error('Failed to load records:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div className="p-8">Loading...</div>;

  return (
    <div className="p-8">
      <h1 className="text-3xl font-bold mb-6">Records ({records.length})</h1>

      {records.length === 0 ? (
        <p className="text-gray-600">No records yet. Upload a PDF to start.</p>
      ) : (
        <div className="space-y-4">
          {records.map((record) => (
            <div key={record._id} className="border border-gray-200 rounded p-4 hover:shadow">
              <h3 className="font-semibold">{record.titulo}</h3>
              <p className="text-sm text-gray-600">{record.autores.join(', ')}</p>
              <p className="text-sm text-gray-500">{record.ano}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default RecordsPage;

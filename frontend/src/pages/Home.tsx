import { useState } from 'react'
import PDFUpload from '../components/PDFUpload'
import ArticlesTable from '../components/ArticlesTable'
import MainLayout from '../components/MainLayout'

export default function Home() {
  const [refreshKey, setRefreshKey] = useState(0)

  const handleUploadSuccess = () => {
    setRefreshKey(k => k + 1)
  }

  return (
    <MainLayout>
      <div className="home-page">
        <h1>Etnopapers - Extração de Metadados Etnobotânicos</h1>
        
        <div className="upload-section">
          <PDFUpload onSuccess={handleUploadSuccess} />
        </div>

        <div className="articles-section">
          <h2>Artigos Salvos</h2>
          <ArticlesTable key={refreshKey} />
        </div>
      </div>
    </MainLayout>
  )
}

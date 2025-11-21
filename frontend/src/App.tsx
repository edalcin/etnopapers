import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import Layout from '@components/Layout'
import Home from '@pages/Home'
import Upload from '@pages/Upload'
import History from '@pages/History'
import Settings from '@pages/Settings'
import NotFound from '@pages/NotFound'

export default function App() {
  return (
    <Router>
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<Home />} />
          <Route path="/upload" element={<Upload />} />
          <Route path="/history" element={<History />} />
          <Route path="/settings" element={<Settings />} />
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    </Router>
  )
}

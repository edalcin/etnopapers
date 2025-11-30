export async function saveMongoConfig(mongoUri: string) {
  const response = await fetch('/api/config/save', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ mongo_uri: mongoUri })
  })
  return response.ok
}

export async function getConfigStatus() {
  const response = await fetch('/api/config/status')
  return response.json()
}

export async function validateMongoConnection(mongoUri: string) {
  const response = await fetch('/api/config/validate-mongo', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ mongo_uri: mongoUri })
  })
  return response.ok
}

export default {
  appId: "com.etnopapers.app",
  productName: "EtnoPapers",
  directories: {
    output: "dist",
    buildResources: "resources"
  },
  files: [
    "dist/**/*",
    "node_modules/**/*",
    "package.json"
  ],
  win: {
    target: [
      {
        target: "nsis",
        arch: ["x64"]
      },
      {
        target: "portable",
        arch: ["x64"]
      }
    ],
    artifactName: "${productName}-${version}-${arch}.${ext}"
  },
  nsis: {
    oneClick: false,
    allowToChangeInstallationDirectory: true,
    createDesktopShortcut: true,
    createStartMenuShortcut: true,
    shortcutName: "EtnoPapers"
  },
  extraMetadata: {
    main: "dist/main/index.js"
  }
}

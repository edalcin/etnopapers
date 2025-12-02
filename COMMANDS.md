# EtnoPapers - Quick Commands Reference

## Development

```bash
# Install dependencies
pnpm install

# Start development server (UI testing)
pnpm run dev                    # http://localhost:5173

# Build for production
pnpm run build                  # Output: dist/renderer/

# Build main process (when ready)
# pnpm run build:main           # Requires esbuild script
```

## Code Quality

```bash
# Check code style
pnpm run lint

# Format code
pnpm run format

# Type check
# (TypeScript checking happens during dev/build)
```

## Testing

```bash
# Unit tests
pnpm run test

# Unit tests with UI
pnpm run test:ui

# E2E tests
pnpm run test:e2e

# (Tests require implementation)
```

## Build & Distribution

```bash
# Build entire application
pnpm run build              # Builds renderer to dist/renderer/

# Create Windows installer (when Electron configured)
pnpm run dist               # Creates installer .exe file
# Requires: main process built, proper Electron config
```

## Development Server Details

```
Server URL:    http://localhost:5173
Hot Reload:    Enabled (automatic)
Source Maps:   Included (for debugging)
Port:          5173 (configurable with --port flag)
```

## Requirements

- **Node.js**: 18+
- **pnpm**: 10.16.1+
- **OLLAMA**: For PDF extraction (http://localhost:11434)

## OLLAMA Commands

```bash
# Start OLLAMA service
ollama serve                # Runs on http://localhost:11434

# List available models
ollama list

# Pull a model
ollama pull llama2          # Default model

# Test connection
curl http://localhost:11434/api/tags
```

## Git Commands

```bash
# Check status
git status

# View recent commits
git log --oneline -10

# Create new branch (for features)
git checkout -b feature-name

# Commit changes
git add .
git commit -m "message"

# Push to main
git push origin main
```

## Environment Setup

```bash
# Set working directory
cd H:\git\etnopapers

# List available scripts
pnpm run
# Shows all available npm scripts
```

## Troubleshooting Commands

```bash
# Clear cache and reinstall
rm -r node_modules pnpm-lock.yaml
pnpm install

# Kill process on port 5173
netstat -ano | findstr :5173
taskkill /PID <PID> /F

# Check OLLAMA health
curl http://localhost:11434/api/tags

# View TypeScript errors
# (Errors shown during build/dev)

# Check eslint issues
pnpm run lint
```

## Project Structure

```
H:\git\etnopapers\
├── src/
│   ├── main/           # Electron main process
│   ├── renderer/       # React UI
│   ├── preload/        # IPC bridge
│   └── shared/         # Shared utilities
├── specs/main/         # Specifications
├── dist/               # Build output
├── node_modules/       # Dependencies
└── package.json        # Project config
```

## Important Files

```
vite.config.ts                    # Vite bundler config
tsconfig.json                     # TypeScript config
package.json                      # Dependencies
postcss.config.js                 # CSS processing
tailwind.config.js                # Styling config
.eslintrc.json                    # Code style rules
electron-builder.config.js        # Windows installer config
```

## URLs

```
Dev Server:       http://localhost:5173
OLLAMA Health:    http://localhost:11434/api/tags
Repository:       https://github.com/edalcin/etnopapers
```

## Key Directories

```
src/renderer/pages/       - React pages (Home, Upload, Records, Settings, About)
src/main/services/        - Business logic (PDF, OLLAMA, Storage, Validation, etc)
src/main/ipc/            - IPC handlers (secure communication)
src/preload/             - Context-isolated APIs
src/shared/types/        - TypeScript type definitions
src/shared/utils/        - Utility functions
specs/main/              - Feature specifications
dist/renderer/           - Built UI (after pnpm run build)
```

## Performance

```
Dev Build Time:         ~200ms (Vite)
Production Build:       ~900ms
Renderer Bundle:        228 kB (71 kB gzipped)
CSS Bundle:             16.7 kB (4.2 kB gzipped)
Typical Page Load:      <500ms (dev), <200ms (prod)
UI Interaction Target:  <200ms
```

## Documentation

```
MVP_COMPLETION_SUMMARY.md     - Full project overview
TESTING_GUIDE.md              - Testing procedures
COMMANDS.md                   - This file
specs/main/spec.md            - Feature specification
specs/main/plan.md            - Technical design
specs/main/tasks.md           - Task breakdown (88 tasks)
specs/main/data-model.md      - Data structures
```

## Common Workflows

### Start Fresh Development Session
```bash
cd H:\git\etnopapers
pnpm install                    # If dependencies changed
pnpm run dev                    # Start dev server
# Open http://localhost:5173
```

### Build for Production
```bash
pnpm run build                  # Build renderer
# pnpm run build:main          # Build main process (when ready)
pnpm run dist                   # Create installer (when Electron ready)
```

### Debug Build Issues
```bash
pnpm run lint                   # Check code style
pnpm run build                  # Build and see errors
npm run build 2>&1 | tee log.txt # Save build output to file
```

### Update Dependencies
```bash
pnpm update                     # Update to latest compatible versions
pnpm install                    # Reinstall with updates
pnpm run build                  # Test build after update
```

## Notes

- All commands run from project root: `H:\git\etnopapers`
- TypeScript types checked during build/dev
- Hot reload works for React components
- CSS/Tailwind changes reflect immediately
- Environment variables can be added to `.env` file
- Data stored in `~/.config/EtnoPapers/` (Electron app)

## Support

- See `TESTING_GUIDE.md` for troubleshooting
- See `MVP_COMPLETION_SUMMARY.md` for overview
- Check `specs/main/` for detailed documentation
- GitHub Issues: https://github.com/edalcin/etnopapers/issues

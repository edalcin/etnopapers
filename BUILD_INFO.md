# EtnoPapers Build Information

Generated: 2025-12-15 07:01

## Executable Files

### Main Application
- **File**: EtnoPapers.exe
- **Size**: 148 KB (standalone executable)
- **Location**: `D:\git\etnopapers\EtnoPapers.exe` (project root for easy access)
- **Framework**: .NET 8.0 (self-contained, no installation required)
- **Runtime**: Windows x64

### Portable Package
- **File**: EtnoPapers-Portable.zip
- **Size**: ~79 MB (includes .NET 8.0 runtime)
- **Location**: `D:\git\etnopapers\artifacts\EtnoPapers-Portable.zip`
- **Contents**: Complete application with all runtime dependencies

## Build Configuration

- **Configuration**: Release
- **Build Type**: Self-contained deployment
- **Framework**: net8.0-windows
- **Runtime**: win-x64
- **No External Dependencies**: All runtime files included

## Changes Included in This Build

### 1. Fixed Extraction Error Messages
- **Issue**: Error dialogs were still referencing OLLAMA after switching to cloud AI
- **Fix**: Updated error messages to reference generic "AI service"
- **File**: `src/EtnoPapers.UI/ViewModels/UploadViewModel.cs:418-460`
- **Commit**: `d451570`

### 2. Fixed Gemini API Timeout Issue (CRITICAL)
- **Issue**: Gemini API requests were timing out after 30 seconds
- **Root Cause**: PDF processing with Gemini takes longer than 30s for large files (50KB+)
- **Symptoms**: "Operation timeout" error after ~30 seconds of extraction
- **Fix**: Increased HttpClient timeout from 30 seconds to 300 seconds (5 minutes) for all AI providers
- **Files Modified**:
  - `src/EtnoPapers.Core/Services/GeminiService.cs:28-30`
  - `src/EtnoPapers.Core/Services/OpenAIService.cs:25-27`
  - `src/EtnoPapers.Core/Services/AnthropicService.cs:25-27`
- **Commit**: `255869c`

## Testing Instructions

1. **Extract the Application**
   ```
   Option A: Use EtnoPapers.exe from project root
   Option B: Extract EtnoPapers-Portable.zip and run EtnoPapers.UI.exe
   ```

2. **Test the Fixes**
   - Run the application
   - Go to Settings and configure your AI provider (Gemini, OpenAI, or Anthropic)
   - Upload a PDF file (test with 40KB+ files)
   - Verify extraction completes without timeout errors
   - Check that error messages now reference "AI service" instead of OLLAMA

3. **Timeout Behavior**
   - User-facing timeout: 10 minutes (shown in error dialog)
   - API provider timeout: 5 minutes (allows sufficient processing time)
   - Network timeout: 300 seconds per request

## System Requirements

- **OS**: Windows 7 or later (x64)
- **.NET Installation**: NOT required (included in portable version)
- **Internet**: Required for cloud AI providers (Gemini, OpenAI, Anthropic)
- **Disk Space**: ~150 MB for installation + storage for local records

## Troubleshooting

If you encounter timeout issues with large PDFs:

1. Ensure internet connection is stable
2. Check your API provider's status (is their service running normally?)
3. Verify API key is valid in Settings
4. Try with a smaller PDF file to eliminate file size as issue
5. Check application logs in `%APPDATA%\EtnoPapers\logs\`

---

**Build Status**: ✓ Successful
**Build Time**: ~2 minutes
**Warnings**: 54 (all non-critical nullability hints)
**Errors**: 0

Ready for testing!

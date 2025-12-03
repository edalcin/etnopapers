# UI Acceptance Testing Checklist - EtnoPapers

**Version**: 1.0.0
**Date**: 2025-12-03
**Application**: EtnoPapers Desktop (C# WPF)
**Target Platform**: Windows 10+

---

## Overview

This checklist provides comprehensive acceptance criteria for UI testing the EtnoPapers desktop application. All scenarios should be tested in the following configurations:
- **Language**: Portuguese (pt-BR) and English (en-US)
- **Window State**: Both windowed and maximized modes
- **Data Conditions**: With sample records and empty database

---

## Phase 1: Application Startup & Navigation

### T1.1 Application Launch
- [ ] Application starts without errors
- [ ] Main window appears within 2 seconds
- [ ] Window displays correct title "EtnoPapers"
- [ ] Home page loads by default
- [ ] All navigation menu items are visible and clickable

### T1.2 Navigation Between Pages
- [ ] Click "Upload" → UploadPage displays correctly
- [ ] Click "Registros" → RecordsPage displays with record list
- [ ] Click "Sincronização" → SyncPage displays correctly
- [ ] Click "Configurações" → SettingsPage displays correctly
- [ ] Back navigation works smoothly
- [ ] Page content loads within 500ms

### T1.3 Home Page Display
- [ ] Welcome message is displayed
- [ ] Getting started section shows 5 steps
- [ ] Record count displays correctly
- [ ] Connection status shows OLLAMA and MongoDB status
- [ ] Call-to-action buttons ("Iniciar Upload", "Configurar") are visible

---

## Phase 2: PDF Upload & Extraction Workflow

### T2.1 File Selection
- [ ] "Selecionar Arquivo" button opens file dialog
- [ ] File dialog filters for .pdf files correctly
- [ ] PDF file can be selected and path displays in UI
- [ ] Selected file appears with checkmark indicator
- [ ] Invalid file types are rejected with error message

### T2.2 PDF Extraction
- [ ] "Iniciar Extração" button starts extraction
- [ ] Progress bar updates during extraction
- [ ] Current step description updates (PDF loading → text extraction → AI processing → validation)
- [ ] Progress percentage displays correctly
- [ ] Extraction completes successfully
- [ ] Cancel button stops extraction if clicked

### T2.3 Results Editing
- [ ] Extracted metadata displays in form fields:
  - [ ] Título (title)
  - [ ] Autores (authors)
  - [ ] Ano (year)
  - [ ] País (country)
  - [ ] Resumo (abstract)
- [ ] All fields are editable
- [ ] Required fields are marked with asterisk (*)
- [ ] Field validation shows errors for invalid data

### T2.4 Save Results
- [ ] "Salvar Registro" button saves extracted data
- [ ] Success message shows record ID
- [ ] Form clears after successful save
- [ ] Record appears in Records page
- [ ] Error message displays if save fails
- [ ] Duplicate warning appears if title/author match existing record

---

## Phase 3: Record Management

### T3.1 View Records
- [ ] Records page loads with full list of records
- [ ] DataGrid displays columns: Título, Autores, Ano, Bioma, País, ID
- [ ] Records load within 500ms
- [ ] Scrolling through large lists is smooth
- [ ] Record count displays at bottom

### T3.2 Filter Records
- [ ] **Text Search**: Type in search box and press "Aplicar Filtros"
  - [ ] Records filtered by title or author name
  - [ ] Search is case-insensitive
  - [ ] Partial matches work correctly
- [ ] **Year Filter**: Enter year range
  - [ ] Records filtered to year range
  - [ ] Min/max fields work independently
- [ ] **Author Filter**: Enter author name
  - [ ] Records filtered to matching authors
- [ ] **Biome Filter**: Enter biome name
  - [ ] Records filtered correctly
- [ ] "Limpar Filtros" button resets all filters
- [ ] Record count updates after filtering

### T3.3 Edit Record
- [ ] Click record and then "Editar" button
- [ ] Edit dialog opens with record data pre-populated
- [ ] All fields can be edited
- [ ] "Salvar" button saves changes
- [ ] Changes persist when viewing record again
- [ ] Cancel button closes dialog without saving

### T3.4 Create New Record
- [ ] Click "Novo Registro" button
- [ ] New record dialog opens with empty form
- [ ] Required fields show error if left empty on save attempt
- [ ] All fields can be filled in
- [ ] "Criar" button creates record
- [ ] Record appears in list immediately

### T3.5 Delete Records
- [ ] Select one or more records with checkboxes
- [ ] Click "Deletar" button
- [ ] Confirmation dialog appears with count of records to delete
- [ ] Click "Deletar" in confirmation
- [ ] Records are removed from list
- [ ] Deleted records do not reappear on refresh
- [ ] Cancel in confirmation does not delete

---

## Phase 4: MongoDB Synchronization

### T4.1 Sync Configuration
- [ ] MongoDB URI can be entered in Sync page
- [ ] "Testar Conexão MongoDB" button tests connection
- [ ] Status shows "Conectado" (green dot) if successful
- [ ] Status shows "Erro ao conectar" (red dot) if failed
- [ ] Error message explains connection failure

### T4.2 Record Selection for Sync
- [ ] Records page shows sync button
- [ ] Sync reminder appears when >500 records locally
- [ ] Sync reminder can be dismissed
- [ ] Can select individual records in ListBox
- [ ] Selected record count updates

### T4.3 Sync Operation
- [ ] Start sync with selected records
- [ ] Progress bar updates during sync
- [ ] Current status shows "Sincronizando: X/Y"
- [ ] Cancel button stops sync
- [ ] Sync completes successfully
- [ ] "Última sincronização" timestamp updates
- [ ] Records are deleted from local storage after upload
- [ ] Success message shows count of uploaded records

### T4.4 Sync Error Handling
- [ ] If MongoDB not configured, shows clear error
- [ ] If connection fails, displays connection error
- [ ] If record upload fails, shows specific record ID in error
- [ ] Error messages are actionable

---

## Phase 5: Settings & Configuration

### T5.1 OLLAMA Configuration
- [ ] OLLAMA URL field accepts input (default: http://localhost:11434)
- [ ] Model field accepts input (default: llama2)
- [ ] Custom prompt field accepts multi-line text
- [ ] "Testar Conexão OLLAMA" button tests connection
- [ ] Status indicator shows connected (green) or error (red)
- [ ] All changes are retained when clicking "Salvar Configurações"

### T5.2 MongoDB Configuration
- [ ] MongoDB URI field accepts input
- [ ] Example URI format is displayed (mongodb+srv://...)
- [ ] "Testar Conexão MongoDB" button tests connection
- [ ] Status indicator shows connected (green) or error (red)
- [ ] Test without URI shows error message

### T5.3 Application Settings
- [ ] Language dropdown shows options: Portuguese (pt-BR), English (en-US)
- [ ] Selected language persists after save
- [ ] Window width and height can be configured
- [ ] "Abrir aplicação maximizada" checkbox can be toggled
- [ ] Window size is applied on next app launch

### T5.4 Settings Persistence
- [ ] Click "Salvar Configurações"
- [ ] Success message shows "Configurações salvas com sucesso!"
- [ ] Settings persist after application restart
- [ ] "Restaurar Padrões" resets to default values
- [ ] Defaults persist after save

---

## Phase 6: Error Handling & User Experience

### T6.1 Input Validation
- [ ] Empty required fields show validation errors
- [ ] Invalid year values are rejected
- [ ] Invalid email/URI formats show errors
- [ ] Too-long text fields show character limits
- [ ] File size validation prevents files >50MB

### T6.2 Network Errors
- [ ] OLLAMA connection failure shows clear error
- [ ] MongoDB connection failure shows actionable error
- [ ] Network timeouts display user-friendly messages
- [ ] Retry options are available where applicable

### T6.3 Data Loss Prevention
- [ ] Unsaved changes prompt before navigation
- [ ] Delete operations require confirmation
- [ ] No data is lost on application crash
- [ ] Partial uploads are handled gracefully

### T6.4 Loading States
- [ ] Loading spinners appear for long operations
- [ ] Progress bars display for multi-step processes
- [ ] UI remains responsive during background operations
- [ ] "Cancel" options available for long operations

---

## Phase 7: Keyboard & Accessibility

### T7.1 Keyboard Navigation
- [ ] Tab key navigates through all controls
- [ ] Enter key activates buttons
- [ ] Escape key closes dialogs and modals
- [ ] Alt+F4 closes application gracefully
- [ ] Ctrl+S saves settings on SettingsPage
- [ ] Arrow keys navigate lists

### T7.2 Screen Reader Compatibility
- [ ] All buttons have descriptive labels
- [ ] Form fields have associated labels
- [ ] Error messages are announced
- [ ] Status indicators are labeled

### T7.3 Responsive Design
- [ ] UI scales correctly at different DPI settings
- [ ] Text is readable at all zoom levels
- [ ] Controls remain accessible when window is resized
- [ ] Minimal scroll required on 1920x1080 resolution

---

## Phase 8: Localization

### T8.1 Portuguese (pt-BR) Localization
- [ ] All UI text is in Portuguese
- [ ] Date formats use DD/MM/YYYY
- [ ] Currency displays correct symbol
- [ ] Error messages are in Portuguese
- [ ] Help text is clear and accurate

### T8.2 English (en-US) Localization
- [ ] Switch language to English
- [ ] All UI text appears in English
- [ ] Date formats use MM/DD/YYYY
- [ ] Application functions identically
- [ ] Switching back to Portuguese works

---

## Phase 9: Performance & Stability

### T9.1 Startup Performance
- [ ] Application launches within 2 seconds
- [ ] Main window visible within 2 seconds
- [ ] No freezing during startup
- [ ] Memory usage is reasonable (<150MB)

### T9.2 Record Management Performance
- [ ] Sorting 1000 records completes within 200ms
- [ ] Filtering 1000 records completes within 200ms
- [ ] Searching 1000 records completes within 200ms
- [ ] Pagination of 1000 records completes within 200ms
- [ ] UI remains responsive during operations

### T9.3 Long-Running Operations
- [ ] PDF extraction doesn't freeze UI
- [ ] MongoDB sync doesn't freeze UI
- [ ] File operations don't freeze UI
- [ ] Progress is visible during operations

### T9.4 Stability
- [ ] Application doesn't crash on invalid input
- [ ] Application doesn't crash on network errors
- [ ] Application doesn't crash when database is full
- [ ] Application recovers gracefully from errors
- [ ] Repeated operations don't degrade performance

---

## Phase 10: Windows Integration

### T10.1 Window Management
- [ ] Application appears in Windows taskbar
- [ ] Window title shows "EtnoPapers"
- [ ] Window can be resized normally
- [ ] Window can be minimized/maximized
- [ ] Window position and size are remembered

### T10.2 File System Integration
- [ ] File dialogs use Windows native dialogs
- [ ] Drag-and-drop works for PDF files
- [ ] File associations work (optional)
- [ ] Recent files list updates (optional)

### T10.3 System Integration
- [ ] Application appears in Windows Settings
- [ ] Uninstall removes application cleanly
- [ ] File associations can be configured
- [ ] Application handles Windows theme changes

---

## Test Execution Notes

### Test Environment
- **OS**: Windows 10 / Windows 11
- **RAM**: Minimum 4GB
- **Disk Space**: 500MB free
- **OLLAMA**: Must be running on localhost:11434
- **MongoDB**: Optional (for sync testing)

### Test Data
- Use the provided sample data set with 100+ records
- Create test records with various field values
- Test with empty database for baseline

### Sign-Off

| Role | Name | Date | Signature |
|------|------|------|-----------|
| QA Tester | | | |
| Release Manager | | | |

---

## Known Issues & Workarounds

| Issue | Status | Workaround |
|-------|--------|-----------|
| | | |

---

## Test Results Summary

- **Total Test Cases**: 83
- **Passed**: __
- **Failed**: __
- **Skipped**: __
- **Pass Rate**: __%

**Overall Status**: [ ] APPROVED [ ] REJECTED

**Comments**:


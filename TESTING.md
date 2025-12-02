# SM Copy - Testing Guide

This guide provides comprehensive test scenarios to verify SM Copy functionality.

## Prerequisites for Testing

1. Build the project:
   ```
   cd Installer
   build.bat
   ```

2. Install SM Copy (as Administrator):
   ```
   Right-click install.bat → Run as administrator
   ```

3. Restart Windows Explorer (optional but recommended):
   - Press Ctrl+Shift+Esc (Task Manager)
   - Find "Windows Explorer"
   - Right-click → Restart

## Test Scenarios

### Test 1: Basic File Copy

**Objective:** Verify single file copy works correctly

**Steps:**
1. Create a test file (e.g., test.txt) with some content
2. Right-click on the file
3. Verify "SM Copy" option appears in context menu
4. Click "SM Copy"
5. Verify confirmation message appears
6. Navigate to a different folder
7. Right-click in empty space
8. Verify "SM Paste" option appears
9. Click "SM Paste"
10. Verify progress window appears with:
    - Progress bar
    - Speed indicator
    - Time remaining
    - Current file name
11. Verify file is copied successfully
12. Compare source and destination files

**Expected Result:**
- ✓ File copied successfully
- ✓ Progress window shows accurate information
- ✓ Source file remains unchanged
- ✓ Destination file matches source

### Test 2: Basic Folder Copy

**Objective:** Verify folder copy with subdirectories

**Steps:**
1. Create a test folder structure:
   ```
   TestFolder/
   ├── file1.txt
   ├── file2.txt
   └── SubFolder/
       └── file3.txt
   ```
2. Right-click on TestFolder
3. Click "SM Copy"
4. Navigate to destination
5. Right-click and select "SM Paste"
6. Verify entire folder structure is copied

**Expected Result:**
- ✓ Folder and all subdirectories copied
- ✓ All files present in destination
- ✓ Folder structure preserved

### Test 3: Multiple File Selection

**Objective:** Verify multiple files can be copied at once

**Steps:**
1. Create multiple test files (file1.txt, file2.txt, file3.txt)
2. Select all three files (Ctrl+Click or drag-select)
3. Right-click on selection
4. Click "SM Copy"
5. Verify message shows "3 file(s)"
6. Navigate to destination
7. Right-click and "SM Paste"
8. Verify all files are copied

**Expected Result:**
- ✓ All selected files copied
- ✓ Progress shows individual file progress
- ✓ All files arrive at destination

### Test 4: Multiple Folder Selection

**Objective:** Verify multiple folders can be copied

**Steps:**
1. Create 2-3 test folders with files
2. Select all folders
3. Right-click → "SM Copy"
4. Navigate to destination
5. Right-click → "SM Paste"
6. Verify all folders copied

**Expected Result:**
- ✓ All folders and contents copied
- ✓ Folder structures preserved

### Test 5: Mixed Selection (Files + Folders)

**Objective:** Verify mixed selection works

**Steps:**
1. Create test files and folders
2. Select mix of files and folders
3. Copy using "SM Copy"
4. Paste to destination
5. Verify all items copied correctly

**Expected Result:**
- ✓ Both files and folders copied
- ✓ Correct organization maintained

### Test 6: Large File Copy

**Objective:** Test performance with large files

**Steps:**
1. Create or use a large file (500MB+)
2. Copy using "SM Copy"
3. Monitor progress window
4. Verify speed calculations are reasonable
5. Verify ETA updates correctly

**Expected Result:**
- ✓ Copy completes successfully
- ✓ Speed shown in MB/s or GB/s
- ✓ Progress bar updates smoothly
- ✓ ETA is reasonably accurate

### Test 7: Many Small Files

**Objective:** Test with many small files

**Steps:**
1. Create folder with 100+ small files
2. Copy using "SM Copy"
3. Monitor progress
4. Verify completion

**Expected Result:**
- ✓ All files copied
- ✓ No files skipped
- ✓ Performance acceptable

### Test 8: Cancel Operation

**Objective:** Verify cancel functionality works

**Steps:**
1. Start copying a large file or folder
2. Click "Cancel" button during copy
3. Confirm cancellation
4. Verify operation stops
5. Check partial files (may exist, this is robocopy behavior)

**Expected Result:**
- ✓ Operation cancels promptly
- ✓ Application doesn't crash
- ✓ User is informed of cancellation

### Test 9: Path with Special Characters

**Objective:** Test handling of special characters in paths

**Steps:**
1. Create files/folders with names containing:
   - Spaces: "Test File.txt"
   - Special chars: "File (1).txt"
   - Unicode: "Tëst Fîlé.txt"
2. Copy using SM Copy
3. Verify all copy correctly

**Expected Result:**
- ✓ All special characters handled
- ✓ No path errors

### Test 10: Network Drive Copy

**Objective:** Test copying to/from network locations

**Steps:**
1. Map a network drive or use UNC path
2. Copy files to network location
3. Copy files from network location
4. Verify functionality

**Expected Result:**
- ✓ Network paths supported
- ✓ Copy works as expected
- ✓ Progress tracking works

### Test 11: Permission Errors

**Objective:** Test handling of permission denied scenarios

**Steps:**
1. Try to copy a system file or locked file
2. Verify error handling
3. Check that application doesn't crash

**Expected Result:**
- ✓ Error message displayed
- ✓ Other files continue (if multi-file)
- ✓ Application remains stable

### Test 12: Destination Full

**Objective:** Test behavior when destination is full

**Steps:**
1. Try copying to a nearly full drive
2. Verify error handling

**Expected Result:**
- ✓ Appropriate error message
- ✓ No corruption of existing files
- ✓ Application handles gracefully

### Test 13: Overwrite Behavior

**Objective:** Verify what happens with existing files

**Steps:**
1. Copy a file to destination
2. Modify the original file
3. Copy again to same destination
4. Check if file is overwritten

**Expected Result:**
- ✓ File is updated (robocopy default behavior)
- ✓ No data loss

### Test 14: Empty Folder

**Objective:** Test copying empty folders

**Steps:**
1. Create an empty folder
2. Copy using SM Copy
3. Verify folder created at destination

**Expected Result:**
- ✓ Empty folder created (/E flag behavior)

### Test 15: Context Menu Persistence

**Objective:** Verify context menu survives reboot

**Steps:**
1. Install SM Copy
2. Restart computer
3. Verify context menu still appears
4. Test functionality after reboot

**Expected Result:**
- ✓ Context menu persists
- ✓ Functionality works after reboot

### Test 16: Uninstall Clean

**Objective:** Verify clean uninstallation

**Steps:**
1. Run uninstall.bat as administrator
2. Check that context menu entries removed
3. Verify AppData folder cleaned
4. Check registry (advanced users)

**Expected Result:**
- ✓ Context menu entries gone
- ✓ No leftover files in AppData
- ✓ Registry entries removed

## Performance Benchmarks

Compare SM Copy (robocopy) vs Windows native copy:

### Test Files:
- Single 1GB file
- Folder with 1000 small files (1-100KB each)
- Folder with mixed sizes (10GB total)

### Measure:
- Time to complete
- CPU usage
- Memory usage

### Expected:
- SM Copy should be faster or comparable
- Multi-threading should show benefits on large operations

## Known Limitations (Expected Behavior)

1. **Admin Rights**: Registration requires admin, but copying doesn't
2. **Robocopy Exit Codes**: Codes 1-7 are considered success (various levels)
3. **Progress Parsing**: May not be 100% accurate for very fast copies
4. **File Locking**: Can't copy files in use by other programs
5. **Icon**: Placeholder icon until custom icon added

## Logging for Debugging

The progress window shows a log of robocopy output. Check this log if:
- Files are skipped
- Errors occur
- Progress seems incorrect

## Automated Testing Notes

Since this is a Windows desktop application with deep OS integration:
- Manual testing is recommended for context menu integration
- File operations can be unit tested separately
- Consider creating test scripts that exercise the command-line interface:
  ```
  SMCopy.exe --copy "C:\test\file.txt"
  SMCopy.exe --paste "C:\destination"
  ```

## Test Sign-off

### Tester Information
- Name: _______________
- Date: _______________
- Build Version: _______________

### Test Results Summary

| Test # | Test Name | Status | Notes |
|--------|-----------|--------|-------|
| 1 | Basic File Copy | ☐ Pass ☐ Fail | |
| 2 | Basic Folder Copy | ☐ Pass ☐ Fail | |
| 3 | Multiple File Selection | ☐ Pass ☐ Fail | |
| 4 | Multiple Folder Selection | ☐ Pass ☐ Fail | |
| 5 | Mixed Selection | ☐ Pass ☐ Fail | |
| 6 | Large File Copy | ☐ Pass ☐ Fail | |
| 7 | Many Small Files | ☐ Pass ☐ Fail | |
| 8 | Cancel Operation | ☐ Pass ☐ Fail | |
| 9 | Special Characters | ☐ Pass ☐ Fail | |
| 10 | Network Drive | ☐ Pass ☐ Fail | |
| 11 | Permission Errors | ☐ Pass ☐ Fail | |
| 12 | Destination Full | ☐ Pass ☐ Fail | |
| 13 | Overwrite | ☐ Pass ☐ Fail | |
| 14 | Empty Folder | ☐ Pass ☐ Fail | |
| 15 | Context Menu Persistence | ☐ Pass ☐ Fail | |
| 16 | Uninstall Clean | ☐ Pass ☐ Fail | |

### Overall Assessment

- All Critical Tests Passed: ☐ Yes ☐ No
- Ready for Production: ☐ Yes ☐ No
- Issues Found: _______________

## Bug Reporting

If you find issues during testing, please report:
1. Test scenario number
2. Steps to reproduce
3. Expected vs actual behavior
4. Screenshots (if applicable)
5. System information (Windows version, .NET version)


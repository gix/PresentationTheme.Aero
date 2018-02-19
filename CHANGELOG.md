# Changelog
All notable changes to this project will be documented in this file.

## [0.2.0] - 2018-02-23
### Added
- Windows 8/8.1 Aero, Aero Lite and High Contrast themes

### Fixed
- Theme assemblies (e.g., PresentationTheme.Aero.Win10.dll) are now copied
  automatically to the output directory as if they were directly referenced in
  the project.
- Aero10: Toolbar ComboBox border renders properly over the dropdown button
- Aero10: Calendar Today cell uses the proper border
- Aero10: The focus rectangle in AeroLite and HighContrast was 1px too high.
- Aero10: Aero and AeroLite had the TabItem focus rectangle start with a gap
          instead of a filled pixel.
- HighContrast10: Fix Explorer.TreeViewItem border on Windows 1709+

## [0.1.6444.1133] - 2017-08-23

Initial release.

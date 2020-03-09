# Changelog
All notable changes to this project will be documented in this file.

## [0.6.1] - 2020-03-09
### Fixed
- ListViewItem using ThemeOptions.UseExplorerStyle now correctly honors
  horizontal alignment.

## [0.6.0] - 2020-01-25
### Fixed
- NuGet package metadata references proper source revision.
- Publishing a project referencing the PresentationTheme.Aero NuGet package
  no longer fails due to duplicate theme assemblies in the list of inputs.
- Top-level menu items are now also painted as pressed when focused with the
  keyboard (e.g. after pressing Alt to reach the menu).

## [0.5.0] - 2019-08-12
### Fixed
- Themes: ThemeManager support for .NET 4.8

### Added
- Themes: Support for .NET Core (3.0 preview 7)
- Source Link for PDB symbols
- UxThemeEx: New UxOpenThemeDataForDpi function

### Changed
- Tools are built with .NET Core

## [0.4.0] - 2018-09-02
### Added
- All themes: Proper TabControl rendering for non-standard TabStripPlacement.

### Fixed
- Aero10: A lone TabItem's height would be 1px smaller than with adjacent tabs.

## [0.3.0] - 2018-09-01
### Fixed
- All themes: TreeView Background now properly extends to its Border. Previously
  a non-zero Padding on a TreeView would leave a gap.
- ThemePreviewer: Do not crash when using unknown/newer versions of uxtheme.dll.
  Attempt to load internal addresses using debug symbols.

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

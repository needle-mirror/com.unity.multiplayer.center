# Changelog

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0-pre.2] - 2024-08-01

### Changed
- Widgets are now recommended for Netcode for Entities.

## [1.0.0-pre.1] - 2024-07-29

### Fixed
- Quickstart Tab: Show HelpBox if no quickstart package is installed but Quickstart Tab is visible. 

## [0.4.0] - 2024-07-19

### Added
- Added Recommendation for Distributed Authority hosting model with Netcode for GameObjects.
- Undo support for Recommendation tab.
- Added status Label at the bottom for package related information. 

### Changed
- Refined the Netcode Recommendation.
- The Quickstart tab is deactivated if no multiplayer-related package is installed.

## [0.3.0] - 2024-06-06

### Added
- Added analytics to the package

### Changed
- **New user interface for the recommendations tab**
- Multiplayer Center no longer upgrades packages that are embedded, linked locally, installed via Git or local Tarball
- Window can now be found under `Window > Multiplayer > Multiplayer Center`
- Updated the recommendation of the Widgets package (only compatible with Netcode for GameObjects at the moment)
- Updated the tooltips and various texts in the recommendations tab 

## [0.2.1] - 2024-04-25

Non user-facing changes only

## [0.2.0] - 2024-04-24

**Added**

- Automatic installation of the Getting Started content.
- Changed API for the getting started content.

## [0.1.0] - 2024-04-22

This is the first release of *com.unity.multiplayer.center*. The package provides a new Window available at `Window > Multiplayer Center` that gives a starting point to create a multiplayer game. 

The window consists of two tabs: `Recommendations` and `Getting Started`. 
- The `Recommendations` tab provides a customized list of packages and solutions to use based on the characteristics of your multiplayer game. 
- The `Getting Started` tab provides a list of resources based on the packages that you have installed.

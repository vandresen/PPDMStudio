# Changelog
 
All notable changes to PPDMStudio will be documented in this file.
 
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
 
---
 
## [1.1.0] - 2026-05-24
 
### Added
 
- **Assigned Field** — field is now displayed in the well grid and editable in the well header panel, backed by the PPDM `FIELD` reference table
### Fixed
 
- **County display** — well grid now shows the county name instead of the FIPS code
- **MudBlazor analyzer warnings** — replaced illegal `Title` attributes on `MudIconButton` with `MudTooltip` wrappers across `WellHeaderPanel`, `WellLocationTab`, and `ReferenceSelect`
- **MudBlazor analyzer warning** — removed illegal `PanelClass` attribute from `MudTabs`; padding moved to individual `MudTabPanel` elements
- **Null reference warning** — added null guard on `args.Item` in `OnRowClick`
---
 
## [1.0.0] - 2026-05-23
 
Initial release.
 
### Added
 
- **Project management** — connect to and switch between multiple PPDM SQL Server databases
- **Well grid** — search and filter wells, multi-select, and save results as named well lists
- **Well header editing** — tabbed panel for identification, dates, depths, elevations, and remarks
- **Location management** — cascading state and county autocomplete backed by the PPDM Area hierarchy
- **Reference table management** — add new operators, statuses, profile types, states, and counties without leaving the panel
- **Audit trail** — all inserts and updates record created/changed user and date
- **Well lists** — save, restore, and delete named sets of wells across sessions
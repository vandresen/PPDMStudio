# PPDM Studio
 
A modern desktop application for viewing and editing **PPDM (Professional Petroleum Data Management)** data, built with .NET 9, WPF, Blazor Hybrid, MudBlazor and Dapper.
 
![.NET 9](https://img.shields.io/badge/.NET-9.0-purple)
![Platform](https://img.shields.io/badge/Platform-Windows-blue)
![License](https://img.shields.io/badge/License-MIT-green)
 
## Features
- 🗂️ **Project management** — manage multiple PPDM database connections with secure credential storage
- 🔍 **Well search and filtering** — filter by UWI, Well Name, Operator, Field, County and State
- 📋 **Well lists** — create, save and manage named sets of wells, supports 100,000+ wells
- 💾 **Remembers your workspace** — last selected project and well list restored on startup
- 🔒 **Secure** — passwords stored in Windows Credential Manager, never in plain text
- 📝 **Well header editing** — edit identification, dates, depths, elevations, and remarks
- 📍 **Location management** — cascading state and county selection backed by the PPDM Area hierarchy
- ➕ **Reference table management** — add new operators, statuses, profile types, states, and counties without leaving the panel
- 🏗️ **Built on PPDM 3.9 standard**
## Technology Stack
- .NET 9 WPF + Blazor Hybrid
- MudBlazor UI components
- Dapper for data access (no Entity Framework)
- SQLite for local workspace storage
- Windows Credential Manager for secure password storage
## Requirements
- Windows 10/11
- SQL Server with a PPDM 3.9 database
## Quick Start (Pre-built)
1. Download `PPDMStudio.zip` from the [latest release](../../releases/latest)
2. Extract all files to a folder e.g. `C:\PPDMStudio\`
3. Run `PPDMStudio.exe`
4. Keep all files together in the same folder
## Getting Started (Build from Source)
1. Install [Visual Studio 2022](https://visualstudio.microsoft.com/) with the **.NET Multi-platform App UI development** workload
2. Clone the repository
```bash
git clone https://github.com/vandresen/PPDMStudio.git
```
3. Open `PPDMStudio.sln` in Visual Studio 2022
4. Build and run
5. Create a project pointing to your PPDM database
## Roadmap
- [x] Well header editing
- [ ] Directional surveys
- [ ] Formation marker picks
- [ ] Log viewer
- [ ] Cross sections
- [ ] Map view
- [ ] LAS file import
- [ ] CSV import
## Contributing
Contributions are welcome! Please feel free to submit a Pull Request.
 
## License
This project is licensed under the MIT License.
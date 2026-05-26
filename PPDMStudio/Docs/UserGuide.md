# PPDMStudio User Guide
 
PPDMStudio is a Windows desktop application for viewing and editing well data stored in a PPDM 3.9 SQL Server database.
 
---
 
## Table of Contents
 
- [Setting Up a Project](#setting-up-a-project)
- [Working with Wells](#working-with-wells)
- [Well Lists](#well-lists)
- [Importing Wells from a CSV File](#importing-wells-from-a-csv-file)
- [Editing a Well Header](#editing-a-well-header)
- [Managing Location Data](#managing-location-data)
- [Adding Reference Values](#adding-reference-values)
- [Data and Audit Trail](#data-and-audit-trail)

---
 
## Setting Up a Project
 
A **project** is a connection to a PPDM SQL Server database. You can have multiple projects pointing to different databases.
 
1. Click **Project Manager** in the navigation menu
2. Click **New Project**
3. Enter a project name, server name, and database name
4. Choose Windows Authentication or SQL Server Authentication
5. If using SQL Server Authentication, enter your username and password — these are stored securely in **Windows Credential Manager** and never saved in plain text
6. Click **Test Connection** to verify, then **Save**

Once saved, select your project from the project selector at the top of the screen. PPDMStudio remembers your last selected project on startup.
 
---
 
## Working with Wells
 
The **Wells** page is the main screen. Use the **Filters** button on the right to search for wells by:
 
| Filter | Notes |
|---|---|
| UWI | Exact match |
| Well Name | Supports `%` wildcard (e.g. `Permian%`) |
| Operator | Supports `%` wildcard |
| Field | Supports `%` wildcard |
| County | Supports `%` wildcard |
| State | Supports `%` wildcard (e.g. `TX`) |
 
Click **Search** to load results. The well count is shown as a chip next to the well list selector.
 
Click any row to open the **Well Header Panel** on the right side of the screen.
 
---
 
## Well Lists
 
Well lists let you save and reuse named sets of wells across sessions.
 
**Saving a well list:**
- Click the **Save** icon next to the well list selector to save all currently filtered wells
- Check individual rows in the grid and click **Save Selected Wells** to save only those wells

**Loading a well list:**
- Select a well list from the dropdown at the top of the Wells page
- PPDMStudio remembers your last selected well list per project on startup

**Deleting a well list:**
- Select the list from the dropdown, then click the **Delete** icon
- This only removes the list — it does not affect any well data in the database

> Well lists support 100,000+ wells using batch loading for performance.
 
---

## Importing Wells from a CSV File

PPDMStudio can import well data from a comma-delimited (`.csv`) file directly into your PPDM database.

Click the **Import CSV** button on the Wells page toolbar to open the import wizard.

### Step 1: Choose a File

Click **Choose CSV File** and select a `.csv` file from your computer. The file must have a header row as the first line. PPDMStudio supports files up to 50 MB.

Once loaded, the number of rows detected is shown below the file name.

### Step 2: Map Columns

Map each PPDM field to the corresponding column in your CSV file using the dropdowns. PPDMStudio will attempt to auto-map columns whose names exactly match PPDM field names.

| Field | Notes |
|---|---|
| UWI | **Required.** All other fields are optional. |
| WELL_NAME | Well name |
| OPERATOR | Operator name |
| ASSIGNED_FIELD | Field name |
| CURRENT_STATUS | Well status |
| SPUD_DATE | Accepts standard date formats |
| SURFACE_LATITUDE / LONGITUDE | Decimal degrees |
| BOTTOM_HOLE_LATITUDE / LONGITUDE | Decimal degrees |
| FINAL_TD, LOG_TD, DRILL_TD, MAX_TVD | Depth values |
| KB_ELEV, GROUND_ELEV | Elevation values |
| DEPTH_DATUM | Depth datum reference |
| STATE | Must match an existing state code in the database |
| COUNTY | Must match an existing county code in the database |
| REMARK | Free text notes |

Fields set to **(skip)** are ignored during import.

### Step 3: Preview & Import

A preview of the first 20 rows is shown based on your column mappings. The header also reports how many duplicate UWIs were detected in the file — these will be skipped.

Optionally check **Add imported wells to a Well List** to automatically add the newly inserted wells to the currently open Well List. If no Well List is open, you will be prompted to name a new one.

Click **Import** to begin. When complete, a summary snackbar reports how many wells were inserted and how many were skipped because they already existed in the database.

> **Note:** CSV import writes only to the `WELL` table and `WELL_AREA` (for state and county). It follows the same audit trail rules as all other PPDMStudio operations — see [Data and Audit Trail](#data-and-audit-trail).

> **Note:** Wells that already exist in the database (matched by UWI) are silently skipped and left unchanged. They are not overwritten.

---
 
## Editing a Well Header
 
Click any well row to open the **Well Header Panel**. The panel has two tabs:
 
### Well Header Tab
 
| Section | Fields |
|---|---|
| Identification | Well name, operator, current status, profile type |
| Dates | Spud, final drill, completion, abandonment |
| Depths | Final TD, log TD, drill TD, max TVD |
| Elevations | Depth datum, KB elevation, ground elevation |
| Remarks | Free text notes |
 
Click any field to edit it. Changes are not saved until you click the **Save** button (💾) in the panel header.
 
The **Cancel** button (✕) discards all unsaved changes and restores the original values.
 
### Location Tab
 
See [Managing Location Data](#managing-location-data) below.
 
---
 
## Managing Location Data
 
The **Location** tab on the Well Header Panel manages the well's geographic assignment and coordinates.
 
### State
 
Click the **State** field to activate a search box. Start typing the state name — results appear as you type. Click a result to select it.
 
> If the state you need does not exist in the database, click the **+** button next to the state field to add it. You will need to provide a state code (e.g. `TX`) and a full name (e.g. `Texas`).
 
### County
 
County is only available after a state has been selected. Click the **County** field and start typing — results are filtered to counties within the selected state.
 
> If the county you need does not exist, click the **+** button next to the county field to add it. County IDs use **FIPS codes** — a 5-digit numeric code that uniquely identifies each US county (e.g. `48113` for Dallas County, Texas). You can look up FIPS codes at [census.gov](https://www.census.gov/library/reference/code-lists/ansi.html).
 
> **Note:** Changing the state automatically clears the county selection.
 
### Coordinates
 
Enter surface and bottom hole latitude/longitude directly in the coordinate fields.
 
---
 
## Adding Reference Values
 
Several fields in the Well Header tab have a **+** button that lets you add new values directly to the PPDM reference tables without leaving the panel. This includes:
 
| Field | Table written to |
|---|---|
| Operator | `BUSINESS_ASSOCIATE` |
| Current Status | `R_WELL_STATUS` |
| Profile Type | `R_WELL_PROFILE_TYPE` |
| State | `AREA` + `R_AREA_TYPE` |
| County | `AREA` + `AREA_CONTAIN` + `R_AREA_TYPE` |
 
> **Important:** These additions write directly and immediately to your PPDM database. They do not require a separate save step and cannot be undone from within PPDMStudio. Ensure you have the correct permissions before adding reference values.
 
---
 
## Data and Audit Trail
 
PPDMStudio follows the PPDM standard for data provenance. Every insert and update records:
 
| Column | Value |
|---|---|
| `ROW_CREATED_BY` | Your Windows username |
| `ROW_CREATED_DATE` | Date and time of insert (UTC) |
| `ROW_CHANGED_BY` | Your Windows username |
| `ROW_CHANGED_DATE` | Date and time of last update (UTC) |
 
This means all changes made through PPDMStudio are fully traceable in the database by user and timestamp.
 
---
 
## Getting Help
 
For issues or feature requests, please open an issue on [GitHub](https://github.com/vandresen/PPDMStudio/issues).
# SolarWise - prototype notes

Run: open `SolarWise.slnx` in Visual Studio and press F5.

Demo accounts (sample data):
- `ilham@example.com` / `ilham1234` (user, member of 3 organizations)
- `admin@example.com` / `admin1234` (administrator)

## Scope

Built: login, registration, forgot password, organization selection, main window (menu, navigation, status bar), home dashboard, locations, solar panel catalog (admin edits), solar PV calculator (3 steps), calculation results with a 10-year savings chart, calculation history and details, recommendations, weather data (honest "unavailable" state), user management (admin), organizations (prototype), settings, UI component library.

Not built (from CLAUDE.md, for time reasons): organization settings, organization members and invitations, PDF report generation, a map picker, and window resizing/maximize (layouts are fixed-size).

## Differences from the ERD and class diagram

1. **Organizations** are not in the ERD. They are a prototype: an in-memory `Organization` and `OrgMembership` (join table). Calculations carry an `OrganizationID` so history is scoped by organization. Labelled PROTOTYPE in the UI.
2. **Location coordinates** are strings, as in the class diagram. Numeric types would be better for calculations. Values are validated on entry (-90..90, -180..180).
3. **SolarPanel `Description`** appears twice in the class diagram. It is modelled once.
4. **Location** has `OwnerUserID` from the ERD (user to locations). In code it is named `SiteLocation` to avoid clashing with WinForms `Control.Location`.
5. **Weather irradiance** is stored as a daily total (kWh/m²/day). The spec suggested W/m²; the daily total is what the energy formula needs.
6. **Calculated values not in the ERD**: panel count, system capacity (kWp), irradiance used, the "sample" flag. They are shown as calculated values, not stored as model attributes.
7. **Recommendation** links to one calculation (ERD: `calculation_id`). The class diagram's Admin-to-Recommendation "views" relationship is implemented as admins seeing the same org-scoped list as users.
8. **PricePerMeter** unit is not defined. Shown as IDR/m, marked "unit to confirm".
9. **Passwords** are stored as SHA-256 hashes with no salt. This is a prototype only; production needs a salted, slow hash (for example ASP.NET Identity or BCrypt).
10. **Payback period and installation cost** are not calculated: the ERD has no cost model.

## Formulas and assumptions (`Core/SolarCalculator.cs`)

- Panel area = Wattage / (1000 x efficiency), at 1000 W/m².
- Panel count = floor(input area / panel area).
- System kWp = panels x Wp / 1000.
- Annual output (kWh) = kWp x irradiance (kWh/m²/day) x 365 x 0.80 performance ratio.
- Annual saving (IDR) = output x Rp 1.444,70/kWh. **Assumed tariff: confirm against the current PLN tariff for the customer class.**
- Irradiance = average of saved weather records for the location; default 4.8 kWh/m²/day (sample) when none exist. Results are marked as sample.
- Projection: output degrades 0.5 % per year.

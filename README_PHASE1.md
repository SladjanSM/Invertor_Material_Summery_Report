# SM.MaterialSummary – FAZA 1 (skeleton)

## Sadržaj
- `src/StandardAddInServer.cs` – Inventor add-in server (dugme u Tools tabu, otvara WinForms).
- `src/MaterialSummaryForm.cs` – WinForms UI sa 6 kolona (X, Naziv, Verzija, Količina, Path, Status).
- `src/SM.MaterialSummary.Addin.csproj` – .NET Framework 4.7 class library (Codex/VS će dodati Inventor COM).
- `deploy/SM.MaterialSummary.addin` – registracija add-ina (ispraviti `<Assembly>` putanju nakon build-a).

## Build (Codex / Visual Studio)
1. Otvoriti solution/projekat i dodati **COM referencu**: *Autodesk Inventor Object Library* (Inventor 2020).  
   - Tipično: `C:\Program Files\Autodesk\Inventor 2020\Bin\Inventor.tlb`
2. Build **Release** → dobićete `SM.MaterialSummary.Addin.dll`.

## Deploy za test (FAZA 1)
1. Kopirati `deploy/SM.MaterialSummary.addin` u:
   `%APPDATA%\Autodesk\Inventor Addins\`
2. U `.addin` fajlu postaviti `<Assembly>` na punu putanju do build-ovanog `.dll` (npr. vaš `bin\Release\SM.MaterialSummary.Addin.dll`).

## Verifikacija u Inventoru
- Pokrenuti Inventor 2020 → **Tools → Add-ins** → proveriti da je add-in Loaded.
- U **Tools** tabu pojaviće se panel **SM Furniture AI** sa dugmetom **Material Summary**.
- Klik → otvara se prazan WinForms prozor (FAZA 1), radi:
  - Add Assembly (dodaje .iam redove, Qty=1),
  - X briše red,
  - Run Analysis je stub (FAZA 2 dodaje logiku).

## GUID
- `CE2E4C3C-1A1B-4F7C-9A0E-9F6D4E3D1B77` (mora biti isti u kodu i u `.addin`).

## Sledeće
- FAZA 2: validacija, čitanje RevisionNumber/Status, rekurzija po subassembly-ima/partovima, kalkulacija m², PDF/XLSX export.
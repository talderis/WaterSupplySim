# WaterSupplySimulator

Ez a projekt egy modern, biztonságos, jól strukturált .NET 8 backendből és egy Vite + React + TypeScript frontendből álló vízellátás-szimulátor rendszer. A cél egy fejlesztőbarát, reszponzív, sötét-kékes témájú admin felület, amely REST API-n keresztül kommunikál a backenddel, JWT alapú admin jogosultsággal.

## Fő funkciók

### Backend (.NET 8, C#)
- **REST API** szenzoradatok, szivattyú vezérlés, riasztások, eseménynapló
- **JWT alapú admin bejelentkezés** (jelszó: `SuperSecretKey12345`)
- **Repository, Service, DTO, AutoMapper minták**
- **Globális exception middleware**, validáció, hibakezelés
- **CORS, HTTPS fejlesztői támogatás**
- **Rate limiting**, API versioning
- **Swagger UI** JWT támogatással

### Frontend (Vite + React + TypeScript)
- **Sötét, kékes, reszponzív UI** (modern táblázatok, kártyák, menü)
- **Szenzor adatok** mindenki számára elérhető
- **Admin funkciók** (szivattyú vezérlés, riasztás visszaigazolás, napló/CSV letöltés) csak bejelentkezve
- **JWT token kezelés** (localStorage, axios interceptor)
- **Fejlesztői HTTPS támogatás** (mkcert tanúsítvány)

## Indítás

### Backend
1. .NET 8 SDK telepítése szükséges
2. Futtatás fejlesztői módban:
   ```
   dotnet run
   ```
3. Swagger UI: [https://localhost:5001/swagger](https://localhost:5001/swagger)

### Frontend
1. Node.js + pnpm telepítése szükséges
2. mkcert-tel generált tanúsítványokat helyezd el a `frontend/` mappában (`localhost.pem`, `localhost-key.pem`)
3. Telepítés és indítás:
   ```
   cd frontend
   pnpm install
   pnpm dev
   ```
4. Elérés: [https://localhost:5173](https://localhost:5173)

## Admin jelszó

```
admin123
```

## Fő könyvtárak/fájlok
- Backend: `Program.cs`, `Controllers/`, `Services/`, `DTOs/`, `Mapping/`, `Middleware/`
- Frontend: `frontend/src/App.tsx`, `frontend/src/App.css`, `frontend/src/api.ts`, `frontend/vite.config.ts`

## Megjegyzések
- A szenzor adatok mindenki számára elérhetők.
- Az admin funkciók (szivattyú, riasztás, napló, CSV letöltés) csak bejelentkezve használhatók.
- A fejlesztői HTTPS tanúsítványokat mkcert-tel kell generálni.
- A projekt fejlesztői környezetre optimalizált, best practice architektúrával.

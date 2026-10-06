# Gülora — ajanlar için referans

Bu dosya Cursor ve otomasyon ajanları için **kısa giriş**tir. Ayrıntılı standartlar `.cursor/rules/` altındaki `.mdc` dosyalarındadır.

## Teknoloji özeti

| Alan | Seçim |
|------|--------|
| Frontend | React 19, Vite 8, TypeScript, Tailwind CSS v4, React Router v7 |
| Backend | ASP.NET Core 9 Web API, Clean Architecture, PostgreSQL |
| Auth | JWT (kısa access) + refresh (DB), BCrypt |

## Kod konumu

- **Alıcı web:** `frontend/store/` — REST: `frontend/store/src/services/api.ts` (port 5173)
- **Satıcı paneli:** `frontend/saller/` — REST: `frontend/saller/src/services/api.ts` (port 5174)
- **Mobil vitrin (Expo):** `mobile/gulora-store/` — `mobile/gulora-store/src/api/guloraApi.ts`
- **Backend:** `backend/src/Gulora.*` — testler `backend/tests/`

## API URL

- Yerel backend: `http://localhost:5284/api/v1/...`
- Vite proxy: `/api` → `5284` (`frontend/store/vite.config.ts`, `frontend/saller/vite.config.ts`)

## Kural haritası

| Dosya | Amaç |
|------|------|
| [gulora-architecture.mdc](.cursor/rules/gulora-architecture.mdc) | Stack, bağımlılık, CI özeti |
| [gulora-api-controllers.mdc](.cursor/rules/gulora-api-controllers.mdc) | Controller, HTTP, CORS |
| [gulora-frontend-api.mdc](.cursor/rules/gulora-frontend-api.mdc) | `api.ts`, token, proxy |
| [gulora-devops-ci.mdc](.cursor/rules/gulora-devops-ci.mdc) | CI/CD, Docker |
| [gulora-tests.mdc](.cursor/rules/gulora-tests.mdc) | xUnit + Vitest stratejisi |

## Hızlı kontrol

- Controller ince katman → MediatR (`gulora-api-controllers.mdc`)
- UI'da doğrudan `fetch` yok — yalnızca `api.ts` / `guloraApi.ts`
- JWT anahtarları: mağaza `gulora_store_*`, satıcı `gulora_seller_*` (ayrı localStorage)
- Sırlar repoda sabit metin değildir (`gulora-security.mdc`)

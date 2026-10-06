# Gülora

Çiçək və hədiyyə e-ticarət platforması — ASP.NET Core 9 API, React 19 vitrinlər, Expo mobil.

## Layihə quruluşu

| Qovluq | Təsvir | Port |
|--------|--------|------|
| `backend/` | Clean Architecture API | 5284 (HTTP) |
| `frontend/store/` | Alıcı web mağazası | 5173 |
| `frontend/saller/` | Satıcı paneli | 5174 |
| `mobile/gulora-store/` | Expo mobil mağaza | 8081 |

## Yerli işə salma

### 1. PostgreSQL + API (Docker)

```bash
docker compose up -d
```

API: `http://localhost:5284` — `/health`, `/swagger` (Development).

### 2. API (lokal dotnet)

```bash
dotnet user-secrets set "Jwt:SigningKey" "YOUR_32_CHAR_MIN_SIGNING_KEY_HERE" --project backend/src/Gulora.API
dotnet run --project backend/src/Gulora.API --launch-profile http
```

### 3. Web mağaza

```bash
cd frontend/store && npm ci && npm run dev
```

Vite proxy: `/api` → `http://localhost:5284`.

### 4. Satıcı paneli

```bash
cd frontend/saller && npm ci && npm run dev
```

## Sənədlər

- [AGENTS.md](AGENTS.md) — ajanlar üçün qısa istinad
- `.cursor/rules/` — kod standartları

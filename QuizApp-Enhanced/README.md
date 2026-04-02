# ⚡ QuizZap — Angular 21 Quiz Application

A complete Angular **21** quiz application with dark cosmic UI, signal-based state, JWT auth, and full API integration.

---

## 🚀 Quick Start

### Prerequisites
- Node.js 22+
- Angular CLI 21+  (`npm install -g @angular/cli@21`)
- Backend running at `http://localhost:5137`

### Install & Run

```bash
cd QuizApp
npm install
ng serve
# → http://localhost:4200
```

---

## 📁 Structure

```
src/app/
├── app.ts / app.html / app.css       Root component
├── app.config.ts                     Angular 21 providers
├── app.routes.ts                     Lazy-loaded standalone routes
│
├── components/navbar/                Auth-aware navbar
├── start-page/                       Landing page
├── login/                            Login page
├── register/                         Register with role selector
├── dashboard/                        Quiz browser + leaderboard preview
├── quiz/                             Quiz play (signal timer)
├── attempt/                          Result page with score ring
├── leaderboard/                      Podium + full rankings
├── profile/                          User profile editor
├── user-details/                     Extended address details
├── question/quiz-create              Creator quiz builder
├── category/category-manage          Creator category CRUD
│
├── service/                          7 HttpClient services (inject())
├── guards/                           authGuard · creatorGuard · guestGuard
├── interceptor/                      JWT interceptor (HttpInterceptorFn)
└── models/models.ts                  All TypeScript interfaces & DTOs
```

---

## ⚡ Angular 21 Features Used

| Feature | Where Used |
|---|---|
| `signal()` / `computed()` | Auth state, quiz state, timer, UI toggles |
| `inject()` (no constructors) | All services, components, guards |
| `@if` / `@for` control flow | Every template |
| `provideZoneChangeDetection({ eventCoalescing })` | app.config.ts |
| `withFetch()` | HttpClient — uses native Fetch API |
| `withComponentInputBinding()` | Router — route params via `@Input` |
| `withViewTransitions()` | Smooth page transitions |
| `UrlTree` returns from guards | Type-safe redirects |
| Standalone components | All 13 components |
| `HttpInterceptorFn` | Functional interceptor |
| Lazy `loadComponent()` | All routes |

---

## 🔐 Auth Flow

1. Register / Login → JWT stored in `localStorage`
2. `jwtInterceptor` → attaches `Authorization: Bearer <token>`
3. `authGuard` → redirects unauthenticated users (returns `UrlTree`)
4. `creatorGuard` → restricts admin routes to `Creator` role
5. `guestGuard` → redirects already-logged-in users away from auth pages

---

## 🌐 API Base URL

All services point to `http://localhost:5137`. Change in each service file if needed.
The `proxy.conf.json` forwards `/api/*` to the backend during `ng serve`.

---

## 🧪 Testing

```bash
ng test
```

22 spec files covering all components, services, guards, and interceptor.

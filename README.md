# OAuth 2.0 and OpenID Connect Lab

A hands-on authentication and authorization lab using a React single-page application, Keycloak as a locally hosted identity provider and authorization server, and a protected ASP.NET Core Web API.

The goal is to understand the protocol flow and the responsibilities of each component—not just to add a login button:

```text
React SPA -- Authorization Code + PKCE --> Keycloak
    |                                         |
    |<----------- ID token + access token ----|
    |
    | Authorization: Bearer <access token>
    v
ASP.NET Core API -- validates signature, issuer, and audience
    |
    +-- [Authorize] and role authorization
```

## What this lab demonstrates

- OAuth 2.0 as an authorization framework and OpenID Connect as its identity layer.
- A browser-based React app registered as a **public client** (no client secret).
- Authorization Code Flow with PKCE (`S256`) for interactive sign-in.
- The difference between ID tokens and access tokens.
- API-side JWT validation, including issuer and `dotnet-api` audience checks.
- Claims-based authentication and realm-role authorization.
- The difference between `401 Unauthorized` and `403 Forbidden`.

## Technologies

- React 19 and Vite
- Keycloak (`keycloak-js`) running in Docker Compose
- ASP.NET Core Web API targeting .NET 10

## Prerequisites

- Docker with the Docker Compose v2 plugin
- .NET 10 SDK
- Node.js 20.19+ or 22.12+, with npm

## 1. Start Keycloak

From the repository root:

```powershell
docker compose up -d
```

Wait for Keycloak to finish starting, then open [http://localhost:8080](http://localhost:8080). The local administration console credentials configured in `docker-compose.yml` are:

```text
Username: admin
Password: admin
```

These credentials and the Keycloak development-mode configuration are for local learning only.

## 2. Configure the Keycloak realm and clients

Keycloak configuration is currently created manually in the admin console; it is not imported automatically from this repository.

### Create a realm and test user

1. Create a realm named `fullstack-lab`.
2. In that realm, create a user (the examples below use `testuser`).
3. Set a password for the user and turn off **Temporary** for the password.

### Register the React SPA

Under **Clients**, create an OpenID Connect client:

| Setting | Value |
| --- | --- |
| Client ID | `react-spa` |
| Client authentication | Off |
| Standard flow | On |
| Valid redirect URIs | `http://localhost:5173/*` |
| Web origins | `http://localhost:5173` |

The SPA is a public client and must not have a client secret. The existing frontend configuration expects the Keycloak URL `http://localhost:8080`, realm `fullstack-lab`, and client ID `react-spa`.

### Register the protected API and audience

1. Create another OpenID Connect client with ID `dotnet-api`; turn **Client authentication** on. This client represents the API and is not used by the browser.
2. Create an OpenID Connect client scope named `api-access`.
3. Add an **Audience** mapper to the scope:
   - **Included Client Audience:** `dotnet-api`
   - **Add to access token:** On
4. Assign `api-access` to `react-spa` as a **Default** client scope.

The access token issued to the SPA must contain `dotnet-api` in its `aud` claim. The API is configured to reject tokens intended for a different audience.

### Configure the Admin role (optional)

To exercise role-based authorization, create the realm role `Admin`. Initially leave it unassigned to `testuser`; later, assign it under **Users → testuser → Role mapping** and sign in again to obtain a token containing the updated role.

## 3. Run the ASP.NET Core API

From the repository root, in a terminal:

```powershell
dotnet dev-certs https --trust
dotnet run --project .\Backend\Backend.csproj --launch-profile https
```

The API listens at:

- HTTPS: `https://localhost:7118`
- HTTP: `http://localhost:5081`

The HTTPS development certificate must be trusted by your browser for the React app to call the API over HTTPS. The API's local configuration allows requests from the Vite origin `http://localhost:5173`.

## 4. Run the React app

In a second terminal:

```powershell
Set-Location .\frontend
npm ci
npm run dev
```

Open [http://localhost:5173](http://localhost:5173). The app initiates sign-in automatically using Authorization Code Flow with PKCE. After logging in, use **Call Protected API** and **Call Admin API**; responses and status codes are written to the browser console.

## Try the authorization flow

With a valid login and no `Admin` role:

| Request | Expected result | Why |
| --- | --- | --- |
| `GET /api/profile` | `200 OK` | Any authenticated user can access this endpoint. |
| `GET /api/profile/admin` | `403 Forbidden` | The user is authenticated but lacks the `Admin` role. |

Assign the realm role `Admin` to `testuser`, log out, then sign in again. The admin endpoint should now return `200 OK`.

Without an access token, either protected endpoint returns `401 Unauthorized`. In short:

- **401:** authentication is missing or failed (for example, no token, invalid token, or expired token).
- **403:** authentication succeeded, but the user is not authorized for that resource.

The API endpoints are implemented in [`Backend/Controllers/ProfileController.cs`](./Backend/Controllers/ProfileController.cs). JWT validation, CORS, and Keycloak realm-role claim mapping are configured in [`Backend/Program.cs`](./Backend/Program.cs).

## Project layout

```text
.
├── Backend/                 ASP.NET Core API
│   ├── Controllers/
│   └── Program.cs
├── frontend/                React + Vite SPA
│   └── src/
│       ├── keycloak.js      Keycloak realm and client configuration
│       ├── main.jsx         Keycloak initialization and PKCE
│       └── App.jsx          Profile and admin API calls
├── docker-compose.yml       Local Keycloak container
└── reference.md             Detailed learning notes and walkthrough
```

## Local development notes

- Keycloak is configured for development over HTTP, and the API disables HTTPS metadata requirements for its local Keycloak authority. Do not use these settings as production defaults.
- `docker-compose.yml` uses the `latest` Keycloak image and bootstrap credentials `admin`/`admin`; pin and secure these for any shared or deployed environment.
- This Compose setup does not configure persistent Keycloak storage. `docker compose down` removes the container and its local realm data; use `docker compose stop` when you want to stop Keycloak without removing that container.
- The current React entry point logs token values to the browser console for learning. Treat tokens as credentials and do not share them; remove token logging before using this pattern beyond a private local lab.
- The backend and frontend contain local URLs and ports. If you change them, keep the Keycloak client redirect URI and web origins, backend CORS origin, JWT authority, and frontend API URLs in sync.

To stop Keycloak while preserving its container data:

```powershell
docker compose stop
```

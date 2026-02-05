<div align="center">
  <a href="https://wristband.dev">
    <picture>
      <img src="https://assets.wristband.dev/images/email_branding_logo_v1.png" alt="Github" width="297" height="64">
    </picture>
  </a>
  <p align="center">
    Enterprise-ready auth that is secure by default, truly multi-tenant, and ungated for small businesses.
  </p>
  <p align="center">
    <b>
      <a href="https://wristband.dev">Website</a> •
      <a href="https://docs.wristband.dev">Documentation</a>
    </b>
  </p>
</div>

<br/>

---

# Wristband Multi-Tenant Demo App for ASP.NET Core (C#)

This demo app consists of:

- **C# Backend**: A C# backend with Wristband authentication integration
- **React Frontend**: A React frontend with authentication context

When an unauthenticated user attempts to access the frontend, it will redirect to the C# backend's Login Endpoint, which in turn redirects the user to Wristband to authenticate. Wristband then redirects the user back to your C#'s Callback Endpoint which sets a session cookie before returning the user's browser to the React frontend.

> [!NOTE]
> This repo uses Microsoft Aspire to launch the C# ASP.NET Core Backend API project, frontend project, and a YARP (Yet Another Reverse Proxy) to expose the frontend and backend projects as a single endpoint to eliminate the need for CORS.

<br>

---

<br>

## Requirements

This demo app requires .NET SDK 8, 9, or 10. If you don't have any of these installed, you can download and install from the official .NET website:
1. Visit [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download).
2. Download and run the .NET SDK installer for your operating system (version 8.0, 9.0, or 10.0).
3. Verify the installation by opening a terminal or command prompt and running:
```bash
dotnet --version # Should show 8.x.x or higher
```

Additionally, the React frontend's Vite server requires Node.js version 20 or higher with `npm`. To install:
1. Visit [https://nodejs.org](https://nodejs.org).
2. Download and run the installer for the LTS version (which should be v20.x or higher).
3. Verify the installation by opening a terminal or command prompt and running:
```bash
node --version   # Should show v20.x.x or higher
```

<br>

---

<br>

## Getting Started

You can start up the demo application in a few simple steps.

### 1) Sign up for a Wristband account.

First, make sure you sign up for a Wristband account at [https://wristband.dev](https://wristband.dev).

### 2) Provision the .NET/C# demo application in the Wristband Dashboard.

After your Wristband account is set up, log in to the Wristband dashboard.  Once you land on the home page of the dashboard, click the "Add Application" button.  Make sure you choose the following options:

- Step 1: Try a Demo
- Step 2: Subject to Authenticate - Humans
- Step 3: Application Framework - ASP.NET (C#) Backend, React Frontend

You can also follow the [Demo App Guide](https://docs.wristband.dev/docs/setting-up-a-demo-app) for more information.

### 3) Apply your Wristband configuration values to the C# server configuration

After completing demo app creation, you will be prompted with values that you should use to create environment variables for the C# server. You should see:

- `APPLICATION_VANITY_DOMAIN`
- `CLIENT_ID`
- `CLIENT_SECRET`

Copy those values, then create an environment variable file on the server at: `<project_root_dir>/aspnet-backend/Apps.Api/.env`. Once created, paste the copied values into this file.

### 4) Install dependencies

Before attempting to run the application, you'll need to install all project dependencies in both C# and React.

#### .NET dependencies

From the root directory of this repo, run the following commands to install dependencies and build all C# projects:

```bash
dotnet restore
dotnet build
```

#### React dependencies

Navigate into React project directory where the `package.json` file is located and install all dependencies:

```bash
cd ./react-frontend/clientapp
npm install
```

Once done, you can navigate back to the root directory to run the application.

### 5) Run the application

> [!WARNING]
> Make sure you are in the root directory of this repository.

With Microsoft Aspire, you will launch both the C# backend AND the desired frontend simultaneously. YARP (Yet Another Reverse Proxy) will expose the frontend and backend projects on port `6001`. To run from the command line, you can use the following command from this project's root directory:

```bash
dotnet run --project ./react-frontend/AppHost/
```

For debugging, using either Visual Studio or Rider, launch the AppHost project using the Aspire launch configuration. The [Microsoft Aspire dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/overview) is located at `http://localhost:15043` where you have access to logs and traces.

<br>

---

<br>

## How to interact with the demo app

### Home Page

For reference, the home page of this demo app can be accessed at [http://localhost:6001](http://localhost:6001).

### Signup Users

Now that the demo app is up and running, you can sign up your first customer on the Signup Page at the following location:

- `https://{application_vanity_domain}/signup`, where `{application_vanity_domain}` should be replaced with the value of the "Application Vanity Domain" value of the application (can be found in the Wristband Dashboard by clicking the Application Settings side nav menu).

This signup page is hosted by Wristband.  Completing the signup form will provision both a new tenant with the specified tenant domain name and a new user that is assigned to that tenant.

### Application-level Login (Tenant Discovery)

Users of this app can access the Application-level Login Page at the following location:

- `https://{application_vanity_domain}/login`, where `{application_vanity_domain}` should be replaced with the value of the "Application Vanity Domain" value of the application (can be found in the Wristband Dashboard by clicking the Application Settings side nav menu).

This login page is hosted by Wristband.  Here, the user will be prompted to enter either their email or their tenant's domain name.  Doing so will redirect the user to the Tenant-level Login Page for their specific tenant.

### Tenant-level Login

If users wish to directly access the Tenant-level Login Page without having to go through the Application-level Login Page, they can do so at the following locations:

- Localhost domain format: [http://localhost:6001/api/auth/login?tenant_name={tenant_name}](http://localhost:6001/api/auth/login?tenant_name={tenant_name}), where `{tenant_name}` should be replaced with the value of the desired tenant's domain name.

This login page is hosted by Wristband.  Here, the user will be prompted to enter their credentials in order to login to the application.

### Architecture

This demo app utilizes the [Backend for Frontend (BFF) pattern](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-browser-based-apps#name-backend-for-frontend-bff).  The C# server is responsible for:

- Storing the client ID and secret.
- Handling the OAuth2 authorization code flow redirections to and from Wristband during user login.
- Creating the application session cookie to be sent back to the browser upon successful login.  The application session contains the access and refresh tokens as well as some basic user info.
- Refreshing the access token if the access token is expired.
- Orchestrating all API calls from the frontend to Wristband and other downstream API calls.
- Destroying the application session and revoking the refresh token when a user logs out.

API calls made from the frontend to C# pass along the application session cookie and a [CSRF token](https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html#double-submit-cookie) with every request.  The server has middlewares for all protected routes that are responsbile for:

- Validating and refreshing the access token (if necessary)
- "Touching" the application session cookie to extend session expiration
- Validating the CSRF token

---

<br/>

## Wristband ASP.NET Auth SDK

This demo app is leveraging the [Wristband aspnet-auth SDK](https://github.com/wristband-dev/aspnet-auth) for all authentication interaction in the C# server. Refer to that GitHub repository for more information.

## Wristband React Client Auth SDK

This demo app is leveraging the [Wristband react-client-auth SDK](https://github.com/wristband-dev/react-client-auth) for any authenticated session interaction in the React frontend. Refer to that GitHub repository for more information.

## Questions

Reach out to the Wristband team at <support@wristband.dev> for any questions regarding this demo app.

<br/>

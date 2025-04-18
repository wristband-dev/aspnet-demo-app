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

# Invotastic for Business (C#) -- A multi-tenant demo app

This repo contains a simple Hello World example demonstrating how to use the Wristband Auth SDK with a C# backend and a React Vite frontend. The repo uses Microsoft Aspire to launch the C# Asp.Net Core Backend API project, frontend project, and a YARP (Yet Another Reverse Proxy) to expose the frontend and backend projects as a single endpoint to eliminate the need for CORS.

When an unauthenticated user attempts to access the frontend, it will redirect to the C# backend's Login Endpoint, which in turn redirects the user to Wristband to authenticate. Wristband then redirects the user back to your application's Callback Endpoint which sets a session cookie before returning the user's browser to the frontend project. The frontend does not need any secrets or other special handling since all authentication/authorization is handled by your C# server

<br>
<hr />
<br>

## Requirements

This demo app requires .NET SDK 8 for the C# server that runs. If you don't have it installed already, you can download and install it from the official .NET website:
1. Visit [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Download and run the .NET 8 SDK installer for your operating system.
3. Verify the installation by opening a terminal or command prompt and running:
```bash
dotnet --version # Should show 8.0.x or higher
```

Additionally, the React frontend requires Node.js version 20 or higher with `npm`. To install:
1. Visit [https://nodejs.org](https://nodejs.org).
2. Download and run the installer for the LTS version (which should be v20.x or higher).
3. Verify the installation by opening a terminal or command prompt and running:
```bash
node --version   # Should show v20.x.x or higher
npm --version    # Should show v10.x.x or higher (for Node 20+)
```

## Getting Started

You can start up the Invotastic demo application in a few simple steps.

### 1) Sign up for a Wristband account.

First, make sure you sign up for a Wristband account at [https://wristband.dev](https://wristband.dev).

### 2) Provision the .NET/C# demo application in the Wristband Dashboard.

After your Wristband account is set up, log in to the Wristband dashboard.  Once you land on the home page of the dashboard, click the button labelled "Add Demo App".  Make sure you choose the following options:

- Step 1: Subject to Authenticate - Humans
- Step 2: Client Framework - ASP.NET / C#
- Step 3: Domain Format  - Choosing `Localhost` is fastest to setup. You can alternatively choose `Vanity Domain` if you want a production-like experience on your local machine for tenant-specific vanity domains, but this method will require additional setup.

You can also follow the [Demo App Guide](https://docs.wristband.dev/docs/setting-up-a-demo-app) for more information.
 
> **Using Vanity Domains**
> If you’re using vanity domains for URLs, check the suggestions later in this README for configuring them to work with your local development server.

### 3) Apply your Wristband configuration values to the C# server configuration

After completing demo app creation, you will be prompted with values that you should use to create environment variables for the C# server. You should see:

- `APPLICATION_DOMAIN`
- `CLIENT_ID`
- `CLIENT_SECRET`
- `DOMAIN_FORMAT`

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
<hr>
<br>

## How to interact with Invotastic for Business

### Signup Invotastic Users

Now that the Invotastic demo app is up and running, you can sign up your first customer on the Signup Page at the following location:

- `https://{application_vanity_domain}/signup`, where `{application_vanity_domain}` should be replaced with the value of the "Application Vanity Domain" value of the Invotastic application (can be found in the Wristband Dashboard by clicking the Application Settings side nav menu).

This signup page is hosted by Wristband.  Completing the signup form will provision both a new tenant with the specified tenant domain name and a new user that is assigned to that tenant.

### Invotastic Home Page

For reference, the home page of this Invotastic demo app can be accessed at the following locations:

- Localhost domain format: [http://localhost:6001/home](http://localhost:6001/home)
- Vanity domain format: [http://{tenant_domain}.business.invotastic.com:6001/home](http://{tenant_domain}.business.invotastic.com:6001/home), where `{tenant_domain}` should be replaced with the value of the desired tenant's domain name.

### Invotastic Application-level Login (Tenant Discovery)

Users of Invotastic can access the Invotastic Application-level Login Page at the following location:

- `https://{application_vanity_domain}/login`, where `{application_vanity_domain}` should be replaced with the value of the "Application Vanity Domain" value of the Invotastic application (can be found in the Wristband Dashboard by clicking the Application Settings side nav menu).

This login page is hosted by Wristband.  Here, the user will be prompted to enter either their email or their tenant's domain name.  Doing so will redirect the user to the Tenant-level Login Page for their specific tenant.

### Invotastic Tenant-level Login

If users wish to directly access the Invotastic Tenant-level Login Page without having to go through the Application-level Login Page, they can do so at the following locations:

- Localhost domain format: [http://localhost:6001/api/auth/login?tenant_domain={tenant_domain}](http://localhost:6001/home), where `{tenant_domain}` should be replaced with the value of the desired tenant's domain name.
- Vanity domain format: [http://{tenant_domain}.business.invotastic.com:6001/api/auth/login](http://{tenant_domain}.business.invotastic.com:6001/api/auth/login), where `{tenant_domain}` should be replaced with the value of the desired tenant's domain name.

This login page is hosted by Wristband.  Here, the user will be prompted to enter their credentials in order to login to the application.

### RBAC

When a new user signs up their company, they are assigned the "Owner" role by default and have full access to their company resources.  Owners of a company can also invite new users into their company.  Invited users can be assigned either the "Owner" role or the "Viewer" role.

### Architecture

This Invotastic for Business demo app utilizes the [Backend for Frontend (BFF) pattern](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-browser-based-apps#name-backend-for-frontend-bff).  The C# server is responsible for:

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

### Wristband Code Touchpoints

Within the demo app code, you can search in your IDE of choice for the text `WRISTBAND_TOUCHPOINT`.  This will show various places in frontend code and C# backend code where Wristband is involved.

<br>
<hr />
<br/>

## Setting up a local DNS when using `VANITY_DOMAIN` for the domain format

If you choose to use vanity domains as the domain format for the demo application, you will need to either install a local DNS server to provide custom configurations or rely on a cloud-hosted solution.  This configuration forces any requests made to domains ending with `.business.invotastic.com` to get routed to your localhost.  This configuration is necessary since all vanity domains that get generated when running the demo application locally will have a domain suffix of  `*.business.invotastic.com`. Therefore, the above setting will force those domains to resolve back to your local machine instead of attempting to route them out to the web.

The goal is the following mapping:
`business.invotastic.com` => `127.0.0.1`.

Here are some options:

- Mac / Linux: [dnsmasq](http://mayakron.altervista.org/support/acrylic/Home.htm)
- Windows: [Acrylic](http://mayakron.altervista.org/support/acrylic/Home.htm)
- Cloudflare Tunnel: You can use a free Cloudflare tunnel to access your local development server via a publicly accessible URL with a valid SSL certificate (provided automatically by Cloudflare). Details on setting up the tunnel are found in the [Cloudflare README](./Wristband/README-Cloudflare-Tunnel.md) in root of this repository.

<br>

## Wristband ASP.NET Auth SDK

This demo app is leveraging the [Wristband aspnet-auth SDK](https://github.com/wristband-dev/aspnet-auth) for all authentication interaction in the C# server. Refer to that GitHub repository for more information.

## Wristband React Client Auth SDK

This demo app is leveraging the [Wristband react-client-auth SDK](https://github.com/wristband-dev/react-client-auth) for any authenticated session interaction in the React frontend. Refer to that GitHub repository for more information.

## CSRF Protection

Cross-Site Request Forgery (CSRF) is a security vulnerability where attackers exploit a user's authenticated session to perform unauthorized actions on a web application without their knowledge or consent. This demo app is leveraging a technique called the Double Cookie Submit Pattern to mitigate CSRF attacks by employing two cookies: a session cookie for user authentication and a CSRF token cookie containing a unique token. With each request, the CSRF token is included both in the cookie and the request payload, enabling server-side validation to prevent CSRF attacks.

Refer to the [OWASP CSRF Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html) for more information about this topic.

> [!WARNING]
> Your own application should take effort to mitigate CSRF attacks in addition to any Wristband authentication, and it is highly recommended to take a similar approach as this demo app to protect against thse types of attacks.

<br/>

## Questions

Reach out to the Wristband team at <support@wristband.dev> for any questions regarding this demo app.

<br/>

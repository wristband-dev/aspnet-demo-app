## Add Wristband to your Blazor WebAssembly Frontend

1. Copy the 5 wristband files from the `Sample-Wristband-Blazor/Apps.Blazor` folder into your frontend project
- `CustomAuthenticationLocalStorageService.cs`
- `CustomAuthenticationStateProvider.cs`
- `CustomAuthenticationUser.cs`
- `RedirectToWristbandLogin.razor`
- `RedirectToWristbandLogout.razor`
2. Add the following nuget packages to your frontend project .csproj file
```xml
...
<ItemGroup>
  ...
  <PackageReference Include="Microsoft.AspNetCore.Components.Authorization" Version="8.0.10" />
  <PackageReference Include="Microsoft.AspNetCore.WebUtilities" Version="8.0.10" />
  <PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
  ...
</ItemGroup>
...
```

3. In your App.razor file, wrap the `<Router AppAssembly="@typeof(App).Assembly">` component with:
```razor
@using Microsoft.AspNetCore.Components.Authorization

<CascadingAuthenticationState>
    ...
</CascadingAuthenticationState>
```
4. Surround the content in MainLayout.razor with
```razor
@using Microsoft.AspNetCore.Components.Authorization

@inherits LayoutComponentBase
<AuthorizeView>
    <Authorized>
        ...
    </Authorized>
    <NotAuthorized>
        <RedirectToWristbandLogin></RedirectToWristbandLogin>
    </NotAuthorized>
</AuthorizeView>
```
5. Add the following usings to Program.cs
```csharp
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Wristband;
```
6. Add the following services code to Program.cs
```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
...
var services = builder.Services;
...
services.AddHttpClient();
services.AddAuthorizationCore();
services.AddCascadingAuthenticationState();
services.AddSingleton<CustomAuthenticationLocalStorageService>();
services.AddSingleton<CustomAuthenticationStateProvider>();
services.AddSingleton<AuthenticationStateProvider>(
    s => s.GetRequiredService<CustomAuthenticationStateProvider>());
```

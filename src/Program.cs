using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using NetTopologySuite.IO.Converters;
using PriMap.Components;
using PriMap.Components.Account;
using PriMap.Data;
using PriMap.Data.Spatial;
using PriMap.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddSingleton<CoordinateReprojectionService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PriMap.Services.FeatureService>();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new GeoJsonConverterFactory()));

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AppPolicies.CanView, p => p.RequireRole(AppRoles.Viewer, AppRoles.Editor, AppRoles.Analyst, AppRoles.Admin))
    .AddPolicy(AppPolicies.CanEdit, p => p.RequireRole(AppRoles.Editor, AppRoles.Analyst, AppRoles.Admin))
    .AddPolicy(AppPolicies.CanAnalyze, p => p.RequireRole(AppRoles.Analyst, AppRoles.Admin))
    .AddPolicy(AppPolicies.CanManage, p => p.RequireRole(AppRoles.Admin));

var connectionString = builder.Configuration.GetConnectionString("SqlConnection") ?? throw new InvalidOperationException("Nu am găsit 'SqlConnection' ca și string de conexiune.");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.UseNetTopologySuite()));builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<PriMapSignInManager>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await RoleSeeder.SeedAsync(scope.ServiceProvider);
    await CategorySeeder.SeedAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.MapFeatureEndpoints();

app.Run();

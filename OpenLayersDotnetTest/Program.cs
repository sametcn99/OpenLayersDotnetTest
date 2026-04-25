using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenLayersDotnetTest.Configuration;
using OpenLayersDotnetTest.Components;
using OpenLayersDotnetTest.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services
    .AddDataProtection(options => options.ApplicationDiscriminator = "OpenLayersDotnetTest");

builder.Services
    .AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Database connection string is required.")
    .ValidateOnStart();

builder.Services.AddDbContext<OpenLayersDbContext>((serviceProvider, options) =>
{
    var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

    options.UseNpgsql(databaseOptions.ConnectionString, npgsqlOptions =>
    {
        npgsqlOptions.UseNetTopologySuite();
    });
});

builder.Services.AddScoped<IMapFeatureSlugUniquenessChecker, MapFeatureSlugUniquenessChecker>();
builder.Services.AddScoped<IMapFeatureQueryService, PostgisMapFeatureQueryService>();
builder.Services.AddScoped<IMapFeatureCommandService, PostgisMapFeatureCommandService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.MapOpenApi();
app.MapGet("/scalar", () => Results.Redirect("/scalar/v1", permanent: false))
    .ExcludeFromDescription();
app.MapScalarApiReference("/scalar");
app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

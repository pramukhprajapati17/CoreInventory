using Microsoft.AspNetCore.HttpOverrides;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
builder.WebHost.UseUrls("http://0.0.0.0:10000");
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Repositories.Interfaces.IUserInterface>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new Repositories.Repositories.IUserInterface_repo(connectionString ?? string.Empty);
});
builder.Services.AddScoped<Repositories.Interfaces.IUomInterface>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new Repositories.Repositories.IUomInterface_repo(connectionString ?? string.Empty);
});
builder.Services.AddScoped<Repositories.Interfaces.IProductCategoryInterface>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new Repositories.Repositories.IProductCategoryInterface_repo(connectionString ?? string.Empty);
});
builder.Services.AddScoped<Repositories.Interfaces.IProductInterface>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new Repositories.Repositories.IProductInterface_repo(connectionString ?? string.Empty);
});
builder.Services.AddScoped<Repositories.Interfaces.IWarehouseInterface>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new Repositories.Repositories.IWarehouseInterface_repo(connectionString ?? string.Empty);
});
builder.Services.AddScoped<Repositories.Interfaces.ILocationInterface>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new Repositories.Repositories.ILocationInterface_repo(connectionString ?? string.Empty);
});
builder.Services.AddScoped<Repositories.Interfaces.ISupplierInterface>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new Repositories.Repositories.ISupplierInterface_repo(connectionString ?? string.Empty);
});
builder.Services.AddScoped<Repositories.Interfaces.ICustomerInterface>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new Repositories.Repositories.ICustomerInterface_repo(connectionString ?? string.Empty);
});
builder.Services.AddScoped<Repositories.Interfaces.IReceiptInterface>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new Repositories.Repositories.IReceiptInterface_repo(connectionString ?? string.Empty);
});
builder.Services.AddScoped<Repositories.Interfaces.IDeliveryInterface>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new Repositories.Repositories.IDeliveryInterface_repo(connectionString ?? string.Empty);
});
builder.Services.AddScoped<Repositories.Interfaces.ITransferInterface>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new Repositories.Repositories.ITransferInterface_repo(connectionString ?? string.Empty);
});
builder.Services.AddScoped<Repositories.Interfaces.IAdjustmentInterface>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new Repositories.Repositories.IAdjustmentInterface_repo(connectionString ?? string.Empty);
});
builder.Services.AddScoped<Repositories.Interfaces.IStockLedgerInterface>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new Repositories.Repositories.IStockLedgerInterface_repo(connectionString ?? string.Empty);
});
builder.Services.AddScoped<Repositories.Interfaces.IReorderRuleInterface>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new Repositories.Repositories.IReorderRuleInterface_repo(connectionString ?? string.Empty);
});
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await DatabaseConnectivityCheckAsync(app);

await app.RunAsync();

static async Task DatabaseConnectivityCheckAsync(WebApplication app)
{
    var connectionString = app.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        app.Logger.LogWarning("DefaultConnection is not set. Skipping database connectivity check.");
        return;
    }

    try
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cts.Token);
        app.Logger.LogInformation("Database connectivity check succeeded.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database connectivity check failed.");
    }
}

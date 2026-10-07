using BKMC.Pharmacy.Web.Services;

var builder = WebApplication.CreateBuilder(args);

//builder.AddServiceDefaults();

// Add Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();



// Register HttpClient for BKMC Pharmacy API
builder.Services.AddHttpClient("BKMC.Pharmacy.API", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:BaseUrl"]
        ?? throw new InvalidOperationException(
            "API Base URL is not configured."));
})
.AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddHttpClient<ReceivingApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl!);
})
.AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddHttpClient<SupplierApiService>(client =>
{
    var baseUrl =
        builder.Configuration["ApiSettings:BaseUrl"];

    client.BaseAddress = new Uri(baseUrl!);
})
.AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddHttpClient<BatchApiService>(client =>
{
    var baseUrl =
        builder.Configuration["ApiSettings:BaseUrl"];

    client.BaseAddress = new Uri(baseUrl!);
})
.AddHttpMessageHandler<JwtAuthorizationHandler>();



builder.Services.AddScoped<DashboardApiService>();
builder.Services.AddScoped<MedicineApiService>();
builder.Services.AddScoped<MedicineApiService>();
builder.Services.AddHttpClient<ReceivingApiService>();
builder.Services.AddScoped<StockApiService>();
builder.Services.AddScoped<StockTransferApiService>();
builder.Services.AddScoped<StockAdjustmentApiService>();
builder.Services.AddScoped<PatientApiService>();
builder.Services.AddScoped<DispensingApiService>();
builder.Services.AddScoped<BillApiService>();
builder.Services.AddScoped<ReturnApiService>();
builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<JwtAuthorizationHandler>();



var app = builder.Build();

//app.MapDefaultEndpoints();

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();



app.MapRazorPages();

app.Run();
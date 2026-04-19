using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.CamelCase;
        });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Göztepe Olympics API",
        Version = "v1",
        Description = "Fetches Göztepe Sports Club Olympics data " +
                      "from İzmir Voleybol and Basketbol İl Temsilciliği.",
        Contact = new OpenApiContact
        {
            Name = "Göztepe S.K."
        }
    });

    // XML comment dosyasını dahil et
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddHttpClient("FixtureClient", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    client.DefaultRequestHeaders.Add("Referer",
        $"{VolleyballConstants.BaseUrl}/Fiksturler");
    client.DefaultRequestHeaders.Add("Accept", "*/*");
    client.BaseAddress = new Uri(VolleyballConstants.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(VolleyballConstants.Timeout);
});

builder.Services.AddHttpClient("StandingsClient", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    client.DefaultRequestHeaders.Add("Referer",
        $"{VolleyballConstants.BaseUrl}/PuanDurumu");
    client.DefaultRequestHeaders.Add("Accept", "*/*");
    client.BaseAddress = new Uri(VolleyballConstants.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(VolleyballConstants.Timeout);
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<FixtureCacheService>();
builder.Services.AddScoped<FixtureScraperService>();
builder.Services.AddSingleton<IStandingsCacheService, StandingsCacheService>();
builder.Services.AddScoped<IStandingsScraperService, StandingsScraperService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Swagger UI — tüm ortamlarda açık (production'da kısıtlamak istersen if bloğuna al)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Olympics API v1");
    options.RoutePrefix = "swagger"; // → https://localhost:PORT/swagger
    options.DocumentTitle = "Göztepe Olympics API";
    options.DefaultModelsExpandDepth(-1); // Model şemalarını kapalı başlat
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");
app.Run();
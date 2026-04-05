using VolleyballScraper.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

// Register HttpClient
builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient("VolleyballClient", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    client.DefaultRequestHeaders.Add("Referer",
        "https://izmir.voleyboliltemsilciligi.com/Fiksturler");
    client.DefaultRequestHeaders.Add("Accept", "*/*");
    client.BaseAddress = new Uri("https://izmir.voleyboliltemsilciligi.com/");
});

// Register our Scraper Service
builder.Services.AddScoped<VolleyballScraperService>();
builder.Services.AddSingleton<GameCacheService>();

// Enable CORS (Since your Vue app will run on a different port)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();

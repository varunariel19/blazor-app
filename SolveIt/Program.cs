using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qdrant.Client;
using SolveIt.Components;
using SolveIt.Data;
using SolveIt.Hubs;
using SolveIt.Models;
using SolveIt.Services;
using SolveIt.UI_state;
using Microsoft.AspNetCore.ResponseCompression;



var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddSingleton<QdrantClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["Qdrant:ApiKey"]!;
    return new QdrantClient(
        host: "af95c35a-873f-4154-9ef0-342b927f81be.us-west-1-0.aws.cloud.qdrant.io",
        https: true,
        apiKey: apiKey
    );
});

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
    })
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
    });

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024;
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<UiStateService>();
builder.Services.AddScoped<QuestionService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<VectorService>();
builder.Services.AddScoped<EmbeddingService>();
builder.Services.AddScoped<ConversationService>();
builder.Services.AddScoped<QuestionInteractionService>();

builder.Services.AddHttpClient<EmbeddingService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseResponseCompression();
app.MapHub<ChatHub>("/chathub");
app.MapControllers();
app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[MIDDLEWARE ERROR] {ex}");
        throw;
    }
});

app.Run();
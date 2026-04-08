using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SolveIt.Models;
using SolveIt.Data;
using SolveIt.Components;
using SolveIt.UI_state;
using SolveIt.Services;
using Qdrant.Client;


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

builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });

builder.Services.AddScoped<UiStateService>();
builder.Services.AddScoped<QuestionService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<VectorService>();
builder.Services.AddScoped<EmbeddingService>();
builder.Services.AddHttpClient<EmbeddingService>();


builder.Services.AddScoped<QuestionInteractionService>();


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

app.MapControllers();
app.MapRazorPages();       
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
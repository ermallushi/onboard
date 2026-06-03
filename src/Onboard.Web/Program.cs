using System.Reflection;
using Onboard.Web.Application;
using Onboard.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DecisionRulesOptions>(builder.Configuration.GetSection("DecisionRules"));
builder.Services.AddSingleton<InMemoryOnboardingStore>();
builder.Services.AddSingleton<IIdentityProviderAdapter, MockIdentityProviderAdapter>();
builder.Services.AddSingleton<IBiometricProviderAdapter, MockBiometricProviderAdapter>();
builder.Services.AddSingleton<IRiskProviderAdapter, MockRiskProviderAdapter>();
builder.Services.AddSingleton<ISignatureProviderAdapter, MockSignatureProviderAdapter>();
builder.Services.AddSingleton<ICallbackSecurityService, CallbackSecurityService>();
builder.Services.AddSingleton<IOnboardingOrchestrator, OnboardingOrchestrator>();

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Onboarding Platform API",
        Version = "v1",
        Description = "Modular customer onboarding platform — digital and in-store journeys, KYC, biometrics, risk screening, e-signature, and audit."
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Seed sample data
using (var scope = app.Services.CreateScope())
{
    var orchestrator = scope.ServiceProvider.GetRequiredService<IOnboardingOrchestrator>();
    SampleDataSeeder.Seed(orchestrator);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Onboarding Platform API v1");
    c.DocumentTitle = "Onboarding Platform – API Explorer";
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program;


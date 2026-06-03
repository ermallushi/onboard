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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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

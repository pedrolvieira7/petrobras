using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PetrobrasContext");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure ConnectionStrings__PetrobrasContext nas variáveis do Railway com a conexão PostgreSQL do Supabase.");
}

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<PetrobrasContext>(options =>
    options.UseNpgsql(connectionString, pg => pg.EnableRetryOnFailure(3)));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Acesso/Login";
        options.AccessDeniedPath = "/Acesso/Login";
        options.Cookie.Name = "SIAF.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Sem [AllowAnonymous], a página exige login (inclusive as rotas MVC antigas).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Executar UMA vez com SIAF_INITIALIZE_DATABASE=true, somente na implantação inicial.
// O Supabase já possui tabelas internas, portanto EnsureCreated() NÃO é apropriado.
if (app.Configuration.GetValue<bool>("SIAF_INITIALIZE_DATABASE"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<PetrobrasContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await db.Database.OpenConnectionAsync();
    try
    {
        await using var check = db.Database.GetDbConnection().CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'siaf' AND table_type = 'BASE TABLE'";
        var count = Convert.ToInt32(await check.ExecuteScalarAsync());
        if (count == 0)
        {
            // Cria apenas as tabelas pertencentes ao modelo do SIAF, no schema privado siaf.
            await db.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();
            logger.LogInformation("Tabelas do SIAF criadas no esquema siaf.");
        }
        else if (count != 10)
        {
            throw new InvalidOperationException(
                "Esquema siaf parcialmente criado. Verifique as tabelas antes de reiniciar a inicialização.");
        }

        if (!await db.Perfils.AnyAsync())
        {
            db.Perfils.AddRange(
                new Perfil { Nome = "Administrador", Descricao = "Administração do SIAF" },
                new Perfil { Nome = "Analista", Descricao = "Análises de falhas" },
                new Perfil { Nome = "Gestor", Descricao = "Gestão operacional" },
                new Perfil { Nome = "Operador", Descricao = "Registro de ocorrências" }
            );
            await db.SaveChangesAsync();
        }
    }
    finally
    {
        await db.Database.CloseConnectionAsync();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

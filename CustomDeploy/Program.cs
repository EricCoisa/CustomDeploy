using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using CustomDeploy.Models;
using CustomDeploy.Services;
using CustomDeploy.Services.Business;
using CustomDeploy.Data;
using CustomDeploy.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Configure Entity Framework with SQLite
builder.Services.AddDbContext<CustomDeployDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=customdeploy.db;Cache=Shared";
    options.UseSqlite(connectionString);
});

// Registrar Repositórios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAcessoNivelRepository, AcessoNivelRepository>();
builder.Services.AddScoped<IDeployRepository, DeployRepository>();

// Registrar Serviços de Negócio
builder.Services.AddScoped<IUsuarioBusinessService, UsuarioBusinessService>();
builder.Services.AddScoped<IDeployBusinessService, DeployBusinessService>();

// Registrar o DeployService (legacy)
builder.Services.AddScoped<DeployService>();

// Registrar o PublicationService
builder.Services.AddScoped<PublicationService>();

// Registrar o IISManagementService
builder.Services.AddScoped<IISManagementService>();

// Registrar o GitHubService
builder.Services.AddScoped<GitHubService>();
builder.Services.AddHttpClient<GitHubService>();

// Registrar o AdministratorService
builder.Services.AddScoped<AdministratorService>();

// Registrar o FileManagerService
builder.Services.AddScoped<IFileManagerService, FileManagerService>();

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

// Validar se as configurações JWT estão presentes
if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    throw new InvalidOperationException("JWT configuration is missing or invalid. Please check your appsettings.json file.");
}

// Configure GitHub Settings
builder.Services.Configure<GitHubSettings>(builder.Configuration.GetSection("GitHubSettings"));

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configure CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CustomDeploy API",
        Version = "v1",
        Description = "API para deploy automático de aplicações do GitHub"
    });

    // Configure JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CustomDeployDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        await SeedDatabaseAsync(context, scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao inicializar banco de dados");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CustomDeploy API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
    });
    app.UseCors(); // CORS apenas em desenvolvimento
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Método para popular o banco de dados com dados iniciais
static async Task SeedDatabaseAsync(CustomDeployDbContext context, IServiceProvider serviceProvider)
{
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Verificar se já existem dados
        if (await context.Usuarios.AnyAsync())
        {
            logger.LogInformation("Banco de dados já possui dados. Seed ignorado.");
            return;
        }

        logger.LogInformation("Iniciando seed do banco de dados...");

        // Criar níveis de acesso se não existirem
        if (!await context.AcessoNiveis.AnyAsync())
        {
            var acessoNiveis = new[]
            {
                new CustomDeploy.Models.Entities.AcessoNivel { Id = 1, Nome = "Administrador" },
                new CustomDeploy.Models.Entities.AcessoNivel { Id = 2, Nome = "Operador" }
            };

            context.AcessoNiveis.AddRange(acessoNiveis);
            await context.SaveChangesAsync();
            logger.LogInformation("Níveis de acesso criados com sucesso.");
        }

        // Criar usuário administrador padrão
        var usuarioBusinessService = serviceProvider.GetRequiredService<IUsuarioBusinessService>();
        
        var adminUser = await usuarioBusinessService.CriarUsuarioAsync(
            "Administrador",
            "admin@customdeploy.com",
            "password",
            1 // Administrador
        );

        logger.LogInformation("Usuário administrador criado: {Email}", adminUser.Email);
        logger.LogInformation("Seed do banco de dados concluído com sucesso.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro durante o seed do banco de dados");
        throw;
    }
}

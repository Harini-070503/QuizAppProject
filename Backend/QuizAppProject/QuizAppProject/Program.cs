using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using QuizAppProject.Context;
using QuizAppProject.Interfaces;
using QuizAppProject.Middleware;
using QuizAppProject.Models;
using QuizAppProject.Repositories;
using QuizAppProject.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// --------------------
// Database
// --------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// --------------------
// CORS
// --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
});

// --------------------
// Controllers
// --------------------
builder.Services.AddControllers();

// --------------------
// Swagger (No JWT)
// --------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Quiz App API",
        Version = "v1"
    });
});

// --------------------
// Dependency Injection
// --------------------
builder.Services.AddScoped<IRepository<Guid, User>, Repository<Guid, User>>();
builder.Services.AddScoped<IRepository<Guid, Quiz>, Repository<Guid, Quiz>>();
builder.Services.AddScoped<IRepository<Guid, Category>, Repository<Guid, Category>>();
builder.Services.AddScoped<IRepository<Guid, Question>, Repository<Guid, Question>>();
builder.Services.AddScoped<IRepository<Guid, Option>, Repository<Guid, Option>>();
builder.Services.AddScoped<IRepository<Guid, AttemptAnswer>, Repository<Guid, AttemptAnswer>>();

// Services
builder.Services.AddScoped<IUserService, UserService>();   
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IOptionService, OptionService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IAttemptService, AttemptService>();
builder.Services.AddScoped<IGroupService, GroupService>();




var app = builder.Build();

// --------------------
// Middleware Pipeline
// --------------------

app.UseGlobalExceptionHandler();  

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("DefaultCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
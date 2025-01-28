using System.Text;
using API.Data;
using API.Interface;
using API.Services;
using API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using API.ExceptionMiddleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

/*builder.Services.AddControllers();
builder.Services.AddDbContext<DataContext>(opt =>{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddCors();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
/*builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ITokenService,TokenService>();*/
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdenttityServices(builder.Configuration);
/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>{
        var tokenkey = builder.Configuration["TokenKey"] ?? throw new Exception("TokenKey not found");
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters{
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenkey)),
            ValidateIssuer=false,
            ValidateAudience=false
        };
    });*/

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(x=>x.AllowAnyHeader().AllowAnyMethod()
.WithOrigins("http://localhost:4200","https://localhost:4200"));

app.UseAuthentication();
app.UseAuthorization();
// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();*/

app.MapControllers();

app.Run();

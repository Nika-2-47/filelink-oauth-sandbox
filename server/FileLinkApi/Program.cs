using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Keycloak の Issuer URL
var keycloakIssuer = "http://localhost:8080/realms/sandbox";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakIssuer;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidIssuer = keycloakIssuer,
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/files/upload", async (HttpRequest request) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest("Form content type required.");

    var form = await request.ReadFormAsync();
    var file = form.Files["file"];
    if (file is null)
        return Results.BadRequest("File is required.");

    var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "storage");
    Directory.CreateDirectory(saveDir);

    var filePath = Path.Combine(saveDir, file.FileName);
    using var stream = File.Create(filePath);
    await file.CopyToAsync(stream);

    return Results.Ok(new
    {
        status = "success",
        fileName = file.FileName,
        storedPath = filePath
    });
})
.RequireAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();

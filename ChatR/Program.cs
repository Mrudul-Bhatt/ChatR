using ChatR.Data;
using ChatR.Hubs;
using ChatR.Service;

var policyName = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<MongoDbContext>(provider => new MongoDbContext(builder.Configuration));
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<ChatDBService>();

builder.Services.AddSignalR();


builder.Services.AddCors(options => options.AddPolicy(policyName,
    builder =>
    {
        builder.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(host => true)
            .AllowCredentials();
    }));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(policyName);

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");

app.Run();
using StudyCenter.System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("https://studycenter.marcellh.me")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

builder.Services.AddControllers();
builder.Services.AddSingleton<Postgresql>();
builder.Services.AddSingleton<Authentication>();
builder.Services.AddSingleton<Mail>();
builder.Services.AddHostedService<NotificationEvent>();



var app = builder.Build();

app.UseCors("AllowReactApp");

app.MapControllers();

app.Run();
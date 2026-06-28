using FeedPulse.Api.Data;
using FeedPulse.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IFeedSyncService, FeedSyncService>();
builder.Services.AddHttpClient<IFeedItemContentService, FeedItemContentService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("FeedPulse/1.0");
});
builder.Services.AddHttpClient<IAiTextService, AiTextService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("WebDatabase")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
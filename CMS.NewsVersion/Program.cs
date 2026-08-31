using CMS.NewsVersion.Context;
using CMS.NewsVersion.Helpers.Solr;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin",
    builder2 => builder2.WithOrigins(builder.Configuration["SiteSettings:CorsUrls"].Split(","))
    .SetIsOriginAllowedToAllowWildcardSubdomains()
     .AllowAnyHeader()
     .AllowCredentials()
     .WithMethods("GET", "PUT", "POST", "DELETE", "OPTIONS"));
});



builder.Services.Configure<SolrSettings>(builder.Configuration.GetSection("SolrSettings"));
// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<NewsVersionContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NewsVersionContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder2 => builder2.AllowAnyHeader().AllowAnyMethod().WithOrigins(builder.Configuration["SiteSettings:CorsUrls"].Split(","))
.AllowAnyMethod()
.AllowCredentials()
.AllowAnyHeader());


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

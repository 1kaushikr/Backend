using DomainLogic.Repository;
using DomainLogic.Supervisor;
using DataLayer;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(p => p.AddPolicy("policy", build =>
{   
        build.WithOrigins("http://localhost:3000/").AllowAnyHeader().AllowAnyOrigin();
  
}));
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IMongoClient>(s=>new MongoClient(builder.Configuration.GetValue<String>("ResumeRetrieverDatabaseSetting:ConnectionString")));
builder.Services.AddSingleton<IDataService,DataService>();
builder.Services.AddSingleton<IApplicantService, ApplicantService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("policy");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

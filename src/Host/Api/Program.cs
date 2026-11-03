using BuildingBlocks.Common.Exceptions.Handler;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


var app = builder.Build();

app.UseExceptionHandler(options => { });

app.Run();
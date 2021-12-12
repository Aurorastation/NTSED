var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/clear", () => 0);

app.Run();

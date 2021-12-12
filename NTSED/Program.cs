using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

var rewriteOptions = new RewriteOptions()
    .AddRewrite("^ntsed/(.*)", "$1", false);
app.UseRewriter(rewriteOptions);

app.MapGet("/clear", int () => throw new NotImplementedException());
app.MapGet("/new_program", int () => throw new NotImplementedException());
app.MapGet("/execute", int () => throw new NotImplementedException());
app.MapGet("/remove", int () => throw new NotImplementedException());
app.MapGet("/computer/get_buffer", string () => throw new NotImplementedException());
app.MapGet("/computer/topic", int () => throw new NotImplementedException());


app.Run();

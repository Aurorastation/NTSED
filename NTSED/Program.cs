using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using NTSED;
using NTSED.Models;
using NTSED.ProgramTypes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ProgramManagementService>();

var app = builder.Build();

app.UseStaticFiles();

var rewriteOptions = new RewriteOptions()
    .AddRewrite("^ntsed/(.*)", "$1", false);
app.UseRewriter(rewriteOptions);


app.MapGet("/clear", int (ProgramManagementService pms) =>
{
    pms.Clear();
    return 1;
});
app.MapGet("/new_program", int (ProgramManagementService pms, [FromQuery] ProgramType type) => pms.NewProgram(type));
app.MapPost("/execute", async Task<int> (ProgramManagementService pms, [FromQuery] int id, [FromBody] string script, [FromQuery] string scriptName) =>
{
    await pms.GetProgram(id).ExecuteScript(script, scriptName);
    return 1;
});
app.MapGet("/remove", int (ProgramManagementService pms, [FromQuery] int id) =>
{
    pms.RemoveProgram(id);
    return 1;
});
app.MapGet("/computer/get_buffer", string (ProgramManagementService pms, [FromQuery] int id) => pms.GetProgram<ComputerProgram>(id).GetTerminalBuffer());
app.MapPost("/computer/topic", async Task<int> (ProgramManagementService pms, [FromQuery] int id, [FromQuery] string topic, [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] string data) => 
{
    await pms.GetProgram<ComputerProgram>(id).HandleTopic(topic, data);
    return 1;
});


app.Run();

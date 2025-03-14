using Microsoft.OpenApi.Models;
using PsihoApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Psiho API", Version = "v1" });
});


builder.Services.AddSingleton<SpeechService>();
builder.Services.AddSingleton<AIServiceOnline>();
builder.Services.AddSingleton<IPsihoServiceAI, AIService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/from-speech", async (SpeechService speechService, IPsihoServiceAI aiServiceLocal, bool? newConversation) =>
{
    string userText = await speechService.RecordAndTranscribe();
    string aiResponse = await aiServiceLocal.AnalyzeText(userText, newConversation ?? false);

    return Results.Json(new { userText, aiResponse });
})
.WithName("TranscribeSpeech");

app.MapPost("/analyze", async (IPsihoServiceAI aiService, bool? newConversation, string userText) =>
{
    string aiResponse = await aiService.AnalyzeText(userText, newConversation ?? false);
    return Results.Json(new { aiResponse });
})
.WithName("AnalyzeText");

app.Run();



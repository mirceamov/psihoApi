namespace PsihoApi.Services
{
    public interface IPsihoServiceAI
    {
        Task<string> AnalyzeText(string userText, bool newConversation = false);
    }
}

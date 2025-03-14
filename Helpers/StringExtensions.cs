namespace PsihoApi.Helpers;

public static class StringExtensions
{
    public static string ToRomanianDiacritics(this string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // Normalize Romanian characters (Fix broken diacritics)
        text = text.Replace("╚¢", "ț")
                   .Replace("╚ü", "ș")
                   .Replace("╚Ö", "ș")
                   .Replace("├Ä", "ă")
                   .Replace("╚¦", "î")
                   .Replace("├«", "î")
                   .Replace("├ô", "â")
                   .Replace("├ó", "â")
                   .Replace("∩┐╜", "ă")
                   .Replace("─â", "ă")
                   .Replace("Daca", "Dacă") // Fix capitalization

                   // Remove unwanted LLaMA `[end of text]` markers
                   .Replace("[end of text]", "");

        // Remove extra whitespace & new lines
        text = text.Replace("\r", "").Replace("\n", " ").Trim();

        return text;
    }
}
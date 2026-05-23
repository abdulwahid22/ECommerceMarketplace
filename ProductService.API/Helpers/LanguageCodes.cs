namespace ProductService.API.Helpers
{
    public static class LanguageCodes
    {
        public const string English = "en";
        public const string German = "de";

        public static readonly List<string> SupportedLanguages = new()
        {
            English,
            German
        };

        public static bool IsSupported(string languageCode)
        {
            return SupportedLanguages.Contains(
                languageCode.Trim().ToLower()
            );
        }

        public static string Normalize(string? languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return English;
            }

            var normalized = languageCode.Trim().ToLower();

            return IsSupported(normalized)
                ? normalized
                : English;
        }
    }
}
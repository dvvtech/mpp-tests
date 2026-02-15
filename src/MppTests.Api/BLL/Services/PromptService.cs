using MppTests.Api.BLL.Abstract;
using MppTests.Api.BLL.Exceptions;
using System.Reflection;

namespace MppTests.Api.BLL.Services
{    
    public class PromptService : IPromptService
    {
        private const string PromptResourceName = "MppTests.Api.BLL.Prompts.ColorPsychologyPrompt.txt";
        private const string PromptResourceNameV2 = "MppTests.Api.BLL.Prompts.ColorPsychologyPromptV2.txt";

        private static readonly Lazy<string> SystemPromptLazy = new Lazy<string>(() =>
            LoadPromptFromResources(PromptResourceName));

        private static readonly Lazy<string> SystemPromptLazyV2 = new Lazy<string>(() =>
            LoadPromptFromResources(PromptResourceNameV2));

        private static string SystemPrompt => SystemPromptLazy.Value;

        private static string SystemPromptV2 => SystemPromptLazyV2.Value;

        private const string UserPromptTemplate = """
Ниже приведены данные о цветовых предпочтениях пользователя.
Проценты показывают долю каждого цвета и суммарно составляют ~100%.

Цвета могут быть указаны на русском языке.

ДАННЫЕ:
{0}
""";

        public string GetSystemPrompt(int version)
        {
            if (version == 1)
            {
                return SystemPrompt;
            }
            else if (version == 2)
            {
                return SystemPromptV2;
            }

            throw new Exception("not found version for system prompt");
        }

        public string GetUserPrompt(int version)
        {
            if (version == 1 || version == 2)
            {
                return UserPromptTemplate;
            }

            throw new Exception("not found version for user prompt");
        }

        private static string LoadPromptFromResources(string promptResourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(promptResourceName);
            if (stream == null)
                throw new ResourceNotFoundException(promptResourceName);

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}

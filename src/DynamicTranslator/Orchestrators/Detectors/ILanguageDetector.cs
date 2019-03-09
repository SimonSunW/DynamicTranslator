using System.Threading.Tasks;

namespace DynamicTranslator.Orchestrators.Detectors
{
    public interface ILanguageDetector
    {
        Task<string> DetectLanguage(string text);
    }
}

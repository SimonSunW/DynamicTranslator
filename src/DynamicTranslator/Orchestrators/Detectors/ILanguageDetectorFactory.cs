using System.Collections.Generic;

namespace DynamicTranslator.Orchestrators.Detectors
{
    public interface ILanguageDetectorFactory
    {
        ICollection<ILanguageDetector> GetLanguageDetectors();
    }
}

using System.Collections.Generic;

namespace DynamicTranslator.Orchestrators.Finders
{
    public interface IMeanFinderFactory
    {
        ICollection<IMeanFinder> GetFinders();
    }
}

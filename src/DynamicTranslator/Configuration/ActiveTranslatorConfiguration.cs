using System;
using System.Collections.Generic;
using System.Linq;
using DynamicTranslator.Extensions;

namespace DynamicTranslator.Configuration
{
    public class ActiveTranslatorConfiguration
    {
        public ActiveTranslatorConfiguration()
        {
            Translators = new List<Translator>();
        }

        public void Activate<T>() where T : ITranslator
        {
            Translators.FirstOrDefault(t => t.Type == typeof(T))?.Activate();
        }

        public void AddTranslator<T>() where T : ITranslator
        {
            Translators.Add(new Translator(typeof(T).Name, typeof(T)));
        }

        public void DeActivate()
        {
            Translators.ForEach(t => t.DeActivate());
        }

        public IReadOnlyList<Translator> ActiveTranslators => Translators.Where(x => x.IsActive).ToList();

        public IList<Translator> Translators { get; }
    }
}
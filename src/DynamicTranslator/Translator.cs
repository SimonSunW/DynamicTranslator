using System;

namespace DynamicTranslator
{
    public class Translator
    {
        public Translator(string name, Type type, bool isEnabled)
        {
            IsEnabled = isEnabled;
            Name = name;
            Type = type;
        }

        public Translator(string name, Type type) : this(name, type, true)
        {
            Name = name;
            Type = type;
        }

        public Translator Activate()
        {
            IsActive = true;
            return this;
        }

        public Translator DeActivate()
        {
            IsActive = false;
            return this;
        }

        public bool IsActive { get; private set; }

        public bool IsEnabled { get; private set; }

        public string Name { get; private set; }

        public Type Type { get; private set; }
    }
}
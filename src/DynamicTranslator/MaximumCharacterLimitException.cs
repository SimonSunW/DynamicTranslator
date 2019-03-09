using System;

namespace DynamicTranslator
{
    public class MaximumCharacterLimitException : Exception
    {
        public MaximumCharacterLimitException(string message) : base(message) {}
    }
}

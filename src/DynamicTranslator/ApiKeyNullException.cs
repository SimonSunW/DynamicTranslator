using System;

namespace DynamicTranslator
{
    public class ApiKeyNullException : Exception
    {
        public ApiKeyNullException(string message) : base(message) {}
    }
}

using System;

namespace DynamicTranslator.Events
{
    public class WhenClipboardContainsTextEventArgs : EventArgs
    {
        public string CurrentString { get; set; }
    }
}

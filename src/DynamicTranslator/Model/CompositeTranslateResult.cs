using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicTranslator.Model
{
    [Serializable]
    public class CompositeTranslateResult 
    {
        public CompositeTranslateResult(string searchText, int frequency, ICollection<TranslateResult> result, DateTime createDate):this()
        {
            Results = result;
            SearchText = searchText;
            CreateDate = createDate;
            Frequency = frequency;
            Id = searchText;
        }

        public string Id { get; set; }

        public CompositeTranslateResult() {}

        [DataMember]
        public DateTime CreateDate { get; protected set; }

        [DataMember]
        public int Frequency { get; protected set; }

        [DataMember]
        public ICollection<TranslateResult> Results { get; protected set; }

        [DataMember]
        public string SearchText { get; protected set; }

        public CompositeTranslateResult IncreaseFrequency()
        {
            Frequency += 1;
            return this;
        }

        public CompositeTranslateResult SetCreateDate(DateTime datetime)
        {
            CreateDate = datetime;
            return this;
        }

        public CompositeTranslateResult SetResults(ICollection<TranslateResult> results)
        {
            Results = results;
            return this;
        }
    }
}

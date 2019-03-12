using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicTranslator.Model;

namespace DynamicTranslator.Orchestrators
{
    public class ResultOrganizer
    {
        public Task<string> OrganizeResult(ICollection<TranslateResult> foundedMeans, string currentString, out string failedResults)
        {
            string succeededResults = Organize(foundedMeans, currentString, true);

            failedResults = Organize(foundedMeans, currentString, false);

            return Task.FromResult(succeededResults);
        }

        private string Organize(ICollection<TranslateResult> foundedMeans, string currentString, bool isSucceeded)
        {
            var mean = new StringBuilder();
            IEnumerable<TranslateResult> results = foundedMeans.Where(result => result.IsSuccess == isSucceeded);

            foreach (TranslateResult result in results)
            {
                mean.AppendLine(result.ResultMessage);
            }

            if (!string.IsNullOrEmpty(mean.ToString()))
            {
                List<string> means = mean.ToString()
                                         .Split('\r')
                                         .Select(x => x.Trim().ToLower())
                                         .Where(s => s != string.Empty && s != currentString.Trim() && s != "Translation")
                                         .Distinct()
                                         .ToList();

                mean.Clear();
                means.ForEach(m => mean.AppendLine($"{Titles.Asterix} {m.ToLower()}"));

                return (mean.ToString());
            }

            return string.Empty;
        }
    }
}

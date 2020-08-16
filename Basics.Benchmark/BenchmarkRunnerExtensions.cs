namespace Basics.Benchmark
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using BenchmarkDotNet.Reports;
    using BenchmarkDotNet.Running;

    internal static class BenchmarkRunnerExtensions
    {
        internal static Summary Run<T>(Action<string> output)
        {
            var summary = BenchmarkRunner.Run<T>();

            output(summary.Title);
            output($"TotalTime: {summary.TotalTime.ToString()}");
            output($"Details in: {summary.LogFilePath}");

            if (summary.HasCriticalValidationErrors)
            {
                var errors = summary
                            .ValidationErrors
                            .Select(error => new InvalidOperationException(error.ToString()))
                            .ToList();

                throw new AggregateException(errors);
            }

            return summary;
        }

        internal static IDictionary<string, decimal> Measures(this Summary summary, string measureColumnName)
        {
            var methodColumn = summary.Table.Column("Method");
            var measureColumn = summary.Table.Column(measureColumnName);

            return summary.Table
                          .FullContent
                          .ToDictionary(rowContent => rowContent[methodColumn],
                                        rowContent =>
                                        {
                                            var measure = rowContent[measureColumn]
                                                         .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                                                         .First();

                                            return decimal.Parse(measure, CultureInfo.InvariantCulture);
                                        });
        }

        private static int Column(this SummaryTable summaryTable, string columnName)
        {
            return summaryTable.FullHeader
                               .Select((element, i) => (element, i))
                               .SingleOrDefault(pair => pair.element == columnName)
                               .i;
        }
    }
}
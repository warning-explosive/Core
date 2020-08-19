namespace Basics.Benchmark
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using BenchmarkDotNet.Columns;
    using BenchmarkDotNet.Reports;
    using BenchmarkDotNet.Running;
    using Microsoft.VisualStudio.TestPlatform.Utilities;
    using Perfolizer.Horology;
    using SpaceEngineers.Core.Basics;

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

        internal static IDictionary<string, decimal> Measures(this Summary summary, string measureColumnName, Action<string> output)
        {
            var methodColumn = summary.Column("Method");
            var measureColumn = summary.Column(measureColumnName);

            if (!measureColumn.IsNumeric)
            {
                throw new InvalidOperationException($"{measureColumnName} isn't numeric");
            }

            var style = new SummaryStyle(CultureInfo.InvariantCulture,
                                         false,
                                         SizeUnit.B,
                                         TimeUnit.Nanosecond,
                                         false,
                                         true);

            return summary
                  .BenchmarksCases
                  .ToDictionary(benchmarksCase => methodColumn.GetValue(summary, benchmarksCase),
                                benchmarksCase =>
                                {
                                    var measure = measureColumn.GetValue(summary, benchmarksCase, style);

                                    output($"{methodColumn.GetValue(summary, benchmarksCase)} -> {measureColumnName} -> {measure} {TimeUnit.Nanosecond.Name}");

                                    return decimal.Parse(measure, CultureInfo.InvariantCulture);
                                });
        }

        private static IColumn Column(this Summary summary, string columnName)
        {
            return summary.GetColumns().Single(col => col.ColumnName == columnName);
        }
    }
}
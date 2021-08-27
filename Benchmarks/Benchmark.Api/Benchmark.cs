namespace SpaceEngineers.Core.Benchmark.Api
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using BenchmarkDotNet.Columns;
    using BenchmarkDotNet.Reports;
    using BenchmarkDotNet.Running;
    using BenchmarkDotNet.Validators;
    using Perfolizer.Horology;

    /// <summary>
    /// Benchmark entry point
    /// </summary>
    [SuppressMessage("Analysis", "CA1724", Justification = "Desired name")]
    public static class Benchmark
    {
        /// <summary>
        /// Benchmark entry point method
        /// </summary>
        /// <param name="output">Output</param>
        /// <typeparam name="TSource">TSource type-argument</typeparam>
        /// <returns>Benchmark summary</returns>
        public static Summary Run<TSource>(Action<string> output)
        {
            var summary = BenchmarkRunner.Run<TSource>();

            output(summary.Title);
            output($"TotalTime: {summary.TotalTime.ToString()}");
            output($"Details in: {summary.LogFilePath}");

            if (summary.HasCriticalValidationErrors)
            {
                var errors = ((IEnumerable<ValidationError>)summary.ValidationErrors)
                    .Select(error => new InvalidOperationException(error.ToString()))
                    .ToList();

                throw new AggregateException(errors);
            }

            return summary;
        }

        /// <summary>
        /// Gets time measure in seconds from benchmark summary
        /// </summary>
        /// <param name="summary">Summary</param>
        /// <param name="measureRecordName">Measure record name</param>
        /// <param name="measure">Measure</param>
        /// <param name="output">Output</param>
        /// <returns>Seconds time measure</returns>
        public static decimal SecondMeasure(
            this Summary summary,
            string measureRecordName,
            Measure measure,
            Action<string> output)
        {
            return summary.TimeMeasure(
                TimeUnit.Second,
                measureRecordName,
                measure,
                output);
        }

        /// <summary>
        /// Gets time measure in milliseconds from benchmark summary
        /// </summary>
        /// <param name="summary">Summary</param>
        /// <param name="measureRecordName">Measure record name</param>
        /// <param name="measure">Measure</param>
        /// <param name="output">Output</param>
        /// <returns>Milliseconds time measure</returns>
        public static decimal MillisecondMeasure(
            this Summary summary,
            string measureRecordName,
            Measure measure,
            Action<string> output)
        {
            return summary.TimeMeasure(
                TimeUnit.Millisecond,
                measureRecordName,
                measure,
                output);
        }

        /// <summary>
        /// Gets time measure in nanoseconds from benchmark summary
        /// </summary>
        /// <param name="summary">Summary</param>
        /// <param name="measureRecordName">Measure record name</param>
        /// <param name="measure">Measure</param>
        /// <param name="output">Output</param>
        /// <returns>Nanoseconds time measure</returns>
        public static decimal NanosecondMeasure(
            this Summary summary,
            string measureRecordName,
            Measure measure,
            Action<string> output)
        {
            return summary.TimeMeasure(
                TimeUnit.Nanosecond,
                measureRecordName,
                measure,
                output);
        }

        /// <summary>
        /// Gets time measure from benchmark summary
        /// </summary>
        /// <param name="summary">Summary</param>
        /// <param name="timeUnit">TimeUnit</param>
        /// <param name="measureRecordName">Measure record name</param>
        /// <param name="measure">Measure</param>
        /// <param name="output">Output</param>
        /// <returns>Time measure</returns>
        public static decimal TimeMeasure(
            this Summary summary,
            TimeUnit timeUnit,
            string measureRecordName,
            Measure measure,
            Action<string> output)
        {
            return summary.TimeMeasures(measure, output, timeUnit)[measureRecordName];
        }

        private static IDictionary<string, decimal> TimeMeasures(
            this Summary summary,
            Measure measure,
            Action<string> output,
            TimeUnit timeUnit)
        {
            var measureColumnName = measure.ToString();

            var methodColumn = summary.Column("Method");
            var measureColumn = summary.Column(measureColumnName);

            if (!measureColumn.IsNumeric)
            {
                throw new InvalidOperationException($"{measureColumnName} isn't numeric");
            }

            var style = new SummaryStyle(CultureInfo.InvariantCulture,
                false,
                SizeUnit.B,
                timeUnit,
                false,
                true);

            return summary
                  .BenchmarksCases
                  .ToDictionary(benchmarksCase => methodColumn.GetValue(summary, benchmarksCase),
                      ParseTime(summary, methodColumn, measureColumn, measureColumnName, timeUnit, style, output));

            static Func<BenchmarkCase, decimal> ParseTime(Summary summary,
                IColumn methodColumn,
                IColumn measureColumn,
                string measureColumnName,
                TimeUnit timeUnit,
                SummaryStyle style,
                Action<string> output)
            {
                return benchmarkCase =>
                {
                    var measure = measureColumn.GetValue(summary, benchmarkCase, style);

                    output($"{methodColumn.GetValue(summary, benchmarkCase)} -> {measureColumnName} -> {measure} {timeUnit.Name}");

                    return decimal.Parse(measure, CultureInfo.InvariantCulture);
                };
            }
        }

        private static IColumn Column(this Summary summary, string columnName)
        {
            return summary.GetColumns().Single(col => col.ColumnName == columnName);
        }
    }
}
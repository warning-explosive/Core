namespace SpaceEngineers.Core.CliArgumentsParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;

    /// <summary>
    /// Cli arguments parser class
    /// Valid parameters forms:
    /// {-,/,--}param{ ,=,:}((')value('))
    /// Example1:
    ///     --action=backup --dbname=%SqlBackupDbName% --filename=%SqlBackupDbName%-for-deploy --connection='Data Source=%env.DatabaseServerName%; Initial Catalog=master;Integrated Security=True;Connection Timeout=300' --shrink=%env.Shrink% --destservers %env.TargetDbServerName% --DbFilePath=%env.DbFilePath%
    /// Example2:
    ///     -param1 value1 --param2 /param3:'Test-:-work' /param4=happy -param5 '--=nice=--'
    /// </summary>
    [Lifestyle(EnLifestyle.Singleton)]
    internal class CliArgumentsParserImpl : ICliArgumentsParser
    {
        private static readonly Regex RegexCliParser = new Regex(@"(?!-{1,2}|/)(?<name>\w+)(?:[=:]?|\s+)(?<value>[^-\s""][^""]*?|""[^""]*"")?(?=\s+[-/]|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex QuotesRemover = new Regex(@"[^'|""].*[^'|""]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex FinishChecker = new Regex(@"(?:-{1,2}|/|=|'|""|\s)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc />
        public T Parse<T>(string[] args)
            where T : class, new()
        {
            var argsDictionary = Init(args);

            var joined = argsDictionary
               .Join(typeof(T).GetProperties(BindingFlags.Instance
                                             | BindingFlags.Public
                                             | BindingFlags.GetProperty
                                             | BindingFlags.SetProperty),
                     cli => cli.Key.ToUpperInvariant(),
                     pd => pd.Name.ToUpperInvariant(),
                     (cli, pd) => new { Info = pd, CliValue = cli.Value });

            var instance = Activator.CreateInstance<T>();

            foreach (var j in joined)
            {
                if (TryGetValue(j.Info.PropertyType, j.CliValue, out var value))
                {
                    j.Info.SetValue(instance, value);
                }
            }

            return instance;
        }

        /// <inheritdoc />
        public bool TryParse<T>(string[] args, out T? arguments)
            where T : class, new()
        {
            arguments = default;

            try
            {
                arguments = Parse<T>(args);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private static Dictionary<string, string?> Init(string[] args)
        {
            var cliArguments = string.Join(" ", args);

            var argsDictionary = new Dictionary<string, string?>();

            foreach (var match in RegexCliParser.Matches(cliArguments))
            {
                var argument = match.ToString();

                var splitedArgument = RegexCliParser
                                     .Split(argument)
                                     .Where(z => !string.IsNullOrEmpty(z))
                                     .ToArray();

                if (splitedArgument.Length < 1 || splitedArgument.Length > 2)
                {
                    throw new ArgumentException($"Invalid argument: '{argument}'");
                }

                var command = string.Empty;
                string? commandValue = string.Empty;

                if (splitedArgument.Length == 1)
                {
                    command = splitedArgument[0];
                    commandValue = null;
                }
                else if (splitedArgument.Length == 2)
                {
                    command = splitedArgument[0];
                    commandValue = QuotesRemover.Match(splitedArgument[1]).Value;
                }

                if (argsDictionary.TryGetValue(command, out _))
                {
                    throw new ArgumentException($"'{command}' already added");
                }

                argsDictionary.Add(command, commandValue);
            }

            var entries = argsDictionary
                         .SelectMany(z => new[]
                                          {
                                              z.Key,
                                              z.Value
                                          })
                         .Where(z => !string.IsNullOrEmpty(z))
                         .OrderByDescending(z => z?.Length)
                         .ToArray();

            var splited = cliArguments
                         .Split(entries, StringSplitOptions.RemoveEmptyEntries)
                         .Select(z => FinishChecker.Replace(z, string.Empty))
                         .Where(z => !string.IsNullOrEmpty(z))
                         .ToArray();

            if (splited.Any())
            {
                throw new ArgumentException($"Untreated CLI arguments: {string.Join(", ", splited.Select(z => "'" + z + "'"))}");
            }

            return argsDictionary;
        }

        private static bool TryGetValue(Type type, string strValue, out object? typedValue)
        {
            typedValue = default;

            // string
            if (type == typeof(string))
            {
                typedValue = strValue;
            }

            // bool
            // Nullable<bool>
            if (type == typeof(bool) || type == typeof(bool?))
            {
                if (string.IsNullOrEmpty(strValue))
                {
                    typedValue = true;
                }
                else if (bool.TryParse(strValue, out var result))
                {
                    typedValue = result;
                }
            }

            // enum,
            // Nullable<enum>
            // enum flags
            var enumType = type.IsNullable()
                               ? type.GetGenericArguments()[0]
                               : type;

            if (enumType.IsEnum)
            {
                var flagsValues = strValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

                if (enumType.IsDefined(typeof(FlagsAttribute), false) && flagsValues.Length > 1)
                {
                    if (TryParseEnum(enumType, string.Join(", ", flagsValues), out var result))
                    {
                        typedValue = result;
                    }
                    else
                    {
                        var perValueResult = flagsValues.Select(value => new
                                                                         {
                                                                             Success = TryParseEnum(enumType, value, out _),
                                                                             Value = value,
                                                                         });
                        throw new ArgumentException($"Values {string.Join(string.Empty, perValueResult.Where(z => !z.Success).Select(z => "'" + z.Value + "'"))} is not recognized");
                    }
                }
                else if (TryParseEnum(enumType, strValue, out var result))
                {
                    typedValue = result;
                }
                else
                {
                    throw new ArgumentException($"Value '{strValue}' is not recognized");
                }
            }

            return typedValue != null;
        }

        private static bool TryParseEnum(Type enumType, string strValue, out object? result)
        {
            result = typeof(CliArgumentsParserImpl).CallMethod(nameof(TryParseEnum))
                                                   .WithTypeArgument(enumType)
                                                   .WithArgument(strValue)
                                                   .Invoke()
                                                   .TryExtractFromNullable<object>();

            var separatedValues = strValue.ToUpperInvariant()
                                          .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                          .SelectMany(s => s)
                                          .ToArray();

            var parsed = ToEnumString(result).ToUpperInvariant();

            return separatedValues.All(single => parsed.Contains(single));
        }

        private static TEnum TryParseEnum<TEnum>(string strValue)
            where TEnum : struct
        {
            Enum.TryParse<TEnum>(strValue, true, out var result);

            return result;
        }

        private static string ToEnumString(object value)
        {
            return value.CallMethod(nameof(Enum.ToString))
                        .WithArgument("G")
                        .Invoke()
                        .TryExtractFromNullable<string>();
        }
    }
}
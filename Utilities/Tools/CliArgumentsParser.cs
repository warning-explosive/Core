namespace SpaceEngineers.Core.Utilities.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Extensions;

    /// <summary>
    /// Cli arguments parser class
    /// Valid parameters forms:
    /// {-,/,--}param{ ,=,:}((')value('))
    /// Example1:
    ///     --action=backup --dbname=%SqlBackupDbName% --filename=%SqlBackupDbName%-for-deploy --connection='Data Source=%env.DatabaseServerName%; Initial Catalog=master;Integrated Security=True;Connection Timeout=300' --shrink=%env.Shrink% --destservers %env.TargetDbServerName% --DbFilePath=%env.DbFilePath%
    /// Example2:
    ///     -param1 value1 --param2 /param3:'Test-:-work' /param4=happy -param5 '--=nice=--'
    /// </summary>
    public class CliArgumentsParser
    {
        private static readonly Regex RegexCliParser = new Regex(@"(?!-{1,2}|/)(?<name>\w+)(?:[=:]?|\s+)(?<value>[^-\s""][^""]*?|""[^""]*"")?(?=\s+[-/]|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        private static readonly Regex QuotesRemover = new Regex(@"[^'|""].*[^'|""]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        private static readonly Regex FinishChecker = new Regex(@"(?:-{1,2}|/|=|'|""|\s)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly Dictionary<string, string> _argsDictionary;

        /// <summary> ctor </summary>
        /// <param name="args">args</param>
        /// <exception cref="ArgumentException">Error</exception>
        public CliArgumentsParser(string[] args)
        {
            _argsDictionary = new Dictionary<string, string>();

            var cliArguments = string.Join(" ", args);

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
                var commandValue = string.Empty;

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

                if (!_argsDictionary.TryAdd(command, commandValue))
                {
                    throw new ArgumentException($"'{command}' already added");
                }
            }

            var entries = _argsDictionary
                          .SelectMany(
                                      z => new[]
                                           {
                                               z.Key,
                                               z.Value
                                           })
                          .Where(z => !string.IsNullOrEmpty(z))
                          .OrderByDescending(z => z.Length)
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
        }

        /// <summary> Parse cli arguments </summary>
        /// <typeparam name="T">Cli args poco-type</typeparam>
        /// <returns>Cli args poco-type instance</returns>
        public T Parse<T>() where T : class, new()
        {
            var joined = _argsDictionary
                .Join(typeof(T)
                          .GetProperties(BindingFlags.Instance
                                         | BindingFlags.Public
                                         | BindingFlags.GetProperty
                                         | BindingFlags.SetProperty),
                      cli => cli.Key.ToLowerInvariant(),
                      pd => pd.Name.ToLowerInvariant(),
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

        /// <summary>
        /// Try parse cli argiments
        /// </summary>
        /// <param name="arguments">Out cli args</param>
        /// <typeparam name="T">Cli args poco-type</typeparam>
        /// <returns>True/false</returns>
        public bool TryParse<T>(out T arguments) where T : class, new()
        {
            arguments = null;
            
            try
            {
                arguments = Parse<T>();
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private bool TryGetValue(Type type, string strValue, out object typedValue)
        {
            typedValue = null;
            
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
            var enumType = type.IsImplementationOfOpenGeneric(typeof(Nullable<>))
                ? type.GetGenericArguments()[0]
                : type;
            
            if (enumType.IsEnum)
            {
                var flagsValues = (strValue?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>()).ToArray();
                
                if (enumType.IsDefined(typeof(FlagsAttribute), false) && flagsValues.Length > 1)
                {
                    if (Enum.TryParse(enumType, string.Join(", ", flagsValues), true, out var result))
                    {
                        typedValue = result;
                    }
                    else
                    {
                        var perValueResult = flagsValues.Select(value => new
                                                    {
                                                        Success = Enum.TryParse(enumType,
                                                                                value,
                                                                                true,
                                                                                out _),
                                                        Value = value,
                                                    });
                        throw new ArgumentException($"Values {string.Join("", perValueResult.Where(z => !z.Success).Select(z => "'" + z.Value + "'"))} is not recognized");
                    }
                }
                else if (Enum.TryParse(enumType, strValue, true, out var result))
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
    }
}
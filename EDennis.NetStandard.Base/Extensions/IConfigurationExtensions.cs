using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDennis.NetStandard.Base
{
    public static partial class IConfigurationExtensions {


        public static void BindSectionOrThrow<T>(this IConfiguration config, string key, T instance, ILogger logger = null) {
            if (config.TryGetSection(key, out IConfigurationSection section)) {
                section.Bind(instance);
            } else {
                Throw(key, typeof(T), logger);
            }
        }


        public static T GetValueOrThrow<T>(this IConfiguration config, string key, ILogger logger = null, bool removeQuotesAroundValue = true) {
            if (config.TryGetValue(key, out T value, removeQuotesAroundValue))
                return value;
            else
                Throw(key, typeof(T), logger);

            return default;
        }

        private static string Throw(string key, Type type, ILogger logger = null) {
            var ex = new ApplicationException($"Could not bind key '{key}' in Configuration to object of type {CSharpName(type)}");
            
            if (logger != null)
                logger.LogError(ex, ex.Message);

            throw ex;
        }


        public static bool TryGetSection(this IConfiguration config, string key, out IConfigurationSection targetSection) {
            targetSection = null;
            var children = config.GetChildren().ToList();
            foreach (var child in children)
                if (child.TryGetSection(key, out IConfigurationSection childSection, "")) {
                    targetSection = childSection;
                    return true;
                }
            return false;
        }

        public static bool TryGetSection(this IConfigurationSection section, string key, out IConfigurationSection targetSection, string parentKey = "") {
            //Console.WriteLine($"{parentKey}{section.Key}");
            targetSection = null;
            if ($"{parentKey}{section.Key}" == key) {
                targetSection = section;
                return true;
            } else if (section.Value == null) {
                var children = section.GetChildren().ToList();
                foreach (var child in children)
                    if (child.TryGetSection(key, out IConfigurationSection childSection, $"{parentKey}{section.Key}:")) {
                        targetSection = childSection;
                        return true;
                    }
            }
            return false;
        }


        public static bool TryGetValue<T>(this IConfiguration config, string key, out T targetValue, bool removeQuotesAroundValue = true) {
            targetValue = default;

            var value = config[key];
            if (value != null) {
                
                if (removeQuotesAroundValue && value.StartsWith("\"") && value.EndsWith("\""))
                    value = value[1..^1];
                try {
                    targetValue = (T)Convert.ChangeType(value, typeof(T));                    
                } catch (Exception) {
                    Throw(key, typeof(T));
                }
                return true;
            }

            targetValue = default;
            var children = config.GetChildren().ToList();
            foreach (var child in children)
                if (child.ContainsKey(key, "")) {
                    targetValue = default;
                    return true;
                }
            return false;
        }


        public static bool TryGetValue<T>(this IConfigurationSection config, string key, out T targetValue) {
            targetValue = default;

            var value = config[key];
            if (value != null) {
                try {
                    targetValue = (T)Convert.ChangeType(value, typeof(T));
                } catch (Exception) {
                    Throw(key, typeof(T));
                }
                return true;
            }

            targetValue = default;
            var children = config.GetChildren().ToList();
            foreach (var child in children)
                if (child.ContainsKey(key, "")) {
                    targetValue = default;
                    return true;
                }
            return false;
        }


        public static bool ContainsKey(this IConfiguration config, string key) {
            if (config[key] != null) {
                return true;
            } else {
                var children = config.GetChildren().ToList();
                foreach (var child in children)
                    if (child.ContainsKey(key, ""))
                        return true;
            }

            return false;
        }

        public static bool ContainsKey(this IConfigurationSection section, string key, string parentKey = "") {
            if ($"{parentKey}{section.Key}" == key) {
                return true;
            } else if (section.Value == null) {
                var children = section.GetChildren().ToList();
                foreach (var child in children)
                    if (child.ContainsKey(key, $"{parentKey}{section.Key}:"))
                        return true;
            }

            return false;
        }


        /// <summary>
        /// from https://stackoverflow.com/a/21269486
        /// Get full type name with full namespace names
        /// </summary>
        /// <param name="type">
        /// The type to get the C# name for (may be a generic type or a nullable type).
        /// </param>
        /// <returns>
        /// Full type name, fully qualified namespaces
        /// </returns>
        public static string CSharpName(this Type type) {
            Type nullableType = Nullable.GetUnderlyingType(type);
            string nullableText;
            if (nullableType != null) {
                type = nullableType;
                nullableText = "?";
            } else {
                nullableText = string.Empty;
            }

            if (type.IsGenericType) {
                return string.Format(
                    "{0}<{1}>{2}",
                    type.Name.Substring(0, type.Name.IndexOf('`')),
                    string.Join(", ", type.GetGenericArguments().Select(ga => ga.CSharpName())),
                    nullableText);
            }

            return type.Name switch
            {
                "String" => "string",
                "Int32" => "int" + nullableText,
                "Decimal" => "decimal" + nullableText,
                "Object" => "object" + nullableText,
                "Void" => "void" + nullableText,
                _ => (string.IsNullOrWhiteSpace(type.FullName) ? type.Name : type.FullName) + nullableText,
            };
        }



        /// <summary>
        /// Gets a configuration enumeration for a particular
        /// provider
        /// </summary>
        /// <param name="config">root configuration</param>
        /// <param name="providerType">provider type</param>
        /// <returns>enumerable of configuration sections</returns>
        public static IEnumerable<IConfigurationSection> GetConfiguration(this IConfiguration config, Type providerType) {
            var root = config as IConfigurationRoot;
            string path = null;

            //filter the list of providers
            var providers = root.Providers.Where(p => p.GetType() == providerType);

            //build the configuration enumeration for the provider.
            //use the Aggregate extension method to build the 
            //configuration cumulatively.
            //(see https://github.com/aspnet/Configuration/blob/master/src/Config/ConfigurationRoot.cs)
            var entries = providers
                .Aggregate(Enumerable.Empty<string>(),
                    (seed, source) => source.GetChildKeys(seed, path))
                .Distinct()
                .Select(key => GetSection(root, path == null ? key : ConfigurationPath.Combine(path, key)));
            return entries;
        }

        /// <summary>
        /// Gets a configuartion section
        /// </summary>
        /// <param name="root">root configuration</param>
        /// <param name="key">key for the section</param>
        /// <returns>the configuratio section</returns>
        public static IConfigurationSection GetSection(IConfigurationRoot root, string key) {
            return new ConfigurationSection(root as ConfigurationRoot, key);
        }


        /// <summary>
        /// Gets command-line arguments from configuration
        /// </summary>
        /// <param name="config">the configuration</param>
        /// <returns>dictionary of command-line arguments</returns>
        public static Dictionary<string, string> GetCommandLineArguments(this IConfiguration config) {
            var args = config.GetConfiguration(typeof(CommandLineConfigurationProvider))
                .ToDictionary(x => x.Key, x => x.Value);
            return args;
        }


        /// <summary>
        /// Gets a configuration enumeration for a particular
        /// provider
        /// </summary>
        /// <param name="config">root configuration</param>
        /// <param name="providerType">provider type</param>
        /// <returns>enumerable of configuration sections</returns>
        public static IEnumerable<IConfigurationSection> GetJsonConfiguration(this IConfiguration config, string filePath) {
            var root = config as IConfigurationRoot;
            string path = null;

            //filter the list of providers
            var providers = root.Providers.Where(p => p.GetType() == typeof(JsonConfigurationProvider)).ToList();
            var jsonProviders = providers.Select(p=>p as JsonConfigurationProvider).Where(x => x.Source.Path == filePath).ToList();

            //build the configuration enumeration for the provider.
            //use the Aggregate extension method to build the 
            //configuration cumulatively.
            //(see https://github.com/aspnet/Configuration/blob/master/src/Config/ConfigurationRoot.cs)
            var entries = jsonProviders
                .Aggregate(Enumerable.Empty<string>(),
                    (seed, source) => source.GetChildKeys(seed, path))
                .Distinct()
                .Select(key => GetSection(root, path == null ? key : ConfigurationPath.Combine(path, key)));
            return entries;
        }


        public static string[] ToCommandLineArgs(this IConfiguration config) {
            List<string> args= new List<string>();
            var flattened = config.Flatten();
            foreach(var entry in flattened) {
                args.Add($"{entry.Key}={entry.Value}");
            }
            return args.ToArray();
        }

        public static Dictionary<string, string> Flatten(this IConfiguration config) {
            var dict = new Dictionary<string, string>();
            config.GetChildren().AsParallel().ToList()
                .ForEach(x => x.Flatten(dict));
            return dict;
        }
        private static void Flatten(this IConfigurationSection section,
            Dictionary<string, string> dict, string parentKey = "") {
            if (section.Value == null)
                section.GetChildren().AsParallel().ToList()
                    .ForEach(x => x.Flatten(dict, $"{parentKey}{section.Key}:"));
            else
                dict.Add($"{parentKey}{section.Key}", section.Value);
        }

    }
}

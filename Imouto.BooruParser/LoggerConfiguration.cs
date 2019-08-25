using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Imouto.BooruParser
{
    public class LoggerAccessor
    {
        public static ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();

        public static ILogger<T> GetLogger<T>() => LoggerFactory.CreateLogger<T>();

        public static ILogger GetLogger(string typeName) => LoggerFactory.CreateLogger(typeName);
    }
}
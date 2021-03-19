using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Crey.MessageContracts
{
#nullable enable
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MessageSerdeAttribute : Attribute
    {
        public string SerdeToken { get; private set; }

        public MessageSerdeAttribute(string serdeType)
        {
            SerdeToken = serdeType;
        }

        public static Type? GetTypeForSerdeToken<TBaseType>(string token)
        {
            var type = GetTypeCandidates<TBaseType>()
                .Where(type => GetAttribute(type)?.SerdeToken == token)
                .SingleOrDefault();

            Trace.Assert(type?.IsSealed ?? true, "Serde type must be sealed");
            return type;
        }

        public static IEnumerable<string> GetAllSerdeTokens<TBaseType>()
        {
            return GetTypeCandidates<TBaseType>()
                .Select(type => GetAttribute(type)?.SerdeToken)
                .Where(x => x != null)
                .ToList()!;
        }

        public static string GetSerdeToken(object obj)
        {
            return GetRequiredAttribute(obj.GetType()).SerdeToken;
        }

        public static MessageSerdeAttribute? GetAttribute(Type type)
        {
            return type.GetCustomAttributes(typeof(MessageSerdeAttribute), false).SingleOrDefault() as MessageSerdeAttribute;
        }

        public static MessageSerdeAttribute GetRequiredAttribute(Type type)
        {
            return GetAttribute(type)
                ?? throw new ContractViolationException($"Missing serde attribute, use {nameof(MessageSerdeAttribute)}");
        }

        private static IEnumerable<Type> GetTypeCandidates<TBaseType>()
        {
            return typeof(MessageSerdeAttribute).Assembly
                .GetTypes()
                // public and non-abstract
                .Where(type => !type.IsAbstract && type.IsPublic)
                // must be "derived" from base to have "scoped" tokens
                .Where(type => typeof(TBaseType).IsAssignableFrom(type) || type.IsSubclassOf(typeof(TBaseType)));
        }
    }
}
using Crey.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;

namespace Crey.Misc
{
    public static class ObjectHelper
    {
        /// <summary>
        /// Swaps two objects
        /// </summary>
        public static void Swap<T>(ref T src, ref T dest)
        {
            var t = src;
            src = dest;
            dest = t;
        }

        public static int CombineHashCodes(int h1, int h2)
        {
            unchecked
            {
                return (((h1 << 5) + h1) ^ h2);
            }
        }

        #region Fiddling with type 

        /// <summary>
        /// Works on null objects as well
        /// </summary>
        /// <typeparam name="TSelf"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Type GetDeclaredType<TSelf>(this TSelf obj)
        {
            return typeof(TSelf);
        }

        public static string SchemaNameFilter(this string source)
        {
            //return source;   
            if (source.IsNullOrEmpty()) return source;
            if (source.Contains("/"))
            {
                var lastSlash = source.LastIndexOf('/');
                return $"{source.Substring(lastSlash + 1, source.Length - lastSlash - 1)}";
            }

            return source;
        }

        public static string SlimSchemaName(this object instance)
        {
            if (instance is string s)
            {
                return s.SchemaNameFilter();
            }

            return instance.ToString().SchemaNameFilter();
        }

        public static string SlimTName(this object instance, bool fullName = false)
        {
            if (instance is Type type) return (fullName ? type.FullName : type.Name).SlimTypeNameFilter();

            return (fullName ? instance.GetType().FullName : instance.GetType().Name).SlimTypeNameFilter();
        }

        private static string SlimTypeNameFilter(this string source)
        {
            //return source;   
            if (source.IsNullOrEmpty()) return source;
            if (source.Contains("`"))
            {
                return $"{source.Substring(0, source.IndexOf('`'))}";
            }

            return source;
        }

        public static string GetTypeAndValueInfo(this object inputObj)
        {
            var type = inputObj.GetType();

            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            var members = type.GetFields(bindingFlags).Cast<MemberInfo>()
                .Concat(type.GetProperties(bindingFlags)).ToArray();

            var result = $"[{type.ToString()}]";

            string FromLastDot(string inputString)
            {
                var lastDot = inputString.LastIndexOf('.') + 1;
                if (lastDot < 0) lastDot = 0;
                var len = inputString.Length - lastDot;
                return inputString.Substring(lastDot, len);
            }

            var typeNames = new List<string>();
            var memberNames = new List<string>();
            var memberValues = new List<string>();

            foreach (var memberInfo in members)
            {
                memberNames.Add(memberInfo.Name);

                var field = memberInfo as FieldInfo;
                if (field != null)
                {
                    typeNames.Add(FromLastDot(field.FieldType.ToString()));
                    memberValues.Add(field.GetValue(inputObj).ToString());
                }

                var property = memberInfo as PropertyInfo;
                if (property != null)
                {
                    typeNames.Add(FromLastDot(property.PropertyType.ToString()));
                    memberValues.Add(property.GetValue(inputObj).ToString());
                }
            }

            var maxTypeNameLen = typeNames.Select(y => y.Length).Max();
            var maxMemberNameLen = memberNames.Select(y => y.Length).Max();

            for (var i = 0; i < typeNames.Count; i++)
            {
                var pTypeName = typeNames[i].PadRight(maxTypeNameLen + 1);
                var pMemberName = memberNames[i].PadLeft(maxMemberNameLen);
                result = $"{result}\n {pTypeName}{pMemberName}: {memberValues[i]}";
            }

            return result;
        }

        #endregion

        public static void DumpInfo(this ClaimsPrincipal user)
        {
            Console.WriteLine($"\nIdentity:\n name:[{user.Identity.Name}] T:[{user.Identity.AuthenticationType}] isAuth:[{user.Identity.IsAuthenticated}]");

            Console.WriteLine("Claims:");
            foreach (var userClaim in user.Claims)
            {
                Console.WriteLine($"  [ {userClaim.Value,-40}]   iss/orig:[{userClaim.Issuer}/{userClaim.OriginalIssuer}] roleClT:[{userClaim.Subject.RoleClaimType.SlimSchemaName()}] userClT:[{userClaim.Type.SlimSchemaName()}] valueT:[{userClaim.ValueType.SlimSchemaName()}]");

                if (userClaim.Properties.Count > 0)
                {
                    Console.WriteLine("\tProperties:");
                    foreach (var property in userClaim.Properties)
                    {
                        Console.WriteLine($"\t\t[{property.Key}] -> [{property.Value}]");
                    }
                }
            }

            Console.WriteLine("Identities:");
            foreach (var claimsIdentity in user.Identities)
            {
                Console.WriteLine($" name:[{claimsIdentity.Name}] lbl:[{claimsIdentity.Label}] isAuth:[{claimsIdentity.IsAuthenticated}] authType:[{claimsIdentity.AuthenticationType}] nameClT:[{claimsIdentity.NameClaimType.SlimSchemaName()}] roleClT:[{claimsIdentity.RoleClaimType.SlimSchemaName()}]");
            }

            Console.WriteLine("");
        }
    }
}
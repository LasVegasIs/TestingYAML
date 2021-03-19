using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Core.Azure
{
    // Helper function to serialize general payload into TableStorage without exposing the internal structure of TableEntity.
    // Code snippets were taken from https://github.com/Hitcents/azure-sdk-for-net-old/tree/e6bad5991f271195c9dd19b5ed791824e7642fae/microsoft-azure-api/Services/Storage/Lib/Common/Table
    public static class CosmosDynamicTableEntityExtensions
    {
        public static T CreateObject<T>(this DynamicTableEntity dynamicTableEntity, ILogger logger = null)
            where T : class, new()
        {
            var obj = new T();
            dynamicTableEntity.ReadObject(obj, logger);
            return obj;
        }

        public static T ReadObject<T>(this DynamicTableEntity dynamicTableEntity, T obj, ILogger logger = null, string prefix = null)
            where T : class
        {
            IEnumerable<PropertyInfo> objectProperties = obj.GetType().GetProperties();
            var properties = dynamicTableEntity.Properties;
            foreach (PropertyInfo property in objectProperties)
            {
                // reserved properties
                if (property.Name == "PartitionKey" ||
                    property.Name == "RowKey" ||
                    property.Name == "Timestamp" ||
                    property.Name == "ETag")
                {
                    continue;
                }

                if (property.GetSetMethod() == null || !property.GetSetMethod().IsPublic
                    || property.GetGetMethod() == null || !property.GetGetMethod().IsPublic)
                {
                    continue;
                }

                if (Attribute.GetCustomAttribute(property, typeof(IgnoreTableEntityAttribute)) != null)
                {
                    continue;
                }

                // only proceed with properties that have a corresponding entry in the dictionary
                var name = prefix != null ? $"{prefix}_{property.Name}" : property.Name;
                if (!properties.ContainsKey(name))
                {
                    continue;
                }

                EntityProperty entityProperty = properties[name];
                switch (entityProperty.PropertyType)
                {
                    case EdmType.String:
                        if (property.PropertyType != typeof(string) && property.PropertyType != typeof(String))
                        {
                            logger?.LogWarning($"{property.Name} has incompatible type, required: {property.PropertyType.Name}, got:{entityProperty.PropertyType}");
                            continue;
                        }

                        property.SetValue(obj, entityProperty.StringValue, null);
                        break;
                    case EdmType.Binary:
                        if (property.PropertyType != typeof(byte[]))
                        {
                            logger?.LogWarning($"{property.Name} has incompatible type, required: {property.PropertyType.Name}, got:{entityProperty.PropertyType}");
                            continue;
                        }

                        property.SetValue(obj, entityProperty.BinaryValue, null);
                        break;
                    case EdmType.Boolean:
                        if (property.PropertyType != typeof(bool) && property.PropertyType != typeof(Boolean) && property.PropertyType != typeof(Boolean?) && property.PropertyType != typeof(bool?))
                        {
                            logger?.LogWarning($"{property.Name} has incompatible type, required: {property.PropertyType.Name}, got:{entityProperty.PropertyType}");
                            continue;
                        }

                        property.SetValue(obj, entityProperty.BooleanValue, null);
                        break;
                    case EdmType.DateTime:
                        if (property.PropertyType == typeof(DateTime))
                        {
                            property.SetValue(obj, entityProperty.DateTimeOffsetValue.Value.UtcDateTime, null);
                        }
                        else if (property.PropertyType == typeof(DateTime?))
                        {
                            property.SetValue(obj, entityProperty.DateTimeOffsetValue.HasValue ? entityProperty.DateTimeOffsetValue.Value.UtcDateTime : (DateTime?)null, null);
                        }
                        else if (property.PropertyType == typeof(DateTimeOffset))
                        {
                            property.SetValue(obj, entityProperty.DateTimeOffsetValue.Value, null);
                        }
                        else if (property.PropertyType == typeof(DateTimeOffset?))
                        {
                            property.SetValue(obj, entityProperty.DateTimeOffsetValue, null);
                        }
                        else
                        {
                            logger?.LogWarning($"{property.Name} has incompatible type, required: {property.PropertyType.Name}, got:{entityProperty.PropertyType}");
                        }

                        break;
                    case EdmType.Double:
                        if (property.PropertyType != typeof(double) && property.PropertyType != typeof(Double) && property.PropertyType != typeof(Double?) && property.PropertyType != typeof(double?))
                        {
                            logger?.LogWarning($"{property.Name} has incompatible type, required: {property.PropertyType.Name}, got:{entityProperty.PropertyType}");
                            continue;
                        }

                        property.SetValue(obj, entityProperty.DoubleValue, null);
                        break;
                    case EdmType.Guid:
                        if (property.PropertyType != typeof(Guid) && property.PropertyType != typeof(Guid?))
                        {
                            logger?.LogWarning($"{property.Name} has incompatible type, required: {property.PropertyType.Name}, got:{entityProperty.PropertyType}");
                            continue;
                        }

                        property.SetValue(obj, entityProperty.GuidValue, null);
                        break;
                    case EdmType.Int32:
                        if (property.PropertyType.IsEnum)
                        {
                            var value = Enum.ToObject(property.PropertyType, entityProperty.Int32Value);
                            property.SetValue(obj, value, null);
                        }
                        else
                        {
                            if (property.PropertyType != typeof(int) && property.PropertyType != typeof(Int32) && property.PropertyType != typeof(Int32?) && property.PropertyType != typeof(int?))
                            {
                                logger?.LogWarning($"{property.Name} has incompatible type, required: {property.PropertyType.Name}, got:{entityProperty.PropertyType}");
                                continue;
                            }

                            property.SetValue(obj, entityProperty.Int32Value, null);
                        }
                        break;
                    case EdmType.Int64:
                        if (property.PropertyType.IsEnum)
                        {
                            var value = Enum.ToObject(property.PropertyType, entityProperty.Int64Value);
                            property.SetValue(obj, value, null);
                        }
                        else
                        {
                            if (property.PropertyType != typeof(long) &&
                            property.PropertyType != typeof(Int64) &&
                            property.PropertyType != typeof(long?) &&
                            property.PropertyType != typeof(uint) &&
                            property.PropertyType != typeof(ulong))
                            {
                                logger?.LogError($"{property.Name} has incompatible type, required: {property.PropertyType.Name}, got:{entityProperty.PropertyType}");
                                continue;
                            }

                            if (property.PropertyType == typeof(uint))
                                property.SetValue(obj, (uint)entityProperty.Int64Value.Value, null);
                            else if (property.PropertyType == typeof(ulong))
                                property.SetValue(obj, (ulong)entityProperty.Int64Value.Value, null);
                            else
                                property.SetValue(obj, entityProperty.Int64Value, null);
                        }
                        break;
                }
            }
            return obj;
        }

        public static DynamicTableEntity WriteObject<T>(this DynamicTableEntity dynamicTableEntity, T obj, ILogger logger = null, string prefix = null)
        {
            IEnumerable<PropertyInfo> objectProperties = obj.GetType().GetProperties();
            foreach (PropertyInfo property in objectProperties)
            {
                // reserved properties
                if (property.Name == "PartitionKey" ||
                    property.Name == "RowKey" ||
                    property.Name == "Timestamp" ||
                    property.Name == "ETag")
                {
                    logger?.LogWarning($"{property.Name} is restricted");
                    continue;
                }

                if (property.GetSetMethod() == null || !property.GetSetMethod().IsPublic
                    || property.GetGetMethod() == null || !property.GetGetMethod().IsPublic)
                {
                    continue;
                }

                if (Attribute.GetCustomAttribute(property, typeof(IgnoreTableEntityAttribute)) != null)
                {
                    continue;
                }

                var value = property.GetValue(obj, null);

                EntityProperty newProperty = null;
                if (value is string)
                {
                    newProperty = new EntityProperty((string)value);
                }
                else if (value is byte[])
                {
                    newProperty = new EntityProperty((byte[])value);
                }
                else if (value is bool)
                {
                    newProperty = new EntityProperty((bool)value);
                }
                else if (value is bool?)
                {
                    newProperty = new EntityProperty((bool?)value);
                }
                else if (value is DateTime)
                {
                    newProperty = new EntityProperty((DateTime)value);
                }
                else if (value is DateTime?)
                {
                    newProperty = new EntityProperty((DateTime?)value);
                }
                else if (value is DateTimeOffset)
                {
                    newProperty = new EntityProperty((DateTimeOffset)value);
                }
                else if (value is DateTimeOffset?)
                {
                    newProperty = new EntityProperty((DateTimeOffset?)value);
                }
                else if (value is double)
                {
                    newProperty = new EntityProperty((double)value);
                }
                else if (value is double?)
                {
                    newProperty = new EntityProperty((double?)value);
                }
                else if (value is Guid?)
                {
                    newProperty = new EntityProperty((Guid?)value);
                }
                else if (value is Guid)
                {
                    newProperty = new EntityProperty((Guid)value);
                }
                else if (value is int)
                {
                    newProperty = new EntityProperty((int)value);
                }
                else if (value is int?)
                {
                    newProperty = new EntityProperty((int?)value);
                }
                else if (value is long)
                {
                    newProperty = new EntityProperty((long)value);
                }
                else if (value is ulong ulongValue)
                {
                    newProperty = new EntityProperty((long)ulongValue);
                }
                else if (value is uint uintValue)
                {
                    newProperty = new EntityProperty((long)uintValue);
                }
                else if (value is long?)
                {
                    newProperty = new EntityProperty((long?)value);
                }
                else if (value == null)
                {
                    newProperty = new EntityProperty((string)null);
                }
                else if (value is Enum)
                {
                    var numericValue = Convert.ChangeType(value, typeof(long));
                    newProperty = new EntityProperty((long)numericValue);
                }
                else
                {
                    logger?.LogWarning($"{property.Name} unknown type ({property.PropertyType}) to serialize");
                }

                // property will be null if unknown type
                if (newProperty != null)
                {
                    var name = prefix != null ? $"{prefix}_{property.Name}" : property.Name;
                    dynamicTableEntity.Properties.Add(name, newProperty);
                }
            }
            return dynamicTableEntity;
        }


    }
}
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Json;

namespace EDennis.AspNet.Base {
    public abstract class CrudEntity : ICrudEntity {

        public string SysUser { get; set; }

        public SysStatus SysStatus { get; set; }


        /// <summary>
        /// By default, use reflection to update an object.  If performance is
        /// unacceptable, this method can be overwritten with an implementation
        /// that doesn't use reflection.
        /// </summary>
        /// <param name="updated">an updated object</param>
        public virtual void Update(object updated) {
            foreach (var prop in GetType().GetProperties()) {
                prop.SetValue(this, prop.GetValue(updated));
            }
        }


        /// <summary>
        /// By default, use reflection to patch the object.  If performance is
        /// unacceptable, this method can be overwritten with an implementation
        /// that doesn't use reflection.
        /// </summary>
        /// <param name="jsonElement">The updated data as a JsonElement</param>
        public virtual void Patch(JsonElement jsonElement, ModelStateDictionary modelState) {
            var camelCase = false;
            foreach (var prop in GetType().GetProperties())
                try {
                    if (!camelCase && jsonElement.TryGetProperty(prop.Name, out JsonElement value)) {
                        prop.SetValue(this, DeserializeJsonValue(prop.PropertyType, value));
                    } else if (jsonElement.TryGetProperty(CamelCase(prop.Name), out JsonElement value2)) {
                        camelCase = true;
                        prop.SetValue(this, DeserializeJsonValue(prop.PropertyType, value2));
                    }
                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, ex.Message);
                }
        }


        private static string CamelCase(string input)
            => (input == null || input.Length < 2) ? input : input.Substring(0, 1).ToLower() + input.Substring(1);

        private static object DeserializeJsonValue(Type type, JsonElement value) {
            if (value.ValueKind == JsonValueKind.Null
                && (type == typeof(string) || Nullable.GetUnderlyingType(type) != null))
                return null;
            else if (type == typeof(bool) || type == typeof(bool?))
                return value.GetBoolean();
            else if (type == typeof(byte) || type == typeof(byte?))
                return value.GetByte();
            else if (type == typeof(short) || type == typeof(short?))
                return value.GetInt16();
            else if (type == typeof(ushort) || type == typeof(ushort?))
                return value.GetUInt16();
            else if (type == typeof(int) || type == typeof(int?))
                return value.GetInt32();
            else if (type == typeof(uint) || type == typeof(uint?))
                return value.GetUInt32();
            else if (type == typeof(long) || type == typeof(long?))
                return value.GetInt64();
            else if (type == typeof(ulong) || type == typeof(ulong?))
                return value.GetUInt64();
            else if (type == typeof(float) || type == typeof(float?))
                return value.GetSingle();
            else if (type == typeof(double) || type == typeof(double?))
                return value.GetDouble();
            else if (type == typeof(decimal) || type == typeof(decimal?))
                return value.GetDecimal();
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
                return value.GetDateTime();
            else if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
                return value.GetDateTimeOffset();
            else if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
                return TimeSpan.Parse(value.GetString());
            else if (type == typeof(string))
                return value.GetString();
            else
                return JsonSerializer.Deserialize(value.GetRawText(), type);
        }


        public override string ToString() {
            return JsonSerializer.Serialize(this);
        }

    }
}

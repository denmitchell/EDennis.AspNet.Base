using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EDennis.AspNet.Base {
    public abstract class CrudEntity {

        public int Id { get; set; }
        public Guid SysId { get; set; } = CombGuid.Create();
        public string SysUser { get; set; }
        public SysStatus SysStatus { get; set; }


        /// <summary>
        /// By default, use reflection to update an object.  If performance
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
        public virtual void Patch(JsonElement jsonElement) {
            foreach (var prop in GetType().GetProperties())
                if (jsonElement.TryGetProperty(prop.Name, out JsonElement value))
                    if (value.ValueKind != JsonValueKind.Object && value.ValueKind != JsonValueKind.Array)
                        prop.SetValue(this, DeserializeJsonValue(prop.PropertyType, value));

        }

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
                return TimeSpan.Parse(value.GetString());
            else
                return JsonSerializer.Deserialize(value.GetRawText(), type);
        }


    }
}

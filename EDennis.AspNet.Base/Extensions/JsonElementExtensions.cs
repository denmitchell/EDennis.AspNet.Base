using System;
using System.Linq.Expressions;
using System.Text.Json;

namespace EDennis.AspNet.Base {
    public static class JsonElementExtensions {


        public static bool TryGetBoolean(this JsonElement jsonElement, 
                string propertyName, out bool value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = propElement.GetBoolean();
            return true;
        }

        public static bool TryGetBoolean(this JsonElement jsonElement,
                string propertyName, out bool? value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            if (propElement.ValueKind == JsonValueKind.Null)
                value = null;
            else
                value = propElement.GetBoolean();
            return true;
        }

        public static bool TryGetByte(this JsonElement jsonElement,
                string propertyName, out byte value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = propElement.GetByte();
            return true;
        }

        public static bool TryGetByte(this JsonElement jsonElement,
                string propertyName, out byte? value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            if (propElement.ValueKind == JsonValueKind.Null)
                value = null;
            else
                value = propElement.GetByte();
            return true;
        }

        public static bool TryGetInt16(this JsonElement jsonElement,
                string propertyName, out short value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = propElement.GetInt16();
            return true;
        }

        public static bool TryGetInt16(this JsonElement jsonElement,
                string propertyName, out short? value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            if (propElement.ValueKind == JsonValueKind.Null)
                value = null;
            else
                value = propElement.GetInt16();
            return true;
        }


        public static bool TryGetUInt16(this JsonElement jsonElement,
                string propertyName, out ushort value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = propElement.GetUInt16();
            return true;
        }

        public static bool TryGetUInt16(this JsonElement jsonElement,
                string propertyName, out ushort? value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            if (propElement.ValueKind == JsonValueKind.Null)
                value = null;
            else
                value = propElement.GetUInt16();
            return true;
        }


        public static bool TryGetInt32(this JsonElement jsonElement,
                string propertyName, out int value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = propElement.GetInt32();
            return true;
        }

        public static bool TryGetInt32(this JsonElement jsonElement,
                string propertyName, out int? value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            if (propElement.ValueKind == JsonValueKind.Null)
                value = null;
            else
                value = propElement.GetInt32();
            return true;
        }


        public static bool TryGetUInt32(this JsonElement jsonElement,
                string propertyName, out uint value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = propElement.GetUInt32();
            return true;
        }

        public static bool TryGetUInt32(this JsonElement jsonElement,
                string propertyName, out uint? value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            if (propElement.ValueKind == JsonValueKind.Null)
                value = null;
            else
                value = propElement.GetUInt32();
            return true;
        }


        public static bool TryGetInt64(this JsonElement jsonElement,
                string propertyName, out long value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = propElement.GetInt64();
            return true;
        }

        public static bool TryGetInt64(this JsonElement jsonElement,
                string propertyName, out long? value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            if (propElement.ValueKind == JsonValueKind.Null)
                value = null;
            else
                value = propElement.GetInt64();
            return true;
        }


        public static bool TryGetUInt64(this JsonElement jsonElement,
                string propertyName, out ulong value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = propElement.GetUInt64();
            return true;
        }

        public static bool TryGetUInt64(this JsonElement jsonElement,
                string propertyName, out ulong? value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            if (propElement.ValueKind == JsonValueKind.Null)
                value = null;
            else
                value = propElement.GetUInt64();
            return true;
        }

        public static bool TryGetSingle(this JsonElement jsonElement,
                string propertyName, out float value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = propElement.GetSingle();
            return true;
        }

        public static bool TryGetSingle(this JsonElement jsonElement,
                string propertyName, out float? value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            if (propElement.ValueKind == JsonValueKind.Null)
                value = null;
            else
                value = propElement.GetSingle();
            return true;
        }


        public static bool TryGetDouble(this JsonElement jsonElement,
                string propertyName, out double value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = propElement.GetDouble();
            return true;
        }

        public static bool TryGetDouble(this JsonElement jsonElement,
                string propertyName, out double? value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            if (propElement.ValueKind == JsonValueKind.Null)
                value = null;
            else
                value = propElement.GetDouble();
            return true;
        }


        public static bool TryGetDecimal(this JsonElement jsonElement,
                string propertyName, out decimal value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = propElement.GetDecimal();
            return true;
        }

        public static bool TryGetDecimal(this JsonElement jsonElement,
                string propertyName, out decimal? value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            if (propElement.ValueKind == JsonValueKind.Null)
                value = null;
            else
                value = propElement.GetDecimal();
            return true;
        }


        public static bool TryGetDateTime(this JsonElement jsonElement,
                string propertyName, out DateTime value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = propElement.GetDateTime();
            return true;
        }

        public static bool TryGetDateTime(this JsonElement jsonElement,
                string propertyName, out DateTime? value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            if (propElement.ValueKind == JsonValueKind.Null)
                value = null;
            else
                value = propElement.GetDateTime();
            return true;
        }

        public static bool TryGetDateTimeOffset(this JsonElement jsonElement,
                string propertyName, out DateTimeOffset value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = propElement.GetDateTimeOffset();
            return true;
        }

        public static bool TryGetDateTimeOffset(this JsonElement jsonElement,
                string propertyName, out DateTimeOffset? value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            if (propElement.ValueKind == JsonValueKind.Null)
                value = null;
            else
                value = propElement.GetDateTimeOffset();
            return true;
        }


        public static bool TryGetTimeSpan(this JsonElement jsonElement,
                string propertyName, out TimeSpan value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = TimeSpan.Parse(propElement.GetString());
            return true;
        }

        public static bool TryGetTimeSpan(this JsonElement jsonElement,
                string propertyName, out TimeSpan? value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            if (propElement.ValueKind == JsonValueKind.Null)
                value = null;
            else
                value = TimeSpan.Parse(propElement.GetString());
            return true;
        }

        public static bool TryGetString(this JsonElement jsonElement,
                string propertyName, out string value) {
            value = default;
            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propElement))
                return false;
            value = propElement.GetString();
            return true;
        }

    }
}

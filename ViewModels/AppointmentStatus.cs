using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ViewModels
{
    [JsonConverter(typeof(AppointmentStatusJsonConverter))]
    [TypeConverter(typeof(AppointmentStatusTypeConverter))]
    public enum AppointmentStatus
    {
        Scheduled,
        InProgress,
        MedicalRecordCreated,
        Completed,
        Canceled
    }

    public class AppointmentStatusTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string str)
            {
                return str.ToAppointmentStatus();
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class AppointmentStatusJsonConverter : JsonConverter<AppointmentStatus>
    {
        public override AppointmentStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == null)
            {
                throw new JsonException("Null status is not allowed");
            }
            return value.ToAppointmentStatus();
        }

        public override void Write(Utf8JsonWriter writer, AppointmentStatus value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToStatusString());
        }
    }

    public static class AppointmentStatusExtensions
    {
        public static string ToStatusString(this AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Scheduled => "Scheduled",
                AppointmentStatus.MedicalRecordCreated => "Medical Record Created",
                AppointmentStatus.InProgress => "In Progress",
                AppointmentStatus.Completed => "Completed",
                AppointmentStatus.Canceled => "Canceled",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        public static AppointmentStatus ToAppointmentStatus(this string statusStr)
        {
            return statusStr switch
            {
                "Scheduled" => AppointmentStatus.Scheduled,
                "Medical Record Created" or "MedicalRecordCreated" => AppointmentStatus.MedicalRecordCreated,
                "InProgress" or "In Progress" => AppointmentStatus.InProgress,
                "Canceled" or "Cancelled" => AppointmentStatus.Canceled,
                "Completed" => AppointmentStatus.Completed,
                _ => throw new ArgumentException($"Invalid status string: {statusStr}", nameof(statusStr))
            };
        }
    }
}

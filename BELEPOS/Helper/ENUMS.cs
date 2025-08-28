using System.Text.Json.Serialization;

namespace BELEPOS.Helper
{
    public enum NotesType
    {
        Customer,
        Internal,
        Diagnostic,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeviceType
    {

        Mobile,
        Tablet
    }

    public enum ProductType
    {

        Product,
        Service,
        Part
    }

    public enum RepairStatus
    {

        Pending,
        InProgress,
        Complted,
        Delivered,
    }
}

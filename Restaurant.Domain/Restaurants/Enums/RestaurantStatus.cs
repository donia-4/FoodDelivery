using System.Text.Json.Serialization;

namespace Restaurant.Domain.Restaurants.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]

public enum RestaurantStatus
{
    Pending,           // FR-02: قبل الاعتماد
    Approved,          // معتمد من الأدمن
    Rejected,          // مرفوض
    Suspended,         // معلق من الأدمن
    Open,              // مفتوح (FR-09)
    Closed,            // مغلق (FR-09)
    TemporarilyClosed, // مغلق مؤقتاً (FR-09)
    UnderMaintenance   // تحت الصيانة (FR-09)
}
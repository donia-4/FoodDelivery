using System.Text.Json.Serialization;

namespace Restaurant.Domain.Restaurants.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]

public enum CuisineType
{
    Arabic, Italian, Chinese, Indian, Mexican, American, Japanese,
    Mediterranean, FastFood, Desserts, Beverages, Other
}
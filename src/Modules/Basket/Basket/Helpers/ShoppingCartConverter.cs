namespace Basket.Helpers
{
    public class ShoppingCartConverter : JsonConverter<ShoppingCart>
    {
        public override ShoppingCart? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDocument = JsonDocument.ParseValue(ref reader);
            var root = jsonDocument.RootElement;

            var id = ShoppingCartId.Of(root.GetProperty("id").GetGuid());
            var userName = UserName.Of(root.GetProperty("userName").GetString()!);
            var items = root.GetProperty("items").Deserialize<List<ShoppingCartItem>>(options) ?? [];

            return ShoppingCart.Restore(id, userName, items);
        }

        public override void Write(Utf8JsonWriter writer, ShoppingCart value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("id", value.Id.Value.ToString());
            writer.WriteString("userName", value.UserName.Value);

            writer.WritePropertyName("items");
            JsonSerializer.Serialize(writer, value.Items, options);

            writer.WriteEndObject();
        }
    }
}
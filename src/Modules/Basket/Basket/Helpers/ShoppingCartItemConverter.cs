namespace Basket.Helpers
{
    public class ShoppingCartItemConverter : JsonConverter<ShoppingCartItem>
    {
        public override ShoppingCartItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonDocument = JsonDocument.ParseValue(ref reader);
            var rootElement = jsonDocument.RootElement;

            var id = rootElement.GetProperty("id").GetGuid();
            var shoppingCartId = rootElement.GetProperty("shoppingCartId").GetGuid();
            var productId = rootElement.GetProperty("productId").GetGuid();
            var quantity = rootElement.GetProperty("quantity").GetInt32();
            var color = rootElement.GetProperty("color").GetString()!;
            var price = rootElement.GetProperty("price").GetDecimal();
            var productName = rootElement.GetProperty("productName").GetString()!;

            return new ShoppingCartItem(id, shoppingCartId, productId, quantity, color, price, productName);
        }

        public override void Write(Utf8JsonWriter writer, ShoppingCartItem value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("id", value.Id.Value.ToString());
            writer.WriteString("shoppingCartId", value.ShoppingCartId.Value.ToString());
            writer.WriteString("productId", value.ProductId.Value.ToString());
            writer.WriteNumber("quantity", value.Quantity.Value);
            writer.WriteString("color", value.Color.Value);
            writer.WriteNumber("price", value.Price.Value);
            writer.WriteString("productName", value.ProductName.Value);

            writer.WriteEndObject();
        }
    }
}
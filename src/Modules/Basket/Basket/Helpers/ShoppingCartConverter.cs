using System.Reflection;

namespace Basket.Helpers
{
    public class ShoppingCartConverter : JsonConverter<ShoppingCart>
    {
        public override ShoppingCart? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonDocument = JsonDocument.ParseValue(ref reader);
            var rootElement = jsonDocument.RootElement;

            var userName = rootElement.GetProperty("userName").GetString()!;
            var itemsElement = rootElement.GetProperty("items");

            var shoppingCart = ShoppingCart.Create(UserName.Of(userName));
            shoppingCart.Id = ShoppingCartId.Of(rootElement.GetProperty("id").GetGuid());

            var items = itemsElement.Deserialize<List<ShoppingCartItem>>(options);
            if (items != null)
            {
                var itemsField = typeof(ShoppingCart).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
                itemsField?.SetValue(shoppingCart, items);
            }

            return shoppingCart;
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
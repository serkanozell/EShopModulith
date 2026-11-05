using BuildingBlocks.Common.Exceptions;

namespace Basket.Baskets.Exceptions
{
    public class BasketNotFoundException(string userName) : NotFoundException("ShoppingCart", userName) { }

}
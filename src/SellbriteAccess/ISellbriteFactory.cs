using SellbriteAccess.Configuration;
using SellbriteAccess.Services.Orders;
using SellbriteAccess.Services.Products;

namespace SellbriteAccess
{
	public interface ISellbriteFactory
    {
		ISellbriteOrdersService CreateOrdersService( SellbriteMerchantCredentials credentials );

		ISellbriteProductsService CreateProductsService( SellbriteMerchantCredentials credentials );
    }
}

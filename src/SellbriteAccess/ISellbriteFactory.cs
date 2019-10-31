using SellbriteAccess.Configuration;
using SellbriteAccess.Services.Orders;

namespace SellbriteAccess
{
	public interface ISellbriteFactory
    {
		ISellbriteOrdersService CreateOrdersService( SellbriteMerchantCredentials credentials );
    }
}

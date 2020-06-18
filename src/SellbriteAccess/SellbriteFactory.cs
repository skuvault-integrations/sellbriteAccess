using SellbriteAccess.Configuration;
using SellbriteAccess.Services.Orders;
using SellbriteAccess.Services.Products;

namespace SellbriteAccess
{
	public class SellbriteFactory : ISellbriteFactory
	{
		private SellbriteConfig _config;

		public SellbriteFactory()
		{
			_config = new SellbriteConfig();
		}

		public ISellbriteOrdersService CreateOrdersService( SellbriteMerchantCredentials credentials )
		{
			return new SellbriteOrdersService( this._config, credentials );
		}

		public ISellbriteProductsService CreateProductsService( SellbriteMerchantCredentials credentials )
		{
			return new SellbriteProductsService( this._config, credentials );
		}
	}
}

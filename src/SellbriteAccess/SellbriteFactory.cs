using System;
using System.Collections.Generic;
using System.Text;
using SellbriteAccess.Configuration;
using SellbriteAccess.Services.Orders;

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
	}
}

using CuttingEdge.Conditions;

namespace SellbriteAccess.Configuration
{
	public class SellbriteMerchantCredentials
	{
		public string AccountToken { get; private set; }
		public string SecretKey { get; private set; }

		public SellbriteMerchantCredentials( string accountToken, string secretKey )
		{
			Condition.Requires( accountToken, "accountToken" ).IsNotNullOrWhiteSpace();
			Condition.Requires( secretKey, "secretKey" ).IsNotNullOrWhiteSpace();

			this.AccountToken = accountToken;
			this.SecretKey = secretKey;
		}
	}
}

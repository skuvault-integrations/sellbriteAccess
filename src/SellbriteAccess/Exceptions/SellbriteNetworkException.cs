using System;

namespace SellbriteAccess.Exceptions
{
	public class SellbriteNetworkException : SellbriteException
	{
		public SellbriteNetworkException( string message, Exception exception ) : base( message, exception) { }
		public SellbriteNetworkException( string message ) : base( message ) { }
	}

	public class SellbriteUnauthorizedException : SellbriteException
	{
		public SellbriteUnauthorizedException( string message ) : base( message) { }
	}

	public class SellbriteRateLimitsExceeded : SellbriteNetworkException
	{
		public SellbriteRateLimitsExceeded( string message ) : base( message ) { }
	}
}

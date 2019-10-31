using CuttingEdge.Conditions;

namespace SellbriteAccess.Configuration
{
	public class SellbriteConfig
	{
		public readonly string ApiBaseUrl = "https://api.sellbrite.com";

		public readonly ThrottlingOptions ThrottlingOptions;
		public readonly NetworkOptions NetworkOptions;

		public int OrdersPageLimit { get; set; }

		public SellbriteConfig( ThrottlingOptions throttlingOptions, NetworkOptions networkOptions )
		{
			Condition.Requires( throttlingOptions, "throttlingOptions" ).IsNotNull();
			Condition.Requires( networkOptions, "networkOptions" ).IsNotNull();

			this.ThrottlingOptions = throttlingOptions;
			this.NetworkOptions = networkOptions;
			this.OrdersPageLimit = 100;
		}

		public SellbriteConfig() : this( ThrottlingOptions.SellbriteDefaultOptions, NetworkOptions.SellbriteDefaultOptions )
		{ }
	}

	public class ThrottlingOptions
	{
		public int MaxRequestsPerTimeInterval { get; private set; }
		public int TimeIntervalInSec { get; private set; }
		public int MaxRetryAttempts { get; private set; }

		public ThrottlingOptions( int maxRequests, int timeIntervalInSec, int maxRetryAttempts )
		{
			Condition.Requires( maxRequests, "maxRequests" ).IsGreaterOrEqual( 1 );
			Condition.Requires( timeIntervalInSec, "timeIntervalInSec" ).IsGreaterOrEqual( 1 );
			Condition.Requires( maxRetryAttempts, "maxRetryAttempts" ).IsGreaterOrEqual( 0 );

			this.MaxRequestsPerTimeInterval = maxRequests;
			this.TimeIntervalInSec = timeIntervalInSec;
			this.MaxRetryAttempts = maxRetryAttempts;
		}

		public static ThrottlingOptions SellbriteDefaultOptions
		{
			get
			{
				return new ThrottlingOptions( 2, 1, 10 );
			}
		}
	}

	public class NetworkOptions
	{
		public int RequestTimeoutMs { get; private set; }
		public int RetryAttempts { get; private set; }
		public int DelayBetweenFailedRequestsInSec { get; private set; }
		public int DelayFailRequestRate { get; private set; }

		public NetworkOptions( int requestTimeoutMs, int retryAttempts, int delayBetweenFailedRequestsInSec, int delayFaileRequestRate )
		{
			Condition.Requires( requestTimeoutMs, "requestTimeoutMs" ).IsGreaterThan( 0 );
			Condition.Requires( retryAttempts, "retryAttempts" ).IsGreaterOrEqual( 0 );
			Condition.Requires( delayBetweenFailedRequestsInSec, "delayBetweenFailedRequestsInSec" ).IsGreaterOrEqual( 0 );
			Condition.Requires( delayFaileRequestRate, "delayFaileRequestRate" ).IsGreaterOrEqual( 0 );

			this.RequestTimeoutMs = requestTimeoutMs;
			this.RetryAttempts = retryAttempts;
			this.DelayBetweenFailedRequestsInSec = delayBetweenFailedRequestsInSec;
			this.DelayFailRequestRate = delayFaileRequestRate;
		}

		public static NetworkOptions SellbriteDefaultOptions
		{
			get
			{
				return new NetworkOptions( 5 * 60 * 1000, 10, 5, 20 );
			}
		}
	}

	public class SellbriteEndPoint
	{
		public static readonly string OrdersUrl = "/v1/orders";
	}
}

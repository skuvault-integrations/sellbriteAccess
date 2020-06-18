using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using SellbriteAccess.Configuration;
using SellbriteAccess.Exceptions;
using SellbriteAccess.Models;
using SellbriteAccess.Shared;

namespace SellbriteAccess.Services.Orders
{
	public class SellbriteOrdersService : BaseService, ISellbriteOrdersService
	{
		public SellbriteOrdersService( SellbriteConfig config, SellbriteMerchantCredentials credentials ) : base( config, credentials )
		{
		}

		public async Task< IEnumerable< SellbriteOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, CancellationToken token )
		{
			Condition.Requires( startDateUtc ).IsLessThan( endDateUtc );
			
			var orders = new List< SellbriteOrder >();
			int page = 1;

			while( true )
			{
				var ordersByPage = await this.GetOrdersByPageAsync( startDateUtc, endDateUtc, page, base.Config.OrdersPageLimit, token ).ConfigureAwait( false );

				if ( ordersByPage.Count() == 0 )
				{
					break;
				}

				orders.AddRange( ordersByPage );

				++page;
			}

			return orders;
		}

		private async Task< IEnumerable< SellbriteOrder > > GetOrdersByPageAsync( DateTime startDateUtc, DateTime endDateUtc, int page, int limit, CancellationToken token )
		{
			var mark = Mark.CreateNew();
			var orders = new List< SellbriteOrder >();
			var url = string.Format("{0}?min_ordered_at={1}&max_ordered_at={2}&page={3}&limit={4}", SellbriteEndPoint.OrdersUrl, startDateUtc.FromUtcToRFC3339(), endDateUtc.FromUtcToRFC3339(), page, limit );

			try
			{
				SellbriteLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ) );

				var response = await base.GetAsync< Order[] >( url, token, mark ).ConfigureAwait( false );
				
				if ( response.Length != 0 )
				{
					orders.AddRange( response.Select( o => o.ToSvOrder() ) );
				}

				SellbriteLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: orders.ToJson(), additionalInfo: this.AdditionalLogInfo() ) );
			}
			catch( Exception ex )
			{
				var sellbriteException = new SellbriteException( this.CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ), ex );
				SellbriteLogger.LogTraceException( sellbriteException );
				throw sellbriteException;
			}

			return orders;
		}
	}
}

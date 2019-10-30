using SellbriteAccess.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SellbriteAccess.Services.Orders
{
	public interface ISellbriteOrdersService
	{
		Task< IEnumerable< SellbriteOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, CancellationToken token );
	}
}

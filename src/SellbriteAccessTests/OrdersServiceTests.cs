using FluentAssertions;
using NUnit.Framework;
using SellbriteAccess.Services.Orders;
using System;
using System.Linq;
using System.Threading;

namespace SellbriteAccessTests
{
	[ TestFixture ]
	public sealed class OrdersServiceTests : BaseTest
	{
		private ISellbriteOrdersService _ordersService;

		[ SetUp ]
		public void Init()
		{
			this._ordersService = new SellbriteOrdersService( base.Config, base.Credentials );
		}

		[ Test ]
		public void GetOrdersAsync()
		{
			var startDateUtc = new DateTime( 1971, 1, 1 );
			var endDateUtc = DateTime.MaxValue;

			var result = _ordersService.GetOrdersAsync( startDateUtc, endDateUtc, CancellationToken.None ).Result;

			result.Should().NotBeNullOrEmpty();
		}

		[ Test ]
		public void GetOrdersPageAsync()
		{
			var startDateUtc = new DateTime( 1971, 1, 1 );
			var endDateUtc = DateTime.MaxValue;
			base.Config.OrdersPageLimit = 5;

			var orders = _ordersService.GetOrdersAsync( startDateUtc, endDateUtc, CancellationToken.None ).Result;

			orders.Should().NotBeNullOrEmpty();
			orders.Count().Should().BeGreaterThan( 10 * base.Config.OrdersPageLimit );
		}

		[ Test ]
		public void GetOrdersThatWereCreatedRecentlyAsync()
		{
			var startDateUtc = DateTime.UtcNow.AddHours( -1 );
			var endDateUtc = DateTime.UtcNow;

			var orders = _ordersService.GetOrdersAsync( startDateUtc, endDateUtc, CancellationToken.None ).Result;

			orders.Should().NotBeNullOrEmpty();
			orders.Count().Should().BeGreaterOrEqualTo( 1 );
		}
	}
}

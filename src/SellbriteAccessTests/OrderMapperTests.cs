using FluentAssertions;
using NUnit.Framework;
using SellbriteAccess.Models;
using SellbriteAccess.Shared;
using System.Linq;

namespace SellbriteAccessTests
{
	[ TestFixture ]
	public class OrderMapperTests
	{
		[ Test ]
		public void ToSVOrder()
		{
			var orderId = "001";
			var carrier = "USPS";
			var carrierClass = "USPS Priority Mail";
			var shippingCity = "Gotham City";
			var contactName = "Arthur Fleck";
			var customerNotes = "Is it just me, or is it getting crazier out there?";

			var order = new Order()
			{
				SbOrderSeq = orderId,
				DisplayRef = "10-ab-cd",
				ChannelTypeDisplayName = "Etsy",
				OrderedAt = "2017-01-06T22:28:42Z",
				PaidAt = "2017-01-06T22:45:42Z",
				ShippedAt = null,
				SbStatus = "open",
				ShipmentStatus = "none",
				SbPaymentStatus = "all",
				RequestedShippingProvider = carrier,
				RequestedShippingService = carrierClass,
				Total = 10,
				ShippingContactName = contactName,
				ShippingCity = shippingCity,
				ShippingCountryCode = "US",
				ShippingStateRegion = "NY",
				CustomerNotes = customerNotes,
				Items = new OrderItem[]
				{
					new OrderItem() { Sku = "testsku1", Quantity = 10 },
					new OrderItem() { Sku = "testsku2", Quantity = 20 },
				}
			};

			var result = order.ToSvOrder();

			result.Should().NotBeNull();
			result.Id.Should().Be( orderId );
			result.ExternalId.Should().Be( order.DisplayRef );
			result.Total.Should().Be( order.Total );
			result.CreatedAtUtc.Should().Be( order.OrderedAt.FromRFC3339ToUtc() );

			result.Status.Should().Be( SellbriteOrderStatus.Open );
			result.PaymentStatus.Should().Be( SellbriteOrderPaymentStatus.All );
			result.ShipmentStatus.Should().Be( SellbriteOrderShipmentStatus.None );
			
			result.ShippingInfo.Should().NotBeNull();
			result.ShippingInfo.Carrier.Should().Be( carrier );
			result.ShippingInfo.CarrierClass.Should().Be( carrierClass );
			result.ShippingInfo.Address.Should().NotBeNull();
			result.ShippingInfo.Address.City.Should().Be( shippingCity );
			result.ShippingInfo.ContactInfo.Should().NotBeNull();
			result.ShippingInfo.ContactInfo.Name.Should().Be( contactName );
			
			result.Notes.Should().Be( customerNotes );
			result.Items.Count().Should().Be( 2 );
		}
	}
}

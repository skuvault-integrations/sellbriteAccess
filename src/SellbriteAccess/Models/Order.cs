using Newtonsoft.Json;
using SellbriteAccess.Shared;
using System;
using System.Globalization;
using System.Linq;

namespace SellbriteAccess.Models
{
	public class Order
	{
		[ JsonProperty( "display_ref" ) ]
		public string DisplayRef { get; set; }
		[ JsonProperty( "sb_order_seq" ) ]
		public string SbOrderSeq { get; set; }
		[ JsonProperty( "ordered_at" ) ]
		public string OrderedAt { get; set; }
		[ JsonProperty( "customer_notes" ) ]
		public string CustomerNotes { get; set; }
		public OrderItem[] Items { get; set; }
		[ JsonProperty( "shipping_contact_name" ) ]
		public string ShippingContactName { get; set; }
		[ JsonProperty( "shipping_email" ) ]
		public string ShippingEmail { get; set; }
		[ JsonProperty( "shipping_company_name" ) ]
		public string ShippingCompanyName { get; set; }
		[ JsonProperty( "shipping_address_1" ) ]
		public string ShippingAddress1 { get; set; }
		[ JsonProperty( "shipping_address_2" ) ]
		public string ShippingAddress2 { get; set; }
		[ JsonProperty( "shipping_city" ) ]
		public string ShippingCity { get; set; }
		[ JsonProperty( "shipping_postal_code" ) ]
		public string ShippingPostalCode { get; set; }
		[ JsonProperty( "shipping_country_code" ) ]
		public string ShippingCountryCode { get; set; }
		[ JsonProperty( "shipping_state_region" ) ]
		public string ShippingStateRegion { get; set; }
		[ JsonProperty( "shipping_phone_number" ) ]
		public string ShippingPhoneNumber { get; set; }
		[ JsonProperty( "requested_shipping_service" ) ]
		public string RequestedShippingService { get; set; }
		[ JsonProperty( "requested_shipping_provider" ) ]
		public string RequestedShippingProvider { get; set; }
		[ JsonProperty( "shipping_cost" ) ]
		public decimal ShippingCost { get; set; }
		[ JsonProperty( "channel_type_display_name" ) ]
		public string ChannelTypeDisplayName { get; set; }
		public decimal Total { get; set; }
		[ JsonProperty( "sb_status" ) ]
		public string SbStatus { get; set; }
		[ JsonProperty( "sb_payment_status" ) ]
		public string SbPaymentStatus { get; set; }
		[ JsonProperty( "shipment_status" ) ]
		public string ShipmentStatus { get; set; }
	}

	public class OrderItem
	{
		public string Sku { get; set; }
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
		public decimal Tax { get; set; }
	}

	public class SellbriteOrder
	{
		public string ExternalId { get; set; }
		public string ChannelName { get; set; }
		public DateTime CreatedAtUtc { get; set; }
		public string Notes { get; set; }
		public SellbriteOrderItem[] Items { get; set; }
		public SellbriteShippingInfo ShippingInfo { get; set; }
		public decimal Total { get; set; }
		public SellbriteOrderStatus Status { get; set; }
		public SellbriteOrderShipmentStatus ShipmentStatus { get; set; }
		public SellbriteOrderPaymentStatus PaymentStatus { get; set; }
	}

	public class SellbriteOrderItem : OrderItem { }

	public class SellbriteShippingInfo
	{
		public SellbriteShippingContactInfo ContactInfo { get; set; }
		public SellbriteShippingAddress Address { get; set; }
		public string Carrier { get; set; }
		public string CarrierClass { get; set; }
		public decimal Cost { get; set; }
	}

	public class SellbriteShippingAddress
	{
		public string CountryCode { get; set; }
		public string Region { get; set; }
		public string PostalCode { get; set; }
		public string City { get; set; }
		public string Line1 { get; set; }
		public string Line2 { get; set; }
	}

	public class SellbriteShippingContactInfo
	{
		public string Name { get; set; }
		public string CompanyName { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
	}

	public static class OrderExtensions
	{
		public static SellbriteOrder ToSvOrder( this Order order )
		{
			var sellbriteOrder = new SellbriteOrder()
			{
				ExternalId = order.DisplayRef,
				ChannelName = order.ChannelTypeDisplayName,
				CreatedAtUtc = order.OrderedAt.FromRFC3339ToUtc(),
				Notes = order.CustomerNotes,
				Items = order.Items.Select( i => new SellbriteOrderItem() { Sku = i.Sku, Quantity = i.Quantity, Tax = i.Tax, UnitPrice = i.UnitPrice }).ToArray(),
				ShippingInfo = new SellbriteShippingInfo()
				{
					Address = new SellbriteShippingAddress()
					{
						CountryCode = order.ShippingCountryCode,
						Region = order.ShippingStateRegion,
						PostalCode = order.ShippingPostalCode,
						City = order.ShippingCity,
						Line1 = order.ShippingAddress1,
						Line2 = order.ShippingAddress2
					},
					ContactInfo = new SellbriteShippingContactInfo()
					{
						Name = order.ShippingContactName,
						CompanyName = order.ShippingCompanyName,
						Email = order.ShippingEmail,
						Phone = order.ShippingPhoneNumber
					},
					Carrier = order.RequestedShippingProvider,
					CarrierClass = order.RequestedShippingService,
					Cost = order.ShippingCost
				},
				Total = order.Total,
				Status = SellbriteOrderStatus.Open,
				PaymentStatus = SellbriteOrderPaymentStatus.None,
				ShipmentStatus = SellbriteOrderShipmentStatus.None
			};

			var orderStatus = SellbriteOrderStatus.Open;

			if ( order.SbStatus.ToLower().Equals( "canceled" ) )
			{
				order.SbStatus = "cancelled";
			}

			if ( Enum.TryParse( order.SbStatus, true, out orderStatus ) )
			{
				sellbriteOrder.Status = orderStatus;
			}

			var paymentStatus = SellbriteOrderPaymentStatus.None;

			if ( Enum.TryParse( order.SbPaymentStatus, true, out paymentStatus ) )
			{
				sellbriteOrder.PaymentStatus = paymentStatus;
			}
			
			var shipmentStatus = SellbriteOrderShipmentStatus.None;

			if ( Enum.TryParse( order.ShipmentStatus, true, out shipmentStatus ) )
			{
				sellbriteOrder.ShipmentStatus = shipmentStatus;
			}

			return sellbriteOrder;
		}
	}

	public enum SellbriteOrderStatus { Open, Completed, Cancelled }
	public enum SellbriteOrderPaymentStatus { All, None, Partial }
	public enum SellbriteOrderShipmentStatus { All, None, Partial }
}

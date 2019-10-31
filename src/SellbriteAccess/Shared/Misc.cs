using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Xml;

namespace SellbriteAccess.Shared
{
	public static class Misc
	{
		public static string ToJson( this object source )
		{
			try
			{
				if ( source == null )
					return "{}";
				else
				{
					var serialized = JsonConvert.SerializeObject( source, new IsoDateTimeConverter() );
					return serialized;
				}
			}
			catch( Exception )
			{
				return "{}";
			}
		}

		public static string FromUtcToRFC3339( this DateTime dateTimeUtc )
		{
			return XmlConvert.ToString( dateTimeUtc, XmlDateTimeSerializationMode.Utc );
		}

		public static DateTime FromRFC3339ToUtc( this string rfc3339DateTime )
		{
			return XmlConvert.ToDateTime( rfc3339DateTime, XmlDateTimeSerializationMode.Utc );
		}
	}
}

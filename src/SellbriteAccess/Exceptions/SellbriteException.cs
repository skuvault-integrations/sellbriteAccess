using System;

namespace SellbriteAccess.Exceptions
{
	public class SellbriteException : Exception
	{
		public SellbriteException( string message, Exception exception ): base( message, exception ) { }
		public SellbriteException( string message ) : base( message ) { }
	}
}

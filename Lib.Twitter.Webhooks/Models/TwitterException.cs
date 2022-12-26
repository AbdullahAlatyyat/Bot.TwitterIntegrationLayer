using System;

namespace Lib.Twitter.Webhooks.Models
{
    /// <summary>
    /// Exception thrown by Twitter.
    /// </summary>
    public class TwitterException : Exception
    {
        internal TwitterException(string message) : base(message)
        {

        }
    }
}

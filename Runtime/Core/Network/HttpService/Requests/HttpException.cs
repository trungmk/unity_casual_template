using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    /// <summary>  
    /// Exception class for HTTP errors  
    /// </summary>  
    public class HttpException : Exception
    {
        public long StatusCode { get; }

        public HttpException(long statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpException(long statusCode, string message, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}

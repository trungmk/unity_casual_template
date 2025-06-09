namespace Core
{
    /// <summary>  
    /// Standard HTTP methods supported in most HTTP APIs.  
    /// </summary>  
    public enum HTTPMethods : byte
    {
        /// <summary>  
        /// The GET method requests a representation of the specified resource. Requests using GET should only retrieve data.  
        /// </summary>  
        Get,

        /// <summary>  
        /// The HEAD method asks for a response identical to that of a GET request, but without the response body.  
        /// </summary>  
        Head,

        /// <summary>  
        /// The POST method is used to submit an entity to the specified resource, often causing a change in state or side effects.  
        /// </summary>  
        Post,

        /// <summary>  
        /// The PUT method replaces all current representations of the target resource with the request payload.  
        /// </summary>  
        Put,

        /// <summary>  
        /// The DELETE method deletes the specified resource.  
        /// </summary>  
        Delete,

        /// <summary>  
        /// The PATCH method is used to apply partial modifications to a resource.  
        /// See: http://restcookbook.com/HTTP%20Methods/patch/  
        /// </summary>  
        Patch,

        /// <summary>  
        /// The OPTIONS method is used to describe the communication options for the target resource.  
        /// </summary>  
        Options,

        /// <summary>  
        /// The CONNECT method establishes a tunnel to the server identified by the target resource.  
        /// https://tools.ietf.org/html/rfc7231#section-4.3.6  
        /// </summary>  
        Connect,

        /// <summary>  
        /// The TRACE method performs a message loop-back test along the path to the target resource.  
        /// https://tools.ietf.org/html/rfc7231#section-4.3.8  
        /// </summary>  
        Trace
    }
}

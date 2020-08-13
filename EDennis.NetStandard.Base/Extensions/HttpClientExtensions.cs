using Flurl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    public static partial class HttpClientExtensions {

        /// <summary>
        /// Convenience method for invoking a GET request and deserializing the result from JSON.  
        /// This method returns an ObjectResult, which encapsulates
        /// both the response status code and the deserialized response body. 
        /// </summary>
        /// <typeparam name="TResponseBody"></typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>ObjectResult instance holding response status code and deserialized response body</returns>
        /// <see cref="GetAsync{TResponseBody}(HttpClient, string, ScopedRequestMessage)"/>
        public static ObjectResult<TResponseBody> Get<TResponseBody>(this HttpClient client, string relativeUrlFromBase, ScopedRequestMessage msg = null)
            => GetAsync<TResponseBody>(client, relativeUrlFromBase, msg).Result;

        /// <summary>
        /// Convenience method for invoking a GET request and deserializing the result from JSON.  
        /// This method returns an ObjectResult, which encapsulates
        /// both the response status code and the deserialized response body. 
        /// </summary>
        /// <typeparam name="TResponseBody"></typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>ObjectResult instance holding response status code and deserialized response body</returns>
        /// <see cref="Get{TResponseBody}(HttpClient, string, ScopedRequestMessage)"/>
        public static async Task<ObjectResult<TResponseBody>> GetAsync<TResponseBody>(this HttpClient client, string relativeUrlFromBase, ScopedRequestMessage msg = null) {
            msg ??= new ScopedRequestMessage();
            msg.Method = HttpMethod.Get;
            msg.RequestUri = new Uri(Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase) + (msg.Properties["QueryString"] ?? ""));
            var response = await client.SendAsync(msg);
            var objResult = await GenerateObjectResult<TResponseBody>(response);
            return objResult;
        }

        /// <summary>
        /// Convenience method for invoking a DELETE request.  
        /// This method returns a StatusCodeResult. 
        /// NOTE: The type parameter is not used, except to differentiate the method from other extension methods; 
        ///       for documentation purposes only, use the entity class associated with the deleted record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>Response status code as StatusCodeResult</returns>
        /// <see cref="DeleteAsync{T}(HttpClient, string, ScopedRequestMessage)"/>
        public static StatusCodeResult Delete<T>(this HttpClient client, string relativeUrlFromBase, ScopedRequestMessage msg = null)
            => DeleteAsync<T>(client, relativeUrlFromBase, msg).Result;

        /// <summary>
        /// Convenience method for invoking a DELETE request.  
        /// This method returns a StatusCodeResult. 
        /// NOTE: The type parameter is not used, except to differentiate the method from other extension methods; 
        ///       for documentation purposes only, use the entity class associated with the deleted record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>Response status code as StatusCodeResult</returns>
        /// <see cref="Delete{T}(HttpClient, string, ScopedRequestMessage)"/>
        public static async Task<StatusCodeResult> DeleteAsync<T>(this HttpClient client, string relativeUrlFromBase, ScopedRequestMessage msg = null) {
            msg ??= new ScopedRequestMessage();
            msg.Method = HttpMethod.Delete;
            msg.RequestUri = new Uri(Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase) + (msg.Properties["QueryString"] ?? ""));
            var response = await client.SendAsync(msg);
            return new StatusCodeResult((int)response.StatusCode);
        }

        /// <summary>
        /// Convenience method for invoking a POST request.  
        /// This method returns an ObjectResult, which encapsulates
        /// both the response status code and the deserialized response body. 
        /// </summary>
        /// <typeparam name="TBody">The type of the request body and response body (here assumed to be the same)</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>ObjectResult instance holding response status code and deserialized response body</returns>
        /// <see cref="PostAsync{TBody}(HttpClient, string, TBody, ScopedRequestMessage)"/>
        public static ObjectResult<TBody> Post<TBody>(this HttpClient client, string relativeUrlFromBase, TBody body, ScopedRequestMessage msg = null)
            => PostAsync(client, relativeUrlFromBase, body, msg).Result;

        /// <summary>
        /// Convenience method for invoking a POST request.  
        /// This method returns an ObjectResult, which encapsulates
        /// both the response status code and the deserialized response body. 
        /// </summary>
        /// <typeparam name="TBody">The type of the request body and response body (here assumed to be the same)</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>ObjectResult instance holding response status code and deserialized response body</returns>
        /// <see cref="Post{TBody}(HttpClient, string, TBody, ScopedRequestMessage)"/>
        public static async Task<ObjectResult<TBody>> PostAsync<TBody>(this HttpClient client, string relativeUrlFromBase, TBody body, ScopedRequestMessage msg = null) {
            msg ??= new ScopedRequestMessage();
            msg.Method = HttpMethod.Post;
            msg.RequestUri = new Uri(Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase) + (msg.Properties["QueryString"] ?? ""));
            msg.AddJsonContent(body);
            var response = await client.SendAsync(msg);
            var objResult = await GenerateObjectResult<TBody>(response);
            return objResult;
        }

        /// <summary>
        /// Convenience method for invoking a POST request.  
        /// This method returns an ObjectResult, which encapsulates
        /// both the response status code and the deserialized response body. 
        /// </summary>
        /// <typeparam name="TRequestBody">The type of the request body</typeparam>
        /// <typeparam name="TResponseBody">The type of the response body</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>ObjectResult instance holding response status code and deserialized response body</returns>
        /// <see cref="PostAsync{TRequestBody,TResponseBody}(HttpClient, string, TRequestBody, ScopedRequestMessage)"/>
        public static ObjectResult<TResponseBody> Post<TRequestBody, TResponseBody>(this HttpClient client, string relativeUrlFromBase, TRequestBody body, ScopedRequestMessage msg = null)
            => PostAsync<TRequestBody,TResponseBody>(client, relativeUrlFromBase, body, msg).Result;

        /// <summary>
        /// Convenience method for invoking a POST request.  
        /// This method returns an ObjectResult, which encapsulates
        /// both the response status code and the deserialized response body. 
        /// </summary>
        /// <typeparam name="TRequestBody">The type of the request body</typeparam>
        /// <typeparam name="TResponseBody">The type of the response body</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>ObjectResult instance holding response status code and deserialized response body</returns>
        /// <see cref="Post{TRequestBody,TResponseBody}(HttpClient, string, TRequestBody, ScopedRequestMessage)"/>
        public static async Task<ObjectResult<TResponseBody>> PostAsync<TRequestBody, TResponseBody>(this HttpClient client, string relativeUrlFromBase, TRequestBody body, ScopedRequestMessage msg = null) {
            msg ??= new ScopedRequestMessage();
            msg.Method = HttpMethod.Post;
            msg.RequestUri = new Uri(Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase) + (msg.Properties["QueryString"] ?? ""));
            msg.AddJsonContent(body);
            var response = await client.SendAsync(msg);
            var objResult = await GenerateObjectResult<TResponseBody>(response);
            return objResult;
        }

        /// <summary>
        /// Convenience method for invoking a PUT request.  
        /// This method returns an ObjectResult, which encapsulates
        /// both the response status code and the deserialized response body. 
        /// </summary>
        /// <typeparam name="TBody">The type of the request body and response body (here assumed to be the same)</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>ObjectResult instance holding response status code and deserialized response body</returns>
        /// <see cref="PutAsync{TBody}(HttpClient, string, TBody, ScopedRequestMessage)"/>
        public static ObjectResult<TBody> Put<TBody>(this HttpClient client, string relativeUrlFromBase, TBody body, ScopedRequestMessage msg = null)
            => PutAsync(client,relativeUrlFromBase,body,msg).Result;

        /// <summary>
        /// Convenience method for invoking a PUT request.  
        /// This method returns an ObjectResult, which encapsulates
        /// both the response status code and the deserialized response body. 
        /// </summary>
        /// <typeparam name="TBody">The type of the request body and response body (here assumed to be the same)</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>ObjectResult instance holding response status code and deserialized response body</returns>
        /// <see cref="Put{TBody}(HttpClient, string, TBody, ScopedRequestMessage)"/>
        public static async Task<ObjectResult<TBody>> PutAsync<TBody>(this HttpClient client, string relativeUrlFromBase, TBody body, ScopedRequestMessage msg = null) {
            msg ??= new ScopedRequestMessage();
            msg.Method = HttpMethod.Put;
            msg.RequestUri = new Uri(Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase) + (msg.Properties["QueryString"] ?? ""));
            msg.AddJsonContent(body);
            var response = await client.SendAsync(msg);
            var objResult = await GenerateObjectResult<TBody>(response);
            return objResult;
        }

        /// <summary>
        /// Convenience method for invoking a PUT request.  
        /// This method returns an ObjectResult, which encapsulates
        /// both the response status code and the deserialized response body. 
        /// </summary>
        /// <typeparam name="TRequestBody">The type of the request body</typeparam>
        /// <typeparam name="TResponseBody">The type of the response body</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>ObjectResult instance holding response status code and deserialized response body</returns>
        /// <see cref="PutAsync{TBody}(HttpClient, string, TBody, ScopedRequestMessage)"/>
        public static ObjectResult<TResponseBody> Put<TRequestBody,TResponseBody>(this HttpClient client, string relativeUrlFromBase, TRequestBody body, ScopedRequestMessage msg = null)
            => PutAsync<TRequestBody,TResponseBody>(client, relativeUrlFromBase, body, msg).Result;

        /// <summary>
        /// Convenience method for invoking a PUT request.  
        /// This method returns an ObjectResult, which encapsulates
        /// both the response status code and the deserialized response body. 
        /// </summary>
        /// <typeparam name="TRequestBody">The type of the request body</typeparam>
        /// <typeparam name="TResponseBody">The type of the response body</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>ObjectResult instance holding response status code and deserialized response body</returns>
        /// <see cref="Put{TBody}(HttpClient, string, TBody, ScopedRequestMessage)"/>
        public static async Task<ObjectResult<TResponseBody>> PutAsync<TRequestBody,TResponseBody>(this HttpClient client, string relativeUrlFromBase, TRequestBody body, ScopedRequestMessage msg = null) {
            msg ??= new ScopedRequestMessage();
            msg.Method = HttpMethod.Put;
            msg.RequestUri = new Uri(Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase) + (msg.Properties["QueryString"] ?? ""));
            msg.AddJsonContent(body);
            var response = await client.SendAsync(msg);
            var objResult = await GenerateObjectResult<TResponseBody>(response);
            return objResult;
        }

        /// <summary>
        /// Convenience method for invoking a PATCH request.  
        /// This method returns an ObjectResult, which encapsulates
        /// both the response status code and the deserialized response body. 
        /// </summary>
        /// <typeparam name="TBody">The type of the request body and response body (here assumed to be the same)</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>ObjectResult instance holding response status code and deserialized response body</returns>
        /// <see cref="PatchAsync{TBody}(HttpClient, string, TBody, ScopedRequestMessage)"/>
        public static ObjectResult<TBody> Patch<TBody>(this HttpClient client, string relativeUrlFromBase, TBody body, ScopedRequestMessage msg = null)
            => PatchAsync(client, relativeUrlFromBase, body, msg).Result;

        /// <summary>
        /// Convenience method for invoking a PATCH request.  
        /// This method returns an ObjectResult, which encapsulates
        /// both the response status code and the deserialized response body. 
        /// </summary>
        /// <typeparam name="TBody">The type of the request body and response body (here assumed to be the same)</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>ObjectResult instance holding response status code and deserialized response body</returns>
        /// <see cref="Patch{TBody}(HttpClient, string, TBody, ScopedRequestMessage)"/>
        public static async Task<ObjectResult<TBody>> PatchAsync<TBody>(this HttpClient client, string relativeUrlFromBase, TBody body, ScopedRequestMessage msg = null) {
            msg ??= new ScopedRequestMessage();
            msg.Method = HttpMethod.Patch;
            msg.RequestUri = new Uri(Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase) + (msg.Properties["QueryString"] ?? ""));
            msg.AddJsonContent(body);
            var response = await client.SendAsync(msg);
            var objResult = await GenerateObjectResult<TBody>(response);
            return objResult;
        }

        /// <summary>
        /// Convenience method for invoking a PATCH request.  
        /// This method returns an ObjectResult, which encapsulates
        /// both the response status code and the deserialized response body. 
        /// </summary>
        /// <typeparam name="TRequestBody">The type of the request body</typeparam>
        /// <typeparam name="TResponseBody">The type of the response body</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>ObjectResult instance holding response status code and deserialized response body</returns>
        /// <see cref="PatchAsync{TBody}(HttpClient, string, TBody, ScopedRequestMessage)"/>
        public static ObjectResult<TResponseBody> Patch<TRequestBody, TResponseBody>(this HttpClient client, string relativeUrlFromBase, TRequestBody body, ScopedRequestMessage msg = null)
            => PatchAsync<TRequestBody, TResponseBody>(client, relativeUrlFromBase, body, msg).Result;

        /// <summary>
        /// Convenience method for invoking a PATCH request.  
        /// This method returns an ObjectResult, which encapsulates
        /// both the response status code and the deserialized response body. 
        /// </summary>
        /// <typeparam name="TRequestBody">The type of the request body</typeparam>
        /// <typeparam name="TResponseBody">The type of the response body</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="msg">An optional starting Message (derived from HttpRequestMessage),
        /// which allow prepopulating headers, cookies (as headers), and query string (as Properties["QueryString"])</param>
        /// <returns>ObjectResult instance holding response status code and deserialized response body</returns>
        /// <see cref="Patch{TBody}(HttpClient, string, TBody, ScopedRequestMessage)"/>
        public static async Task<ObjectResult<TResponseBody>> PatchAsync<TRequestBody, TResponseBody>(this HttpClient client, string relativeUrlFromBase, TRequestBody body, ScopedRequestMessage msg = null) {
            msg ??= new ScopedRequestMessage();
            msg.Method = HttpMethod.Patch;
            msg.RequestUri = new Uri(Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase) + (msg.Properties["QueryString"] ?? ""));
            msg.AddJsonContent(body);
            var response = await client.SendAsync(msg);
            var objResult = await GenerateObjectResult<TResponseBody>(response);
            return objResult;
        }


        /// <summary>
        /// Forwards a request through the provided HttpClient to another endpoint.
        /// NOTE: This overload of the Forward method is suitable for use with GET or DELETE.
        /// NOTE: This method is especially helpful for dealing with complex query strings.
        /// </summary>
        /// <typeparam name="TResponseBody">The type of the response body</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="request">The incoming HttpRequest object</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="options">Optionally specify certain aspects of the request to copy</param>
        /// <returns>A ObjectResult consisting of a status code and a deserialized response body.</returns>
        /// <see cref="ForwardAsync{T}(HttpClient, HttpRequest, string, RequestForwardingOptions)"/>
        public static ObjectResult<TResponseBody> Forward<TResponseBody>(this HttpClient client, HttpRequest request, string relativeUrlFromBase, RequestForwardingOptions options = null)
            => ForwardAsync<TResponseBody>(client, request, relativeUrlFromBase, options).Result;


        /// <summary>
        /// Forwards a request through the provided HttpClient to another endpoint.
        /// NOTE: This overload of the Forward method is suitable for use with GET or DELETE.
        /// NOTE: This method is especially helpful for dealing with complex query strings.
        /// </summary>
        /// <typeparam name="TResponseBody">The type of the response body</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="request">The incoming HttpRequest object</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="options">Optionally specify certain aspects of the request to copy</param>
        /// <returns>A ObjectResult consisting of a status code and a deserialized response body.</returns>
        /// <see cref="Forward{T}(HttpClient, HttpRequest, string, RequestForwardingOptions)"/>
        public static async Task<ObjectResult<TResponseBody>> ForwardAsync<TResponseBody>(this HttpClient client, HttpRequest request, string relativeUrlFromBase, RequestForwardingOptions options = null) {
            var msg = request.ToHttpRequestMessage(client, options);
            var url = relativeUrlFromBase + (msg.Properties["QueryString"] ?? "");
            return await ForwardRequestAsync<TResponseBody>(client, msg, url);
        }


        /// <summary>
        /// Forwards a request through the provided HttpClient to another endpoint.
        /// NOTE: This overload of the Forward method takes a TBody object representing a request body;
        ///       however, the method only specifies a single type parameter, which represents the
        ///       type for both the request and response body; accordingly, the method may be 
        ///       most suitable for use with POST or PUT.
        /// </summary>
        /// <typeparam name="TBody">The type of the request body and response body (here, assumed to be the same)</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="request">The incoming HttpRequest object</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="body">The request body (prior to serialization)</param>
        /// <param name="options">Optionally specify certain aspects of the request to copy</param>
        /// <returns>A ObjectResult consisting of a status code and a deserialized response body.</returns>
        /// <see cref="ForwardAsync{TBody}(HttpClient, HttpRequest, string, TBody, RequestForwardingOptions)"/>
        public static ObjectResult<TBody> Forward<TBody>(this HttpClient client, HttpRequest request, string relativeUrlFromBase, TBody body, RequestForwardingOptions options = null)
            => ForwardAsync(client, request, relativeUrlFromBase, body, options).Result;


        /// <summary>
        /// Forwards a request through the provided HttpClient to another endpoint.
        /// NOTE: This overload of the Forward method takes a TBody object representing a request body;
        ///       however, the method only specifies a single type parameter, which represents the
        ///       type for both the request and response body; accordingly, the method may be 
        ///       most suitable for use with POST or PUT.
        /// </summary>
        /// <typeparam name="TBody">The type of the request body and response body (here, assumed to be the same)</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="request">The incoming HttpRequest object</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="body">The request body (prior to serialization)</param>
        /// <param name="options">Optionally specify certain aspects of the request to copy</param>
        /// <returns>A ObjectResult consisting of a status code and a deserialized response body.</returns>
        /// <see cref="Forward{TBody}(HttpClient, HttpRequest, string, TBody, RequestForwardingOptions)"/>
        public static async Task<ObjectResult<TBody>> ForwardAsync<TBody>(this HttpClient client, HttpRequest request, string relativeUrlFromBase, TBody body, RequestForwardingOptions options = null) {
            var msg = request.ToHttpRequestMessage(client, body, options);
            var url = relativeUrlFromBase + (msg.Properties["QueryString"] ?? "");
            return await ForwardRequestAsync<TBody>(client, msg, url);
        }

        /// <summary>
        /// Forwards a request through the provided HttpClient to another endpoint.
        /// NOTE: This overload of the Forward method takes a TRequestBody object representing a request body
        ///       and returns a TResponseBody object representing the response body.  The method is
        ///       particularly well-suited for PATCH scenarios.
        /// </summary>
        /// <typeparam name="TRequestBody">The type of the request body</typeparam>
        /// <typeparam name="TResponseBody">The type of the response body</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="request">The incoming HttpRequest object</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="body">The request body (prior to serialization)</param>
        /// <param name="options">Optionally specify certain aspects of the request to copy</param>
        /// <returns>A ObjectResult consisting of a status code and a deserialized response body.</returns>
        /// <see cref="ForwardAsync{TRequestBody,TResponseBody}(HttpClient, HttpRequest, string, TRequestBody, RequestForwardingOptions)"/>
        public static ObjectResult<TResponseBody> Forward<TRequestBody, TResponseBody>(this HttpClient client, HttpRequest request, string relativeUrlFromBase, TRequestBody body, RequestForwardingOptions options = null)
            => ForwardAsync<TRequestBody, TResponseBody>(client, request, relativeUrlFromBase, body, options).Result;

        /// <summary>
        /// Forwards a request through the provided HttpClient to another endpoint.
        /// NOTE: This overload of the Forward method takes a TRequestBody object representing a request body
        ///       and returns a TResponseBody object representing the response body.  The method is
        ///       particularly well-suited for PATCH scenarios.
        /// </summary>
        /// <typeparam name="TRequestBody">The type of the request body</typeparam>
        /// <typeparam name="TResponseBody">The type of the response body</typeparam>
        /// <param name="client">The HttpClient making the request</param>
        /// <param name="request">The incoming HttpRequest object</param>
        /// <param name="relativeUrlFromBase">Relative URL from the HttpClient's base url</param>
        /// <param name="body">The request body (prior to serialization)</param>
        /// <param name="options">Optionally specify certain aspects of the request to copy</param>
        /// <returns>A ObjectResult consisting of a status code and a deserialized response body.</returns>
        /// <see cref="Forward{TRequestBody,TResponseBody}(HttpClient, HttpRequest, string, TRequestBody, RequestForwardingOptions)"/>
        public static async Task<ObjectResult<TResponseBody>> ForwardAsync<TRequestBody, TResponseBody>(this HttpClient client, HttpRequest request, string relativeUrlFromBase, TRequestBody body, RequestForwardingOptions options = null) {
            var msg = request.ToHttpRequestMessage(client, body, options);
            var url = relativeUrlFromBase + (msg.Properties["QueryString"] ?? "");
            return await ForwardRequestAsync<TResponseBody>(client, msg, url);
        }


        /// <summary>
        /// Pings an HttpClient's BaseAddress endpoint
        /// </summary>
        /// <param name="client">The HttpClient whose BaseAddress is pinged</param>
        /// <param name="timeoutSeconds">How long to wait before failing</param>
        /// <returns>true if the endpoint can be reached; false otherwise</returns>
        public static bool Ping(this HttpClient client, int timeoutSeconds = 5) 
            => client.PingAsync(timeoutSeconds).Result;



        /// <summary>
        /// Pings an HttpClient's BaseAddress endpoint
        /// </summary>
        /// <param name="client">The HttpClient whose BaseAddress is pinged</param>
        /// <param name="timeoutSeconds">How long to wait before failing</param>
        /// <returns>true if the endpoint can be reached; false otherwise</returns>
        public static async Task<bool> PingAsync(this HttpClient client, int timeoutSeconds = 5) {

            var pingable = false;

            await Task.Run(() =>
            {

                var port = client.BaseAddress.Port;
                var host = client.BaseAddress.Host;
                var sw = new Stopwatch();

                sw.Start();
                while (sw.ElapsedMilliseconds < (timeoutSeconds * 1000)) {
                    try {
                        using var tcp = new TcpClient(host, port);
                        var connected = tcp.Connected;
                        pingable = true;
                        break;
                    } catch (Exception ex) {
                        if (!ex.Message.Contains("No connection could be made because the target machine actively refused it"))
                            throw ex;
                        else
                            Thread.Sleep(1000);
                    }

                }

            });
            return pingable;
        }


        /// <summary>
        /// Internal method for forwarding a request.
        /// </summary>
        /// <typeparam name="TResponseBody"></typeparam>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <param name="relativeUrlFromBase"></param>
        /// <returns></returns>
        private static async Task<ObjectResult<TResponseBody>> ForwardRequestAsync<TResponseBody>(this HttpClient client, HttpRequestMessage msg, string relativeUrlFromBase) {
            var url = new Url(client.BaseAddress)
                  .AppendPathSegment(relativeUrlFromBase);

            url = WebUtility.UrlDecode(url);

            var uri = url.ToUri();
            msg.RequestUri = uri;
            var response = await client.SendAsync(msg);
            var objResult = await GenerateObjectResult<TResponseBody>(response);

            return objResult;

        }



        /// <summary>
        /// Internal method for building an HttpRequestMessage from a parent HttpRequest
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="client"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static HttpRequestMessage ToHttpRequestMessage(this HttpRequest httpRequest, HttpClient client, RequestForwardingOptions options = null) {
            var msg = new HttpRequestMessage();
            msg
                .CopyMethod(httpRequest)
                .CopyHeaders(httpRequest, client, options)
                .CopyCookies(httpRequest, options);

            if (options.ForwardQueryString)
                msg.CopyQueryString(httpRequest);

            return msg;
        }


        /// <summary>
        /// Internal method for building an HttpRequestMessage from a parent HttpRequest
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="client"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static HttpRequestMessage ToHttpRequestMessage<TRequestBody>(this HttpRequest httpRequest, HttpClient client, TRequestBody body, RequestForwardingOptions options = null) {
            var msg = new HttpRequestMessage();
            msg
                .CopyMethod(httpRequest)
                .CopyHeaders(httpRequest, client, options)
                .CopyCookies(httpRequest, options);

            if (options.ForwardQueryString)
                msg.CopyQueryString(httpRequest);

            msg.AddJsonContent(body);

            return msg;
        }



        /// <summary>
        /// Internal method for serializing an object as JSON and adding it as request body content
        /// </summary>
        /// <typeparam name="TRequestBody"></typeparam>
        /// <param name="msg"></param>
        /// <param name="body"></param>
        private static void AddJsonContent<TRequestBody>(this HttpRequestMessage msg, TRequestBody body) {
            var json = JsonSerializer.Serialize(body);
            msg.Content = new StringContent(json,
                    Encoding.UTF8, "application/json");

        }


        /// <summary>
        /// Internal method for copying the HTTP Method from an HttpRequest to an HttpRequestMessage
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public static HttpRequestMessage CopyMethod(this HttpRequestMessage msg, HttpRequest req) {
            if (req.Method.ToUpper() == "POST")
                msg.Method = HttpMethod.Post;
            else if (req.Method.ToUpper() == "PUT")
                msg.Method = HttpMethod.Put;
            else if (req.Method.ToUpper() == "DELETE")
                msg.Method = HttpMethod.Delete;
            else if (req.Method.ToUpper() == "GET")
                msg.Method = HttpMethod.Get;
            else if (req.Method.ToUpper() == "HEAD")
                msg.Method = HttpMethod.Head;

            return msg;
        }


        /// <summary>
        /// Internal method for copying Headers from an HttpRequest to an HttpRequestMessage
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="req"></param>
        /// <param name="client"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static HttpRequestMessage CopyHeaders(this HttpRequestMessage msg, HttpRequest req, HttpClient client, RequestForwardingOptions options = null) {
            options ??= new RequestForwardingOptions();
            var currentHeaders = client.DefaultRequestHeaders.Select(x => x.Key);
            var headers = req.Headers.Where(h => options.HeadersToForward.Contains(h.Key) && !currentHeaders.Contains(h.Key));
            foreach (var header in headers) {
                if (header.Key.StartsWith(":"))
                    msg.Headers.Add(header.Key.Substring(1), header.Value.AsEnumerable());
                else
                    msg.Headers.Add(header.Key, header.Value.AsEnumerable());
            }
            msg.Headers.Host = client.BaseAddress.Host;
            return msg;
        }


        /// <summary>
        /// Internal method for copying Headers from an HttpRequest to an HttpRequestMessage
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="req"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static HttpRequestMessage CopyHeaders(this HttpRequestMessage msg, HttpRequest req, RequestForwardingOptions options = null) {
            options ??= new RequestForwardingOptions();
            var headers = req.Headers.Where(h => options.HeadersToForward.Contains(h.Key));
            foreach (var header in headers) {
                if (header.Key.StartsWith(":"))
                    msg.Headers.Add(header.Key.Substring(1), header.Value.AsEnumerable());
                else
                    msg.Headers.Add(header.Key, header.Value.AsEnumerable());
            }
            return msg;
        }


        /// <summary>
        /// Internal method for copying a query string from an HttpRequest to the Properties property of an HttpRequestMessage
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public static HttpRequestMessage CopyQueryString(this HttpRequestMessage msg, HttpRequest req) {
            msg.Properties.Add("QueryString", req.QueryString);
            return msg;
        }


        /// <summary>
        /// Internal method for copying cookies from an HttpRequest to an HttpRequestMessage's headers
        /// NOTE: Assumes that the HttpClientHandler is set as such
        /// <code>
        /// services.AddHttpClient<ColorApiClient>(options => {
        //      options.BaseAddress = new Uri(SOME_URI);
        //  }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
        //      UseCookies = false
        //  });
        /// </code>
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="req"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static HttpRequestMessage CopyCookies(this HttpRequestMessage msg, HttpRequest req, RequestForwardingOptions options = null) {
            options ??= new RequestForwardingOptions();
            if (req.Cookies != null && req.Cookies.Count > 0) {
                var sb = new StringBuilder();
                foreach (var cookie in req.Cookies.Where(c => options.CookiesToForward.Contains(c.Key))) {
                    sb.Append(cookie.Key);
                    sb.Append("=");
                    sb.Append(cookie.Value);
                    sb.Append("; ");
                }
                msg.Headers.Add("Cookie", sb.ToString().TrimEnd());
            }
            return msg;
        }


        /// <summary>
        /// Internal method for deserializing an HttpResponse body.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        private async static Task<ObjectResult<T>> GenerateObjectResult<T>(HttpResponseMessage response) {

            object value = null;

            int statusCode = (int)response.StatusCode;

            if (response.Content.Headers.ContentLength > 0) {
                var json = await response.Content.ReadAsStringAsync();

                if (statusCode < 299 && typeof(T) != typeof(string)) {
                    var options = new JsonSerializerOptions {
                        PropertyNameCaseInsensitive = true
                    };
                    value = JsonSerializer.Deserialize<T>(json, options);
                } else {
                    value = json;
                }
            }

            if (statusCode < 299)
                return new ObjectResult<T>(value) {
                    StatusCode = statusCode
                };
            else {
                var result = new ObjectResult<T>(default) {
                    StatusCode = statusCode,
                    Value = value
                };
                return result;
            }

        }

    }
}


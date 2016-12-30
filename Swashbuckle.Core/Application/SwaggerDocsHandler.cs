﻿using System;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using System.Collections.Generic;
using Swashbuckle.Swagger;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Swashbuckle.Application
{
    public class SwaggerDocsHandler : HttpMessageHandler
    {
        private readonly SwaggerDocsConfig _config;
        public Func<bool> IsAuthenticated;

        public SwaggerDocsHandler(SwaggerDocsConfig config)
        {
            _config = config;
            IsAuthenticated = CheckThreadAuthentication;
        }

        private bool CheckThreadAuthentication()
        {
            return Thread.CurrentPrincipal.Identity.IsAuthenticated;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!IsAuthenticated())
            {
                var response = request.CreateResponse(HttpStatusCode.Unauthorized);
                return Task.FromResult(response);
            }

            var swaggerProvider = _config.GetSwaggerProvider(request);
            var rootUrl = _config.GetRootUrl(request);
            var apiVersion = request.GetRouteData().Values["apiVersion"].ToString();

            try
            {
                var swaggerDoc = swaggerProvider.GetSwagger(rootUrl, apiVersion);
                var content = ContentFor(request, swaggerDoc);
                return Task.FromResult(new HttpResponseMessage { Content = content });
            }
            catch (UnknownApiVersion ex)
            {
                return Task.FromResult(request.CreateErrorResponse(HttpStatusCode.NotFound, ex));
            }
        }

        private HttpContent ContentFor(HttpRequestMessage request, SwaggerDocument swaggerDoc)
        {
            var negotiator = request.GetConfiguration().Services.GetContentNegotiator();
            var result = negotiator.Negotiate(typeof(SwaggerDocument), request, GetSupportedSwaggerFormatters());

            return new ObjectContent(typeof(SwaggerDocument), swaggerDoc, result.Formatter, result.MediaType);
        }

        private IEnumerable<MediaTypeFormatter> GetSupportedSwaggerFormatters()
        {
            var jsonFormatter = new JsonMediaTypeFormatter
            {
                SerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = _config.GetFormatting(),
                    Converters = new[] { new VendorExtensionsConverter() }
                }
            };
            // NOTE: The custom converter would not be neccessary in Newtonsoft.Json >= 5.0.5 as JsonExtensionData
            // provides similar functionality. But, need to stick with older version for WebApi 5.0.0 compatibility 
            return new[] { jsonFormatter };
        }
    }
}

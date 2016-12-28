using System;
using System.Net.Http;
using System.Web.Http;
using WebActivatorEx;
using $rootnamespace$;
using Swashbuckle.Application;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace $rootnamespace$
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            GlobalConfiguration.Configuration 
                .EnableSwagger(c =>
                    {
                        c.RootUrl(GetRootUrl);
                        c.Schemes(new[] { "https" });
                        c.SingleApiVersion("v1", "Auth Server")
                            .Description("")
                            .TermsOfService("Terms of Service sample");
                        c.IncludeXmlComments(GetXmlCommentsPath());
                        c.DescribeAllEnumsAsStrings();
                    });
        }

		private static string GetRootUrl(HttpRequestMessage req)
		{
			return new Uri(req.RequestUri, req.GetRequestContext().VirtualPathRoot).ToString();
		}

		private static string GetXmlCommentsPath() => $@"{AppDomain.CurrentDomain.BaseDirectory}\bin\$rootnamespace$.XML";
	}
}
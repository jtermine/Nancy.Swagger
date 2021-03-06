﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using HttpMultipartParser;
using Nancy.ModelBinding;
using Nancy.Swagger.Annotations.Attributes;
using Nancy.Swagger.Autofac.Demo.Models;
using Nancy.Swagger.Services;
using Swagger.ObjectModel;

namespace Nancy.Swagger.Autofac.Demo.Modules
{
    public class ServiceDetailsModule : NancyModule
    {
        private const string ServiceTagName = "Service Details";
        private const string ServiceTagDescription = "Operations for handling the service";

        public ServiceDetailsModule(ISwaggerModelCatalog modelCatalog, ISwaggerTagCatalog tagCatalog) : base("/service")
        {
            modelCatalog.AddModel<ServiceOwner>();

            tagCatalog.AddTag(new Tag()
            {
                Name = ServiceTagName,
                Description = ServiceTagDescription
            });

            Get("/", _ => GetHome(), null, "ServiceHome");

            Get("/details", _ => GetServiceDetails(), null, "GetDetails");

            Get("/customers", _ => GetServiceCustomers(), null, "GetCustomers");

            Get("/customers/{name}", parameters => GetServiceCustomer(parameters.name), null, "GetCustomer");

            Post("/customer/{service}", parameters => PostServiceCustomer(parameters.service, this.Bind<ServiceCustomer>()), null, "PostNewCustomer");
            
            Post("/customer/{name}/file", parameters => PostCustomerReview(parameters.name, null), null, "PostCustomerReview");
            
        }


        [Route("ServiceHome")]
        [Route(HttpMethod.Get, "/")]
        [Route(Summary = "Get Service Home")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "OK")]
        [Route(Tags = new[] { ServiceTagName })]
        private string GetHome()
        {
            return "Hello again, Swagger!";
        }


        [Route("GetDetails")]
        [Route(HttpMethod.Get, "/details")]
        [Route(Summary = "Get Service Details")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "OK", Model = typeof(ServiceDetails))]
        [Route(Tags = new[] { ServiceTagName })]
        private ServiceDetails GetServiceDetails()
        {
            return new ServiceDetails()
            {
                Name = "Nancy Swagger Service",
                Owner = new ServiceOwner()
                {
                    CompanyName = "Swagger Example Inc.",
                    CompanyContactEmail = "company@swaggerexample.inc"
                },
                Customers = new[]
                {
                    new ServiceCustomer() {CustomerName = "Jack"},
                    new ServiceCustomer() {CustomerName = "Jill"}
                }
            };
        }
        
        [Route("GetCustomers")]
        [Route(HttpMethod.Get, "/customers")]
        [Route(Summary = "Get Service Customers")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "OK", Model = typeof(IEnumerable<ServiceCustomer>))]
        [Route(Tags = new[] { ServiceTagName })]
        private ServiceCustomer[] GetServiceCustomers()
        {
            return new[]
            {
                new ServiceCustomer() {CustomerName = "Jack"},
                new ServiceCustomer() {CustomerName = "Jill"}
            };
        }

        [Route("GetCustomer")]
        [Route(HttpMethod.Get, "/customers/{name}")]
        [Route(Summary = "Get Service Customer")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "OK", Model = typeof(ServiceCustomer))]
        [Route(Tags = new[] { ServiceTagName })]
        private ServiceCustomer GetServiceCustomer([RouteParam(ParameterIn.Path, DefaultValue = "Jack")] string name)
        {
            return new ServiceCustomer() { CustomerName = name, CustomerEmail = name + "@my-service.com" };
        }

        [Route("PostNewCustomer")]
        [Route(HttpMethod.Post, "/customer/{service}")]
        [Route(Summary = "Post Service Customer")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "OK", Model = typeof(ServiceCustomer))]
        [Route(Produces = new[] { "application/json" })]
        [Route(Consumes = new[] { "application/json", "application/xml" })]
        [Route(Tags = new[] { ServiceTagName })]
        private ServiceCustomer PostServiceCustomer(
            [RouteParam(ParameterIn.Path, DefaultValue = "my-service")] string service, 
            [RouteParam(ParameterIn.Body)] ServiceCustomer customer)
        {
            return customer;
        }

        [Route("PostCustomerReview")]
        [Route(HttpMethod.Post, "/customer/{name}/file")]
        [Route(Summary = "Post Customer Review")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "OK", Model = typeof(SwaggerFile))]
        [Route(Tags = new[] { ServiceTagName })]
        [Route(Consumes = new[] { "multipart/form-data" })]
        private string PostCustomerReview(
            [RouteParam(ParameterIn.Path, DefaultValue = "Jill")] string name,
            [RouteParam(ParameterIn.Form)] SwaggerFile file) //'file' is unused, but needed for the swagger doc
        {

            var parsed = new MultipartFormDataParser(Request.Body);
            var uploadedFile = parsed.Files.FirstOrDefault()?.Data;
            if (uploadedFile == null)
            {
                return "File Parsing Failed";
            }
            var reader = new StreamReader(uploadedFile);
            return reader.ReadToEnd();
        }
    }
}
using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Next.Cqrs.Commands;

namespace Next.Web.Application.Error
{
    internal class ProblemDetailsBuilder: IProblemDetailsBuilder
    {
        private IServiceCollection Services { get; }

        public ProblemDetailsBuilder(IServiceCollection services)
        {
            Services = services;
        }
        
        public IProblemDetailsBuilder AddProfiles(Assembly assembly)
        {
            var problemDetailsProfileTypes = assembly
                .GetTypes()
                .Where(t => !t.GetTypeInfo().IsAbstract &&
                            typeof(IProblemDetailsProfile)
                                .GetTypeInfo()
                                .IsAssignableFrom(t))
                .ToList();


            foreach (var problemDetailsProfileType in problemDetailsProfileTypes)
            {
                Services
                    .AddSingleton(typeof(IProblemDetailsProfile),problemDetailsProfileType);
            }
            
            var problemDetailsExceptionProfileTypes = assembly
                .GetTypes()
                .Where(t => !t.GetTypeInfo().IsAbstract &&
                            typeof(IProblemDetailsExceptionProfile)
                                .GetTypeInfo()
                                .IsAssignableFrom(t))
                .ToList();


            foreach (var problemDetailsExceptionProfileType in problemDetailsExceptionProfileTypes)
            {
                Services
                    .AddSingleton(typeof(IProblemDetailsExceptionProfile),problemDetailsExceptionProfileType);
            }
            return this;
        }

        public IProblemDetailsBuilder AddProfile<TProfile>() 
            where TProfile : class, IProblemDetailsProfile
        {
            Services
                .AddSingleton<IProblemDetailsProfile, TProfile>();
            return this;
        }

        public IProblemDetailsBuilder AddExceptionProfile<TExceptionProfile>() 
            where TExceptionProfile : class, IProblemDetailsExceptionProfile
        {
            Services
                .AddSingleton<IProblemDetailsExceptionProfile, TExceptionProfile>();
            return this;
        }

        public IProblemDetailsBuilder MapError<TException>(Action<ProblemDetails, TException> func)
            where TException : Exception
        {
            Services
                .AddSingleton<IProblemDetailsExceptionProfile>(new FuncProblemDetailsExceptionProfile<TException>(func));
            return this;
        }

        public IProblemDetailsBuilder Map<TCommandResponse>(Action<ProblemDetails, TCommandResponse> func) 
            where TCommandResponse : ICommandResponse
        {
            Services
                .AddSingleton<IProblemDetailsProfile>(new FuncProblemDetailsProfile<TCommandResponse>(func));
            return this;
        }

        public IProblemDetailsBuilder Map(
            string errorCode,
            int statusCode)
        {
            Services
                .AddSingleton<IProblemDetailsProfile>(new StatusCodeProblemDetailsProfile(
                    errorCode,
                    statusCode));
            return this;
        }
    }
}
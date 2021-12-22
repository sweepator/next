using Automapper = AutoMapper;
using System;
using Next.Abstractions.Mapper.Exceptions;
using IMapper = Next.Abstractions.Mapper.IMapper;

namespace Next.Mapper.AutoMapper
{
    public class Mapper : IMapper
    {
        private readonly Automapper.IMapper _mapperObject;

        public Mapper(params Automapper.Profile[] profiles)
        {
            _mapperObject = this.Init(profiles);
        }

        public TDestination Map<TDestination>(object source)
        {
            try
            {
                return _mapperObject.Map<TDestination>(source);
            }
            catch (Exception ex)
            {
                throw new MappingException(ex);
            }
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            try
            {
                return _mapperObject.Map<TSource, TDestination>(source);
            }
            catch (Exception ex)
            {
                throw new MappingException(ex);
            }
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            try
            {
                return _mapperObject.Map<TSource, TDestination>(source, destination);
            }
            catch (Exception ex)
            {
                throw new MappingException(ex);
            }
        }

        private Automapper.IMapper Init(params Automapper.Profile[] profiles)
        {
            try
            {
                var config = new Automapper.MapperConfiguration(cfg =>
                {
                    cfg.AllowNullCollections = true;

                    if (profiles != null)
                    {
                        foreach (var profile in profiles)
                        {
                            cfg.AddProfile(profile);
                        }
                    }
                });

                var mapper = config.CreateMapper();
                config.AssertConfigurationIsValid();
                return mapper;
            }
            catch (Exception ex)
            {
                throw new MappingException(ex);
            }
        }
    }
}

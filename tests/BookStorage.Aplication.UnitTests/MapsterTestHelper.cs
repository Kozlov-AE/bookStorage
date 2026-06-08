using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Mapster;
using MapsterMapper;

namespace BookStorage.Aplication.UnitTests
{
    public class MapsterTestHelper
    {
        public static Mapper GetMapperForTests()
        {
            var config = new TypeAdapterConfig();

            config.Scan(typeof(Api.Mapping.MapsterProfiles.MappingProfile).Assembly);

            return new Mapper(config);
        }
    }
}

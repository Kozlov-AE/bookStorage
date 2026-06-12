using BookStorage.Api.DTOs;
using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Application;
using Microsoft.AspNetCore.Mvc;

namespace BookStorage.Api.Modules;

public static class PersonsModule
{
   public static void MapPersons(this IEndpointRouteBuilder app)
   {
      var group = app.MapGroup("/api/Persons").WithTags("Persons");
      group.MapPost("/", CreatePerson);
      group.MapGet("/", GetAllPersons)
          .Produces<IEnumerable<PersonDto>>();
      group.MapGet("/search", SearchPersons)
          .Produces<IEnumerable<PersonDto>>();
   }

private static async Task<IResult> CreatePerson([FromBody] PersonDto request, IPersonService ps, IMapper mapper, CancellationToken ct)
    {
       var person = mapper.Map<PersonDto, Person>(request);
       var result = await ps.CreateAsync(person, ct);
       if (result != null)
       {
          return Results.Ok(mapper.Map<Person, PersonDto>(result));
       }
       else
       {
          return Results.BadRequest("Failed to create person");
       }
    }

private static async Task<IResult> GetAllPersons(IPersonService ps, IMapper mapper, CancellationToken ct)
    {
       var persons = await ps.GetAllAsync(ct);
       var personsDto = mapper.Map<IEnumerable<Person>, IEnumerable<PersonDto>>(persons);

       return Results.Ok(personsDto);
    }

private static async Task<IResult> SearchPersons(
        [FromQuery] string query,
        IPersonService ps,
        IMapper mapper,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Results.Ok(Enumerable.Empty<PersonDto>());

        var persons = await ps.SearchAsync(query, ct);
        var personsDto = mapper.Map<IEnumerable<Person>, IEnumerable<PersonDto>>(persons);
        return Results.Ok(personsDto);
    }
}

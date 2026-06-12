using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BookStorage.Infrastructure.Persistence;


 
public class GuidConverter : ValueConverter<Guid, byte[]>
{
    public readonly static GuidConverter Instance = new GuidConverter();
 
    public GuidConverter()
        : base(x => x.ToByteArray(), x => new Guid(x))
    {
    }
}
 
public class NullableGuidConverter : ValueConverter<Guid?, byte[]?>
{
    public readonly static NullableGuidConverter Instance = new NullableGuidConverter();
 
    public NullableGuidConverter()
        : base(
            x => x != null ? x.Value.ToByteArray() : null,
            x => x != null ? new Guid(x) : null)
    {
    }
}
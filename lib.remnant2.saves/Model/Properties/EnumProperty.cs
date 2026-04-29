using System.Diagnostics.CodeAnalysis;
using lib.remnant2.saves.Model.Memory;
using lib.remnant2.saves.Model.Parts;
using lib.remnant2.saves.Model.Properties.Parts;
using Serilog;

namespace lib.remnant2.saves.Model.Properties;

public class EnumProperty : ModelBase
{
    public static ILogger Logger => Log.Logger.ForContext(Log.Category, Log.Parser).ForContext<EnumProperty>();
    public required FName EnumType;
    public required byte HasPropertyGuid;
    public FGuid? PropertyGuid;
    public required FName EnumValue;

    public EnumProperty()
    {
    }

    [SetsRequiredMembers]
    public EnumProperty(Reader r, SerializationContext ctx)
    {
        ReadOffset = r.Position + ctx.ContainerOffset;
        EnumType = new(r, ctx.NamesTable);
        (HasPropertyGuid, PropertyGuid) = PropertyValue.ReadPropertyGuid(r);
        EnumValue = new(r, ctx.NamesTable);
        ReadLength = r.Position + ctx.ContainerOffset - ReadOffset;
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        EnumType.Write(w, ctx);
        PropertyValue.WritePropertyGuid(w, HasPropertyGuid, PropertyGuid);
        EnumValue.Write(w, ctx);
        WriteLength = (int)w.Position + ctx.ContainerOffset - WriteOffset;
    }

    public override string ToString()
    {
        return EnumValue.ToString();
    }
    public override IEnumerable<(ModelBase obj, int? index)> GetChildren()
    {
        yield break;
    }
}

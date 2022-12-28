namespace Reed;

[AttributeUsage(AttributeTargets.Method)]
public class ResilientAttribute<T> : Attribute
    where T : IResiliencyPolicy
{
    private string? _customName;
    
    public ResilientAttribute()
    {
        _customName = null;
    }
    
    public ResilientAttribute(string customName)
    {
        _customName = customName;
    }
}
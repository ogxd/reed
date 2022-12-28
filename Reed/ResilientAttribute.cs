namespace Reed;

[AttributeUsage(AttributeTargets.Method)]
public class ResilientAttribute<T> : Attribute
    where T : IResiliencyPolicy
{
    
}
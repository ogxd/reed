namespace Reed.Generators;

public class ResilientMethodSourceBuilder
{
    private static int _idIncr;
    
    private readonly ResilientClassSourceBuilder _classBuilder;
    private List<PolicyFeatureSourceBuilder> _featureSourceBuilders = new();
    private IList<(string argumentType, string argumentName)> _arguments;
    private string _name;
    private string _policyName;
    private string? _customName;
    private string? _returnType;
    private bool _isAsync;
    private int _id;

    public ResilientClassSourceBuilder ClassSourceBuilder => _classBuilder;
    public int Id => _id;
    
    internal ResilientMethodSourceBuilder(ResilientClassSourceBuilder classBuilder)
    {
        _classBuilder = classBuilder;
        _id = Interlocked.Increment(ref _id);
    }

    public ResilientMethodSourceBuilder WithResiliencyPolicy(string? @namespace, string name)
    {
        _policyName = name;
        ClassSourceBuilder.AddResiliencyPolicy(@namespace, name);
        return this;
    }

    public ResilientMethodSourceBuilder AddPolicyFeature<T>(Func<ResilientMethodSourceBuilder, T> ctor)
        where T : PolicyFeatureSourceBuilder
    {
        T methodSourceBuilder = ctor(this);
        _featureSourceBuilders.Add(methodSourceBuilder);
        return this;
    }
    
    public ResilientMethodSourceBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public ResilientMethodSourceBuilder WithCustomName(string customName)
    {
        _customName = customName;
        return this;
    }
    
    public ResilientMethodSourceBuilder WithAsync()
    {
        _isAsync = true;
        return this;
    }
    
    public ResilientMethodSourceBuilder WithReturnType(string typeName)
    {
        _returnType = typeName;
        return this;
    }

    public ResilientMethodSourceBuilder WithArguments(IList<(string argumentType, string argumentName)> arguments)
    {
        _arguments = arguments;
        return this;
    }
    
    public void Build(CsharpStringBuilder strbldr)
    {
        _featureSourceBuilders = _featureSourceBuilders.OrderBy(x => x.Priority).ToList();
        
        foreach (var policyWriter in _featureSourceBuilders)
        {
            policyWriter.BuildFields(strbldr); // TODO: Handle name collision when multiple resilient methods within a class need the name kind of fields
        }

        string customName = string.IsNullOrEmpty(_customName) ? $"{_name}_Resilient" : _customName;
        
        bool hasReturn = !string.IsNullOrEmpty(_returnType);
        
        strbldr.NewLine();
        strbldr.AppendLine($"public {(_isAsync ? "async " : null)}{(hasReturn ? _returnType : _isAsync ? "Task" : "void")} {customName}({string.Join(", ", _arguments.Select(x => $"{x.argumentType} {x.argumentName}"))})");
        strbldr.OpenBracket();
        
        if (hasReturn)
        {
            strbldr.AppendLine($"{_returnType} result = default;");
        }

        strbldr.AppendLine($"var resiliencyPolicy = _reed{_policyName};");

        foreach (var policyWriter in _featureSourceBuilders)
        {
            policyWriter.BuildBefore(strbldr);
        }

        // 2 flavors : Rewrite method OR just call the original method
        //strbldr.AppendLine(userMethod.Body.ToString());
        strbldr.AppendLine($"{(hasReturn ? "result = " : null)}{(_isAsync ? "await " : null)}{_name}({string.Join(", ", _arguments.Select(x => $"{x.argumentName}"))});");

        foreach (var policyWriter in _featureSourceBuilders)
        {
            policyWriter.BuildAfter(strbldr);
        }
        
        if (hasReturn)
        {
            strbldr.AppendLine($"return result;");
        }

        strbldr.CloseBracket();
    }
}
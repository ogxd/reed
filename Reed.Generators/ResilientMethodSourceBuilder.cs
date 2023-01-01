namespace Reed.Generators;

public class ResilientMethodSourceBuilder
{
    private readonly ResilientClassSourceBuilder _classBuilder;
    private IList<IPolicySourceWriter> _policyWriters;
    private IList<(string argumentType, string argumentName)> _arguments;
    private string _name;
    private string? _customName;
    private string? _returnType;
    private bool _isAsync;

    public ResilientClassSourceBuilder ClassSourceBuilder => _classBuilder;
    
    public ResilientMethodSourceBuilder(ResilientClassSourceBuilder classBuilder)
    {
        _classBuilder = classBuilder;
    }

    public ResilientMethodSourceBuilder WithResiliencyPolicy(List<IPolicySourceWriter> policyWriters)
    {
        _policyWriters = policyWriters;
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
        foreach (var policyWriter in _policyWriters)
        {
            policyWriter.WriteFields(strbldr); // TODO: Handle name collision when multiple resilient methods within a class need the name kind of fields
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

        foreach (var policyWriter in _policyWriters)
        {
            policyWriter.WriteBefore(strbldr);
        }

        // 2 flavors : Rewrite method OR just call the original method
        //strbldr.AppendLine(userMethod.Body.ToString());
        strbldr.AppendLine($"{(hasReturn ? "result = " : null)}{(_isAsync ? "await " : null)}{_name}({string.Join(", ", _arguments.Select(x => $"{x.argumentName}"))});");

        foreach (var policyWriter in _policyWriters)
        {
            policyWriter.WriteAfter(strbldr);
        }
        
        if (hasReturn)
        {
            strbldr.AppendLine($"return result;");
        }

        strbldr.CloseBracket();
    }
}
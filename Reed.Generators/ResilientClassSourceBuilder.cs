namespace Reed.Generators;

public class ResilientClassSourceBuilder
{
    private readonly HashSet<string> _usings = new();
    private readonly HashSet<string> _resiliencyPolicies = new();
    private readonly List<ResilientMethodSourceBuilder> _methodSourceBuilders = new();
    
    private string? _namespace;
    private string _name;

    public string FullName => string.IsNullOrEmpty(_namespace) ? _name : $"{_namespace}.{_name}";

    public ResilientClassSourceBuilder WithNamespace(string value)
    {
        _namespace = value;
        return this;
    }
    
    public ResilientClassSourceBuilder WithName(string value)
    {
        _name = value;
        return this;
    }
    
    public ResilientClassSourceBuilder AddUsing(string value)
    {
        _usings.Add(value);
        return this;
    }
    
    public ResilientClassSourceBuilder AddResiliencyPolicy(string value)
    {
        _resiliencyPolicies.Add(value);
        return this;
    }
    
    public ResilientClassSourceBuilder AddResilientMethod(Func<ResilientMethodSourceBuilder, bool> config)
    {
        ResilientMethodSourceBuilder methodSourceBuilder = new (this);
        config(methodSourceBuilder);
        _methodSourceBuilders.Add(methodSourceBuilder);
        return this;
    }

    public void Build(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine("using System;");
        strbldr.AppendLine("using Microsoft.Extensions.DependencyInjection;"); // ActivatorUtilitiesConstructor's namespace
        foreach (string value in _usings)
        {
            strbldr.AppendLine($"using {value};");
        }
        strbldr.NewLine();

        if (!string.IsNullOrEmpty(_namespace))
        {
            strbldr.AppendLine($"namespace {_namespace};");
        }

        strbldr.NewLine();
        strbldr.AppendLine($"public partial class {_name}");
        strbldr.OpenBracket();
        // Write resiliency policies fields
        foreach (string value in _resiliencyPolicies)
        {
            // strbldr.AppendLine($"private {value} _reed{value};");
            strbldr.AppendLine($"private {value} _resiliencyPolicy;"); // TODO: Handle several policies
        }
        strbldr.NewLine();
        
        strbldr.AppendLine($"[ActivatorUtilitiesConstructor]");
        // strbldr.AppendLine($"public {_name}({string.Join(", ", _resiliencyPolicies.Select(x => $"{x} reed{x}"))})");
        strbldr.AppendLine($"public {_name}({string.Join(", ", _resiliencyPolicies.Select(x => $"{x} resiliencyPolicy"))})");
        strbldr.OpenBracket();
        foreach (string value in _resiliencyPolicies)
        {
            // strbldr.AppendLine($"_reed{value} = reed{value};");
            strbldr.AppendLine($"_resiliencyPolicy = resiliencyPolicy;");
        }
        strbldr.CloseBracket();
        strbldr.NewLine();

        foreach (var methodBuilder in _methodSourceBuilders)
        {
            methodBuilder.Build(strbldr);
        }
        
        strbldr.CloseBracket();
    }
}
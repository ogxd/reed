using Microsoft.CodeAnalysis;

namespace Reed.Generators;

public static class Diagnostics
{
    public static void LogWarning(this GeneratorExecutionContext context, string log)
    {
        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
            "REED0002",
            "Log",
            log, 
            "Log", 
            DiagnosticSeverity.Warning, true), null));
    }
    
    public static void LogError(this GeneratorExecutionContext context, string log)
    {
        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
            "REED0002",
            "Log",
            log, 
            "Log", 
            DiagnosticSeverity.Error, true), null));
    }

    public static void Log0010(this GeneratorExecutionContext context, Location location, string name)
    {
        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
            "REED0010",
            "Method Naming Issue",
            "Method with name {0} already exists. Please rename wrapped method or use a different custom name in the ResilientAttribute", 
            "Log", 
            DiagnosticSeverity.Error, true), location, name));
    }
}
    

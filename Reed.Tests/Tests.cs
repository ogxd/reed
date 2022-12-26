using System.Text.Json.Nodes;
using NUnit.Framework;
using Reed;

//namespace Reed;

public class Tests
{
    [Test]
    public void NonResilient()
    {
        new ResilientClass().SayHello("John");
    }
    
    [Test]
    public void Resilient()
    {
        new ResilientClass().SayHello_Resilient("John");
    }
}

public partial class ResilientClass
{
    [Resilient<ExceptionHandlingPolicy>]
    public void SayHello(string name)
    {
        Console.WriteLine("Hello " + name);
        throw new Exception("Heh");
    }
}
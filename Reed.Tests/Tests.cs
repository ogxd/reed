using NUnit.Framework;
using Reed.Generators;

namespace Reed;

public class Tests
{
    [Test]
    public void NonResilient()
    {
        //new ResilientClass(new ExceptionHandlingPolicy()).SayHello("John");
    }
    
    [Test]
    public void Resilient()
    {
        
        //new ResilientClass(new ExceptionHandlingPolicy()).SayHello_Resilient("John");
    }
}

public partial class ResilientClass
{
    //[Resilient<IMyResiliencyPolicy>]
    public void SayHello(string name)
    {
        Console.WriteLine("Hello " + name);
        throw new Exception("Heh");
    }
}

public class Caca : ICaca
{
    
}

using NUnit.Framework;
using Reed;

//namespace Reed;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        HelloWorldGenerated.HelloWorld.SayHello();
        Assert.Pass();
    }
}

[Resilient]
public partial class ResilientClass
{
    public void UserMethod()
    {
        this.Coucou();
        //BlablaUserClass.Coucou();
        //Coucou.Caca();
        //this.GeneratedMethod();
        // call into a generated method inside the class
        //this.GeneratedMethod();
    }
}
namespace Lambda.Host.Example.HelloWorld;

public interface IService
{
    Task<string> GetMessage();
}

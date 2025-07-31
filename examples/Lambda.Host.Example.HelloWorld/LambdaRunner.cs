namespace Lambda.Host.Example.HelloWorld;

public class LambdaRunner
{
    public static void Run(IServiceProvider services, Delegate handler)
    {
        var func = (Func<Task, string>)handler;

        handler.DynamicInvoke(null);
    }
}

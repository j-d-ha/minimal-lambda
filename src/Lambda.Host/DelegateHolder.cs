namespace Lambda.Host;

public class DelegateHolder
{
    public Delegate? Handler { get; set; } = null;
    public bool IsHandlerSet => Handler != null;
}

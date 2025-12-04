using System.Threading.Tasks;
using JetBrains.Annotations;
using Xunit;

namespace Lambda.Host.Example.HelloWorld;

[TestSubject(typeof(Program))]
public class LambdaHostTest
{
    [Fact]
    public async Task LambdaHost_CanStartWithoutError() { }
}

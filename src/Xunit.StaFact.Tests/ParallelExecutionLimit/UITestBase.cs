using System.Runtime.CompilerServices;
using System.Threading;

public class UITestBase
{
    protected static void TestMethod([CallerMemberName] string name = null)
    {
        try
        {
            Thread.Sleep(200);
            Counter.Increment();
            Thread.Sleep(200);
        }
        finally
        {
            Counter.Decrement();
        }
    }
}

using Pulumi;
using System.Threading.Tasks;

public class DeferredOutput<T>
{
    private readonly TaskCompletionSource<T> _tcs;
    private readonly Output<T> _value;
    public DeferredOutput()
    {
        _tcs = new TaskCompletionSource<T>();
        _value = Output.Create(_tcs.Task);
    }

    public Output<T> Value => _value;
    public void Resolve(T value) => _tcs.SetResult(value);
}
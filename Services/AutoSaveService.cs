using ProjectTest.Models;

namespace ProjectTest.Services;

public sealed class AutoSaveService : IDisposable
{
    private readonly TimeSpan _debounce;
    private CancellationTokenSource? _pendingSave;

    public AutoSaveService(TimeSpan? debounce = null)
    {
        _debounce = debounce ?? TimeSpan.FromMilliseconds(900);
    }

    public AutoSaveState State { get; private set; } = AutoSaveState.Idle;

    public event EventHandler<AutoSaveState>? StateChanged;

    public void Schedule(Func<CancellationToken, Task> saveAction)
    {
        _pendingSave?.Cancel();
        _pendingSave?.Dispose();
        _pendingSave = new CancellationTokenSource();
        var token = _pendingSave.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_debounce, token);
                SetState(AutoSaveState.Saving);
                await saveAction(token);
                if (!token.IsCancellationRequested)
                {
                    SetState(AutoSaveState.Saved);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
                SetState(AutoSaveState.Error);
            }
        }, token);
    }

    public void Dispose()
    {
        _pendingSave?.Cancel();
        _pendingSave?.Dispose();
    }

    private void SetState(AutoSaveState state)
    {
        State = state;
        StateChanged?.Invoke(this, state);
    }
}

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinYouTubeMusic.Native;

public static class SingleInstance
{
    private const string MutexName = "Global\\KHeresy.WinYouTubeMusic.SingleInstanceMutex";
    private const string PipeName = "KHeresy.WinYouTubeMusic.IpcPipe";

    private static Mutex? _mutex;
    private static CancellationTokenSource? _cts;

    public static bool TryAcquire(out bool isPrimary)
    {
        try
        {
            _mutex = new Mutex(true, MutexName, out isPrimary);
            return isPrimary;
        }
        catch
        {
            isPrimary = false;
            return false;
        }
    }

    public static void StartIpcServer(Action<string> onUrlReceived)
    {
        _cts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.In,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    await server.WaitForConnectionAsync(_cts.Token);

                    using var reader = new StreamReader(server, Encoding.UTF8);
                    string? url = await reader.ReadLineAsync(_cts.Token);
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        onUrlReceived(url);
                    }
                }
                catch when (_cts.Token.IsCancellationRequested)
                {
                    break;
                }
                catch
                {
                    try
                    {
                        await Task.Delay(200, _cts.Token);
                    }
                    catch
                    {
                        break;
                    }
                }
            }
        }, _cts.Token);
    }

    public static bool SendToPrimaryInstance(string url)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(2000);
            using var writer = new StreamWriter(client, Encoding.UTF8) { AutoFlush = true };
            writer.WriteLine(url);
            writer.Flush();
            client.WaitForPipeDrain();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> SendToPrimaryInstanceAsync(string url)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            await client.ConnectAsync(2000);
            using var writer = new StreamWriter(client, Encoding.UTF8) { AutoFlush = true };
            await writer.WriteLineAsync(url);
            await writer.FlushAsync();
            client.WaitForPipeDrain();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void Release()
    {
        try
        {
            _cts?.Cancel();
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }
        catch { }
    }
}

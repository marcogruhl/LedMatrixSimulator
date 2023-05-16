using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Threading.Tasks;

namespace RpiRgbLedMatrixSimulator;

// https://www.codeproject.com/Articles/1199046/A-Csharp-Named-Pipe-Library-That-Supports-Multiple
public class NamedPipeServer
{
    private readonly string pipeName;
    private readonly int maxNumberOfServerInstances;

    private List<NamedPipeServerInstance> servers = new List<NamedPipeServerInstance>();

    public event EventHandler<string> newRequestEvent = delegate { };
    public event EventHandler newServerInstanceEvent = delegate { };
    public event EventHandler<int> newServerInstanceCreated = delegate { };

    public event EventHandler<int> clientExited = delegate { };
    public event EventHandler<int> clientConnected = delegate { };

    public int ClientClount { get; private set; }

    public NamedPipeServer(string pipeName) : this(pipeName, 20, 4) { }

    public NamedPipeServer(string pipeName, int maxNumberOfServerInstances, int initialNumberOfServerInstances)
    {
        this.pipeName = pipeName;
        this.maxNumberOfServerInstances = maxNumberOfServerInstances;

        for (int i = 0; i < initialNumberOfServerInstances; i++)
        {
            NewServerInstance();
        }
    }

    public void Dispose()
    {
        CleanServers(true);
    }

    private void NewServerInstance()
    {
        // Start a new server instance only when the number of server instances
        // is smaller than maxNumberOfServerInstances
        if (servers.Count < maxNumberOfServerInstances)
        {
            var server = new NamedPipeServerInstance(pipeName, maxNumberOfServerInstances);

            server.newServerInstanceEvent += (s, e) =>
            {
                NewServerInstance();
                newServerInstanceEvent?.Invoke(this, EventArgs.Empty);
                clientConnected?.Invoke(this, ++ClientClount);

            };
            server.newRequestEvent += (s, e) => newRequestEvent.Invoke(s, e);
                
            servers.Add(server);

            newServerInstanceCreated?.Invoke(this, servers.Count);

            server.clientExited += (s, e) =>
            {
                servers.Remove(e);
                clientExited?.Invoke(this, --ClientClount);
            };
        }

        // Run clean servers anyway
        CleanServers(false);
    }

    /// <summary>
    /// A routine to clean NamedPipeServerInstances. When disposeAll is true,
    /// it will dispose all server instances. Otherwise, it will only dispose
    /// the instances that are completed, canceled, or faulted.
    /// PS: disposeAll is true only for this.Dispose()
    /// </summary>
    /// <param name="disposeAll"></param>
    private void CleanServers(bool disposeAll)
    {
        if (disposeAll)
        {
            foreach (var server in servers)
            {
                server.Dispose();
            }
        }
        else
        {
            for (int i = servers.Count - 1; i >= 0; i--)
            {
                if (servers[i] == null)
                {
                    servers.RemoveAt(i);
                }
                else if (servers[i].TaskCommunication != null &&
                         (servers[i].TaskCommunication.Status == TaskStatus.RanToCompletion ||
                          servers[i].TaskCommunication.Status == TaskStatus.Canceled ||
                          servers[i].TaskCommunication.Status == TaskStatus.Faulted))
                {
                    servers[i].Dispose();
                    servers.RemoveAt(i);
                }
            }
        }
    }
}

class NamedPipeServerInstance : IDisposable
{
    private NamedPipeServerStream server;
    private bool disposeFlag = false;

    public Task TaskCommunication { get; private set; }

    public event EventHandler newServerInstanceEvent = delegate { };
    public event EventHandler<string> newRequestEvent = delegate { };
    public event EventHandler<NamedPipeServerInstance> clientExited = delegate { };
    public event EventHandler<NamedPipeServerInstance> clientConnected = delegate { };

    public NamedPipeServerInstance(string pipeName, int maxNumberOfServerInstances)
    {
        server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, maxNumberOfServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
        var asyncResult = server.BeginWaitForConnection(OnConnected, null);
    }

    public void Dispose()
    {
        disposeFlag = true;
        server.Dispose();
    }

    private void OnConnected(IAsyncResult result)
    {
        /// This method might be invoked either on new client connection
        /// or on dispose. Thus, it is necessary to check disposeFlag.
        if (!disposeFlag)
        {
            server.EndWaitForConnection(result);

            newServerInstanceEvent.Invoke(this, EventArgs.Empty);
            clientConnected.Invoke(this, this);

            TaskCommunication = Task.Factory.StartNew(Communication);
        }
    }

    private void Communication()
    {
        using (var reader = new StreamReader(server))
        {
            while (!reader.EndOfStream)
            {
                var request = reader.ReadLine();

                if (request != null)
                {
                    newRequestEvent.Invoke(this, request);
                }
            }

            clientExited?.Invoke(this, this);
        }
    }
}
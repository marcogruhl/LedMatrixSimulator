using System.Diagnostics;
using System.IO.Pipes;

namespace AspSample;

public class NamedPipeClient : IDisposable
{
    private NamedPipeClientStream client;
    private StreamReader reader;
    private StreamWriter writer;

    public bool WaitForConnection { get; private set; }
    public bool ServerStarted { get; private set; }
    public event EventHandler ClientConnected = delegate { };


    private string _pipeName;
    public NamedPipeClient(string pipeName, string server = "127.0.0.1")
    {
        _pipeName = pipeName;
        StartClient(server);
    }

    private async Task StartClient(string server = "127.0.0.1")
    {
        client = new NamedPipeClientStream(server,_pipeName);

        await client.ConnectAsync();
        ServerStarted = true;
        ClientConnected?.Invoke(this, EventArgs.Empty);

        reader = new StreamReader(client);
        writer = new StreamWriter(client);
    }

    public void Dispose()
    {
        ServerStarted = false;

        try
        {
            writer.Dispose();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        try
        {
            reader.Dispose();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        try
        {
            client.Dispose();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public async void SendData(string request)
    {
        if (request != null)
        {
            try
            {
                writer.WriteLine(request);
                writer.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("{0}\r\nDetails:\r\n{1}", "Error on server communication.", ex.Message));
                    
                Dispose();
                await StartClient();
            }
        }
        else
        {
            Console.WriteLine("Error. Null request.");
        }
    }
}
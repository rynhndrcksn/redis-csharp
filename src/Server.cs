using System.Buffers;
using System.Net;
using System.Net.Sockets;

var pongResponse = "+PONG\r\n"u8.ToArray();

// Set up TcpListener and start it.
var server = new TcpListener(IPAddress.Any, 6379);
server.Start();

// We want to handle more than just 1 client, so we need an infinite loop.
while (true)
{
    // Open up a socket and wait for a connection
    var acceptSocket = server.AcceptSocket();

    // Use a "discard variable" to make the compiler happy.
    _ = HandleSocketAsync(acceptSocket);
}

// An asynchronous socket handler.
async Task HandleSocketAsync(Socket socket)
{
    // Create a new ArrayPool<> we can use to get buffers from later.
    var pool = ArrayPool<byte>.Shared;

    // As long as the client is feeding us data, keep running.
    while (socket.Connected)
    {
        // Rather than allocating a new buffer every time, grab one from the ArrayPool<>.
        // This reduces the amount of allocations and increases performance of the app.
        var buffer = pool.Rent(socket.ReceiveBufferSize);
        try
        {
            // Currently we don't care about what the response is, this will eventually change.
            // However, for now just read the request, assume it's "PING", and respond with "PONG".
            await socket.ReceiveAsync(buffer);
            await socket.SendAsync(pongResponse, SocketFlags.None);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            socket.Close();
            throw;
        }
        finally
        {
            // Return the shared buffer to the pool, and clear the data from it.
            pool.Return(buffer, clearArray: true);
        }
    }
}
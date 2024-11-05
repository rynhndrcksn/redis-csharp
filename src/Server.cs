using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

const string pong = "+PONG\r\n";

// Uncomment this block to pass the first stage
var server = new TcpListener(IPAddress.Any, 6379);
server.Start();
var socket = server.AcceptSocket(); // wait for client

while (socket.Connected)
{
    var buffer = new byte[1024];
    await socket.ReceiveAsync(buffer);
    await socket.SendAsync(Encoding.UTF8.GetBytes(pong), SocketFlags.None);
}
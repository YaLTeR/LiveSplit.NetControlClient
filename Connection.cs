using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiveSplit.NetControlClient
{
    class Connection : IDisposable
    {
        private TcpClient Client;
        private NetworkStream Stream;

        public enum Result
        {
            Success,
            Error,
            ConnectionTimeout,
            WrongPassword
        };

        public Result Connect(string password)
        {
            try
            {
                var client = new TcpClient();

                // If we didn't connect, return early.
                if (!client.ConnectAsync("play.sourceruns.org", 12345).Wait(1000))
                {
                    client.Close();
                    return Result.ConnectionTimeout;
                }

                Client = client;

                var stream = client.GetStream();
                stream.ReadTimeout = 3000;
                stream.WriteTimeout = 3000;

                // Send the password.
                var message = Encoding.UTF8.GetBytes(password + '\n');
                stream.Write(message, 0, message.Length);

                // Read the response.
                var response = new byte[11];
                if (stream.Read(response, 0, response.Length) == response.Length
                    && Encoding.UTF8.GetString(response) == "authorized\n")
                {
                    // Restore the timeouts to prevent automatic disconnect.
                    stream.ReadTimeout = Timeout.Infinite;
                    stream.WriteTimeout = Timeout.Infinite;
                    Stream = stream;
                    return Result.Success;
                }
                else
                {
                    stream.Close();
                    client.Close();
                    return Result.WrongPassword;
                }
            }
            catch (SocketException)
            {
            }
            catch (IOException)
            {
            }

            Client?.Close();
            Client = null;

            return Result.Error;
        }

        private void Send(string message)
        {
            if (Client == null)
                return;

            var bytes = Encoding.UTF8.GetBytes(message + '\n');

            try
            {
                Stream.BeginWrite(bytes,
                                  0,
                                  bytes.Length,
                                  (IAsyncResult ar) =>
                                  {
                                      (ar.AsyncState as Stream).EndWrite(ar);
                                  },
                                  Stream);
            }
            catch (IOException)
            {
                Stream.Close();
                Client.Close();
                Stream = null;
                Client = null;
            }
        }

        public void SendStart(TimeSpan offset)
        {
            Send("runoffset " + (long)offset.TotalMilliseconds);
            Send("starttimer");
        }

        public void SendPause()
        {
            Send("pause");
        }

        public void SendResume()
        {
            Send("resume");
        }

        public void SendUndoAllPauses()
        {
            Send("undoallpauses");
        }

        public void SendSplit()
        {
            Send("split");
        }

        public void SendUnsplit()
        {
            Send("unsplit");
        }

        public void SendReset()
        {
            Send("reset");
        }

        public void SendCurrentTime(TimeSpan time)
        {
            Send("currenttime " + (long)time.TotalMilliseconds);
        }

        public void Dispose()
        {
            Stream?.Close();
            Client?.Close();
        }
    }
}

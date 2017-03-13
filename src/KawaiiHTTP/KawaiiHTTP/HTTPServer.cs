using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace KawaiiHTTP
{
    public class HTTPServer
    {
        private Thread httpListenerThread;
        private bool listening = false;
        private TcpListener tcpListener;
        public int BindPort { get; private set; } = 80;
        public IHTTPHandler DefaultHTTPHandler { get; private set; } = new Handlers.StockHandler();
        public IHTTPHandler CoreHandler { get; private set; } = new Handlers.CoreHandler();
        public IHTTPHandler AutoProxyHandler { get; set; } = new Handlers.ProxyHandler();
        public List<IHTTPHandler> HTTPHandlers { get; private set; } = new List<IHTTPHandler>();
        public Settings.SettingsObject Settings { get; private set; } = new KawaiiHTTP.Settings.SettingsObject();
        public string BindAddress { get; private set; } = "0.0.0.0";
        public HTTPServer()
        {

        }
        public HTTPServer(int tcp_port)
        {
            this.BindPort = tcp_port;
        }
        public HTTPServer(string bind_address, int tcp_port)
        {
            this.BindAddress = bind_address;
            this.BindPort = tcp_port;
        }

        private void ListenerMethod()
        {
            this.listening = true;
            this.tcpListener.Start();

            while (this.listening)
            {
                TcpClient remoteClient = this.tcpListener.AcceptTcpClient();
                Log.d("Accepting TCP Client: {0}", remoteClient.Client.RemoteEndPoint.ToString());
                HTTPProcessor processor = new HTTPProcessor(remoteClient, this);

                Thread.Sleep(1);
            }
        }

        public void StartListening()
        {
            IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse(this.BindAddress), this.BindPort);
            // IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse("25.57.0.109"), this.port_tcp);
            this.tcpListener = new TcpListener(ipEnd);

            ThreadStart listenStarter = new ThreadStart(this.ListenerMethod);
            this.httpListenerThread = new Thread(listenStarter);
            this.httpListenerThread.Start();
        }
        public void StopListening()
        {
            this.listening = false;
            try
            {
                this.httpListenerThread.Abort();
            }
            catch (Exception ex)
            {
                Log.d("Could not terminate HTTP thread: {0}", ex.Message);
            }
        }
    }
}

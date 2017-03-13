using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace KawaiiHTTP
{
    public class HTTPProcessor
    {
        private Thread processorThread;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private BufferedStream inputStream;
        private BufferedStream outputStream;
        private bool KeepAlive = false;
        private HTTPServer handlingServer;
        private bool quit = false;
        private bool closeConnection = false;

        public int BufferSize { get; set; } = 4096;
        public bool Finished { get; private set; }
        public HTTPProcessor(TcpClient remoteClient, HTTPServer sender)
        {
            this.handlingServer = sender;
            this.tcpClient = remoteClient;

            ThreadStart starter = new ThreadStart(this.ProcessorMethod);
            this.processorThread = new Thread(starter);
            this.processorThread.Start();
        }
        private void PullRequest()
        {
            // Just cache this value in case it randomly decides to change
            int bufSize = this.BufferSize;
            // Shove the message in here, so we can peek around freely
            MemoryStream fullMessage = new MemoryStream();

            byte[] readBuffer = new byte[bufSize]; // Reserve a bit of memory we can receive data in
            while (true)
            {
                int readCount = 0;
                try
                {
                    readCount = this.inputStream.Read(readBuffer, 0, bufSize); // Read data from the TCP stream
                }
                catch (Exception ex)
                {
                    Log.e("Could not read from TCP stream: {0}", ex.Message);
                    this.QuitProcessor();
                    return;
                }

                if (readCount == 0) { break; } // When we get zero bytes, we can assume it is the end of the stream
                fullMessage.Write(readBuffer, 0, readCount); // Take the data we got, and put it in our own stream object
                if (readCount < bufSize) { break; } // If the received data length is smaller then the buffer, assume end of stream
            }
            fullMessage.Seek(0, SeekOrigin.Begin); // Set the cursor of our own stream to the start

            if (fullMessage.Length <= 0) // We can't do anything with empty requests, kill the stream
            {
                if (this.handlingServer.Settings.ChargenOnEmptyStream)
                {
                    Log.m("Got an empty stream, shoving through a load of characters before quitting");
                    Random aRandom = new Random((int)DateTime.Now.Ticks);
                    try
                    {
                        for (int i = 0; i < this.handlingServer.Settings.ChargenLength; i++)
                        {
                            this.outputStream.WriteByte((byte)aRandom.Next(0, 254));
                        }

                        this.outputStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        Log.e("Failed to shove a bunch of characters", ex.Message);
                    }
                }
                else
                {
                    Log.e("Got an empty message from a remote client, aborting processing stream");
                }
                this.QuitProcessor();
                return;
            }

            StringBuilder requestString = new StringBuilder(); // StringBuilers are faster than just appending strings or chars ^^
            int lastByte = -1;
            while (fullMessage.CanRead)
            {
                int readByte = fullMessage.ReadByte(); // Get one byte from the received message
                if (readByte == 13)
                { // We're going to ignore carriage returns
                    continue;
                }

                if (lastByte == readByte && readByte == 10)
                { // We've got two newlines, end of request!
                    break;
                }
                requestString.Append((char)(byte)readByte); // Just throw the received byte onto the end of our string

                lastByte = readByte;
            }

            int contentLength = 0;
            HTTPHeader requestHeader = HTTPHeader.ParseRequest(requestString.ToString()); // Turn our request string into a header object, so we can do stuff
            Log.d("Connection mode is {0}", requestHeader.Connection[0]);
            this.KeepAlive = requestHeader.Connection.Contains("keep-alive"); // We have to keep pulling requests until specified otherwise
            this.closeConnection = requestHeader.Connection.Contains("close"); // Have a license to kill
            if (requestHeader.HTTPFields.ContainsKey("Content-Length"))
            {
                contentLength = requestHeader.ContentLength;
            }
            Log.d("Got request:\n{0}", requestHeader.ToString());

            // Read the http message content from the stream
            // If we get no content, we're wasting some resources
            // cri
            ContentStream requestContent = new ContentStream();
            if (requestContent.Length > 0)
            {
                while (fullMessage.CanRead)
                {
                    requestContent.WriteByte((byte)fullMessage.ReadByte());
                    if (requestContent.Length >= contentLength)
                    {
                        break;
                    }
                }
            }

            // Finish the stream boogaloo for this request
            fullMessage.Dispose(); // We've got from the stream what we needed, get rid of it
            fullMessage = null;
            // this.inputStream.Seek(0, SeekOrigin.End); // Skip the buffered stream to the end

            // Set up the data containers for the response
            HTTPHeader responseHeader = HTTPHeader.GenerateResponse();
            responseHeader.SetField("Server", this.handlingServer.Settings.Server);
            ContentStream responseContent = new ContentStream();

            // Shove all of our junk in one neat little strucht
            Handlers.HandlePackage handlePackage = new Handlers.HandlePackage(requestHeader, responseHeader, requestContent, responseContent, this.handlingServer);
            if (this.handlingServer.Settings.AutoProxy && !MachineData.HostNames.ContainsName(requestHeader.Host))
            {
                Log.m("Autoproxying for {0}", requestHeader.Host);
                if (!this.handlingServer.AutoProxyHandler.HandleRequest(handlePackage))
                {
                    responseContent.Write("wah");
                }
            }
            else
            {
                if (!this.handlingServer.CoreHandler.HandleRequest(handlePackage)) // Allow the core handler a try first
                {
                    bool notExecuted = true;
                    foreach (IHTTPHandler handler in this.handlingServer.HTTPHandlers)
                    {
                        notExecuted = !handler.HandleRequest(handlePackage);
                        // Might have to put a break in here some day
                    }

                    if (notExecuted)
                    { // Nothing handled our request, shove it through the default handler
                        this.handlingServer.DefaultHTTPHandler.HandleRequest(handlePackage);
                    }
                }
            }

            responseHeader.SetField("Content-Length", responseContent.Length.ToString()); // We have to set the content length on the response header
            responseHeader.SetField("Connection", "closed");

            string statusString = string.Format("HTTP/{2} {0} {1}\r\n", responseHeader.StatusCode, responseHeader.StatusMessage, responseHeader.Version);
            byte[] statusStringBytes = Encoding.UTF8.GetBytes(statusString);
            this.outputStream.Write(statusStringBytes, 0, statusStringBytes.Length);

            Log.d("Response header: {0}\n", responseHeader.ToString());
            responseHeader.DumpFields(this.outputStream);
            this.outputStream.WriteByte(13); // CRLF the really stupid way
            this.outputStream.WriteByte(10);

            if (requestHeader.Method == HTTP_Method.HEAD)
            {
                // WELL OK
            }
            else {
                // Copy the content generated by our other stuff into the TCP stream
                byte[] contentBytes = new byte[responseContent.Length];
                if (responseContent.Length > 0)
                {
                    responseContent.Seek(0, SeekOrigin.Begin);
                    responseContent.Read(contentBytes, 0, (int)responseContent.Length);
                }
                try
                {
                    this.outputStream.Write(contentBytes, 0, contentBytes.Length);
                }
                catch (Exception ex)
                {
                    Log.e(ex.Message);
                }
            }

            this.KeepAlive = false; // heh...
            // this.outputStream.Flush();
        }
        private void ProcessorMethod()
        {
            // Set up some stream objects to interface with
            this.networkStream = this.tcpClient.GetStream();
            this.inputStream = new BufferedStream(this.networkStream);
            this.outputStream = new BufferedStream(this.networkStream);

            this.PullRequest();
            if (this.KeepAlive)
            {
                Log.d("Keep-alive detected, keep pulling until close");
                while (!this.closeConnection && this.KeepAlive)
                {
                    this.PullRequest();
                }
            }
            if (!this.quit && !this.closeConnection)
            {
                this.SendClose();
            }
            else
            {
                this.CloseStreams();
            }

            this.Finished = true;
        }

        private void QuitProcessor(int errorCode = 500, string errorMessage = "Malformed request/Internal server error")
        {
            this.quit = true;
            this.KeepAlive = false;
            this.CloseStreams();
        }
        private void SendClose()
        {
            if (!this.outputStream.CanWrite || !this.networkStream.CanWrite)
            {
                Log.e("Cannot write to the output, for some reason.");
                this.CloseStreams();
                return;
            }
            /* HTTPHeader closeHeader = new HTTPHeader();
            closeHeader.Method = HTTP_Method.HEAD;
            closeHeader.SetField("Server", this.handlingServer.Settings.Server);
            closeHeader.SetField("Connection", "close");

            string closingString = string.Format("HTTP/1.1 {0} {1}\n", closeHeader.StatusCode, closeHeader.StatusMessage);
            byte[] closingBytes = Encoding.UTF8.GetBytes(closingString);
            this.outputStream.Write(closingBytes, 0, closingBytes.Length);
            closeHeader.DumpFields(this.outputStream); */
            try
            {
                this.outputStream.Flush();
            }
            catch (Exception ex)
            {
                Log.e("Could not flush the TCP stream: {0}", ex.Message);
            }

            this.CloseStreams();
        }
        private void CloseStreams()
        {
            try
            {
                this.inputStream.Close();
            }
            catch (Exception ex)
            {
                Log.e(ex.Message);
            }


            try
            {
                this.networkStream.Close();
            }
            catch (Exception ex)
            {
                Log.e(ex.Message);
            }
            Log.d("Closed HTTP Processor streams");
        }
    }
}

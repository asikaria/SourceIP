using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Net;
using System.Threading;


namespace SourceIPService
{
    class Service : ServiceBase
    {

        private HttpListener listener;
        private Thread serviceMainThread;
        int stopsignal = 0;

        public Service()
        {
            this.ServiceName = "SourceIPService";
            this.AutoLog = true;
            this.CanPauseAndContinue = false;
            this.CanShutdown = false;
        }

        //Calls onstart, but is public (needed for the console app to run). OnStart is protected, so cant be called from Main.
        public void DoStart()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            //OnStart shoud return right away, otherwise SCM kills the service. Main processing happens in a different thread running ServiceMain
            serviceMainThread = new Thread(this.ServiceMain);
            serviceMainThread.Start();

        }

        protected override void OnStop()
        {
            Interlocked.Increment(ref stopsignal);
            listener.Stop();
            listener.Close();
        }

        //The main thread in the service
        public void ServiceMain()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:8080/myip/");
            listener.Start();
            while (true)
            {
                HttpListenerContext ctx = listener.GetContext();
                Thread t = new Thread(this.ProcessRequest);   // TODO: this is wasteful to fork a new thread on every request. Better to have a pool of threads that pulls from a queue
                t.Start(ctx);
            }
        }

        //The method that runs in request processing worker threads
        public void ProcessRequest(object contextObject)
        {
            HttpListenerContext context = (HttpListenerContext)contextObject;
            IPEndPoint clientEndpoint = context.Request.RemoteEndPoint;
            string clientIPString = clientEndpoint.Address.ToString();
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(clientIPString);

            HttpListenerResponse response = context.Response;
            response.ContentLength64 = buffer.Length;
            response.ContentType = "text/plain";
            response.KeepAlive = false;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            //Console.WriteLine("Finished processing request from {0}", clientIPString);
            // TODO: There is no logging or stats collection in this. Need to implement that, without sacrifing the current no-disk speed
        }
    }
    
}

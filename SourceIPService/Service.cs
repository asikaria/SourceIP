using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Net;
using System.Threading;
using System.Configuration;



namespace SourceIPService
{
    public class Service : ServiceBase
    {

        private HttpListener listener;
        int stopsignal = 0;

        public Service()
        {
            this.ServiceName = "SourceIPService";
            this.AutoLog = true;
            this.CanPauseAndContinue = false;
            this.CanShutdown = false;
        }

        //Calls onstart, but is public (needed for the console app to run). OnStart is protected, so cant be called from the console app project.
        public void DoStart()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            //OnStart shoud return right away, otherwise SCM kills the service. Main processing happens in a different thread running ServiceMain
            stopsignal = 0;
            Interlocked.MemoryBarrier();
            Thread serviceMainThread;
            serviceMainThread = new Thread(this.ServiceMain);
            serviceMainThread.Start();
        }

        //Calls onstop, but is public (needed for the console app to run). OnStart is protected, so cant be called from the console app project.
        public void DoStop()
        {
            OnStop();
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
            listener.Prefixes.Add(getHttpListenerPrefix());
            listener.Start();
            int numConcurrentRequests = Environment.ProcessorCount;
            for (int i = 0; i < numConcurrentRequests; i++)
            {
                try
                {
                    IAsyncResult iar = listener.BeginGetContext(ProcessRequest, listener);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.GetType().FullName);
                }
            }

        }

        public string getHttpListenerPrefix()
        {
            if (ConfigurationManager.AppSettings["prefix"] == null)
            {
                return @"http://*:8080/myip/";
            }
            else
            {
                return ConfigurationManager.AppSettings["prefix"];
            }
        }

        //The method that runs in request processing worker threads
        public void ProcessRequest(IAsyncResult iar)
        {
            HttpListener listener = iar.AsyncState as HttpListener;
            HttpListenerContext context = null;
            try
            {
                context = listener.EndGetContext(iar);
            }
            catch (System.Net.HttpListenerException)
            {
                if (stopsignal > 0)
                {
                    // If stopsignal is signalled, then this is expected behavior - just continue shutting down
                }
                else
                {
                    //Console.WriteLine(ex.GetType().FullName);   --> Log this
                }
            }
            if (context != null)
            {
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
            }
            if (stopsignal == 0)    // stopsignal > 0 will prevent new request-waits from getting queued
            {
                IAsyncResult iar2 = listener.BeginGetContext(ProcessRequest, listener);
            }
        }


    }
    
}

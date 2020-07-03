﻿using JMS.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using JMS.Impls;
namespace JMS
{
    class Gateway
    {
        TcpListener _tcpListener;
        ILogger<Gateway> _Logger;
        IRequestReception _requestReception;
        internal IServiceProvider ServiceProvider { get; set; }
        public List<MicroServiceReception> OnlineMicroServices { get; set; }


        public Gateway(ILogger<Gateway> logger)
        {
            _Logger = logger;
            OnlineMicroServices = new List<MicroServiceReception>();
        }

        void onSocketConnect(Socket socket)
        {
            try
            {
                using (var client = new Way.Lib.NetStream(socket))
                {
                    _requestReception.Interview(client);
                }
            }
            catch(Exception ex)
            {
                _Logger?.LogError(ex, ex.Message);
            }           
        }

        public void Run(int port)
        {
            _requestReception = ServiceProvider.GetService<IRequestReception>();
               _tcpListener = new TcpListener(IPAddress.Any, port);
            _tcpListener.Start();
            _Logger?.LogInformation("Gateway started, port:{0}", port);
            while (true)
            {
                try
                {
                    var socket = _tcpListener.AcceptSocket();
                    Task.Run(() => onSocketConnect(socket));
                }
                catch (Exception ex)
                {
                    _Logger?.LogError(ex, ex.Message);
                    break;
                }
               
            }
        }

        public RegisterServiceInfo[] GetAllServiceProviders()
        {
            List<RegisterServiceInfo> ret = new List<RegisterServiceInfo>();
            for(int i = 0; i < OnlineMicroServices.Count; i ++)
            {
                var client = OnlineMicroServices[i];
                if(client != null && ret.Contains(client.ServiceInfo) == false)
                {
                    ret.Add(client.ServiceInfo);
                }
            }
            return ret.ToArray();
        }
    }
}
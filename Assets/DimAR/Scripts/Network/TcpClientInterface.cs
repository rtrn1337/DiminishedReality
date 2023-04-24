using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine.Tilemaps;

namespace GRT
{
    public class TcpClientInterface
    {
        private string ipAddr;
        public string IpAddr
        {
            get { return ipAddr; }
            set { ipAddr = value; }
        }

        private int port;
        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        const int READ_BUFFER_SIZE = 4096;
        //BaseGRTClient baseGrtClient;
        byte[] readBuffer;
        TcpClient tcpClient;

        private string fullmsg = "";
        //RequestHelper requestHelper;
        GRT.EventProcessor eventProcessor; 
        [SerializeField]
        private bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
            set { isConnected = value; }
        }


        Thread readThread;
        bool appIsRunning = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddr"></param>
        /// <param name="port"></param>
        /// <param name="baseGrtClient"></param>
        /// <param name="eventProcessor">cause event processor is type of mono behaviour it has to be assigned via constructor</param>
        public TcpClientInterface(string ipAddr, int port, GRT.EventProcessor eventProcessor)
        {
            //this.baseGrtClient = baseGrtClient;
            this.ipAddr = ipAddr;
            this.port = port;
            this.eventProcessor = eventProcessor;
        }

        public bool Connect(string ipAddr, int port)
        {
            if(isConnected)
            {
                return true;
            }
            this.ipAddr = ipAddr;
            this.port = port;

            IPAddress ip;

            if (!IPAddress.TryParse(ipAddr, out ip))
            {
                Debug.LogError("Could not parse tcp ip address");
                return false;
            }

            IPEndPoint ip_end = new IPEndPoint(ip, port);
            Debug.Log("IP END " + ip_end);
            tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(ip_end);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            } 
            tcpClient.NoDelay = true;
            appIsRunning = true;
            isConnected = true;

            readBuffer = new byte[READ_BUFFER_SIZE];
            readThread = new Thread(new ThreadStart(ReadData));
            readThread.IsBackground = true;

            readThread.Start();
            return true;
        }

        public bool Send(ref RequestHelper.InPaintData data)
        {
            if(data.raw_data.Length < 3)
            {
                Debug.LogError("Error while parsing message for reqeust: " + data.request.ToString());
                return false;
            }
            if (!isConnected)
            {
                //Debug.LogError("Tcp client is not initialized");
                return false;
            }

            StreamWriter writer = new StreamWriter(tcpClient.GetStream());
            writer.Write(data.raw_data);
            writer.Flush();
            return true;
        }

        public void StopReading()
        {
            appIsRunning = false;
            if(tcpClient != null)
                tcpClient.Close();
        }

        void ReadData()
        {
            if(appIsRunning)
            {
                tcpClient.Client.BeginReceive(readBuffer, 0, READ_BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReadData), null);
            }
        }

        void ReadData(IAsyncResult ar)
        {
            if (!isConnected)
            {
                Debug.LogError("Tcp client is not initialized");
                return;
            }

            int bytesRead = 0;
            try
            {
                // Finish asynchronous read into readBuffer and return number of bytes read.
                bytesRead = tcpClient.GetStream().EndRead(ar);
                if (bytesRead < 1)
                {
                    // if no bytes were read server has close.  
                    Debug.LogError("Server has closed");
#if  UNITY_EDITOR
                    Application.Quit();             
#endif
                  
                    return;
                }
                string chunk = Encoding.ASCII.GetString(readBuffer, 0, bytesRead);
             
                // save msg without end tag ;
                // Debug.Log("in chunk: " +chunk);
                
                if (chunk.Contains("_"))
                { 
                    //Debug.Log("Got end Message");
                    fullmsg += chunk;
                    fullmsg =  fullmsg.Substring(0, fullmsg.Length - 1);
                    
                    Action<string> action = new Action<string>(HandleReceivedData);
                    eventProcessor.QueueEvent(action, fullmsg);
                    fullmsg = "";
                }
                else
                {
                    //Debug.Log(" " + chunk);
                    fullmsg += chunk;
                }

            }
            catch
            { 
                string msg = "";
                Action<string> action = new Action<string>(ServerClosed);
                eventProcessor.QueueEvent(action, msg);
            }
                         
            if (appIsRunning)
                ReadData();
 
        }

        void ServerClosed(string msg)
        {
            StopReading();
            readThread.Abort();
            isConnected = false;
            BaseGRTClient.Instance.FireServerClosed();
        }

        void HandleReceivedData(string msg)
        {
         
              RequestHelper.InPaintData data = new RequestHelper.InPaintData();
              if (RequestHelper.GetInpaintData(msg, ref data))
              { 
                  BaseGRTClient.Instance.HandleCallback(data);
              } 

        }
    }
}
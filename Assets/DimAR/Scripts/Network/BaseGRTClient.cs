        using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
        using System.Text;

        namespace GRT
{
    public class BaseGRTClient : Singleton<BaseGRTClient>
    {
        public delegate void GrtDataHandler(RequestHelper.InPaintData data);
        public event GrtDataHandler RequestSend, RequestReceived;
        public event EventHandler ServerClosed, ServerConnected, InitialRequestsOver;

        [SerializeField] [Tooltip("Ip Address of grt server")]
         string ipAddr;

        [SerializeField]
        [Tooltip("TCP Port of grt server for tracking data")]
        int tcpPort = 13;
 
        protected List<IGrtDataHandler> observer;
        IEnumerator requestUpdater;
        bool isUpdatingRequests = false;

        TcpClientInterface tcpClientIFace;
 
  

        private bool isSendingInapintingData = false;

        public bool IsSendingInapintingData
        {
            get { return isSendingInapintingData; }
            set { isSendingInapintingData = value; }
        }

        bool initialRequestsOver = false;


        Stack<RequestHelper.Request> stack;

        void Awake()
        {
            EventProcessor ep = GetComponent<EventProcessor>();
            if(ep == null)
            {
                ep = gameObject.AddComponent<EventProcessor>();
            }
            
            if (PlayerPrefs.HasKey("serverip")) 
            { 
                ipAddr = PlayerPrefs.GetString("serverip"); 
                tcpClientIFace = new TcpClientInterface(ipAddr, tcpPort, ep);
            }
            else {
                    Debug.LogWarning("No server IP address found!");
            }
            
            stack = new Stack<RequestHelper.Request>();
           
           /* stack.Push(RequestHelper.Request.SendBackground);
            stack.Push(RequestHelper.Request.SendMask);
            stack.Push(RequestHelper.Request.GetInpaintedImage); */
            // stack.Push(RequestHelper.Request.Connect);

            requestUpdater = UpdateRequests();
        }

        public void FireServerClosed()
        {
            if(ServerClosed != null)
            { 
                ServerClosed(this, null);
            }
        }

        void FireServerConnected()
        {
            if (ServerConnected != null)
                ServerConnected(this, null);
        }

        public bool Connect(string ipAddr, int tcpPort, int udpPort)
        {
            bool result = tcpClientIFace.Connect(ipAddr, tcpPort);
            if(result)
            {
                FireServerConnected();  
                   // Send(RequestHelper.Request.Connect);
            }
            return result;
        }
        
        public bool Connect()
        {
            bool result = tcpClientIFace.Connect(ipAddr, tcpPort);
            if(result)
            {
                FireServerConnected();
                //Send(RequestHelper.Request.Connect);
            }
            return result;
        }

        void FireRequestSend(RequestHelper.InPaintData data)
        {
            if(RequestSend != null)
            {
                RequestSend(data);
            }
        }

        void FireRequestReceived(RequestHelper.InPaintData data)
        { 
            switch (data.request)
            {
                case RequestHelper.Request.Connect:
                {
                    //Connection successfull with server
                    //Debug.Log(data.raw_data);
                    //recordTime = Int32.Parse(data.seperated_data[2]);
                }
                    break;
                case RequestHelper.Request.SendBackground:
                    {
                        //Debug.Log("Not implemented yet " + data.raw_data);
                        //recordTime = Int32.Parse(data.seperated_data[2]);
                    }
                    break;
                case RequestHelper.Request.SendMask:
                    {
                        //Debug.Log("Not implemented yet");
                        //prepTime = Int32.Parse(data.seperated_data[2]);
                    }
                    break;
                case RequestHelper.Request.GetInpaintedImage:
                    { 
                        //Debug.Log("Not implemented yet");
                        ///  Send(RequestHelper.Request.GetNumOfSamples); 
                    }
                    break; 
               
            }

            if (RequestReceived != null)
            {
                RequestReceived(data);
            }
        }

        public void HandleCallback(RequestHelper.InPaintData data)
        {
            if(isUpdatingRequests)
            {
                requestUpdater.MoveNext();
            }

            if(observer != null)
            {
                foreach (IGrtDataHandler handler in observer)
                {
                    handler.HandleGrtData(ref data); 
                }
            }
            
            FireRequestReceived(data);
        }

        public void Add(IGrtDataHandler handler)
        {
             
            if(observer == null)
            {
                observer = new List<IGrtDataHandler>();
            }
            observer.Add(handler);
        }

        bool Send(RequestHelper.Request request)
        { 
            RequestHelper.InPaintData data = new RequestHelper.InPaintData(); 
            RequestHelper.GetInpaintData("handshake:Connecting To server", request, ref data);
            //data.raw_data += ";"; // set end tag
            if (tcpClientIFace.Send(ref data))
            {
                FireRequestSend(data); 
                return true;
            }
            return false;
        }

        public bool Send(ref RequestHelper.InPaintData data)
        {  
            //data.raw_data += ";"; // set end tag
            if (tcpClientIFace.Send(ref data))
            {
                FireRequestSend(data);
                return true;
            }
            return false;
        }

       

        public void StopServer()
        { 
           tcpClientIFace.StopReading();
        }

        /// <summary>
        /// Updating getter request like get preptime (grt interface only)
        /// </summary>
        /// <returns></returns>
        IEnumerator UpdateRequests()
        {
            isUpdatingRequests = true;
            // this request should call at last 

            UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if(eventSystem == null)
            {
                Debug.LogError("Missing event system in scene");
            }
  
            RequestHelper.InPaintData data = new RequestHelper.InPaintData();
            RequestHelper.GetInpaintData("", RequestHelper.Request.SendBackground, ref data);
 
            Send(ref data);
            yield return null; 
       
            isUpdatingRequests = false;
        }

        /// <summary>
        /// load data in grt interface only
        /// </summary>
        public void Load()
        {
            if(tcpClientIFace.IsConnected)
            {
                StartCoroutine(requestUpdater);
            }
        }
    }
}
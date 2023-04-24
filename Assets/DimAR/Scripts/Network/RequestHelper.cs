using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GRT
{
    public class RequestHelper
    {
        public struct InPaintData
        { 
            public String raw_data;
            public Request request;
        }

   

        /// <summary>
        /// Get request, specific classifier und data (string[]) when getting callback of a request
        /// </summary>
        /// <param name="data_msg">received message</param>
        /// <param name="data">GRTData that will be generated from data_msg</param>
        /// <returns></returns>
        public static void GetInpaintData(string data_msg, Request req, ref InPaintData data)
        {
            data.request = req;
            data.raw_data = data_msg;
            
        }

        
        /// <summary>
        /// Get request, specific classifier und data (string[]) when getting callback of a request
        /// </summary>
        /// <param name="data_msg">received message</param>
        /// <param name="data">GRTData that will be generated from data_msg</param>
        /// <returns></returns>
        public static bool GetInpaintData(string  data_msg, ref InPaintData data)
        { 
           
            if (data_msg.Length < 3)
            {
                Debug.LogError("Message: " + (data_msg)  + " is to small");
                return false;
            }
            
            data.raw_data = data_msg;  
            return true;
        }

      

        #region Enums

        public enum Request
        {
            //ChooseClassifier,
            Connect,
            SendBackground,
            SendMask,
            GetInpaintedImage,
            Quit
        }
  

        #endregion
    }
}


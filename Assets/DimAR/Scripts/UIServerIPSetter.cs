using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
public class UIServerIPSetter : MonoBehaviour
{
    private string regExIPPattern = @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$";

    private void Start()
    {
        if (PlayerPrefs.HasKey("serverip"))
        {
            GetComponent<InputField>().text = PlayerPrefs.GetString("serverip");
        }
        else
        {
            var localipadress = GetLocalIPAddress();
            SetServerIP(localipadress); 
            GetComponent<InputField>().text = localipadress;
        }
    }

    public void CheckIPAdressFormat(string input)
    { 
        if( Regex.IsMatch(input, regExIPPattern, RegexOptions.IgnoreCase))
        {
           Debug.Log("correct ip format");
           SetServerIP(input);
        }
        else
        {
            Debug.Log("wrong ip adress");
            ResetServerIPInputField();
        }
    }

    private void SetServerIP(string inputIP)
    {
        Debug.Log("SafeIP to Playerprefs");
        PlayerPrefs.SetString("serverip",inputIP);
        PlayerPrefs.Save();
        GetComponent<InputField>().textComponent.color = Color.green;
    }

    private void ResetServerIPInputField()
    {
        GetComponent<InputField>().text = "127.0.0.1";
        GetComponent<InputField>().textComponent.color = Color.red;
        if (PlayerPrefs.HasKey("serverip"))
        {
            PlayerPrefs.DeleteKey("serverip"); 
            PlayerPrefs.Save();
        }
    }
    
    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            { 
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}

using System.Collections;
using System.Collections.Generic;
using GRT;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public void LoadScene(string sceneName)
    { 
        if(PlayerPrefs.HasKey("serverip")) {SceneManager.LoadScene(sceneName, LoadSceneMode.Single);}  
    }
    
    public void LoadSceneByID(int sceneId)
    {
        if (PlayerPrefs.HasKey("serverip"))
        {
            SceneManager.LoadScene(sceneId, LoadSceneMode.Single); 
            if(sceneId == 0) ClosePythonServer();
        }
    }

    private void ClosePythonServer()
    {
        RequestHelper.InPaintData connect_data = new RequestHelper.InPaintData();
        connect_data.raw_data = "close:EndConnection==";
        connect_data.request = RequestHelper.Request.Connect;
        BaseGRTClient.Instance.Send(ref connect_data);  
        BaseGRTClient.Instance.StopServer();
        Destroy(BaseGRTClient.Instance);
    }
}

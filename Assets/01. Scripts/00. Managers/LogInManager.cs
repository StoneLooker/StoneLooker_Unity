using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text;

public class LogInManager : MonoBehaviour
{
    private string signUpUrl = "http://43.203.76.106:8080/save";
    private string logInUrl = "http://43.203.76.106:8080/login";
    private string logOutUrl = "http://43.203.76.106:8080/logout";

    public TMP_InputField idInput;
    public TMP_InputField passwordInput;

    public GameObject infoScreen;
    public GameObject logInScreen;

    public GameObject logInBtn;
    public GameObject logOutBtn;
    

    private bool isLoggedIn = false;

    public void SignUp()
    {
        StartCoroutine(SignUpRequest());
    }

    public void LogIn()
    {
        StartCoroutine(LogInRequest());
    }

    public void LogOut()
    {
        StartCoroutine(LogOutRequest());
    }

    IEnumerator SignUpRequest()
    {
        Member m = getMemberFromFields();

        if (m == null) yield break;

        string json = JsonUtility.ToJson(m);

        Debug.Log(json);

        signUpUrl = signUpUrl + "?memberNickName=" + m.memberNickName + "&memberPassword=" + m.memberPassword;

        UnityWebRequest www = new UnityWebRequest(signUpUrl, "POST");
        byte[] jsonToSend = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        switch(www.result){
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.Log("Error: " + www.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.Log("HTTP Error: " + www.error + " Response: " + www.downloadHandler.text);
                if(www.downloadHandler.text == "Save fail")
                    UpdateInfoText("ID already exists.");
                break;
            case UnityWebRequest.Result.Success:
                UpdateInfoText("Sign Up Successful.");
                break;
        }
    }

    IEnumerator LogInRequest()
    {
        Member m = getMemberFromFields();

        string json = JsonUtility.ToJson(m);

        string requestUrl = logInUrl + "?memberNickName=" + m.memberNickName + "&memberPassword=" + m.memberPassword;

        Debug.Log(json);

        UnityWebRequest www = new UnityWebRequest(requestUrl, "POST");
        byte[] jsonToSend = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        www.SetRequestHeader("Cache-Control", "no-cache");
        www.SetRequestHeader("Pragma", "no-cache");

        
        yield return www.SendWebRequest();

        Debug.Log(www.downloadHandler.text);

        switch(www.result){
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.Log("Error: " + www.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.Log("HTTP Error: " + www.error + " Response: " + www.downloadHandler.text);
                break;
            case UnityWebRequest.Result.Success:
                if(www.downloadHandler.text == "login fail")
                    UpdateInfoText("Incorrect user ID or Password.");
                else{
                    isLoggedIn = true;
                    UpdateButtonListener();
                    logInBtn.SetActive(false);
                    logOutBtn.SetActive(true);
                    logInScreen.SetActive(false);
                    GameManager.Instance.nickname = m.memberNickName;
                    GameManager.Instance.id = www.downloadHandler.text;
                    DataManager.Instance.LoadGameData(GameManager.Instance.id);
                    GameManager.Instance.ApplyGameData(DataManager.Instance.data);
                }
                break;
        }
    }

    IEnumerator LogOutRequest()
    {
        UnityWebRequest www = UnityWebRequest.Get(logOutUrl);

        www.SetRequestHeader("Access", "application/json");
        
        yield return www.SendWebRequest();

        switch(www.result){
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.Log("Error: " + www.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.Log("HTTP Error: " + www.error);
                break;
            case UnityWebRequest.Result.Success:
                isLoggedIn = false;
                UpdateButtonListener();
                logInBtn.SetActive(true);
                logOutBtn.SetActive(false);
                UpdateInfoText("Log Out successful.");
                break;
        }
    }

    public void UpdateButtonListener()
    {
        GameManager.Instance._controller._button.gameStart.onClick.RemoveAllListeners();
        if (isLoggedIn)
            GameManager.Instance._controller._button.gameStart.onClick.AddListener(GameManager.Instance._controller._button.gameStart.GetComponent<FadeOut>().LoadingSceneLoad);
        else
            GameManager.Instance._controller._button.gameStart.onClick.AddListener(() => UpdateInfoText("Log In is required."));
    }

    private void UpdateInfoText(string str)
    {
        infoScreen.SetActive(true);
        infoScreen.transform.GetChild(0).GetComponent<TMP_Text>().text = str;
    }

    private Member getMemberFromFields()
    {
        Member m = new Member();
        if(idInput.text.Length < 10)
            m.memberNickName = idInput.text;
        else
        {   
            UpdateInfoText("Nickname up to 10 characters.");
            return null;
        }
        m.memberPassword = passwordInput.text;
        return m;
    }
}
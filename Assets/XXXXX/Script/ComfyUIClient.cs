













using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class ComfyUIClient : MonoBehaviour
{
    public string comfyUIUrl = "http://localhost:8188/prompt";

    [Header("是否自动循环请求")]
    public bool autoLoop = false;

    private Coroutine loopCoroutine;

    void Start()
    {
        if (autoLoop)
        {
            loopCoroutine = StartCoroutine(AutoLoopRequest());
        }
    }

    /// <summary>
    /// 👉 手动触发（比如挂到按钮）
    /// </summary>
    public void TriggerRequest()
    {
        StartCoroutine(SendComfyUIRequest());
    }

    /// <summary>
    /// 🔁 实时轮询：前一个请求完成后立刻进行下一次
    /// </summary>
    IEnumerator AutoLoopRequest()
    {
        while (true)
        {
            yield return SendComfyUIRequest(); // 没有 WaitForSeconds
        }
    }

    /// <summary>
    /// 📡 发送 ComfyUI 请求
    /// </summary>
    IEnumerator SendComfyUIRequest()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("workflow");
        if (jsonFile == null)
        {
            Debug.LogError("❌ 找不到 workflow.json 文件！");
            yield break;
        }

        string workflowJson = jsonFile.text;
        byte[] bodyRaw = Encoding.UTF8.GetBytes(workflowJson);

        UnityWebRequest request = new UnityWebRequest(comfyUIUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Workflow submitted successfully!");
            Debug.Log("Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("❌ Error: " + request.responseCode + " - " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }

    /// <summary>
    /// 👉 停止自动循环（可选）
    /// </summary>
    public void StopAutoLoop()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }
    }
}

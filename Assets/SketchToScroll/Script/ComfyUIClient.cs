


using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Simple client for sending workflow JSON payloads to a local ComfyUI instance.
/// Optionally uploads a sketch texture to ComfyUI via HTTP before submitting the workflow.
/// </summary>
[DisallowMultipleComponent]
public class ComfyUIClient : MonoBehaviour
{
    private const string DefaultWorkflowResourceName = "workflow";
    private const string PromptPathSuffix = "/prompt";

    [Header("ComfyUI Endpoint")]
    [Tooltip("HTTP endpoint of ComfyUI /prompt API (e.g. http://localhost:8188/prompt).")]
    public string comfyUIUrl = "http://localhost:8188/prompt";

    [Header("Workflow Settings")]
    [Tooltip("Name of the workflow JSON file under a Resources folder, without extension.")]
    public string workflowResourceName = DefaultWorkflowResourceName;

    [Header("Optional Image Upload")]
    [Tooltip("If assigned, the current board texture will be uploaded before the workflow is sent.")]
    public BoardPaintManager boardManager;

    [Tooltip("Upload the current board texture to ComfyUI before submitting the workflow.")]
    public bool uploadCurrentBoardTexture = true;

    [Tooltip("Filename used when uploading the sketch image.")]
    public string uploadFileName = "unity_sketch.png";

    [Header("Looping")]
    [Tooltip("If true, continuously sends workflows one after another.")]
    public bool autoLoop;

    private Coroutine loopCoroutine;

    [System.Serializable]
    private class UploadResponse
    {
        public string name;
        public string subfolder;
        public string type;
    }

    private void Start()
    {
        if (autoLoop)
        {
            loopCoroutine = StartCoroutine(AutoLoopRequest());
        }
    }

    /// <summary>
    /// Manually trigger a single workflow submission (e.g. from a UI button).
    /// </summary>
    public void TriggerRequest()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        StartCoroutine(SendComfyUIRequest());
    }

    /// <summary>
    /// Real-time polling loop: after one request finishes, immediately send the next.
    /// </summary>
    private IEnumerator AutoLoopRequest()
    {
        while (true)
        {
            yield return SendComfyUIRequest();
        }
    }

    /// <summary>
    /// Sends the configured workflow JSON to ComfyUI.
    /// If configured, uploads the current board texture first and injects the uploaded filename
    /// into the workflow JSON by replacing the placeholder "__UNITY_IMAGE_NAME__".
    /// </summary>
    private IEnumerator SendComfyUIRequest()
    {
        var jsonFile = Resources.Load<TextAsset>(workflowResourceName);
        if (jsonFile == null)
        {
            Debug.LogError($"❌ Workflow TextAsset '{workflowResourceName}.json' not found in any Resources folder.");
            yield break;
        }

        var workflowJson = jsonFile.text;

        var baseUrl = GetServerBaseUrl();
        var promptUrl = $"{baseUrl}{PromptPathSuffix}";
        var uploadUrl = $"{baseUrl}/upload/image";

        if (uploadCurrentBoardTexture && boardManager != null && boardManager.drawTexture != null)
        {
            var pngData = boardManager.drawTexture.EncodeToPNG();
            if (pngData == null || pngData.Length == 0)
            {
                Debug.LogWarning("⚠️ Failed to encode board texture to PNG; sending workflow without image upload.", this);
            }
            else
            {
                var form = new WWWForm();
                form.AddBinaryData("image", pngData, uploadFileName, "image/png");

                using (var uploadRequest = UnityWebRequest.Post(uploadUrl, form))
                {
                    yield return uploadRequest.SendWebRequest();

                    if (uploadRequest.result == UnityWebRequest.Result.Success)
                    {
                        var responseText = uploadRequest.downloadHandler.text;
                        if (!string.IsNullOrEmpty(responseText))
                        {
                            try
                            {
                                var uploadResponse = JsonUtility.FromJson<UploadResponse>(responseText);
                                if (uploadResponse != null && !string.IsNullOrEmpty(uploadResponse.name))
                                {
                                    workflowJson = workflowJson.Replace("__UNITY_IMAGE_NAME__", uploadResponse.name);

                                    if (!string.IsNullOrEmpty(uploadResponse.subfolder))
                                    {
                                        workflowJson = workflowJson.Replace("__UNITY_IMAGE_SUBFOLDER__", uploadResponse.subfolder);
                                    }
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogWarning($"⚠️ Failed to parse upload response JSON: {ex.Message}. Raw: {responseText}", this);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"❌ Image upload failed. Code: {uploadRequest.responseCode}, Error: {uploadRequest.error}", this);
                        if (!string.IsNullOrEmpty(uploadRequest.downloadHandler.text))
                        {
                            Debug.LogError("Upload response: " + uploadRequest.downloadHandler.text);
                        }
                    }
                }
            }
        }

        var bodyRaw = Encoding.UTF8.GetBytes(workflowJson);

        using (var request = new UnityWebRequest(promptUrl, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Workflow submitted successfully.");
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"❌ ComfyUI request failed. Code: {request.responseCode}, Error: {request.error}", this);

                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    Debug.LogError("Response: " + request.downloadHandler.text);
                }
            }
        }
    }

    /// <summary>
    /// Stops the auto-looping request cycle, if it is currently running.
    /// </summary>
    public void StopAutoLoop()
    {
        if (loopCoroutine == null)
        {
            return;
        }

        StopCoroutine(loopCoroutine);
        loopCoroutine = null;
    }

    /// <summary>
    /// Returns the base server URL (without the /prompt suffix).
    /// </summary>
    private string GetServerBaseUrl()
    {
        if (string.IsNullOrWhiteSpace(comfyUIUrl))
        {
            return "http://localhost:8188";
        }

        var trimmed = comfyUIUrl.TrimEnd('/');

        if (trimmed.EndsWith(PromptPathSuffix))
        {
            return trimmed.Substring(0, trimmed.Length - PromptPathSuffix.Length);
        }

        return trimmed;
    }
}

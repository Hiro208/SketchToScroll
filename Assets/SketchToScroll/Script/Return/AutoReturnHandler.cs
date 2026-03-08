using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoReturnManager : MonoBehaviour
{
    [System.Serializable]
    public class ReturnGroup
    {
        public GameObject objectA;         
        public Transform returnTarget;     
        public string triggerTagB;         
        public bool hasReturned = false;   
    }

    public List<ReturnGroup> returnGroups;

    private void OnTriggerEnter(Collider other)
    {
        foreach (var group in returnGroups)
        {
            if (!group.hasReturned && other.CompareTag(group.triggerTagB))
            {
                StartCoroutine(HandleReturnSequence(group));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        foreach (var group in returnGroups)
        {
            if (group.hasReturned && other.CompareTag(group.triggerTagB))
            {
                group.hasReturned = false;
                Debug.Log($"触发器退出，重置 hasReturned 状态：{group.objectA.name}");
            }
        }
    }

    private IEnumerator HandleReturnSequence(ReturnGroup group)
    {
        group.hasReturned = true;

        
        Transform handGrabChild = group.objectA.transform.Find("HandGrabInteractable");
        if (handGrabChild != null)
        {
            handGrabChild.gameObject.SetActive(false);
            Debug.Log($"已禁用 HandGrabInteractable 子物体：{group.objectA.name}");

            yield return new WaitForSeconds(0.5f); // 等待 1 秒

            // 归位操作
            group.objectA.transform.position = group.returnTarget.position;
            group.objectA.transform.rotation = group.returnTarget.rotation;
            Debug.Log($"物体已归位：{group.objectA.name}");

            // 重新启用抓取子物体
            handGrabChild.gameObject.SetActive(true);
            Debug.Log($"重新启用 HandGrabInteractable 子物体：{group.objectA.name}");
        }
        else
        {
            Debug.LogWarning($"未找到 HandGrabInteractable 子物体：{group.objectA.name}");
        }
    }
}


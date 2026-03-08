using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 当 PenTip 进入触发区域时，将当前 Trigger 的颜色传递给它，并同步更新其绘画颜色（BallPainter）。
/// </summary>
public class ColorTrigger : MonoBehaviour
{
    // 触发器中指定的颜色
    public Color assignedColor = Color.white;

    // 只响应指定 Tag 的物体（通常是笔尖）
    public string targetTag = "PenTip";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            // 1. 修改 ColorChangeM 的颜色
            var colorManager = other.GetComponent<ColorChangeM>();
            if (colorManager != null)
            {
                colorManager.ApplyColor(assignedColor);
            }

            // 2. 同步到 BallPainter 的绘画颜色
            var painter = other.GetComponent<BallPainter>();
            if (painter != null)
            {
                painter.paintColor = assignedColor;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            // 暂时无离开逻辑，可在此重置颜色等操作
        }
    }
}


using UnityEngine;

public class StampTestTrigger : MonoBehaviour
{
    public StampProcessor stampProcessor; // 引用同一个物体上的 StampProcessor 脚本

    void Update()
    {
        // 按下键盘空格键，触发盖章
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (stampProcessor != null)
            {
                stampProcessor.ProcessStamp();
                Debug.Log("测试触发：键盘按下空格，盖章函数已调用");
            }
            else
            {
                Debug.LogError("StampProcessor 没有拖进来！");
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkPen : MonoBehaviour
{

    private Texture tex;//这张图是板子的原图
    public RenderTexture cacheTex;//缓存上一帧的图
    RenderTexture currentTex;//当前帧操作的图
    private float brushMaxSize;

    public float brushSize=0.01f;
    public Color brushCol=Color.red;

    private Material effectMat;//用来处理图像的材质

    private Material renderMat;//原始面板的材质


    public Transform penHead;
    public Transform board;

    private Vector2 lastuv;
    private bool isDown;
    // Start is called before the first frame update
    void Start()
    {
        Initialized();
    }

    // Update is called once per frame
    void Update()
    {

        Ray ray = new Ray(penHead.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 1, LayerMask.GetMask("Board")))
        {
            if (raycastHit.distance < 0.05f)
            {
                if (!isDown)
                {
                    isDown = true;
                    lastuv = raycastHit.textureCoord2;
                }
                brushSize = Mathf.Clamp((0.05f / raycastHit.distance) * 0.001f,0, brushMaxSize);
                RenderBrushToBoard(raycastHit);
                lastuv = raycastHit.textureCoord2;
            }
            else {
                isDown = false;
            }
           
        }
        else {
            isDown = false;
        }
        
    }

    private void RenderBrushToBoard(RaycastHit hit) {

        Vector2 dir = hit.textureCoord2 - lastuv;

        if (Vector3.SqrMagnitude(dir)>brushSize*brushSize) {
            int length = Mathf.CeilToInt(dir.magnitude / brushSize);

            for (int i = 0; i < length; i++)
            {
                RenderToMatTex(lastuv+dir.normalized*i*brushSize);
            }
        
        }
        RenderToMatTex(hit.textureCoord2);
    }

    private void RenderToMatTex(Vector2 uv) {
        effectMat.SetVector("_BrushPos", new Vector4(uv.x, uv.y,lastuv.x,lastuv.y));
        effectMat.SetColor("_BrushColor", brushCol);
        effectMat.SetFloat("_BrushSize", brushSize);
        Graphics.Blit(cacheTex, currentTex, effectMat);
        renderMat.SetTexture("_MainText", currentTex);
        Graphics.Blit(currentTex,cacheTex);
    
    }

    private void Initialized()
    {
        brushMaxSize = brushSize;
        effectMat =new Material(Shader.Find("Brush/MarkPenEffect"));
        Material boardMat = board.GetComponent<MeshRenderer>().material;
        tex = boardMat.mainTexture;

        renderMat = boardMat;

        cacheTex = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(tex,cacheTex);
        renderMat.SetTexture("_MainTex",cacheTex);

        currentTex = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);

    }

   
}

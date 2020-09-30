using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class Painting : MonoBehaviour
{
    public RawImage rawImage;
    public Material _material = null;
    public RenderTexture texRender;
    public Texture2D brush1;
    public Texture2D brush2;
    private Texture2D curbrush;
    float scale = 0f;
    private bool bDrawed = false;
    public bool Drawed { get {
            return bDrawed;
        } 
    }

    private RectTransform rectTransform;
    
    float ratio = 0.8f;

    private void SetBrush(Texture2D brush)
    {
        curbrush = brush;
        _material.SetTexture("_MainTex", curbrush);
    }

    public void SetRation(float ratio)
    {
        this.ratio = ratio;
    }

    public void SetDivide(int div)
    {
        num = div;
    }

    private void OnEnable()
    {
        Initializetor();
    }

    private void Initializetor()
    {
        rectTransform = rawImage.GetComponent<RectTransform>();
        texRender = CreateRenderTexture((int)rectTransform.rect.width, (int)rectTransform.rect.height);
        SetBrush(brush1);
        
    }

    private RenderTexture CreateRenderTexture(int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        rt.Create();
        return rt;
    }

    Vector3 startPosition = Vector3.zero;
    Vector3 endPosition = Vector3.zero;
    private float lastDistance;
    private float brushScale = 0.5f;
    private Vector3[] PositionArray = new Vector3[3];
    private int a = 0;
    private Vector3[] BezierArray = new Vector3[4];
    private int b = 0;
    private float[] speedArray = new float[4];
    private int s = 0;
    public int num = 50;


    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
            {
                if (!bDrawed)
                {
                    bDrawed = !bDrawed;
                }
                OnMouseMove(Input.mousePosition);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp();
        }
    }


    public void ClearBrush()
    {
        bDrawed = false;
        Graphics.SetRenderTarget(texRender);
        GL.PushMatrix();
        GL.Clear(true, true, Color.clear);
        GL.PopMatrix();
    }


    private void OnMouseUp()
    {
        startPosition = Vector3.zero;
        //brushScale = 0.5f;
        a = 0;
        b = 0;
        s = 0;
    }

    public Texture2D GetTexture2D()
    {
        return texRender.RenderTexture2Texture2D();
    }
    

    private void OnMouseMove(Vector3 pos)
    {
        if (startPosition == Vector3.zero)
        {
            startPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        }
        endPosition = pos;
        float distance = Vector3.Distance(startPosition, endPosition);
        brushScale = SetScale(distance);

        ThreeOrderBézierCurse(pos, distance, 4.5f);

        startPosition = endPosition;
        lastDistance = distance;
    }

    private bool bChanged = false;
    private void ThreeOrderBézierCurse(Vector3 pos, float distance, float targetPosOffset)
    {
        bChanged = false;
        //记录坐标
        BezierArray[b] = pos;
        b++;
        //记录速度
        speedArray[s] = distance;
        s++;
        if (b == 4)
        {
            Vector3 temp1 = BezierArray[1];
            Vector3 temp2 = BezierArray[2];

            var dir = BezierArray[2] - BezierArray[3];
            float temp = 1f;
            if (dir.x * dir.y > 0)
            {
                temp = 1f;
            }
            else if (dir.x * dir.y < 0)
            {
                temp = 0.5f;
            }
            else
            {
                bChanged = true;
            }

            for (int index = 0; index < num; index++)
            {
                float t = (1.0f / (1f*num)) * index;
                Vector3 target = GetPoint(BezierArray[0], BezierArray[1], BezierArray[2], BezierArray[3], t);
                float deltaspeed = (float)(speedArray[3] - speedArray[0]) / num;
                float randomOffset = Random.Range(-targetPosOffset, targetPosOffset);
                if(!bChanged)
                    scale = temp * SetScale(speedArray[0] + (deltaspeed * index));

                Draw(target + randomOffset * Vector3.one, scale);
            }

            BezierArray[0] = temp1;
            BezierArray[1] = temp2;
            BezierArray[2] = BezierArray[3];

            speedArray[0] = speedArray[1];
            speedArray[1] = speedArray[2];
            speedArray[2] = speedArray[3];
            b = 3;
            s = 3;
        }
        else
        {
            Draw(endPosition, brushScale);
        }

    }

    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            oneMinusT * oneMinusT * oneMinusT * p0 +
            3f * oneMinusT * oneMinusT * t * p1 +
            3f * oneMinusT * t * t * p2 +
            t * t * t * p3;
    }



    float SetScale(float distance)
    {
        float Scale = 0;
        if (distance < 100)
        {
            Scale = 0.8f - 0.005f * distance;
        }
        else
        {
            Scale = 0.425f - 0.00125f * distance;
        }
        if (Scale <= 0.05f)
        {
            Scale = 0.05f;
        }
        return ratio* Scale;
    }

    private void Draw(Vector3 point, float scale)
    {
        var rect = CaculateRealtiveRect(point, scale);
        DrawIng(rect);
    }



    private Rect CaculateRealtiveRect(Vector3 point, float scale)
    {
        Camera camera = rawImage.canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, point, camera, out Vector2 localpositon);

        localpositon.x += rectTransform.rect.width / 2;
        localpositon.y += rectTransform.rect.height / 2;

        float xmin = (localpositon.x - curbrush.width* scale / 2f) / rectTransform.rect.width;
        float xmax = (localpositon.x + curbrush.width * scale / 2f) / rectTransform.rect.width;
        float ymin = (localpositon.y - curbrush.height * scale / 2f) / rectTransform.rect.height;
        float yMax = (localpositon.y + curbrush.height * scale / 2f) / rectTransform.rect.height;

        Rect rect = new Rect();
        rect.xMin = xmin;
        rect.xMax = xmax;
        rect.yMin = ymin;
        rect.yMax = yMax;

        return rect;
    }

    void DrawIng(Rect rect)
    {
        Graphics.SetRenderTarget(texRender.colorBuffer, texRender.depthBuffer);
        GL.PushMatrix();
        _material.SetPass(0);
        GL.LoadOrtho();
        GL.Begin(GL.QUADS);
        GL.TexCoord2(0, 0);GL.Vertex3(rect.xMin, rect.yMin, 0);
        GL.TexCoord2(0, 1);GL.Vertex3(rect.xMin, rect.yMax, 0);
        GL.TexCoord2(1, 1);GL.Vertex3(rect.xMax, rect.yMax, 0);
        GL.TexCoord2(1, 0);GL.Vertex3(rect.xMax, rect.yMin, 0);
        GL.End();
        GL.PopMatrix();
        rawImage.texture  = texRender;
    }
}
public static class ExtensionMethod
{
    public static Texture2D RenderTexture2Texture2D(this RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
       
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        return tex;
    }
}

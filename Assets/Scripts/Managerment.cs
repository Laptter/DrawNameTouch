using System;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Managerment : MonoBehaviour
{
    private Painting painting;
    private string saveFolder;
    

    private void Start()
    {
        var path = Application.streamingAssetsPath + "/" +"Config.xml";
        XElement root = XElement.Load(path);
        saveFolder = root.Element("SavePath").Value;
        float ratio = float.Parse(root.Element("Ratio").Value);
        int div = int.Parse(root.Element("Divide").Value);
            
        painting = GetComponent<Painting>();
        painting.SetRation(ratio);
        painting.SetDivide(div);
        Cursor.visible = bool.Parse(root.Element("CursorVisible").Value);
    }



    public void RePaint()
    {
        painting.ClearBrush();
    }
    public void SaveTextureToFile()
    {
        if (painting.Drawed)
        {
            var t2d = painting.GetTexture2D();
            byte[] _bytes = t2d.EncodeToPNG();
            string fileName = string.Format("recording_{0}.png", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff"));
            string fullPath = System.IO.Path.Combine(saveFolder, fileName).Replace('\\', '/');
            System.IO.File.WriteAllBytes(fullPath, _bytes);
            DestroyImmediate(t2d);
        }
    }


    private void OnDisable()
    {
        
    }
}

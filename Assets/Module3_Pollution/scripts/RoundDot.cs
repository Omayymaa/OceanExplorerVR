using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RoundDot : MonoBehaviour
{
    public int size = 64;

    void Awake()
    {
        Texture2D t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size / 2f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                float a = Mathf.Clamp01((r - d) / 2f);
                t.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }

        t.Apply();
        GetComponent<Image>().sprite = Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
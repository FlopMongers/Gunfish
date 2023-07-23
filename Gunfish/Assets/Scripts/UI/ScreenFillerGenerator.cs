using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFillerGenerator : MonoBehaviour
{
    public List<GameObject> sprites = new List<GameObject>();
    Vector2 rowRange = new Vector2(0.1f, 0.3f), startPos = new Vector2(-0.1f, -0.1f), endPos = new Vector2(1.1f, 1.1f);

    float postScale = 4f, colorScale = 0.4f;

    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Generate()
    {
        Vector2 tracker = startPos;
        float minY = float.MaxValue;

        while (tracker.y < endPos.y)
        {
            while (tracker.x < endPos.x)
            {
                GameObject sprite = Instantiate(sprites[Random.Range(0, sprites.Count)]);
                sprite.transform.parent = transform;
                var rect = sprite.GetComponent<RectTransform>();
                rect.anchorMin = tracker;
                rect.anchorMax = tracker + (Vector2.one * Random.Range(rowRange.x, rowRange.y));
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                minY = Mathf.Min(minY, rect.anchorMax.y);
                tracker = new Vector2(rect.anchorMax.x, tracker.y);
                rect.Rotate(Vector3.forward * Random.Range(0, 360));
                rect.localScale *= postScale;
                Vector3 col = Random.insideUnitSphere * colorScale;
                Image img = sprite.GetComponent<Image>();
                img.color += new Color(col.x, col.y, col.z);
            }
            tracker = new Vector2(startPos.x, minY);
            minY = float.MaxValue;
        }

        List<int> indexes = new List<int>();
        List<Transform> items = new List<Transform>();
        for (int i = 0; i < transform.childCount; ++i)
        {
            indexes.Add(i);
            items.Add(transform.GetChild(i));
        }

        foreach (var item in items)
        {
            item.SetSiblingIndex(indexes[Random.Range(0, indexes.Count)]);
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Image img = transform.GetChild(i).GetComponent<Image>();
            img.color *= (1f - i / (float)transform.childCount) - 0.25f;
            img.color += new Color(0, 0, 0, 1f);
        }
    }
}

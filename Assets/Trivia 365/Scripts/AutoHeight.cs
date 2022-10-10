using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoHeight : MonoBehaviour
{
    public GameObject RankBg;
    int childCounts = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RectTransform rt = RankBg.transform.GetComponent<RectTransform>();
        //Debug.Log(rt.rect.width);

        // resize card background
        childCounts = gameObject.transform.childCount;
        if (childCounts > 12)
        {
            rt.sizeDelta = new Vector2(920, 5 + childCounts * 105); // width, height
        } else
        {
            rt.sizeDelta = new Vector2(920, 1315); // width, height
        }

        //Debug.Log(gameObject.transform.GetComponent<RectTransform>().rect.width);
    }
}

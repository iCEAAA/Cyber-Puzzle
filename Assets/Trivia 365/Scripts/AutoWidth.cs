using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoWidth : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RectTransform rt = GameObject.Find("CardBg").transform.GetComponent<RectTransform>();
        //Debug.Log(rt.rect.width);

        // resize card background
        int childCounts = gameObject.transform.childCount;
        rt.sizeDelta = new Vector2(20 + childCounts * 220, 300); // width, height

        //Debug.Log(gameObject.transform.GetComponent<RectTransform>().rect.width);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

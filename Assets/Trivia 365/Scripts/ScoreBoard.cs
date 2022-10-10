using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class ScoreBoard : MonoBehaviour
{
    public class Record
    {
        public string uuid { get; set; }
        public string player { get; set; }
        public string score { get; set; }
        public string ptime { get; set; }
    }

    public class Records
    {
        public List<Record> records { get; set; }
    }

    public List<Record> items;
    /// <summary>
    /// 总数据数量
    /// </summary>
    public int ItemsCount;
    /// <summary>
    /// 总页数，没有数据默认为1
    /// </summary>
    public int PageCount = 1;
    /// <summary>
    /// 当前页数的标签
    /// </summary>
    public Text PanelText;
    /// <summary>
    /// 当前页面索引
    /// </summary>
    public int PageIndex = 1;
    /// <summary>
    /// 上一页按钮
    /// </summary>
    public Button BtnPrevious;
    /// <summary>
    /// 下一页按钮
    /// </summary>
    public Button BtnNext;
    /// <summary>
    /// 父物体组件，所有的子物体全部挂在这个上
    /// </summary>
    public GameObject ParentObj;
    /// <summary>
    /// 需要使用的预制件,该游戏物体上会绑定各种事件、资源等，同时也是子物体
    /// </summary>
    public GameObject gameObjectPrefab;
    void Start()
    {
        InitUGUI();
        //Init();
    }

    /// <summary>
    /// 初始化UGUI
    /// </summary>
    private void InitUGUI()
    {
        DestroyChildObject(ParentObj);
    }

    /// <summary>
    /// 下一页事件
    /// </summary>
    public void Next()
    {
        //最后一页禁止翻页
        if (PageIndex == PageCount)
            return;
        if (PageIndex >= PageCount)
            PageIndex = PageCount;
        DestroyChildObject(ParentObj);
        PageIndex += 1;
        UpdateUI(PageIndex);
        //更新页面页数
        PanelText.text = string.Format("{0}/{1}", PageIndex.ToString(), PageCount.ToString());
    }

    public void Previous()
    {
        //第一页禁止翻页
        if (PageIndex == 1)
            return;
        DestroyChildObject(ParentObj);
        PageIndex -= 1;
        UpdateUI(PageIndex);
        //更新页面页数
        PanelText.text = string.Format("{0}/{1}", PageIndex.ToString(), PageCount.ToString());
    }

    /// <summary>
    /// 初始化元素
    /// </summary>
    public void Init(List<Record> records)
    {
        //计算元素总个数        
        items = records;

        ItemsCount = items.Count;
        //计算总页数
        PageCount = items.Count % 10 == 0 ? items.Count / 10 : items.Count / 10 + 1;
        if (items.Count <= 10)
            PageCount = 1;
        //调用绑定页数方法
        UpdateUI(PageIndex);
        //更新界面页数
        PanelText.text = string.Format("{0}/{1}", PageIndex.ToString(), PageCount.ToString());
    }
    /// <summary>
    /// 绑定页数方法
    /// </summary>
    /// <param name="当前页码"></param>
    public void UpdateUI(int currentIndex)
    {
        //没有数据则直接return
        if (ItemsCount <= 0)
        {
            return;
        }
        for (int i = (PageIndex - 1) * 10; i < ((PageIndex - 1) * 10 + 10 > ItemsCount ? ItemsCount : (PageIndex - 1) * 10 + 10); i++)
        {
            var record = Instantiate(gameObjectPrefab) as GameObject;
            record.transform.SetParent(ParentObj.transform,false);
            // rank
            int rank = (i + 1);
            record.transform.GetChild(0).GetComponent<Text>().text = string.Concat(rank.ToString());
            // name
            record.transform.GetChild(1).GetComponent<Text>().text = string.Concat(items[i].player);
            // score
            record.transform.GetChild(2).GetComponent<Text>().text = string.Concat(items[i].score.ToString());
        }
    }

    /// <summary>
    /// 删除对象下的子对象
    /// </summary>
    /// <param name="父物体"></param>
    public void DestroyChildObject(GameObject parentObject)
    {
        if (parentObject == null)
            return;
        for (int i = parentObject.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(parentObject.transform.GetChild(i).gameObject);
        }
        Resources.UnloadUnusedAssets();
    }

    public void getData(string jsonstr)
    {
        // 这里解析
        Records allRecords = JsonConvert.DeserializeObject<Records>(jsonstr);
        //Debug.Log(allRecords.records[0].uuid);
        //Debug.Log(allRecords.records[0].player);
        //Debug.Log(allRecords.records[0].score);
        //Debug.Log(allRecords.records[0].ptime);
        InitUGUI();
        Init(allRecords.records);
    }



   

}

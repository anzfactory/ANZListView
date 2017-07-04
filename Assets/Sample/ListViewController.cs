/*********************************
 さんぷる

 素材CREDIT
 ぴぽや　http://piposozai.blog76.fc2.com/
*********************************/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Xyz.AnzFactory.UI;

public class ListViewController : MonoBehaviour, ANZListView.IDataSource, ANZListView.IActionDelegate
{
    #region "Serialize Fields"
    [SerializeField] private Font font;
    [SerializeField] private ANZListView listView;
    [SerializeField] private GameObject itemTemplate;
    #endregion

    #region "Fields"
    private List<ItemData> items;
    #endregion

    #region "Lifecycle"
    private void Awake()
    {
        this.listView.DataSource = this;
        this.listView.ActionDelegate = this;
    }

    private void Start()
    {
        this.items = new List<ItemData>();
        items.Add(new ItemData("icon1", "あいてむ１"));
        items.Add(new ItemData("icon2", "あいてむ２"));
        items.Add(new ItemData("icon3", "あいてむ３"));
        items.Add(new ItemData("icon4", "あいてむ４"));
        items.Add(new ItemData("icon5", "あいてむ５"));
        items.Add(new ItemData("icon4", "あいてむ６"));
        items.Add(new ItemData("icon3", "あいてむ７"));
        items.Add(new ItemData("icon2", "あいてむ８"));
        items.Add(new ItemData("icon1", "あいてむ９"));
        items.Add(new ItemData("icon5", "あいてむ１０"));

        this.listView.ReloadData();
    }
    #endregion

    #region "ANZListView.IDataSource"
    public int NumOfItems()
    {
        return this.items.Count;
    }
    public float HeightItem()
    {
        return 50f;
    }
    public GameObject ListViewItem(int index, GameObject item)
    {
        if (item == null) { // 新規作成
            item = GameObject.Instantiate<GameObject>(this.itemTemplate);
        }
        item.GetComponent<ListItem>().Display(this.items[index]);
        return item;
    }
    #endregion

    #region "ANZListView.IActionDelegate"
    public void TapListItem(int index, GameObject listItem)
    {
        Debug.Log(this.items[index].Title + "がたっぷされたよ！");
    }
    #endregion
}

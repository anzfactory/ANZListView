/*********************************
 Like UITableView
*********************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Xyz.AnzFactory.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class ANZListView : MonoBehaviour
    {

        public interface IDataSource
        {
            int NumOfItems();
            float HeightItem();
            GameObject ListViewItem(int index, GameObject item);
        }

        public interface IActionDelegate
        {
            void TapListItem(int index, GameObject listItem);
        }

        #region "Fields"
        private ScrollRect scrollRect;
        private float rowHeight;
        private int itemCount;
        private int visibleItemCount;
        private List<ListItemData> itemDataList;
        private List<ListItemData> visibleItemDataList;
        private float prevPositionY;
        #endregion

        #region "Properties"
        public float RowHeight
        {
            get { return this.rowHeight; }
        }
        public int ItemCount
        {
            get { return this.itemCount; }
        }
        public IDataSource DataSource { get; set; }
        public IActionDelegate ActionDelegate { get; set; }
        #endregion

        #region "Events"
        private void Awake()
        {
            this.rowHeight = 0;
            this.itemCount = 0;
            this.visibleItemCount = 0;
            this.itemDataList = new List<ListItemData>();
            this.prevPositionY = -100f;

            this.Setup();
        }
        #endregion

        #region "Events"
        public void ChangedScrollPosition(Vector2 position)
        {
            if (-100f >= this.prevPositionY) {
                // なにもしない
            } else if (this.prevPositionY > position.y) {
                List<ListItemData> items = this.VisibleItems();
                if (items.Count > 0) {
                    while (items[items.Count - 1].Position > this.visibleItemDataList[this.visibleItemDataList.Count - 1].Position) {
                        var topItem = this.visibleItemDataList[0];
                        var lastItem = this.visibleItemDataList[this.visibleItemDataList.Count - 1];
                        var targetItem = this.itemDataList[lastItem.Position + 1];

                        if (this.visibleItemDataList.Count >= this.visibleItemCount) {
                            // 外す
                            var recycleItemObject = topItem.PopItem();
                            this.visibleItemDataList.Remove(topItem);
                            // つけかえる
                            targetItem.SetItemObjcet(recycleItemObject);
                        }

                        this.UpdateListItem(targetItem);
                        this.visibleItemDataList.Add(targetItem);
                    }
                }
            } else if (this.prevPositionY < position.y) {
                List<ListItemData> items = this.VisibleItems();
                if (items.Count > 0) {
                    while (this.visibleItemDataList[0].Position > items[0].Position) {
                        var topItem = this.visibleItemDataList[0];
                        var lastItem = this.visibleItemDataList[this.visibleItemDataList.Count - 1];
                        if (topItem.Position > 0 && this.itemDataList.Count > topItem.Position) {
                            var targetItem = this.itemDataList[topItem.Position - 1];

                            if (this.visibleItemDataList.Count >= this.visibleItemCount) {
                                // 外す
                                var recycleItemObject = lastItem.PopItem();
                                this.visibleItemDataList.Remove(lastItem);
                                // つけかえる
                                targetItem.SetItemObjcet(recycleItemObject);
                            }

                            this.UpdateListItem(targetItem);
                            this.visibleItemDataList.Insert(0, targetItem);
                        }
                    }
                }
            }
            this.prevPositionY = position.y;
        }

        public void TapItem(GameObject listItem)
        {
            if (this.ActionDelegate == null) {
                return;
            }

            for (int i = 0; i < this.visibleItemDataList.Count; i++) {
                if (this.visibleItemDataList[i].Item == listItem) {
                    this.ActionDelegate.TapListItem(this.visibleItemDataList[i].Position, listItem);
                    break;
                }
            }

        }
        #endregion

        #region "Public Methods"
        public void ReloadData()
        {
            StartCoroutine(this._reloadData());
        }
        private IEnumerator _reloadData()
        {
            yield return new WaitForEndOfFrame();

            this.itemCount = this.DataSource.NumOfItems();
            this.rowHeight = this.DataSource.HeightItem();
            this.visibleItemCount = Mathf.CeilToInt(this.scrollRect.viewport.rect.height / this.rowHeight) + 2;

            this.FillItems();
            this.visibleItemDataList = this.VisibleItems();
            foreach (var listItem in this.visibleItemDataList) {
                this.UpdateListItem(listItem);
            }
        }
        #endregion

        #region "Private Methods"
        private void Setup()
        {
            this.scrollRect = this.gameObject.GetComponent<ScrollRect>();

            var verticalLayout = this.scrollRect.content.gameObject.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout == null) {
                verticalLayout = this.scrollRect.content.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childControlHeight = true;
            verticalLayout.childControlWidth = true;

            var contentSizeFitter = this.scrollRect.content.gameObject.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null) {
                contentSizeFitter = this.scrollRect.content.gameObject.AddComponent<ContentSizeFitter>();
            }
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            this.scrollRect.onValueChanged.RemoveListener(this.ChangedScrollPosition);
            this.scrollRect.onValueChanged.AddListener(this.ChangedScrollPosition);
        }

        private void FillItems()
        {
            var index = 0;
            // ある分はリサイクル
            while (index < this.itemDataList.Count) {
                var listItemData = this.itemDataList[index];
                if (this.ItemCount > listItemData.Position) {
                    this.BuildItemContainer(listItemData.Position);
                } else {
                    break;
                }
                index++;
            }

            // 不足分の追加
            while (this.itemDataList.Count < this.ItemCount) {
                var listItemData = this.BuildItemContainer(this.itemDataList.Count);
                this.itemDataList.Add(listItemData);
            }

            // 余分なものの削除
            if (this.itemDataList.Count > this.ItemCount) {
                while (this.itemDataList.Count > this.ItemCount) {
                    var lastItem = this.itemDataList[this.itemDataList.Count - 1];
                    if (this.visibleItemDataList.Contains(lastItem)) {
                        this.visibleItemDataList.Remove(lastItem);
                    }
                    this.itemDataList.Remove(lastItem);
                    Destroy(lastItem.Container);
                }
            }
        }

        private List<ListItemData> VisibleItems()
        {
            int index = 0;
            if (this.scrollRect.content.rect.height > this.scrollRect.viewport.rect.height) {
                var length = (this.scrollRect.content.rect.size.y - this.scrollRect.viewport.rect.size.y);
                var frameY = length - (length * this.scrollRect.verticalNormalizedPosition);
                index = Mathf.FloorToInt(frameY / this.rowHeight);
            }

            index = Mathf.Max(index, 0);

            int indexLast = index + this.visibleItemCount;
            var items = new List<ListItemData>();
            while (index < indexLast) {
                if (index >= this.itemDataList.Count) {
                    break;
                }
                items.Add(this.itemDataList[index]);
                index++;
            }

            return items;
        }

        private void UpdateListItem(ListItemData listItem)
        {
            GameObject item;
            if (listItem.Item == null) {
                item = this.DataSource.ListViewItem(listItem.Position, null);
                Assert.IsNotNull(item, "ListItem is null!!");
                item.name = "ListItem";
                listItem.SetItemObjcet(item);
                var itemRectTransform = item.GetComponent<RectTransform>();
                itemRectTransform.anchorMin = new Vector2(0, 0);
                itemRectTransform.anchorMax = new Vector2(1, 1);

                var clickHandler = item.GetComponent<ClickHandler>() ?? item.AddComponent<ClickHandler>();
                clickHandler.callback = (gameObject) => {
                    this.TapItem(gameObject);
                };
            } else {
                item = this.DataSource.ListViewItem(listItem.Position, listItem.Item);
            }
            item.SetActive(true);
        }

        private GameObject CreateContainer(string name)
        {
            var itemContainer = new GameObject(name);
            itemContainer.name = name;
            var layoutElement = itemContainer.AddComponent<LayoutElement>();
            itemContainer.transform.SetParent(this.scrollRect.content.gameObject.transform, false);
            layoutElement.preferredHeight = this.RowHeight;
            return itemContainer;
        }

        private ListItemData BuildItemContainer(int position)
        {
            ListItemData listItemData;
            if (this.itemDataList.Count > position) {
                listItemData = this.itemDataList[position];
            } else {
                var newContainer = this.CreateContainer("ItemContainer");
                listItemData = new ListItemData(position, newContainer);
            }
            listItemData.Layout.preferredHeight = this.rowHeight;
            listItemData.ContainerRectTransform.pivot = new Vector2(0, 1);
            return listItemData;
        }

        #endregion

        private class ListItemData
        {
            private int position;
            private GameObject container;
            private RectTransform containerRectTransform;
            private LayoutElement layoutElement;
            private GameObject item;

            public int Position { get { return this.position; } }
            public GameObject Container { get { return this.container; } }
            public RectTransform ContainerRectTransform
            {
                get {
                    if (this.containerRectTransform == null) {
                        this.containerRectTransform = this.container.GetComponent<RectTransform>();
                    }
                    return this.containerRectTransform;
                }
            }
            public LayoutElement Layout { get { return this.layoutElement; } }
            public GameObject Item
            {
                get { return this.item; }
            }

            public ListItemData(int position, GameObject containerGameObject)
            {
                this.position = position;
                this.container = containerGameObject;
                this.layoutElement = containerGameObject.GetComponent<LayoutElement>();
                this.item = null;
            }

            public GameObject PopItem()
            {
                var target = this.item;
                this.item = null;
                return target;
            }

            public void SetItemObjcet(GameObject item)
            {
                this.item = item;
                this.item.transform.SetParent(this.container.transform, false);
                var rectTransform = this.item.GetComponent<RectTransform>();
                rectTransform.offsetMin = new Vector2(0, 0);
                rectTransform.offsetMax = new Vector2(0, 0);
            }
        }

        /// <summary>
        /// EventTriggerでやっちゃうとドラッグイベントも全部もっていっちゃって
        /// スクロールできなくなってしまうので、自分でやーる
        /// </summary>
        private class ClickHandler: MonoBehaviour, IPointerClickHandler
        {
            public System.Action<GameObject> callback;
            public void OnPointerClick(PointerEventData eventData)
            {
                this.callback(this.gameObject);
            }
        }

    }
}
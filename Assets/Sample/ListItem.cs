/*********************************
 リストアイテム
*********************************/
using UnityEngine;
using UnityEngine.UI;

public class ListItem : MonoBehaviour
{
    #region "Serialize Fields"
    [SerializeField] private Image icon;
    [SerializeField] private Text title;
    #endregion

    #region "Public Methods"
    public void Display(ItemData data)
    {
        this.icon.sprite = Resources.Load<Sprite>(data.IconName);
        this.title.text = data.Title;
    }
    #endregion

}
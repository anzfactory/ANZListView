/*********************************
 アイテムデータクラス
*********************************/

public struct ItemData
{
    public string IconName;
    public string Title;
    public ItemData(string iconName, string title)
    {
        this.IconName = iconName;
        this.Title = title;
    }
}
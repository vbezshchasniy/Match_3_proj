using Logic;
using System.Text;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class DataGrid : MonoBehaviour
{
    #region Fields
    public GridItem[,] Items;

    [SerializeField] [Range(7, 10)] private int Xsize;
    [SerializeField] [Range(10, 13)] private int Ysize;
    [SerializeField] private GameObject[] Presets;
    [SerializeField] private float ItemWidth = 1f;
    [SerializeField] private float ItemHeight = 1f;
    [SerializeField] private float MoveDuration = .25f;
    [SerializeField] private float DestroyDuration = .25f;
    [SerializeField] private float TimeForCreation = 1f;
    [SerializeField] private GameObject BackgroundTile;

    private readonly int ItemsForMatch = 3;
    private GridItem SelectedItem;
    private MatchGetter MatchGetter;
    private BalanceParser BalanceParser;
    #endregion

    #region Properties
    #endregion

    #region Methods
    private void Awake()
    {
        MatchGetter = new MatchGetter(ItemsForMatch, this);
        BalanceParser = new BalanceParser(Xsize, Ysize);
    }

    public int GetSizeX()
    {
        return Xsize;
    }

    public int GetSizeY()
    {
        return Ysize;
    }

    private void Start()
    {
        CreateBoard();
        CreateField();
        GridItem.OnMouseOverItemHandler += OnMouseOverGridItem;

        StartCoroutine(CheckField());
    }

    private void CreateNewItems()
    {
        DropDownItems();
    }

    private void DropDownItems()
    {
        bool isEmptyBelow = false;
        int down = 0;
        for (int x = 0; x < Xsize; x++)
        for (int y = 0; y < Ysize; y++)
        {
            down = y + 1;
            while (down < Ysize && Items[x, down].Type == ItemType.Empty)
            {
                isEmptyBelow = true;
                if ((down + 1 < Ysize && Items[x, down + 1].Type == ItemType.Empty))
                {
                    down++;
                }
            }

            if (isEmptyBelow)
            {
                Items[x, y].transform.DOMove(Items[x, down].transform.position, MoveDuration);
                isEmptyBelow = false;
            }
        }
    }

    private void CreateField()
    {
        for (int x = 0; x < Xsize; x++)
        for (int y = 0; y < Ysize; y++)
            Items[x, y] = InstantiateItem(x, y);
    }

    //TODO Refactor VITALIY
    private void CreateBoard()
    {
        Items = new GridItem[Xsize, Ysize];
        GameObject BgTiles = GameObject.Find("BgTiles");
        for (int x = 0; x < Xsize; x++)
        for (int y = 0; y < Ysize; y++)
            CreateBoardBackground(BgTiles, x, y);
    }

    private void CreateBoardBackground(GameObject bgTiles, int x, int y)
    {
        Vector2 bgTilePos = new Vector2(x, y);
        GameObject bgTile = Instantiate(BackgroundTile, bgTilePos, Quaternion.identity);
        bgTile.transform.SetParent(bgTiles.transform);
        bgTile.name = string.Format("BgTile{0}_{1}", x.ToString(), y.ToString());
    }

    private GridItem InstantiateItem(int x, int y)
    {
        int itemIndex = BalanceParser.GetItemIndex(x, y);
        GameObject itemGameObject = FindCorrectPreset(itemIndex);

        Vector2 tilePos = new Vector2(x * ItemWidth, y * ItemHeight);
        GridItem item = Instantiate(itemGameObject, tilePos, Quaternion.identity).GetComponent<GridItem>();
        item.transform.SetParent(this.transform);

        item.name = string.Format("Tile{0}_{1}", x.ToString(), y.ToString());
        item.Type = (ItemType) itemIndex;
        item.OnItemPositionChange(x, y);

        return item;
    }

    private GameObject FindCorrectPreset(int itemIndex)
    {
        foreach (GameObject preset in Presets)
            if (preset.GetComponent<GridItem>().Type == (ItemType) itemIndex)
                return preset;

        return Presets[0];
    }

    private void OnMouseOverGridItem(GridItem item)
    {
        if (SelectedItem == null)
        {
            SelectedItem = item;
        }
        else
        {
            int xDiff = Mathf.Abs(item.X - SelectedItem.X);
            int yDiff = Mathf.Abs(item.Y - SelectedItem.Y);
            if (xDiff + yDiff == 1)
            {
                print("Try match");
                TryMatch(SelectedItem, item);
            }
            else
            {
                print("Error");
            }

            SelectedItem = null;
        }
    }

    private IEnumerator CheckField()
    {
        Debug.Log("AutoMatch");
//        while (true)
//        {
        HashSet<GridItem> autoMatchSet = new HashSet<GridItem>();
        for (int x = 0; x < Xsize; x++)
        {
            for (int y = 0; y < Ysize; y++)
            {
                MatchInfo autoMatchInfo = GetAutoMatchInfo(Items[x, y]);
                List<GridItem> autoMatchInfoList = autoMatchInfo.Matches;
                if (autoMatchInfoList != null)
                {
                    autoMatchSet.UnionWith(autoMatchInfoList);
                }
            }
        }

        if (autoMatchSet.Count > 0)
        {
            DestroyItems(autoMatchSet);
            yield return new WaitForSeconds(DestroyDuration + .01f);

            CreateNewItems();
            yield return new WaitForSeconds(TimeForCreation);
        }
//            else
//            {
//                break;
//            }
//        }

        yield return null;
    }

    //TODO: Block mouse on move period
    private void TryMatch(GridItem selectedItem, GridItem item)
    {
        Debug.Log("Start swap");
        Swap(selectedItem, item);

        MatchInfo matchForSelectedItem = MatchGetter.GetMatchInfo(selectedItem);
        MatchInfo matchForItem = MatchGetter.GetMatchInfo(item);

        if (!matchForSelectedItem.IsValidMatch() && !matchForItem.IsValidMatch())
        {
            Swap(selectedItem, item, true);
        }

        if (matchForSelectedItem.IsValidMatch())
        {
            Debug.Log("Match For Selected Item");
            DestroyItems(matchForSelectedItem.Matches);

            //yield return new WaitForSeconds(DelayBetweenMathes);
            //yield return StartCoroutine(UpdateGridAfterMAtch(matchA));
        }
        else if (matchForItem.IsValidMatch())
        {
            Debug.Log("Match For Item");
            DestroyItems(matchForItem.Matches);

            //yield return new WaitForSeconds(DelayBetweenMathes);
            //yield return StartCoroutine(UpdateGridAfterMAtch(matchB));
        }
    }

    private MatchInfo GetAutoMatchInfo(GridItem gridItem)
    {
        return MatchGetter.GetAutoMatchInfo(gridItem);
    }

    private void DestroyItems(ICollection<GridItem> matches)
    {
        foreach (GridItem item in matches)
        {
            item.transform.DOScale(Vector3.zero, DestroyDuration);
            Items[item.X, item.Y].Type = ItemType.Empty;
            Destroy(item.gameObject, DestroyDuration);
        }
#if DEBUG
        PrintColors();
#endif
    }

    private void Swap(GridItem selectedItem, GridItem item, bool isDelay = false)
    {
        Vector3 selectedItemPosition = selectedItem.transform.position;
        Vector3 itemPosition = item.transform.position;
        float delay = isDelay ? MoveDuration : 0;
        selectedItem.transform.DOMove(itemPosition, MoveDuration).SetDelay(delay);
        item.transform.DOMove(selectedItemPosition, MoveDuration).SetDelay(delay);

        SwapItemsIndex(selectedItem, item);
    }

    private void SwapItemsIndex(GridItem selectedItem, GridItem item)
    {
        //Swap in Items[,]
        GridItem tmp = Items[selectedItem.X, selectedItem.Y];
        Items[selectedItem.X, selectedItem.Y] = Items[item.X, item.Y];
        Items[item.X, item.Y] = tmp;

        //Swap in Scene
        int tmpX = selectedItem.X;
        int tmpY = selectedItem.Y;
        selectedItem.OnItemPositionChange(item.X, item.Y);
        item.OnItemPositionChange(tmpX, tmpY);
    }
    #endregion

    #region Auxilary
    [ContextMenu("Print grid Ints")]
    private void PrintInts()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("\n");
        for (int y = Ysize - 1; y > -1; y--)
        {
            for (int x = 0; x < Xsize; x++)
            {
                sb.Append((int) Items[x, y].Type);
                sb.Append(", ");
            }

            sb.Append("\n");
        }

        Debug.Log(sb);
    }

    [ContextMenu("Print grid Colors")]
    private void PrintColors()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("\n");
        for (int y = Ysize - 1; y > -1; y--)
        {
            for (int x = 0; x < Xsize; x++)
            {
                sb.Append(Items[x, y].Type);
                sb.Append(", ");
            }

            sb.Append("\n");
        }

        Debug.Log(sb);
    }
    #endregion
}
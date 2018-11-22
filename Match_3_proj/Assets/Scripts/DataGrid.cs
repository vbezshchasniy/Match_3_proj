using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class DataGrid : MonoBehaviour
{
    #region Fields

    public GridItem[,] Items;
    
    [SerializeField] private GameObject[] Presets;
    [SerializeField] private float ItemWidth = 1f;
    [SerializeField] private float ItemHeight = 1f;
    [SerializeField] private int Xsize;
    [SerializeField] private int Ysize;
    [SerializeField] private float MoveDuration = .25f;

    private readonly int ItemsForMatch = 3;
    private GridItem SelectedItem;
    private Logic.MatchGetter MatchGetter;

    #endregion

    #region Properties

    #endregion


    #region Methods

    public DataGrid()
    {
        MatchGetter = new Logic.MatchGetter(ItemsForMatch, this);
    }
    
    public int GetSizeX()
    {
        return Xsize;
    }
    
    public int GetSizeY()
    {
        return Ysize;
    }
    
    private void OnEnable()
    {
        CreateField();
        GridItem.OnMouseOverItemHandler += OnMouseOverGridItem;
    }

    private void CreateField()
    {
        Items = new GridItem[Xsize, Ysize];

        for (int x = 0; x < Xsize; x++)
            for (int y = 0; y < Ysize; y++)
                Items[x, y] = InstantiateItem(x, y);
    }

    private GridItem InstantiateItem(int x, int y)
    {
        GameObject itemGameObject = GetRandomItemFromPreset(Presets.Length);
        Vector2 itemPos = new Vector2(x * ItemWidth + ItemWidth / 2, y * ItemHeight + ItemHeight / 2);
        Quaternion itemAngle = Quaternion.identity;
        
        GridItem item = Instantiate(itemGameObject, itemPos, itemAngle).GetComponent<GridItem>();
        item.OnItemPositionChange(x,y);
        return item;
    }

    private GameObject GetRandomItemFromPreset(int presetsLength)
    {
        int randomIndex = Random.Range(0, presetsLength);
        return Presets[randomIndex];
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
                StartCoroutine(TryMatch(SelectedItem, item));
            }
            else
            {
                print("Error");
            }
            SelectedItem = null;
        }
    }

    private IEnumerator TryMatch(GridItem selectedItem, GridItem item)
    {
        yield return StartCoroutine(Swap(selectedItem, item));
        Debug.Log("Start swap");

        Logic.MatchInfo matchForSelectedItem = MatchGetter.GetMatchInfo(selectedItem);
        Logic.MatchInfo matchForItem = MatchGetter.GetMatchInfo(item);
        
        if (!matchForSelectedItem.IsValidMatch() && !matchForItem.IsValidMatch())
        {
            yield return StartCoroutine(Swap(selectedItem, item));
            yield break;
        }
//        if (matchA.validMatch())
//        {
//            Debug.Log("Matche");
//            yield return StartCoroutine(MyDestroy(matchA.match));
//            yield return new WaitForSeconds(DelayBetweenMathes);
//            yield return StartCoroutine(UpdateGridAfterMAtch(matchA));
//
//        }
//        else if (matchB.validMatch())
//        {
//            Debug.Log("No Matches");
//            yield return StartCoroutine(MyDestroy(matchB.match));
//            yield return new WaitForSeconds(DelayBetweenMathes);
//            yield return StartCoroutine(UpdateGridAfterMAtch(matchB));
//        }

    }

    private IEnumerator Swap(GridItem selectedItem, GridItem item)
    {
        Vector3 selectedItemPosition = selectedItem.transform.position;
        Vector3 itemPosition = item.transform.position;
        StartCoroutine(selectedItem.transform.Move(itemPosition, MoveDuration));
        StartCoroutine(item.transform.Move(selectedItemPosition, MoveDuration));
        yield return new WaitForSeconds(MoveDuration);
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
}
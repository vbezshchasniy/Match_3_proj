using System;
using UnityEngine;

public class GridItem : MonoBehaviour
{
    #region Fields

    public ParticleSystem DestroyEffect;
    public delegate void OnMouseOverItem(GridItem item);

    public ItemType Type;

    #endregion

    #region Properties

    public int X { get; private set; }
    public int Y { get; private set; }

    #endregion

    #region Methods

    public void OnItemPositionChange(int x, int y)
    {
        X = x;
        Y = y;
        gameObject.name = string.Format("item_{0}_{1}", X.ToString(), Y.ToString());
    }

    private void OnMouseDown()
    {
        if (OnMouseOverItemHandler != null)
        {
            OnMouseOverItemHandler(this);
        }
    }

    #endregion

    #region Events

    public static OnMouseOverItem OnMouseOverItemHandler;

    #endregion
}
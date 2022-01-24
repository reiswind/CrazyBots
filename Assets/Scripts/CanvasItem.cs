using Engine.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class CanvasItem : MonoBehaviour
{

    public Sprite Normal;
    public Sprite Accept;
    public Sprite Deny;
    internal TileObjectState TileObjectState { get; set; }
    internal TileObjectType TileObjectType { get; set; }
    internal Text Count { get; set; }
    internal GameObject State { get; set; }
    internal GameObject Icon { get; set; }
    internal Button Button { get; set; }

    public void UpdateImage()
    {
        SpriteState spriteState = Button.spriteState;
        if (TileObjectState == TileObjectState.None)
        {
            spriteState.pressedSprite = Normal;
            spriteState.highlightedSprite = Normal;
            spriteState.selectedSprite = Normal;
            Button.image.sprite = Normal;
        }
        if (TileObjectState == TileObjectState.Accept)
        {
            spriteState.pressedSprite = Accept;
            spriteState.highlightedSprite = Accept;
            spriteState.selectedSprite = Accept;
            Button.image.sprite = Accept;
        }
        if (TileObjectState == TileObjectState.Deny)
        {
            spriteState.pressedSprite = Deny;
            spriteState.highlightedSprite = Deny;
            spriteState.selectedSprite = Deny;
            Button.image.sprite = Deny;
            //canvasItem.Button.image.sprite = canvasItem.Deny;
            //image.sprite = canvasItem.Deny;
            //image.overrideSprite = canvasItem.Deny;
        }
        Button.spriteState = spriteState;
    }

    public void SetCount(int count)
    {
        Count.text = count.ToString();
    }
}
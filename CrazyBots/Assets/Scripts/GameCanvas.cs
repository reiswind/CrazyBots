using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
    public GameObject MineralText;
    public GameObject SelectedItemText;
    public HexGrid Game;

    private Text UIMineralText;
    private Text UISelectedItemText;

    // Start is called before the first frame update
    void Start()
    {
        UIMineralText = MineralText.GetComponent<Text>();
        UISelectedItemText = SelectedItemText.GetComponent<Text>();

        Game.StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (Game != null && Game.MapInfo != null)
        {
            UIMineralText.text = Game.MapInfo.TotalMetal.ToString();
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit raycastHit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit, Mathf.Infinity))
            {
                //int x = 0;
                UISelectedItemText.text = raycastHit.collider.name;
            }
            else
            {
                UISelectedItemText.text = "Nothing";
            }
        }
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChampionSelectionPanel : MonoBehaviour
{
    private StartMenuPanelHandler StartMenuPanelHandler;

    //Images
    [SerializeField] Image[] championList;
    [SerializeField] Image HoveredChampionImage;
    [SerializeField] Image captionImage;


    [SerializeField] Text HoveredChampionName;
    [SerializeField] Button select;

    private const int rowCount = 3;
    private const int colCount = 4;
    private int[,] twoDGrid = new int[rowCount, colCount];

    public class Pair
    {
        public int x;
        public int y;
    }

    private Pair currentPosition = new Pair();

    // Start is called before the first frame update
    void Start()
    {
        StartMenuPanelHandler = GetComponentInParent<StartMenuPanelHandler>();

        currentPosition.x = 0;
        currentPosition.y = 0;

        previewCharacter(0);
        HoveredChampionName.text = "Azure Blitz";

        fillArrayWithIndexes();
        disableAllOutline();

        glow();

        select.onClick.AddListener(CharacterSelected);
    }

    public void CharacterSelected()
    {
        StartMenuPanelHandler.StartMenuPanel.gameObject.SetActive(true);
        StartMenuPanelHandler.StartMenuPanel.SetChampionSelectionIndex(getIndex());
        captionImage.GetComponent<Image>().sprite = championList[getIndex()].GetComponent<Image>().sprite;
        this.gameObject.SetActive(false);
    }


    private void fillArrayWithIndexes()
    {
        int imageCount = 0;
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                if (imageCount >= championList.Length)
                {
                    twoDGrid[i, j] = -1; //The area of the grid not occupied by buttons
                }
                else
                {
                    twoDGrid[i, j] = imageCount;
                }
                imageCount++;
            }
        }
    }

    void previewCharacter(int index)
    {
        if (index >= 0 && index < championList.Length)
        {
            HoveredChampionImage.GetComponent<Image>().sprite = championList[index].GetComponent<Image>().sprite;
            HoveredChampionName.text = championList[index].name;
        }
    }

    public void glow()
    {
        championList[getIndex()].GetComponent<Outline>().enabled = true;
    }

    public void moveSelected()
    {
        Pair prev = new Pair();
        prev.x = currentPosition.x;
        prev.y = currentPosition.y;

        bool moved = false;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentPosition.x > 0)
            {
                if (twoDGrid[currentPosition.x - 1, currentPosition.y] != -1)
                {
                    currentPosition.x--;
                    moved = true;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentPosition.x < rowCount - 1)
            {
                if (twoDGrid[currentPosition.x + 1, currentPosition.y] != -1)
                {
                    currentPosition.x++;
                    moved = true;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentPosition.y > 0)
            {
                if (twoDGrid[currentPosition.x, currentPosition.y - 1] != -1)
                {
                    currentPosition.y--;
                    moved = true;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentPosition.y < colCount - 1)
            {
                if (twoDGrid[currentPosition.x, currentPosition.y + 1] != -1)
                {
                    currentPosition.y++;
                    moved = true;
                }
            }
        }

        if (moved)
        {
            disablePreviousOutline(prev);
        }

    }

    // Update is called once per frame
    void Update()
    {
        moveSelected();
        glow();
        previewCharacter(getIndex());
    }

    int getIndex()
    {
        return twoDGrid[currentPosition.x, currentPosition.y];
    }

    void disableAllOutline()
    {
        for (int i = 0; i < championList.Length; i++)
        {
            championList[i].GetComponent<Outline>().enabled = false;
        }
    }

    void disablePreviousOutline(Pair prev)
    {
        championList[twoDGrid[prev.x, prev.y]].GetComponent<Outline>().enabled = false;
    }
}

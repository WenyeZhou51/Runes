using System.Collections.Generic;
using UnityEngine;

public class CardsManager : MonoBehaviour
{
    List<Weapon> playerWeapons;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject cell;
    private float cellsize;
    private RectTransform cmRect;
    private GameObject slot;
    public Vector2 offset;
    public GameObject panel;
    private RectTransform panelRect;
    private Camera mainCamera;
    private Canvas myCanvas;
    public float h_shift = -12;
    public float h_gap = 1.25f;
    public float v_shift = 14;

    private void Awake()
    {
        cmRect = GetComponent<RectTransform>();
        panelRect = panel.GetComponent<RectTransform>();
        myCanvas = FindObjectOfType<Canvas>();
        mainCamera = Camera.main;
        cmRect.localScale = Vector3.one;
    }

    private void Start()
    {


        cellsize = cell.GetComponent<RectTransform>().sizeDelta.x;

        PlayerController playerController = player.GetComponent<PlayerController>();
        playerWeapons = playerController.getWeapons();
        myCanvas = FindObjectOfType<Canvas>();

        if (playerWeapons.Count > 0)
        {

            displayCells(playerWeapons[0]);
        }

    }

    public void displayCells(Weapon weapon)
    {
        Cell[] toDestroy = FindObjectsOfType<Cell>();
        GenericCard[] toDestroy2 = FindObjectsOfType<GenericCard>();
        foreach (Cell cell in toDestroy)
        {
            Destroy(cell.gameObject);
            
        }


        for (int i = 0; i < weapon.size; i++)
        {
            slot = Instantiate(cell, cmRect);
            slot.GetComponent<Cell>().containedCard = weapon.cards[i];
            slot.GetComponent<Cell>().cellIndex = i;
            slot.GetComponent<Cell>().parentWeapon = weapon;
            slot.GetComponent<Cell>().manager = this;
            RectTransform cellRect = slot.GetComponent<RectTransform>();
            cellRect.anchoredPosition = new Vector2(i * (cellsize * h_gap) + h_shift, v_shift) + offset;
        }
    }
}

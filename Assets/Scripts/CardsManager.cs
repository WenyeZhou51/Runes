using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UIElements;

public class CardsManager : MonoBehaviour
{
    // Start is called before the first frame update
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
    public float threshold;
    private void Awake()
    {
        cmRect = GetComponent<RectTransform>();
        panelRect = panel.GetComponent<RectTransform>();
        mainCamera = Camera.main;
        threshold = 420f;
        
    }
    private void Start()
    {
        if (cell == null || player == null)
        {
            return;
        }

        cellsize = cell.GetComponent<RectTransform>().sizeDelta.x;

        PlayerController playerController = player.GetComponent<PlayerController>();
        playerWeapons = playerController.getWeapons();




        if (playerWeapons.Count > 0)
        {
            StartCoroutine(DelayedDisplay());
        }
        else
        {
            Debug.LogWarning("No weapons found for the player.");
        }

    }


    IEnumerator DelayedDisplay()
    {
        yield return new WaitForSecondsRealtime(0.0001f);
        displayCells(playerWeapons[0]);
    }

    public void displayCells(Weapon weapon) {


        Cell[] toDestroy = FindObjectsOfType<Cell>();
        GenericCard[] toDestroy2 = FindObjectsOfType<GenericCard>();
        foreach (Cell cell in toDestroy) {
                Destroy(cell.gameObject);
            
        }
        foreach (GenericCard genericCard in toDestroy2)
        {
            Vector3 ScreenPoint = mainCamera.WorldToScreenPoint(genericCard.transform.position);
            if (ScreenPoint.y > threshold)
            {
                Destroy(genericCard.gameObject);
            }
        }
        

        for (int i = 0; i < weapon.size; i++)
        {
            slot = Instantiate(cell, cmRect);
            slot.GetComponent<Cell>().containedCard = weapon.cards[i];
            slot.GetComponent<Cell>().cellIndex = i;
            slot.GetComponent<Cell>().parentWeapon = weapon;
            slot.GetComponent<Cell>().manager = this;
            RectTransform cellRect = slot.GetComponent<RectTransform>();
            cellRect.anchoredPosition = new Vector2(i * (cellsize*1.2f), 0)+offset;
        }
    }

}

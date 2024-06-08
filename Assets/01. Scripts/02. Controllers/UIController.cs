using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("Scene-Sauna")]
    [SerializeField] public GameObject main;
    [SerializeField] public GameObject collectingBook;
    [SerializeField] public GameObject ProfileList;

    private void Awake()
    {
    }

    void Start()
    {
    }

    public void EnableUI(GameObject ui)
    {
        ui.SetActive(true);
    }

    public void DisableUI(GameObject ui)
    {
        ui.SetActive(false);
    }

    public void SwitchUI(GameObject ui)
    {
        ui.SetActive(!ui.activeSelf);
    }
}

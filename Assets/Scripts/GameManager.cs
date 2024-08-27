using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    [SerializeField] private ZombieController zombieController;

    [SerializeField] private RectTransform healthBar;

    private float width, height;
    
    // Start is called before the first frame update
    void Start()
    {
        height = healthBar.sizeDelta.y;
        width = healthBar.sizeDelta.x;
        var newWidth = (float)playerController.Health / (float)playerController.MaxHealth * width;
        healthBar.sizeDelta = new Vector2(newWidth, height);
    }

    // Update is called once per frame
    void Update()
    {
        var newWidth = (float)playerController.Health / (float)playerController.MaxHealth * width;
        healthBar.sizeDelta = new Vector2(newWidth, height);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Number : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] SpriteRenderer[] currentNumber;

    public void OnDisplayNumber(int value)
    {
        if(value > 9)
        {
            currentNumber[1].sprite = sprites[(int)value % 10];
            currentNumber[0].sprite = sprites[(int)value / 10];
            transform.position = new Vector2(transform.position.x - 4, transform.position.y);
        }
        else
        {
            currentNumber[0].sprite = sprites[value];
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickObject : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null)
            {
                GameObject GO = hit.collider.gameObject;
                CheckForEnemy(GO);
            }
        }
    }

    private void CheckForEnemy(GameObject GO)
    {
        Enemy_FlyingEye enemy = GO.GetComponent<Enemy_FlyingEye>();

        if (enemy)
            enemy.isSelected = true;
    }
}

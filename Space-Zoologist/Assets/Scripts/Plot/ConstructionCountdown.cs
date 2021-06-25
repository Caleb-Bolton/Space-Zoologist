using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionCountdown : MonoBehaviour
{
    public Vector2Int position;
    public int progress;
    public int target;
    private Material material;
    public void Initialize(Vector2Int pos, int time, int progress = -1)
    {
        this.position = pos;
        this.progress = progress == -1 ? 0 : progress;
        this.target = time;
        this.material = this.gameObject.GetComponent<SpriteRenderer>().material;
        this.material.SetFloat("_Target", this.target + 1); //Add 1 to enhance graphical representation
    }
    public void CountDown()
    {
        this.progress += 1;
        this.material.SetFloat("_Progress", this.progress + 1);//Add 1 to enhance graphical representation
        // TODO animate material
    }
    public bool IsFinished()
    {
        if (progress >= target)
        {
            return true;
        }
        return false;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class BuildBufferManager : GridObjectManager
{
    [SerializeField] private GameObject bufferGO;
    private Dictionary<Vector4, List<ConstructionCountdown>> colorTimesToCCs = new Dictionary<Vector4, List<ConstructionCountdown>>();// For serialization
    private bool[,] isConstructing;
    private ReservePartitionManager RPM;
    public bool IsConstructing(int x, int y) => isConstructing[x,y];
    private void Awake()
    {
        LevelDataReference levelDataReference = FindObjectOfType<LevelDataReference>();
        if (levelDataReference == null)
        {
            Debug.LogWarning("Level data reference not found, using default width and height values");
            this.isConstructing = new bool[100, 100];
        }
        int w = levelDataReference.LevelData.MapWidth;
        int h = levelDataReference.LevelData.MapHeight;
        this.isConstructing = new bool[w, h];
        this.RPM = FindObjectOfType<ReservePartitionManager>();
    }
    public override void Parse()
    {
        foreach (KeyValuePair<string, GridItemSet> keyValuePair in SerializedMapObjects)
        {
            if (keyValuePair.Key.Equals(this.MapObjectName))
            {
                Vector3[] coords = SerializationUtils.ParseVector3(keyValuePair.Value.coords);
                Vector4 v4 = ParseVector4(keyValuePair.Value.name);
                foreach (Vector3 coord in coords)
                {
                    Vector2Int pos = new Vector2Int((int)coord.x, (int)coord.y);
                    this.CreateUnitBuffer(pos, (int)coord.z, new Color(v4.x,v4.y,v4.z), (int)v4.w);
                }
            }
        }
    }
    public override void Serialize(SerializedMapObjects serializedMapObjects)
    {
        foreach (KeyValuePair<Vector4, List<ConstructionCountdown>> keyValuePair in this.colorTimesToCCs)
        {
            serializedMapObjects.AddType(this.MapObjectName, new GridItemSet(this.SerializeColorAndTotal(keyValuePair.Key), this.GetPositionsAndTimes(keyValuePair.Value)));
        }              
    }
    public void CreateUnitBuffer(Vector2Int pos, int time, Color color, int progress = -1)
    {
        if (time == 0 || time == -1)
        {
            return;
        }
        this.isConstructing[pos.x, pos.y] = true;
        GameObject newGo = Instantiate(this.bufferGO, this.gameObject.transform);
        Debug.Log("Placing item under constuction");
        newGo.transform.position = new Vector3(pos.x, pos.y, 0);
        color.a = 1; //Enforce alpha channel to be 1, prevent human error

        newGo.GetComponent<SpriteRenderer>().color = color;
        ConstructionCountdown cc = newGo.GetComponent<ConstructionCountdown>();
        cc.Initialize(pos, time, progress);
        Vector4 colorTimePair = new Vector4(color.r, color.g, color.b, time);
        if (!this.colorTimesToCCs.ContainsKey(colorTimePair))
        {
            this.colorTimesToCCs.Add(colorTimePair, new List<ConstructionCountdown>());
        }
        this.colorTimesToCCs[colorTimePair].Add(cc);

    }
    public void CreateSquareBuffer(Vector2Int pos, int time, int size, Color color, int progress = -1)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                this.CreateUnitBuffer(new Vector2Int(pos.x + i, pos.y + j), time, color, progress);
            }

        }
    }
    public ConstructionCountdown GetBuffer(Vector2Int pos)
    {
        if (!this.isConstructing[pos.x, pos.y])
        {
            return null;
        }
        Vector4[] av4 = this.colorTimesToCCs.Keys.ToArray();
        List<Vector3Int> changedTiles = new List<Vector3Int>();
        List<ConstructionCountdown>[] aCCs = this.colorTimesToCCs.Values.ToArray();
        for (int i = this.colorTimesToCCs.Values.Count - 1; i >= 0; i--)
        {
            List<ConstructionCountdown> ccs = aCCs[i];
            for (int j = ccs.Count - 1; j >= 0; j--)
            {
                ConstructionCountdown cc = ccs[j];
                if (pos == cc.position)
                {
                    return cc;
                }
            }
        }
        return null;
    }
    public void DestoryBuffer(Vector2Int pos, int size = 1)
    {
        if (!this.isConstructing[pos.x, pos.y])
        {
            Debug.Log("No construction buffer to remove, proceeding");
            return;
        }
        HashSet<Vector2Int> bufferToRemove = new HashSet<Vector2Int>();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                bufferToRemove.Add(new Vector2Int(pos.x + i, pos.y + j));
            }

        }
        Vector4[] av4 = this.colorTimesToCCs.Keys.ToArray();
        List<Vector3Int> changedTiles = new List<Vector3Int>();
        List<ConstructionCountdown>[] aCCs = this.colorTimesToCCs.Values.ToArray();
        for (int i = this.colorTimesToCCs.Values.Count - 1; i >= 0; i--)
        {
            List<ConstructionCountdown> ccs = aCCs[i];
            for (int j = ccs.Count - 1; j >= 0; j--)
            {
                ConstructionCountdown cc = ccs[j];
                if (bufferToRemove.Contains(cc.position))
                {
                    this.isConstructing[(int)cc.position.x, (int)cc.position.y] = false;
                    ccs.RemoveAt(j);
                    changedTiles.Add(new Vector3Int(cc.position.x, cc.position.y, 0));
                    Destroy(cc.gameObject);
                }
            }
            if (ccs.Count == 0)
            {
                this.colorTimesToCCs.Remove(av4[i]);
            }
        }
        //Report updates to RPM
        if (changedTiles.Count > 0)
        {
            this.RPM.UpdateAccessMapChangedAt(changedTiles);
        }
    }
    public void CountDown()
    {
        Vector4[] av4 = this.colorTimesToCCs.Keys.ToArray();
        List<Vector3Int> changedTiles = new List<Vector3Int>();
        List<ConstructionCountdown>[] aCCs = this.colorTimesToCCs.Values.ToArray();
        for (int i = this.colorTimesToCCs.Values.Count - 1; i >= 0; i--)
        {
            List<ConstructionCountdown> ccs = aCCs[i];
            for (int j = ccs.Count - 1; j >= 0; j--)
            {
                ConstructionCountdown cc = ccs[j];
                cc.CountDown();
                if (cc.IsFinished())
                {
                    this.isConstructing[(int)cc.position.x, (int)cc.position.y] = false;
                    ccs.RemoveAt(j);
                    changedTiles.Add(new Vector3Int(cc.position.x, cc.position.y, 0));
                    Destroy(cc.gameObject);
                }
            }
            if (ccs.Count == 0)
            {
                this.colorTimesToCCs.Remove(av4[i]);
            }
        }
        //Report updates to RPM
        if (changedTiles.Count > 0)
        {
            this.RPM.UpdateAccessMapChangedAt(changedTiles);
        }
    }
    private Vector3[] GetPositionsAndTimes(List<ConstructionCountdown> cCs)
    {
        Vector3[] positions = new Vector3[cCs.Count];
        for (int i=0;i< cCs.Count;i++)
        {
            Vector2 pos = cCs[i].position;
            positions[i] = new Vector3(pos.x, pos.y, cCs[i].progress);
        }
        return positions;
    }
    private string SerializeColorAndTotal(Vector4 v4)
    {
        return v4.x.ToString() + "," + v4.y.ToString() + "," + v4.z.ToString() + "," + v4.w.ToString();
    }
    private Vector4 ParseVector4(string s)
    {
        string[] channels = s.Split(',');
        return new Vector4(float.Parse(channels[0]), float.Parse(channels[1]), float.Parse(channels[2]), float.Parse(channels[3]));
    }
    protected override string GetMapObjectName()
    {
        return "buildBuffer";
    }
}

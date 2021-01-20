using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveArrow : MonoBehaviour
{
    [System.Serializable]
    public class ObjectiveItem {
        public Transform transform; // used for where the arrow points to
        public string objectiveName = ""; // used to display an identifier with the arrow
        public string objectiveText = ""; // used for the checklist
        [HideInInspector, SerializeField]  public float timer = 0;
        public float timeOut = 0; // used to set a timer when the objective should be displayed
        GameObject arrow; // the instantiated arrow object with this objective
        public GameObject Arrow {
            get { return arrow; }
            set { arrow = value; }
        }
        public bool enabled = false; // is this arrow enabled?
        public bool reached = false; // is the objective reached?
    }
    public Bounds screenBounds = new Bounds(new Vector3(0f,0f,10f),new Vector3(0.4f,0.4f,0.1f));
    public GameObject pointerObjectPrefab;
    public Sprite crossSprite;
    public Sprite arrowSprite;
    public Text objectiveListTextObject;
    public List<ObjectiveItem> objectiveItems;
    public bool enableArrows = true;
    private Camera Cam;
    private List<int> objectiveList;
    void Awake()
    {
        objectiveList = new List<int>();
        if(Cam == null)
        {
            Cam = GetComponent<Camera>();
        }
        for(int index = 0; index < objectiveItems.Count; index++)
        {
            objectiveItems[index].Arrow = Instantiate(pointerObjectPrefab,Cam.transform);
            objectiveItems[index].Arrow.GetComponentInChildren<TextMesh>().text = objectiveItems[index].objectiveName;
            if(objectiveItems[index].enabled == true)
            {
                if(objectiveList.Contains(index) == false)
                {
                    objectiveList.Add(index);
                }
            }
        }
        setObjectiveList();
    }
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.O))
        {
            enableArrows = !enableArrows;
        }
        for(int index = 0; index < objectiveItems.Count; index++)
        {
            if(objectiveItems[index].enabled == true)
            {
                if(objectiveList.Contains(index) == false)
                {
                    objectiveList.Add(index);
                    setObjectiveList();//ensure that offline enabled objectives are shown in the objectivelist
                }
            }
            else
            {
                if(objectiveList.Contains(index) == true)
                {
                    objectiveList.Remove(index);
                    setObjectiveList();//ensure that offline enabled objectives are shown in the objectivelist
                }
            }
            if(objectiveItems[index].enabled == true && objectiveItems[index].reached == false && enableArrows == true)
            {
                Vector3 target = Cam.WorldToViewportPoint(objectiveItems[index].transform.position);
                //keep arrows hidden until timeout
                if(objectiveItems[index].timer < objectiveItems[index].timeOut)
                {
                    objectiveItems[index].timer += Time.deltaTime;
                    objectiveItems[index].Arrow.SetActive(false);
                    continue;
                }
                if( target.x > screenBounds.min.x && 
                    target.x < screenBounds.max.x && 
                    target.y > screenBounds.min.y && 
                    target.y < screenBounds.max.y )//if target is in screen show cross
                {                    
                    if(crossSprite != null)
                    {
                        Vector3 posInScreen = target;
                        posInScreen.z = screenBounds.center.z;//to ensure it is on the bounds-z-axis

                        objectiveItems[index].Arrow.transform.GetChild(0).up = Vector3.zero;
                        objectiveItems[index].Arrow.transform.position = Cam.ViewportToWorldPoint(posInScreen); 

                        objectiveItems[index].Arrow.GetComponentInChildren<SpriteRenderer>().sprite = crossSprite;
                        objectiveItems[index].Arrow.SetActive(true);
                    }
                    else
                    {
                        objectiveItems[index].Arrow.SetActive(false);
                    }
                }
                else // else show arrow
                {
                    Vector3 posInScreen = screenBounds.ClosestPoint( target );
                    Vector3 dir = (objectiveItems[index].transform.position - Cam.transform.position).normalized;
                    dir.z = 0;//this ensures the arrow always faces the camera
                    objectiveItems[index].Arrow.transform.GetChild(0).up = dir; //sprite is a child, and will rotate, as opposed to the text with it
                    objectiveItems[index].Arrow.transform.position = Cam.ViewportToWorldPoint(posInScreen); 
                    objectiveItems[index].Arrow.GetComponentInChildren<SpriteRenderer>().sprite = arrowSprite;
                    objectiveItems[index].Arrow.SetActive(true);
                }
            }
            else
            {
                objectiveItems[index].Arrow.SetActive(false);
            }
        }
    }
    public int getIndex(Transform transform)
    {
        for(int index = 0; index < objectiveItems.Count; index++)
        {
            if(objectiveItems[index].transform == transform)
            {
               return index;
            }
        }
        return -1;
    }

    public int getIndex(string name)
    {
        for(int index = 0; index < objectiveItems.Count; index++)
        {
            if(objectiveItems[index].objectiveName == name)
            {
               return index;
            }
        }
        return -1;
    }

    public void EnableObjective(int index)
    {
        if(index < 0 || index >= objectiveItems.Count) {
            return;
        }
        objectiveItems[index].enabled = true;
        if(objectiveList.Contains(index) == false)
        {
            objectiveList.Add(index);
        }
        setObjectiveList();
    }

    public void DisableObjective(int index)
    {
        if(index < 0 || index >= objectiveItems.Count) {
            return;
        }
        objectiveItems[index].timer = 0;
        objectiveItems[index].enabled = false;
        if(objectiveList.Contains(index) == true)
        {
            objectiveList.Remove(index);
        }
        setObjectiveList();
    }

    public void ReachedObjective(int index)
    {
        if(index < 0 || index >= objectiveItems.Count) {
            return;
        }
        objectiveItems[index].timer = 0;
        objectiveItems[index].enabled = false;
        objectiveItems[index].reached = true;
        setObjectiveList();
    }
    void setObjectiveList()
    {
        objectiveListTextObject.text = "Objectives:\n";
        foreach (int i in objectiveList)
        {
            if(objectiveItems[i].objectiveText != "") // we want an objective
            {
                if(objectiveItems[i].reached == true) // if it was met
                {
                    objectiveListTextObject.text += "[*] " + objectiveItems[i].objectiveText + "\n";
                }
                else if(objectiveItems[i].enabled == true) // if it was told, but not met
                {
                    objectiveListTextObject.text += "[  ] " + objectiveItems[i].objectiveText + "\n";
                }
            }
        }
    }
}

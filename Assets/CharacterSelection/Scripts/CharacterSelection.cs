using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class Character
{
    public GameObject obj;
    public string name;
    public string description;
    public float price;
    public bool opened = false;
}

public class CharacterSelection: MonoBehaviour {

    [Header("Characters carousel")]
    public Character[] characters;
    public int startIndex = 0;
    public float zAxis = 10;
    [Tooltip("Distance between characters")]
    public float xOffset = 1;
    public Vector3 startObjectScale = Vector3.one;
    public Vector3 zoomObjectScale = Vector3.one * 2;
    public float zoomSpeed = 10;

    private Character selectedCharacter;
    private Material[] materials;

    [Header("UI Settings")]
    public GameObject rootUI;
    public Button buyUIButton;
    public Text charNameUIText;
    public Material closeMaterial;
    private Text priceText;

    public string pricePrefix = "";
    public string pricePostfix = "$";
    public string openedText = "Select";

    private int current = 0;
    private Vector3 mouseOriginX = Vector3.zero;
    private Vector3 mouseOffsetX;
    private GameObject root;
    private Vector3 v = Vector3.zero;
    private bool isScaling = false;
    private bool isSelectorOn = false;

    private CSCallback callback;

	void Awake () {
        Init();
	}

    void Init()
    {
        callback = GetComponent<CSCallback>();

        if(GameObject.Find("CharacterSelectorRoot") == null)
        {
            root = new GameObject();
            root.name = "CharacterSelectorRoot";
            root.transform.parent = Camera.main.transform;
        }

        materials = new Material[characters.Length];

        int index = 0;
        foreach (Character _char in characters)
        {
            _char.obj.SetActive(true);
            _char.obj.transform.parent = root.transform;
            _char.obj.transform.localPosition = new Vector3(index * xOffset, 0, 0);
            _char.obj.transform.localScale = startObjectScale;
            materials[index] = _char.obj.GetComponent<Renderer>().material;

            if(_char.opened == false)
            {
                _char.obj.GetComponent<Renderer>().material = closeMaterial;
            }

            index++;
        }

        root.SetActive(false);

        priceText = buyUIButton.GetComponentInChildren<Text>();

        if (callback)
        {
            callback.OnInit();
        }

    }

    public bool OnSwipePanel()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 1;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        var pointer = new PointerEventData(EventSystem.current);

        pointer.position = Camera.main.WorldToScreenPoint(mouseWorldPos);
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);
        if (raycastResults.Count > 0)
        {
            return raycastResults[0].gameObject.CompareTag("SwipePanel");
        }

        return false;
    }

    public void GoToCharacter(int index)
    {
        root.transform.localPosition = new Vector3(index * xOffset, 0, zAxis);
        ChangeCurrentIndex(index, true);
        isScaling = true;
    }

	void Update () {
        SwipeLoop();
    }

    void SwipeLoop()
    {
        if (Input.GetMouseButtonDown(0) && OnSwipePanel())
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 1;
            mouseOriginX = Camera.main.ScreenToWorldPoint(mousePos);

            v.x = root.transform.localPosition.x;

        }
        else
        {
            root.transform.localPosition = Vector3.Lerp(root.transform.localPosition,
                new Vector3(current * xOffset * -1, 0, zAxis), zoomSpeed * Time.deltaTime);

        }

        if (Input.GetMouseButton(0) && mouseOriginX != Vector3.zero)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 1;
            mouseOffsetX = new Vector3(mouseOriginX.x - Camera.main.ScreenToWorldPoint(mousePos).x, 0, zAxis);

            float clamp = Mathf.Clamp(v.x - mouseOffsetX.x, -characters.Length * xOffset + xOffset, 0);
            Vector3 vec = new Vector3(clamp, 0, zAxis);
            root.transform.localPosition = vec;

            int index = 0;
            isScaling = true;
            int current = Mathf.Abs(Mathf.RoundToInt(root.transform.position.x / xOffset));
            float off = root.transform.position.x / xOffset;
            ChangeCurrentIndex(current);

        }

        if (Input.GetMouseButtonUp(0))
        {
            v.x = root.transform.localPosition.x;
            mouseOriginX = Vector3.zero;
        }

        if (isScaling)
        {
            characters[current].obj.transform.localScale = Vector3.Lerp(characters[current].obj.transform.localScale, zoomObjectScale, 5 * Time.deltaTime);
            if (current > 0)
                characters[current - 1].obj.transform.localScale = startObjectScale;
            if (current < characters.Length - 1)
                characters[current + 1].obj.transform.localScale = startObjectScale;

            if (characters[current].obj.transform.localScale == Vector3.one * 2)
                isScaling = false;
        }
    }

    public void ShowSelector()
    {
        v.x = current * xOffset * -1;
        
        if (callback)
        {
            callback.OnShowSelector();
        }

        isSelectorOn = true;
        root.SetActive(true);
        rootUI.SetActive(true);
        selectedCharacter = characters[startIndex];
        GoToCharacter(startIndex);
    }

    public void CloseSelector()
    {
        if (callback)
        {
            callback.OnCloseSelector();
        }

        isSelectorOn = false;
        root.SetActive(false);
        rootUI.SetActive(false);
    }

    void ChangeCurrentIndex(int index, bool isForce = false)
    {
        if(index != current || isForce)
        {
            current = index;

            if (characters[index] == selectedCharacter)
            {
                buyUIButton.interactable = false;
            }
            else
            {
                buyUIButton.interactable = true;
            }


            if (characters[index].opened)
            {
                characters[index].obj.GetComponent<Renderer>().material = materials[index];
                priceText.text = openedText;
            }
            else
            {
                characters[index].obj.GetComponent<Renderer>().material = closeMaterial;
                priceText.text = pricePrefix + characters[index].price + pricePostfix;
            }


            charNameUIText.text = characters[index].name;

            if (callback)
            {
                callback.OnSwipe(index, characters[index]);
            }
        }
    }

    public void ClickToSelectCharacter()
    {
        if (callback)
        {
            callback.OnClickToSelectCharacter(current, characters[current]);
        }
        else
        {
            Debug.LogWarning("Need CSCallback script to open character");
        }
    }

    public void OpenCharacter(int index)
    {
        characters[index].opened = true;
        characters[index].obj.GetComponent<Renderer>().material = materials[index];
        priceText.text = openedText;
    }

    public void SelectCharacter(int index)
    {
        selectedCharacter = characters[index];
        startIndex = index;
        buyUIButton.interactable = false;
    }

    public Character GetSelectedCharacter()
    {
        return selectedCharacter;
    }

}

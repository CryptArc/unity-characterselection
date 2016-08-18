using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterSelection))]
public class CSCallback : MonoBehaviour {

    CharacterSelection characterSelection;
    public GameObject currentCharacterParent;

    public void OnInit()
    {
        characterSelection = GetComponent<CharacterSelection>();
        if (!characterSelection)
        {
            Debug.LogError("Character Selection component missed!");
        }

        GameObject current = Instantiate(characterSelection.characters[characterSelection.startIndex].obj);
        current.transform.parent = currentCharacterParent.transform;
        current.transform.localPosition = Vector3.zero;
        current.SetActive(true);
            
        Debug.Log("(CSCallback) " + "OnInit: Init Success!");
    }

    public void OnShowSelector()
    {
        currentCharacterParent.SetActive(false);
        Debug.Log("(CSCallback) " + "OnShowSelector: Selector Showed!");
    }

    public void OnCloseSelector()
    {
        currentCharacterParent.SetActive(true);
        Debug.Log("(CSCallback) " + "OnCloseSelector: Selector Closed!");
    }

    public void OnSwipe(int index, Character character)
    {
        Debug.Log("(CSCallback) " + "OnSwipe: index = " + index + " Character name = " + character.name);
    }

    public void OnClickToSelectCharacter(int index, Character character)
    {
        if (!character.opened)
        {
            //  If the character is not purchased, buy it
            characterSelection.OpenCharacter(index);

            Debug.Log("(CSCallback) " + "OnPurchase: index = " + index + " Character name = " + character.name);
        }
        else
        {
            //  If the character is purchased, select it
            characterSelection.SelectCharacter(index);

            // set current character object
            Destroy(currentCharacterParent.transform.GetChild(0).gameObject);
            GameObject current = Instantiate(character.obj);
            current.transform.parent = currentCharacterParent.transform;
            current.transform.localPosition = Vector3.zero;

            Debug.Log("(CSCallback) " + "OnSelect: index = " + index + " Character name = " + character.name);

        }
        
    }

}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using UnityEngine.UIElements;
using UnityEngine.Reflect;

//using DentedPixel;

public class SlidingMenu : MonoBehaviour
{
    // Animation
    private float startPosition, endPosition;
    private const int AnimationDurationMs = 1000;

    // UI
    private Button but;
    private VisualElement main, arrowContainer, mm;

    // Start is called before the first frame update
    void OnEnable()
    {
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("USS/room-inspector"));
        main = rootVisualElement.Q<VisualElement>("mainB");
        main.style.right = -120.0f; // For some reason I have to set it here first, instead of simply read it from uxml..
        main.style.width = 150.0f; // For some reason I have to set it here first, instead of simply read it from uxml..
        arrowContainer = rootVisualElement.Q<VisualElement>("arrow-container");
        mm = rootVisualElement.Q<VisualElement>("moving-menu");
        but = rootVisualElement.Q<Button>("show");
        but.RegisterCallback<ClickEvent>(ev => Animate());
    }

    /// <summary>
    /// Fills the sliding menu with the list of room names, and register adequate callbacks.
    /// </summary>
    /// <param name="roomList">A list of string containing the names of all the rooms.</param>
    public void PopulateMenu(List<string> roomList)
    {
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        var perRoomScript = GameObject.Find("PerRoomListMenu").GetComponent<PerRoomListMenu>();
        VisualElement mm = rootVisualElement.Q<VisualElement>("moving-menu");
        var fao = GameObject.Find("Root").GetComponent<FindAllObjects>();

        for (int i = 0; i < roomList.Count; i++)
        {
            Button newButton = new Button();
            newButton.text = roomList[i];
            newButton.RegisterCallback<ClickEvent>(ev => fao.GoToLocation(ev.target as Button));
            newButton.RegisterCallback<ClickEvent>(ev => perRoomScript.RefreshMenu());
            newButton.RegisterCallback<ClickEvent>(ev => perRoomScript.PopulateMenu(ev.target as Button));
            newButton.AddToClassList("room-notdone");
            mm.Add(newButton);
        }
    }

    private void Animate()
    {
        // Check which rooms are validated or not
        var web = GameObject.Find("Root").GetComponent<Web>();
        StartCoroutine(web.GetAllSurfacesValidities(RestyleMenu));

        // see https://github.com/Unity-Technologies/UIToolkitUnityRoyaleRuntimeDemo/blob/7f5d60d438f46a437dfed54dcbfc6ceb15eb02de/Assets/Scripts/UI/EndScreen.cs#L79
        // Get Starting position
        startPosition = main.style.right.value.value;
        float diff = main.worldBound.width - arrowContainer.worldBound.width;
        endPosition = -(diff + startPosition) ;

        main.experimental.animation.Start(new StyleValues { right = startPosition, opacity = 1 }, new StyleValues { right = endPosition, opacity = 1 }, AnimationDurationMs).Ease(Easing.OutQuad);
    }

    /// <summary>
    /// Color the items of this menu accordingly to their validated or not status.
    /// It's passed as argument to GetAllRoomsValidities so that it gets executed once the list of rooms is obtained.
    /// </summary>
    private void RestyleMenu(Dictionary<int, Tuple<bool, string>> surfacesValidityDict)
    {
        // Update lists of rooms and surfaces validities
        var fao = GameObject.Find("Root").GetComponent<FindAllObjects>();
        fao.UpdateSurfacesAndRoomsValiditiesDict(surfacesValidityDict);

        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        foreach (KeyValuePair<string, bool> entry in fao.roomValidities)
        {
            Button b = rootVisualElement.Q<Button>(entry.Key);
            if (b != null)
            {
                b.RemoveFromClassList("room-done");
                b.RemoveFromClassList("room-notdone");
                if (entry.Value)
                {
                    b.AddToClassList("room-done");
                }
                else
                {
                    b.AddToClassList("room-notdone");
                }
            }
        }

        /*
        foreach (KeyValuePair<string, List<int>> entry in fao.AsurfacesPerRoom)
        {
            string currentRoom = entry.Key;
            int isRoomValid = 1;
            foreach (int surfId in entry.Value)
            {
                if (surfacesValidityDict[surfId.ToString()] == 0)
                {
                    isRoomValid = 0;
                    break;
                }
            }
            fao.roomValidities.Add(currentRoom, isRoomValid);
        }
        */
    }

    private void OnApplicationQuit()
    {
        foreach (Button item in mm.Children())
        {
            mm.Remove(item);
        } 
    }
}

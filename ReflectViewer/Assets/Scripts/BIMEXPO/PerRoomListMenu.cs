using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.UIElements;
using System.Linq;
using System;

public class PerRoomListMenu : MonoBehaviour
{
    List<string> roomNames;
    public List<bool> roomValidated { get; private set; }
    Label title;
    VisualElement root;
    List<int> surfacesInRoom, validatedSurfaces;

    // Called when building is fully loaded
    public void Initialize()
    {
        var fao = GameObject.Find("Root").GetComponent<FindAllObjects>();
        roomNames = fao.roomNames;
        roomValidated = Enumerable.Repeat(false, roomNames.Count).ToList(); // to do: retrieve from DB which rooms are validated yet or not
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("USS/room-inspector"));
        title = rootVisualElement.Q<Label>("roomName");
        root = rootVisualElement.Q<VisualElement>("root");
        root.style.display = DisplayStyle.None;
    }

    public void RefreshMenu()
    {
        if (root.style.display == DisplayStyle.None)
        {
            root.style.display = DisplayStyle.Flex;
        }
        ClearMenu();
    }

    void ClearMenu()
    {
        ListView list = root.Q<ListView>();
        Label errorMsg = root.Q<Label>("errorLabel");
        if (list != null)
        {
            root.Remove(list);
        }
        if (errorMsg != null)
        {
            root.Remove(errorMsg);
        }
    }

    public void PopulateMenu(Button sender)
    {
        // Title
        string room_name = sender.text;
        title.text = room_name;
        bool isRoomValidatedYet = roomValidated[roomNames.IndexOf(room_name)];
        if (isRoomValidatedYet)
        {
            title.AddToClassList("item-done");
        }
        else
        {
            title.AddToClassList("item-notdone");
        }

        // Get surfaces of this room
        var fao = GameObject.Find("Root").GetComponent<FindAllObjects>();
        try
        {
            surfacesInRoom = fao.surfacesPerRoom[room_name];
        }
        catch (KeyNotFoundException)
        {
            Label errorMsg = new Label("No tilable surfaces!");
            errorMsg.name = "errorLabel";
            errorMsg.AddToClassList("item-notdone");
            root.Add(errorMsg);
            return;
        }

        validatedSurfaces = new List<int>();

        // The "makeItem" function will be called as needed
        // when the ListView needs more items to render
        Func<VisualElement> makeItem = () => new Toggle();

        // As the user scrolls through the list, the ListView object
        // will recycle elements created by the "makeItem"
        // and invoke the "bindItem" callback to associate
        // the element with the matching data item (specified as an index in the list)
        Action<VisualElement, int> bindItem = (e, i) => { (e as Toggle).label = surfacesInRoom[i].ToString(); (e as Toggle).AddToClassList("red-toggle"); (e as Toggle).AddToClassList("short-toggle"); (e as Toggle).RegisterCallback<ClickEvent>(ev => ChangeSurfaceStatus(ev.target as Toggle)); };

        // Provide the list view with an explict height for every row
        // so it can calculate how many items to actually display
        const int itemHeight = 16;

        // Create the ListView
        var listView = new ListView(surfacesInRoom, itemHeight, makeItem, bindItem);

        listView.selectionType = SelectionType.Multiple;

        //listView.onItemChosen += obj => Debug.Log(obj);
        //listView.onSelectionChanged += objects => Debug.Log(objects);

        listView.style.flexGrow = 1.0f;

        // Add the ListView to the menu
        root.Add(listView);
    }

    /// <summary>
    /// Checks wether all the tilable surfaces of the room currently being adressed have been validated/chosen yet.
    /// This check is performed via the class variables surfacesInRoom and validatedSurfaces.
    /// </summary>
    /// <returns>True if the room is considered fully adressed, false otherwise.</returns>
    bool IsRoomValidated()
    {
        if (validatedSurfaces.Count == surfacesInRoom.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Adjust the surfaces and title styles accordingly to their status (validated or not).
    /// This method is bound to each toggle of the menu, and called upon check/uncheck of a toggle.
    /// </summary>
    /// <param name="sender">The toggle that has been checked or unchecked.</param>
    void ChangeSurfaceStatus(Toggle sender)
    {
        if (sender.value)
        {
            sender.AddToClassList("green-toggle");
            sender.RemoveFromClassList("red-toggle");
            validatedSurfaces.Add(Int32.Parse(sender.label));
        }
        else
        {
            sender.AddToClassList("red-toggle");
            sender.RemoveFromClassList("green-toggle");
            try
            {
                validatedSurfaces.RemoveAt(validatedSurfaces.IndexOf(Int32.Parse(sender.label)));
            }
            catch (ArgumentOutOfRangeException)
            {
                throw;
            }
        }
        /*
        ListView list = root.Q<ListView>();
        Debug.Log(list.itemsSource);
        bool allGreen = true;



        foreach (var item in list.itemsSource)
        {
            if (!item.value)
            {
                allGreen = false;
                break;
            }
        }*/
        if (IsRoomValidated())
        {
            title.RemoveFromClassList("item-notdone");
            title.AddToClassList("item-done");
            roomValidated[roomNames.IndexOf(title.text)] = true;
        }
        else
        {
            title.RemoveFromClassList("item-done");
            title.AddToClassList("item-notdone");
            roomValidated[roomNames.IndexOf(title.text)] = false;
        }
    }
}

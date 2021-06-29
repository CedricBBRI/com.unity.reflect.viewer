using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using Unity.Reflect.Viewer.UI;
using UnityEngine.UIElements;
using System;

namespace UnityEngine.Reflect
{
    public class FindAllObjects : MonoBehaviour  //Finds objects and makes them ready for editing and stuff?
    {

        Transform[] transformArr;
        List<Transform> transformList; //List of all the transforms of all objects imported by Reflect
        List<GameObject> objList; //List of all objects imported
        List<Metadata> metaList; //List of all metadata
        List<string> phases; //List of all phasing info
        int numPhases = 0; //Number of phases
        public Dropdown dropDownPhases; //An EMPTY dropdown menu, gets populated with the detected phases

        public UI.Slider slider; //EMPTY slider to go between phases
        public Text sliderVal; //Name of current phase shown
        public UI.Toggle prevToggle; //To select if to show phase alone or include previous phases as well, toggles between off and on

        public InputField sortBy; //EMPTY inputfield, gets populated automatically
        public Dropdown sortByDrop; //Empty dropdown, auto populated
        public Dropdown showOnly; //Empty dropdown

        List<string> keyList;
        List<string> keyList2;
        GameObject root; //GameObject under which all imported gameobjects are stored

        private bool buildingLoaded = false;
        public List<Vector3> roomCenters { get; private set; }
        public List<string> roomNames { get; private set; }
        public List<GameObject> roomPlaceHolders { get; private set; }

        public Dictionary<string, List<int>> surfacesPerRoom { get; private set; }

        void ExploitPLaceHolders()
        {
            roomCenters = new List<Vector3>();
            Transform[] mytransformArr = GameObject.FindObjectsOfType(typeof(Transform)) as Transform[];
            roomNames = new List<string>();
            roomPlaceHolders = new List<GameObject>();
            foreach (Transform tr in mytransformArr)
            {
                GameObject go = tr.gameObject;
                var meta = go.GetComponent<Metadata>();
                var mr = go.GetComponent<MeshRenderer>();
                if (meta != null && mr != null && meta.GetParameter("Mark") == "BIMEXPOPH")
                {
                    roomCenters.Add(mr.bounds.center);
                    mr.enabled = false;
                    roomNames.Add(meta.GetParameter("Comments"));
                    roomPlaceHolders.Add(go);
                    // also create reflection probes here later


                }
            }
            // Fill Menu
            var sms = GameObject.Find("SlidingMenu").GetComponent<SlidingMenu>();
            sms.PopulateMenu(roomNames);
        }

        public void GoToLocation(UIElements.Button button)
        {
            Vector3 loc = roomCenters[roomNames.IndexOf(button.text)];
            GameObject go = roomPlaceHolders[roomNames.IndexOf(button.text)];
            FreeFlyCamera cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FreeFlyCamera>();
            cam.SetMovePosition(loc, cam.transform.rotation);
        }

        public void FindAll(string strInput)
        {
            Dictionary<int, string> tempSurfacesPerRoom = new Dictionary<int, string>();
            if (transformList.Count == 0) //If the elements are not yet detected, then detect them
            {
                Initialize();
                string curPhase;
                foreach (Transform tr in transformList)
                {
                    GameObject go = tr.gameObject;
                    var meta = go.GetComponent<Metadata>();
                    if (go.transform.IsChildOf(root.transform) && meta != null && meta.GetParameters().Count() >= 1)
                    {
                        objList.Add(go);
                        //Adds collision boxes to all objects except those labeled as door and the placeholders
                        if (!meta.GetParameter("Category").Contains("Door") && meta.GetParameter("Mark") != "BIMEXPOPH")
                        { 
                            go.AddComponent<MeshCollider>();
                        }
                        metaList.Add(meta);
                        if (go.name.Contains(strInput)) //Find all elements whose name includes...
                        {
                            //Debug.Log(go.name + "\n");
                        }
                        Dictionary<string, Metadata.Parameter> dict = meta.GetParameters();
                        curPhase = meta.GetParameter(dropDownPhases.captionText.text);
                        if (!phases.Contains(curPhase) && curPhase.Count() >= 1)
                        {
                            phases.Add(curPhase);
                        }

                        // Store in which room is each surface
                        if (meta.GetParameter("Comments") != null && meta.GetParameter("Comments") != "" && (meta.GetParameter("Mark") == "O" || meta.GetParameter("Mark") == "A"))
                        {
                            tempSurfacesPerRoom.Add(Int32.Parse(meta.GetParameter("Id")), meta.GetParameter("Comments"));
                        }
                    }
                }
                phases.Sort();
                numPhases = phases.Count; //number of phases

                if (numPhases == 0)
                {
                    numPhases = 1;
                }
                slider.maxValue = numPhases;
                slider.value = slider.maxValue;

                keyList = new List<string>(metaList[0].GetParameters().Keys);
                Debug.Log(metaList[0].GetParameters().Keys.Count());
                sortByDrop.ClearOptions();
                sortByDrop.AddOptions(keyList);

                //UpdatePhasesShown(); //AC - 23/06/21: I comment this because it leads to crash, (phases is empty). Not used for the moment, we can fix later.
            }

            int count = 0;
            List<string> roomsDone = new List<string>();
            while (count < tempSurfacesPerRoom.Count)
            {
                string room = tempSurfacesPerRoom.Values.ElementAt(count);
                List<int> ids = new List<int>();
                if (!roomsDone.Contains(room))
                {
                    roomsDone.Add(room);
                    foreach (var item in tempSurfacesPerRoom)
                    {
                        if (item.Value == room)
                        {
                            ids.Add(item.Key);
                        }
                    }
                    surfacesPerRoom.Add(room, ids);
                }
                count += 1;
                
            }
            
        }
        public void SortCategories() //Gets the categories from the metadata, and makes it possible to filter by them
        { 
            string sortVal = keyList[sortByDrop.value];
            List<string> categories = new List<string>();
            foreach (GameObject go in objList)
            {
                var meta = go.GetComponent<Metadata>();
                string sortValGo = meta.GetParameter(sortVal);
                Debug.Log(sortValGo);
                if (sortValGo.Length >= 1 && !categories.Contains(sortValGo)) //If the category doesn't exist yet, create it
                {
                    categories.Add(sortValGo);
                    //catObj.name = sortValGo;
                }
            }

            showOnly.ClearOptions();
            keyList2 = categories;
            showOnly.AddOptions(keyList2);

            string sortCatsText = "list of " + sortVal + "\n";
            foreach(string str in categories)
            {
                Debug.Log(str);
                sortCatsText+= "\n" + str;
            }
            //sortCats.text = sortCatsText;

        }
        public void showOnlySelected() //Go through all gameobjects and disable those that don't have a specific metadata parameter
        {
            string sortVal = keyList2[showOnly.value];
            string param = keyList[sortByDrop.value];
            foreach (GameObject go in objList)
            {
                var meta = go.GetComponent<Metadata>();
                string sortValGo = meta.GetParameter(param);
                Debug.Log(sortVal + " " + sortValGo);
                
                if (sortVal.Equals(sortValGo))
                {
                    go.SetActive(true);
                }
                else
                {
                    go.SetActive(false);
                }
            }
        }

        public void showAll() //Enable all objects again
        {
            foreach (GameObject go in objList)
            {
                go.SetActive(true);
            }
        }

        public void UpdatePhasesShown() //This shows the currently active phase, and possibly the previous phases as well
        {
            sliderVal.text = phases[(int)slider.value-1].ToString();
            float maxPhase = slider.value;
            foreach(GameObject go in objList)
            {
                var meta = go.GetComponent<Metadata>();
                int phase = phases.IndexOf(meta.GetParameter(dropDownPhases.captionText.text));
                if (prevToggle.isOn)
                {
                    if (phase >= maxPhase)
                    {
                        go.SetActive(false);
                    }
                    else
                    {
                        go.SetActive(true);
                    }
                }
                else
                {
                    if (phase != maxPhase-1)
                    {
                        go.SetActive(false);
                    }
                    else
                    {
                        go.SetActive(true);
                    }
                }
            }
        }

        public void ClearLists() //Reset some lists if importing a new model
        {
            transformList = new List<Transform>();
            objList = new List<GameObject>();
            metaList = new List<Metadata>();
            phases = new List<string>();
        }
        public void Initialize() //Find Root object, list all the transforms in the scene, initialize some lists
        {
            root = GameObject.Find("Root");
            transformArr = GameObject.FindObjectsOfType(typeof(Transform)) as Transform[];
            transformList = new List<Transform>(transformArr);
            objList = new List<GameObject>();
            metaList = new List<Metadata>();
            phases = new List<string>();
        }


        // Start is called before the first frame update
        void Start()
        {
            surfacesPerRoom = new Dictionary<string, List<int>>();
            slider.minValue = 1;
            slider.maxValue = 1;
            slider.value = 1;

            UIStateManager.stateChanged += UIStateManager_stateChanged; // Listening to UI state change in order to know when the building is loaded.
        }

        private void UIStateManager_stateChanged(UIStateData obj)
        {
            if (obj.progressData.totalCount > 0 && obj.progressData.currentProgress == obj.progressData.totalCount)    // Then the building is fully loaded
            {
                if (!buildingLoaded)
                {
                    ExploitPLaceHolders();
                    buildingLoaded = true;
                }
            }
        }

    }
}

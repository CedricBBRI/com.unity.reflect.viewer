using UnityEngine.UIElements;

public class CustomComment : VisualElement
{
    public TextField textElem;

    public CustomComment(string comment = "...")
    {
        textElem = new TextField();
        textElem.multiline = true;
        textElem.style.flexDirection = FlexDirection.Column;
        textElem.style.flexGrow = 1;
        textElem.style.flexWrap = Wrap.Wrap;
        textElem.style.height = textElem.contentContainer.style.height;
        textElem.value = comment;
        Add(textElem);
        

        //Add USS style properties to the elements
        //Icon.AddToClassList("slotIcon");
        //AddToClassList("slotContainer");
    }
}

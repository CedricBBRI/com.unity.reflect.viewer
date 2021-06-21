using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using UnityEngine.UIElements;
//using DentedPixel;

public class SlidingMenu : MonoBehaviour
{
    // Animation
    private float startPosition, endPosition;
    private const int AnimationDurationMs = 1500;

    // UI
    private Button but;
    private VisualElement main, arrowContainer;

    // Start is called before the first frame update
    void OnEnable()
    {
        
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        main = rootVisualElement.Q<VisualElement>("mainB");
        //Vector2 tutu = GetComponent<RectTransform>().anchoredPosition;
        //float toto = main.contentRect.top;
        //LeanTween.move(GetComponent<RectTransform>(), new Vector2(10f, 0f), 1.0f).setEase(LeanTweenType.easeOutQuad);
        //main = rootVisualElement.Q<VisualElement>("testt");
        arrowContainer = rootVisualElement.Q<VisualElement>("arrow-container");
        but = rootVisualElement.Q<Button>("show");
        but.RegisterCallback<ClickEvent>(ev => Animate());
        //Debug.Log(main.style.top.value.value);
    }

    private void Update()
    {
        //var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        //main = rootVisualElement.Q<VisualElement>("testt");
        //Debug.Log("DBG: " + main.style.top.value.value);
    }

    private void Animate()
    {
        // see https://github.com/Unity-Technologies/UIToolkitUnityRoyaleRuntimeDemo/blob/7f5d60d438f46a437dfed54dcbfc6ceb15eb02de/Assets/Scripts/UI/EndScreen.cs#L79
        // Get Starting position

        var newrootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        VisualElement toto = newrootVisualElement.Q<VisualElement>("toto");
        VisualElement newmain = newrootVisualElement.Q<VisualElement>("mainB");
        //Length toto = newmain.style.top.value;
        //startPosition = main.worldBound.y;
        //startPosition = main.style.top.value.value;
        Debug.Log("MAIN STYLE: " + newmain.style);
        //Debug.Log("MAIN: " + main.style.bottom);
        Debug.Log("MAIN Y BBOX VALUE: " + startPosition);
        Debug.Log("MAIN BOT VALUE: " + newmain.style.bottom.value.value);
        float diff = main.worldBound.height - arrowContainer.worldBound.height;
        //main.style.height.value.value - arrowContainer.style.height.value.value;
        endPosition = diff - startPosition;
        Debug.Log("DIFF: " + diff);
        Debug.Log("endPosition: " + endPosition);

        //main.experimental.animation.Start(new StyleValues { top = startPosition, opacity = 1 }, new StyleValues { top = endPosition, opacity = 1 }, AnimationDurationMs).Ease(Easing.OutQuad);
    }
}

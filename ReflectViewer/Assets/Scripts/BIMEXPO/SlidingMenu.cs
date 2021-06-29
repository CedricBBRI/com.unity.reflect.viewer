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
    private const int AnimationDurationMs = 1000;

    // UI
    private Button but;
    private VisualElement main, arrowContainer, mm;

    // Start is called before the first frame update
    void OnEnable()
    {
        
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        main = rootVisualElement.Q<VisualElement>("mainB");
        main.style.right = -120.0f; // For some reason I have to set it here first, instead of simply read it from uxml..
        main.style.width = 150.0f; // For some reason I have to set it here first, instead of simply read it from uxml..
        arrowContainer = rootVisualElement.Q<VisualElement>("arrow-container");
        mm = rootVisualElement.Q<VisualElement>("moving-menu");
        but = rootVisualElement.Q<Button>("show");
        but.RegisterCallback<ClickEvent>(ev => Animate());
    }

    private void Animate()
    {
        // see https://github.com/Unity-Technologies/UIToolkitUnityRoyaleRuntimeDemo/blob/7f5d60d438f46a437dfed54dcbfc6ceb15eb02de/Assets/Scripts/UI/EndScreen.cs#L79
        // Get Starting position
        startPosition = main.style.right.value.value;
        float diff = main.worldBound.width - arrowContainer.worldBound.width;
        //main.style.height.value.value - arrowContainer.style.height.value.value;
        endPosition = -(diff + startPosition) ;

        main.experimental.animation.Start(new StyleValues { right = startPosition, opacity = 1 }, new StyleValues { right = endPosition, opacity = 1 }, AnimationDurationMs).Ease(Easing.OutQuad);
    }

    private void OnApplicationQuit()
    {
        foreach (Button item in mm.Children())
        {
            mm.Remove(item);
        } 
    }
}

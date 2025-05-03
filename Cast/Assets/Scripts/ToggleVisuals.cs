using UnityEngine;
using UnityEngine.UI;

public class ToggleVisuals : MonoBehaviour
{
    [SerializeField]
    Sprite onSprite = null;
    [SerializeField]
    Sprite offSprite = null;
    
    Image image = null;
    Toggle toggle = null;
    
    void Start()
    {
        toggle = GetComponent<Toggle>();
        image = GetComponent<Image>();

        if (toggle)
        {
            SetSprite(toggle.isOn);
            toggle.onValueChanged.AddListener(SetSprite);
        }
    }

    void SetSprite(bool on)
    {
        if (!image) return;
        
        if (on) image.sprite = onSprite;
        else image.sprite = offSprite;
    }
}

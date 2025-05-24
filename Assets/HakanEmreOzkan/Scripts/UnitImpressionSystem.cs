using UnityEngine;
using EagleByte.FriendlyCharacterController;
using VInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class UnitImpressionSystem : MonoBehaviour
{
    [Header("Get Parameters")]
    [SerializeField] private ObjectSelectionManager selectionManager;
    [SerializeField, ReadOnly] private GameObject animChar;

    [Header("Set Parameters")]
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private List<TextMeshProUGUI> txts;

    private void Update()
    {
        if (selectionManager.animationObj != null)
        {
            animChar = selectionManager.animationObj;
        }

        #region Methods
        if (animChar != null)
        {
            HealthBarSliderAndTextController();
            SpeedController();
            AttackSpeedController();
            LevelController();
        }
        #endregion
    }
    #region Methods
    private void HealthBarSliderAndTextController()
    {
        healthBarSlider.minValue = animChar.gameObject.GetComponent<FriendlyCharacterController>().healthBarSlider.minValue;
        healthBarSlider.maxValue = animChar.gameObject.GetComponent<FriendlyCharacterController>().healthBarSlider.maxValue;
        healthBarSlider.value = animChar.gameObject.GetComponent<FriendlyCharacterController>().healthBarSlider.value;

        txts[0].text = animChar.gameObject.GetComponent<FriendlyCharacterController>().healthBarSlider.value + $"/{healthBarSlider.maxValue}";
    }

    private void SpeedController() { txts[1].text = (animChar.gameObject.GetComponent<FriendlyCharacterController>().moveMinSpeed * 10).ToString(); }
    private void AttackSpeedController() { txts[2].text = (animChar.gameObject.GetComponent<FriendlyCharacterController>().attackSpeed * 10).ToString(); }
    private void LevelController() { txts[3].text = (animChar.gameObject.GetComponent<FriendlyCharacterController>().level).ToString(); }
    #endregion
}

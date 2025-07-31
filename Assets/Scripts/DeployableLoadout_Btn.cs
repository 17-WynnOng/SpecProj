using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeployableLoadout_Btn : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    private DeployableData deployableData;

    public void Initialize(DeployableData data)
    {
        deployableData = data;
        nameText.text = data.deployableName;
    }


    public void OnClick()
    {
        if (deployableData == null)
            return;

        LoadoutUI.Instance.SelectDeployables(deployableData);
    }
}

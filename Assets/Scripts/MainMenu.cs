using UnityEngine;
using UnityEngine.UI;
using VS.Core;
using VS.Parser;

public class MainMenu : MonoBehaviour
{

    public InputField pathInput;
    public CanvasGroup pathCG;
    public CanvasGroup menuCG;
    private VSPConfig conf;


    // Start is called before the first frame update
    void Start()
    {
        if (conf == null)
        {
            conf = Memory.LoadConfig();
            if (conf == null)
            {
                Memory.SaveConfig(new VSPConfig());
                conf = Memory.LoadConfig();
            }
            if (conf.VSPath != null && LBA.checkVSROM(conf.VSPath) != null)
            {
                Hide(pathCG);
                Show(menuCG);
            }
        }
    }

    private void Show(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    private void Hide(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f; //this makes everything transparent
        canvasGroup.blocksRaycasts = false; //this prevents the UI element to receive input events
    }

    public void OnSetPath()
    {
        bool validVSROM = (LBA.checkVSROM(pathInput.text) != null);
        if (pathInput && validVSROM)
        {
            conf.VSPath = pathInput.text;
            Memory.SaveConfig(conf);
            Hide(pathCG);
            Show(menuCG);
        }
    }
}

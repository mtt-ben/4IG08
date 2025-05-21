using UnityEngine;
using UnityEngine.UI;
using static Config;
public class SimulationUI : MonoBehaviour
{
    public Simulation simulation;

    public Button pauseButton;
    public Slider kernelSlider;
    public Slider dtSlider;
    public Toggle showVelocityToggle;

    private bool isPaused = false;

    void Start()
    {
        pauseButton.onClick.AddListener(TogglePause);
        kernelSlider.onValueChanged.AddListener(UpdateKernelRadius);
        dtSlider.onValueChanged.AddListener(UpdateDT);
        showVelocityToggle.onValueChanged.AddListener(UpdateVelocityDisplay);

        // Optional: Set initial values
        kernelSlider.value = KERNEL_RADIUS;
        dtSlider.value = DT;
        showVelocityToggle.isOn = simulation.showVelocity;
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        simulation.enabled = !isPaused;
        pauseButton.GetComponentInChildren<Text>().text = isPaused ? "Resume" : "Pause";
    }

    void UpdateKernelRadius(float value)
    {
        Config.KERNEL_RADIUS = value;
    }

    void UpdateDT(float value)
    {
        Config.DT = value;
    }

    void UpdateVelocityDisplay(bool isOn)
    {
        simulation.showVelocity = isOn;
    }
}

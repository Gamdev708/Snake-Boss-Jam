using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] Text healthText;
    Health playerHealth;
    private void Awake()
    {
        if (healthText == null)
        {
            Debug.LogError("Health Text is not assigned in the inspector.");
        }
        playerHealth = GameObject.FindGameObjectWithTag("Player").TryGetComponent<Health>(out Health health) ? health : null;
        healthText.text = playerHealth != null ? playerHealth.GetHealthPoints().ToString() : "N/A";
    }

    public void UpdateHealth()
    {
        healthText.text = playerHealth != null ? playerHealth.GetHealthPoints().ToString() : "N/A";
    }
}

using UnityEngine;
using System;

public enum TutorialStep
{
    NotStarted,
    PetCat,       // Bước 1: Vuốt ve mèo
    ServeCoffee,  // Bước 2: Phục vụ khách
    OpenShop,     // Bước 3: Mở cửa hàng
    Completed     // Hoàn thành
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("State")]
    public TutorialStep currentStep = TutorialStep.NotStarted;
    public bool isTutorialActive = false;

    public event Action<TutorialStep, string> OnTutorialStepChanged;
    public event Action OnTutorialFinished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartTutorial()
    {
        isTutorialActive = true;
        SetStep(TutorialStep.PetCat);
    }

    public void SetStep(TutorialStep step)
    {
        currentStep = step;
        string instruction = "";

        switch (step)
        {
            case TutorialStep.PetCat:
                instruction = "Bước 1: Chạm hoặc click chuột vào chú mèo để vuốt ve làm quen!";
                break;
            case TutorialStep.ServeCoffee:
                instruction = "Bước 2: Khách đã vào bàn! Bấm nút 'Phục vụ Cafe' để kiếm tiền!";
                break;
            case TutorialStep.OpenShop:
                instruction = "Bước 3: Tuyệt vời! Bấm vào nút 'Cửa Hàng' để xem bàn ghế & mèo mới!";
                break;
            case TutorialStep.Completed:
                instruction = "Chúc mừng bạn đã sẵn sàng làm chủ Quán Cà Phê Mèo!";
                break;
        }

        OnTutorialStepChanged?.Invoke(currentStep, instruction);

        if (step == TutorialStep.Completed)
        {
            CompleteTutorial();
        }
    }

    public void OnCatPetted()
    {
        if (isTutorialActive && currentStep == TutorialStep.PetCat)
        {
            SetStep(TutorialStep.ServeCoffee);
        }
    }

    public void OnCoffeeServed()
    {
        if (isTutorialActive && currentStep == TutorialStep.ServeCoffee)
        {
            SetStep(TutorialStep.OpenShop);
        }
    }

    public void OnShopOpened()
    {
        if (isTutorialActive && currentStep == TutorialStep.OpenShop)
        {
            SetStep(TutorialStep.Completed);
        }
    }

    private void CompleteTutorial()
    {
        isTutorialActive = false;

        // Thưởng tân thủ: $50 và 10 Kim Cương Mèo
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(50f);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGems(10);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFloatingText("🎁 Thưởng Tân Thủ: +$50 & +10 🐾!", Camera.main.transform.position + Camera.main.transform.forward * 2.5f, Color.green);
        }

        OnTutorialFinished?.Invoke();
        SaveManager.Instance?.SaveGame();
    }
}

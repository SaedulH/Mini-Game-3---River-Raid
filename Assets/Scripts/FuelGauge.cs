using UnityEngine;

public class FuelGauge : MonoBehaviour
{
    public static FuelGauge Instance;
    public bool _refueling = false;
    public float FuelUsageRate = 7f;
    private readonly float _tick = 1;
    private float _timer = 0;
    private RectTransform _rectTransform;

    void Start()
    {
        Instance = this;
        _rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (PlayerManager.Instance.IsPlaying)
        {
            if (!_refueling)
            {
                UseFuel();
            }
            else
            {
                Refuel();
            }

            if (_rectTransform.anchoredPosition.x <= -200 && PlayerManager.Instance.IsPlayerAlive)
            {
                GameManager.Instance.GameOver();
                Debug.Log("Out of Fuel!!");
            }
        }
    }

    void UseFuel()
    {
        _timer += Time.deltaTime;
        if (_timer >= _tick)
        {
            _timer = 0;
            if (_rectTransform.anchoredPosition.x > -201)
            {
                _rectTransform.anchoredPosition += Vector2.left * FuelUsageRate;
            }
        }
    }

    void Refuel()
    {
        if (_rectTransform.anchoredPosition.x <= 200)
        {
            _rectTransform.anchoredPosition += 40 * Time.deltaTime * Vector2.right;
        }
    }

    public void Reset()
    {
        _rectTransform.anchoredPosition = new Vector2(200, _rectTransform.anchoredPosition.y);
    }
}

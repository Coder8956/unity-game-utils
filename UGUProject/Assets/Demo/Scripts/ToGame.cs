using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ToGame : MonoBehaviour
{
    [SerializeField] private GameObject m_particleObject;
    [SerializeField] private float m_scaleSpeed = 1f;
    [SerializeField] private GameObject m_activateObject;
    [SerializeField] private Button m_jumpButton;

    private Coroutine m_scaleCoroutine;

    private void Awake()
    {
        if (m_activateObject != null)
            m_activateObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (m_jumpButton != null)
            m_jumpButton.onClick.AddListener(OnJumpButtonClick);
    }

    private void OnDisable()
    {
        if (m_jumpButton != null)
            m_jumpButton.onClick.RemoveListener(OnJumpButtonClick);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        ScaleTo(Vector3.zero, OnScaleComplete);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (m_activateObject != null)
            m_activateObject.SetActive(false);
        ScaleTo(Vector3.one, null);
    }

    private void ScaleTo(Vector3 targetScale, Action onComplete)
    {
        if (m_particleObject == null) return;

        if (m_scaleCoroutine != null)
            StopCoroutine(m_scaleCoroutine);

        m_scaleCoroutine = StartCoroutine(ScaleRoutine(m_particleObject.transform, targetScale, m_scaleSpeed, onComplete));
    }

    private IEnumerator ScaleRoutine(Transform target, Vector3 targetScale, float speed, Action onComplete)
    {
        Vector3 startScale = target.localScale;
        float progress = 0f;

        while (progress < 1f)
        {
            progress += speed * Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, progress);
            target.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        target.localScale = targetScale;
        m_scaleCoroutine = null;
        onComplete?.Invoke();
    }

    private void OnScaleComplete()
    {
        if (m_activateObject != null)
            m_activateObject.SetActive(true);
    }

    private void OnJumpButtonClick()
    {
        SceneManager.LoadScene("Game");
    }
}

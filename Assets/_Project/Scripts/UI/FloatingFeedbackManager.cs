using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.UI
{
    /// <summary>
    /// Displays floating revenue/profit receipts above the checkout register counter
    /// when customer purchases are completed.
    /// </summary>
    public class FloatingFeedbackManager : MonoBehaviourSingleton<FloatingFeedbackManager>
    {
        private readonly Queue<GameObject> _textPool = new Queue<GameObject>(6);

        public void ShowCashEarned(Vector3 worldPosition, double amount, double profit = 0.0)
        {
            GameObject textObj = GetOrCreateTextObject();
            textObj.transform.position = worldPosition + new Vector3(0f, 1.2f, 0f);
            textObj.transform.rotation = Quaternion.Euler(35f, 0f, 0f); // Tilted towards typical isometric camera
            textObj.SetActive(true);

            TextMesh textMesh = textObj.GetComponent<TextMesh>();
            if (profit > 0.0)
            {
                textMesh.text = $"+${amount:F2}\n<color=#FFD700>(+${profit:F2} Net)</color>";
            }
            else
            {
                textMesh.text = $"+${amount:F2}";
            }

            StartCoroutine(AnimateFloatingText(textObj, textMesh));
        }

        public void ShowLevelUpFeedback(int level)
        {
            Vector3 centerPos = new Vector3(0f, 2.5f, -4f);
            GameObject textObj = GetOrCreateTextObject();
            textObj.transform.position = centerPos;
            textObj.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
            textObj.SetActive(true);

            TextMesh textMesh = textObj.GetComponent<TextMesh>();
            textMesh.text = $"★ MARKET LEVEL UP! ★\n<color=#FFD700>LEVEL {level}</color>";
            textMesh.color = new Color(1f, 0.85f, 0.2f, 1f);

            StartCoroutine(AnimateFloatingText(textObj, textMesh));
        }

        public void ShowExpansionUnlockedFeedback(int level, string title)
        {
            Vector3 centerPos = new Vector3(0f, 2.8f, -2.5f);
            GameObject textObj = GetOrCreateTextObject();
            textObj.transform.position = centerPos;
            textObj.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
            textObj.SetActive(true);

            TextMesh textMesh = textObj.GetComponent<TextMesh>();
            string header = string.IsNullOrEmpty(title) ? "NEW MARKET AREA" : title.ToUpper();
            textMesh.text = $"★ {header} ★\n<color=#4CE685>EXPANSION UNLOCKED!</color>";
            textMesh.color = new Color(0.2f, 0.95f, 0.35f, 1f);

            StartCoroutine(AnimateFloatingText(textObj, textMesh));
        }

        public void ShowMoodFeedback(Vector3 worldPosition, string text, Color color)
        {
            GameObject textObj = GetOrCreateTextObject();
            textObj.transform.position = worldPosition + new Vector3(0f, 1.8f, 0f);
            textObj.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
            textObj.SetActive(true);

            TextMesh textMesh = textObj.GetComponent<TextMesh>();
            textMesh.text = text;
            textMesh.color = color;

            StartCoroutine(AnimateFloatingText(textObj, textMesh, color));
        }

        private GameObject GetOrCreateTextObject()
        {
            if (_textPool.Count > 0)
            {
                return _textPool.Dequeue();
            }

            GameObject obj = new GameObject("Floating_Cash_Feedback");
            obj.transform.parent = transform;
            TextMesh tm = obj.AddComponent<TextMesh>();
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.characterSize = 0.08f;
            tm.fontSize = 32;
            tm.color = new Color(0.2f, 0.95f, 0.3f, 1f); // Vibrant emerald green
            return obj;
        }

        private IEnumerator AnimateFloatingText(GameObject obj, TextMesh tm, Color? customColor = null)
        {
            float duration = 1.3f;
            float elapsed = 0f;
            Vector3 startPos = obj.transform.position;
            Color startColor = customColor ?? tm.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Drift upwards smoothly
                obj.transform.position = startPos + new Vector3(0f, t * 1.0f, 0f);

                // Fade alpha out in second half
                float alpha = Mathf.Clamp01(1f - (t * 1.2f));
                tm.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

                yield return null;
            }

            obj.SetActive(false);
            _textPool.Enqueue(obj);
        }
    }
}

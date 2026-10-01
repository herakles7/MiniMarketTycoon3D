using UnityEngine;

namespace MiniMarketTycoon.Gameplay
{
    /// <summary>
    /// Crisp, modern 3D world-space floating prompt tooltip that displays above
    /// interactable objects (shelves, cash register, wholesale PC, boxes).
    /// Billboards dynamically towards the main camera with elastic scale animations.
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private GameObject _container;
        [SerializeField] private TextMesh _promptTextMesh;
        [SerializeField] private Transform _backgroundPlate;

        [Header("Animation Settings")]
        [SerializeField] private float _popSpeed = 14f;
        [SerializeField] private Vector3 _targetScale = new Vector3(1f, 1f, 1f);
        [SerializeField] private float _heightOffset = 0.4f;

        private Vector3 _targetWorldPos;
        private bool _isVisible = false;
        private Camera _mainCamera;

        private void Awake()
        {
            if (_container == null)
            {
                EnsureVisualHierarchy();
            }

            _mainCamera = Camera.main;
            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
        }

        private void EnsureVisualHierarchy()
        {
            _container = gameObject;

            // Background quad (semi-transparent dark pill background)
            Transform bg = transform.Find("Background");
            if (bg == null)
            {
                GameObject bgGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
                bgGo.name = "Background";
                bgGo.transform.SetParent(transform, false);
                bgGo.transform.localPosition = new Vector3(0f, 0f, 0.02f);
                bgGo.transform.localScale = new Vector3(1.8f, 0.5f, 1f);

                // Destroy collider
                Collider c = bgGo.GetComponent<Collider>();
                if (c != null) Destroy(c);

                var r = bgGo.GetComponent<Renderer>();
                if (r != null)
                {
                    Shader s = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
                    Material m = new Material(s);
                    m.color = new Color(0.08f, 0.10f, 0.14f, 0.88f); // Modern slate dark
                    r.material = m;
                }
                _backgroundPlate = bgGo.transform;
            }
            else
            {
                _backgroundPlate = bg;
            }

            // TextMesh
            Transform txt = transform.Find("PromptText");
            if (txt == null)
            {
                GameObject txtGo = new GameObject("PromptText");
                txtGo.transform.SetParent(transform, false);
                txtGo.transform.localPosition = new Vector3(0f, 0f, 0f);
                _promptTextMesh = txtGo.AddComponent<TextMesh>();
                _promptTextMesh.alignment = TextAlignment.Center;
                _promptTextMesh.anchor = TextAnchor.MiddleCenter;
                _promptTextMesh.characterSize = 0.055f;
                _promptTextMesh.fontSize = 32;
                _promptTextMesh.color = Color.white;
            }
            else
            {
                _promptTextMesh = txt.GetComponent<TextMesh>();
            }
        }

        private void LateUpdate()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            // Always face camera (billboard)
            transform.rotation = _mainCamera.transform.rotation;

            // Smooth scale pop
            Vector3 desiredScale = _isVisible ? _targetScale : Vector3.zero;
            transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * _popSpeed);

            if (!_isVisible && transform.localScale.x < 0.02f)
            {
                gameObject.SetActive(false);
            }
        }

        public void ShowPrompt(string promptText, Vector3 worldPosition)
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            _targetWorldPos = worldPosition + Vector3.up * _heightOffset;
            transform.position = _targetWorldPos;

            SetPromptText(promptText);

            gameObject.SetActive(true);
            _isVisible = true;
        }

        public void SetPromptText(string promptText)
        {
            if (_promptTextMesh != null)
            {
                _promptTextMesh.text = promptText;

                // Adjust background width dynamically based on text length
                if (_backgroundPlate != null)
                {
                    float width = Mathf.Clamp(promptText.Length * 0.085f + 0.4f, 1.2f, 3.8f);
                    _backgroundPlate.localScale = new Vector3(width, 0.45f, 1f);
                }
            }
        }

        public void UpdatePosition(Vector3 worldPosition)
        {
            _targetWorldPos = worldPosition + Vector3.up * _heightOffset;
            transform.position = Vector3.Lerp(transform.position, _targetWorldPos, Time.deltaTime * 18f);
        }

        public void HidePrompt()
        {
            _isVisible = false;
        }
    }
}

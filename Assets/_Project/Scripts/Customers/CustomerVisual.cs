using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Manages the visual presentation, stylized-realistic human proportions,
    /// material variations (Customer_A, Customer_B, Customer_C), shopping basket accessory,
    /// and optional debug state badge.
    /// </summary>
    public class CustomerVisual : MonoBehaviour
    {
        [Header("Mesh Renderers")]
        [SerializeField] private MeshRenderer _headRenderer;
        [SerializeField] private MeshRenderer _hairRenderer;
        [SerializeField] private MeshRenderer _torsoRenderer;
        [SerializeField] private MeshRenderer _leftArmRenderer;
        [SerializeField] private MeshRenderer _rightArmRenderer;
        [SerializeField] private MeshRenderer _leftLegRenderer;
        [SerializeField] private MeshRenderer _rightLegRenderer;
        [SerializeField] private MeshRenderer _leftShoeRenderer;
        [SerializeField] private MeshRenderer _rightShoeRenderer;
        [SerializeField] private GameObject _shoppingBasketObject;

        [Header("Debug Visuals")]
        [SerializeField] private TextMesh _debugTextMesh;
        [SerializeField] private GameObject _debugBadgeRoot;

        [Header("Articulated Transforms")]
        [SerializeField] private Transform _torsoTransform;
        [SerializeField] private Transform _headTransform;
        [SerializeField] private Transform _leftArmTransform;
        [SerializeField] private Transform _rightArmTransform;
        [SerializeField] private Transform _leftLegTransform;
        [SerializeField] private Transform _rightLegTransform;

        private CustomerVariationType _variationType;

        public Transform TorsoTransform => _torsoTransform;
        public Transform HeadTransform => _headTransform;
        public Transform LeftArmTransform => _leftArmTransform;
        public Transform RightArmTransform => _rightArmTransform;
        public Transform LeftLegTransform => _leftLegTransform;
        public Transform RightLegTransform => _rightLegTransform;

        private void Awake()
        {
            if (transform.childCount == 0)
            {
                Material skinMat = LoadMat("Mat_Skin_Warm");
                Material hairMat = LoadMat("Mat_Hair_Brown");
                Material clothesMat = LoadMat("Mat_Clothes_Blue");
                Material pantsMat = LoadMat("Mat_Pants_Khaki");
                Material shoeMat = LoadMat("Mat_Shoes_Dark");
                Material basketMat = LoadMat("Mat_Basket_Red");
                BuildProceduralHumanoidRig(skinMat, hairMat, clothesMat, pantsMat, shoeMat, basketMat);

                var anim = GetComponent<CustomerAnimationController>();
                if (anim != null)
                {
                    anim.BindProceduralBones(_torsoTransform, _headTransform, _leftArmTransform, _rightArmTransform, _leftLegTransform, _rightLegTransform);
                }
            }
        }

        public void ApplyVariation(CustomerVariationType variation)
        {
            _variationType = variation;

            // Load materials
            Material skinMat = null;
            Material hairMat = null;
            Material clothesMat = null;
            Material pantsMat = null;
            Material shoeMat = null;

            switch (variation)
            {
                case CustomerVariationType.Customer_A:
                    skinMat = Resources.Load<Material>("Mat_Skin_Warm") ?? LoadMat("Mat_Skin_Warm");
                    hairMat = Resources.Load<Material>("Mat_Hair_Brown") ?? LoadMat("Mat_Hair_Brown");
                    clothesMat = Resources.Load<Material>("Mat_Clothes_Blue") ?? LoadMat("Mat_Clothes_Blue");
                    pantsMat = Resources.Load<Material>("Mat_Pants_Khaki") ?? LoadMat("Mat_Pants_Khaki");
                    shoeMat = Resources.Load<Material>("Mat_Shoes_Dark") ?? LoadMat("Mat_Shoes_Dark");
                    break;

                case CustomerVariationType.Customer_B:
                    skinMat = Resources.Load<Material>("Mat_Skin_Olive") ?? LoadMat("Mat_Skin_Olive");
                    hairMat = Resources.Load<Material>("Mat_Hair_Black") ?? LoadMat("Mat_Hair_Black");
                    clothesMat = Resources.Load<Material>("Mat_Clothes_Teal") ?? LoadMat("Mat_Clothes_Teal");
                    pantsMat = Resources.Load<Material>("Mat_Pants_Navy") ?? LoadMat("Mat_Pants_Navy");
                    shoeMat = Resources.Load<Material>("Mat_Shoes_White") ?? LoadMat("Mat_Shoes_White");
                    break;

                case CustomerVariationType.Customer_C:
                    skinMat = Resources.Load<Material>("Mat_Skin_Fair") ?? LoadMat("Mat_Skin_Fair");
                    hairMat = Resources.Load<Material>("Mat_Hair_Blonde") ?? LoadMat("Mat_Hair_Blonde");
                    clothesMat = Resources.Load<Material>("Mat_Clothes_Maroon") ?? LoadMat("Mat_Clothes_Maroon");
                    pantsMat = Resources.Load<Material>("Mat_Pants_DarkGrey") ?? LoadMat("Mat_Pants_DarkGrey");
                    shoeMat = Resources.Load<Material>("Mat_Shoes_Dark") ?? LoadMat("Mat_Shoes_Dark");
                    break;
            }

            if (skinMat != null && _headRenderer != null) _headRenderer.sharedMaterial = skinMat;
            if (hairMat != null && _hairRenderer != null) _hairRenderer.sharedMaterial = hairMat;
            if (clothesMat != null)
            {
                if (_torsoRenderer != null) _torsoRenderer.sharedMaterial = clothesMat;
                if (_leftArmRenderer != null) _leftArmRenderer.sharedMaterial = clothesMat;
                if (_rightArmRenderer != null) _rightArmRenderer.sharedMaterial = clothesMat;
            }
            if (pantsMat != null)
            {
                if (_leftLegRenderer != null) _leftLegRenderer.sharedMaterial = pantsMat;
                if (_rightLegRenderer != null) _rightLegRenderer.sharedMaterial = pantsMat;
            }
            if (shoeMat != null)
            {
                if (_leftShoeRenderer != null) _leftShoeRenderer.sharedMaterial = shoeMat;
                if (_rightShoeRenderer != null) _rightShoeRenderer.sharedMaterial = shoeMat;
            }
        }

        private Material LoadMat(string name)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>($"Assets/_Project/Art/Materials/{name}.mat");
#else
            return null;
#endif
        }

        public void SetBasketVisible(bool visible)
        {
            if (_shoppingBasketObject != null)
            {
                _shoppingBasketObject.SetActive(visible);
            }
        }

        public void SetDebugState(CustomerState state, bool debugEnabled)
        {
            if (_debugBadgeRoot == null) return;

            if (!debugEnabled)
            {
                if (_debugBadgeRoot.activeSelf) _debugBadgeRoot.SetActive(false);
                return;
            }

            if (!_debugBadgeRoot.activeSelf) _debugBadgeRoot.SetActive(true);

            if (_debugTextMesh != null)
            {
                _debugTextMesh.text = state.ToString().ToUpper();
                _debugTextMesh.color = GetStateColor(state);
            }
        }

        private Color GetStateColor(CustomerState state)
        {
            switch (state)
            {
                case CustomerState.Shopping: return Color.yellow;
                case CustomerState.WaitingInQueue: return new Color(1f, 0.5f, 0f); // Orange
                case CustomerState.CheckingOut: return Color.green;
                case CustomerState.Leaving: return Color.cyan;
                case CustomerState.Stuck: return Color.red;
                default: return Color.white;
            }
        }

        /// <summary>
        /// Assembles an articulated stylized human character hierarchy if building procedural prefab.
        /// </summary>
        public void BuildProceduralHumanoidRig(Material defaultSkin, Material defaultHair, Material defaultClothes, Material defaultPants, Material defaultShoes, Material basketMat)
        {
            // Clear existing children
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            GameObject modelRoot = new GameObject("Humanoid_Model");
            modelRoot.transform.parent = transform;
            modelRoot.transform.localPosition = Vector3.zero;
            modelRoot.transform.localRotation = Quaternion.identity;

            // 1. Pelvis / Hips root
            GameObject hips = new GameObject("Hips");
            hips.transform.parent = modelRoot.transform;
            hips.transform.localPosition = new Vector3(0f, 0.85f, 0f);

            // 2. Torso (0.42m wide, 0.52m tall, 0.24m thick)
            GameObject torsoObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torsoObj.name = "Torso";
            torsoObj.transform.parent = hips.transform;
            torsoObj.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            torsoObj.transform.localScale = new Vector3(0.42f, 0.55f, 0.24f);
            DestroyImmediate(torsoObj.GetComponent<Collider>());
            _torsoRenderer = torsoObj.GetComponent<MeshRenderer>();
            _torsoRenderer.sharedMaterial = defaultClothes;
            _torsoTransform = torsoObj.transform;

            // 3. Neck & Head
            GameObject headObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            headObj.name = "Head";
            headObj.transform.parent = torsoObj.transform;
            headObj.transform.localPosition = new Vector3(0f, 0.62f, 0.02f);
            headObj.transform.localScale = new Vector3(0.6f, 0.52f, 0.7f);
            DestroyImmediate(headObj.GetComponent<Collider>());
            _headRenderer = headObj.GetComponent<MeshRenderer>();
            _headRenderer.sharedMaterial = defaultSkin;
            _headTransform = headObj.transform;

            // Hair cap
            GameObject hairObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hairObj.name = "Hair";
            hairObj.transform.parent = headObj.transform;
            hairObj.transform.localPosition = new Vector3(0f, 0.28f, -0.05f);
            hairObj.transform.localScale = new Vector3(1.05f, 0.65f, 1.05f);
            DestroyImmediate(hairObj.GetComponent<Collider>());
            _hairRenderer = hairObj.GetComponent<MeshRenderer>();
            _hairRenderer.sharedMaterial = defaultHair;

            // 4. Left Arm (Shoulder pivot at x: -0.27m, y: 0.2m)
            GameObject lArmPivot = new GameObject("Left_Arm_Pivot");
            lArmPivot.transform.parent = torsoObj.transform;
            lArmPivot.transform.localPosition = new Vector3(-0.58f, 0.2f, 0f);
            _leftArmTransform = lArmPivot.transform;

            GameObject lArmMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lArmMesh.name = "Left_Arm_Mesh";
            lArmMesh.transform.parent = lArmPivot.transform;
            lArmMesh.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            lArmMesh.transform.localScale = new Vector3(0.24f, 0.26f, 0.24f);
            DestroyImmediate(lArmMesh.GetComponent<Collider>());
            _leftArmRenderer = lArmMesh.GetComponent<MeshRenderer>();
            _leftArmRenderer.sharedMaterial = defaultClothes;

            // 5. Right Arm (Shoulder pivot at x: 0.27m, y: 0.2m)
            GameObject rArmPivot = new GameObject("Right_Arm_Pivot");
            rArmPivot.transform.parent = torsoObj.transform;
            rArmPivot.transform.localPosition = new Vector3(0.58f, 0.2f, 0f);
            _rightArmTransform = rArmPivot.transform;

            GameObject rArmMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rArmMesh.name = "Right_Arm_Mesh";
            rArmMesh.transform.parent = rArmPivot.transform;
            rArmMesh.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            rArmMesh.transform.localScale = new Vector3(0.24f, 0.26f, 0.24f);
            DestroyImmediate(rArmMesh.GetComponent<Collider>());
            _rightArmRenderer = rArmMesh.GetComponent<MeshRenderer>();
            _rightArmRenderer.sharedMaterial = defaultClothes;

            // Shopping Basket held in left hand
            GameObject basket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basket.name = "Shopping_Basket";
            basket.transform.parent = lArmPivot.transform;
            basket.transform.localPosition = new Vector3(-0.05f, -0.52f, 0.12f);
            basket.transform.localScale = new Vector3(0.34f, 0.22f, 0.26f);
            DestroyImmediate(basket.GetComponent<Collider>());
            basket.GetComponent<MeshRenderer>().sharedMaterial = basketMat;
            _shoppingBasketObject = basket;
            _shoppingBasketObject.SetActive(false);

            // 6. Left Leg (Hip pivot at x: -0.11m, y: 0f)
            GameObject lLegPivot = new GameObject("Left_Leg_Pivot");
            lLegPivot.transform.parent = hips.transform;
            lLegPivot.transform.localPosition = new Vector3(-0.12f, 0f, 0f);
            _leftLegTransform = lLegPivot.transform;

            GameObject lLegMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lLegMesh.name = "Left_Leg_Mesh";
            lLegMesh.transform.parent = lLegPivot.transform;
            lLegMesh.transform.localPosition = new Vector3(0f, -0.4f, 0f);
            lLegMesh.transform.localScale = new Vector3(0.14f, 0.42f, 0.14f);
            DestroyImmediate(lLegMesh.GetComponent<Collider>());
            _leftLegRenderer = lLegMesh.GetComponent<MeshRenderer>();
            _leftLegRenderer.sharedMaterial = defaultPants;

            // Left Shoe
            GameObject lShoe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lShoe.name = "Left_Shoe";
            lShoe.transform.parent = lLegPivot.transform;
            lShoe.transform.localPosition = new Vector3(0f, -0.8f, 0.04f);
            lShoe.transform.localScale = new Vector3(0.13f, 0.09f, 0.22f);
            DestroyImmediate(lShoe.GetComponent<Collider>());
            _leftShoeRenderer = lShoe.GetComponent<MeshRenderer>();
            _leftShoeRenderer.sharedMaterial = defaultShoes;

            // 7. Right Leg (Hip pivot at x: 0.11m, y: 0f)
            GameObject rLegPivot = new GameObject("Right_Leg_Pivot");
            rLegPivot.transform.parent = hips.transform;
            rLegPivot.transform.localPosition = new Vector3(0.12f, 0f, 0f);
            _rightLegTransform = rLegPivot.transform;

            GameObject rLegMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rLegMesh.name = "Right_Leg_Mesh";
            rLegMesh.transform.parent = rLegPivot.transform;
            rLegMesh.transform.localPosition = new Vector3(0f, -0.4f, 0f);
            rLegMesh.transform.localScale = new Vector3(0.14f, 0.42f, 0.14f);
            DestroyImmediate(rLegMesh.GetComponent<Collider>());
            _rightLegRenderer = rLegMesh.GetComponent<MeshRenderer>();
            _rightLegRenderer.sharedMaterial = defaultPants;

            // Right Shoe
            GameObject rShoe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rShoe.name = "Right_Shoe";
            rShoe.transform.parent = rLegPivot.transform;
            rShoe.transform.localPosition = new Vector3(0f, -0.8f, 0.04f);
            rShoe.transform.localScale = new Vector3(0.13f, 0.09f, 0.22f);
            DestroyImmediate(rShoe.GetComponent<Collider>());
            _rightShoeRenderer = rShoe.GetComponent<MeshRenderer>();
            _rightShoeRenderer.sharedMaterial = defaultShoes;

            // 8. Debug State Floating Badge
            GameObject debugBadge = new GameObject("Debug_State_Badge");
            debugBadge.transform.parent = transform;
            debugBadge.transform.localPosition = new Vector3(0f, 2.05f, 0f);
            _debugTextMesh = debugBadge.AddComponent<TextMesh>();
            _debugTextMesh.alignment = TextAlignment.Center;
            _debugTextMesh.anchor = TextAnchor.MiddleCenter;
            _debugTextMesh.fontSize = 28;
            _debugTextMesh.characterSize = 0.06f;
            _debugTextMesh.text = "IDLE";
            _debugBadgeRoot = debugBadge;
            _debugBadgeRoot.SetActive(false);
        }
    }
}

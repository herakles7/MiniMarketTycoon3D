using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Manages high-level customer visual presentation and legacy variation compatibility.
    /// Interfaces with the modular CustomerVisualController, coordinates debugging badges,
    /// and ensures backward compatibility with Stage 8 systems.
    /// </summary>
    public class CustomerVisual : MonoBehaviour
    {
        [Header("Modular Visual Controller Reference")]
        [SerializeField] private CustomerVisualController _visualController;

        [Header("Mesh Renderers (Fallback / Legacy)")]
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

        public CustomerVisualController VisualController => _visualController;
        public Transform TorsoTransform => _visualController != null && _visualController.TorsoTransform != null ? _visualController.TorsoTransform : _torsoTransform;
        public Transform HeadTransform => _visualController != null && _visualController.HeadTransform != null ? _visualController.HeadTransform : _headTransform;
        public Transform LeftArmTransform => _visualController != null && _visualController.LeftArmTransform != null ? _visualController.LeftArmTransform : _leftArmTransform;
        public Transform RightArmTransform => _visualController != null && _visualController.RightArmTransform != null ? _visualController.RightArmTransform : _rightArmTransform;
        public Transform LeftLegTransform => _visualController != null && _visualController.LeftLegTransform != null ? _visualController.LeftLegTransform : _leftLegTransform;
        public Transform RightLegTransform => _visualController != null && _visualController.RightLegTransform != null ? _visualController.RightLegTransform : _rightLegTransform;

        private void Awake()
        {
            if (_visualController == null)
            {
                _visualController = GetComponent<CustomerVisualController>();
            }

            if (transform.childCount == 0 && _visualController == null)
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

        public void ApplyVisualProfile(CustomerVisualProfile profile)
        {
            if (_visualController != null)
            {
                _visualController.ApplyProfile(profile);
            }
        }

        public void ApplyVariation(CustomerVariationType variation)
        {
            _variationType = variation;

            if (_visualController != null)
            {
                var profile = new CustomerVisualProfile();
                switch (variation)
                {
                    case CustomerVariationType.Customer_A:
                        profile.Gender = CustomerGender.Male;
                        profile.AgeGroup = CustomerAgeGroup.Adult;
                        profile.SkinTone = CustomerSkinTone.Warm;
                        profile.HairStyle = CustomerHairStyle.Short;
                        profile.HairColor = CustomerHairColor.Brown;
                        profile.TopType = CustomerTopType.TShirt;
                        profile.BottomType = CustomerBottomType.Chino;
                        profile.ShoeType = CustomerShoeType.Sneakers;
                        profile.SkinMaterialName = "Mat_Skin_Warm";
                        profile.HairMaterialName = "Mat_Hair_Brown";
                        profile.TopMaterialName = "Mat_Clothes_Blue";
                        profile.BottomMaterialName = "Mat_Pants_Khaki";
                        profile.ShoeMaterialName = "Mat_Shoes_Dark";
                        break;

                    case CustomerVariationType.Customer_B:
                        profile.Gender = CustomerGender.Female;
                        profile.AgeGroup = CustomerAgeGroup.YoungAdult;
                        profile.SkinTone = CustomerSkinTone.Olive;
                        profile.HairStyle = CustomerHairStyle.Ponytail;
                        profile.HairColor = CustomerHairColor.Black;
                        profile.TopType = CustomerTopType.Blouse;
                        profile.BottomType = CustomerBottomType.Jeans;
                        profile.ShoeType = CustomerShoeType.Sneakers;
                        profile.SkinMaterialName = "Mat_Skin_Olive";
                        profile.HairMaterialName = "Mat_Hair_Black";
                        profile.TopMaterialName = "Mat_Clothes_Teal";
                        profile.BottomMaterialName = "Mat_Pants_Navy";
                        profile.ShoeMaterialName = "Mat_Shoes_White";
                        break;

                    case CustomerVariationType.Customer_C:
                        profile.Gender = CustomerGender.Male;
                        profile.AgeGroup = CustomerAgeGroup.MiddleAdult;
                        profile.SkinTone = CustomerSkinTone.Fair;
                        profile.HairStyle = CustomerHairStyle.Medium;
                        profile.HairColor = CustomerHairColor.DarkBlonde;
                        profile.TopType = CustomerTopType.Shirt;
                        profile.BottomType = CustomerBottomType.CasualPants;
                        profile.ShoeType = CustomerShoeType.CasualShoes;
                        profile.SkinMaterialName = "Mat_Skin_Fair";
                        profile.HairMaterialName = "Mat_Hair_Blonde";
                        profile.TopMaterialName = "Mat_Clothes_Maroon";
                        profile.BottomMaterialName = "Mat_Pants_DarkGrey";
                        profile.ShoeMaterialName = "Mat_Shoes_Dark";
                        profile.HasGlasses = true;
                        break;
                }

                _visualController.ApplyProfile(profile);
                return;
            }

            // Fallback for procedural rig without CustomerVisualController
            Material skinMat = null;
            Material hairMat = null;
            Material clothesMat = null;
            Material pantsMat = null;
            Material shoeMat = null;

            switch (variation)
            {
                case CustomerVariationType.Customer_A:
                    skinMat = LoadMat("Mat_Skin_Warm");
                    hairMat = LoadMat("Mat_Hair_Brown");
                    clothesMat = LoadMat("Mat_Clothes_Blue");
                    pantsMat = LoadMat("Mat_Pants_Khaki");
                    shoeMat = LoadMat("Mat_Shoes_Dark");
                    break;

                case CustomerVariationType.Customer_B:
                    skinMat = LoadMat("Mat_Skin_Olive");
                    hairMat = LoadMat("Mat_Hair_Black");
                    clothesMat = LoadMat("Mat_Clothes_Teal");
                    pantsMat = LoadMat("Mat_Pants_Navy");
                    shoeMat = LoadMat("Mat_Shoes_White");
                    break;

                case CustomerVariationType.Customer_C:
                    skinMat = LoadMat("Mat_Skin_Fair");
                    hairMat = LoadMat("Mat_Hair_Blonde");
                    clothesMat = LoadMat("Mat_Clothes_Maroon");
                    pantsMat = LoadMat("Mat_Pants_DarkGrey");
                    shoeMat = LoadMat("Mat_Shoes_Dark");
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
            Material m = Resources.Load<Material>(name);
#if UNITY_EDITOR
            if (m == null)
            {
                m = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>($"Assets/_Project/Art/Materials/{name}.mat");
            }
#endif
            return m;
        }

        public void SetBasketVisible(bool visible)
        {
            if (_visualController != null)
            {
                _visualController.SetBasketVisible(visible);
            }
            else if (_shoppingBasketObject != null)
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
                case CustomerState.WaitingInQueue: return new Color(1f, 0.5f, 0f);
                case CustomerState.CheckingOut: return Color.green;
                case CustomerState.Leaving: return Color.cyan;
                case CustomerState.Stuck: return Color.red;
                default: return Color.white;
            }
        }

        public void BuildProceduralHumanoidRig(Material defaultSkin, Material defaultHair, Material defaultClothes, Material defaultPants, Material defaultShoes, Material basketMat)
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            GameObject modelRoot = new GameObject("Humanoid_Model");
            modelRoot.transform.parent = transform;
            modelRoot.transform.localPosition = Vector3.zero;
            modelRoot.transform.localRotation = Quaternion.identity;

            GameObject hips = new GameObject("Hips");
            hips.transform.parent = modelRoot.transform;
            hips.transform.localPosition = new Vector3(0f, 0.85f, 0f);

            GameObject torsoObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torsoObj.name = "Torso";
            torsoObj.transform.parent = hips.transform;
            torsoObj.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            torsoObj.transform.localScale = new Vector3(0.42f, 0.55f, 0.24f);
            DestroyImmediate(torsoObj.GetComponent<Collider>());
            _torsoRenderer = torsoObj.GetComponent<MeshRenderer>();
            _torsoRenderer.sharedMaterial = defaultClothes;
            _torsoTransform = torsoObj.transform;

            GameObject headObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            headObj.name = "Head";
            headObj.transform.parent = torsoObj.transform;
            headObj.transform.localPosition = new Vector3(0f, 0.62f, 0.02f);
            headObj.transform.localScale = new Vector3(0.6f, 0.52f, 0.7f);
            DestroyImmediate(headObj.GetComponent<Collider>());
            _headRenderer = headObj.GetComponent<MeshRenderer>();
            _headRenderer.sharedMaterial = defaultSkin;
            _headTransform = headObj.transform;

            GameObject hairObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hairObj.name = "Hair";
            hairObj.transform.parent = headObj.transform;
            hairObj.transform.localPosition = new Vector3(0f, 0.28f, -0.05f);
            hairObj.transform.localScale = new Vector3(1.05f, 0.65f, 1.05f);
            DestroyImmediate(hairObj.GetComponent<Collider>());
            _hairRenderer = hairObj.GetComponent<MeshRenderer>();
            _hairRenderer.sharedMaterial = defaultHair;

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

            GameObject lLegPivot = new GameObject("Left_Leg_Pivot");
            lLegPivot.transform.parent = hips.transform;
            lLegPivot.transform.localPosition = new Vector3(-0.16f, 0f, 0f);
            _leftLegTransform = lLegPivot.transform;

            GameObject lLegMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lLegMesh.name = "Left_Leg_Mesh";
            lLegMesh.transform.parent = lLegPivot.transform;
            lLegMesh.transform.localPosition = new Vector3(0f, -0.42f, 0f);
            lLegMesh.transform.localScale = new Vector3(0.26f, 0.42f, 0.26f);
            DestroyImmediate(lLegMesh.GetComponent<Collider>());
            _leftLegRenderer = lLegMesh.GetComponent<MeshRenderer>();
            _leftLegRenderer.sharedMaterial = defaultPants;

            GameObject lShoe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lShoe.name = "Left_Shoe";
            lShoe.transform.parent = lLegPivot.transform;
            lShoe.transform.localPosition = new Vector3(0f, -0.82f, 0.06f);
            lShoe.transform.localScale = new Vector3(0.26f, 0.12f, 0.36f);
            DestroyImmediate(lShoe.GetComponent<Collider>());
            _leftShoeRenderer = lShoe.GetComponent<MeshRenderer>();
            _leftShoeRenderer.sharedMaterial = defaultShoes;

            GameObject rLegPivot = new GameObject("Right_Leg_Pivot");
            rLegPivot.transform.parent = hips.transform;
            rLegPivot.transform.localPosition = new Vector3(0.16f, 0f, 0f);
            _rightLegTransform = rLegPivot.transform;

            GameObject rLegMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rLegMesh.name = "Right_Leg_Mesh";
            rLegMesh.transform.parent = rLegPivot.transform;
            rLegMesh.transform.localPosition = new Vector3(0f, -0.42f, 0f);
            rLegMesh.transform.localScale = new Vector3(0.26f, 0.42f, 0.26f);
            DestroyImmediate(rLegMesh.GetComponent<Collider>());
            _rightLegRenderer = rLegMesh.GetComponent<MeshRenderer>();
            _rightLegRenderer.sharedMaterial = defaultPants;

            GameObject rShoe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rShoe.name = "Right_Shoe";
            rShoe.transform.parent = rLegPivot.transform;
            rShoe.transform.localPosition = new Vector3(0f, -0.82f, 0.06f);
            rShoe.transform.localScale = new Vector3(0.26f, 0.12f, 0.36f);
            DestroyImmediate(rShoe.GetComponent<Collider>());
            _rightShoeRenderer = rShoe.GetComponent<MeshRenderer>();
            _rightShoeRenderer.sharedMaterial = defaultShoes;

            GameObject basket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basket.name = "ShoppingBasket";
            basket.transform.parent = rArmPivot.transform;
            basket.transform.localPosition = new Vector3(0f, -0.5f, 0.15f);
            basket.transform.localScale = new Vector3(0.35f, 0.25f, 0.45f);
            DestroyImmediate(basket.GetComponent<Collider>());
            var basketRend = basket.GetComponent<MeshRenderer>();
            basketRend.sharedMaterial = basketMat;
            _shoppingBasketObject = basket;
            _shoppingBasketObject.SetActive(false);

            GameObject badge = new GameObject("DebugBadge");
            badge.transform.parent = transform;
            badge.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            _debugBadgeRoot = badge;

            GameObject textObj = new GameObject("BadgeText");
            textObj.transform.parent = badge.transform;
            textObj.transform.localPosition = Vector3.zero;
            _debugTextMesh = textObj.AddComponent<TextMesh>();
            _debugTextMesh.characterSize = 0.12f;
            _debugTextMesh.fontSize = 24;
            _debugTextMesh.alignment = TextAlignment.Center;
            _debugTextMesh.anchor = TextAnchor.MiddleCenter;
            _debugTextMesh.text = "IDLE";
            _debugBadgeRoot.SetActive(false);
        }
    }
}

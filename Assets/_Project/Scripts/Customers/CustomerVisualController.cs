using System.Collections.Generic;
using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Controls the modular realistic 3D mobile customer character model.
    /// Manages head, anatomical facial features, iris colors, hairstyle variants, tops, bottoms,
    /// shoes, accessories (glasses, cap, backpack, handbag, shopping bag, shopping basket),
    /// LOD levels (LOD0, LOD1, LOD2), anatomical morphology, and material application.
    /// </summary>
    public class CustomerVisualController : MonoBehaviour
    {
        [Header("Modular Roots")]
        [SerializeField] private GameObject _modelRoot;
        [SerializeField] private GameObject _lod0Root;
        [SerializeField] private GameObject _lod1Root;
        [SerializeField] private GameObject _lod2Root;
        [SerializeField] private LODGroup _lodGroup;

        [Header("Head & Face Renderers")]
        [SerializeField] private MeshRenderer _headRenderer;
        [SerializeField] private MeshRenderer _jawRenderer;
        [SerializeField] private MeshRenderer _leftEyeRenderer;
        [SerializeField] private MeshRenderer _rightEyeRenderer;
        [SerializeField] private MeshRenderer _leftIrisRenderer;
        [SerializeField] private MeshRenderer _rightIrisRenderer;
        [SerializeField] private MeshRenderer _leftBrowRenderer;
        [SerializeField] private MeshRenderer _rightBrowRenderer;
        [SerializeField] private MeshRenderer _noseRenderer;
        [SerializeField] private MeshRenderer _mouthRenderer;
        [SerializeField] private MeshRenderer _leftEarRenderer;
        [SerializeField] private MeshRenderer _rightEarRenderer;

        [Header("Hairstyle Variants")]
        [SerializeField] private GameObject _hairShortObj;
        [SerializeField] private GameObject _hairFadeObj;
        [SerializeField] private GameObject _hairMediumObj;
        [SerializeField] private GameObject _hairLongObj;
        [SerializeField] private GameObject _hairPonytailObj;
        [SerializeField] private GameObject _hairMessyObj;
        [SerializeField] private GameObject _hairBuzzObj;
        [SerializeField] private List<MeshRenderer> _allHairRenderers = new List<MeshRenderer>();

        [Header("Clothing Renderers")]
        [SerializeField] private MeshRenderer _torsoRenderer;
        [SerializeField] private MeshRenderer _leftArmRenderer;
        [SerializeField] private MeshRenderer _rightArmRenderer;
        [SerializeField] private MeshRenderer _leftHandRenderer;
        [SerializeField] private MeshRenderer _rightHandRenderer;
        [SerializeField] private MeshRenderer _leftLegRenderer;
        [SerializeField] private MeshRenderer _rightLegRenderer;
        [SerializeField] private MeshRenderer _leftShoeRenderer;
        [SerializeField] private MeshRenderer _rightShoeRenderer;

        [Header("Accessories")]
        [SerializeField] private GameObject _shoppingBasketObject;
        [SerializeField] private GameObject _glassesObject;
        [SerializeField] private GameObject _capObject;
        [SerializeField] private GameObject _backpackObject;
        [SerializeField] private GameObject _handbagObject;
        [SerializeField] private GameObject _shoppingBagObject;

        [Header("Articulated Transform Hierarchy")]
        [SerializeField] private Transform _torsoTransform;
        [SerializeField] private Transform _headTransform;
        [SerializeField] private Transform _leftArmTransform;
        [SerializeField] private Transform _rightArmTransform;
        [SerializeField] private Transform _leftLegTransform;
        [SerializeField] private Transform _rightLegTransform;

        private CustomerVisualProfile _activeProfile;
        private static readonly Dictionary<string, Material> MaterialCache = new Dictionary<string, Material>();

        public CustomerVisualProfile ActiveProfile => _activeProfile;
        public Transform TorsoTransform => _torsoTransform;
        public Transform HeadTransform => _headTransform;
        public Transform LeftArmTransform => _leftArmTransform;
        public Transform RightArmTransform => _rightArmTransform;
        public Transform LeftLegTransform => _leftLegTransform;
        public Transform RightLegTransform => _rightLegTransform;
        public LODGroup CharacterLODGroup => _lodGroup;

        public GameObject GlassesObject => _glassesObject;
        public GameObject CapObject => _capObject;
        public GameObject BackpackObject => _backpackObject;
        public GameObject HandbagObject => _handbagObject;
        public GameObject ShoppingBagObject => _shoppingBagObject;
        public GameObject ShoppingBasketObject => _shoppingBasketObject;

        public void ApplyProfile(CustomerVisualProfile profile)
        {
            if (profile == null) return;
            _activeProfile = profile;

            // 1. Resolve Materials
            Material skinMat = ResolveMaterial(profile.SkinMaterialName, "Mat_Skin_Warm");
            Material hairMat = ResolveMaterial(profile.HairMaterialName, "Mat_Hair_Brown");
            Material irisMat = ResolveMaterial(profile.IrisMaterialName, "Mat_Eyes_Iris_Brown");
            Material topMat = ResolveMaterial(profile.TopMaterialName, "Mat_Clothes_Blue");
            Material bottomMat = ResolveMaterial(profile.BottomMaterialName, "Mat_Pants_Khaki");
            Material shoeMat = ResolveMaterial(profile.ShoeMaterialName, "Mat_Shoes_Dark");

            // 2. Apply Skin Material to Head, Jaw, Face, Ears, Hands
            if (skinMat != null)
            {
                if (_headRenderer != null) _headRenderer.sharedMaterial = skinMat;
                if (_jawRenderer != null) _jawRenderer.sharedMaterial = skinMat;
                if (_noseRenderer != null) _noseRenderer.sharedMaterial = skinMat;
                if (_leftEarRenderer != null) _leftEarRenderer.sharedMaterial = skinMat;
                if (_rightEarRenderer != null) _rightEarRenderer.sharedMaterial = skinMat;
                if (_leftHandRenderer != null) _leftHandRenderer.sharedMaterial = skinMat;
                if (_rightHandRenderer != null) _rightHandRenderer.sharedMaterial = skinMat;
            }

            // 3. Eyebrows & Hair Material
            if (hairMat != null)
            {
                if (_leftBrowRenderer != null) _leftBrowRenderer.sharedMaterial = hairMat;
                if (_rightBrowRenderer != null) _rightBrowRenderer.sharedMaterial = hairMat;
                for (int i = 0; i < _allHairRenderers.Count; i++)
                {
                    if (_allHairRenderers[i] != null) _allHairRenderers[i].sharedMaterial = hairMat;
                }
            }

            // 4. Iris Material
            if (irisMat != null)
            {
                if (_leftIrisRenderer != null) _leftIrisRenderer.sharedMaterial = irisMat;
                if (_rightIrisRenderer != null) _rightIrisRenderer.sharedMaterial = irisMat;
                // Fallback if iris renderers aren't separate objects
                if (_leftIrisRenderer == null && _leftEyeRenderer != null) _leftEyeRenderer.sharedMaterial = irisMat;
                if (_rightIrisRenderer == null && _rightEyeRenderer != null) _rightEyeRenderer.sharedMaterial = irisMat;
            }

            // 5. Hair Style Variant Selection
            ActivateHairStyle(profile.HairStyle);

            // 6. Clothing Materials
            if (topMat != null)
            {
                if (_torsoRenderer != null) _torsoRenderer.sharedMaterial = topMat;
                if (_leftArmRenderer != null) _leftArmRenderer.sharedMaterial = topMat;
                if (_rightArmRenderer != null) _rightArmRenderer.sharedMaterial = topMat;
            }

            if (bottomMat != null)
            {
                if (_leftLegRenderer != null) _leftLegRenderer.sharedMaterial = bottomMat;
                if (_rightLegRenderer != null) _rightLegRenderer.sharedMaterial = bottomMat;
            }

            if (shoeMat != null)
            {
                if (_leftShoeRenderer != null) _leftShoeRenderer.sharedMaterial = shoeMat;
                if (_rightShoeRenderer != null) _rightShoeRenderer.sharedMaterial = shoeMat;
            }

            // 7. Accessories
            if (_glassesObject != null) _glassesObject.SetActive(profile.HasGlasses);
            if (_capObject != null) _capObject.SetActive(profile.HasCap);
            if (_backpackObject != null) _backpackObject.SetActive(profile.HasBackpack);
            if (_handbagObject != null) _handbagObject.SetActive(profile.HasHandbag);
            if (_shoppingBagObject != null) _shoppingBagObject.SetActive(profile.HasShoppingBag);

            SetBasketVisible(profile.HasShoppingBasket);

            // 8. Morphological adjustments (Gender, Age, Height, Body Build)
            ApplyMorphology(profile);
        }

        private void ActivateHairStyle(CustomerHairStyle style)
        {
            if (_hairShortObj != null) _hairShortObj.SetActive(style == CustomerHairStyle.Short);
            if (_hairFadeObj != null) _hairFadeObj.SetActive(style == CustomerHairStyle.Fade);
            if (_hairMediumObj != null) _hairMediumObj.SetActive(style == CustomerHairStyle.Medium);
            if (_hairLongObj != null) _hairLongObj.SetActive(style == CustomerHairStyle.Long);
            if (_hairPonytailObj != null) _hairPonytailObj.SetActive(style == CustomerHairStyle.Ponytail);
            if (_hairMessyObj != null) _hairMessyObj.SetActive(style == CustomerHairStyle.Messy);
            if (_hairBuzzObj != null) _hairBuzzObj.SetActive(style == CustomerHairStyle.Buzz);
        }

        private CustomerModularSlot _headSlot;
        private CustomerModularSlot _hairSlot;
        private CustomerModularSlot _topSlot;
        private CustomerModularSlot _bottomSlot;
        private CustomerModularSlot _shoesSlot;
        private CustomerModularSlot _accessorySlot;

        public CustomerModularSlot GetModularSlot(VisualSlotType slotType)
        {
            EnsureModularSlotsInitialized();
            switch (slotType)
            {
                case VisualSlotType.Head: return _headSlot;
                case VisualSlotType.Hair: return _hairSlot;
                case VisualSlotType.Top: return _topSlot;
                case VisualSlotType.Bottom: return _bottomSlot;
                case VisualSlotType.Shoes: return _shoesSlot;
                case VisualSlotType.Accessory: return _accessorySlot;
                case VisualSlotType.Body:
                default:
                    return _topSlot;
            }
        }

        private void EnsureModularSlotsInitialized()
        {
            if (_headSlot == null)
            {
                _headSlot = new CustomerModularSlot(VisualSlotType.Head, "HeadSlot", _headTransform);
                _headSlot.BindRenderers(_headRenderer, _jawRenderer, _noseRenderer, _mouthRenderer, _leftEarRenderer, _rightEarRenderer, _leftEyeRenderer, _rightEyeRenderer, _leftIrisRenderer, _rightIrisRenderer);
            }
            if (_hairSlot == null)
            {
                _hairSlot = new CustomerModularSlot(VisualSlotType.Hair, "HairSlot", _headTransform);
                if (_allHairRenderers != null) _hairSlot.BindRenderers(_allHairRenderers.ToArray());
            }
            if (_topSlot == null)
            {
                _topSlot = new CustomerModularSlot(VisualSlotType.Top, "TopSlot", _torsoTransform);
                _topSlot.BindRenderers(_torsoRenderer, _leftArmRenderer, _rightArmRenderer);
            }
            if (_bottomSlot == null)
            {
                _bottomSlot = new CustomerModularSlot(VisualSlotType.Bottom, "BottomSlot", _torsoTransform);
                _bottomSlot.BindRenderers(_leftLegRenderer, _rightLegRenderer);
            }
            if (_shoesSlot == null)
            {
                _shoesSlot = new CustomerModularSlot(VisualSlotType.Shoes, "ShoesSlot", _torsoTransform);
                _shoesSlot.BindRenderers(_leftShoeRenderer, _rightShoeRenderer);
            }
            if (_accessorySlot == null)
            {
                _accessorySlot = new CustomerModularSlot(VisualSlotType.Accessory, "AccessorySlot", _torsoTransform);
            }
        }

        public void ClearAllModularSlots()
        {
            if (_headSlot != null) _headSlot.Clear();
            if (_hairSlot != null) _hairSlot.Clear();
            if (_topSlot != null) _topSlot.Clear();
            if (_bottomSlot != null) _bottomSlot.Clear();
            if (_shoesSlot != null) _shoesSlot.Clear();
            if (_accessorySlot != null) _accessorySlot.Clear();
        }

        private void ApplyMorphology(CustomerVisualProfile profile)
        {
            if (_torsoTransform == null) return;

            float widthMult = Mathf.Clamp(profile.BodyWidthMultiplier, 0.90f, 1.10f);
            float heightMult = Mathf.Clamp(profile.HeightMultiplier, 0.90f, 1.10f);
            float headMult = Mathf.Clamp(profile.HeadScale, 0.95f, 1.05f);

            // Body type morphology scaling (Aşama 11)
            float bodyTypeWidth = 1.0f;
            float bodyTypeDepth = 1.0f;
            float armSpacingMult = 1.0f;
            float legSpacingMult = 1.0f;

            switch (profile.BodyType)
            {
                case CustomerBodyType.Slim:
                    bodyTypeWidth = 0.94f;
                    bodyTypeDepth = 0.94f;
                    armSpacingMult = 0.96f;
                    legSpacingMult = 0.97f;
                    break;
                case CustomerBodyType.Average:
                    bodyTypeWidth = 1.00f;
                    bodyTypeDepth = 1.00f;
                    armSpacingMult = 1.00f;
                    legSpacingMult = 1.00f;
                    break;
                case CustomerBodyType.Athletic:
                    bodyTypeWidth = 1.05f;
                    bodyTypeDepth = 1.02f;
                    armSpacingMult = 1.06f;
                    legSpacingMult = 1.01f;
                    break;
                case CustomerBodyType.Heavy:
                    bodyTypeWidth = 1.08f;
                    bodyTypeDepth = 1.08f;
                    armSpacingMult = 1.08f;
                    legSpacingMult = 1.05f;
                    break;
            }

            float finalWidthMult = widthMult * bodyTypeWidth;
            float finalDepthMult = widthMult * bodyTypeDepth;

            // Natural sexual dimorphism in shoulder width and hip-to-waist ratio
            if (profile.Gender == CustomerGender.Female)
            {
                _torsoTransform.localScale = new Vector3(0.36f * finalWidthMult, 0.54f, 0.22f * finalDepthMult);
                if (_leftArmTransform != null) _leftArmTransform.localPosition = new Vector3(-0.48f * finalWidthMult * armSpacingMult, 0.18f, 0f);
                if (_rightArmTransform != null) _rightArmTransform.localPosition = new Vector3(0.48f * finalWidthMult * armSpacingMult, 0.18f, 0f);
                if (_leftLegTransform != null) _leftLegTransform.localPosition = new Vector3(-0.11f * legSpacingMult, -0.42f, 0f);
                if (_rightLegTransform != null) _rightLegTransform.localPosition = new Vector3(0.11f * legSpacingMult, -0.42f, 0f);
            }
            else
            {
                _torsoTransform.localScale = new Vector3(0.44f * finalWidthMult, 0.56f, 0.25f * finalDepthMult);
                if (_leftArmTransform != null) _leftArmTransform.localPosition = new Vector3(-0.56f * finalWidthMult * armSpacingMult, 0.20f, 0f);
                if (_rightArmTransform != null) _rightArmTransform.localPosition = new Vector3(0.56f * finalWidthMult * armSpacingMult, 0.20f, 0f);
                if (_leftLegTransform != null) _leftLegTransform.localPosition = new Vector3(-0.13f * legSpacingMult, -0.42f, 0f);
                if (_rightLegTransform != null) _rightLegTransform.localPosition = new Vector3(0.13f * legSpacingMult, -0.42f, 0f);
            }

            // Head scale
            if (_headTransform != null)
            {
                _headTransform.localScale = Vector3.one * headMult;
            }

            // Age scale adjustment: subtle height / posture variation
            float ageHeightMod = 1.0f;
            switch (profile.AgeGroup)
            {
                case CustomerAgeGroup.YoungAdult:
                    ageHeightMod = 1.01f;
                    break;
                case CustomerAgeGroup.Adult:
                    ageHeightMod = 1.00f;
                    break;
                case CustomerAgeGroup.MiddleAged:
                    ageHeightMod = 0.99f;
                    break;
                case CustomerAgeGroup.Senior:
                    ageHeightMod = 0.975f;
                    break;
            }

            if (_modelRoot != null)
            {
                _modelRoot.transform.localScale = new Vector3(finalWidthMult, heightMult * ageHeightMod, finalWidthMult);
            }
        }

        public void SetBasketVisible(bool visible)
        {
            if (_shoppingBasketObject != null)
            {
                _shoppingBasketObject.SetActive(visible);
            }
        }

        public void ResetVisuals()
        {
            _activeProfile = null;
            SetBasketVisible(false);

            if (_hairShortObj != null) _hairShortObj.SetActive(false);
            if (_hairFadeObj != null) _hairFadeObj.SetActive(false);
            if (_hairMediumObj != null) _hairMediumObj.SetActive(false);
            if (_hairLongObj != null) _hairLongObj.SetActive(false);
            if (_hairPonytailObj != null) _hairPonytailObj.SetActive(false);
            if (_hairMessyObj != null) _hairMessyObj.SetActive(false);
            if (_hairBuzzObj != null) _hairBuzzObj.SetActive(false);

            if (_glassesObject != null) _glassesObject.SetActive(false);
            if (_capObject != null) _capObject.SetActive(false);
            if (_backpackObject != null) _backpackObject.SetActive(false);
            if (_handbagObject != null) _handbagObject.SetActive(false);
            if (_shoppingBagObject != null) _shoppingBagObject.SetActive(false);
            if (_shoppingBasketObject != null) _shoppingBasketObject.SetActive(false);

            ClearAllModularSlots();

            transform.localScale = Vector3.one;
            if (_modelRoot != null) _modelRoot.transform.localScale = Vector3.one;
            if (_headTransform != null) _headTransform.localScale = Vector3.one;
            if (_torsoTransform != null) _torsoTransform.localScale = new Vector3(0.42f, 0.54f, 0.24f);
            if (_leftArmTransform != null) _leftArmTransform.localScale = Vector3.one;
            if (_rightArmTransform != null) _rightArmTransform.localScale = Vector3.one;
            if (_leftLegTransform != null) _leftLegTransform.localScale = Vector3.one;
            if (_rightLegTransform != null) _rightLegTransform.localScale = Vector3.one;
        }

        private Material ResolveMaterial(string name, string fallback)
        {
            if (string.IsNullOrEmpty(name)) name = fallback;

            if (MaterialCache.TryGetValue(name, out Material cached) && cached != null)
            {
                return cached;
            }

            Material mat = Resources.Load<Material>(name);
#if UNITY_EDITOR
            if (mat == null)
            {
                mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>($"Assets/_Project/Art/Materials/{name}.mat");
            }
#endif
            if (mat == null && !string.IsNullOrEmpty(fallback) && name != fallback)
            {
                mat = ResolveMaterial(fallback, null);
            }

            if (mat != null)
            {
                MaterialCache[name] = mat;
            }

            return mat;
        }

        public void AssignReferences(
            GameObject modelRoot, GameObject lod0, GameObject lod1, GameObject lod2, LODGroup lodGroup,
            MeshRenderer head, MeshRenderer lEye, MeshRenderer rEye, MeshRenderer lBrow, MeshRenderer rBrow,
            MeshRenderer nose, MeshRenderer mouth, MeshRenderer lEar, MeshRenderer rEar,
            GameObject hShort, GameObject hFade, GameObject hMedium, GameObject hLong, GameObject hPonytail,
            List<MeshRenderer> allHairs,
            MeshRenderer torso, MeshRenderer lArm, MeshRenderer rArm, MeshRenderer lHand, MeshRenderer rHand,
            MeshRenderer lLeg, MeshRenderer rLeg, MeshRenderer lShoe, MeshRenderer rShoe,
            GameObject basket, GameObject glasses,
            Transform tTorso, Transform tHead, Transform tLArm, Transform tRArm, Transform tLLeg, Transform tRLeg)
        {
            _modelRoot = modelRoot;
            _lod0Root = lod0;
            _lod1Root = lod1;
            _lod2Root = lod2;
            _lodGroup = lodGroup;

            _headRenderer = head;
            _leftEyeRenderer = lEye;
            _rightEyeRenderer = rEye;
            _leftBrowRenderer = lBrow;
            _rightBrowRenderer = rBrow;
            _noseRenderer = nose;
            _mouthRenderer = mouth;
            _leftEarRenderer = lEar;
            _rightEarRenderer = rEar;

            _hairShortObj = hShort;
            _hairFadeObj = hFade;
            _hairMediumObj = hMedium;
            _hairLongObj = hLong;
            _hairPonytailObj = hPonytail;
            _allHairRenderers = allHairs;

            _torsoRenderer = torso;
            _leftArmRenderer = lArm;
            _rightArmRenderer = rArm;
            _leftHandRenderer = lHand;
            _rightHandRenderer = rHand;
            _leftLegRenderer = lLeg;
            _rightLegRenderer = rLeg;
            _leftShoeRenderer = lShoe;
            _rightShoeRenderer = rShoe;

            _shoppingBasketObject = basket;
            _glassesObject = glasses;

            _torsoTransform = tTorso;
            _headTransform = tHead;
            _leftArmTransform = tLArm;
            _rightArmTransform = tRArm;
            _leftLegTransform = tLLeg;
            _rightLegTransform = tRLeg;
        }

        public void AssignExtendedReferences(
            MeshRenderer jaw,
            MeshRenderer lIris, MeshRenderer rIris,
            GameObject hMessy, GameObject hBuzz,
            GameObject cap, GameObject backpack, GameObject handbag, GameObject shoppingBag)
        {
            _jawRenderer = jaw;
            _leftIrisRenderer = lIris;
            _rightIrisRenderer = rIris;
            _hairMessyObj = hMessy;
            _hairBuzzObj = hBuzz;
            _capObject = cap;
            _backpackObject = backpack;
            _handbagObject = handbag;
            _shoppingBagObject = shoppingBag;
        }
    }
}

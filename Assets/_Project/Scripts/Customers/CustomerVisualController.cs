using System.Collections.Generic;
using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Controls the modular stylized-realistic 3D mobile customer character model.
    /// Manages head, facial features, hairstyle variants, tops, bottoms, shoes, accessories,
    /// LOD levels (LOD0, LOD1, LOD2), and material application.
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
        [SerializeField] private MeshRenderer _leftEyeRenderer;
        [SerializeField] private MeshRenderer _rightEyeRenderer;
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

        public void ApplyProfile(CustomerVisualProfile profile)
        {
            if (profile == null) return;
            _activeProfile = profile;

            // 1. Resolve Materials
            Material skinMat = ResolveMaterial(profile.SkinMaterialName, "Mat_Skin_Warm");
            Material hairMat = ResolveMaterial(profile.HairMaterialName, "Mat_Hair_Brown");
            Material topMat = ResolveMaterial(profile.TopMaterialName, "Mat_Clothes_Blue");
            Material bottomMat = ResolveMaterial(profile.BottomMaterialName, "Mat_Pants_Khaki");
            Material shoeMat = ResolveMaterial(profile.ShoeMaterialName, "Mat_Shoes_Dark");

            // 2. Apply Skin Material to Head, Face, Ears, Hands
            if (skinMat != null)
            {
                if (_headRenderer != null) _headRenderer.sharedMaterial = skinMat;
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

            // 4. Hair Style Variant Selection
            ActivateHairStyle(profile.HairStyle);

            // 5. Clothing Materials
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

            // 6. Accessories
            if (_glassesObject != null)
            {
                _glassesObject.SetActive(profile.HasGlasses);
            }

            SetBasketVisible(profile.HasShoppingBasket);

            // 7. Morphological adjustments (Gender & Age)
            ApplyMorphology(profile);
        }

        private void ActivateHairStyle(CustomerHairStyle style)
        {
            if (_hairShortObj != null) _hairShortObj.SetActive(style == CustomerHairStyle.Short);
            if (_hairFadeObj != null) _hairFadeObj.SetActive(style == CustomerHairStyle.Fade);
            if (_hairMediumObj != null) _hairMediumObj.SetActive(style == CustomerHairStyle.Medium);
            if (_hairLongObj != null) _hairLongObj.SetActive(style == CustomerHairStyle.Long);
            if (_hairPonytailObj != null) _hairPonytailObj.SetActive(style == CustomerHairStyle.Ponytail);
        }

        private void ApplyMorphology(CustomerVisualProfile profile)
        {
            if (_torsoTransform == null) return;

            // Subtle sexual dimorphism in shoulder width and hip-to-waist ratio
            if (profile.Gender == CustomerGender.Female)
            {
                _torsoTransform.localScale = new Vector3(0.38f, 0.54f, 0.22f);
                if (_leftArmTransform != null) _leftArmTransform.localPosition = new Vector3(-0.52f, 0.18f, 0f);
                if (_rightArmTransform != null) _rightArmTransform.localPosition = new Vector3(0.52f, 0.18f, 0f);
            }
            else
            {
                _torsoTransform.localScale = new Vector3(0.44f, 0.56f, 0.25f);
                if (_leftArmTransform != null) _leftArmTransform.localPosition = new Vector3(-0.58f, 0.20f, 0f);
                if (_rightArmTransform != null) _rightArmTransform.localPosition = new Vector3(0.58f, 0.20f, 0f);
            }

            // Age scale adjustment: subtle height / posture variation
            float heightScale = 1.0f;
            switch (profile.AgeGroup)
            {
                case CustomerAgeGroup.YoungAdult:
                    heightScale = 1.02f;
                    break;
                case CustomerAgeGroup.Adult:
                    heightScale = 1.00f;
                    break;
                case CustomerAgeGroup.MiddleAdult:
                    heightScale = 0.98f;
                    break;
            }

            if (_modelRoot != null)
            {
                _modelRoot.transform.localScale = new Vector3(1f, heightScale, 1f);
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
            SetBasketVisible(false);
            if (_glassesObject != null) _glassesObject.SetActive(false);
            _activeProfile = null;
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
    }
}

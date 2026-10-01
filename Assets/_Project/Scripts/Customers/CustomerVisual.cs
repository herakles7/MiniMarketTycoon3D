using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Manages high-level customer visual presentation using low-poly Kenney Animated Characters.
    /// Provides human scale (~1.75m), character skins (Male, Female, Skater, Criminal, Cyborg),
    /// shopping basket display, and state debug badges.
    /// </summary>
    public class CustomerVisual : MonoBehaviour
    {
        [Header("Modular Visual Controller Reference")]
        [SerializeField] private CustomerVisualController _visualController;

        [Header("Character Model & Renderer")]
        [SerializeField] private Renderer _characterRenderer;
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
        public Renderer CharacterRenderer => _characterRenderer;
        public Transform TorsoTransform => _torsoTransform != null ? _torsoTransform : transform;
        public Transform HeadTransform => _headTransform != null ? _headTransform : transform;
        public Transform LeftArmTransform => _leftArmTransform != null ? _leftArmTransform : transform;
        public Transform RightArmTransform => _rightArmTransform != null ? _rightArmTransform : transform;
        public Transform LeftLegTransform => _leftLegTransform != null ? _leftLegTransform : transform;
        public Transform RightLegTransform => _rightLegTransform != null ? _rightLegTransform : transform;

        private void Awake()
        {
            if (_visualController == null)
            {
                _visualController = GetComponent<CustomerVisualController>();
            }

            // Purge old procedural primitive monsters (VisualRoot, Torso, etc.)
            Transform oldVisualRoot = transform.Find("VisualRoot");
            if (oldVisualRoot != null)
            {
                DestroyImmediate(oldVisualRoot.gameObject);
                _characterRenderer = null;
            }

            Transform modelChild = transform.Find("Model");
            if (modelChild == null)
            {
                SetupKenneyCharacterModel();
            }
            else
            {
                _characterRenderer = modelChild.GetComponentInChildren<SkinnedMeshRenderer>() as Renderer ?? modelChild.GetComponentInChildren<MeshRenderer>();
                DiscoverBones();
            }

            var anim = GetComponent<CustomerAnimationController>();
            if (anim != null && _torsoTransform != null)
            {
                anim.BindProceduralBones(_torsoTransform, _headTransform, _leftArmTransform, _rightArmTransform, _leftLegTransform, _rightLegTransform);
            }
        }

        public void DiscoverBones()
        {
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);
            foreach (var t in allChildren)
            {
                string lower = t.name.ToLower();
                if (lower.Contains("head")) _headTransform = t;
                else if (lower == "root" || lower.Contains("hips") || lower.Contains("spine") || lower.Contains("torso"))
                {
                    if (_torsoTransform == null) _torsoTransform = t;
                }
                else if (lower.Contains("leftarm") || lower.Contains("left_arm") || lower.Contains("leftshoulder") || lower.Contains("leftupperarm")) _leftArmTransform = t;
                else if (lower.Contains("rightarm") || lower.Contains("right_arm") || lower.Contains("rightshoulder") || lower.Contains("rightupperarm")) _rightArmTransform = t;
                else if (lower.Contains("leftleg") || lower.Contains("left_leg") || lower.Contains("leftupleg")) _leftLegTransform = t;
                else if (lower.Contains("rightleg") || lower.Contains("right_leg") || lower.Contains("rightupleg")) _rightLegTransform = t;
            }
        }

        public void SetupKenneyCharacterModel()
        {
            GameObject kenneyPrefab = LoadKenneyModel();
            if (kenneyPrefab != null)
            {
                GameObject model = Instantiate(kenneyPrefab, transform);
                model.name = "Model";
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                // Natural human scale: 3.765m FBX * 0.465f = 1.75m human height
                model.transform.localScale = Vector3.one * 0.465f;

                // Setup Animator and Controller
                Animator anim = model.GetComponent<Animator>();
                if (anim == null) anim = model.AddComponent<Animator>();
                var rac = LoadAnimatorController();
                if (rac != null)
                {
                    anim.runtimeAnimatorController = rac;
                }
                var avatar = LoadCharacterAvatar();
                if (avatar != null)
                {
                    anim.avatar = avatar;
                }
                anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                _characterRenderer = model.GetComponentInChildren<SkinnedMeshRenderer>() as Renderer ?? model.GetComponentInChildren<MeshRenderer>();
                Material defaultMat = LoadMat("Mat_Char_HumanMale");
                if (_characterRenderer != null && defaultMat != null)
                {
                    _characterRenderer.sharedMaterial = defaultMat;
                }

                DiscoverBones();
            }
        }

        private GameObject LoadKenneyModel()
        {
            GameObject go = Resources.Load<GameObject>("characterMedium");
#if UNITY_EDITOR
            if (go == null)
            {
                go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Characters/characterMedium.fbx");
            }
#endif
            return go;
        }

        private RuntimeAnimatorController LoadAnimatorController()
        {
            RuntimeAnimatorController rac = Resources.Load<RuntimeAnimatorController>("AC_Customer");
#if UNITY_EDITOR
            if (rac == null)
            {
                rac = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Art/Characters/AC_Customer.controller");
            }
#endif
            return rac;
        }

        private Avatar LoadCharacterAvatar()
        {
            GameObject go = LoadKenneyModel();
            if (go != null)
            {
                Animator a = go.GetComponent<Animator>();
                if (a != null && a.avatar != null) return a.avatar;
            }
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Avatar>("Assets/_Project/Art/Characters/characterMedium.fbx");
#else
            return null;
#endif
        }

        public void ApplyVisualProfile(CustomerVisualProfile profile)
        {
            if (_visualController != null)
            {
                _visualController.ApplyProfile(profile);
            }
            ApplyRandomVariation();
        }

        public void ApplyRandomVariation()
        {
            int count = System.Enum.GetValues(typeof(CustomerVariationType)).Length;
            CustomerVariationType randomType = (CustomerVariationType)UnityEngine.Random.Range(0, count);
            ApplyVariation(randomType);
        }

        public void ApplyVariation(CustomerVariationType variation)
        {
            _variationType = variation;

            if (_characterRenderer == null)
            {
                _characterRenderer = GetComponentInChildren<SkinnedMeshRenderer>() as Renderer ?? GetComponentInChildren<MeshRenderer>();
            }

            Material skinMat = null;
            switch (variation)
            {
                case CustomerVariationType.Customer_A:
                    skinMat = LoadMat("Mat_Char_HumanMale");
                    break;
                case CustomerVariationType.Customer_B:
                    skinMat = LoadMat("Mat_Char_HumanFemale");
                    break;
                case CustomerVariationType.Customer_C:
                    skinMat = LoadMat("Mat_Char_SkaterMale");
                    break;
                case CustomerVariationType.Customer_D:
                    skinMat = LoadMat("Mat_Char_SkaterFemale");
                    break;
                case CustomerVariationType.Customer_E:
                    skinMat = LoadMat("Mat_Char_CriminalMale");
                    break;
                case CustomerVariationType.Customer_F:
                    skinMat = LoadMat("Mat_Char_CyborgFemale");
                    break;
                case CustomerVariationType.Customer_G:
                    skinMat = LoadMat("Mat_Char_BusinessMale");
                    break;
                case CustomerVariationType.Customer_H:
                    skinMat = LoadMat("Mat_Char_BusinessFemale");
                    break;
                case CustomerVariationType.Customer_I:
                    skinMat = LoadMat("Mat_Char_Grandpa");
                    break;
                case CustomerVariationType.Customer_J:
                    skinMat = LoadMat("Mat_Char_Student");
                    break;
                case CustomerVariationType.Customer_K:
                    skinMat = LoadMat("Mat_Char_SurvivorFemale");
                    break;
                case CustomerVariationType.Customer_L:
                    skinMat = LoadMat("Mat_Char_SurvivorMale");
                    break;
                default:
                    skinMat = LoadMat("Mat_Char_HumanMale");
                    break;
            }

            if (_characterRenderer != null && skinMat != null)
            {
                _characterRenderer.sharedMaterial = skinMat;
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
                case CustomerState.WaitingInQueue: return new Color(1f, 0.5f, 0f);
                case CustomerState.CheckingOut: return Color.green;
                case CustomerState.Leaving: return Color.cyan;
                case CustomerState.Stuck: return Color.red;
                default: return Color.white;
            }
        }

        public void BuildProceduralHumanoidRig(Material defaultSkin, Material defaultHair, Material defaultClothes, Material defaultPants, Material defaultShoes, Material basketMat)
        {
            // Replaced procedural primitive assembly with Kenney character model
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
            SetupKenneyCharacterModel();
        }
    }
}

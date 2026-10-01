using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Enumerates distinct modular visual attachment slots for realistic mobile customer characters.
    /// Supports plug-and-play swapping of real rigged meshes, clothing, hair, and accessories (Aşama 11).
    /// </summary>
    public enum VisualSlotType
    {
        Body,
        Head,
        Hair,
        Top,
        Bottom,
        Shoes,
        Accessory
    }

    /// <summary>
    /// Represents an individual modular visual slot on a customer character.
    /// Encapsulates bone socket binding, active attachment, renderer collection, and material application.
    /// Provides anti-clipping scale and position offsets for seamless layer stacking.
    /// </summary>
    [Serializable]
    public class CustomerModularSlot
    {
        [SerializeField] private VisualSlotType _slotType;
        [SerializeField] private string _slotName;
        [SerializeField] private Transform _socketTransform;
        [SerializeField] private GameObject _activeAttachment;
        [SerializeField] private List<MeshRenderer> _slotRenderers = new List<MeshRenderer>();

        public VisualSlotType SlotType => _slotType;
        public string SlotName => _slotName;
        public Transform SocketTransform => _socketTransform;
        public GameObject ActiveAttachment => _activeAttachment;
        public IReadOnlyList<MeshRenderer> SlotRenderers => _slotRenderers;

        public CustomerModularSlot(VisualSlotType slotType, string slotName, Transform socket)
        {
            _slotType = slotType;
            _slotName = slotName;
            _socketTransform = socket;
        }

        public void BindRenderers(params MeshRenderer[] renderers)
        {
            if (_slotRenderers == null) _slotRenderers = new List<MeshRenderer>();
            _slotRenderers.Clear();
            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null) _slotRenderers.Add(renderers[i]);
                }
            }
        }

        public void SetAttachmentObject(GameObject attachment)
        {
            _activeAttachment = attachment;
        }

        public void SetActive(bool active)
        {
            if (_activeAttachment != null)
            {
                _activeAttachment.SetActive(active);
            }
            for (int i = 0; i < _slotRenderers.Count; i++)
            {
                if (_slotRenderers[i] != null)
                {
                    _slotRenderers[i].enabled = active;
                }
            }
        }

        public void ApplyMaterial(Material material)
        {
            if (material == null) return;
            for (int i = 0; i < _slotRenderers.Count; i++)
            {
                if (_slotRenderers[i] != null)
                {
                    _slotRenderers[i].sharedMaterial = material;
                }
            }
        }

        public void Clear()
        {
            if (_activeAttachment != null)
            {
                _activeAttachment.SetActive(false);
            }
        }
    }
}

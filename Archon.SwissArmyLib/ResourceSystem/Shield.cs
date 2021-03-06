﻿using System.Collections.Generic;
using Archon.SwissArmyLib.Events;
using Archon.SwissArmyLib.Utils.Editor;
using JetBrains.Annotations;
using UnityEngine;

namespace Archon.SwissArmyLib.ResourceSystem
{
    /// <summary>
    /// A resource pool that is used to protect another resource pool from getting drained. 
    /// The shield intercepts the event and applies some of the change to itself, only letting part (or none at all) of the change get through.
    /// 
    /// If the <see cref="ProtectedTarget"/> is not set, it will try to find a resource pool on the same GameObject.
    /// </summary>
    public class Shield : ResourcePool, IEventListener<IResourcePreChangeEvent>, IEventListener<IResourceEvent>
    {
        private static readonly List<ResourcePool> GetComponentResults = new List<ResourcePool>();
            
        [Tooltip("The target resource pool that should be protected.")]
        [SerializeField, ReadOnly(OnlyWhilePlaying = true)]
        private ResourcePool _protectedTarget;

        [Tooltip("Flat amount of removed resource that should be absorbed.")]
        [SerializeField]
        private float _absorptionFlat;

        [Tooltip("Fraction of removed resource that should be absorbed by the shield.")]
        [SerializeField, Range(0, 1)]
        private float _absorptionScaling = 0.5f;

        [Tooltip("Whether the shield should get drained when the target is empty.")]
        [SerializeField]
        private bool _emptiesWithTarget = true;

        [Tooltip("Whether the shield should renew when the target does.")]
        [SerializeField]
        private bool _renewsWithTarget = true;

        /// <summary>
        /// Gets or sets the target that this shield should protect.
        /// </summary>
        public ResourcePool ProtectedTarget
        {
            get { return _protectedTarget; }
            set
            {
                if (_protectedTarget != null && enabled)
                {
                    _protectedTarget.OnPreChange.RemoveListener(this);
                    _protectedTarget.OnEmpty.RemoveListener(this);
                }

                _protectedTarget = value;

                if (_protectedTarget != null && enabled)
                {
                    _protectedTarget.OnPreChange.AddListener(this);
                    _protectedTarget.OnEmpty.AddListener(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the flat amount of removed resource that should be absorbed by the shield.
        /// </summary>
        public float AbsorptionFlat
        {
            get { return _absorptionFlat; }
            set { _absorptionFlat = value; }
        }

        /// <summary>
        /// Gets or sets the fraction of removed resource that should be absorbed by the shield.
        /// </summary>
        public float AbsorptionScaling
        {
            get { return _absorptionScaling; }
            set { _absorptionScaling = value; }
        }

        /// <summary>
        /// Gets or sets whether the shield should get fully drained when the target is empty.
        /// </summary>
        public bool EmptiesWithTarget
        {
            get { return _emptiesWithTarget; }
            set { _emptiesWithTarget = value; }
        }

        /// <summary>
        /// Gets or sets whether the shield should renew when the target does.
        /// </summary>
        public bool RenewsWithTarget
        {
            get { return _renewsWithTarget; }
            set { _renewsWithTarget = value; }
        }

        /// <inheritdoc />
        [UsedImplicitly]
        protected override void Awake()
        {
            base.Awake();

            if (_protectedTarget == null)
                _protectedTarget = GetDefaultTarget();
        }

        /// <summary>
        /// Called when the MonoBehaviour is enabled.
        /// </summary>
        [UsedImplicitly]
        protected void OnEnable()
        {
            if (_protectedTarget != null)
            {
                _protectedTarget.OnPreChange.AddListener(this);
                _protectedTarget.OnEmpty.AddListener(this);
                _protectedTarget.OnRenew.AddListener(this);
            }
        }

        /// <summary>
        /// Called when the MonoBehaviour is disabled.
        /// </summary>
        [UsedImplicitly]
        protected void OnDisable()
        {
            if (_protectedTarget != null)
            {
                _protectedTarget.OnPreChange.RemoveListener(this);
                _protectedTarget.OnEmpty.RemoveListener(this);
                _protectedTarget.OnRenew.RemoveListener(this);
            }
        }

        /// <inheritdoc />
        protected override float Change(float delta, object source = null, object args = null, bool forced = false)
        {
            if (_emptiesWithTarget && ProtectedTarget.IsEmpty && !forced)
                return 0;

            return base.Change(delta, source, args, forced);
        }

        /// <inheritdoc />
        public void OnEvent(int eventId, IResourcePreChangeEvent args)
        {
            if (args.ModifiedDelta < 0 && !IsEmpty)
            {
                var absorbed = AbsorptionFlat;
                absorbed += -args.ModifiedDelta * AbsorptionScaling;
                absorbed = Mathf.Clamp(absorbed, 0, Mathf.Min(-args.ModifiedDelta, Current));

                args.ModifiedDelta += absorbed;

                Remove(absorbed, args.Source, args.Args);
            }
        }

        /// <inheritdoc />
        public void OnEvent(int eventId, IResourceEvent args)
        {
            switch (eventId)
            {
                case EventIds.Empty:
                    if (_emptiesWithTarget)
                        Empty(args.Source, args.Args, true);
                    return;
                case EventIds.Renew:
                    if (_renewsWithTarget)
                        Renew(args.Source, args.Args, true);
                    return;
            }
        }

        /// <summary>
        /// Attempts to find a different resource pool on this GameObject.
        /// </summary>
        /// <returns>The found pool, or null if none were found.</returns>
        private ResourcePool GetDefaultTarget()
        {
            GetComponentsInChildren(GetComponentResults);

            for (var i = 0; i < GetComponentResults.Count; i++)
            {
                var pool = GetComponentResults[i];
                if (pool != this)
                    return pool;
            }

            return null;
        }

    }
}

﻿using UnityEngine;

namespace Archon.SwissArmyLib.Gravity
{
    /// <summary>
    /// Makes this <see cref="GameObject"/>'s <see cref="Rigidbody"/> part of the gravitational system.
    /// 
    /// For 2D physic see <see cref="GravitationalEntity2D"/>.
    /// </summary>
    [AddComponentMenu("Archon/Gravity/GravitationalEntity")]
    [RequireComponent(typeof(Rigidbody))]
    public class GravitationalEntity : MonoBehaviour
    {
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            GravitationalSystem.Register(_rigidbody);
        }

        private void OnDisable()
        {
            GravitationalSystem.Unregister(_rigidbody);
        }
    }
}
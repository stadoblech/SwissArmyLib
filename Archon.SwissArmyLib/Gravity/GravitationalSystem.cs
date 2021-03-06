﻿using System.Collections.Generic;
using Archon.SwissArmyLib.Events;
using Archon.SwissArmyLib.Utils;
using UnityEngine;

namespace Archon.SwissArmyLib.Gravity
{
    /// <summary>
    /// A gravitational system to allow for a more flexible gravity instead of just a constant directional gravity.
    /// 
    /// Useful for planets, black holes, magnets etc.
    /// 
    /// Rigidbodies that should be affected should have the <see cref="GravitationalEntity"/> component (or <see cref="GravitationalEntity2D"/> if using 2d physics).
    /// 
    /// Add gravitational forces by implementing the <see cref="IGravitationalAffecter"/> interface and registering it in the system.
    /// See <see cref="SphericalGravitationalPoint"/> for a simple example implementation.
    /// 
    /// <remarks>You might want to set Unity's gravity to (0,0,0).</remarks>
    /// </summary>
    public class GravitationalSystem : IEventListener {

        private static readonly List<IGravitationalAffecter> Affecters = new List<IGravitationalAffecter>();
        private static readonly List<Rigidbody> Rigidbodies = new List<Rigidbody>();
        private static readonly List<Rigidbody2D> Rigidbodies2D = new List<Rigidbody2D>();

        static GravitationalSystem()
        {
            var instance = new GravitationalSystem();
            ServiceLocator.RegisterSingleton(instance);
            ServiceLocator.GlobalReset += () => ServiceLocator.RegisterSingleton(instance);
        }

        private GravitationalSystem()
        {
            ManagedUpdate.OnFixedUpdate.AddListener(this);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~GravitationalSystem()
        {
            ManagedUpdate.OnFixedUpdate.RemoveListener(this);
        }

        /// <summary>
        /// Registers a gravitational affecter to be part of the system.
        /// </summary>
        /// <param name="affecter">The affecter to register.</param>
        public static void Register(IGravitationalAffecter affecter)
        {
            Affecters.Add(affecter);
        }

        /// <summary>
        /// Registers a <see cref="Rigidbody"/> that should be affected by gravitational forces in this system.
        /// </summary>
        /// <param name="rigidbody">The rigidbody to register.</param>
        public static void Register(Rigidbody rigidbody)
        {
            Rigidbodies.Add(rigidbody);
        }

        /// <summary>
        /// Registers a <see cref="Rigidbody2D"/> that should be affected by gravitational forces in this system.
        /// </summary>
        /// <param name="rigidbody">The rigidbody to register.</param>
        public static void Register(Rigidbody2D rigidbody)
        {
            Rigidbodies2D.Add(rigidbody);
        }

        /// <summary>
        /// Unregisters a gravitational affecter from the system, so it no longer affects entities.
        /// </summary>
        /// <param name="affecter">The affecter to unregister.</param>
        public static void Unregister(IGravitationalAffecter affecter)
        {
            Affecters.Remove(affecter);
        }

        /// <summary>
        /// Unregisters a <see cref="Rigidbody"/> from the system, so it no longer is affected by gravitational forces in this system.
        /// </summary>
        /// <param name="rigidbody">The rigidbody to unregister.</param>
        public static void Unregister(Rigidbody rigidbody)
        {
            Rigidbodies.Remove(rigidbody);
        }

        /// <summary>
        /// Unregisters a <see cref="Rigidbody2D"/> from the system, so it no longer is affected by gravitational forces in this system.
        /// </summary>
        /// <param name="rigidbody">The rigidbody to unregister.</param>
        public static void Unregister(Rigidbody2D rigidbody)
        {
            Rigidbodies2D.Remove(rigidbody);
        }

        /// <summary>
        /// Gets the sum of all gravitational forces at a specific location.
        /// </summary>
        /// <param name="location">The location to test.</param>
        /// <returns>A vector representing the sum of gravitational force at <paramref name="location"/>.</returns>
        public Vector3 GetGravityAtPoint(Vector3 location)
        {
            var gravity = new Vector3();

            for (var i = 0; i < Affecters.Count; i++)
                gravity += Affecters[i].GetForceAt(location);

            return gravity;
        }

        void IEventListener.OnEvent(int eventId)
        {
            if (eventId != ManagedUpdate.EventIds.FixedUpdate)
                return;

            for (var i = 0; i < Rigidbodies.Count; i++)
            {
                var body = Rigidbodies[i];

                if (body.useGravity && !body.IsSleeping())
                {
                    var gravity = GetGravityAtPoint(body.position);
                    body.AddForce(gravity);
                }
            }

            for (var i = 0; i < Rigidbodies2D.Count; i++)
            {
                var body = Rigidbodies2D[i];

                if (body.simulated && body.gravityScale > 0 && body.IsAwake())
                {
                    var gravity = GetGravityAtPoint(body.position);
                    body.AddForce((Vector2)gravity * body.gravityScale);
                }
            }
        }
    }
}
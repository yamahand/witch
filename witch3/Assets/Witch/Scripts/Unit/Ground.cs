using UnityEngine;

namespace Witch.Unit
{
    public class Ground : MonoBehaviour
    {
        public bool OnGround { get; private set; }
        public float Friction { get; private set; }

        private Vector2 _normal;
        private PhysicsMaterial2D _material;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            EvaluteCollision(collision);
            RetrieveFriction(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            EvaluteCollision(collision);
            RetrieveFriction(collision);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            OnGround = false;
            Friction = 0.0f;
        }

        private void EvaluteCollision(Collision2D collision2D)
        {
            for (int i = 0; i < collision2D.contactCount; i++)
            {
                _normal = collision2D.GetContact(i).normal;
                OnGround |= _normal.y >= 0.9f;
            }
        }

        private void RetrieveFriction(Collision2D collision2D)
        {
            _material = collision2D.rigidbody.sharedMaterial;
            Friction = 0;
            if (_material != null)
            {
                Friction = _material.friction;
            }
        }
    }
}

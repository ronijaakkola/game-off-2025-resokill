using Game.CharacterPlayer;
using Pathfinding;
using UnityEngine;

namespace Game.EntityMovement
{
    public class MoveTowardsPlayer : MonoBehaviour
    {
        protected RichAI aiPath;

        protected Transform AttackTarget { get; set; }

        void Start()
        {
            AttackTarget = GameObject.FindAnyObjectByType<PlayerMovement>().transform;
            aiPath = GetComponent<RichAI>();
        }

        void Update()
        {
            aiPath.destination = AttackTarget.position;
        }
    }
}

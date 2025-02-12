using Ddd.Application;
using Ddd.Infrastructure;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Ddd.Domain
{
    public class AiNpc : AbstractEntity
    {
        public static Action<int> DeathEvent;

        [Header("Movement parameters")]
        [SerializeField] private float standardMovementSpeed = 2.5f;
        [SerializeField] private float acceleratedMovementSpeed = 4.2f;
        [SerializeField] private float changePositionTime = 0.2f;
        [SerializeField] private float moveDistance = 30f;
        [SerializeField] private float detectionRadius = 7f;

        private bool hasDied = false;
        private bool isMovingToPlayer = false;
        private bool isRunningCoroutine = false;

        [Header("GameObject")]
        [Inject] private DiContainer container;
        [SerializeField] private GameObject NPC;
        [SerializeField] private GameObject NPCMaterial;
        private NavMeshAgent navMeshAgent;
        [Inject(Id = "ExplosionGameobject")] private GameObject explosion;
        [Inject(Id = "RunNPCGameobject")] private GameObject runNPC;
        [Inject(Id = "GearGameobject")] private GameObject gearPrefab;
        [Inject(Id = "PlayerGameobject")] private GameObject player;

        private void Start()
        {
            InitializeNavMeshAgent();
            InvokeRepeating(nameof(MoveNpc), changePositionTime, changePositionTime);
        }

        private void InitializeNavMeshAgent()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.speed = standardMovementSpeed;
        }

        public override void GetDamage(float damage)
        {
            base.GetDamage(damage);
        }

        public override void OnDeath()
        {
            if (!hasDied)
            {
                StartCoroutine(EnableExplosion(0.61f));
                hasDied = true;
            }
        }

        public override void OnRevival()
        {
            base.OnRevival();
        }

        private Vector3 RandomNavSphere(float distance)
        {
            var randomDirection = UnityEngine.Random.insideUnitSphere * distance;
            randomDirection += transform.position;
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, distance, -1);
            return navHit.position;
        }

        private void Update()
        {
            if (isMovingToPlayer && !isRunningCoroutine)
            {
                StartCoroutine(EnableRun(0.6f));
            }
            if (!hasDied && Vector3.Distance(NPC.transform.position, player.transform.position) <= 2f)
            {
                player.GetComponent<IDamagable>().GetDamage(15);
                GetDamage(50);
                OnDeath();
                hasDied = true;
            }
        }

        private void MoveNpc()
        {
            if (CanSeePlayer())
            {
                AttackPlayer();
            }
            else if (!isMovingToPlayer)
            {
                Patrol();
            }
        }

        private void AttackPlayer()
        {
            navMeshAgent.SetDestination(player.transform.position);
            isMovingToPlayer = true;
            navMeshAgent.speed = acceleratedMovementSpeed;
        }

        private bool CanSeePlayer()
            => player != null && Vector3.Distance(transform.position, player.transform.position) <= detectionRadius;

        private void Patrol()
        {
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
            {
                navMeshAgent.SetDestination(RandomNavSphere(moveDistance));
                navMeshAgent.speed = standardMovementSpeed;
            }
        }

        private IEnumerator EnableExplosion(float duration)
        {
            NPCMaterial.SetActive(false);
            var newExplosion = Instantiate(explosion, NPC.transform.position, Quaternion.identity);
            var gear = Instantiate(gearPrefab, NPC.transform.position, Quaternion.identity);
            yield return new WaitForSeconds(duration);
            base.OnDeath();
            Destroy(newExplosion);
            DeathEvent?.Invoke(UnityEngine.Random.Range(6, 15));
        }

        private IEnumerator EnableRun(float duration)
        {
            if (!hasDied)
            {
                isRunningCoroutine = true;
                var prefabRun = Instantiate(runNPC, NPC.transform.position, NPC.transform.rotation);
                prefabRun.transform.Rotate(new Vector3(0f, 1f, 0f), 90f);
                yield return new WaitForSeconds(duration);
                isRunningCoroutine = false;
                Destroy(prefabRun);
            }
        }
    }
}
using Mirror;
using UnityEngine;

namespace Complete
{
    public class Item : NetworkBehaviour
    {

        [ServerCallback]
        public static void SpawnItem()
        {
            if (!FindObjectOfType<Item>())
            {
                var itemPrefab = FindObjectOfType<NetworkManagerTank>().itemPrefab;
                GameObject item = Instantiate(itemPrefab);

                NetworkServer.Spawn(item);
            }
        }

        private void Start()
        {
            if (isServer)
            {
                var positions = FindObjectOfType<NetworkManagerTank>().itemSpawnPosition;

                var tanks = FindObjectsOfType<TankManager>();
                var tankAI = FindObjectOfType<TankManagerAI>();

                var idx = Random.Range(0, positions.Length);

                while (true)
                {
                    idx += 1;
                    if (idx >= positions.Length) idx = 0;

                    bool valid = true;

                    var pos = positions[idx];
                    foreach (var tank in tanks)
                    {
                        if (Vector3.Distance(pos.position, tank.transform.position) <= 5) valid = false;
                    }
                    if (tankAI && Vector3.Distance(pos.position, tankAI.transform.position) <= 5) valid = false;

                    if (valid)
                    {
                        transform.position = pos.position;
                        return;
                    }
                }
            }
        }

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("enter");

            TankManager targetTank = other.GetComponent<TankManager>();
            if (targetTank)
            {
                targetTank.Boost();
            }
            else
            {
                TankManagerAI aiTargetTank = other.GetComponent<TankManagerAI>();
                if (aiTargetTank)
                {
                    aiTargetTank.Boost();
                }
            }

            // Destroy the shell.
            //Destroy (gameObject);
            NetworkServer.Destroy(gameObject);
        }
    }
}
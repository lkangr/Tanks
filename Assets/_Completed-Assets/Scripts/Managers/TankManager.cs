using Mirror;
using System;
using System.Collections;
using UnityEngine;

namespace Complete
{
    public class TankManager : NetworkBehaviour
    {
        // This class is to manage various settings on a tank.
        // It works with the GameManager class to control how the tanks behave
        // and whether or not players have control of their tank in the 
        // different phases of the game.

        public Color m_PlayerColor;                             // This is the color this tank will be tinted.
        public Transform m_SpawnPoint;                          // The position and direction the tank will have when it spawns.
        [HideInInspector] public int m_PlayerNumber = 1;            // This specifies which player this the manager for.
        [HideInInspector] public string m_ColoredPlayerText;    // A string that represents the player with their number colored to match their tank.
        [HideInInspector] public GameObject m_Instance;         // A reference to the instance of the tank when it is created.
        [HideInInspector] public int m_Wins;                    // The number of wins this player has so far.
        [HideInInspector] public TankHealth m_TankHealth;

        private TankMovement m_Movement;                        // Reference to tank's movement script, used to disable and enable control.
        private TankShooting m_Shooting;                        // Reference to tank's shooting script, used to disable and enable control.
        private GameObject m_CanvasGameObject;                  // Used to disable the world space UI during the Starting and Ending phases of each round.


        private void Update()
        {
            if (isLocalPlayer)
            {
                m_Movement.CallUpdate();
                m_Shooting.CallUpdate();
            }
        }


        private void OnEnable()
        {
            if (isLocalPlayer)
            {
                m_Movement.CallOnEnable();
                m_Shooting.CallOnEnable();
                m_TankHealth.CallOnEnable();
            }
        }


        private void OnDisable()
        {
            if (isLocalPlayer)
            {
                m_Movement.CallOnDisable();
            }
        }


        private void Start()
        {
            Setup();
            if (isLocalPlayer)
            {
                m_Movement.CallStart();
                m_Shooting.CallStart();
            }
        }


        private void FixedUpdate()
        {
            if (isLocalPlayer)
            {
                m_Movement.CallFixedUpdate();
            }
        }


        public void Setup ()
        {
            // Get references to the components.
            m_Movement = GetComponent<TankMovement> ();
            m_Shooting = GetComponent<TankShooting> ();
            m_TankHealth = GetComponent<TankHealth>();

            m_Movement.m_TankManager = this;
            m_Shooting.m_TankManager = this;
            m_TankHealth.m_TankManager = this;

            m_CanvasGameObject = GetComponentInChildren<Canvas> ().gameObject;

            //// Set the player numbers to be consistent across the scripts.
            //m_Movement.m_PlayerNumber = m_PlayerNumber;
            //m_Shooting.m_PlayerNumber = m_PlayerNumber;

            // Create a string using the correct color that says 'PLAYER 1' etc based on the tank's color and the player's number.
            m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

            // Get all of the renderers of the tank.
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer> ();

            // Go through all the renderers...
            for (int i = 0; i < renderers.Length; i++)
            {
                // ... set their material color to the color specific to this tank.
                renderers[i].material.color = m_PlayerColor;
            }
        }


        // Used during the phases of the game where the player shouldn't be able to control their tank.
        public void DisableControl ()
        {
            m_Movement.enabled = false;
            m_Shooting.enabled = false;

            m_CanvasGameObject.SetActive (false);
        }


        // Used during the phases of the game where the player should be able to control their tank.
        public void EnableControl ()
        {
            m_Movement.enabled = true;
            m_Shooting.enabled = true;

            m_CanvasGameObject.SetActive (true);
        }


        // Used at the start of each round to put the tank into it's default state.
        public IEnumerator Reset ()
        {
            transform.position = m_SpawnPoint.position;
            transform.rotation = m_SpawnPoint.rotation;

            gameObject.SetActive (false);
            yield return new WaitForSeconds(0.5f);
            gameObject.SetActive (true);
        }

        [Command]
        public void Fire(Vector3 velocity)
        {
            Rigidbody shellInstance =
                Instantiate(m_Shooting.m_Shell, m_Shooting.m_FireTransform.position, m_Shooting.m_FireTransform.rotation) as Rigidbody;

            // Set the shell's velocity to the launch force in the fire position's forward direction.
            shellInstance.velocity = velocity;

            NetworkServer.Spawn(shellInstance.gameObject);
        }

        //[ClientCallback]
        public void AddExplosionForce(float m_ExplosionForce, Vector3 position, float m_ExplosionRadius)
        {
            GetComponent<Rigidbody>().AddExplosionForce(m_ExplosionForce, position, m_ExplosionRadius);
        }

        //[ClientCallback]
        public void TakeDamage(float damage)
        {
            //m_TankHealth.TakeDamage(damage);

            // Reduce current health by the amount of damage done.
            m_TankHealth.m_CurrentHealth -= damage;

            // Change the UI elements appropriately.
            m_TankHealth.SetHealthUI();

            // If the current health is at or below zero and it has not yet been registered, call OnDeath.
            if (m_TankHealth.m_CurrentHealth <= 0f && !m_TankHealth.m_Dead)
            {
                m_TankHealth.OnDeath();
            }
        }
    }
}
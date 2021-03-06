﻿//Code adapted from https://unity3d.college/2019/04/28/unity3d-ai-with-state-machine-drones-and-lasers/  
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Boss : MonoBehaviour
{
    [Header("Pathfinding Variables")]
    public GameObject[] destinations;
    private State _currentState;
    public NavMeshAgent agent;
    private float follow_distance = 10f;
    private int set_path = 0;
    private float look_range = 25f;
    public float rotation_speed = 35;
    private GameObject player;
    float current_rotation;
    bool location_set = false;
    float stopping_rotation;
    float enemy_current_rotation;
    private float player_health = 0;
    public Animator swing;
    const float attackDelayReset = 2f;
    float attackDelay;
    public bool damage;

    [Header("Attacking Variables")]
    public PlayerHealth ph;

    //checks to see if the enemy has been attacked by a player weapon
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PlayerWeapon")
        {
            damage = true;
        }
    }

    private void Update()
    {
        swing = GameObject.Find("Handle").GetComponent<Animator>();
        player = GameObject.FindWithTag("Player");
        Vector3 enemy_location = destinations[set_path].transform.position;
        Vector3 current_location = transform.position;
        if(attackDelay > 0)
        {
            swing.SetBool("Attacking", false);
            attackDelay -= Time.deltaTime;
        }
        switch (_currentState)
        {
            //in this state the enemy walks from one marker to another, then searches for the player
            //will follow the player if they get too close
            case State.Patrol:
                {
                    agent.SetDestination(enemy_location);
                    float distance_to_player = Vector3.Distance(transform.position, player.transform.position);
                    if (current_location.x == enemy_location.x && current_location.z == enemy_location.z)
                    {
                        if (set_path == (destinations.Length-1))
                        {
                            set_path = -1;
                        }
                        set_path += 1;
                    }


                    if (distance_to_player <= follow_distance)
                    {
                        _currentState = State.Follow;
                    }

                    break;
                }
            //follows the player unless they go out of range of the enemy
            //if the player gets far enough away the enemy goes back to patrolling
            case State.Follow:
                {
                    float distance_to_player = Vector3.Distance(transform.position, player.transform.position);
                    Vector3 player_location = player.transform.position;
                    agent.SetDestination(player_location);

                    if (distance_to_player <= 4)
                    {
                        _currentState = State.Attack;
                    }

                    if (distance_to_player > (follow_distance * 2))
                    {
                        _currentState = State.Patrol;
                    }
                    break;

                }
            //placeholder for now, enemy attacks the player until one dies
            case State.Attack:
                {
                    if(attackDelay <= 0)
                    {
                        ph.playerHealth -= ph.damageTaken;
                        agent.SetDestination(current_location);
                        swing.SetBool("Attacking", true);
                        //player.SetActive(false);      
                        if (ph.playerHealth <= 0)
                        {
                            player.SetActive(false);
                            SceneManager.LoadScene("SCN_Menu_Defeat");
                        }
                        //_currentState = State.Retreat;
                        attackDelay = attackDelayReset;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            case State.Retreat:
                {
                    agent.SetDestination(enemy_location);
                    if (current_location.x == enemy_location.x && current_location.z == enemy_location.z)
                    {
                        _currentState = State.Patrol;
                    }
                    break;
                }
        }
    }

    public enum State
    {
        Patrol,
        Follow,
        Attack,
        Retreat
    }
}
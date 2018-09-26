﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Zomz/AI/Action/Chase Fast", fileName = "Action_ChaseFast_New")]
public class ChaseFastAction : Action
{
    GameObject _player;

    void OnEnable()
    {
        _player = GameObject.FindWithTag("Player");
    }

    public override void Act(AIStateController pController)
    {
        Chase(pController);
    }

    private void Chase(AIStateController pController)
    {
        if (pController.ChaseTarget == null)
        {
            _player = GameObject.FindWithTag("Player");
            pController.ChaseTarget = _player.transform;
            pController.navMeshAgent.speed = pController.CharacterStats.RunSpeed;
        }

        pController.navMeshAgent.destination = pController.ChaseTarget.position;
        pController.navMeshAgent.isStopped = false;
    }
}

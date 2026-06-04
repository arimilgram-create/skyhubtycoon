// Simple passenger state machine: entrance → check-in wait → security wait → waiting/gate → board and disappear.
using System.Collections.Generic;
using UnityEngine;
using SkyHubTycoon.Build;
using SkyHubTycoon.Data;

namespace SkyHubTycoon.Simulation
{
    public class PassengerAgent : MonoBehaviour
    {
        private enum PassengerStage
        {
            ToCheckIn,
            CheckInWait,
            ToSecurity,
            SecurityWait,
            ToWaiting,
            ToGate,
            Boarding
        }

        public float walkSpeed = 2.4f;
        public float checkInWaitSeconds = 1.2f;
        public float securityWaitSeconds = 1.4f;
        public float waitingPauseSeconds = 1.0f;

        private PassengerManager manager;
        private BuildableInstance checkIn;
        private BuildableInstance security;
        private BuildableInstance waitingSeat;
        private BuildableInstance gate;
        private readonly List<Vector3> path = new List<Vector3>();
        private int pathIndex;
        private float waitUntil;
        private PassengerStage stage;
        private bool countedCheckInLine;
        private bool countedSecurityLine;

        public void Initialize(PassengerManager manager, BuildableInstance entrance, BuildableInstance checkIn, BuildableInstance security, BuildableInstance waitingSeat, BuildableInstance gate)
        {
            this.manager = manager;
            this.checkIn = checkIn;
            this.security = security;
            this.waitingSeat = waitingSeat;
            this.gate = gate;
            transform.position = manager.GetBuildableCenter(entrance) + Vector3.up * 0.18f;
            BeginPathTo(PassengerStage.ToCheckIn, checkIn);
        }

        private void Update()
        {
            if (manager == null) return;

            if (stage == PassengerStage.CheckInWait || stage == PassengerStage.SecurityWait || stage == PassengerStage.Boarding)
            {
                if (Time.time >= waitUntil) AdvanceAfterWait();
                return;
            }

            FollowPath();
        }

        private void FollowPath()
        {
            if (pathIndex >= path.Count)
            {
                ArriveAtStageTarget();
                return;
            }

            Vector3 target = path[pathIndex];
            transform.position = Vector3.MoveTowards(transform.position, target, walkSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, target) < 0.05f) pathIndex++;
        }

        private void ArriveAtStageTarget()
        {
            if (stage == PassengerStage.ToCheckIn)
            {
                countedCheckInLine = true;
                manager.EnterCheckInLine();
                stage = PassengerStage.CheckInWait;
                waitUntil = Time.time + checkInWaitSeconds;
                return;
            }

            if (stage == PassengerStage.ToSecurity)
            {
                countedSecurityLine = true;
                manager.EnterSecurityLine();
                stage = PassengerStage.SecurityWait;
                waitUntil = Time.time + securityWaitSeconds;
                return;
            }

            if (stage == PassengerStage.ToWaiting)
            {
                waitUntil = Time.time + waitingPauseSeconds;
                stage = PassengerStage.Boarding;
                return;
            }

            if (stage == PassengerStage.ToGate)
            {
                BeginBoarding();
            }
        }

        private void AdvanceAfterWait()
        {
            if (stage == PassengerStage.CheckInWait)
            {
                if (countedCheckInLine) manager.LeaveCheckInLine();
                BeginPathTo(PassengerStage.ToSecurity, security);
                return;
            }

            if (stage == PassengerStage.SecurityWait)
            {
                if (countedSecurityLine) manager.LeaveSecurityLine();
                if (waitingSeat != null) BeginPathTo(PassengerStage.ToWaiting, waitingSeat);
                else BeginPathTo(PassengerStage.ToGate, gate);
                return;
            }

            if (stage == PassengerStage.Boarding)
            {
                BeginPathTo(PassengerStage.ToGate, gate);
            }
        }

        private void BeginPathTo(PassengerStage nextStage, BuildableInstance target)
        {
            stage = nextStage;
            if (!manager.TryGetPath(transform.position, target, path))
            {
                manager.PassengerCouldNotFindRoute(this);
                return;
            }
            pathIndex = 0;
        }

        private void BeginBoarding()
        {
            transform.position = manager.GetBuildableCenter(gate) + Vector3.up * 0.18f;
            manager.PassengerBoarded(this);
        }
    }
}

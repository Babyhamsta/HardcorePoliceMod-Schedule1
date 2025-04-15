using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.NPCs.Behaviour;
using System.Collections.Generic;
using System.Reflection;
using System;
using Il2CppScheduleOne.Police;
using System.Collections;
using System.Linq;
using UnityEngine.AI;
using Il2CppScheduleOne.PlayerScripts;
using MelonLoader.Utils;
using UnityEngine.Playables;
using Il2CppFishNet.Object;

[assembly: MelonInfo(typeof(HardcorePolice.Main), "HardcorePolice", "1.2.0", "Babyhamsta")]
[assembly: MelonGame(null, null)]

namespace HardcorePolice
{
    public class Main : MelonMod
    {
        private static ModConfig config;
        private static string configPath;

        private static HarmonyLib.Harmony harmony;

        private static readonly HashSet<IntPtr> boostedNpcs = new HashSet<IntPtr>();
        private static readonly Dictionary<IntPtr, float> stuckTimers = new Dictionary<IntPtr, float>();
        private static readonly Dictionary<IntPtr, float> lastSpeedMultiplier = new Dictionary<IntPtr, float>();
        private static readonly Dictionary<IntPtr, float> lastSpeedUpdateTime = new Dictionary<IntPtr, float>();

        private static readonly Dictionary<IntPtr, string> officerRoles = new Dictionary<IntPtr, string>();
        private static readonly Dictionary<IntPtr, Vector3> lastAssignedIntercept = new Dictionary<IntPtr, Vector3>();
        private static readonly Dictionary<IntPtr, float> flankTimers = new Dictionary<IntPtr, float>();

        private static bool RadioedOfficers = false;

        private static void LoadConfig()
        {
            configPath = System.IO.Path.Combine(MelonEnvironment.UserDataDirectory, "HardcorePoliceConfig.json");

            if (System.IO.File.Exists(configPath))
            {
                try
                {
                    config = Newtonsoft.Json.JsonConvert.DeserializeObject<ModConfig>(System.IO.File.ReadAllText(configPath));
                    MelonLogger.Msg("[Hardcore Police] Config loaded.");
                }
                catch (Exception e)
                {
                    MelonLogger.Warning($"[Hardcore Police] Failed to parse config: {e.Message}. Recreating default config.");
                }
            }
            else
            {
                config = new ModConfig();
                SaveConfig();
                MelonLogger.Msg("[Hardcore Police] Default config created.");
            }
        }

        private static void SaveConfig()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(configPath, json);
        }

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("[Hardcore Police] Loaded version v1.2.0 | Babyhamsta");
            LoadConfig();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (sceneName == "Main")
            {
                harmony = new HarmonyLib.Harmony("HardcorePolice.Patches");
                Patch(harmony, typeof(NPCMovement), "UpdateDestination", nameof(UpdateDestination_Postfix));
                Patch(harmony, typeof(PoliceOfficer), "Activate", nameof(VisionCone_Postfix));
                Patch(harmony, typeof(PursuitBehaviour), "UpdateArrest", nameof(UpdateArrest_Postfix));
                Patch(harmony, typeof(PursuitBehaviour), "Begin", nameof(OnPursuitBegin_Postfix));
                Patch(harmony, typeof(PursuitBehaviour), "Stop", nameof(OnPursuitEnd_Postfix));
                Patch(harmony, typeof(PlayerCrimeData), "SetPursuitLevel", nameof(OnPursuitTimeout_Postfix));
                MelonLogger.Msg("[HardcorePolice] Harmony patches applied in Main scene.");
            }
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Main")
            {
                harmony.UnpatchSelf();
                boostedNpcs.Clear();
                stuckTimers.Clear();
                lastSpeedMultiplier.Clear();
                lastSpeedUpdateTime.Clear();
                officerRoles.Clear();
                lastAssignedIntercept.Clear();
                flankTimers.Clear();
                RadioedOfficers = false;

                MelonLogger.Msg("[Hardcore Police] Main scene unloaded. Resetting state.");
            }
        }

        private static void Patch(HarmonyLib.Harmony harmony, Type targetType, string methodName, string hookMethod)
        {
            MethodInfo method = targetType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            MethodInfo patch = typeof(Main).GetMethod(hookMethod, BindingFlags.Static | BindingFlags.NonPublic);

            if (method == null || patch == null)
            {
                MelonLogger.Error($"[Hardcore Police] Failed to patch {targetType.Name}.{methodName}");
                return;
            }

            harmony.Patch(method, postfix: new HarmonyMethod(patch));
            MelonLogger.Msg($"[Hardcore Police] Patched {targetType.Name}.{methodName}");
        }

        private static void UpdateDestination_Postfix(NPCMovement __instance)
        {
            if (!config.Flanking || __instance == null || __instance.Agent == null || !__instance.Agent.enabled)
                return;

            var npc = __instance.npc;
            if (npc == null || !(npc is PoliceOfficer))
                return;

            IntPtr id = npc.Pointer;
            Vector3 dest = __instance.CurrentDestination;
            float dist = Vector3.Distance(__instance.FootPosition, dest);

            if (!__instance.Agent.pathPending && !__instance.Agent.hasPath && dist > 10f)
            {
                if (!stuckTimers.ContainsKey(id))
                    stuckTimers[id] = 0f;

                stuckTimers[id] += Time.deltaTime;

                if (stuckTimers[id] > 0.25f)
                {
                    __instance.Agent.ResetPath();
                    __instance.Agent.SetDestination(dest);
                    stuckTimers[id] = 0f;
                }
            }
            else
            {
                stuckTimers[id] = 0f;
            }
        }

        private static void VisionCone_Postfix(PoliceOfficer __instance)
        {
            if (!config.ExtendedVision || __instance == null || __instance.awareness == null || __instance.awareness.VisionCone == null)
                return;

            var visionCone = __instance.awareness.VisionCone;
            visionCone.RangeMultiplier = config.VisionRangeMultiplier;
            visionCone.HorizontalFOV = config.VisionHorizontalFOV;
            visionCone.VerticalFOV = config.VisionVerticalFOV;

            // Faster detection speed
            if (config.FasterDetection)
            {
                foreach (var stateDict in visionCone.StateSettings.Values)
                {
                    foreach (var state in stateDict.Values)
                    {
                        state.RequiredNoticeTime = Mathf.Max(0.25f, state.RequiredNoticeTime * 0.5f);
                    }
                }
            }

            MelonLogger.Msg($"[Hardcore Police] Adjusted VisionCone for {__instance.ID}");
        }

        private static void OnPursuitBegin_Postfix(PursuitBehaviour __instance)
        {
            ApplySpeedBoostOnce(__instance);

            IntPtr id = __instance.Npc.Pointer;

            if (config.Flanking && !officerRoles.ContainsKey(id))
            {
                Vector3 origin = __instance.transform.position;
                string role = "Chaser";
                float closest = float.MaxValue;
                IntPtr closestId = IntPtr.Zero;

                foreach (var entry in officerRoles)
                {
                    if (entry.Value == "Chaser")
                    {
                        var existing = GameObject.FindObjectsOfType<PoliceOfficer>()
                            ?.FirstOrDefault(p => p?.Pointer == entry.Key);
                        if (existing != null)
                        {
                            float d = Vector3.Distance(origin, existing.transform.position);
                            if (d < closest)
                            {
                                closest = d;
                                closestId = entry.Key;
                            }
                        }
                    }
                }

                if (closest < 20f && closestId != IntPtr.Zero)
                {
                    officerRoles[id] = "Interceptor";
                }
                else
                {
                    officerRoles[id] = "Chaser";
                }
            }

            if (!RadioedOfficers)
            {
                RadioedOfficers = true;
                MelonCoroutines.Start(RadioNearbyPolice(__instance));
            }
        }

        private static void OnPursuitEnd_Postfix(PursuitBehaviour __instance)
        {
            if (__instance?.Npc?.Movement?.SpeedController != null)
            {
                var controller = __instance.Npc.Movement.SpeedController;
                controller.RemoveSpeedControl("hardcore_chase");
                boostedNpcs.Remove(__instance.Npc.Pointer);
            }

            IntPtr id = __instance.Npc.Pointer;
            lastAssignedIntercept.Remove(id);
            flankTimers.Remove(id);
            officerRoles.Remove(id);
        }

        private static void OnPursuitTimeout_Postfix(PlayerCrimeData __instance, PlayerCrimeData.EPursuitLevel level)
        {
            if (!__instance || __instance.Player == null)
                return;

            if (level == PlayerCrimeData.EPursuitLevel.None)
                return;

            MelonLogger.Msg("[Hardcore Police] Pursuit ended - performing full reset");

            stuckTimers.Clear();
            lastSpeedMultiplier.Clear();
            lastSpeedUpdateTime.Clear();
            officerRoles.Clear();
            lastAssignedIntercept.Clear();
            flankTimers.Clear();
            RadioedOfficers = false;

            // Reset all officers that were pursuing this player
            foreach (var officer in PoliceOfficer.Officers)
            {
                if (officer == null)
                    continue;

                // Reset any controller values if they exist
                if (officer?.Movement?.SpeedController != null)
                {
                    var controller = officer.Movement.SpeedController;
                    controller.RemoveSpeedControl("hardcore_chase");
                    
                    // Reset speed multipliers
                    officer.Movement.MovementSpeedScale = 1f;
                    officer.Movement.MoveSpeedMultiplier = 1f;
                }
            }
        }

        private static void UpdateArrest_Postfix(PursuitBehaviour __instance, float tick)
        {
            if (__instance?.TargetPlayer == null)
                return;

            bool isVisible = __instance.sync___get_value_isTargetVisible();
            Vector3 playerPos = __instance.TargetPlayer.Avatar.CenterPoint;
            Vector3 officerPos = __instance.transform.position;

            float distance = Vector3.Distance(officerPos, playerPos);
            if (config.ArrestAdjustments)
            {
                if (isVisible && distance <= config.ArrestCooldownCircleDistance)
                {
                    __instance.arrestingEnabled = true;
                    __instance.timeWithinArrestRange += tick;

                    float arrestProgress = __instance.timeWithinArrestRange / config.ArrestProgressSpeed;
                    if (__instance.TargetPlayer.IsOwner && arrestProgress > __instance.TargetPlayer.CrimeData.CurrentArrestProgress)
                    {
                        __instance.TargetPlayer.CrimeData.SetArrestProgress(arrestProgress);
                    }
                }
            }

            ApplySpeedBoostOnce(__instance);
            ApplyDistanceBasedSpeedBoost(__instance);

            if (config.Flanking)
            {
                Vector3 destination = isVisible ? GetFlankDestination(__instance) : __instance.TargetPlayer.CrimeData.LastKnownPosition;

                if (!__instance.Npc.Movement.IsMoving || Vector3.Distance(__instance.Npc.Movement.CurrentDestination, destination) > 2.5f)
                {
                    if (__instance.Npc.Movement.CanGetTo(destination, 1.5f))
                    {
                        __instance.Npc.Movement.SetDestination(destination);
                    }
                }
            }
        }

        private static IEnumerator RadioNearbyPolice(PursuitBehaviour originOfficer)
        {
            if (!config.RadioNearby || originOfficer == null)
                yield break;

            // Initial delay before starting to call officers
            yield return new WaitForSeconds(4f);

            // Bail out if originOfficer or player is now null
            if (originOfficer == null || originOfficer.TargetPlayer == null)
                yield break;

            var player = originOfficer.TargetPlayer;
            Vector3 playerPos = player.Avatar.CenterPoint;

            // Max number of officers (could be added to config)
            int maxOfficersToQueue = 10;

            // Collect potential backup officers with distances
            List<(PoliceOfficer officer, float distanceSqr)> candidateOfficers = new List<(PoliceOfficer, float)>(maxOfficersToQueue);
            float radioDistanceSquared = config.RadioDistance * config.RadioDistance;

            foreach (var officer in PoliceOfficer.Officers)
            {
                if (officer == null || officer == originOfficer ||
                    officer.PursuitBehaviour == null || officer.PursuitBehaviour.Active)
                    continue;

                float distSqr = (officer.transform.position - playerPos).sqrMagnitude;
                if (distSqr <= radioDistanceSquared)
                {
                    candidateOfficers.Add((officer, distSqr));

                    if (candidateOfficers.Count >= maxOfficersToQueue * 1.5f) // Get more than needed so we can sort
                        break;
                }
            }

            // Sort by distance - closest officers respond first
            candidateOfficers.Sort((a, b) => a.distanceSqr.CompareTo(b.distanceSqr));

            // Limit to max number
            int officersToQueue = Mathf.Min(candidateOfficers.Count, maxOfficersToQueue);

            if (officersToQueue <= 0)
                yield break;

            MelonLogger.Msg($"[Hardcore Police] Queueing {officersToQueue} officers for backup over {officersToQueue * 3} seconds...");

            // Activate officers one at a time with a delay between each
            for (int i = 0; i < officersToQueue; i++)
            {
                // Check if the pursuit is still active before activating more officers
                if (originOfficer == null || originOfficer.TargetPlayer == null ||
                    !originOfficer.Active || player.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
                {
                    MelonLogger.Msg("[Hardcore Police] Pursuit ended, stopping officer activation");
                    yield break;
                }

                var officerInfo = candidateOfficers[i];

                MelonCoroutines.Start(ActivateOfficerStaggered(officerInfo.officer, player.NetworkObject, i, officersToQueue));
                yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 4f));
            }
        }

        private static Vector3 GetFlankDestination(PursuitBehaviour pursuit)
        {
            var npc = pursuit.Npc;
            var player = pursuit.TargetPlayer;
            IntPtr id = npc.Pointer;

            // Early exit if flanking is disabled
            if (!config.Flanking)
                return player.CrimeData.LastKnownPosition;

            // Check cooldown timer - avoid unnecessary processing
            float currentTime = Time.time;
            if (flankTimers.TryGetValue(id, out float lastTime) &&
                currentTime - lastTime < config.FlankUpdateCooldown)
            {
                return lastAssignedIntercept.TryGetValue(id, out var lastDest) ?
                    lastDest : player.CrimeData.LastKnownPosition;
            }

            // Update flank timer now
            flankTimers[id] = currentTime;

            // Fetch role once
            string role = officerRoles.TryGetValue(id, out var r) ? r : "Chaser";

            // Get player position and velocity
            Vector3 playerPos = player.Avatar.CenterPoint;
            Rigidbody playerRb = player.Avatar.GetComponent<Rigidbody>();
            Vector3 velocity = (playerRb != null) ? playerRb.velocity : Vector3.zero;

            // Return early if player is essentially stationary
            float speed = velocity.magnitude;
            if (speed < 0.1f)
                return player.CrimeData.LastKnownPosition;

            // Calculate prediction only once
            Vector3 predictedDir = velocity / speed; // More efficient than normalized
            float predictionTime = (role == "Interceptor") ? 8f : 2f;
            Vector3 predictedPos = playerPos + predictedDir * speed * predictionTime;

            // Apply role-specific modifications
            if (role == "Interceptor")
            {
                // Deterministic flanking based on officer ID instead of random
                Vector3 side = Vector3.Cross(Vector3.up, predictedDir);

                // Use hash code for deterministic "randomness" - much faster than Random.Range
                int hash = id.GetHashCode();
                float lateral = 8f + (hash % 8); // Range 8-15
                bool goLeft = (hash & 1) == 0;   // Deterministic left/right

                predictedPos += side * (goLeft ? -lateral : lateral);
            }
            else
            {
                // Chasers trail using deterministic offset
                int hash = id.GetHashCode();
                float backOffset = 2f + (hash % 3); // Range 2-4
                predictedPos -= predictedDir * backOffset;
            }

            // Check if we need to sample NavMesh (expensive operation)
            bool needsNavMeshSample = true;
            if (lastAssignedIntercept.TryGetValue(id, out var lastPos))
            {
                // Only sample if moved significantly (use squared distance - faster)
                needsNavMeshSample = Vector3.SqrMagnitude(predictedPos - lastPos) > 9f; // 3 units
            }

            if (needsNavMeshSample)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(predictedPos, out hit, 10f, NavMesh.AllAreas))
                {
                    lastAssignedIntercept[id] = hit.position;
                    return hit.position;
                }
            }
            else if (lastAssignedIntercept.TryGetValue(id, out var cachedPos))
            {
                return cachedPos;
            }

            return player.CrimeData.LastKnownPosition;
        }

        private static void ApplyDistanceBasedSpeedBoost(PursuitBehaviour __instance)
        {
            if (!config.DistanceSpeedBoost || __instance?.Npc?.Movement?.SpeedController == null || __instance.TargetPlayer == null)
                return;

            IntPtr id = __instance.Npc.Pointer;
            float now = Time.time;

            if (lastSpeedUpdateTime.ContainsKey(id) && now - lastSpeedUpdateTime[id] < 0.25f)
                return;

            Vector3 officerPos = __instance.transform.position;
            Vector3 playerPos = __instance.TargetPlayer.Avatar.CenterPoint;
            float distance = Vector3.Distance(officerPos, playerPos);

            float cappedDistance = Mathf.Clamp(distance, 0f, config.DistanceMax);
            float boost = Mathf.Lerp(config.DistanceBoostBase, config.DistanceBoostMax, cappedDistance / 100f);

            float lastBoost = lastSpeedMultiplier.TryGetValue(id, out float previous) ? previous : -1f;

            if (Mathf.Abs(boost - previous) > 0.05f)
            {
                var movement = __instance.Npc.Movement;
                var controller = movement.SpeedController;

                controller.SpeedMultiplier = boost;
                movement.MovementSpeedScale = boost * 0.92f;
                movement.MoveSpeedMultiplier = boost * 0.92f;

                if (controller.DoesSpeedControlExist("hardcore_chase"))
                    controller.RemoveSpeedControl("hardcore_chase");

                controller.AddSpeedControl(new NPCSpeedController.SpeedControl("hardcore_chase", 50, boost));

                lastSpeedMultiplier[id] = boost;
                lastSpeedUpdateTime[id] = now;
            }
        }

        private static void ApplySpeedBoostOnce(PursuitBehaviour __instance)
        {
            if (!config.SpeedBoost || __instance?.Npc?.Movement?.SpeedController == null)
                return;

            IntPtr id = __instance.Npc.Pointer;
            if (boostedNpcs.Contains(id))
                return;

            boostedNpcs.Add(id);

            var movement = __instance.Npc.Movement;
            var controller = movement.SpeedController;

            controller.SpeedMultiplier = config.SpeedBoostMultiplier;
            movement.MovementSpeedScale = config.MovementSpeedScale;
            movement.MoveSpeedMultiplier = config.MoveSpeedMultiplier;

            controller.AddSpeedControl(new NPCSpeedController.SpeedControl("hardcore_chase", 50, config.SpeedBoostMultiplier));
            movement.SetStance(NPCMovement.EStance.Stanced);
        }

        private static IEnumerator ActivateOfficerStaggered(PoliceOfficer officer, NetworkObject target, int index, int total)
        {
            if (officer == null || target == null)
                yield break;

            officer.BeginFootPursuit_Networked(target, true);

            MelonLogger.Msg($"[Hardcore Police] Officer {index + 1}/{total} activated");

            yield return new WaitForSeconds(0.25f);

            if (!officer.Movement.IsMoving && target.transform != null)
            {
                Vector3 dest = target.transform.position;
                if (officer.Movement.CanGetTo(dest, 1.5f))
                {
                    officer.Movement.SetDestination(dest);
                }
            }
        }
    }
}

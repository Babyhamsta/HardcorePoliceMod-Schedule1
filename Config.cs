using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardcorePolice
{
    public class ModConfig
    {
        public bool ExtendedVision = true;
        public bool SpeedBoost = true;
        public bool DistanceSpeedBoost = true;
        public bool Flanking = true;
        public bool ArrestAdjustments = true;
        public bool RadioNearby = true;
        public bool FasterDetection = false;

        public float VisionRangeMultiplier = 10f;
        public float VisionHorizontalFOV = 165f;
        public float VisionVerticalFOV = 80f;

        public float SpeedBoostMultiplier = 1.3f;
        public float MovementSpeedScale = 1.15f;
        public float MoveSpeedMultiplier = 1.15f;

        public float DistanceBoostBase = 1.45f;
        public float DistanceBoostMax = 2.0f;
        public float DistanceMax = 100f;

        public float FlankUpdateCooldown = 3f;

        public float ArrestCooldownCircleDistance = 5f;
        public float ArrestProgressSpeed = 1.5f;

        public float RadioDistance = 100f;
    }

}

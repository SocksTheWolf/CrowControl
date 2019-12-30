using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CrowControl
{
    public class SpawnHelper
    {
        private Player ply;
        private Level currentLevel;
        public bool extendedPathfinder = false;
        private Random rand;

        private Bumper currentBumper;

        public List<Seeker> spawnedSeekers = new List<Seeker>();
        public List<Snowball> spawnedSnowballs = new List<Snowball>();
        public List<AngryOshiro> spawnedOshiros = new List<AngryOshiro>();

        public SpawnHelper()
        {
            rand = new Random();
        }

        public void SetPlayer(Player ply)
        {
            this.ply = ply;
        }

        public void SetLevel(Level currentLevel)
        {
            this.currentLevel = currentLevel;
        }

        public void SpawnSeeker(bool addToList, string name)
        {
            if (currentLevel == null)
            {
                return;
            }

            if (currentLevel.Bounds.Width / 8 > 659 || currentLevel.Bounds.Height / 8 > 407)
            {
                return;
            }

            if (!extendedPathfinder)
            {
                extendedPathfinder = true;
                currentLevel.Pathfinder = new Pathfinder(currentLevel);
            }

            for (int i = 0; i < 100; i++)
            {
                int x = rand.Next(currentLevel.Bounds.Width) + currentLevel.Bounds.X;
                int y = rand.Next(currentLevel.Bounds.Height) + currentLevel.Bounds.Y;

                // should be at least 100 pixels from the player
                double playerDistance = Math.Sqrt(Math.Pow(MathHelper.Distance(x, ply.X), 2) + Math.Pow(MathHelper.Distance(y, ply.Y), 2));

                // also check if we are not spawning in a wall, that would be a shame
                Rectangle collideRectangle = new Rectangle(x - 8, y - 8, 16, 16);
                if (playerDistance > 50 && !currentLevel.CollideCheck<Solid>(collideRectangle) && !currentLevel.CollideCheck<Seeker>(collideRectangle))
                {
                    // build a Seeker with a proper EntityID to make Speedrun Tool happy (this is useless in vanilla Celeste but the constructor call is intercepted by Speedrun Tool)
                    EntityData seekerData = generateBasicEntityData(currentLevel, 10 + 1);
                    seekerData.Position = new Vector2(x, y);
                    Seeker seeker = new Seeker(seekerData, Vector2.Zero);
                    SeekerName seekerName = new SeekerName(true, true, name);
                    seeker.Add(seekerName);
                    if (addToList)
                    {
                        spawnedSeekers.Add(seeker);
                    }
                    currentLevel.Add(seeker);
                    break;
                }
            }
        }

        public void SpawnOshiro(bool addToList)
        {
            if (currentLevel == null)
            {
                return;
            }

            Vector2 position = new Vector2(currentLevel.Bounds.Left - 32, currentLevel.Bounds.Top + currentLevel.Bounds.Height / 2);
            AngryOshiro oshiro = new AngryOshiro(position, false);
            if (addToList)
            {
                spawnedOshiros.Add(oshiro);
            }
            currentLevel.Add(oshiro);
        }

        public void SpawnSnowball(bool addToList, string name)
        {
            if (currentLevel == null)
            {
                return;
            }

            Snowball snowball = new Snowball();
            SnowballName snowballName = new SnowballName(true, true, name);
            snowball.Add(snowballName);
            if (addToList)
            {
                spawnedSnowballs.Add(snowball);
            }
            currentLevel.Add(snowball);

            snowball.X = currentLevel.Camera.Left - 60f;
        }

        public void SpawnBumper() 
        {
            if (currentLevel == null)
            {
                return;
            }

            Vector2 offset = new Vector2(rand.Next(-8, 8), rand.Next(-8, 8));
            currentBumper = new Bumper(ply.Position + offset, Vector2.Zero);

            /* This doesn't do it
            FieldInfo fireMode = typeof(Bumper).GetField("fireMode", BindingFlags.NonPublic | BindingFlags.Instance);
            fireMode.SetValue(currentBumper, false);
            Console.WriteLine(fireMode.GetValue(currentBumper));
            */

            currentBumper.Active = false;

            currentLevel.Add(currentBumper);
        }

        public void RemoveCurrentBumper() 
        {
            currentLevel.Remove(currentBumper);
        }

        public void SpawnFish() 
        {
            if (currentLevel == null) 
            {
                return;
            }

            for (int i = 0; i < 100; i++)
            {
                int x = rand.Next(currentLevel.Bounds.Width) + currentLevel.Bounds.X;
                int y = rand.Next(currentLevel.Bounds.Height) + currentLevel.Bounds.Y;

                // should be at least 100 pixels from the player
                double playerDistance = Math.Sqrt(Math.Pow(MathHelper.Distance(x, ply.X), 2) + Math.Pow(MathHelper.Distance(y, ply.Y), 2));

                // also check if we are not spawning in a wall, that would be a shame
                Rectangle collideRectangle = new Rectangle(x - 8, y - 8, 16, 16);
                if (playerDistance > 25 && !currentLevel.CollideCheck<Solid>(collideRectangle))
                {
                    // build a Seeker with a proper EntityID to make Speedrun Tool happy (this is useless in vanilla Celeste but the constructor call is intercepted by Speedrun Tool)
                    EntityData seekerData = generateBasicEntityData(currentLevel, 10 + 1);
                    seekerData.Position = new Vector2(x, y);
                    Puffer puffer = new Puffer(seekerData, Vector2.Zero);
                    currentLevel.Add(puffer);
                    break;
                }
            }
        }

        private EntityData generateBasicEntityData(Level level, int entityNumber)
        {
            EntityData entityData = new EntityData();

            // we hash the current level name, so we will get a hopefully-unique "room hash" for each room in the level
            // the resulting hash should be between 0 and 49_999_999 inclusive
            int roomHash = Math.Abs(level.Session.Level.GetHashCode()) % 50_000_000;

            // generate an ID, minimum 1_000_000_000 (to minimize chances of conflicting with existing entities)
            // and maximum 1_999_999_999 inclusive (1_000_000_000 + 49_999_999 * 20 + 19) => max value for int32 is 2_147_483_647
            // => if the same entity (same entityNumber) is generated in the same room, it will have the same ID, like any other entity would
            entityData.ID = 1_000_000_000 + roomHash * 20 + entityNumber;

            entityData.Level = level.Session.LevelData;
            entityData.Values = new Dictionary<string, object>();

            return entityData;
        }
    }
}

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

        private ColdBumper currentBumper;

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

        public void ClearAllSpawnLists()
        {
            spawnedSeekers.Clear();
            spawnedSnowballs.Clear();
            spawnedOshiros.Clear();
        }

        public void ClearSeekers(bool clearList=true)
        {
            if (currentLevel != null)
            {
                currentLevel.Remove(spawnedSeekers);
                if (clearList)
                    spawnedSeekers.Clear();
            }
        }

        public void ClearSnowballs(bool clearList=true)
        {
            if (currentLevel != null)
            {
                currentLevel.Remove(spawnedSnowballs);
                if (clearList)
                    spawnedSnowballs.Clear();
            }
        }

        public void ClearOshiros(bool clearList=true)
        {
            if (currentLevel != null)
            {
                currentLevel.Remove(spawnedOshiros);
                if (clearList)
                    spawnedOshiros.Clear();
            }
        }

        // returns true if the user has a seeker already
        public bool DoesUserHaveSeeker(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return spawnedSeekers.Any((seeker) => {
                if (seeker.Get<CrowControlName>().Name == name)
                    return true;
                return false;
            });
        }

        public bool SpawnSeeker(bool addToList, string name, bool checkUniques=false)
        {
            if (currentLevel == null)
                return false;

            if (currentLevel.Bounds.Width / 8 > 659 || currentLevel.Bounds.Height / 8 > 407)
                return false;

            // If we only allow one user to spawn one object, check to see if this
            // user has an active seeker already.
            if (addToList && checkUniques && DoesUserHaveSeeker(name))
                return false;

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
                    CrowControlName seekerName = new CrowControlName(true, true, name);
                    seeker.Add(seekerName);
                    if (addToList)
                    {
                        spawnedSeekers.Add(seeker);
                    }
                    currentLevel.Add(seeker);
                    return true;
                }
            }
            return false;
        }

        public bool DoesUserHaveOshiro(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return spawnedOshiros.Any((oshiro) => {
                if (oshiro.Get<CrowControlName>().Name == name)
                    return true;
                return false;
            });
        }

        public bool SpawnOshiro(bool addToList, string name, bool checkUniques=false)
        {
            if (currentLevel == null)
                return false;

            // Same with seekers, check to see if the oshiro are allowed to be spawned.
            if (addToList && checkUniques && DoesUserHaveOshiro(name))
                return false;

            Vector2 position = new Vector2(currentLevel.Bounds.Left - 32, currentLevel.Bounds.Top + currentLevel.Bounds.Height / 2);
            AngryOshiro oshiro = new AngryOshiro(position, false);
            CrowControlName oshiroName = new CrowControlName(true, false, name);
            oshiro.Add(oshiroName);
            if (addToList)
            {
                spawnedOshiros.Add(oshiro);
            }
            currentLevel.Add(oshiro);
            return true;
        }

        public bool DoesUserHaveSnowball(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            
            return spawnedSnowballs.Any((snow) => {
                if (snow.Get<CrowControlName>().Name == name)
                    return true;
                return false;
            });
        }

        public bool SpawnSnowball(bool addToList, string name, bool checkUniques=false)
        {
            if (currentLevel == null)
                return false;

            // Same with seekers, check to see if the snowballs are allowed to be spawned.
            if (addToList && checkUniques && DoesUserHaveSnowball(name))
                return false;

            Snowball snowball = new Snowball();
            CrowControlName snowballName = new CrowControlName(true, true, name);
            snowball.Add(snowballName);
            if (addToList)
            {
                spawnedSnowballs.Add(snowball);
            }
            currentLevel.Add(snowball);

            snowball.X = currentLevel.Camera.Left - 60f;
            return true;
        }

        public void SpawnBumper() 
        {
            if (currentLevel == null)
                return;

            Vector2 offset = new Vector2(rand.Next(-8, 8), rand.Next(-8, 8));
            currentBumper = new ColdBumper(ply.Position + offset, Vector2.Zero);
            currentBumper.Active = false;
            currentLevel.Add(currentBumper);
        }

        public void RemoveCurrentBumper() 
        {
            if (currentBumper != null)
            {
                currentBumper.Visible = false;
                currentLevel.Remove(currentBumper);
            }
        }

        public bool SpawnFish() 
        {
            if (currentLevel == null) 
                return false;

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
                    return true;
                }
            }
            return false;
        }

        public void SpawnFeatherOnPlayer() 
        {
            if (currentLevel == null) 
                return;

            FlyFeather feather = new FlyFeather(ply.Position, false, true);
            currentLevel.Add(feather);
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

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace RimSpace
{
	
	public class WorldObject_SpaceStation : WorldObject_Orbital
	{

		public bool isUnderConstruction = false;


		/*

		 * 
		 * is under construction
		 * construction progress
		 * construction materials remaning
		 * construction labor remaining
		 * 
		 * 
		*/



		public Station_Spaceships spaceships;
		public Station_Shuttles shuttles;
		public Station_Manufacturing manufacturing;
		public Station_Economy economy;

		public override void PostMake()
		{
			this.spaceships = new Station_Spaceships(this);
			this.shuttles = new Station_Shuttles(this);
			this.manufacturing = new Station_Manufacturing(this);
			this.economy = new Station_Economy(this);
			isUnderConstruction = true;


			base.PostMake();
		}

		public ThingOwner innerContainer;

		public override void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
			if (this.HasMap)
			{
				outChildren.Add(this.Map);
			}
		}
		public new ThingOwner GetDirectlyHeldThings()
		{
			return null;
		}

		public override void SpawnSetup()
		{

			this.innerContainer = new ThingOwner<Thing>(this);
			base.SpawnSetup();
		}

		public void AcceptShuttle()
		{


		}


	}


	public class Station_Shuttles : IExposable
	{
		public void ExposeData()
		{

		}
		public WorldObject_SpaceStation spaceStation;
		private int numOfShuttles = 0;
		public Station_Shuttles(WorldObject_SpaceStation spaceStation)
		{
			this.spaceStation = spaceStation;
		}

		public void addShuttle()
		{
			this.numOfShuttles++;
		}
		public void removeShuttle()
		{
			if (numOfShuttles > 0) this.numOfShuttles--;
		}

		/*
		 * make shuttle
		 * launch shuttle
		 * accept shuttle
		 * number of shuttles
		*/


	}

	public class Station_Spaceships : IExposable
	{
		public void ExposeData()
		{

		}
		public WorldObject_SpaceStation spaceStation;
		public Station_Spaceships(WorldObject_SpaceStation spaceStation)
		{
			this.spaceStation = spaceStation;
		}
		/*
		 * List of spaceships
		 * launch spaceship group
		 * Accept Spaceship group 
		 */


	}

	public class Station_Economy : IExposable
	{
		public void ExposeData()
		{

		}
		public int population;
		public WorldObject_SpaceStation spaceStation;
		public Station_Economy(WorldObject_SpaceStation spaceStation)
		{
			this.spaceStation = spaceStation;
		}



		/*
		 * station population
		 * station needs - food, fuel, goods
		 * station tax revenue
		 * station colonists
		 * station schedules
			*/

	}


	public class Station_Manufacturing : IExposable
	{
		public StationProject curProject;
		public List<StationProjectDef> availableProjects;
		public List<StationProjectDef> finishedProjects;
		public int LaborNeeded;
		public void ExposeData()
		{

		}
		public WorldObject_SpaceStation spaceStation;
		public Station_Manufacturing(WorldObject_SpaceStation spaceStation)
		{
			this.spaceStation = spaceStation;
		}

		public bool tryStartProject(StationProjectDef def)
        {
			if(curProject == null && !finishedProjects.Contains(def))
            {
				curProject = new StationProject(def, spaceStation);
				return true;
            }
			else return false;
        }
		public void ManufacturingTick()
        {
			if(curProject != null)
            {
				curProject.tickProject();
            }
        }


		/*
		 * current building project
		 * building progress
		 * building materials remaining
		 * building labor remaining
			*/

	}

	public class StationProject
    {
		public WorldObject_SpaceStation station;
		public StationProjectDef Def;
		public float RemainingLabor;

		public float TotalLabor => Def.TotalLabor;
		public int MaxWorkers => Def.MaxWorkers;
		public float MaxLaborSpeedPerTick => Def.MaxLaborRatePerTick;
		public float ticksToFinish => (float)this.RemainingLabor / (LaborRatio * MaxLaborSpeedPerTick);
		public List<ThingDefCountClass> costList => Def.costList;

		public int availableLabor => station.economy.population >= this.MaxWorkers ? this.MaxWorkers : station.economy.population;
		public float LaborRatio => (float)availableLabor / (float)MaxWorkers;

		public StationProject(StationProjectDef def, WorldObject_SpaceStation station)
        {
			this.station = station;
			this.Def = def;
        }
		public virtual void SetupProject()
        {
			this.RemainingLabor = this.TotalLabor;
        }

		public virtual void tickProject()
        {
			if(RemainingLabor <= 0)
            {
				CompleteProject();

			}
            else
            {
				this.RemainingLabor -= (LaborRatio * MaxLaborSpeedPerTick);
            }
			

        }

		public void pullThingFromStation(Thing thing,int count = 1)
        {
			var holder = station.innerContainer.ToList().FindAll(s => s.def.Equals(thing.def));
			if(holder != null && holder.Select(s=> s.stackCount).Sum() >= count)
            {
				bool runFlag = true;
				int remaining = count;
                while (runFlag)
                {
					var buff = holder.First();
					if (buff.stackCount > remaining)
                    {
						buff.stackCount -= remaining;
						runFlag = false; 

					}
                    else
                    {
						remaining -= buff.stackCount;
						station.innerContainer.Remove(buff);


					}


                }

            }


        }

		public void CompleteProject()
        {
			this.station.manufacturing.finishedProjects.Add(Def);

        }

    } 


}

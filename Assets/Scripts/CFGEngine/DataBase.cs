using System;

using System.Collections.Generic;
using System.IO;

namespace CFGEngine
{
	public static class DataBase
	{
		private static List<Figure> figures = new List<Figure>();
		private static List<Figure> bosses = new List<Figure>();

		static DataBase()
		{
			bosses.Add (new Hero (1, "Boss", 2, 20, 2, 100, 0, "Goblin", "Goblin", false, false, 1, false, 0, false, false, false));

			figures.Add (new Figure (2, "Fireball", 4, -1, -1, 0, 1, 2, "Fireball", "Fireball", true, false, 0, false, 0, false, false, true));

			figures.Add (new Figure (2, "Zombie", 2, 20, 2, 0, 1, 2, "Zombie", "Zombie", false, false, 0, true, 0, false, false, false));
			figures.Add (new Figure (14, "Ghoul", 3, 3, 5, 0, 1, 10, "Ghoul", "Ghoul", false, false, 0, true, 0, false, false, false));

			figures.Add (new Figure (13, "Allosaurus", 5, 6, 5, 0, 1, 10, "Allosaurus", "Allosaurus", false, false, 0, false, 0, false, true, false));

			figures.Add (new Figure (10, "Clay Golem", 10, 100, 2, 0, 1, 10, "ClayGolem", "ClayGolem", false, false, 0, false, 0, false, true, false));

			figures.Add (new Figure (12, "Amazon-archer", 3, 6, 5, 0, 1, 10, "Amazon-archer", "Amazon-archer", true, false, 0, false, 0, false, false, false));
			figures.Add (new Figure (1, "Assasin", 1, 1, 10, 0, 1, 1, "Assasin", "Assasin", false, false, 0, false, 0, false, false, false));

			figures.Add (new Figure (15, "CyclopSoldier", 1, 4, 5, 0, 1, 10, "CyclopSoldier", "CyclopSoldier", false, false, 0, false, 0, false, false, false));

			figures.Add (new Figure (15, "Dino1", 1, 4, 5, 0, 1, 10, "Dino1", "Dino1", false, false, 0, false, 0, false, false, false));
			figures.Add (new Figure (15, "DragonWarlord", 1, 4, 5, 0, 1, 10, "DragonWarlord", "DragonWarlord", false, true, 0, false, 0, false, false, false));

			figures.Add (new Figure (15, "Spider Green", 1, 4, 5, 0, 1, 10, "SpiderGreen", "SpiderGreen", false, false, 0, false, 0, false, false, false));
			figures.Add (new Figure (1, "Spider", 1, 10, 10, 0, 1, 1,"Spider", "Spider", false, false, 0, false, 0, false, false, false));

			figures.Add (new Figure (15, "Shaman", 3, 3, 5, 0, 1, 10, "Shaman", "Shaman", true, false, 0, false, 2, true, false, false));
			figures.Add (new Figure (1, "Skeleton", 1, 1, 10, 0, 1, 1,"Skeleton", "Skeleton", false, false, 0, false, 0, false, false, false));

			figures.Add (new Figure (15, "Voodoo Doll", 10, 1, 5, 0, 1, 10, "ChibiMummy", "ChibiMummy", false, false, 0, false, 0, false, false, false));
			figures.Add (new Figure (1, "Spartan King", 1, 1, 2, 0, 1, 1,"SpartanKing", "SpartanKing", false, true, 0, false, 0, false, false, false));
			figures.Add (new Figure (3, "Dude", 3, 3, 2, 0, 1, 3, "Dude", "", false, false, 0, false, 0, false, false, false));
		}

		public static IList<Figure> Figures
		{
			get	{return figures.AsReadOnly();}
		}

		public static IList<Figure> Bosses
		{
			get	{return bosses.AsReadOnly();}
		}
	}

	public class Figure
	{
		public Figure()
		{
		}

		public Figure(int id, 
		              String name, 
		              int strength, 
		              int defaultHP, 
		              int speed, 
		              int rarity, 
		              int cost, 
		              double weight, 
		              String modelName, 
		              String imageName,
		              bool isRangedAttack,
		              bool isHaste,
		              int armor,
		              bool isVampire,
		              int healing,
					  bool isTeleport,
					  bool isGiant,
					  bool isMagic)
		{
			this.id = id;
			this.name = name;
			this.strength = strength;
			this.defaultHP = defaultHP;
			this.speed = speed;
			this.rarity = rarity;
			this.cost = cost;
			this.modelName = modelName;
			this.imageName = imageName;
			this.isRangedAttack = isRangedAttack;
			this.isHaste = isHaste;
			this.armor = armor;
			this.isVampire = isVampire;
			this.healing = healing;
			this.isTeleport = isTeleport;
			this.isGiant = isGiant;
			this.isMagic = isMagic;

			this.weight = weight;
			this.isBoss = false;
		}

		public Figure(Figure other)
		{
			this.id = other.id;
			this.name = other.name;
			this.strength = other.strength;
			this.defaultHP = other.defaultHP;
			this.speed = other.speed;
			this.rarity = other.rarity;
			this.cost = other.cost;
			this.modelName = other.modelName;
			this.imageName = other.imageName;
			this.isRangedAttack = other.isRangedAttack;
			this.isHaste = other.isHaste;
			this.armor = other.armor;
			this.isVampire = other.isVampire;
			this.healing = other.healing;
			this.isTeleport = other.isTeleport;
			this.isGiant = other.isGiant;
			this.isMagic = other.isMagic;
			
			this.weight = other.weight;
			this.isBoss = other.isBoss;
		}


        [System.Xml.Serialization.XmlElement("IsBoss")]
		public virtual bool IsBoss
		{
			get {return isBoss;}
            set { isBoss = value; }
		}

        [System.Xml.Serialization.XmlElement("Weight")]
        public virtual double Weight
        {
            get { return weight; }
            set { weight = value; }
        }

		public int id;                       //ToDo - change on private, add public properties
		public String name;
		public int strength;
		public int defaultHP;
		public int speed;
		public int rarity;
		public int cost;
		public String modelName;
		public String imageName;
		public bool isRangedAttack = false;
		public bool isHaste = false;
		public int armor = 0;
		public bool isVampire = false;
		public int healing = 0;
		public bool isTeleport = false;
		public bool isGiant = false;
		public bool isMagic = false;

		protected double weight;
		protected bool isBoss;
	}

	public class Hero: Figure
	{
		public Hero(int id, 
		            String name, 
		            int strength, 
		            int defaultHP, 
		            int speed, 
		            int rarity, 
		            int cost, 
		            String modelName, 
		            String imageName,
		            bool isRangedAttack,
		            bool isHaste,
		            int armor,
		            bool isVampire,
		            int healing,
					bool isTeleport,
					bool isGiant,
					bool isMagic)
			: base(id, 
			       name, 
			       strength,
			       defaultHP, 
			       speed, 
			       rarity, 
			       cost, 
			       20, 
			       modelName, 
			       imageName, 
			       isRangedAttack, 
			       isHaste,
			       armor,
			       isVampire,
			       healing,
				   isTeleport,
				   isGiant,
				   isMagic)
		{
			isBoss = true;
		}

		public Hero(Hero other)
			: base (other)
		{
		}
	}
}


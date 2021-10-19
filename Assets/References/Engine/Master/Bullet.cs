using Engine.Algorithms;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{
    public class Bullet
    {
        /// <summary>
        /// Zerro if extract
        /// </summary>
        public TileObject TileObject { get; set; }
        public ulong Target { get; set; }
    }

    public class AbilityBullet : Ability
    {
        public override string Name { get { return "Bullet"; } }

        //public  AbilityBulletModel Model;
        public Move Move;


        public AbilityBullet(Unit owner) : base(owner, TileObjectType.None)
        {
            //Model = model;
        }


    }
}

using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class UnitBasePart
    {
        public TileObjectContainer TileObjectContainer { get; set; }
        public UnitBasePart(UnitBase unitBase)
        {
            this.UnitBase = unitBase;
        }

        public UnitBase UnitBase { get; private set; }

        public string Name { get; set; }
        public TileObjectType PartType { get; set; }
        public int Level { get; set; }
        public int Range { get; set; }
        public int CompleteLevel { get; set; }
        public bool Destroyed { get; set; }
        public bool Exists { get; set; }
        public bool WillExist { get; set; }
        public GameObject Part { get; set; }

        private HitByBullet hitByBullet;
        public bool ReadyToFire
        {
            get
            {
                return hitByBullet != null;
            }
        }

        public float AnimateFrom { get; set; }
        public float AnimateTo { get; set; }

        Renderer m_Rd;
        Material[] m_BackupMaterials;
        MeshFilter m_SelfMeshFilter;
        public Mesh m_MeshBackup;
        public Mesh m_MeshNew;
        public void SetMeshGhost()
        {
            m_Rd = Part.GetComponent<Renderer>();

            // cache all original materials
            Material[] mats = m_Rd.materials;
            int len = mats.Length;
            m_BackupMaterials = new Material[len];
            for (int i = 0; i < len; i++)
                m_BackupMaterials[i] = mats[i];

            // generate wireframe mesh vertex
            m_SelfMeshFilter = Part.GetComponent<MeshFilter>();
            m_MeshBackup = m_SelfMeshFilter.mesh;   // backup original mesh

            // generate vertices data with barycentric coordinate
            Vector3[] pos = m_SelfMeshFilter.mesh.vertices;
            Vector3[] nor = m_SelfMeshFilter.mesh.normals;
            Vector4[] tan = m_SelfMeshFilter.mesh.tangents;
            Vector2[] tex = m_SelfMeshFilter.mesh.uv;
            int[] tri = m_SelfMeshFilter.mesh.triangles;
            List<Vector3> wireframePos = new List<Vector3>();
            List<Vector3> wireframeNor = new List<Vector3>();
            List<Vector4> wireframeTan = new List<Vector4>();
            List<Vector2> wireframeTex = new List<Vector2>();
            List<Color> wireframeBc = new List<Color>();
            List<int> wireframeTriangle = new List<int>();
            for (int i = 0; i < tri.Length; i += 3)
            {
                int ind1 = tri[i + 0];
                int ind2 = tri[i + 1];
                int ind3 = tri[i + 2];

                wireframePos.Add(pos[ind1]);
                wireframePos.Add(pos[ind2]);
                wireframePos.Add(pos[ind3]);

                wireframeNor.Add(nor[ind1]);
                wireframeNor.Add(nor[ind2]);
                wireframeNor.Add(nor[ind3]);

                wireframeTan.Add(tan[ind1]);
                wireframeTan.Add(tan[ind2]);
                wireframeTan.Add(tan[ind3]);

                wireframeTex.Add(tex[ind1]);
                wireframeTex.Add(tex[ind2]);
                wireframeTex.Add(tex[ind3]);

                wireframeBc.Add(new Color(1, 0, 0));
                wireframeBc.Add(new Color(0, 1, 0));
                wireframeBc.Add(new Color(0, 0, 1));

                wireframeTriangle.Add(i + 0);
                wireframeTriangle.Add(i + 1);
                wireframeTriangle.Add(i + 2);
            }

            // create the wireframe mesh
            m_MeshNew = new Mesh();
            m_MeshNew.name = m_SelfMeshFilter.mesh.name + "_Wireframe";
            m_MeshNew.vertices = wireframePos.ToArray();
            m_MeshNew.normals = wireframeNor.ToArray();
            m_MeshNew.tangents = wireframeTan.ToArray();
            m_MeshNew.uv = wireframeTex.ToArray();
            m_MeshNew.colors = wireframeBc.ToArray();
            m_MeshNew.triangles = wireframeTriangle.ToArray();
        }
        public void ApplyWireframeMaterial()
        {
            int len = m_Rd.materials.Length;
            Material[] mats = new Material[len];
            for (int i = 0; i < len; i++)
            {
                mats[i] = HexGrid.MainGrid.GetMaterial("WireframeBasic");
                /*
                mats[i] = HexGrid.MainGrid.GetMaterial("glow 1");
                mats[i].SetColor("_color_fresnel", UnitBase.GetPlayerColor(UnitBase.PlayerId));
                mats[i].SetColor("_Color_main", UnitBase.GetPlayerColor(UnitBase.PlayerId));
                */

                mats[i].SetColor("_LineColor", UnitBase.GetPlayerColor(UnitBase.PlayerId));
                mats[i].SetFloat("_LineWidth", 0.001f);
                mats[i].SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back);
            }
            m_Rd.materials = mats;
            m_SelfMeshFilter.mesh = m_MeshNew;
        }

        public void Fire(Move move)
        {
            if (UnitBase.UnitId == "unit2")
            {
                int x = 0;
            }
            // Reload
            foreach (MoveRecipeIngredient moveRecipeIngredient in move.MoveRecipe.Ingredients)
            {
                // Transit the ingrdient into the weapon. This is the reloaded ammo. (Can be empty)
                UnitBaseTileObject unitBaseTileObject;
                unitBaseTileObject = UnitBase.RemoveTileObject(moveRecipeIngredient);
                if (unitBaseTileObject != null)
                {
                    // Transit ingredient
                    TransitObject transitObject = new TransitObject();
                    transitObject.GameObject = unitBaseTileObject.GameObject;
                    transitObject.TargetPosition = Part.transform.position;
                    transitObject.DestroyAtArrival = true;

                    unitBaseTileObject.GameObject = null;
                    HexGrid.MainGrid.AddTransitTileObject(transitObject);
                }
            }

            // Find the fileobject to fire with
            UnitBaseTileObject ammo = UnitBase.FindAmmoTileObject(move.MoveRecipe.Result);
            if (ammo == null)
            {
                throw new Exception("NoAmmo");
            }
            else
            {
                hitByBullet = HexGrid.MainGrid.Fire(UnitBase, ammo.TileObject);

                Position2 targetPosition = move.Positions[move.Positions.Count - 1];

                GroundCell weaponTargetCell;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(targetPosition, out weaponTargetCell))
                {
                    // Determine which direction to rotate towards
                    Vector3 turnWeaponIntoDirection = (weaponTargetCell.transform.position - Part.transform.position).normalized;
                    turnWeaponIntoDirection.y = 0;
                    UnitBase.TurnWeaponIntoDirection = turnWeaponIntoDirection;
                }
            }
        }
        public void FireBullet()
        {
            GroundCell weaponSourceCell;
            if (!HexGrid.MainGrid.GroundCells.TryGetValue(hitByBullet.FireingPosition, out weaponSourceCell))
            {
                throw new Exception("wtf");
            }
            GroundCell weaponTargetCell;
            if (!HexGrid.MainGrid.GroundCells.TryGetValue(hitByBullet.TargetPosition, out weaponTargetCell))
            {
                throw new Exception("wtf");
            }

            GameObject gameObject = HexGrid.MainGrid.CreateShell(weaponSourceCell.transform, hitByBullet.Bullet);
            gameObject.transform.position = Part.transform.position;

            Shell shell = gameObject.GetComponent<Shell>();
            shell.FireingUnit = UnitBase;
            shell.HitByBullet = hitByBullet;

            Vector3 targetPos = weaponTargetCell.transform.position;
            Rigidbody rigidbody = shell.GetComponent<Rigidbody>();
            rigidbody.velocity = CalcBallisticVelocityVector(shell.transform.position, targetPos, UnitBase.HasEngine()?30:1);
            
        }
        private Vector3 CalcBallisticVelocityVector(Vector3 initialPos, Vector3 finalPos, float angle)
        {
            var toPos = initialPos - finalPos;

            var h = toPos.y;

            toPos.y = 0;
            var r = toPos.magnitude;

            //float rpercent = r * 10; // / 100;
            //angle = 70 * rpercent / 100;
            //if (r > 5)
            //    angle = 30;

            var g = -Physics.gravity.y;
            var a = Mathf.Deg2Rad * angle;

            var vI = Mathf.Sqrt(((Mathf.Pow(r, 2f) * g)) / (r * Mathf.Sin(2f * a) + 2f * h * Mathf.Pow(Mathf.Cos(a), 2f)));

            Vector3 velocity = (finalPos - initialPos).normalized * Mathf.Cos(a);
            velocity.y = Mathf.Sin(a);

            return velocity * vI;
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
	[ExecuteInEditMode]
	public class HexMapEditor : MonoBehaviour
	{
		public static bool IsPlaying
		{
			get
			{
				return UnityEditor.EditorApplication.isPlaying;
				//return true; For WebGl
			}
		}
		public int ChangeMap;

		public HexGrid hexGrid;

		private int changeMap;
		
		void Update()
		{
			if (ChangeMap != changeMap)
			{
				changeMap = ChangeMap;

				if (!HexMapEditor.IsPlaying)
				{
					if (changeMap == 0)
						hexGrid.RemoveAllChildren();
					else
						hexGrid.CreateMapInEditor();
				}
			}
			if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
			{
				HandleInput();
			}
		}

		void HandleInput()
		{

			/*
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(inputRay, out hit)) {
				hexGrid.ColorCell(hit.point, activeColor);
			}*/
		}
	}
}
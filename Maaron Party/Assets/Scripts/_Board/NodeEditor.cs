using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Node))] [CanEditMultipleObjects]
public class NodeEditor : Editor
{
	public override void OnInspectorGUI()
	{
		var script = (Node)target;
		if (script == null) return;

		Undo.RecordObject(script, "Node Editor Change");

		//typeProp = (TileTypes) EditorGUILayout.EnumPopup(TileTypes);

		EditorGUI.BeginChangeCheck();
		script.nodeSpace = (NodeSpace) EditorGUILayout.EnumPopup ("NodeSpace", script.nodeSpace);
		//script.nodeSpace = 
		if (EditorGUI.EndChangeCheck())
		{
			Debug.Log("<color=#FF9900>New Node Type</color>");
			script.ChangeNodeSpace();
		}

		base.OnInspectorGUI();
	}
}
#endif

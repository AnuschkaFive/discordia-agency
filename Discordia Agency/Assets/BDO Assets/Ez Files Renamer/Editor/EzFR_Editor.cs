using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;

public class EzFR_Editor : EditorWindow 
{
	#region ========== Rename ==========================================================================================
	private enum SeparatedBy 
	{
		nothing,
		hyphen, 
		point, 
		space_, 
		underline,
		custom
	}
	private enum SequenceOptions 
	{
		afterName, 
		beforeName,
		betweenPrefixAndName
	}

	private SeparatedBy prefixSeparatedBy = SeparatedBy.nothing;
	private SeparatedBy sequentialSeparatedBy = SeparatedBy.nothing;
	SequenceOptions sequenceOptions = SequenceOptions.afterName;

	// Name
	private bool usePrefix;
	private string filePrefix = "";
	private string fileNewName;
	private string fileSeparator = "";
	private string prefixCustomSeparator = "";
	// Make Sequencial
	private bool makeSequential;
	private int sequentialInitalNumber = 0;
	private int sequentialNumber = 0;
	private string sequentialCustomSeparator;
	#endregion ========== Rename =======================================================================================

	#region ========== Sort ============================================================================================
	private enum SortOptions
	{
		nameAscending,
		nameDescending,
		axisXAscending,
		axisXDescending,
		axisYAscending,
		axisYDescending,
		axisZAscending,
		axisZDescending
	}

	private SortOptions sortOption = SortOptions.nameAscending;

	private bool sortChildren; // This option will be true when only one gameobject is select and if it has children
	#endregion ========== Sort =========================================================================================

	[MenuItem("Window/BDO Assets/Ez Files Renamer")]
	public static void ShowWindow()
	{
		EditorWindow editorWindow = EditorWindow.GetWindow(typeof(EzFR_Editor));
		GUIContent _titleContent = new GUIContent("Ez Files Renamer");

		editorWindow.autoRepaintOnSceneChange = true;
		editorWindow.titleContent = _titleContent;
		editorWindow.Show();
	}

	private void OnGUI()
	{
		RenameGUI();
		SortGUI();
		EZ_Style.DisplayInformation("https://forum.unity3d.com/threads/ez-files-renamer.300182/");
	}

	#region ========== Rename ==========================================================================================
	private void RenameGUI()
	{
		EZ_Style.Header("Rename", false, false);
					
		// New Name
		fileNewName = EditorGUILayout.TextField("New Name:", fileNewName);

		// Prefix
		usePrefix = EditorGUILayout.ToggleLeft("Use Prefix:", usePrefix);
		if(usePrefix)
		{
			filePrefix = EditorGUILayout.TextField("Prefix:", filePrefix);

			prefixSeparatedBy = (SeparatedBy)EditorGUILayout.EnumPopup("Separated by:", prefixSeparatedBy);
			if(prefixSeparatedBy == SeparatedBy.custom)
				prefixCustomSeparator = EditorGUILayout.TextField("Custom Separator:", prefixCustomSeparator);
			else
				prefixCustomSeparator = "";
				
			EditorGUILayout.HelpBox("The separation will be placed between the prefix and the new name of the file", MessageType.Info);
		}
			
		// Make it Sequential
		makeSequential = EditorGUILayout.ToggleLeft("Make it Sequential:", makeSequential);
		if(makeSequential)
		{
			sequenceOptions = (SequenceOptions)EditorGUILayout.EnumPopup("Sequence goes:", sequenceOptions);
			sequentialInitalNumber = EditorGUILayout.IntField("Initial Number", sequentialInitalNumber);
			sequentialSeparatedBy = (SeparatedBy)EditorGUILayout.EnumPopup("Separated by:", sequentialSeparatedBy);

			if(sequentialSeparatedBy == SeparatedBy.custom)
				sequentialCustomSeparator = EditorGUILayout.TextField("Custom Separator:", sequentialCustomSeparator);
			else
				sequentialCustomSeparator = "";

			EditorGUILayout.HelpBox("The separation will be placed between the new name and the number", MessageType.Info);
		}

		EditorGUILayout.Space();
		if(GUILayout.Button("Rename on Hierarchy"))
			DoRenameHierarchy();
		if(GUILayout.Button("Rename on Project Folder"))
			DoRenameFilesFolder();


		EZ_Style.Footer();
	}

	private void DoRenameHierarchy()
	{
		GameObject[] _gameObjectsSelected = Selection.gameObjects;

		if(!RenameCheckErrorsToContinue(_gameObjectsSelected, null))
			return;	
			
		// Sort the gameobjects inside the array based on the siblin index
		System.Array.Sort(_gameObjectsSelected, delegate(GameObject tempSelection0, GameObject tempSelection1) {
			return EditorUtility.NaturalCompare(tempSelection0.transform.GetSiblingIndex().ToString(), tempSelection1.transform.GetSiblingIndex().ToString());
		});
			
		string _filePrefix = GetPrefix();
		sequentialNumber = sequentialInitalNumber;
		// Calculate the amount that each file will increase in the progress bar
		float _result = (float)_gameObjectsSelected.Length / 100f;

		for (int i = 0; i < _gameObjectsSelected.Length; i++) 
		{
			EditorUtility.DisplayProgressBar("Renaming", "Wait until the gameObjects are renamed...", _result * i);

			if(makeSequential)
			{
				switch(sequenceOptions)
				{
				case SequenceOptions.afterName:
					_gameObjectsSelected[i].name = _filePrefix + fileNewName + GetSeparationType(sequentialSeparatedBy, false) + sequentialNumber;
					break;

				case SequenceOptions.beforeName:
					_gameObjectsSelected[i].name = sequentialNumber.ToString() + GetSeparationType(sequentialSeparatedBy, false) + _filePrefix + fileNewName;
					break;

				case SequenceOptions.betweenPrefixAndName:
					_gameObjectsSelected[i].name = _filePrefix + sequentialNumber + GetSeparationType(sequentialSeparatedBy, false) + fileNewName;
					break;
				}

				sequentialNumber++;
			}
			else
				_gameObjectsSelected[i].name = _filePrefix + fileNewName;
		}

		EditorUtility.ClearProgressBar();
	}

	private void DoRenameFilesFolder()
	{
		Object[] _objectsSelected = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

		if(!RenameCheckErrorsToContinue(null, _objectsSelected))
			return;

		// Sort the gameobjects inside the array based on name
		System.Array.Sort(_objectsSelected, delegate(Object objectSelected0, Object objectSelected1) {
			return EditorUtility.NaturalCompare(objectSelected0.name, objectSelected1.name);
		});

		sequentialNumber = sequentialInitalNumber;
		string _path; // Keep the path of the current file

		// Reset the obj name to prevent conflict, because in the project folder it is not permited 2 files with the same name
		foreach(Object obj in _objectsSelected)
		{
			_path = AssetDatabase.GetAssetPath(obj);
			AssetDatabase.RenameAsset(_path, sequentialNumber.ToString());
			sequentialNumber++;
		}
			
		string _filePrefix = GetPrefix();

		// Calculate the amount that each file will increase in the progress bar
		float _result = (float)_objectsSelected.Length / 100f;

		if(!makeSequential)
		{
			makeSequential = true;
			EditorUtility.DisplayDialog("Attention!", EzFR_Messages.WARNING_01, "Continue");
			sequentialInitalNumber = 0;
		}
		sequentialNumber = sequentialInitalNumber;

		// Rename the files
		for (int i = 0; i < _objectsSelected.Length; i++) 
		{
			EditorUtility.DisplayProgressBar("Renaming", "Wait until the files are renamed...", _result * i);
			_path = AssetDatabase.GetAssetPath(_objectsSelected[i]);

			switch(sequenceOptions)
			{
			case SequenceOptions.afterName:
				AssetDatabase.RenameAsset(_path, _filePrefix + fileNewName + GetSeparationType(sequentialSeparatedBy, false) + sequentialNumber);
				break;

			case SequenceOptions.beforeName:
				AssetDatabase.RenameAsset(_path, sequentialNumber.ToString() + GetSeparationType(sequentialSeparatedBy, false) + _filePrefix + fileNewName);
				break;

			case SequenceOptions.betweenPrefixAndName:
				AssetDatabase.RenameAsset(_path, _filePrefix + sequentialNumber + GetSeparationType(sequentialSeparatedBy, false) + fileNewName);
				break;
			}

			sequentialNumber++;
		}

		EditorUtility.ClearProgressBar();
	}

	private bool RenameCheckErrorsToContinue(GameObject[] gameObjectSelection, Object[] objectsSelected)
	{
		if(gameObjectSelection != null)
		{
			// Verify if there's at least one gameobject selected
			if(gameObjectSelection.Length <= 0)
			{
				EditorUtility.DisplayDialog("Attention!", EzFR_Messages.ERROR_00, "Close");
				return false;
			}

			// Verify if all selected gameobjects has the same parent
			List<Transform> _parents = new List<Transform>();
			for (int i = 0; i < gameObjectSelection.Length; i++) 
			{
				if(!_parents.Contains(gameObjectSelection[i].transform.parent))
					_parents.Add(gameObjectSelection[i].transform.parent);

				if(_parents.Count > 1)
				{
					if(EditorUtility.DisplayDialog("Attention!", EzFR_Messages.WARNING_00, "Continue", "Cancel"))
						return true;
					else
						return false;
				}
			}
		}
		else if(objectsSelected != null)
		{
			if(objectsSelected.Length <= 0)
			{
				EditorUtility.DisplayDialog("Attention!", EzFR_Messages.ERROR_01, "Close");
				return false;
			}

			// Verify if all the files are the same type
			List<string> _types = new List<string>();
			for (int i = 0; i < objectsSelected.Length; i++) 
			{
				if(!_types.Contains(Path.GetExtension(AssetDatabase.GetAssetPath(objectsSelected[i])).ToString()))
					_types.Add(Path.GetExtension(AssetDatabase.GetAssetPath(objectsSelected[i])).ToString());

				if(_types.Count > 1)
				{
					EditorUtility.DisplayDialog("Attention!", EzFR_Messages.ERROR_02, "Close");
					return false;
				}
			}

			// Verify the file extension
			if(string.Equals(_types[0], ".cs") || string.Equals(_types[0], ".js") || string.Equals(_types[0], ".shader"))
			{
				EditorUtility.DisplayDialog("Attention!", EzFR_Messages.ERROR_03, "Close");
				return false;
			}
		}
		else
		{
			Debug.LogError("Internal Error. GameObject and Object can't be both null");
			return false;
		}
			

		return true;
	}

	private string GetPrefix()
	{
		if(usePrefix)
			return filePrefix + GetSeparationType(prefixSeparatedBy, true);
		else
			return "";
	}

	private string GetSeparationType(SeparatedBy separatedBy, bool prefix)
	{
		switch(separatedBy)
		{
		case SeparatedBy.nothing:
			fileSeparator = "";
			break;

		case SeparatedBy.hyphen:
			fileSeparator =  "-";
			break;

		case SeparatedBy.point:
			fileSeparator = ".";
			break;

		case SeparatedBy.space_:
			fileSeparator = " ";
			break;

		case SeparatedBy.underline:
			fileSeparator = "_";
			break;

		case SeparatedBy.custom:
			if(prefix)
				fileSeparator = prefixCustomSeparator;
			else
				fileSeparator = sequentialCustomSeparator;
			break;
		}

		return fileSeparator;
	}
	#endregion ========== Rename =======================================================================================

	#region ========== Sort ============================================================================================
	[MenuItem("GameObject/Sort Children/Name Ascending", false, 49)]
	private static void SortNameAscending() { SortSelection(SortOptions.nameAscending, true); }
	[MenuItem("GameObject/Sort Children/Name Descending", false, 50)]
	private static void SortNameDescending() { SortSelection(SortOptions.nameDescending, true); }
	[MenuItem("GameObject/Sort Children/Axis X Ascending", false, 51)]
	private static void SortAxisXAscending() { SortSelection(SortOptions.axisXAscending, true); }
	[MenuItem("GameObject/Sort Children/Axis X Descending", false, 52)]
	private static void SortAxisXDescending() { SortSelection(SortOptions.axisXDescending, true); }
	[MenuItem("GameObject/Sort Children/Axis Y Ascending", false, 53)]
	private static void SortAxisYAscending() { SortSelection(SortOptions.axisYAscending, true); }
	[MenuItem("GameObject/Sort Children/Axis Y Descending", false, 54)]
	private static void SortAxisYDescending() { SortSelection(SortOptions.axisYDescending, true); }
	[MenuItem("GameObject/Sort Children/Axis Z Ascending", false, 55)]
	private static void SortAxisZAscending() { SortSelection(SortOptions.axisZAscending, true); }
	[MenuItem("GameObject/Sort Children/Axis Z Descending", false, 56)]
	private static void SortAxisZDescending() { SortSelection(SortOptions.axisZDescending, true); }

	private void SortGUI()
	{
		EZ_Style.Header("Sort", false, false);

		sortOption = (SortOptions)EditorGUILayout.EnumPopup("Sort Option:", sortOption);
		if(GUILayout.Button("Sort"))
			SortSelection(this.sortOption);

		EZ_Style.Footer();
	}

	private static void SortSelection(SortOptions sortOption, bool sortChildren = false)
	{
		// Get the selected transforms
		List<GameObject> _tempGameobjectsSelected = new List<GameObject>();
		if(!sortChildren)
		{
			foreach(GameObject gameObj in Selection.gameObjects)
				_tempGameobjectsSelected.Add(gameObj);
		}
		else
		{
			if(Selection.gameObjects.Length > 1)
			{
				EditorUtility.DisplayDialog("Attention!", EzFR_Messages.ERROR_05, "Close");
				return;
			}
				
			for (int i = 0; i < Selection.gameObjects[0].transform.childCount; i++)
				_tempGameobjectsSelected.Add(Selection.gameObjects[0].transform.GetChild(i).gameObject);
		}

		GameObject[] _gameobjectsSelected = _tempGameobjectsSelected.ToArray();

		if(!SortCheckErrorsToContinue(_gameobjectsSelected))
			return;

		// Sort the array based on the sort option
		if(sortOption == SortOptions.nameAscending || sortOption == SortOptions.nameDescending)
		{
			System.Array.Sort(_gameobjectsSelected, delegate(GameObject tempObjTrans0, GameObject tempObjTrans1) {
				return EditorUtility.NaturalCompare(tempObjTrans0.name, tempObjTrans1.name);
			});
		}
		else if(sortOption == SortOptions.axisXAscending || sortOption == SortOptions.axisXDescending)
		{
			System.Array.Sort(_gameobjectsSelected, delegate(GameObject object0, GameObject object1) {
				return EditorUtility.NaturalCompare(object0.transform.position.x.ToString(), object1.transform.position.x.ToString());
			});
		}
		else if(sortOption == SortOptions.axisYAscending || sortOption == SortOptions.axisYDescending)
		{
			System.Array.Sort(_gameobjectsSelected, delegate(GameObject object0, GameObject object1) {
				return EditorUtility.NaturalCompare(object0.transform.position.y.ToString(), object1.transform.position.y.ToString());
			});
		}
		else
		{
			System.Array.Sort(_gameobjectsSelected, delegate(GameObject object0, GameObject object1) {
				return EditorUtility.NaturalCompare(object0.transform.position.z.ToString(), object1.transform.position.z.ToString());
			});
		}

		// Calculate the amount that each file will increase in the progress bar
		float _result = (float)_gameobjectsSelected.Length / 100f;
			
		// Set the new sibling index
		if(sortOption == SortOptions.nameAscending || 
			sortOption == SortOptions.axisXAscending ||
			sortOption == SortOptions.axisYAscending ||
			sortOption == SortOptions.axisZAscending) 
		{
			for (int i = 0; i < _gameobjectsSelected.Length; i++) 
			{
				EditorUtility.DisplayProgressBar("Sorting", "Wait until the gameObjects are sorted...", _result * i);
				_gameobjectsSelected[i].transform.SetSiblingIndex(i);
			}
				
		}
		else
		{
			for (int i = 0; i < _gameobjectsSelected.Length; i++) 
			{
				EditorUtility.DisplayProgressBar("Sorting", "Wait until the gameObjects are sorted...", _result * i);
				int _index = (_gameobjectsSelected.Length - i) - 1;
				_gameobjectsSelected[i].transform.SetSiblingIndex(_index);
			}
		}

		EditorUtility.ClearProgressBar();
	}

	private static bool SortCheckErrorsToContinue(GameObject[] gameobjectsSelected)
	{
		if(gameobjectsSelected.Length <= 0)
		{
			EditorUtility.DisplayDialog("Attention!", EzFR_Messages.ERROR_00, "Close");
			return false;
		}

		// Verify if all selected gameobjects has the same parent
		List<Transform> _parents = new List<Transform>();
		for (int i = 0; i < gameobjectsSelected.Length; i++) 
		{
			if(!_parents.Contains(gameobjectsSelected[i].transform.parent))
				_parents.Add(gameobjectsSelected[i].transform.parent);

			if(_parents.Count > 1)
			{
				EditorUtility.DisplayDialog("Attention!", EzFR_Messages.ERROR_04, "Close");
				return false;
			}
		}

		return true;
	}
	#endregion ========== Sort =========================================================================================
}
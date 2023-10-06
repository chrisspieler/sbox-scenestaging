﻿
using Editor;
using Sandbox;
using System.Linq;
using System.Text.Json.Nodes;

public static class SceneEditorMenus
{
	[Menu( "Editor", "Scene/Cut", Shortcut = "Ctrl+X" )]
	public static void Cut()
	{
		var options = new GameObject.SerializeOptions();
		var go = EditorScene.Selection.First() as GameObject;
		var json = go.Serialize( options );

		SceneUtility.MakeGameObjectsUnique( json );

		EditorUtility.Clipboard.Copy( json.ToString() );
	}


	[Menu( "Editor", "Scene/Copy", Shortcut = "Ctrl+C" )]
	public static void Copy()
	{
		var options = new GameObject.SerializeOptions();
		var go = EditorScene.Selection.First() as GameObject;
		var json = go.Serialize( options );
		SceneUtility.MakeGameObjectsUnique( json );

		EditorUtility.Clipboard.Copy( json.ToString() );
	}

	[Menu( "Editor", "Scene/Paste", Shortcut = "Ctrl+V" )]
	public static void Paste()
	{
		var selected = EditorScene.Selection.First() as GameObject;

		var text = EditorUtility.Clipboard.Paste();
		if ( JsonNode.Parse( text ) is JsonObject jso )
		{
			var go = EditorScene.Active.CreateObject();
			go.Deserialize( jso );

			if ( selected is not null )
			{
				selected.AddSibling( go, false );
			}

			go.MakeNameUnique();

			EditorScene.Selection.Clear();
			EditorScene.Selection.Add( go );

			//TreeView.SelectItem( go );
		}
	}

	[Menu( "Editor", "Scene/Paste As Child", Shortcut = "Ctrl+Shift+V" )]
	public static void PasteAsChild()
	{
		var selected = EditorScene.Selection.First() as GameObject;

		var text = EditorUtility.Clipboard.Paste();
		if ( JsonNode.Parse( text ) is JsonObject jso )
		{
			var go = EditorScene.Active.CreateObject();
			go.Deserialize( jso );
			go.SetParent( selected );
			go.MakeNameUnique();

			// treeview - open parent selected somehow

			EditorScene.Selection.Clear();
			EditorScene.Selection.Add( go );
		}
	}

	[Menu( "Editor", "Scene/Duplicate", Shortcut = "Ctrl+D" )]
	public static void Duplicate()
	{
		using var scope = EditorScene.Active.Push();
		var options = new GameObject.SerializeOptions();
		var source = EditorScene.Selection.First() as GameObject;
		var json = source.Serialize( options );

		SceneUtility.MakeGameObjectsUnique( json );

		var go = GameObject.Create();
		go.Deserialize( json );
		go.WorldTransform = source.WorldTransform;

		source.AddSibling( go, false );

		EditorScene.Selection.Clear();
		EditorScene.Selection.Add( go );
	}

	[Menu( "Editor", "Scene/Delete", Shortcut = "Del" )]
	public static void Delete()
	{
		using var scope = EditorScene.Active.Push();

		foreach ( var entry in EditorScene.Selection.OfType<GameObject>() )
		{
			entry.Destroy();
		}
	}

	[Menu( "Editor", "Scene/Frame", Shortcut = "f" )]
	public static void Frame()
	{
		var bbox = new BBox();

		int i = 0;
		foreach ( var entry in EditorScene.Selection.OfType<GameObject>() )
		{
			if ( i++ == 0 )
			{
				bbox = new BBox( entry.WorldTransform.Position, 16 );
			}

			// get the bounding box of the selected objects
			bbox = bbox.AddBBox( new BBox( entry.WorldTransform.Position, 16 ) );

			foreach ( var model in entry.GetComponents<ModelComponent>( true, true ) )
			{
				bbox = bbox.AddBBox( model.Bounds );
			}
		}

		EditorEvent.Run( "scene.frame", bbox );
	}


	[Menu( "Editor", "Scene/Align To View" )]
	public static void AlignToView()
	{
		if ( !SceneViewWidget.Current.IsValid() )
			return;

		foreach ( var entry in EditorScene.Selection.OfType<GameObject>() )
		{
			entry.WorldTransform = new Transform( SceneViewWidget.Current.Camera.Position, SceneViewWidget.Current.Camera.Rotation );

		}
	}
}

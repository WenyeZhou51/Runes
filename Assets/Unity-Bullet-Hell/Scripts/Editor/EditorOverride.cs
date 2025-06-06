using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BulletHell
{
	[CustomEditor(typeof(Object), true, isFallback = true)]
	[CanEditMultipleObjects]
	public class EditorOverride : Editor
	{
		private Dictionary<string, Cache> cache = new Dictionary<string, Cache>();
		private List<SerializedProperty> props = new List<SerializedProperty>();
		private SerializedProperty propScript;
		private Type type;
		private int length;
		private List<FieldInfo> objectFields;
		private bool initialized;
		private Colors colors;
		private FoldoutAttribute prevFold;
		private GUIStyle style;

		void OnEnable()
		{
			// Delay style initialization to avoid null reference when EditorStyles is not ready
			EditorApplication.delayCall += InitializeStyles;
			
			bool pro = EditorGUIUtility.isProSkin;
			if (!pro)
			{
				colors = new Colors();
				colors.col0 = new Color(0.2f, 0.2f, 0.2f, 1f);
				colors.col1 = new Color(1, 1, 1, 0.55f);
				colors.col2 = new Color(0.7f, 0.7f, 0.7f, 1f);
			}
			else
			{
				colors = new Colors();
				colors.col0 = new Color(0.2f, 0.2f, 0.2f, 1f);
				colors.col1 = new Color(1, 1, 1, 0.1f);
				colors.col2 = new Color(0.25f, 0.25f, 0.25f, 1f);
			}

			var t = target.GetType();
			var typeTree = t.GetTypeTree();

			objectFields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).OrderByDescending(x => typeTree.IndexOf(x.DeclaringType)).ToList();
			length = objectFields.Count;

			Repaint();
			initialized = false;
		}
		
		private void InitializeStyles()
		{
			try
			{
				var uiTex_in = Resources.Load<Texture2D>("IN foldout focus-6510");
				var uiTex_in_on = Resources.Load<Texture2D>("IN foldout focus on-5718");
				
				if (EditorStyles.foldout != null)
				{
					style = new GUIStyle(EditorStyles.foldout);
					
					style.overflow = new RectOffset(-10, 0, 3, 0);
					style.padding = new RectOffset(25, 0, -3, 0);
					
					var c_on = Color.white;
					style.active.textColor = c_on;
					if (uiTex_in != null) style.active.background = uiTex_in;
					style.onActive.textColor = c_on;
					if (uiTex_in_on != null) style.onActive.background = uiTex_in_on;
					
					style.focused.textColor = c_on;
					if (uiTex_in != null) style.focused.background = uiTex_in;
					style.onFocused.textColor = c_on;
					if (uiTex_in_on != null) style.onFocused.background = uiTex_in_on;
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Error initializing editor styles: " + e.Message);
				style = null;
			}
		}

		private void OnDisable()
		{
			foreach (var cach in cache)
			{
				FoldManager.SetFold(cach.Value.atr.Id, cach.Value.expanded);
				cach.Value.Dispose();
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			if (!initialized)
			{
				for (var i = 0; i < length; i++)
				{
					var fold = Attribute.GetCustomAttribute(objectFields[i], typeof(FoldoutAttribute)) as FoldoutAttribute;
					
					Cache c;
					if (fold == null)
					{
						if (prevFold != null && prevFold.FoldEverything)
						{                            
							if (!cache.TryGetValue(prevFold.Name, out c)) {                               
								cache.Add(prevFold.Name, new Cache {atr = prevFold, types = new HashSet<string> {objectFields[i].Name}});
								cache[prevFold.Name].expanded = FoldManager.GetFold(prevFold.Id);
							}
							else
							{
								c.types.Add(objectFields[i].Name);
							}
						}
						continue;
					}

					prevFold = fold;
					fold.Id = fold.Name + target.GetHashCode().ToString();

					if (!cache.TryGetValue(fold.Name, out c))
					{
						cache.Add(fold.Name, new Cache {atr = fold, types = new HashSet<string> {objectFields[i].Name}});
						cache[prevFold.Name].expanded = FoldManager.GetFold(fold.Id);
					}
					else
					{
						c.types.Add(objectFields[i].Name);
					}
				}
				
				var property = serializedObject.GetIterator();
				var next = property.NextVisible(true);
				if (next)
				{
					do
					{
						HandleProp(property);
					} while (property.NextVisible(false));
				}
			}
			
			if (props.Count == 0)
			{
				DrawDefaultInspector();
				return;
			}

			initialized = true;

			using (new EditorGUI.DisabledScope("m_Script" == props[0].propertyPath))
			{
				EditorGUILayout.PropertyField(props[0], true);
			}

			EditorGUILayout.Space();

			foreach (var pair in cache)
			{
				var rect = EditorGUILayout.BeginVertical();
				EditorGUILayout.Space();
				EditorGUI.DrawRect(new Rect(rect.x - 1, rect.y - 1, rect.width + 1, rect.height + 1), colors.col0);
				EditorGUI.DrawRect(new Rect(rect.x - 1, rect.y - 1, rect.width + 1, rect.height + 1), colors.col1);
				
				// Simple foldout without depending on custom style
				pair.Value.expanded = EditorGUILayout.Foldout(pair.Value.expanded, pair.Value.atr.Name, true);
 
				EditorGUILayout.EndVertical();

				rect = EditorGUILayout.BeginVertical();

				EditorGUI.DrawRect(new Rect(rect.x - 1, rect.y - 1, rect.width + 1, rect.height + 1), colors.col2);

				if (pair.Value.expanded)
				{
					for (int i = 0; i < pair.Value.props.Count; i++)
					{
						EditorGUI.indentLevel = 1;

						EditorGUILayout.PropertyField(pair.Value.props[i],
							new GUIContent(pair.Value.props[i].name.FirstLetterToUpperCase()), true);
						if (i == pair.Value.props.Count - 1)
							EditorGUILayout.Space();
					}
				}

				EditorGUI.indentLevel = 0;
				EditorGUILayout.EndVertical();
			}

			for (var i = 1; i < props.Count; i++)
			{
				EditorGUILayout.PropertyField(props[i], true);
			}

			serializedObject.ApplyModifiedProperties();
			EditorGUILayout.Space();
		}

		public void HandleProp(SerializedProperty prop)
		{
			bool shouldBeFolded = false;

			foreach (var pair in cache)
			{
				if (pair.Value.types.Contains(prop.name))
				{
					shouldBeFolded = true;
					pair.Value.props.Add(prop.Copy());

					break;
				}
			}

			if (shouldBeFolded == false)
			{
				props.Add(prop.Copy());
			}
		}

		private struct Colors
		{
			public Color col0;
			public Color col1;
			public Color col2;
		}

		private class Cache
		{
			public HashSet<string> types = new HashSet<string>();
			public List<SerializedProperty> props = new List<SerializedProperty>();
			public FoldoutAttribute atr;
			public bool expanded;

			public void Dispose()
			{
				props.Clear();
				types.Clear();
				atr = null;
			}
		}
	}

	public static partial class FrameworkExtensions
	{
		public static string FirstLetterToUpperCase(this string s)
		{
			if (string.IsNullOrEmpty(s))
				return string.Empty;

			char[] a = s.ToCharArray();
			a[0] = char.ToUpper(a[0]);
			return new string(a);
		}

		public static IList<Type> GetTypeTree(this Type t)
		{
			var types = new List<Type>();
			while (t.BaseType != null)
			{
				types.Add(t);
				t = t.BaseType;
			}

			return types;
		}
	}
}

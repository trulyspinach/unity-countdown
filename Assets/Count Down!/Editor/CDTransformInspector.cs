using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityCountDown
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(Transform))]
	public class CDTransformInspector : Editor
	{

		public static CDTransformInspector self;

		private SerializedProperty m_position;
		private SerializedProperty m_rotation;
		private SerializedProperty m_scale;

		private DateTime m_timerDate = new DateTime();
		private bool m_timerSet;
		
		private bool m_showTimer;
		private bool m_showTransformUtil;
		private bool m_showLineCount;
		
		private string m_label;

		private GUIStyle m_countdownLabelStyle;
		private GUIStyle m_titleLabelStyle;
		private GUIStyle m_detailLabelStyle;

		private Texture2D m_cloud;

		private readonly Color m_skyColorPro = new Color(0.1568628f, 0.2078431f, 0.5764706f);
		private readonly Color m_skyColor = new Color(0.3921569f, 0.7098039f, 0.9647059f);


		private void LoadTimer() {
			
			m_showTimer = EditorPrefs.GetBool("TimeOut_ShowTimer", true);
			m_showTransformUtil = EditorPrefs.GetBool("TimeOut_ShowTU", true);
			m_showLineCount = EditorPrefs.GetBool("TimeOut_ShowLC", true);
			m_label = EditorPrefs.GetString("TimeOut_Label", "");
			
			var d = EditorPrefs.GetString("TimeOut_TimerDate", "NONE");
			m_timerSet = d != "NONE";
			m_timerDate = m_timerSet ? DateTime.Parse(d) : Directory.GetLastAccessTime(Application.dataPath);
		}
		
		private void Awake()
		{
			LoadTimer();
			
			m_countdownLabelStyle = new GUIStyle();
			var fontGuid = AssetDatabase.FindAssets("Roboto-Regular")[0];
			m_countdownLabelStyle.font = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(fontGuid));
			m_countdownLabelStyle.alignment = TextAnchor.MiddleCenter;
			m_countdownLabelStyle.fontSize = 18;
			m_countdownLabelStyle.normal.textColor = Color.white;

			m_titleLabelStyle = new GUIStyle {
				font = m_countdownLabelStyle.font,
				alignment = TextAnchor.UpperLeft,
				fontSize = 15,
				normal = {textColor = Color.white}
			};

			m_detailLabelStyle = new GUIStyle {
				font = m_countdownLabelStyle.font,
				alignment = TextAnchor.LowerRight,
				fontSize = 13,
				normal = {textColor = Color.white}
			};

			var cloudGuid = AssetDatabase.FindAssets(EditorGUIUtility.isProSkin ? "background_cloud" : "background_cloud light")[0];
			m_cloud = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(cloudGuid));

		}

		private void OnEnable()
		{
			self = this;

			m_position = serializedObject.FindProperty("m_LocalPosition");
			m_rotation = serializedObject.FindProperty("m_LocalRotation");
			m_scale = serializedObject.FindProperty("m_LocalScale");



			EditorApplication.update += Update;
		}

		private void OnDestroy()
		{
			self = null;
			EditorApplication.update -= Update;
		}

		public override void OnInspectorGUI()
		{
			EditorGUIUtility.LookLikeControls(15f);

			serializedObject.Update();

			if(m_showTimer) DrawCountDown();
			EditorGUILayout.Space();
			DrawPosition();
			DrawRotation();
			DrawScale();

			serializedObject.ApplyModifiedProperties();
		}

		private double m_lastUpdate;

		private void Update()
		{
			if (EditorApplication.timeSinceStartup - m_lastUpdate < 0.06f) return;
			m_lastUpdate = EditorApplication.timeSinceStartup;
			Repaint();
		}

		private void DrawCountDown()
		{
			var span = m_timerSet ? m_timerDate - DateTime.Now : DateTime.Now - m_timerDate;

			var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(120));
			rect.y += 10;
			rect.height -= 20;

			const float space = 1;
			var inner = new Rect(rect.x + space, rect.y + space, rect.width - space * 2, rect.height - space * 2);
			EditorGUI.DrawRect(inner, EditorGUIUtility.isProSkin ? m_skyColorPro : m_skyColor);

			GUI.BeginGroup(inner);

			var title = new Rect(12, 9, inner.width, inner.height);
			var detail = new Rect(0, 0, inner.width, inner.height);

			const float imageWidth = 530f;
			var cloudStartX = -(float)EditorApplication.timeSinceStartup * 10f;
			var curPos = cloudStartX;
			while (true)
			{
				if (curPos + imageWidth < 0)
				{
					curPos += imageWidth;
					continue;
				}

				if (curPos > inner.width) break;

				var cloudRect = new Rect(curPos, 10, imageWidth, inner.height);
				GUI.DrawTexture(cloudRect, m_cloud);
				curPos += imageWidth;
			}

			if(m_label != "")EditorGUI.LabelField(title, m_label + " :", m_titleLabelStyle);
			if(m_showLineCount)
				EditorGUI.LabelField(detail, string.Format("{0} Lines ", CDLineCounter.Count()), m_detailLabelStyle);

			GUI.EndGroup();

			var formatedStr = span.Days > 0 ? string.Format("{0} day, ", span.Days) : "";
			formatedStr += span.Hours > 0 || span.Days > 0? string.Format("{0} h, ", span.Hours) : "";
			formatedStr += span.Minutes > 0 || span.Hours > 0 || span.Days > 0
				? string.Format("{0} min, ", span.Minutes)
				: "";
			formatedStr += string.Format("{0} sec", span.Seconds);
			
			EditorGUI.LabelField(inner, formatedStr, m_countdownLabelStyle);


		}

		private void DrawPosition()
		{
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("Position");
				EditorGUILayout.PropertyField(m_position.FindPropertyRelative("x"));
				EditorGUILayout.PropertyField(m_position.FindPropertyRelative("y"));
				EditorGUILayout.PropertyField(m_position.FindPropertyRelative("z"));

				if (m_showTransformUtil) {
					var reset = GUILayout.Button("X", GUILayout.Width(20f));

					if (reset) m_position.vector3Value = Vector3.zero;
				}
				
			}
			GUILayout.EndHorizontal();
		}

		private void DrawScale()
		{
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("Scale");
				EditorGUILayout.PropertyField(m_scale.FindPropertyRelative("x"));
				EditorGUILayout.PropertyField(m_scale.FindPropertyRelative("y"));
				EditorGUILayout.PropertyField(m_scale.FindPropertyRelative("z"));

				if (m_showTransformUtil) {
					var reset = GUILayout.Button("X", GUILayout.Width(20f));

					if (reset) m_scale.vector3Value = Vector3.one;
				}
			}
			GUILayout.EndHorizontal();
		}

		[Flags]
		private enum Axes {
			None = 0,
			X = 1,
			Y = 2,
			Z = 4,
			All = 7
		}

		private static Axes CheckDifference(Transform t, Vector3 original)
		{
			var next = t.localEulerAngles;

			var axes = Axes.None;

			if (Differs(next.x, original.x)) axes |= Axes.X;
			if (Differs(next.y, original.y)) axes |= Axes.Y;
			if (Differs(next.z, original.z)) axes |= Axes.Z;

			return axes;
		}

		private void CheckDifference(SerializedProperty property)
		{
			var axes = Axes.None;

			if (!property.hasMultipleDifferentValues) return;
			var original = property.quaternionValue.eulerAngles;

			foreach (var obj in serializedObject.targetObjects)
			{
				axes |= CheckDifference(obj as Transform, original);
				if (axes == Axes.All) break;
			}

		}

		private static bool FloatField(string name, ref float value, GUILayoutOption opt)
		{
			var newValue = value;
			GUI.changed = false;
			newValue = EditorGUILayout.FloatField(name, newValue, opt);

			if (!GUI.changed || !Differs(newValue, value)) return false;
			value = newValue;
			return true;
		}

		private static bool Differs(float a, float b) { return Mathf.Abs(a - b) > 0.0001f; }

		private void DrawRotation()
		{
			GUILayout.BeginHorizontal();
			{
				var visible = ((Transform) serializedObject.targetObject).localEulerAngles;
				CheckDifference(m_rotation);
				var altered = Axes.None;

				var opt = GUILayout.MinWidth(30f);

				EditorGUILayout.LabelField("Rotation");
				if (FloatField("X", ref visible.x, opt)) altered |= Axes.X;
				if (FloatField("Y", ref visible.y, opt)) altered |= Axes.Y;
				if (FloatField("Z", ref visible.z, opt)) altered |= Axes.Z;

				if (m_showTransformUtil) {
					var reset = GUILayout.Button("X", GUILayout.Width(20f));

					if (reset)
					{
						m_rotation.quaternionValue = Quaternion.identity;
					}
					else if (altered != Axes.None)
					{
						Undo.RecordObjects(serializedObject.targetObjects, "Undo Inspector");

						foreach (var obj in serializedObject.targetObjects)
						{
							var t = obj as Transform;
							var v = t.localEulerAngles;

							if ((altered & Axes.X) != 0) v.x = visible.x;
							if ((altered & Axes.Y) != 0) v.y = visible.y;
							if ((altered & Axes.Z) != 0) v.z = visible.z;

							t.localEulerAngles = v;
						}
					}
				}

			}
			GUILayout.EndHorizontal();
		}
	}
}
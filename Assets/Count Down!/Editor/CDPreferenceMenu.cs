using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;

namespace UnityCountDown {
	public class CDPreferenceMenu {

		private static bool s_showTimer;
		private static bool s_showTransformUtil;
		private static bool s_showLineCount;
		
		private static string m_date;
		private static string m_label;
		private static bool m_timerSet;
		
		private static bool prefsLoaded = false;

		private static bool m_settingTimer;

		private static void LoadPreference() {
			s_showTimer = EditorPrefs.GetBool("TimeOut_ShowTimer", true);
			s_showTransformUtil = EditorPrefs.GetBool("TimeOut_ShowTU", true);
			s_showLineCount = EditorPrefs.GetBool("TimeOut_ShowLC", true);
			
			m_date = EditorPrefs.GetString("TimeOut_TimerDate", "NONE");
			m_label = EditorPrefs.GetString("TimeOut_Label", "");
			m_timerSet = m_date != "NONE";
			prefsLoaded = true;
		}

		private static void SavePreference() {
			EditorPrefs.SetBool("TimeOut_ShowTimer", s_showTimer);
			EditorPrefs.SetBool("TimeOut_ShowTU", s_showTransformUtil);
			EditorPrefs.SetBool("TimeOut_ShowLC", s_showLineCount);
			
			EditorPrefs.SetString("TimeOut_TimerDate", m_date);
			EditorPrefs.SetString("TimeOut_Label", m_label);
		}
		
		private static void LinkButton(string caption, string url){
              var style = GUI.skin.label;
              style.richText = true;
        
              var bClicked = GUILayout.Button(caption, style);
        
              var rect = GUILayoutUtility.GetLastRect();
              rect.width = style.CalcSize(new GUIContent(caption)).x;
              EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
        
              if (bClicked)
                Application.OpenURL(url);
        }
		
		[PreferenceItem("Count Down!")]
		public static void PreferencesGUI()
		{
			if(!prefsLoaded) LoadPreference();

			s_showTimer = EditorGUILayout.Toggle("Show Timer", s_showTimer);
			s_showTransformUtil = EditorGUILayout.Toggle("Show Transform Clear Buttons", s_showTransformUtil);
			if(s_showTimer) s_showLineCount = EditorGUILayout.Toggle("Show Line Count", s_showLineCount);
			EditorGUILayout.Space();
			
			if (!m_timerSet) {
				EditorGUILayout.HelpBox("Timer is not set, displaying time after last modification", MessageType.Info);

				if (GUILayout.Button("Set Timer")) {
					m_date = DateTime.Now.ToString();
					m_settingTimer = true;
				}
					
			}
			else {
				EditorGUILayout.HelpBox("Timer set to " + m_date, MessageType.Info);

				if (GUILayout.Button("Update Timer")) {
					m_settingTimer = true;
				}
				
				if (GUILayout.Button("Clear Timer")) {
					m_date = "NONE";
					m_settingTimer = false;
					SavePreference();
					LoadPreference();
				}	
			}

			if (m_settingTimer) {
				m_label = EditorGUILayout.TextField("Label", m_label);
				m_date = EditorGUILayout.TextField("Set Timer", m_date);

				DateTime dt;
				if(DateTime.TryParse(m_date, out dt))
					EditorGUILayout.HelpBox("Confirm? " + dt, MessageType.None);
				else {
					EditorGUILayout.HelpBox("Invaild Syntax, example: " + DateTime.Now, MessageType.Error);
				}
				
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Set")) {
					SavePreference();
					LoadPreference();
					m_settingTimer = false;
				}

				if (GUILayout.Button("Cancel")) {
					m_date = "NONE";
					SavePreference();
					LoadPreference();
					m_settingTimer = false;
				}
			}
			
			// Save the preferences
			if (GUI.changed) SavePreference();
		}
	}
}



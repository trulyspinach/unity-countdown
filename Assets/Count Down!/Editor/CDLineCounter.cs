using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityCountDown
{

	public static class CDLineCounter
	{

		private static int s_currentLineCount = -1;
		private static AssetImportListener s_listener;

		private static List<string> DirSearch(string sDir)
		{
			var files = Directory.GetFiles(sDir).ToList();

			foreach (var d in Directory.GetDirectories(sDir))
			{
				files.AddRange(DirSearch(d));
			}

			return files;
		}

		public static int Count()
		{
			if (s_listener == null) (s_listener = new AssetImportListener()).SetCallback(() => s_currentLineCount = -1);

			if (s_currentLineCount >= 0) return s_currentLineCount;
			var count = 0;
			var files = DirSearch(Application.dataPath);
			foreach (var path in files)
			{
				if (!path.EndsWith(".cs")) continue;
				count += File.ReadAllText(path).Count(x => x == ';');
			}

			s_currentLineCount = count;

			return s_currentLineCount;
		}
	}

	internal class AssetImportListener : AssetPostprocessor
	{
		public static AssetImportListener self;
		public Action cb;

		public AssetImportListener()
		{
			self = this;
		}

		public void SetCallback(Action callback)
		{
			cb = callback;
		}

		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			var scriptsChanged = false;
			foreach (var path in importedAssets)
			{
				if (!path.EndsWith(".cs") && !path.EndsWith(".js")) continue;
				scriptsChanged = true;
			}
			foreach (var path in deletedAssets)
			{
				if (!path.EndsWith(".cs") && !path.EndsWith(".js")) continue;
				scriptsChanged = true;
			}
			foreach (var path in movedAssets)
			{
				if (!path.EndsWith(".cs") && !path.EndsWith(".js")) continue;
				scriptsChanged = true;
			}

			if (self.cb != null && scriptsChanged) self.cb();
		}
	}
}
